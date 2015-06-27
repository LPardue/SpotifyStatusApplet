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