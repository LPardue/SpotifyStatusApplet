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
        private LcdGdiPage m_privatePage;

        private MediaPlayerDetails m_playerDetails;

        public void setMediaPlayerDetails(MediaPlayerDetails mpd)
        {
            m_playerDetails = mpd;
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
            monoDevice.CurrentPage = m_nowPlayingPage;
        }

        // Event handler for the page update (invoked indirectly by DoUpdateAndDraw)
        public void updateNowPlayingPage(object sender, UpdateEventArgs e)
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

            updateTextFields(page.Device, page);

            // Turn on/off the playing symbol
            LcdGdiPolygon polygon = (LcdGdiPolygon)page.Children[8];
            polygon.Brush = m_playerDetails.playing ? Brushes.Black : Brushes.White;
            polygon.Pen = m_playerDetails.playing ? Pens.Black : Pens.White;

            // Show offline or not
            LcdGdiImage image = (LcdGdiImage)page.Children[0];
            image.Image = m_playerDetails.online ? m_imageOnline : m_imageOffline;
        }

        // Update the Now Playing pagetext fields
        public void updateTextFields(LcdDevice device, LcdGdiPage page)
        {
            LcdGdiScrollViewer scrollViewer = (LcdGdiScrollViewer)page.Children[2];
            LcdGdiText track = (LcdGdiText)scrollViewer.Child;
            track.Text = m_playerDetails.currentTrack;

            LcdGdiScrollViewer scrollViewer2 = (LcdGdiScrollViewer)page.Children[4];
            LcdGdiText album = (LcdGdiText)scrollViewer2.Child;
            album.Text = m_playerDetails.currentAlbum;

            LcdGdiScrollViewer scrollViewer3 = (LcdGdiScrollViewer)page.Children[6];
            LcdGdiText artist = (LcdGdiText)scrollViewer3.Child;
            artist.Text = m_playerDetails.currentArtist;

            LcdGdiText playTime = (LcdGdiText)page.Children[7];
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
