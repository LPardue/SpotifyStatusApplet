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

using GammaJul.LgLcd;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SpotifyStatusApplet
{
    // LcdGraphics encapsulates the graphical properties related to the LCD display
    // suh as pages, text, graphics and update event handlers.
    class LcdGraphics
    {
        private const string CURRENT_TRACK_FIELD_TITLE = "Track";
        private const string CURRENT_ALBUM_FIELD_TITLE = "Album";
        private const string CURRENT_ARTIST_FIELD_TITLE = "Artist";

        private Image m_imageOnline;
        private Image m_imageOffline;

        private LcdGdiPage m_nowPlayingPage;
        private LcdGdiPage m_nowPlayingNoTitlesPage;
        private LcdGdiPage m_privatePage;

        private MediaPlayerDetails m_playerDetails;
        private bool m_showTitles = true;

        public void setMediaPlayerDetails(MediaPlayerDetails mpd)
        {
            m_playerDetails = mpd;
        }

        public void setShowTitles(bool showTitles)
        {
            m_showTitles = showTitles;
        }

        // Create the Mono Pages
        public void createMonochromeGdiPages(LcdDevice monoDevice)
        {
            // Get the images from the assembly
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpotifyStatusApplet.lcdIcon.bmp"))
                m_imageOnline = Image.FromStream(stream);
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpotifyStatusApplet.lcdIcon_offline.bmp"))
                m_imageOffline = Image.FromStream(stream);

            m_nowPlayingPage = new LcdGdiPage(monoDevice)
            {
                Children = {
					new LcdGdiImage(m_imageOffline),
                    new LcdGdiText{Text = CURRENT_TRACK_FIELD_TITLE + ": ", 
                                    Margin = new MarginF(34.0f, 0.0f, 2.0f, 0.0f) }, 
					new LcdGdiScrollViewer {
						Child = new LcdGdiText(""),
						HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch,
						VerticalAlignment = LcdGdiVerticalAlignment.Stretch,
						Margin = new MarginF(64.0f, 0.0f, 2.0f, 0.0f),
						AutoScrollX = true,
					},
                    new LcdGdiText{Text = CURRENT_ALBUM_FIELD_TITLE + ": ", 
                                    Margin = new MarginF(34.0f, 10.0f, 2.0f, 0.0f) }, 
                    new LcdGdiScrollViewer {
						Child = new LcdGdiText(""),
						HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch,
						VerticalAlignment = LcdGdiVerticalAlignment.Stretch,
						Margin = new MarginF(64.0f, 10.0f, 2.0f, 0.0f),
						AutoScrollX = true,
					},
                    new LcdGdiText{Text = CURRENT_ARTIST_FIELD_TITLE + ": ", 
                                    Margin = new MarginF(34.0f, 20.0f, 2.0f, 0.0f) },
                    new LcdGdiScrollViewer {
						Child = new LcdGdiText( ""),
						HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch,
						VerticalAlignment = LcdGdiVerticalAlignment.Stretch,
						Margin = new MarginF(64.0f, 20.0f, 2.0f, 0.0f),
						AutoScrollX = true,
					},
                    new LcdGdiText{Text = "", 
                        Margin = new MarginF(34.0f, 30.0f, 2.0f, 0.0f) },
                    new LcdGdiPolygon(Pens.Black, Brushes.Black, new[] {
						new PointF(0.0f, 10.0f), new PointF(0.0f, 0.0f), new PointF(10.0f, 5.0f),
					}, false) {
						HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
						VerticalAlignment = LcdGdiVerticalAlignment.Bottom,
						Margin = new MarginF(0.0f, 0.0f, 5.0f, 5.0f)
					}
				}
            };
            m_nowPlayingPage.Updating += updateNowPlayingPage;

            m_nowPlayingNoTitlesPage = new LcdGdiPage(monoDevice)
            {
                Children = {
					new LcdGdiImage(m_imageOffline),                    
					new LcdGdiScrollViewer {
						Child = new LcdGdiText(""),
						HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch,
						VerticalAlignment = LcdGdiVerticalAlignment.Stretch,
						Margin = new MarginF(34.0f, 0.0f, 2.0f, 0.0f),
						AutoScrollX = true,
					},                     
                    new LcdGdiScrollViewer {
						Child = new LcdGdiText(""),
						HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch,
						VerticalAlignment = LcdGdiVerticalAlignment.Stretch,
						Margin = new MarginF(34.0f, 10.0f, 2.0f, 0.0f),
						AutoScrollX = true,
					},                    
                    new LcdGdiScrollViewer {
						Child = new LcdGdiText( ""),
						HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch,
						VerticalAlignment = LcdGdiVerticalAlignment.Stretch,
						Margin = new MarginF(34.0f, 20.0f, 2.0f, 0.0f),
						AutoScrollX = true,
					},
                    new LcdGdiText{Text = "", 
                        Margin = new MarginF(34.0f, 30.0f, 2.0f, 0.0f) },
                    new LcdGdiPolygon(Pens.Black, Brushes.Black, new[] {
						new PointF(0.0f, 10.0f), new PointF(0.0f, 0.0f), new PointF(10.0f, 5.0f),
					}, false) {
						HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
						VerticalAlignment = LcdGdiVerticalAlignment.Bottom,
						Margin = new MarginF(0.0f, 0.0f, 5.0f, 5.0f)
					}
				}
            };
            m_nowPlayingNoTitlesPage.Updating += updateNowPlayingMinPage;

            m_privatePage = new LcdGdiPage(monoDevice)
            {
                Children = {
					new LcdGdiImage(m_imageOffline),
                    new LcdGdiText{Text = "Warning!", 
                                    Margin = new MarginF(24.0f, 0.0f, 2.0f, 0.0f), 
                                    HorizontalAlignment = LcdGdiHorizontalAlignment.Center }, 					
                    new LcdGdiText{Text = "Spotify in private mode.", 
                                    Margin = new MarginF(24.0f, 10.0f, 2.0f, 0.0f), 
                                    HorizontalAlignment = LcdGdiHorizontalAlignment.Center },                     
                    new LcdGdiText{Text = "No track info.", 
                                    Margin = new MarginF(24.0f, 20.0f, 2.0f, 0.0f), 
                                    HorizontalAlignment = LcdGdiHorizontalAlignment.Center },                    
                    new LcdGdiPolygon(Pens.Black, Brushes.Black, new[] {
						new PointF(0.0f, 10.0f), new PointF(0.0f, 0.0f), new PointF(10.0f, 5.0f),
					}, false) {
						HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
						VerticalAlignment = LcdGdiVerticalAlignment.Bottom,
						Margin = new MarginF(0.0f, 0.0f, 5.0f, 5.0f)
					}
				}
            };
            m_privatePage.Updating += updatePrivatePage;

            // Finally add page to the device's Pages collection set the current page
            monoDevice.Pages.Add(m_nowPlayingPage);
            monoDevice.Pages.Add(m_nowPlayingNoTitlesPage);
            if (m_showTitles)
            {
                monoDevice.CurrentPage = m_nowPlayingPage;
            }
            else
            {
                monoDevice.CurrentPage = m_nowPlayingNoTitlesPage;
            }
        }

        // Event handler for the page update (invoked indirectly by DoUpdateAndDraw)
        public void updateNowPlayingPage(object sender, UpdateEventArgs e)
        {
            LcdGdiPage page = (LcdGdiPage)sender;

            if (m_playerDetails.privateSession == true)
            {
                page.Device.CurrentPage = m_privatePage;
            }
            else if (m_showTitles == true)
            {
                page.Device.CurrentPage = m_nowPlayingPage;
            }
            else
            {
                page.Device.CurrentPage = m_nowPlayingNoTitlesPage;                
            }

            updateTextFields(page.Device, 
                page,
                (LcdGdiScrollViewer)page.Children[2],
                (LcdGdiScrollViewer)page.Children[4],
                (LcdGdiScrollViewer)page.Children[6],
                (LcdGdiText)page.Children[7]);

            // Turn on/off the playing symbol
            LcdGdiPolygon polygon = (LcdGdiPolygon)page.Children[8];            
           
            polygon.Brush = m_playerDetails.playing ? Brushes.Black : Brushes.White;
            polygon.Pen = m_playerDetails.playing ? Pens.Black : Pens.White;

            // Show offline or not
            LcdGdiImage image = (LcdGdiImage)page.Children[0];
            image.Image = m_playerDetails.online ? m_imageOnline : m_imageOffline;
        }

        // Event handler for the page update (invoked indirectly by DoUpdateAndDraw)
        public void updateNowPlayingMinPage(object sender, UpdateEventArgs e)
        {
            LcdGdiPage page = (LcdGdiPage)sender;

            if (m_playerDetails.privateSession == true)
            {
                page.Device.CurrentPage = m_privatePage;
            }
            else if (m_showTitles == true)
            {
                page.Device.CurrentPage = m_nowPlayingPage;
            }
            else
            {
                page.Device.CurrentPage = m_nowPlayingNoTitlesPage;
            }

            updateTextFields(page.Device,
                page,
                (LcdGdiScrollViewer)page.Children[1],
                (LcdGdiScrollViewer)page.Children[2],
                (LcdGdiScrollViewer)page.Children[3],
                (LcdGdiText)page.Children[4]);

            // Turn on/off the playing symbol
            LcdGdiPolygon polygon = (LcdGdiPolygon)page.Children[5];
            
            polygon.Brush = m_playerDetails.playing ? Brushes.Black : Brushes.White;
            polygon.Pen = m_playerDetails.playing ? Pens.Black : Pens.White;

            // Show offline or not
            LcdGdiImage image = (LcdGdiImage)page.Children[0];
            image.Image = m_playerDetails.online ? m_imageOnline : m_imageOffline;
        }

        // Update the Now Playing pagetext fields
        public void updateTextFields(LcdDevice device, LcdGdiPage page, LcdGdiScrollViewer scrollViewer,
            LcdGdiScrollViewer scrollViewer2, LcdGdiScrollViewer scrollViewer3, LcdGdiText playTime)
        {   
            LcdGdiText track = (LcdGdiText)scrollViewer.Child;
            track.Text = m_playerDetails.currentTrack;


            LcdGdiText album = (LcdGdiText)scrollViewer2.Child;
            album.Text = m_playerDetails.currentAlbum;

            LcdGdiText artist = (LcdGdiText)scrollViewer3.Child;
            artist.Text = m_playerDetails.currentArtist;

            playTime.Text = m_playerDetails.playTime;
        }

        // Event handler for the page update (invoked indirectly by DoUpdateAndDraw)
        public void updatePrivatePage(object sender, UpdateEventArgs e)
        {
            LcdGdiPage page = (LcdGdiPage)sender;

            if (m_playerDetails.privateSession == true)
            {
                page.Device.CurrentPage = m_privatePage;
            }
            else
            {
                page.Device.CurrentPage = m_nowPlayingPage;
            }

            // Turn on/off the playing symbol
            LcdGdiPolygon polygon = (LcdGdiPolygon)page.Children[4];
            polygon.Brush = m_playerDetails.playing ? Brushes.Black : Brushes.White;
            polygon.Pen = m_playerDetails.playing ? Pens.Black : Pens.White;
        }        
    }
}
