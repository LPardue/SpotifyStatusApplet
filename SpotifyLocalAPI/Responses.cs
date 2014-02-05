using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JariZ
{
    public class Responses
    {
        public class ClientVersion
        {
            public Internal.error error { get; set; }
            public int version { get; set; }
            public string client_version { get; set; }
            public bool running { get; set; }
        }

        public class CFID
        {
            public Internal.error error { get; set; }
            public string token { get; set; }
        }

        public class Status
        {
            public Internal.error error { get; set; }
            public int version { get; set; }
            public string client_version { get; set; }
            public bool playing { get; set; }
            public bool shuffle { get; set; }
            public bool repeat { get; set; }
            public bool play_enabled { get; set; }
            public bool prev_enabled { get; set; }
            public Internal.track track { get; set; }
            public double playing_position { get; set; }
            public int server_time { get; set; }
            public double volume { get; set; }
            public bool online { get; set; }
            public Internal.open_graph_state open_graph_state { get; set; }
            public bool running { get; set; }
        }

        public class Internal
        {
            #region Misc
            public class error
            {
                public string type { get; set; }
                public string message { get; set; }
            }
            #endregion

            #region Status
            public class open_graph_state
            {
                public bool private_session { get; set; }
                public bool posting_disabled { get; set; }
            }

            public class track
            {
                public resource track_resource { get; set; }
                public resource artist_resource { get; set; }
                public resource album_resource { get; set; }
                public int length { get; set; }
                public string track_type { get; set; }
            }

            public class resource
            {
                public string name { get; set; }
                public string uri { get; set; }
                public location location { get; set; }
            }

            public class location
            {
                public string og { get; set; }
            }
            #endregion
        }
    }
}
