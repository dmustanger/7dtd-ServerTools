using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using UnityEngine;

namespace ServerTools.Website
{
    public partial class WebPanel
    {
        //1xx: Informational - Request received, continuing process

        //2xx: Success - The action was successfully received, understood, and accepted

        //3xx: Redirection - Further action must be taken in order to complete the request

        //4xx: Client Error - The request contains bad syntax or cannot be fulfilled

        //5xx: Server Error - The server failed to fulfill an apparently valid request:


        public static bool IsEnabled = false, IsRunning = false, DirFound = false;
        public static string SITE_DIR = "", ExternalIp = "", PostClient = "";
        public static int Port = 8084;
        private static string file = string.Format("WebPanelLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filePath = string.Format("{0}/Logs/WebPanelLogs/{1}", API.ConfigPath, file);
        private static HttpListener Listener = new HttpListener();
        private static readonly Version HttpVersion = new Version(1, 1);
        private static System.Random Rnd = new System.Random();
        private static Thread Thread;

        public static Dictionary<string, string[]> Authorized = new Dictionary<string, string[]>();
        public static Dictionary<string, DateTime> AuthorizedTime = new Dictionary<string, DateTime>();
        public static Dictionary<string, string[]> Visitor = new Dictionary<string, string[]>();
        public static Dictionary<string, int> PageHits = new Dictionary<string, int>();
        public static Dictionary<string, int> LoginAttempts = new Dictionary<string, int>();
        public static Dictionary<string, DateTime> TimeOut = new Dictionary<string, DateTime>();
        public static List<string> BannedIP = new List<string>();
        public static List<string> GETPassThrough = new List<string>();
        public static List<string> POSTFollowUp = new List<string>();

        public static void Load()
        {
            IsRunning = true;
            BuildLists();
            Start();
        }

        public static void Unload()
        {
            IsRunning = false;
            if (Listener.IsListening)
            {
                Listener.Stop();
                Listener.Close();
            }
            if (Thread.IsAlive)
            {
                Thread.Abort();
            }
        }

        private static void Start()
        {
            Thread = new Thread(new ThreadStart(Exec));
            Thread.IsBackground = true;
            Thread.Start();
        }

        public static void SetDirectory()
        {
            try
            {
                if (!DirFound)
                {
                    if (Directory.Exists(API.GamePath + "/Mods/ServerTools/WebPanel/"))
                    {
                        SITE_DIR = API.GamePath + "/Mods/ServerTools/WebPanel/";
                        DirFound = true;
                        return;
                    }
                    Log.Out("[SERVERTOOLS] Unable to verify ServerTools directory. Web panel failed to start");
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.Directory: {0}" + e.Message));
            }
        }

        public static void SetExternalIP()
        {
            try
            {
                ExternalIp = new WebClient().DownloadString("https://ipinfo.io/ip/");
                if (string.IsNullOrEmpty(ExternalIp))
                {
                    ExternalIp = new WebClient().DownloadString("https://api.ipify.org/");
                    if (string.IsNullOrEmpty(ExternalIp))
                    {
                        Log.Out("[SERVERTOOLS] Host IP could not be verified. Web panel has been disabled");
                        return;
                    }
                }
                ExternalIp = ExternalIp.RemoveLineBreaks();
                ExternalIp = ExternalIp.Trim();
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.SetExternalIP: {0}" + e.Message));
            }
        }


        private static void BuildLists()
        {
            try
            {
                List<string> _banList = PersistentContainer.Instance.WebPanelBanList;
                if (_banList != null && _banList.Count > 0)
                {
                    BannedIP = _banList;
                }
                Dictionary<string, DateTime> _timeoutList = PersistentContainer.Instance.WebPanelTimeoutList;
                if (_timeoutList != null && _timeoutList.Count > 0)
                {
                    for (int i = 0; i < _timeoutList.Count; i++)
                    {
                        KeyValuePair<string, DateTime> _expires = _timeoutList.ElementAt(i);
                        TimeSpan varTime = DateTime.Now - _expires.Value;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed >= 10)
                        {
                            _timeoutList.Remove(_expires.Key);
                        }
                    }
                    TimeOut = _timeoutList;
                }
                Dictionary<string, DateTime> _authorizedTimeList = PersistentContainer.Instance.WebsiteAuthorizedTimeList;
                Dictionary<string, string[]> _authorizedList = PersistentContainer.Instance.WebsiteAuthorizedList;
                if (_authorizedTimeList != null && _authorizedList != null && _authorizedTimeList.Count > 0 && _authorizedList.Count > 0)
                {
                    for (int i = 0; i < _authorizedTimeList.Count; i++)
                    {
                        KeyValuePair<string, DateTime> _expires = _authorizedTimeList.ElementAt(i);
                        TimeSpan varTime = DateTime.Now - _expires.Value;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed >= 30)
                        {
                            _authorizedTimeList.Remove(_expires.Key);
                            _authorizedList.Remove(_expires.Key);
                        }
                    }
                    AuthorizedTime = _authorizedTimeList;
                    Authorized = _authorizedList;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.BuildLists: {0}" + e.Message));
            }
        }

        public static string SetPassword()
        {
            string _newPassword = "";
            try
            {
                string _characters = "jJk9Kl3wWxXAbyYz0ZLmMn5NoO6dDe1EfFpPqQrRsStaBc2CgGhH7iITu4U8vV";
                System.Random _rnd = new System.Random();
                for (int i = 0; i < 8; i++)
                {
                    _newPassword += _characters.ElementAt(_rnd.Next(0, 62));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.SetPassword: {0}" + e.Message));
            }
            return _newPassword;
        }

        public static void Exec()
        {
            try
            {
                int _controlPanelPort = GamePrefs.GetInt(EnumGamePrefs.ControlPanelPort);
                int _telnetPanelPort = GamePrefs.GetInt(EnumGamePrefs.TelnetPort);
                if (Port == _controlPanelPort || Port == _telnetPanelPort)
                {
                    Log.Out("[SERVERTOOLS] Web_Panel port was set identically to the server control panel or telnet port. You must use a unique and unused port that is open to transmission. Panel has been disabled");
                    return;
                }
                if (Port > 1000 && Port < 65536)
                {
                    if (HttpListener.IsSupported)
                    {
                        Listener.Prefixes.Add(string.Format("http://*:{0}/", Port));
                        Listener.Prefixes.Add(string.Format("http://*:{0}/st.html/", Port));
                        Listener.Start();
                        string _page = "http://" + ExternalIp + ":" + Port.ToString() + "/st.html";
                        Log.Out(string.Format("[SERVERTOOLS] ServerTools web panel has opened on port {0}", Port.ToString()));
                        Log.Out(string.Format("[SERVERTOOLS] Use {0} or if accessing it locally you can use localhost instead of your server IP", _page));
                        while (Listener.IsListening)
                        {
                            HttpListenerContext _context = Listener.GetContext();
                            if (_context != null)
                            {
                                HttpListenerRequest _request = _context.Request;
                                HttpListenerResponse _response = _context.Response;
                                string _ip = _request.RemoteEndPoint.Address.ToString();
                                string _uri = _request.Url.AbsoluteUri;
                                if (_uri.Length > 111)
                                {
                                    Writer(string.Format("URI request was too long. Request denied for IP: {0}", _ip));
                                    _response.StatusCode = 414;
                                }
                                else if (GameManager.Instance.World != null)
                                {
                                    if (_uri.Contains("<script>"))
                                    {
                                        if (!BannedIP.Contains(_ip))
                                        {
                                            if (!_request.IsLocal)
                                            {
                                                BannedIP.Add(_ip);
                                                Writer(string.Format("Banned IP: {0} for attempting to run a script at address {1}", _ip, _uri));
                                                _response.StatusCode = 403;
                                            }
                                            else
                                            {
                                                Writer(string.Format("Detected local IP: {0}. Attempting to run a script at address {1}", _ip, _uri));
                                                _response.StatusCode = 403;
                                            }
                                        }
                                        else
                                        {
                                            Writer(string.Format("Detected banned IP: {0}. Attempting to run a script at address {1}", _ip, _uri));
                                            _response.StatusCode = 403;
                                        }
                                        return;
                                    }
                                    if (!BannedIP.Contains(_ip))
                                    {
                                        if (!TimeOut.ContainsKey(_ip))
                                        {
                                            IsAllowed(_request, _response, _ip, _uri);
                                        }
                                        else
                                        {
                                            TimeOut.TryGetValue(_ip, out DateTime _timeout);
                                            TimeSpan varTime = DateTime.Now - _timeout;
                                            double fractionalMinutes = varTime.TotalMinutes;
                                            int _timepassed = (int)fractionalMinutes;
                                            if (_timepassed >= 10)
                                            {
                                                TimeOut.Remove(_ip);
                                                PersistentContainer.Instance.WebPanelTimeoutList.Remove(_ip);
                                                IsAllowed(_request, _response, _ip, _uri);
                                            }
                                            else
                                            {
                                                Writer(string.Format("Request denied for IP: {0} on timeout until: {1}", _ip, _timeout));
                                                _response.StatusCode = 403;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Writer(string.Format("Request denied for banned IP: {0}", _ip));
                                        _response.StatusCode = 403;
                                    }
                                }
                                else
                                {
                                    Writer(string.Format("World data has not loaded. Request denied for IP: {0}", _ip));
                                    _response.StatusCode = 409;
                                }
                                _response.Close();
                                _context.Response.Close();
                            }
                        }
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] This host can not support the web panel. Panel has been disabled"));
                    }
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] The port is set to an invalid number. It must be between 1001-65531 for ServerTools web panel to function. Web panel has been disabled"));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.Exec: {0}" + e.Message));
            }
            if (Listener.IsListening)
            {
                Listener.Stop();
                Listener.Close();
            }
            if (Thread.IsAlive)
            {
                Thread.Abort();
            }
            Start();
        }

        private static void IsAllowed(HttpListenerRequest _request, HttpListenerResponse _response, string _ip, string _uri)
        {
            try
            {
                _uri = _uri.Remove(0, _uri.IndexOf(Port.ToString()) + Port.ToString().Length + 1);
                if (_request.HttpMethod == "GET")
                {
                    if (_uri.Contains("st.html"))
                    {
                        if (_uri.EndsWith("/"))
                        {
                            _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                            _response.StatusCode = 308;
                        }
                        else if (PageHits.ContainsKey(_ip))
                        {
                            PageHits.TryGetValue(_ip, out int _count);
                            if (_count++ >= 6)
                            {
                                PageHits.Remove(_ip);
                                if (!_request.IsLocal)
                                {
                                    TimeOut.Add(_ip, DateTime.Now);
                                    PersistentContainer.Instance.WebPanelTimeoutList.Add(_ip, DateTime.Now);
                                    Writer(string.Format("Homepage request denied for IP {0}. Client is now in time out", _ip));
                                    _response.StatusCode = 403;
                                }
                            }
                            else
                            {
                                PageHits[_ip] = _count + 1;
                                if (!GETPassThrough.Contains(_ip))
                                {
                                    GETPassThrough.Add(_ip);
                                }
                                Writer(string.Format("Homepage request granted for IP {0}", _ip));
                                GET(_response, _uri);
                            }
                        }
                        else
                        {
                            PageHits.Add(_ip, 1);
                            if (!GETPassThrough.Contains(_ip))
                            {
                                GETPassThrough.Add(_ip);
                            }
                            Writer(string.Format("Homepage request granted for IP {0}", _ip));
                            GET(_response, _uri);
                        }
                    }
                    else if (GETPassThrough.Contains(_ip))
                    {
                        GET(_response, _uri);
                    }
                    else
                    {
                        Writer(string.Format("Request denied: {0} for IP {1}", SITE_DIR + _uri, _ip));
                        _response.StatusCode = 403;
                    }
                }
                else if (_request.HttpMethod == "POST")
                {
                    if (_request.HasEntityBody)
                    {
                        string _postMessage = "";
                        using (Stream _body = _request.InputStream)
                        {
                            Encoding _encoding = _request.ContentEncoding;
                            using (StreamReader _read = new StreamReader(_body, _encoding))
                            {
                                _postMessage = _read.ReadToEnd();
                                _body.Close();
                                _read.Close();
                            }
                        }
                        POST(_request, _response, _uri, _postMessage);
                    }
                    else
                    {
                        Writer(string.Format("Received post request with missing body at IP {0}", _ip));
                        _response.StatusCode = 406;
                    }
                }
                else
                {
                    Writer(string.Format("Unaccepted method requested at IP {0}", _ip));
                    _response.StatusCode = 405;
                }
            }
            catch (Exception e)
            {
                if (Listener.IsListening)
                {
                    Listener.Stop();
                    Listener.Close();
                }
                if (Thread.IsAlive)
                {
                    Thread.Abort();
                }
                Start();
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.IsAllowed: {0}" + e.Message));
            }
            _response.Close();
        }

        private static void GET(HttpListenerResponse _response, string _uri)
        {
            try
            {
                string _location = SITE_DIR + _uri;
                if (_uri == "Config")
                {
                    _location = API.ConfigPath + "/ServerToolsConfig.xml";
                }
                FileInfo _fileInfo = new FileInfo(_location);
                if (_fileInfo != null && _fileInfo.Exists)
                {
                    byte[] _c = File.ReadAllBytes(_location);
                    if (_c != null)
                    {
                        _response.StatusCode = 200;
                        _response.SendChunked = false;
                        _response.ProtocolVersion = HttpVersion;
                        _response.KeepAlive = true;
                        _response.ContentType = MimeType.GetMimeType(Path.GetExtension(_fileInfo.Extension));
                        _response.ContentLength64 = (long)_c.Length;
                        _response.ContentEncoding = Encoding.UTF8;
                        _response.OutputStream.Write(_c, 0, _c.Length);
                    }
                    else
                    {
                        Writer(string.Format("Requested file was found but unable to form: {0}", _location));
                        _response.StatusCode = 404;
                    }
                }
                else
                {
                    Writer(string.Format("Requested file not found: {0}", _location));
                    _response.StatusCode = 404;
                }
            }
            catch (Exception e)
            {
                if (Listener.IsListening)
                {
                    Listener.Stop();
                    Listener.Close();
                }
                if (Thread.IsAlive)
                {
                    Thread.Abort();
                }
                Start();
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.GET: {0}" + e.Message));
            }
            _response.Close();
        }

        private static void POST(HttpListenerRequest _request, HttpListenerResponse _response, string _uri, string _postMessage)
        {
            try
            {
                string _ip = _request.RemoteEndPoint.Address.ToString();
                if (_postMessage.Length >= 8)
                {
                    string _responseMessage = "";
                    if (_uri == "Handshake")
                    {
                        if (!Authorized.ContainsKey(_postMessage))
                        {
                            if (!Visitor.ContainsKey(_postMessage))
                            {
                                string _newKey = "";
                                for (int i = 0; i < 249; i++)
                                {
                                    _newKey += Rnd.Next(12345678, 87654321).ToString();
                                }
                                string[] _clientHandle = { "", _newKey, "" };
                                Visitor.Add(_postMessage, _clientHandle);
                                _responseMessage = _newKey.Insert(0, _postMessage);
                                _response.StatusCode = 200;
                            }
                            else
                            {
                                _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                _response.StatusCode = 401;
                            }
                        }
                        else
                        {
                            _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "Secure")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (Visitor.ContainsKey(_clientId))
                        {
                            Visitor.TryGetValue(_clientId, out string[] _clientHandle);
                            string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                            string _newKey = "";
                            for (int i = 0; i < 249; i++)
                            {
                                _newKey += Rnd.Next(12345678, 87654321).ToString();
                            }
                            _clientHandle[0] = _decrypt;
                            _clientHandle[1] = _newKey;
                            Visitor[_clientId] = _clientHandle;
                            _responseMessage = _newKey.Insert(0, _clientId);
                            _response.StatusCode = 200;
                            Writer(string.Format("Secure handshake established at IP {0}", _ip));
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "SignIn")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (Visitor.ContainsKey(_clientId))
                        {
                            Visitor.TryGetValue(_clientId, out string[] _clientHandle);
                            string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                            if (_decrypt.Contains("`"))
                            {
                                string[] _decryptSplit = _decrypt.Split('`');
                                if (_decryptSplit[0] == _clientHandle[0])
                                {
                                    if (!string.IsNullOrEmpty(_decryptSplit[1]) && !string.IsNullOrEmpty(_decryptSplit[2]))
                                    {
                                        if (_decryptSplit[2] == PersistentContainer.Instance.Players[_decryptSplit[1]].WP)
                                        {
                                            if (LoginAttempts.ContainsKey(_ip))
                                            {
                                                LoginAttempts.Remove(_ip);
                                            }
                                            Visitor.Remove(_clientId);
                                            PageHits.Remove(_ip);
                                            _clientHandle[2] = _decryptSplit[1];
                                            Authorized.Add(_clientId, _clientHandle);
                                            AuthorizedTime.Add(_clientId, DateTime.Now);
                                            _responseMessage += SetKey(_clientId, _clientHandle);
                                            _response.StatusCode = 200;
                                            Writer(string.Format("Client {0} at IP {1} has signed in", _clientHandle[2], _ip));
                                        }
                                        else
                                        {
                                            if (LoginAttempts.ContainsKey(_ip))
                                            {
                                                LoginAttempts.TryGetValue(_ip, out int _count);
                                                if (_count + 1 == 3)
                                                {
                                                    LoginAttempts.Remove(_ip);
                                                    TimeOut.Add(_ip, DateTime.Now);
                                                    PersistentContainer.Instance.WebPanelTimeoutList = TimeOut;
                                                }
                                                else
                                                {
                                                    LoginAttempts[_ip] = _count + 1;
                                                }
                                            }
                                            else
                                            {
                                                LoginAttempts.Add(_ip, 1);
                                            }
                                            string _newKey = "";
                                            for (int i = 0; i < 249; i++)
                                            {
                                                _newKey += Rnd.Next(12345678, 87654321).ToString();
                                            }
                                            _clientHandle[1] = _newKey;
                                            Visitor[_clientId] = _clientHandle;
                                            _responseMessage = _newKey.Insert(0, _clientId);
                                            _response.StatusCode = 401;
                                            Writer(string.Format("Sign in failure at IP {0} for client ID {1}", _ip, _decryptSplit[1]));
                                        }
                                    }
                                    else
                                    {
                                        Visitor.Remove(_ip);
                                        GETPassThrough.Remove(_ip);
                                        if (LoginAttempts.ContainsKey(_ip))
                                        {
                                            LoginAttempts.Remove(_ip);
                                        }
                                        if (!_request.IsLocal)
                                        {
                                            BannedIP.Add(_ip);
                                            Writer(string.Format("Client at IP {0} has been banned", _ip));
                                        }
                                        _response.StatusCode = 403;
                                    }
                                }
                                else
                                {
                                    Visitor.Remove(_ip);
                                    GETPassThrough.Remove(_ip);
                                    if (LoginAttempts.ContainsKey(_ip))
                                    {
                                        LoginAttempts.Remove(_ip);
                                    }
                                    if (!_request.IsLocal)
                                    {
                                        BannedIP.Add(_ip);
                                        Writer(string.Format("Client at IP {0} has been banned", _ip));
                                    }
                                    _response.StatusCode = 403;
                                }
                            }
                            else
                            {
                                Visitor.Remove(_ip);
                                GETPassThrough.Remove(_ip);
                                if (LoginAttempts.ContainsKey(_ip))
                                {
                                    LoginAttempts.Remove(_ip);
                                }
                                if (!_request.IsLocal)
                                {
                                    BannedIP.Add(_ip);
                                    Writer(string.Format("Client at IP {0} has been banned", _ip));
                                }
                                _response.StatusCode = 403;
                            }
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "SignOut")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (Authorized.ContainsKey(_clientId))
                        {
                            AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                            if (Expired(_expires))
                            {
                                Authorized.Remove(_clientId);
                                AuthorizedTime.Remove(_clientId);
                                GETPassThrough.Remove(_ip);
                                _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                _response.StatusCode = 401;
                            }
                            else
                            {
                                Authorized.TryGetValue(_clientId, out string[] _clientHandle);
                                string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                                if (_decrypt == _clientHandle[0])
                                {
                                    AuthorizedTime.Remove(_clientId);
                                    Authorized.Remove(_clientId);
                                    GETPassThrough.Remove(_ip);
                                    _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                    _response.StatusCode = 200;
                                    Writer(string.Format("Client {0} at IP {1} has signed out", _clientHandle[2], _ip));
                                }
                                else
                                {
                                    AuthorizedTime.Remove(_clientId);
                                    Authorized.Remove(_clientId);
                                    GETPassThrough.Remove(_ip);
                                    if (!_request.IsLocal)
                                    {
                                        BannedIP.Add(_ip);
                                        Writer(string.Format("Client at IP {0} has been banned", _ip));
                                    }
                                    _response.StatusCode = 403;
                                }
                            }
                        }
                        else
                        {
                            _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "NewPass")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (Authorized.ContainsKey(_clientId))
                        {
                            AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                            if (Expired(_expires))
                            {
                                Authorized.Remove(_clientId);
                                AuthorizedTime.Remove(_clientId);
                                GETPassThrough.Remove(_ip);
                                _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                _response.StatusCode = 401;
                            }
                            else
                            {
                                AuthorizedTime[_clientId] = DateTime.Now;
                                Authorized.TryGetValue(_clientId, out string[] _clientHandle);
                                string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                                if (_decrypt == _clientHandle[0])
                                {
                                    _responseMessage += SetKey(_clientId, _clientHandle);
                                    _response.StatusCode = 200;
                                }
                                else
                                {
                                    AuthorizedTime.Remove(_clientId);
                                    Authorized.Remove(_clientId);
                                    GETPassThrough.Remove(_ip);
                                    if (!_request.IsLocal)
                                    {
                                        BannedIP.Add(_ip);
                                        Writer(string.Format("Client at IP {0} has been banned", _ip));
                                    }
                                    _response.StatusCode = 403;
                                }
                            }
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "SetPass")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (Authorized.ContainsKey(_clientId))
                        {
                            AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                            if (Expired(_expires))
                            {
                                Authorized.Remove(_clientId);
                                AuthorizedTime.Remove(_clientId);
                                GETPassThrough.Remove(_ip);
                                _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                _response.StatusCode = 401;
                            }
                            else
                            {
                                AuthorizedTime[_clientId] = DateTime.Now;
                                Authorized.TryGetValue(_clientId, out string[] _clientHandle);
                                string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                                if (_decrypt.Contains('`'))
                                {
                                    string[] _decryptSplit = _decrypt.Split('`');
                                    if (_decryptSplit[0] == _clientHandle[0])
                                    {
                                        if (!string.IsNullOrEmpty(_decryptSplit[1]))
                                        {
                                            PersistentContainer.Instance.Players[_clientHandle[2]].WP = _decryptSplit[1];
                                            _responseMessage += SetKey(_clientId, _clientHandle);
                                            _response.StatusCode = 200;
                                            Writer(string.Format("Client {0} at IP {1} has set a new password", _clientHandle[2], _ip));
                                        }
                                        else
                                        {
                                            AuthorizedTime.Remove(_clientId);
                                            Authorized.Remove(_clientId);
                                            GETPassThrough.Remove(_ip);
                                            if (!_request.IsLocal)
                                            {
                                                BannedIP.Add(_ip);
                                                Writer(string.Format("Client at IP {0} has been banned", _ip));
                                            }
                                            _response.StatusCode = 403;
                                        }
                                    }
                                    else
                                    {
                                        AuthorizedTime.Remove(_clientId);
                                        Authorized.Remove(_clientId);
                                        GETPassThrough.Remove(_ip);
                                        if (!_request.IsLocal)
                                        {
                                            BannedIP.Add(_ip);
                                            Writer(string.Format("Client at IP {0} has been banned", _ip));
                                        }
                                        _response.StatusCode = 403;
                                    }
                                }
                                else
                                {
                                    AuthorizedTime.Remove(_clientId);
                                    Authorized.Remove(_clientId);
                                    GETPassThrough.Remove(_ip);
                                    if (!_request.IsLocal)
                                    {
                                        BannedIP.Add(_ip);
                                        Writer(string.Format("Client at IP {0} has been banned", _ip));
                                    }
                                    _response.StatusCode = 403;
                                }
                            }
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "Console")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (!POSTFollowUp.Contains(_ip))
                        {
                            if (Authorized.ContainsKey(_clientId))
                            {
                                AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                if (Expired(_expires))
                                {
                                    Authorized.Remove(_clientId);
                                    AuthorizedTime.Remove(_clientId);
                                    GETPassThrough.Remove(_ip);
                                    _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                    _response.StatusCode = 401;
                                }
                                else
                                {
                                    AuthorizedTime[_clientId] = DateTime.Now;
                                    Authorized.TryGetValue(_clientId, out string[] _clientHandle);
                                    string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                                    if (_decrypt == _clientHandle[0])
                                    {
                                        POSTFollowUp.Add(_ip);
                                        _responseMessage += SetKey(_clientId, _clientHandle);
                                        _response.StatusCode = 200;
                                    }
                                    else
                                    {
                                        AuthorizedTime.Remove(_clientId);
                                        Authorized.Remove(_clientId);
                                        GETPassThrough.Remove(_ip);
                                        if (!_request.IsLocal)
                                        {
                                            BannedIP.Add(_ip);
                                            Writer(string.Format("Client at IP {0} has been banned", _ip));
                                        }
                                        _response.StatusCode = 403;
                                    }
                                }
                            }
                            else
                            {
                                _response.StatusCode = 401;
                            }
                        }
                        else
                        {
                            POSTFollowUp.Remove(_ip);
                            int.TryParse(_postMessage.Substring(8), out int _lineNumber);
                            int _logCount = OutputLog.ActiveLog.Count;
                            if (_logCount >= _lineNumber + 1)
                            {
                                for (int i = _lineNumber; i < _logCount; i++)
                                {
                                    _responseMessage += OutputLog.ActiveLog[i] + "\n";
                                }
                                _responseMessage += "☼" + _logCount;
                            }
                            _response.StatusCode = 200;
                        }
                    }
                    else if (_uri == "Command")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (!POSTFollowUp.Contains(_ip))
                        {
                            if (Authorized.ContainsKey(_clientId))
                            {
                                AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                if (Expired(_expires))
                                {
                                    Authorized.Remove(_clientId);
                                    AuthorizedTime.Remove(_clientId);
                                    _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                    _response.StatusCode = 401;
                                }
                                else
                                {
                                    Authorized.TryGetValue(_clientId, out string[] _clientHandle);
                                    string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                                    if (_decrypt == _clientHandle[0])
                                    {
                                        POSTFollowUp.Add(_ip);
                                        _responseMessage += SetKey(_clientId, _clientHandle);
                                        _response.StatusCode = 200;
                                    }
                                    else
                                    {
                                        AuthorizedTime.Remove(_clientId);
                                        Authorized.Remove(_clientId);
                                        GETPassThrough.Remove(_ip);
                                        if (!_request.IsLocal)
                                        {
                                            BannedIP.Add(_ip);
                                            Writer(string.Format("Client at IP {0} has been banned", _ip));
                                        }
                                        _response.StatusCode = 403;
                                    }
                                }
                            }
                            else
                            {
                                _response.StatusCode = 401;
                            }
                        }
                        else
                        {
                            POSTFollowUp.Remove(_ip);
                            string[] idLineCountandCommand = _postMessage.Split(new[] { '`' }, 3);
                            Authorized.TryGetValue(idLineCountandCommand[0], out string[] _clientHandle);
                            string _command = idLineCountandCommand[2];
                            if (_command.Length > 300)
                            {
                                _responseMessage += "300 characters is the command limit. You tried to execute a command with " + _command.Length + " characters.";
                                _response.StatusCode = 400;
                            }
                            else
                            {
                                IConsoleCommand _commandValid = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(_command, false);
                                if (_commandValid == null)
                                {
                                    _responseMessage += "Unknown command";
                                    _response.StatusCode = 406;
                                }
                                else
                                {
                                    ClientInfo _cInfo = new ClientInfo();
                                    _cInfo.playerId = "-Web_Panel-" + _clientHandle[2];
                                    _cInfo.entityId = -1;
                                    List<string> _cmdReponse = SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(_command, _cInfo);
                                    int _logCount = OutputLog.ActiveLog.Count;
                                    int.TryParse(idLineCountandCommand[1], out int _lineNumber);
                                    if (_logCount >= _lineNumber + 1)
                                    {
                                        for (int i = _lineNumber; i < _logCount; i++)
                                        {
                                            _responseMessage += OutputLog.ActiveLog[i] + "\n";
                                        }
                                    }
                                    for (int i = 0; i < _cmdReponse.Count; i++)
                                    {
                                        _responseMessage += _cmdReponse[i] + "\n";
                                    }
                                    _responseMessage += "☼" + OutputLog.ActiveLog.Count;
                                    Writer(string.Format("Executed console command '{0}' from Client: {1} IP: {2}", _command, _clientHandle[2], _ip));
                                    _response.StatusCode = 200;
                                }
                            }
                        }
                    }
                    else if (_uri == "Players")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (!POSTFollowUp.Contains(_ip))
                        {
                            if (Authorized.ContainsKey(_clientId))
                            {
                                AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                if (Expired(_expires))
                                {
                                    Authorized.Remove(_clientId);
                                    AuthorizedTime.Remove(_clientId);
                                    GETPassThrough.Remove(_ip);
                                    _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                    _response.StatusCode = 401;
                                }
                                else
                                {
                                    AuthorizedTime[_clientId] = DateTime.Now;
                                    Authorized.TryGetValue(_clientId, out string[] _clientHandle);
                                    string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                                    if (_decrypt == _clientHandle[0])
                                    {
                                        POSTFollowUp.Add(_ip);
                                        _responseMessage += SetKey(_clientId, _clientHandle);
                                        _response.StatusCode = 200;
                                    }
                                    else
                                    {
                                        AuthorizedTime.Remove(_clientId);
                                        Authorized.Remove(_clientId);
                                        GETPassThrough.Remove(_ip);
                                        if (!_request.IsLocal)
                                        {
                                            BannedIP.Add(_ip);
                                            Writer(string.Format("Client at IP {0} has been banned", _ip));
                                        }
                                        _response.StatusCode = 403;
                                    }
                                }
                            }
                            else
                            {
                                _response.StatusCode = 401;
                            }
                        }
                        else
                        {
                            POSTFollowUp.Remove(_ip);
                            List<ClientInfo> _clientList = PersistentOperations.ClientList();
                            if (_clientList != null && _clientList.Count > 0)
                            {
                                for (int i = 0; i < _clientList.Count; i++)
                                {
                                    ClientInfo _cInfo = _clientList[i];
                                    if (_cInfo != null)
                                    {
                                        EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                                        if (_player != null && _player.Progression != null)
                                        {
                                            if (_cInfo.playerName.Contains("☼") || _cInfo.playerName.Contains("§") || _cInfo.playerName.Contains("/"))
                                            {
                                                _responseMessage += _cInfo.playerId + "/" + _cInfo.entityId + "§" + "<Invalid Chars>" + "§"
                                                    + _player.Health + "/" + (int)_player.Stamina + "§" + _player.Progression.Level + "§"
                                                    + (int)_player.position.x + "," + (int)_player.position.y + "," + (int)_player.position.z;
                                            }
                                            else
                                            {
                                                _responseMessage += _cInfo.playerId + "/" + _cInfo.entityId + "§" + _cInfo.playerName + "§"
                                                    + _player.Health + "/" + (int)_player.Stamina + "§" + _player.Progression.Level + "§"
                                                    + (int)_player.position.x + "," + (int)_player.position.y + "," + (int)_player.position.z;
                                            }
                                            if (Mute.IsEnabled && Mute.Mutes.Contains(_cInfo.playerId))
                                            {
                                                _responseMessage += "§" + "True";
                                            }
                                            else
                                            {
                                                _responseMessage += "§" + "False";
                                            }
                                            if (Jail.IsEnabled && Jail.Jailed.Contains(_cInfo.playerId))
                                            {
                                                _responseMessage += "/" + "True" + "☼";
                                            }
                                            else
                                            {
                                                _responseMessage += "/" + "False" + "☼";
                                            }
                                        }
                                    }
                                }
                                _responseMessage = _responseMessage.TrimEnd('☼');
                                _response.StatusCode = 200;

                            }
                            else
                            {
                                _response.StatusCode = 200;
                            }
                        }
                    }
                    else if (_uri == "Config")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (Authorized.ContainsKey(_clientId))
                        {
                            AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                            if (Expired(_expires))
                            {
                                Authorized.Remove(_clientId);
                                AuthorizedTime.Remove(_clientId);
                                GETPassThrough.Remove(_ip);
                                _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                _response.StatusCode = 401;
                            }
                            else
                            {
                                AuthorizedTime[_clientId] = DateTime.Now;
                                Authorized.TryGetValue(_clientId, out string[] _clientHandle);
                                string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                                if (_decrypt == _clientHandle[0])
                                {
                                    _responseMessage += SetKey(_clientId, _clientHandle);
                                    _response.StatusCode = 200;
                                }
                                else
                                {
                                    AuthorizedTime.Remove(_clientId);
                                    Authorized.Remove(_clientId);
                                    GETPassThrough.Remove(_ip);
                                    if (!_request.IsLocal)
                                    {
                                        BannedIP.Add(_ip);
                                        Writer(string.Format("Client at IP {0} has been banned", _ip));
                                    }
                                    _response.StatusCode = 403;
                                }
                            }
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "UpdateConfig")
                    {
                        if (!POSTFollowUp.Contains(_ip))
                        {
                            string _clientId = _postMessage.Substring(0, 8);
                            if (Authorized.ContainsKey(_clientId))
                            {
                                AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                if (Expired(_expires))
                                {
                                    Authorized.Remove(_clientId);
                                    AuthorizedTime.Remove(_clientId);
                                    _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                    _response.StatusCode = 401;
                                }
                                else
                                {
                                    AuthorizedTime[_clientId] = DateTime.Now;
                                    Authorized.TryGetValue(_clientId, out string[] _clientHandle);
                                    string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                                    if (_decrypt == _clientHandle[0])
                                    {
                                        PostClient = _clientHandle[2];
                                        POSTFollowUp.Add(_ip);
                                        _responseMessage += SetKey(_clientId, _clientHandle);
                                        _response.StatusCode = 200;
                                    }
                                    else
                                    {
                                        AuthorizedTime.Remove(_clientId);
                                        Authorized.Remove(_clientId);
                                        GETPassThrough.Remove(_ip);
                                        if (!_request.IsLocal)
                                        {
                                            BannedIP.Add(_ip);
                                            Writer(string.Format("Client at IP {0} has been banned", _ip));
                                        }
                                        _response.StatusCode = 403;
                                    }
                                }
                            }
                            else
                            {
                                _response.StatusCode = 403;
                            }
                        }
                        else
                        {
                            POSTFollowUp.Remove(_ip);
                            XmlDocument _xmlDoc = new XmlDocument();
                            try
                            {
                                _xmlDoc.Load(Config.configFilePath);
                            }
                            catch (XmlException e)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", Config.configFilePath, e.Message));
                            }
                            if (_xmlDoc != null)
                            {
                                bool _changed = false;
                                XmlNodeList _nodes = _xmlDoc.GetElementsByTagName("Tool");
                                _postMessage = _postMessage.TrimEnd('☼');
                                string[] _tools = _postMessage.Split('☼');
                                for (int i = 0; i < _tools.Length; i++)
                                {
                                    XmlNode _node = _nodes[i];
                                    string[] _nameAndOptions = _tools[i].Split('§');
                                    if (_nameAndOptions[1].Contains("╚"))
                                    {
                                        string[] _options = _nameAndOptions[1].Split('╚');
                                        for (int j = 0; j < _options.Length; j++)
                                        {
                                            string[] _optionNameAndValue = _options[j].Split('σ');
                                            int _nodePosition = j + 1;
                                            if (_nameAndOptions[0] == _node.Attributes[0].Value && _optionNameAndValue[0] == _node.Attributes[_nodePosition].Name && _optionNameAndValue[1] != _node.Attributes[_nodePosition].Value)
                                            {
                                                _changed = true;
                                                _node.Attributes[_nodePosition].Value = _optionNameAndValue[1];
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string[] _optionNameAndValue = _nameAndOptions[1].Split('σ');
                                        if (_nameAndOptions[0] == _node.Attributes[0].Value && _optionNameAndValue[0] == _node.Attributes[1].Name && _optionNameAndValue[1] != _node.Attributes[1].Value)
                                        {
                                            _changed = true;
                                            _node.Attributes[1].Value = _optionNameAndValue[1];
                                        }
                                    }
                                }
                                if (_changed)
                                {
                                    _xmlDoc.Save(Config.configFilePath);
                                    Writer(string.Format("Client {0} at IP {1} has updated the Config ", PostClient, _ip));
                                    _response.StatusCode = 200;
                                }
                                else
                                {
                                    _response.StatusCode = 406;
                                }
                            }
                        }
                    }
                    else if (_uri == "Kick")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (Authorized.ContainsKey(_clientId))
                        {
                            AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                            if (Expired(_expires))
                            {
                                Authorized.Remove(_clientId);
                                AuthorizedTime.Remove(_clientId);
                                GETPassThrough.Remove(_ip);
                                _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                _response.StatusCode = 401;
                            }
                            else
                            {
                                AuthorizedTime[_clientId] = DateTime.Now;
                                Authorized.TryGetValue(_clientId, out string[] _clientHandle);
                                string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                                if (_decrypt.Contains('`'))
                                {
                                    string[] _decryptSplit = _decrypt.Split('`');
                                    if (_decryptSplit[0] == _clientHandle[0])
                                    {
                                        if (!string.IsNullOrEmpty(_decryptSplit[1]))
                                        {
                                            _responseMessage += SetKey(_clientId, _clientHandle);
                                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_decryptSplit[1]);
                                            if (_cInfo != null)
                                            {
                                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0}", _cInfo.playerId), null);
                                                Writer(string.Format("Client {0} at IP {1} has kicked {2}", _clientHandle[2], _ip, _cInfo.playerId));
                                                _response.StatusCode = 200;
                                            }
                                            else
                                            {
                                                _response.StatusCode = 406;
                                            }
                                        }
                                        else
                                        {
                                            AuthorizedTime.Remove(_clientId);
                                            Authorized.Remove(_clientId);
                                            GETPassThrough.Remove(_ip);
                                            if (!_request.IsLocal)
                                            {
                                                BannedIP.Add(_ip);
                                                Writer(string.Format("Client at IP {0} has been banned", _ip));
                                            }
                                            _response.StatusCode = 403;
                                        }
                                    }
                                    else
                                    {
                                        AuthorizedTime.Remove(_clientId);
                                        Authorized.Remove(_clientId);
                                        GETPassThrough.Remove(_ip);
                                        if (!_request.IsLocal)
                                        {
                                            BannedIP.Add(_ip);
                                            Writer(string.Format("Client at IP {0} has been banned", _ip));
                                        }
                                        _response.StatusCode = 403;
                                    }
                                }
                                else
                                {
                                    AuthorizedTime.Remove(_clientId);
                                    Authorized.Remove(_clientId);
                                    GETPassThrough.Remove(_ip);
                                    if (!_request.IsLocal)
                                    {
                                        BannedIP.Add(_ip);
                                        Writer(string.Format("Client at IP {0} has been banned", _ip));
                                    }
                                    _response.StatusCode = 403;
                                }
                            }
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "Ban")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (Authorized.ContainsKey(_clientId))
                        {
                            AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                            if (Expired(_expires))
                            {
                                Authorized.Remove(_clientId);
                                AuthorizedTime.Remove(_clientId);
                                GETPassThrough.Remove(_ip);
                                _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                _response.StatusCode = 401;
                            }
                            else
                            {
                                AuthorizedTime[_clientId] = DateTime.Now;
                                Authorized.TryGetValue(_clientId, out string[] _clientHandle);
                                string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                                if (_decrypt.Contains('`'))
                                {
                                    string[] _decryptSplit = _decrypt.Split('`');
                                    if (_decryptSplit[0] == _clientHandle[0])
                                    {
                                        if (!string.IsNullOrEmpty(_decryptSplit[1]))
                                        {
                                            _responseMessage += SetKey(_clientId, _clientHandle);
                                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_decryptSplit[1]);
                                            if (_cInfo != null)
                                            {
                                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 year", _cInfo.playerId), null);
                                                Writer(string.Format("Client {0} at IP {1} has banned id {2} named {3}", _clientHandle[2], _ip, _cInfo.playerId, _cInfo.playerName));
                                                _response.StatusCode = 200;
                                            }
                                            else
                                            {
                                                PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_decryptSplit[1]);
                                                if (_pdf != null)
                                                {
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 year", _decryptSplit[1]), null);
                                                    Writer(string.Format("Client {0} at IP {1} has banned id {2} named {3}", _clientHandle[2], _ip, _decryptSplit[1], _pdf.ecd.entityName));
                                                    _response.StatusCode = 406;
                                                }
                                                else
                                                {
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 year", _decryptSplit[1]), null);
                                                    Writer(string.Format("Client {0} at IP {1} has banned id {2}", _clientHandle[2], _ip, _decryptSplit[1]));
                                                    _response.StatusCode = 406;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            AuthorizedTime.Remove(_clientId);
                                            Authorized.Remove(_clientId);
                                            GETPassThrough.Remove(_ip);
                                            if (!_request.IsLocal)
                                            {
                                                BannedIP.Add(_ip);
                                                Writer(string.Format("Client at IP {0} has been banned", _ip));
                                            }
                                            _response.StatusCode = 403;
                                        }
                                    }
                                    else
                                    {
                                        AuthorizedTime.Remove(_clientId);
                                        Authorized.Remove(_clientId);
                                        GETPassThrough.Remove(_ip);
                                        if (!_request.IsLocal)
                                        {
                                            BannedIP.Add(_ip);
                                            Writer(string.Format("Client at IP {0} has been banned", _ip));
                                        }
                                        _response.StatusCode = 403;
                                    }
                                }
                                else
                                {
                                    AuthorizedTime.Remove(_clientId);
                                    Authorized.Remove(_clientId);
                                    GETPassThrough.Remove(_ip);
                                    if (!_request.IsLocal)
                                    {
                                        BannedIP.Add(_ip);
                                        Writer(string.Format("Client at IP {0} has been banned", _ip));
                                    }
                                    _response.StatusCode = 403;
                                }
                            }
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "Mute")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (Authorized.ContainsKey(_clientId))
                        {
                            AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                            if (Expired(_expires))
                            {
                                Authorized.Remove(_clientId);
                                AuthorizedTime.Remove(_clientId);
                                GETPassThrough.Remove(_ip);
                                _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                _response.StatusCode = 401;
                            }
                            else
                            {
                                AuthorizedTime[_clientId] = DateTime.Now;
                                Authorized.TryGetValue(_clientId, out string[] _clientHandle);
                                string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                                if (_decrypt.Contains('`'))
                                {
                                    string[] _decryptSplit = _decrypt.Split('`');
                                    if (_decryptSplit[0] == _clientHandle[0])
                                    {
                                        if (!string.IsNullOrEmpty(_decryptSplit[1]))
                                        {
                                            _responseMessage += SetKey(_clientId, _clientHandle);
                                            if (Mute.IsEnabled)
                                            {
                                                ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_decryptSplit[1]);
                                                if (_cInfo != null)
                                                {
                                                    if (Mute.Mutes.Contains(_cInfo.playerId))
                                                    {
                                                        Mute.Mutes.Remove(_cInfo.playerId);
                                                        PersistentContainer.Instance.Players[_cInfo.playerId].MuteTime = 0;
                                                        Writer(string.Format("Client {0} at IP {1} has unmuted id {2} named {3}", _clientHandle[2], _ip, _cInfo.playerId, _cInfo.playerName));
                                                        _response.StatusCode = 202;
                                                    }
                                                    else
                                                    {
                                                        Mute.Mutes.Add(_cInfo.playerId);
                                                        PersistentContainer.Instance.Players[_cInfo.playerId].MuteTime = -1;
                                                        PersistentContainer.Instance.Players[_cInfo.playerId].MuteName = _cInfo.playerName;
                                                        PersistentContainer.Instance.Players[_cInfo.playerId].MuteDate = DateTime.Now;
                                                        Writer(string.Format("Client {0} at IP {1} has muted id {2} named {3}", _clientHandle[2], _ip, _cInfo.playerId, _cInfo.playerName));
                                                        _response.StatusCode = 200;
                                                    }
                                                }
                                                else
                                                {
                                                    if (Mute.Mutes.Contains(_decryptSplit[1]))
                                                    {
                                                        Mute.Mutes.Remove(_decryptSplit[1]);
                                                        PersistentContainer.Instance.Players[_decryptSplit[1]].MuteTime = 0;
                                                        Writer(string.Format("Client {0} at IP {1} has unmuted id {2}", _clientHandle[2], _ip, _decryptSplit[1]));
                                                        _response.StatusCode = 202;
                                                    }
                                                    else
                                                    {
                                                        Mute.Mutes.Add(_decryptSplit[1]);
                                                        PersistentContainer.Instance.Players[_decryptSplit[1]].MuteTime = -1;
                                                        PersistentContainer.Instance.Players[_decryptSplit[1]].MuteName = "-Unknown-";
                                                        PersistentContainer.Instance.Players[_decryptSplit[1]].MuteDate = DateTime.Now;
                                                        Writer(string.Format("Client {0} at IP {1} has muted id {2}", _clientHandle[2], _ip, _decryptSplit[1]));
                                                        _response.StatusCode = 200;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                _response.StatusCode = 406;
                                            }
                                        }
                                        else
                                        {
                                            AuthorizedTime.Remove(_clientId);
                                            Authorized.Remove(_clientId);
                                            GETPassThrough.Remove(_ip);
                                            if (!_request.IsLocal)
                                            {
                                                BannedIP.Add(_ip);
                                                Writer(string.Format("Client at IP {0} has been banned", _ip));
                                            }
                                            _response.StatusCode = 403;
                                        }
                                    }
                                    else
                                    {
                                        AuthorizedTime.Remove(_clientId);
                                        Authorized.Remove(_clientId);
                                        GETPassThrough.Remove(_ip);
                                        if (!_request.IsLocal)
                                        {
                                            BannedIP.Add(_ip);
                                            Writer(string.Format("Client at IP {0} has been banned", _ip));
                                        }
                                        _response.StatusCode = 403;
                                    }
                                }
                                else
                                {
                                    AuthorizedTime.Remove(_clientId);
                                    Authorized.Remove(_clientId);
                                    GETPassThrough.Remove(_ip);
                                    if (!_request.IsLocal)
                                    {
                                        BannedIP.Add(_ip);
                                        Writer(string.Format("Client at IP {0} has been banned", _ip));
                                    }
                                    _response.StatusCode = 403;
                                }
                            }
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "Jail")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (Authorized.ContainsKey(_clientId))
                        {
                            AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                            if (Expired(_expires))
                            {
                                Authorized.Remove(_clientId);
                                AuthorizedTime.Remove(_clientId);
                                GETPassThrough.Remove(_ip);
                                _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                _response.StatusCode = 401;
                            }
                            else
                            {
                                AuthorizedTime[_clientId] = DateTime.Now;
                                Authorized.TryGetValue(_clientId, out string[] _clientHandle);
                                string _decrypt = Decrypt(_clientHandle[1], _postMessage.Substring(8));
                                if (_decrypt.Contains('`'))
                                {
                                    string[] _decryptSplit = _decrypt.Split('`');
                                    if (_decryptSplit[0] == _clientHandle[0])
                                    {
                                        if (!string.IsNullOrEmpty(_decryptSplit[1]))
                                        {
                                            _responseMessage += SetKey(_clientId, _clientHandle);
                                            if (Jail.IsEnabled)
                                            {
                                                ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_decryptSplit[1]);
                                                if (_cInfo != null)
                                                {
                                                    if (Jail.Jailed.Contains(_cInfo.playerId))
                                                    {
                                                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                        if (_player != null)
                                                        {
                                                            EntityBedrollPositionList _position = _player.SpawnPoints;
                                                            Jail.Jailed.Remove(_cInfo.playerId);
                                                            PersistentContainer.Instance.Players[_cInfo.playerId].JailTime = 0;
                                                            if (_position != null && _position.Count > 0)
                                                            {
                                                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_position[0].x, -1, _position[0].z), null, false));
                                                            }
                                                            else
                                                            {
                                                                Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_pos[0].x, -1, _pos[0].z), null, false));
                                                            }
                                                            Writer(string.Format("Client {0} at IP {1} has unjailed {2} named {3}", _clientHandle[2], _ip, _cInfo.playerId, _cInfo.playerName));
                                                            _response.StatusCode = 200;
                                                        }
                                                    }
                                                    else if (Jail.Jail_Position != "")
                                                    {
                                                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                        if (_player != null && _player.IsSpawned())
                                                        {
                                                            if (Jail.Jail_Position.Contains(","))
                                                            {
                                                                string[] _cords = Jail.Jail_Position.Split(',');
                                                                int.TryParse(_cords[0], out int _x);
                                                                int.TryParse(_cords[1], out int _y);
                                                                int.TryParse(_cords[2], out int _z);
                                                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                                                            }
                                                        }
                                                        Jail.Jailed.Add(_cInfo.playerId);
                                                        PersistentContainer.Instance.Players[_cInfo.playerId].JailTime = -1;
                                                        PersistentContainer.Instance.Players[_cInfo.playerId].JailName = _cInfo.playerName;
                                                        PersistentContainer.Instance.Players[_cInfo.playerId].JailDate = DateTime.Now;
                                                        Writer(string.Format("Client {0} at IP {1} has jailed {2} named {3}", _clientHandle[2], _ip, _cInfo.playerId, _cInfo.playerName));
                                                        _response.StatusCode = 200;
                                                    }
                                                    else
                                                    {
                                                        _response.StatusCode = 409;
                                                    }
                                                }
                                                else
                                                {
                                                    if (Jail.Jailed.Contains(_decryptSplit[1]))
                                                    {
                                                        Jail.Jailed.Remove(_decryptSplit[1]);
                                                        PersistentContainer.Instance.Players[_decryptSplit[1]].JailTime = 0;
                                                        Writer(string.Format("Client {0} at IP {1} has unjailed {2}", _clientHandle[2], _ip, _decryptSplit[1]));
                                                    }
                                                    else if (Jail.Jail_Position != "")
                                                    {
                                                        Jail.Jailed.Add(_decryptSplit[1]);
                                                        PersistentContainer.Instance.Players[_decryptSplit[1]].JailTime = -1;
                                                        PersistentContainer.Instance.Players[_decryptSplit[1]].JailName = "-Unknown-";
                                                        PersistentContainer.Instance.Players[_decryptSplit[1]].JailDate = DateTime.Now;
                                                        Writer(string.Format("Client {0} at IP {1} has jailed {2}", _clientHandle[2], _ip, _decryptSplit[1]));
                                                    }
                                                    _response.StatusCode = 406;
                                                }
                                            }
                                            else
                                            {
                                                _response.StatusCode = 500;
                                            }
                                        }
                                        else
                                        {
                                            AuthorizedTime.Remove(_clientId);
                                            Authorized.Remove(_clientId);
                                            GETPassThrough.Remove(_ip);
                                            if (!_request.IsLocal)
                                            {
                                                BannedIP.Add(_ip);
                                                Writer(string.Format("Client at IP {0} has been banned", _ip));
                                            }
                                            _response.StatusCode = 403;
                                        }
                                    }
                                    else
                                    {
                                        AuthorizedTime.Remove(_clientId);
                                        Authorized.Remove(_clientId);
                                        GETPassThrough.Remove(_ip);
                                        if (!_request.IsLocal)
                                        {
                                            BannedIP.Add(_ip);
                                            Writer(string.Format("Client at IP {0} has been banned", _ip));
                                        }
                                        _response.StatusCode = 403;
                                    }
                                }
                                else
                                {
                                    AuthorizedTime.Remove(_clientId);
                                    Authorized.Remove(_clientId);
                                    GETPassThrough.Remove(_ip);
                                    if (!_request.IsLocal)
                                    {
                                        BannedIP.Add(_ip);
                                        Writer(string.Format("Client at IP {0} has been banned", _ip));
                                    }
                                    _response.StatusCode = 403;
                                }
                            }
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "Reward")
                    {
                        string _clientId = _postMessage.Substring(0, 8);
                        if (Authorized.ContainsKey(_clientId))
                        {
                            AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                            if (Expired(_expires))
                            {
                                Authorized.Remove(_clientId);
                                AuthorizedTime.Remove(_clientId);
                                GETPassThrough.Remove(_ip);
                                _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                _response.StatusCode = 401;
                            }
                            else
                            {
                                AuthorizedTime[_clientId] = DateTime.Now;
                                Authorized.TryGetValue(_clientId, out string[] _clientHandle);
                                string _key = _clientHandle[1];
                                string _encrypted = _postMessage.Substring(8);
                                string _decrypt = Decrypt(_key, _encrypted);
                                if (_decrypt.Contains('`'))
                                {
                                    string[] _decryptSplit = _decrypt.Split('`');
                                    if (_decryptSplit[0] == _clientHandle[0])
                                    {
                                        if (!string.IsNullOrEmpty(_decryptSplit[1]))
                                        {
                                            if (VoteReward.IsEnabled)
                                            {
                                                _responseMessage += SetKey(_clientId, _clientHandle);
                                                ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_decryptSplit[1]);
                                                if (_cInfo != null)
                                                {
                                                    VoteReward.ItemOrBlockCounter(_cInfo, VoteReward.Reward_Count);
                                                    Writer(string.Format("Client {0} at IP {1} has rewarded {2} named {3}", _clientHandle[2], _ip, _cInfo.playerId, _cInfo.playerName));
                                                }
                                                else
                                                {
                                                    _response.StatusCode = 406;
                                                }
                                            }
                                            else
                                            {
                                                _response.StatusCode = 500;
                                            }
                                        }
                                        else
                                        {
                                            AuthorizedTime.Remove(_clientId);
                                            Authorized.Remove(_clientId);
                                            GETPassThrough.Remove(_ip);
                                            if (!_request.IsLocal)
                                            {
                                                BannedIP.Add(_ip);
                                                Writer(string.Format("Client at IP {0} has been banned", _ip));
                                            }
                                            _response.StatusCode = 403;
                                        }
                                    }
                                    else
                                    {
                                        AuthorizedTime.Remove(_clientId);
                                        Authorized.Remove(_clientId);
                                        GETPassThrough.Remove(_ip);
                                        if (!_request.IsLocal)
                                        {
                                            BannedIP.Add(_ip);
                                            Writer(string.Format("Client at IP {0} has been banned", _ip));
                                        }
                                        _response.StatusCode = 403;
                                    }
                                }
                                else
                                {
                                    AuthorizedTime.Remove(_clientId);
                                    Authorized.Remove(_clientId);
                                    GETPassThrough.Remove(_ip);
                                    if (!_request.IsLocal)
                                    {
                                        BannedIP.Add(_ip);
                                        Writer(string.Format("Client at IP {0} has been banned", _ip));
                                    }
                                    _response.StatusCode = 403;
                                }
                            }
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    byte[] _c = Encoding.UTF8.GetBytes(_responseMessage);
                    if (_c != null)
                    {
                        _response.SendChunked = false;
                        _response.ProtocolVersion = HttpVersion;
                        _response.KeepAlive = false;
                        _response.ContentLength64 = (long)_c.Length;
                        _response.ContentEncoding = Encoding.UTF8;
                        _response.ContentType = "text/html; charset=utf-8";
                        using (Stream output = _response.OutputStream)
                        {
                            output.Write(_c, 0, _c.Length);
                        }
                    }
                }
                else
                {
                    GETPassThrough.Remove(_ip);
                    if (!_request.IsLocal)
                    {
                        BannedIP.Add(_ip);
                        Writer(string.Format("Client at IP {0} has been banned", _ip));
                    }
                    _response.StatusCode = 403;
                }
            }
            catch (Exception e)
            {
                if (Listener.IsListening)
                {
                    Listener.Stop();
                    Listener.Close();
                }
                if (Thread.IsAlive)
                {
                    Thread.Abort();
                }
                Start();
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.POST: {0}" + e.Message));
            }
            _response.Close();
        }

        private static bool Expired(DateTime _expires)
        {
            try
            {
                TimeSpan varTime = DateTime.Now - _expires;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (_timepassed >= 30)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.Expired: {0}" + e.Message));
            }
            return false;
        }

        //private static string Encrypt(string _k, string _s)
        //{
        //    int _sL = _s.Length;
        //    for (int i = 3992; i > 0; i -= 8)
        //    {
        //        _k = _k.Insert(i, " ");
        //    }
        //    string[] _kA = _k.Split(' ');
        //    string _enc = "";
        //    for (int j = 0; j < _sL; j++)
        //    {
        //        char _ch = _s.ElementAt(j);
        //        int _cHP = int.Parse(_ch.ToString());
        //        int _kP = int.Parse(_kA[(_sL - 1) - j]);
        //        string _bK = (_cHP + _kP).ToString().PadLeft(8, '0');
        //        _enc += _bK;
        //    }
        //    return _enc;
        //}

        private static string Decrypt(string _key, string _string)
        {
            try
            {
                for (int i = _string.Length - 8; i > 0; i -= 8)
                {
                    _string = _string.Insert(i, " ");
                }
                for (int i = _key.Length - 8; i > 0; i -= 8)
                {
                    _key = _key.Insert(i, " ");
                }
                string[] _keySplit = _key.Split(' ');
                string[] _stringSplit = _string.Split(' ');
                string _decrypted = string.Empty;
                int _length = _stringSplit.Length;
                for (int j = 0; j < _length; j++)
                {
                    char _char = (char)BinaryToDecimal((int.Parse(_stringSplit[j]) - int.Parse(_keySplit[j])).ToString().PadLeft(8, '0'));
                    _decrypted += _char;
                }
                return _decrypted;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("Error in WebPanel.Decrypt: {0}" + e.Message));
            }
            return string.Empty;
        }

        private static int BinaryToDecimal(string _binary)
        {
            double _decimal = 0;
            try
            {
                int _binaryLength = _binary.Length;
                for (int i = 0; i < _binaryLength; ++i)
                {
                    _decimal += ((byte)_binary[i] - 48) * Math.Pow(2, ((_binaryLength - i) - 1));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("Error in WebPanel.BinaryToDecimal: {0}" + e.Message));
            }
            return (int)_decimal;
        }

        private static string SetKey(string _id, string[] _clientHandle)
        {
            try
            {
                string _newKey = "";
                for (int i = 0; i < 249; i++)
                {
                    _newKey += Rnd.Next(12345678, 87654321).ToString();
                }
                _clientHandle[1] = _newKey;
                Authorized[_id] = _clientHandle;
                AuthorizedTime[_id] = DateTime.Now;
                string _output = _newKey.Insert(0, _id);
                return _output;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("Error in WebPanel.SetKey: {0}" + e.Message));
            }
            return string.Empty;
        }

        private static void Writer(string _input)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: {1}", DateTime.Now, _input));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("Error in WebPanel.Writer: {0}" + e.Message));
            }
        }
    }
}
