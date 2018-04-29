/*
 * SpotifyStatusApplet 
 *
 * Copyright (c) 2015 Lucas Pardue
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the LICENSE file
 * distributed with this work for additional information
 * regarding copyright ownership.  This file is licensed 
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.Threading;
using System.Diagnostics;
using GammaJul.LgLcd;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Models;

namespace SpotifyStatusApplet
{
    class SpotifyStatusApplet
    {
        //Imported in order to use media key emulation functionality for track skip and play/pause.
            //SpotifyAPI only had Play() and Pause() separate which didn't suit my need.
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        //Declared for use with SpotifyAPI
        private SpotifyLocalAPI s_spotify;
        //Declared for use in applet
        private static int a_ConnectionStatus = -1;
        private static bool a_SongLoadTimeoutExpired = false;
        private readonly AutoResetEvent m_waitAutoResetEvent = new AutoResetEvent(false);
        // The folowing are declared volatile as they could be written to in event handlers, where the thread belongs to the caller
        private volatile bool m_monoArrived = false;
        private volatile bool m_qvgaArrived = false;
        private volatile bool m_keepRunning = true;
        private volatile bool m_showTitles = false;

        private LcdGraphics m_lcdGraphics;

        SpotifyStatusApplet(bool showTitles)
        {
            m_showTitles = showTitles;
        }

        // Application lifecycle follows the following sequence
        // 1. Acquire the Spotify Local resources
        // 2. Create Notification Tray Icon
        // 3. Block until a suitable device is connected
        // 4. Loop until the application needs to terminate and perform the following
        //    4.1. Test if device handle exists, if not create it, if does the device will need to be reopened
        //    4.2  Test conditions are correct to allow a redraw (applet is enabled by user, device is accessible)
        //    4.4  Obtain the latest Spotify details
        //    4.5  Recreate the graphical elements 
        //    4.6  Call DoUpdateAndDraw
        //    4.7  Invokes delegates for the page updates to retrieve Spotify details and redraw graphics
        //
        // NB the thread sleeps at a rate of roughly 30fps. 
        // This provides a smooth scrolling effect for the scrolling text without overloading system or device resources.
        // Track details update in response to system action (e.g. automatically traversing playlists)
        // or user initiated events (skipping via Spotify application UI or keyboard media keys),
        // pragmatically 
        // Additional tuning could be performed
        [MTAThread]
        internal static int Main(string[] args)
        {
            bool showTitles = true;

            if (args != null && args.Length !=0)
            {
                if (args[0] == "notitles")
                {
                    showTitles = false;
                }
            }

            try
            {
                SpotifyStatusApplet ssa = new SpotifyStatusApplet(showTitles);
                a_ConnectionStatus = ssa.SetupSpotify();
                if (a_ConnectionStatus != 0)
                {
                    Trace.TraceError("Critical Error: Spotify couldn't start.\n\tTerminating SpotifyStatusApplet.\t\nReturn Value of SetupSpotify(): " + a_ConnectionStatus);
                    //Applet cannot function if Spotify or SpotifyWebHelper is unable to open. Return from Main thread with error 1.
                    return 1;
                }

                StatusResponse spotify_Status = ssa.s_spotify.GetStatus();
                //The applet crashes when having to wait for the Spotify song information (When the app is completely exited).
                //This waits until it loads in and has a 2 second timeout. 
                for (int i = 0; (spotify_Status.Track == null); i++)
                {
                    Thread.Sleep(1);
                    if (i > 2000) break;
                    if (i > 1998) a_SongLoadTimeoutExpired = true;
                }
                if (a_SongLoadTimeoutExpired == true)
                {
                    Trace.TraceWarning("Couldn't load track information. \n\tTerminating SpotifyStatusApplet.");
                    return 1;
                }

                ssa.setupTrayIcon();

                LcdApplet applet = new LcdApplet("Spotify Status Applet", LcdAppletCapabilities.Both);

                // Register to events to know when a device arrives, then connects the applet to the LCD Manager
                applet.Configure += appletConfigure;
                applet.DeviceArrival += ssa.appletDeviceArrival;
                applet.DeviceRemoval += ssa.appletDeviceRemoval;
                applet.IsEnabledChanged += ssa.appletIsEnabledChanged;
                applet.Connect();

                // We are waiting for the handler thread to warn us for device arrival
                LcdDeviceMonochrome monoDevice = null;
                ssa.m_waitAutoResetEvent.WaitOne();

                do
                {
                    // A monochrome device is connected: creates a monochrome device or reopens an old one
                    if (true == ssa.m_monoArrived)
                    {
                        if (null == monoDevice)
                        {
                            monoDevice = (LcdDeviceMonochrome)applet.OpenDeviceByType(LcdDeviceType.Monochrome);
                            monoDevice.SoftButtonsChanged += ssa.monoDeviceSoftButtonsChanged;
                            ssa.m_lcdGraphics = new LcdGraphics();
                            ssa.m_lcdGraphics.createMonochromeGdiPages(monoDevice);
                            monoDevice.SetAsForegroundApplet = true;
                        }
                        else
                        {
                            monoDevice.ReOpen();
                        }

                        // serviced the last device connection so reset the arrival flag
                        ssa.m_monoArrived = false;
                    }

                    if (ssa.m_qvgaArrived)
                    {
                        // TODO add the implementation as a future project
                        ssa.m_qvgaArrived = false;
                    }
                    
                    // DoUpdateAndDraw only occurs at the framerate specified by LcdPage.DesiredFrameRate, which is 30 by default.
                    if (true == applet.IsEnabled &&
                        null != monoDevice &&
                        false == monoDevice.IsDisposed)
                    {
                        ssa.m_lcdGraphics.setMediaPlayerDetails(ssa.getCurrentSpotifyDetails());
                        ssa.m_lcdGraphics.setShowTitles(ssa.m_showTitles);
                        monoDevice.DoUpdateAndDraw();
                    }

                    Thread.Sleep(33);
                }
                while (true == ssa.m_keepRunning);
            }
            catch (Exception e)
            {
                Trace.TraceError("Caught exception, application exited " + e.ToString());
            }
            return 0;
        }

        /// <summary>
        /// Attempts to acquire the Spotify resources. Will start needed programs and return an error code if it fails.
        /// Gives the programs a maximum of 1 second to successfully start up. <c>Thread.Sleep(2000);</c> is to fix a problem with the programming returing NULL when Spotify is not already running.
        /// It works most of the time, but I'm still trying to work out the kinks.
        /// </summary>
        /// <returns>
        /// 0 Success
        /// 1 Spotify failed to start
        /// 2 SpotifyWebHelper failed to start
        /// 3 Unkown Error
        /// </returns>
        private int SetupSpotify()
        {

            if (!SpotifyLocalAPI.IsSpotifyRunning())
            {
                Trace.TraceInformation("Spotify isn't running.\n\tAttempting to start.");
                SpotifyLocalAPI.RunSpotify();

                for (int i = 0; (i < 1000) && (SpotifyLocalAPI.IsSpotifyRunning() == false); i++) Thread.Sleep(1);
                if (!SpotifyLocalAPI.IsSpotifyRunning())
                {
                    Trace.TraceError("\tSpotify couldn't start.");
                    return 1;
                }
            }
            if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
            {
                Trace.TraceInformation("SpotifyWebHelper isn't running.\n\tAttempting to start.");
                SpotifyLocalAPI.RunSpotifyWebHelper();
                for (int i = 0; (i < 1000) && (SpotifyLocalAPI.IsSpotifyWebHelperRunning() == false); i++) Thread.Sleep(1);

                if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
                {
                    Trace.TraceError("\tSpotifyWebHelper couldn't start.");
                    return 2;
                }
            }
            s_spotify = new SpotifyLocalAPI();

            bool successful = s_spotify.Connect();
            if (successful)
            {
                Trace.TraceInformation("Connection to Spotify was successful");
                s_spotify.ListenForEvents = true;
                Thread.Sleep(2000);
                return 0;
            }
            return 3;
        }

        // Obtain the current details of the Spotify player, both track info and the player status
        private MediaPlayerDetails getCurrentSpotifyDetails()
        {
            MediaPlayerDetails retVal = new MediaPlayerDetails();
            //Responses.Status current_Status = m_spotifyApiInstance.Status;
            StatusResponse current_Status = s_spotify.GetStatus();
            if (current_Status.Track == null) ;
            else if (current_Status.Track.IsAd()) ;
            else
            {
                Debug.Print("Track Information:");
                Debug.Print("\tTrack: " + current_Status.Track.TrackResource.Name);
                Debug.Print("\tArtist: " + current_Status.Track.ArtistResource.Name);
                Debug.Print("\tAlbum: " + current_Status.Track.AlbumResource.Name);
                Debug.Print("\tPlaying Position (Seconds): " + current_Status.PlayingPosition);
                Debug.Print("\tPlay Time Total (Seconds): " + current_Status.Track.Length);
                retVal.currentTrack = current_Status.Track.TrackResource.Name;
                retVal.currentAlbum = current_Status.Track.AlbumResource.Name;
                retVal.currentArtist = current_Status.Track.ArtistResource.Name;
                int pos = (int)current_Status.PlayingPosition;
                int len = current_Status.Track.Length;
                retVal.playTime = String.Format("{0}:{1:D2}/{2}:{3:D2}", pos / 60, pos % 60, len / 60, len % 60);
            }
            Debug.Print("Spotify Status Information:");
            Debug.Print("\tIs Playing?: " + current_Status.Playing);
            Debug.Print("\tIs Online?: " + current_Status.Online);
            Debug.Print("\tIs Private Session?: " + current_Status.OpenGraphState.PrivateSession);
            retVal.playing = current_Status.Playing;
            retVal.online = current_Status.Online;
            retVal.privateSession = current_Status.OpenGraphState.PrivateSession;

            return retVal;
        }

        // Launch the thread that manages the notification tray icon
        private void setupTrayIcon()
        {
            var myThread = new Thread(delegate()
            {
                using (AppTrayIcon ati = new AppTrayIcon(this))
                {
                    ati.Display();
                    Application.Run();
                }
            });

            myThread.SetApartmentState(ApartmentState.STA);
            myThread.Start();
        }

        // Event handler for new device arrical in the system.
        // Monochrome devices include (G510, G13, G15, Z10)
        private void appletDeviceArrival(object sender, LcdDeviceTypeEventArgs e)
        {            
            switch (e.DeviceType)
            {
                // A monochrome device (G13/G15/Z10) was connected
                case LcdDeviceType.Monochrome:
                    m_monoArrived = true;
                    break;
                case LcdDeviceType.Qvga:
                    m_qvgaArrived = true;
                    break;
                default:
                    break;
            }

            m_waitAutoResetEvent.Set();
        }       

        /////////////////////////////////////////////
        // Unused functions at this time
        //////////////////////////////////////////////

        /// Event handler for Configure button click (for this applet) in the LCD Manager.
        private static void appletConfigure(object sender, EventArgs e)
        {
            // No action required
        }

        // Event handler for device removal
        private void appletDeviceRemoval(object sender, LcdDeviceTypeEventArgs e)
        {
            // No action required
        }

        // Event handler for applet enablement or disablement in the LCD Manager
        private void appletIsEnabledChanged(object sender, EventArgs e)
        {
            // No action required
        }

        /// This event handler is called whenever the soft buttons are pressed or released.

        private void monoDeviceSoftButtonsChanged(object sender, LcdSoftButtonsEventArgs e)
        {
            LcdDevice device = (LcdDevice)sender;

            // First button 
            if ((e.SoftButtons & LcdSoftButtons.Button0) == LcdSoftButtons.Button0)
            {
                m_showTitles = !m_showTitles;
            }

            // Second button 
            if ((e.SoftButtons & LcdSoftButtons.Button1) == LcdSoftButtons.Button1)
            {
                keybd_event(Convert.ToByte(Keys.MediaPreviousTrack), 0, 0x00, 0); //KEYDOWN PrevTrack Key
                keybd_event(Convert.ToByte(Keys.MediaPreviousTrack), 0, 0x02, 0); //KEYUP PrevTrack Key
            }

            // Third button 
            if ((e.SoftButtons & LcdSoftButtons.Button2) == LcdSoftButtons.Button2)
            {
                keybd_event(Convert.ToByte(Keys.MediaPlayPause), 0, 0x00, 0); //KEYDOWN PlayPause Key
                keybd_event(Convert.ToByte(Keys.MediaPlayPause), 0, 0x02, 0); //KEYUP PlayPause Key
            }

            // Fourth button 
            if ((e.SoftButtons & LcdSoftButtons.Button3) == LcdSoftButtons.Button3)
            {
                //m_keepRunning = false;
                keybd_event(Convert.ToByte(Keys.MediaNextTrack), 0, 0x00, 0); //KEYDOWN NextTrack Key
                keybd_event(Convert.ToByte(Keys.MediaNextTrack), 0, 0x02, 0); //KEYUP NextTrack Key
            }
        }

        public void Terminate()
        {
            m_keepRunning = false;
        }
    }
}
