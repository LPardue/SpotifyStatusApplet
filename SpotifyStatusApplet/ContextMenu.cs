using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace SpotifyStatusApplet
{
    class ContextMenu
    {
        private bool m_isAboutLoaded = false;

        public ContextMenuStrip Create()
        {
            // Add the default menu options.
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem item;
            ToolStripSeparator sep;

            // About.
            item = new ToolStripMenuItem();
            item.Text = "About";
            item.Click += new EventHandler(About_Click);
            menu.Items.Add(item);

            // Separator.
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);

            // Exit.
            item = new ToolStripMenuItem();
            item.Text = "Exit";
            item.Click += new System.EventHandler(Exit_Click);
            menu.Items.Add(item);

            return menu;
        }

        void About_Click(object sender, EventArgs e)
        {
            if (!m_isAboutLoaded)
            {
                m_isAboutLoaded = true;
                new AboutBox().ShowDialog();
                m_isAboutLoaded = false;
            }
        }

        void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
