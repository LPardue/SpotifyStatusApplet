using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using JariZ;
using System.Threading;
using System.Diagnostics;
using GammaJul.LgLcd;
using System.IO;
using System.Reflection;

namespace SpotifyStatusApplet
{
    class SpotifyStatusApplet
    {
        // Constants
        private const string CURRENT_TRACK_FIELD_TITLE = "Track";
        private const string CURRENT_ALBUM_FIELD_TITLE = "Album";
        private const string CURRENT_ARTIST_FIELD_TITLE = "Artist";

        // Variables
        private static readonly AutoResetEvent m_waitAutoResetEvent = new AutoResetEvent(false);
        // The folowing are declared volatile as they could be written to in event handlers, where the thread belongs to the caller
        private static volatile bool m_monoArrived = false;
        private static volatile bool m_qvgaArrived = false;
        private static volatile bool m_keepRunning = true;

        private static SpotifyAPI m_spotifyApiInstance;
        private static Responses.CFID m_cfid;

        private static Image m_imageOnline;
        private static Image m_imageOffline;
        private static string m_currentTrack;
        private static string m_currentAlbum;
        private static string m_currentArtist;
        private static bool m_playing;
        private static bool m_online;

        // Application lifecycle follows the following sequence
        // 1. Acquire the Spotify Local resources
        // 2. Block until a suitable device is connected
        // 3. Loop until the application needs to terminate and perform the following
        //    3.1. Test if device handle exists, if not create it, if does the device will need to be reopened
        //    3.2  Test conditions are correct to allow a redraw (applet is enabled by user, device is accessible)
        //    3.4  Obtain the latest Spotify details
        //    3.5  Recreate the graphical elements 
        //    3.6  Call DoUpdateAndDraw
        //    3.7  Invokes delegates for the page updates to retrieve Spotify details and redraw graphics
        //
        // NB the thread sleeps at a rate of roughly 30fps. 
        // This provides a smooth scrolling effect for the scrolling text without overloading system or device resources.
        // Track details update in response to system action (e.g. automatically traversing playlists)
        // or user initiated events (skipping via Spotify application UI or keyboard media keys),
        // pragmatically 
        // Additional tuning could be performed
        [MTAThread]
        internal static void Main()
        {
            try
            {
                setupSpotify();

                LcdApplet applet = new LcdApplet("Spotify Status Applet", LcdAppletCapabilities.Both);

                // Register to events to know when a device arrives, then connects the applet to the LCD Manager
                applet.Configure += appletConfigure;
                applet.DeviceArrival += appletDeviceArrival;
                applet.DeviceRemoval += appletDeviceRemoval;
                applet.IsEnabledChanged += appletIsEnabledChanged;
                applet.Connect();

                // We are waiting for the handler thread to warn us for device arrival
                LcdDeviceMonochrome monoDevice = null;
                m_waitAutoResetEvent.WaitOne();

                do
                {
                    // A monochrome device is connected: creates a monochrome device or reopens an old one
                    if (true == m_monoArrived)
                    {
                        if (null == monoDevice)
                        {
                            monoDevice = (LcdDeviceMonochrome)applet.OpenDeviceByType(LcdDeviceType.Monochrome);
                            monoDevice.SoftButtonsChanged += monoDeviceSoftButtonsChanged;
                            createMonochromeGdiPages(monoDevice);
                        }
                        else
                        {
                            monoDevice.ReOpen();
                        }

                        // serviced the last device connection so reset the arrival flag
                        m_monoArrived = false;
                    }

                    if (m_qvgaArrived)
                    {
                        // TODO add the implementation as a future project
                        m_qvgaArrived = false;
                    }
                    
                    // DoUpdateAndDraw only occurs at the framerate specified by LcdPage.DesiredFrameRate, which is 30 by default.
                    if (true == applet.IsEnabled &&
                        null != monoDevice &&
                        false == monoDevice.IsDisposed)
                    {
                        monoDevice.DoUpdateAndDraw();
                    }

                    Thread.Sleep(33);
                }
                while (true == m_keepRunning);
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception, application exited " + e.ToString());
            }
        }

        // Attempt to acquire the Spotify resources
        private static void setupSpotify()
        {

            m_spotifyApiInstance = new SpotifyAPI(SpotifyAPI.GetOAuth(), "jariz-example.spotilocal.com");
            m_cfid = m_spotifyApiInstance.CFID;

            if (m_cfid.error != null)
            {
                Console.WriteLine(string.Format("Spotify returned a error {0} (0x{1})", m_cfid.error.message, m_cfid.error.type));
               // Thread.Sleep(-1);
            }
            else
            {
                // it was ok
            }

        }

        private static void getCurrentSpotifyDetails()
        {
            Responses.Status Current_Status = m_spotifyApiInstance.Status;
            if (m_cfid.error != null)
            {
                Console.WriteLine(string.Format("Spotify returned a error {0} (0x{1})", m_cfid.error.message, m_cfid.error.type));
                Thread.Sleep(-1);
            }

            if (Current_Status.track != null)
            {
                m_currentTrack = Current_Status.track.track_resource.name;
                m_currentAlbum = Current_Status.track.album_resource.name;
                m_currentArtist = Current_Status.track.artist_resource.name;
            }

            m_playing = Current_Status.playing;
            m_online = Current_Status.online;
        }

        private static void createMonochromeGdiPages(LcdDevice monoDevice)
        {
            // Get the images from the assembly
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpotifyStatusApplet.lcdIcon.bmp"))
                m_imageOnline = Image.FromStream(stream);
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpotifyStatusApplet.lcdIcon_offline.bmp"))
                m_imageOffline = Image.FromStream(stream);

            // Creates first page
            LcdGdiPage page1 = new LcdGdiPage(monoDevice)
            {
                Children = {
					new LcdGdiImage(m_imageOffline),
                    new LcdGdiText{Text = CURRENT_TRACK_FIELD_TITLE + ": ", 
                                    Margin = new MarginF(34.0f, 0.0f, 2.0f, 0.0f) }, 
					new LcdGdiScrollViewer {
						Child = new LcdGdiText(m_currentTrack),
						HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch,
						VerticalAlignment = LcdGdiVerticalAlignment.Stretch,
						Margin = new MarginF(64.0f, 0.0f, 2.0f, 0.0f),
						AutoScrollX = true,
					},
                    new LcdGdiText{Text = CURRENT_ALBUM_FIELD_TITLE + ": ", 
                                    Margin = new MarginF(34.0f, 10.0f, 2.0f, 0.0f) }, 
                    new LcdGdiScrollViewer {
						Child = new LcdGdiText(m_currentAlbum),
						HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch,
						VerticalAlignment = LcdGdiVerticalAlignment.Stretch,
						Margin = new MarginF(64.0f, 10.0f, 2.0f, 0.0f),
						AutoScrollX = true,
					},
                    new LcdGdiText{Text = CURRENT_ARTIST_FIELD_TITLE + ": ", 
                                    Margin = new MarginF(34.0f, 20.0f, 2.0f, 0.0f) },
                    new LcdGdiScrollViewer {
						Child = new LcdGdiText( m_currentArtist),
						HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch,
						VerticalAlignment = LcdGdiVerticalAlignment.Stretch,
						Margin = new MarginF(64.0f, 20.0f, 2.0f, 0.0f),
						AutoScrollX = true,
					},
                    new LcdGdiPolygon(Pens.Black, Brushes.Black, new[] {
						new PointF(0.0f, 10.0f), new PointF(0.0f, 0.0f), new PointF(10.0f, 5.0f),
					}, false) {
						HorizontalAlignment = LcdGdiHorizontalAlignment.Right,
						VerticalAlignment = LcdGdiVerticalAlignment.Bottom,
						Margin = new MarginF(0.0f, 0.0f, 5.0f, 5.0f)
					}
				}
            };
            page1.Updating += updatePages;

            // Create second page
            // TODO extend the functionality here by adding pages e.g
            // LcdGdiPage page2 = new LcdGdiPage(monoDevice)
            //{
            //   ...
            //    }
            //};            

            // Finally add page to the device's Pages collection set the current page
            monoDevice.Pages.Add(page1);          
            monoDevice.CurrentPage = page1;
        } 

        // Event handler for the page update (invoked indirectly by DoUpdateAndDraw)
        private static void updatePages(object sender, UpdateEventArgs e)
        {
            LcdGdiPage page = (LcdGdiPage)sender;
            updateTextFields(page);

            // Turn on/off the playing symbol
            LcdGdiPolygon polygon = (LcdGdiPolygon)page.Children[7];
            polygon.Brush = m_playing ? Brushes.Black : Brushes.White;
            polygon.Pen = m_playing ? Pens.Black : Pens.White;

            // Show offline or not
            LcdGdiImage image = (LcdGdiImage)page.Children[0];
            image.Image = m_online ? m_imageOnline : m_imageOffline;
        }

        private static void updateTextFields(LcdGdiPage page)
        {
            getCurrentSpotifyDetails();
            LcdGdiScrollViewer scrollViewer = (LcdGdiScrollViewer)page.Children[2];
            LcdGdiText track = (LcdGdiText)scrollViewer.Child;
            track.Text = m_currentTrack;

            LcdGdiScrollViewer scrollViewer2 = (LcdGdiScrollViewer)page.Children[4];
            LcdGdiText album = (LcdGdiText)scrollViewer2.Child;
            album.Text = m_currentAlbum;

            LcdGdiScrollViewer scrollViewer3 = (LcdGdiScrollViewer)page.Children[6];
            LcdGdiText artist = (LcdGdiText)scrollViewer3.Child;
            artist.Text = m_currentArtist;
        }

        // Event handler for new device arrical in the system.
        // Monochrome devices include (G510, G13, G15, Z10)
        private static void appletDeviceArrival(object sender, LcdDeviceTypeEventArgs e)
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
        private static void appletDeviceRemoval(object sender, LcdDeviceTypeEventArgs e)
        {
            // No action required
        }

        // Event handler for applet enablement or disablement in the LCD Manager
        private static void appletIsEnabledChanged(object sender, EventArgs e)
        {
            // No action required
        }

        /// This event handler is called whenever the soft buttons are pressed or released.

        private static void monoDeviceSoftButtonsChanged(object sender, LcdSoftButtonsEventArgs e)
        {
            LcdDevice device = (LcdDevice)sender;

            // First button 
            if ((e.SoftButtons & LcdSoftButtons.Button0) == LcdSoftButtons.Button0)
            {

            }

            // Second button 
            if ((e.SoftButtons & LcdSoftButtons.Button1) == LcdSoftButtons.Button1)
            {

            }

            // Third button 
            if ((e.SoftButtons & LcdSoftButtons.Button2) == LcdSoftButtons.Button2)
            {

            }

            // Fourth button 
            if ((e.SoftButtons & LcdSoftButtons.Button3) == LcdSoftButtons.Button3)
            {
                //m_keepRunning = false;
            }
        }
    }
}
