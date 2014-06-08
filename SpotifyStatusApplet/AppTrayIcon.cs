using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SpotifyStatusApplet
{
    class AppTrayIcon : IDisposable
    {
        private NotifyIcon m_ni;
        private SpotifyStatusApplet m_ssa;

        public AppTrayIcon(SpotifyStatusApplet ssa)
        {
            m_ni = new NotifyIcon();
            m_ssa = ssa;
        }

        public void Display()
        {
            // Put the icon in the system tray and allow it react to mouse clicks.			
            m_ni.MouseClick += new MouseEventHandler(ni_MouseClick);
            m_ni.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); ;
            m_ni.Text = "SpotifyStatusApplet";
            m_ni.Visible = true;

            // Attach a context menu.
            m_ni.ContextMenuStrip = new ContextMenu().Create();
        }

        public void Dispose()
        {
            m_ni.Visible = false;
            m_ssa.Terminate();
            m_ni.Dispose();
        }

        // TODO handle any mouse click directly on the notification icon
        void ni_MouseClick(object sender, MouseEventArgs e)
        {
            
        }
    }
}