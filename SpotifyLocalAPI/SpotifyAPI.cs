using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace JariZ
{
    public class SpotifyAPI
    {
        private string _oauth;
        private string _host;
        private string _cfid;

        private WebClient wc;

        /// <summary>
        /// Initializes a new SpotifyAPI object which can be used to recieve
        /// </summary>
        /// <param name="OAuth">Use <seealso cref="SpotifyAPI.GetOAuth()"/> to get this, Or specify your own</param>
        /// <param name="Host">Most of the time 127.0.0.1, or for lulz use something like my-awesome-program.spotilocal.com</param>
        public SpotifyAPI(string OAuth, string Host = "127.0.0.1")
        {
            _oauth = OAuth;
            _host = Host;

            wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.Headers.Add("User-Agent: SpotifyAPI");
            //emulate the embed code [NEEDED]
            wc.Headers.Add("Origin", "https://embed.spotify.com");
            wc.Headers.Add("Referer", "https://embed.spotify.com/?uri=spotify:track:5Zp4SWOpbuOdnsxLqwgutt");
            wc.Encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Get a link to the 640x640 cover art image of a spotify album
        /// </summary>
        /// <param name="uri">The Spotify album URI</param>
        /// <returns></returns>
        public string getArt(string uri)
        {
            try
            {
                var wc = new WebClient();
                wc.Headers.Add("User-Agent: SpotifyAPI");
                string raw = wc.DownloadString("http://open.spotify.com/album/" + uri.Split(new string[] { ":" }, StringSplitOptions.None)[2]);
                raw = raw.Replace("\t", ""); ;
                string[] lines = raw.Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string line in lines)
                {
                    if (line.StartsWith("<meta property=\"og:image\""))
                    {
                        string[] l = line.Split(new string[] { "/" }, StringSplitOptions.None);
                        return "http://o.scdn.co/640/" + l[4].Replace("\"", "").Replace(">", "");
                    }
                }
            }
            catch
            {
                return "";
            }
            return "";
        }


        /// <summary>
        /// Gets the current Unix Timestamp
        /// Mostly for internal use
        /// </summary>
        public int TimeStamp
        {
            get
            {
                return Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            }
        }

        /// <summary>
        /// Gets the 'CFID', a unique identifier for the current session.
        /// Note: It's required to get the CFID before making any other calls
        /// </summary>
        public Responses.CFID CFID
        {
            get
            {
                string a = recv("simplecsrf/token.json");
                List<Responses.CFID> d = (List<Responses.CFID>)JsonConvert.DeserializeObject(a, typeof(List<Responses.CFID>));
                _cfid = d[0].token;
                return d[0];
            }
        }

        string _uri = "";
        /// <summary>
        /// Used by SpotifyAPI.Play to play Spotify URI's
        /// Change this URI and then call SpotifyAPI.Play
        /// </summary>
        public string URI
        {
            get
            {
                return _uri;
            }
            set
            {
                _uri = value;
            }
        }
        
        /// <summary>
        /// Plays a certain URI and returns the status afterwards
        /// Change SpotifyAPI.URI into the needed uri!
        /// </summary>
        public Responses.Status Play
        {
            get
            {
                string a = recv("remote/play.json?uri=" + URI, true, true, -1);
                List<Responses.Status> d = (List<Responses.Status>)JsonConvert.DeserializeObject(a, typeof(List<Responses.Status>));
                return d[0];
            }
        }

        /// <summary>
        /// Resume Spotify playback and return the status afterwards 
        /// </summary>
        public Responses.Status Resume
        {
            get
            {
                string a = recv("remote/pause.json?pause=false", true, true, -1);
                List<Responses.Status> d = (List<Responses.Status>)JsonConvert.DeserializeObject(a, typeof(List<Responses.Status>));
                return d[0];
            }
        }

        /// <summary>
        /// Pause Spotify playback and return the status afterwards
        /// </summary>
        public Responses.Status Pause
        {
            get
            {
                string a = recv("remote/pause.json?pause=true", true, true, -1);
                List<Responses.Status> d = (List<Responses.Status>)JsonConvert.DeserializeObject(a, typeof(List<Responses.Status>));
                return d[0];
            }
        }

        /// <summary>
        /// Returns the current track info.
        /// Change <seealso cref="Wait"/> into the amount of waiting time before it will return
        /// When the current track info changes it will return before elapsing the amount of seconds in <seealso cref="Wait"/>
        /// (look at the project site for more information if you do not understand this)
        /// </summary>
        public Responses.Status Status
        {
            get
            {
                string a = recv("remote/status.json", true, true, _wait);
                List<Responses.Status> d = (List<Responses.Status>)JsonConvert.DeserializeObject(a, typeof(List<Responses.Status>));
                return d[0];
            }
        }

        int _wait = -1;
        /// <summary>
        /// Please see <seealso cref="Status"/> for more information
        /// </summary>
        public int Wait
        {
            get
            {
                return _wait;
            }
            set
            {
                _wait = value;
            }
        }

        /// <summary>
        /// Recieves an OAuth key from the Spotify site
        /// </summary>
        /// <returns></returns>
        public static string GetOAuth()
        {
            var wc = new WebClient();
            wc.Headers.Add("User-Agent: SpotifyAPI");
            var raw = wc.DownloadString("https://embed.spotify.com/openplay/?uri=spotify:track:5Zp4SWOpbuOdnsxLqwgutt");
            try {
                char[] charsToTrim = { '\'' }; 
                var line = Regex.Match(raw, @"tokenData ?= ?'[\w-]+',").Groups[0].Value;
                var token = Regex.Match(line, @"'[\w-]+'").Groups[0].Value.Trim(charsToTrim);
                return token;
            } catch(Exception e) {
                throw new Exception("Could not find OAuth token");    
            }
        }



        private string recv(string request)
        {
            return recv(request, false, false, -1);
        }

        private string recv(string request, bool oauth, bool cfid)
        {
            return recv(request, oauth, cfid, -1);
        }

        private string recv(string request, bool oauth, bool cfid, int wait)
        {
            string parameters = "?&ref=&cors=&_=" + TimeStamp;
            if (request.Contains("?"))
            {
                parameters = parameters.Substring(1);
            }

            if (oauth)
            {
                parameters += "&oauth=" + _oauth;
            }
            if (cfid)
            {
                parameters += "&csrf=" + _cfid;
            }

            if (wait != -1)
            {
                parameters += "&returnafter=" + wait;
                parameters += "&returnon=login%2Clogout%2Cplay%2Cpause%2Cerror%2Cap";
            }

            string a = "http://" + _host + ":4380/" + request + parameters;
            string derp = "";
            try
            {
                derp = wc.DownloadString(a);
                derp = "[ " + derp + " ]";
            }
            catch (System.Net.WebException e)
            {
                //perhaps spotifywebhelper isn't started (happens sometimes)
                if (Process.GetProcessesByName("SpotifyWebHelper").Length < 1)
                {
                    try
                    {
                        string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        System.Diagnostics.Process.Start(path + "\\Spotify\\SpotifyWebHelper.exe");
                        //Thread.Sleep(5000);

                    }
                    catch (Exception dd)
                    {
                        throw new Exception("Could not launch SpotifyWebHelper. Your installation of Spotify might be corrupt or you might not have Spotify installed", dd);
                    }

                    return recv(request, oauth, cfid);
                }
                //spotifywebhelper is running but we still can't connect, wtf?!
                else throw new Exception("Unable to connect to SpotifyWebHelper", e);
            }
            return derp;
        }

        /// <summary>
        /// Recieves client version information.
        /// Doesn't require a OAuth/CFID
        /// </summary>
        public Responses.ClientVersion ClientVersion
        {
            get
            {
                string a = recv("service/version.json?service=remote");
                List<Responses.ClientVersion> d = (List<Responses.ClientVersion>)JsonConvert.DeserializeObject(a, typeof(List<Responses.ClientVersion>));
                return d[0];
            }
        }
    }
}
