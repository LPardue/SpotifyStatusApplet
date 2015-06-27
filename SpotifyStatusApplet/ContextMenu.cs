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
            ToolStripMenuItem about;
            ToolStripSeparator sep;

            // About.
            about = new ToolStripMenuItem();
            about.Text = "About";
            about.Click += new EventHandler(About_Click);
            menu.Items.Add(about);

            // Separator.
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);

            // Exit.
            about = new ToolStripMenuItem();
            about.Text = "Exit";
            about.Click += new System.EventHandler(Exit_Click);
            menu.Items.Add(about);

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
