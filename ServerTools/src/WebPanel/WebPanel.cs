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

        //5xx: Server Error - The server failed to fulfill an apparently valid request[1]:


        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static bool DirFound = false;
        public static string SITE_DIR = "";
        public static string Password = "";
        private static string Port = "";
        private static string _File = string.Format("WebPanelLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string FilePath = string.Format("{0}/Logs/WebPanelLogs/{1}", API.ConfigPath, _File);
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
        public static List<string> Clients = new List<string>();
        public static List<string> BannedIP = new List<string>();
        public static List<string> FollowUp = new List<string>();

        public static void Load()
        {
            IsRunning = true;
            BuildLists();
            Start();
        }

        public static void Unload()
        {
            IsRunning = false;
            Thread.Abort();
        }

        private static void Start()
        {
            Thread = new Thread(new ThreadStart(Exec));
            Thread.IsBackground = true;
            Thread.Start();
        }

        public static void CheckDir()
        {
            try
            {
                if (Directory.Exists(API.GamePath + "/Mods/"))
                {
                    string[] _directories = Directory.GetDirectories(API.GamePath + "/Mods/");
                    if (_directories != null && _directories.Length > 0)
                    {
                        for (int i = 0; i < _directories.Length; i++)
                        {
                            string _directory = _directories[i];
                            if (_directory.Contains("ServerTools"))
                            {
                                if (Directory.Exists(_directory + "/WebPanel/"))
                                {
                                    SITE_DIR = _directory + "/WebPanel/";
                                    DirFound = true;
                                }
                            }
                        }
                    }
                }
                if (!DirFound)
                {
                    Unload();
                    Log.Out("[SERVERTOOLS] Unable to verify ServerTools directory. Web panel failed to start");
                }
            }
            catch (Exception e)
            {
                Unload();
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.CheckDir: {0}" + e.Message));
            }
        }

        private static void BuildLists()
        {
            try
            {
                List<string> _clientList = PersistentContainer.Instance.WebPanelClientList;
                if (_clientList != null && _clientList.Count > 0)
                {
                    Clients = _clientList;
                }
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
            string _characters = "jJk9Kl3wWxXAbyYz0ZLmMn5NoO6dDe1EfFpPqQrRsStaBc2CgGhH7iITu4U8vV";
            string _newPassword = "";
            System.Random _rnd = new System.Random();
            for (int i = 0; i < 8; i++)
            {
                _newPassword += _characters.ElementAt(_rnd.Next(0, 62));
            }
            return _newPassword;
        }

        public static string SetUID()
        {
            string _characters = "4829017536";
            string _newUID = "";
            System.Random _rnd = new System.Random();
            for (int i = 0; i < 8; i++)
            {
                _newUID += _characters.ElementAt(_rnd.Next(0, 10));
            }
            return _newUID;
        }

        public static void Exec()
        {
            try
            {
                int _hostPort = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("ControlPanelPort", false));
                int _host4 = _hostPort + 4;
                if (_host4 > 0 && _host4 < 65536)
                {
                    if (HttpListener.IsSupported)
                    {
                        string _externalIp = new WebClient().DownloadString("https://ipinfo.io/ip/");
                        if (string.IsNullOrEmpty(_externalIp))
                        {
                            _externalIp = new WebClient().DownloadString("https://api.ipify.org/");
                            if (string.IsNullOrEmpty(_externalIp))
                            {
                                Log.Out("[SERVERTOOLS] This host can not support ServerTools web panel. It has been disabled");
                                return;
                            }
                        }
                        Port = _host4.ToString();
                        Listener.Prefixes.Add(string.Format("http://*:{0}/", Port));
                        Listener.Prefixes.Add(string.Format("http://*:{0}/st.html/", Port));
                        Listener.Start();
                        string _localPage = "http://localhost:" + _host4.ToString() + "/st.html";
                        Log.Out(string.Format("[SERVERTOOLS] ServerTools web panel has opened on port {0}", _host4.ToString()));
                        Log.Out(string.Format("[SERVERTOOLS] Use {0} or if accessing it remotely, replace localhost with your server IP of {1}", _localPage, _externalIp));
                        while (Listener.IsListening)
                        {
                            HttpListenerContext _context = Listener.GetContext();
                            if (_context != null)
                            {
                                HttpListenerRequest _request = _context.Request;
                                HttpListenerResponse _response = _context.Response;
                                string _clientIp = _request.RemoteEndPoint.Address.ToString();
                                Writer(string.Format("Web Panel request from IP: {0}", _clientIp));
                                if (_request.Url.AbsoluteUri.Length < 101 && GameManager.Instance.World != null)
                                {
                                    if (_request.Url.AbsoluteUri.Contains("<script>"))
                                    {
                                        if (!BannedIP.Contains(_clientIp))
                                        {
                                            if (!_request.IsLocal)
                                            {
                                                BannedIP.Add(_clientIp);
                                                Writer(string.Format("Banned IP: {0} for attempting to run a script at address {1}", _clientIp, _request.Url.AbsoluteUri));
                                            }
                                            else
                                            {
                                                Writer(string.Format("Detected local IP: {0}. Attempting to run a script at address {1}", _clientIp, _request.Url.AbsoluteUri));
                                            }
                                        }
                                        else
                                        {
                                            Writer(string.Format("Detected banned IP: {0}. Attempting to run a script at address {1}", _clientIp, _request.Url.AbsoluteUri));
                                        }
                                        return;
                                    }
                                    if (!BannedIP.Contains(_clientIp))
                                    {
                                        if (!TimeOut.ContainsKey(_clientIp))
                                        {
                                            IsAllowed(_request, _response, _clientIp);
                                        }
                                        else
                                        {
                                            TimeOut.TryGetValue(_clientIp, out DateTime _timeout);
                                            TimeSpan varTime = DateTime.Now - _timeout;
                                            double fractionalMinutes = varTime.TotalMinutes;
                                            int _timepassed = (int)fractionalMinutes;
                                            if (_timepassed >= 10)
                                            {
                                                TimeOut.Remove(_clientIp);
                                                PersistentContainer.Instance.WebPanelTimeoutList = TimeOut;
                                                IsAllowed(_request, _response, _clientIp);
                                            }
                                            else
                                            {
                                                Writer(string.Format("Request denied for IP: {0} on timeout until: {1}", _clientIp, _timeout));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Writer(string.Format("Request denied for banned IP: {0}", _clientIp));
                                    }
                                }
                                _response.Close();
                                _context.Response.Close();
                            }
                        }
                        Listener.Stop();
                        Listener.Close();
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] This host can not support ServerTools web panel. It has been disabled"));
                    }
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Control panel port is set too high. It must be between 1-65531 for ServerTools web panel to function. Web panel has been disabled"));
                }
            }
            catch (Exception e)
            {
                Writer(string.Format("Error in WebPanel.Exec: {0}", e.Message));
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.Exec: {0}" + e.Message));
                Listener.Stop();
                Listener.Close();
                Unload();
            }
        }

        private static void IsAllowed(HttpListenerRequest _request, HttpListenerResponse _response, string _clientIp)
        {
            try
            {
                string _uri = _request.Url.AbsoluteUri;
                //if (_uri.Contains("/api/"))
                //{
                //    Log.Out(string.Format("[SERVERTOOLS] API requested "));
                //    string[] command = Regex.Split(_uri, "/api/");
                //    //Response(_uri, _response, true, command[1]);
                //}
                _uri = _uri.Remove(0, _uri.IndexOf(Port) + Port.Length + 1);
                if (_request.HttpMethod == "GET")
                {
                    if (_uri == "st.html")
                    {
                        if (PageHits.ContainsKey(_clientIp))
                        {
                            PageHits.TryGetValue(_clientIp, out int _count);
                            if (_count++ >= 5)
                            {
                                PageHits.Remove(_clientIp);
                                TimeOut.Add(_clientIp, DateTime.Now);
                                PersistentContainer.Instance.WebPanelTimeoutList = TimeOut;
                                Writer(string.Format("Web panel request denied: {0} for IP {1}. Client in time out", SITE_DIR + _uri, _clientIp));
                            }
                            else
                            {
                                PageHits[_clientIp] = _count + 1;
                                Writer(string.Format("Web panel request granted: {0} for IP {1}", SITE_DIR + _uri, _clientIp));
                                GET(_request, _response, _uri);
                            }
                        }
                        else
                        {
                            PageHits.Add(_clientIp, 1);
                            Writer(string.Format("Web panel request granted: {0} for IP {1}", SITE_DIR + _uri, _clientIp));
                            GET(_request, _response, _uri);
                        }
                    }
                    else
                    {
                        Writer(string.Format("Web panel request granted: {0} for IP {1}", SITE_DIR + _uri, _clientIp));
                        GET(_request, _response, _uri);
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
                }
                else
                {
                    Writer(string.Format("Unaccepted method requested by IP {0}", _clientIp));
                }
            }
            catch (Exception e)
            {
                Writer(string.Format("Error in WebPanel.IsAllowed: {0}", e.Message));
            }
            _response.Close();
        }

        private static void GET(HttpListenerRequest _request, HttpListenerResponse _response, string _uri)
        {
            try
            {
                FileInfo _fileInfo = new FileInfo(SITE_DIR + _uri);
                if (_fileInfo != null && _fileInfo.Exists)
                {
                    byte[] _c = File.ReadAllBytes(SITE_DIR + _uri);
                    if (_c != null)
                    {
                        _response.StatusCode = 200;
                        _response.SendChunked = false;
                        _response.ProtocolVersion = HttpVersion;
                        _response.KeepAlive = false;
                        _response.ContentType = MimeType.GetMimeType(Path.GetExtension(_fileInfo.Extension));
                        _response.ContentLength64 = (long)_c.Length;
                        _response.ContentEncoding = Encoding.UTF8;
                        _response.OutputStream.Write(_c, 0, _c.Length);
                    }
                    else
                    {
                        _response.StatusCode = 404;
                    }
                }
                else
                {
                    _response.StatusCode = 404;
                    Writer(string.Format("Requested web panel file not found at {0}", SITE_DIR + _uri));
                }
            }
            catch (Exception e)
            {
                Writer(string.Format("Error in WebPanel.GET: {0}", e.Message));
            }
            _response.Close();
        }

        private static void POST(HttpListenerRequest _request, HttpListenerResponse _response, string _uri, string _postMessage)
        {
            try
            {
                if (_postMessage.Length == 4000)
                {
                    string _responseMessage = "";
                    if (_uri == "Handshake")
                    {
                        string _pMR = Reverse(_postMessage);
                        string _clId = _pMR.Substring(0, 8);
                        if (!Authorized.ContainsKey(_clId) && !Visitor.ContainsKey(_clId))
                        {
                            string _nK = "";
                            for (int i = 0; i < 499; i++)
                            {
                                _nK += Rnd.Next(12345678, 87654321).ToString();
                            }
                            string[] _clientHandle = { _nK, "" };
                            Visitor.Add(_clId, _clientHandle);
                            string _returnHandshake = _nK.Insert(0, _clId);
                            for (int i = _returnHandshake.Length - 1; i >= 0; i--)
                            {
                                _responseMessage += _returnHandshake.ElementAt(i);
                            }
                            _response.StatusCode = 200;
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "Secure")
                    {
                        string _pMR = Reverse(_postMessage);
                        string _id = _pMR.Substring(0, 8);
                        if (ValidVisit(_id))
                        {
                            Visitor.TryGetValue(_id, out string[] _clientHandle);
                            string _k = _clientHandle[0];
                            string _s = _pMR.Substring(8);
                            string _decrypt = Decrypt(_k, _s);
                            string _pEnc = "";
                            for (int i = 0; i < 499; i++)
                            {
                                _pEnc += Rnd.Next(12345678, 87654321).ToString();
                            }
                            _clientHandle[0] = _pEnc;
                            _clientHandle[1] = _decrypt;
                            Visitor[_id] = _clientHandle;
                            string _returnSecure = _pEnc.Insert(0, _id);
                            for (int i = _returnSecure.Length - 1; i >= 0; i--)
                            {
                                _responseMessage += _returnSecure.ElementAt(i);
                            }
                            _response.StatusCode = 200;
                            Writer(string.Format("Secure handshake established with IP {0}", _request.RemoteEndPoint.Address.ToString()));
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }

                    }
                    else if (_uri == "SignIn")
                    {
                        string _pMR = Reverse(_postMessage);
                        string _id = _pMR.Substring(0, 8);
                        if (ValidVisit(_id))
                        {
                            Visitor.TryGetValue(_id, out string[] _clientHandle);
                            string _k = _clientHandle[0];
                            string _s = _pMR.Substring(8);
                            string _decrypt = Decrypt(_k, _s);
                            if (_decrypt.Contains(" "))
                            {
                                string[] _decryptSplit = _decrypt.Split(' ');
                                if (_decryptSplit[0] == _clientHandle[1])
                                {
                                    if (!string.IsNullOrEmpty(_decryptSplit[1]) && Clients.Contains(_decryptSplit[1]))
                                    {
                                        string _ip = _request.RemoteEndPoint.Address.ToString();
                                        if (!string.IsNullOrEmpty(_decryptSplit[2]) && !string.IsNullOrEmpty(PersistentContainer.Instance.Players[_decryptSplit[1]].WP) && _decryptSplit[2] == PersistentContainer.Instance.Players[_decryptSplit[1]].WP)
                                        {
                                            if (LoginAttempts.ContainsKey(_ip))
                                            {
                                                LoginAttempts.Remove(_ip);
                                            }
                                            string _pEnc = "";
                                            for (int i = 0; i < 499; i++)
                                            {
                                                _pEnc += Rnd.Next(12345678, 87654321).ToString();
                                            }
                                            _clientHandle[0] = _pEnc;
                                            Visitor.Remove(_id);
                                            Authorized.Add(_id, _clientHandle);
                                            AuthorizedTime.Add(_id, DateTime.Now);
                                            PageHits.Remove(_ip);
                                            string _returnSecure = _pEnc.Insert(0, _id);
                                            for (int i = _returnSecure.Length - 1; i >= 0; i--)
                                            {
                                                _responseMessage += _returnSecure.ElementAt(i);
                                            }
                                            _response.StatusCode = 200;
                                            Writer(string.Format("Web panel client {0} has signed in from IP {1}", _decryptSplit[1], _request.RemoteEndPoint.Address.ToString()));
                                        }
                                        else
                                        {
                                            if (LoginAttempts.ContainsKey(_ip))
                                            {
                                                LoginAttempts.TryGetValue(_ip, out int _count);
                                                if (_count + 1 == 6)
                                                {
                                                    LoginAttempts.Remove(_ip);
                                                    TimeOut.Add(_ip, DateTime.Now);
                                                    PersistentContainer.Instance.WebPanelTimeoutList = TimeOut;
                                                    return;
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
                                            string _pEnc = "";
                                            for (int j = 0; j < 499; j++)
                                            {
                                                _pEnc += Rnd.Next(12345678, 87654321).ToString();
                                            }
                                            _clientHandle[0] = _pEnc;
                                            Visitor[_id] = _clientHandle;
                                            string _returnSecure = _pEnc.Insert(0, _id);
                                            for (int k = _returnSecure.Length - 1; k >= 0; k--)
                                            {
                                                _responseMessage += _returnSecure.ElementAt(k);
                                            }
                                            _response.StatusCode = 401;
                                            Writer(string.Format("Web panel sign in failure by IP {0}", _request.RemoteEndPoint.Address.ToString()));
                                        }
                                    }
                                    else
                                    {
                                        string _pEnc = "";
                                        for (int i = 0; i < 499; i++)
                                        {
                                            _pEnc += Rnd.Next(12345678, 87654321).ToString();
                                        }
                                        _clientHandle[0] = _pEnc;
                                        Visitor[_id] = _clientHandle;
                                        string _returnSecure = _pEnc.Insert(0, _id);
                                        for (int i = _returnSecure.Length - 1; i >= 0; i--)
                                        {
                                            _responseMessage += _returnSecure.ElementAt(i);
                                        }
                                        _response.StatusCode = 401;
                                    }
                                }
                                else
                                {
                                    _response.StatusCode = 401;
                                }
                            }
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "NewPass")
                    {
                        string _pMR = Reverse(_postMessage);
                        string _id = _pMR.Substring(0, 8);
                        if (ValidAuth(_id))
                        {
                            Authorized.TryGetValue(_id, out string[] _clientHandle);
                            string _k = _clientHandle[0];
                            string _s = _pMR.Substring(8);
                            string _decrypt = Decrypt(_k, _s);
                            if (_decrypt.Contains(' '))
                            {
                                string[] _decryptSplit = _decrypt.Split(' ');
                                if (_decryptSplit[0] == _clientHandle[1])
                                {
                                    if (!string.IsNullOrEmpty(_decryptSplit[1]) && Clients.Contains(_decryptSplit[1]))
                                    {
                                        _responseMessage += SetKey(_id, _clientHandle);
                                        _response.StatusCode = 200;
                                    }
                                    else
                                    {
                                        _response.StatusCode = 401;
                                    }
                                }
                                else
                                {
                                    _response.StatusCode = 401;
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
                        string _pMR = Reverse(_postMessage);
                        string _id = _pMR.Substring(0, 8);
                        if (ValidAuth(_id))
                        {
                            Authorized.TryGetValue(_id, out string[] _clientHandle);
                            string _k = _clientHandle[0];
                            string _s = _pMR.Substring(8);
                            string _decrypt = Decrypt(_k, _s);
                            if (_decrypt.Contains(' '))
                            {
                                string[] _decryptSplit = _decrypt.Split(' ');
                                if (_decryptSplit[0] == _clientHandle[1])
                                {
                                    if (!string.IsNullOrEmpty(_decryptSplit[1]) && Clients.Contains(_decryptSplit[1]))
                                    {
                                        PersistentContainer.Instance.Players[_decryptSplit[1]].WP = _decryptSplit[2];
                                        _responseMessage += SetKey(_id, _clientHandle);
                                        _response.StatusCode = 200;
                                        Writer(string.Format("Web panel client {0} has set a new pass from IP {1}", _decryptSplit[1], _request.RemoteEndPoint.Address.ToString()));
                                    }
                                }
                            }
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    else if (_uri == "SignOut")
                    {
                        string _pMR = Reverse(_postMessage);
                        string _id = _pMR.Substring(0, 8);
                        if (ValidAuth(_id))
                        {
                            Authorized.TryGetValue(_id, out string[] _clientHandle);
                            string _k = _clientHandle[0];
                            string _s = _pMR.Substring(8);
                            string _decrypt = Decrypt(_k, _s);
                            if (_decrypt.Contains(' '))
                            {
                                string[] _decryptSplit = _decrypt.Split(' ');
                                if (_decryptSplit[0] == _clientHandle[1])
                                {
                                    AuthorizedTime.Remove(_id);
                                    Authorized.Remove(_id);
                                    _response.StatusCode = 200;
                                    Writer(string.Format("Web panel client has signed out from IP {0}", _request.RemoteEndPoint.Address.ToString()));
                                }
                                else
                                {
                                    _response.StatusCode = 401;
                                }
                            }
                        }
                        else
                        {
                            _response.StatusCode = 401;
                        }
                    }
                    //else if (_uri == "Command")
                    //{
                    //    Log.Out(string.Format("Command Detected"));
                    //    string _pMR = Reverse(_postMessage);
                    //    string _id = _pMR.Substring(0, 8);
                    //    if (ValidAuth(_id))
                    //    {
                    //        Authorized.TryGetValue(_id, out string[] _clientHandle);
                    //        string _k = _clientHandle[0];
                    //        string _s = _pMR.Substring(8);
                    //        string _decrypt = Decrypt(_k, _s);
                    //        if (_decrypt.Contains(' '))
                    //        {
                    //            string[] _decryptSplit = _decrypt.Split(' ');
                    //            if (_decryptSplit[0] == _clientHandle[1])
                    //            {
                    //                if (!string.IsNullOrEmpty(_decryptSplit[1]) && Clients.Contains(_decryptSplit[1]))
                    //                {
                    //                    if (!string.IsNullOrEmpty(_decryptSplit[2]))
                    //                    {
                    //                        Log.Out(string.Format("[SERVERTOOLS] Attempting to run command"));
                    //
                    //                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Web panel console command: {0}. Run by client {1} with IP {2}", _decryptSplit[2], _decryptSplit[1], _request.RemoteEndPoint.Address.ToString()));
                    //                        _responseMessage += SetKey(_id, _clientHandle);
                    //                        _response.StatusCode = 200;
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    _response.StatusCode = 401;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                _response.StatusCode = 401;
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        _response.StatusCode = 401;
                    //    }
                    //}
                    else if (_uri == "Players")
                    {
                        string _pMR = Reverse(_postMessage);
                        string _id = _pMR.Substring(0, 8);
                        if (!FollowUp.Contains(_id))
                        {
                            if (Authorized.ContainsKey(_id))
                            {
                                AuthorizedTime.TryGetValue(_id, out DateTime _expires);
                                if (Expired(_expires))
                                {
                                    Authorized.Remove(_id);
                                    AuthorizedTime.Remove(_id);
                                    _response.StatusCode = 401;
                                }
                                else
                                {
                                    Authorized.TryGetValue(_id, out string[] _clientHandle);
                                    string _k = _clientHandle[0];
                                    string _s = _pMR.Substring(8);
                                    string _decrypt = Decrypt(_k, _s);
                                    if (_decrypt.Contains(' '))
                                    {
                                        string[] _decryptSplit = _decrypt.Split(' ');
                                        if (_decryptSplit[0] == _clientHandle[1])
                                        {
                                            if (!string.IsNullOrEmpty(_decryptSplit[1]) && Clients.Contains(_decryptSplit[1]))
                                            {
                                                string _uid = SetUID();
                                                FollowUp.Add(_uid);
                                                string _pEnc = "";
                                                for (int j = 0; j < 499; j++)
                                                {
                                                    _pEnc += Rnd.Next(12345678, 87654321).ToString();
                                                }
                                                _clientHandle[0] = _pEnc;
                                                Authorized[_id] = _clientHandle;
                                                AuthorizedTime[_id] = DateTime.Now;
                                                string _returnSecure = _pEnc.Insert(0, _uid);
                                                for (int k = _returnSecure.Length - 1; k >= 0; k--)
                                                {
                                                    _responseMessage += _returnSecure.ElementAt(k);
                                                }
                                                _response.StatusCode = 200;
                                                Writer(string.Format("Updated players list for client {0} at IP {1}", _decryptSplit[1], _request.RemoteEndPoint.Address.ToString()));
                                            }
                                            else
                                            {
                                                _response.StatusCode = 401;
                                            }
                                        }
                                        else
                                        {
                                            _response.StatusCode = 401;
                                        }
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
                            FollowUp.Remove(_id);
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
                        string _pMR = Reverse(_postMessage);
                        string _id = _pMR.Substring(0, 8);
                        if (!FollowUp.Contains(_id))
                        {
                            if (ValidAuth(_id))
                            {
                                Authorized.TryGetValue(_id, out string[] _clientHandle);
                                string _k = _clientHandle[0];
                                string _s = _pMR.Substring(8);
                                string _decrypt = Decrypt(_k, _s);
                                if (_decrypt.Contains(' '))
                                {
                                    string[] _decryptSplit = _decrypt.Split(' ');
                                    if (_decryptSplit[0] == _clientHandle[1])
                                    {
                                        if (!string.IsNullOrEmpty(_decryptSplit[1]) && Clients.Contains(_decryptSplit[1]))
                                        {
                                            string _uid = SetUID();
                                            FollowUp.Add(_uid);
                                            string _pEnc = "";
                                            for (int i = 0; i < 499; i++)
                                            {
                                                _pEnc += Rnd.Next(12345678, 87654321).ToString();
                                            }
                                            _clientHandle[0] = _pEnc;
                                            Authorized[_id] = _clientHandle;
                                            AuthorizedTime[_id] = DateTime.Now;
                                            string _returnSecure = _pEnc.Insert(0, _uid);
                                            for (int j = _returnSecure.Length - 1; j >= 0; j--)
                                            {
                                                _responseMessage += _returnSecure.ElementAt(j);
                                            }
                                            _response.StatusCode = 200;
                                            Writer(string.Format("Loaded config for client {0} at IP {1}", _decryptSplit[1], _request.RemoteEndPoint.Address.ToString()));
                                        }
                                        else
                                        {
                                            _response.StatusCode = 401;
                                        }
                                    }
                                    else
                                    {
                                        _response.StatusCode = 401;
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
                            FollowUp.Remove(_id);
                            if (Utils.FileExists(Config.configFilePath))
                            {
                                XmlDocument xmlDoc = new XmlDocument();
                                try
                                {
                                    xmlDoc.Load(Config.configFilePath);
                                }
                                catch (XmlException e)
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", Config.configFilePath, e.Message));
                                    return;
                                }
                                XmlNode _XmlNode = xmlDoc.DocumentElement;
                                XmlNodeList _XmlNodeList = _XmlNode.ChildNodes;
                                for (int i = 0; i < _XmlNodeList.Count; i++)
                                {
                                    XmlNode _childNode = _XmlNodeList[i];
                                    if (_childNode.Name == "Tools")
                                    {
                                        for (int j = 0; j < _childNode.ChildNodes.Count; j++)
                                        {
                                            XmlNode _SubChild = _childNode.ChildNodes[j];
                                            if (_SubChild.Name == "Tool")
                                            {
                                                XmlElement _element = (XmlElement)_SubChild;
                                                XmlAttributeCollection _attributes = _element.Attributes;
                                                for (int k = 1; k < _attributes.Count; k++)
                                                {
                                                    _responseMessage += _attributes[k].Value + "§";
                                                }
                                            }
                                        }
                                    }
                                }
                                _responseMessage = _responseMessage.TrimEnd('§');
                                _response.StatusCode = 200;
                            }
                        }
                    }
                    else if (_uri == "UpdateConfig")
                    {
                        string _pMR = Reverse(_postMessage);
                        string _id = _pMR.Substring(0, 8);
                        if (!FollowUp.Contains(_id))
                        {
                            if (ValidAuth(_id))
                            {
                                Authorized.TryGetValue(_id, out string[] _clientHandle);
                                string _k = _clientHandle[0];
                                string _s = _pMR.Substring(8);
                                string _decrypt = Decrypt(_k, _s);
                                if (_decrypt.Contains(' '))
                                {
                                    string[] _decryptSplit = _decrypt.Split(' ');
                                    if (_decryptSplit[0] == _clientHandle[1])
                                    {
                                        if (!string.IsNullOrEmpty(_decryptSplit[1]) && Clients.Contains(_decryptSplit[1]))
                                        {
                                            string _uid = SetUID();
                                            FollowUp.Add(_uid);
                                            string _pEnc = "";
                                            for (int i = 0; i < 499; i++)
                                            {
                                                _pEnc += Rnd.Next(12345678, 87654321).ToString();
                                            }
                                            _clientHandle[0] = _pEnc;
                                            Authorized[_id] = _clientHandle;
                                            AuthorizedTime[_id] = DateTime.Now;
                                            string _returnSecure = _pEnc.Insert(0, _uid);
                                            for (int j = _returnSecure.Length - 1; j >= 0; j--)
                                            {
                                                _responseMessage += _returnSecure.ElementAt(j);
                                            }
                                            _response.StatusCode = 200;
                                            Writer(string.Format("Client {0} at IP {1} has updated the config", _decryptSplit[1], _request.RemoteEndPoint.Address.ToString()));
                                        }
                                        else
                                        {
                                            _response.StatusCode = 401;
                                        }
                                    }
                                    else
                                    {
                                        _response.StatusCode = 401;
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
                            FollowUp.Remove(_id);
                            string _newConfig = _pMR.Substring(8);
                            if (_newConfig.Contains("☼") && _newConfig.Contains("§") && Utils.FileExists(Config.configFilePath))
                            {
                                int _count = 0;
                                string[] _dropExtra = _newConfig.Split('☼');
                                string[] _configValues = _dropExtra[0].Split('§');
                                XmlDocument _xmlDoc = new XmlDocument();
                                try
                                {
                                    _xmlDoc.Load(Config.configFilePath);
                                }
                                catch (XmlException e)
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", Config.configFilePath, e.Message));
                                    return;
                                }
                                XmlNode _XmlNode = _xmlDoc.DocumentElement;
                                XmlNodeList _XmlNodeList = _XmlNode.ChildNodes;
                                for (int i = 0; i < _XmlNodeList.Count; i++)
                                {
                                    XmlNode _childNode = _XmlNodeList[i];
                                    if (_childNode.Name == "Tools")
                                    {
                                        for (int j = 0; j < _childNode.ChildNodes.Count; j++)
                                        {
                                            XmlNode _SubChild = _childNode.ChildNodes[j];
                                            if (_SubChild.Name == "Tool")
                                            {
                                                XmlElement _element = (XmlElement)_SubChild;
                                                XmlAttributeCollection _attributes = _element.Attributes;
                                                for (int k = 1; k < _attributes.Count; k++)
                                                {
                                                    _attributes[k].Value = _configValues[_count];
                                                    _count++;
                                                }
                                            }
                                        }
                                    }
                                }
                                _xmlDoc.Save(Config.configFilePath);
                            }
                        }
                    }
                    else if (_uri == "Kick")
                    {
                        string _pMR = Reverse(_postMessage);
                        string _id = _pMR.Substring(0, 8);
                        if (ValidAuth(_id))
                        {
                            Authorized.TryGetValue(_id, out string[] _clientHandle);
                            string _k = _clientHandle[0];
                            string _s = _pMR.Substring(8);
                            string _decrypt = Decrypt(_k, _s);
                            if (_decrypt.Contains(' '))
                            {
                                string[] _decryptSplit = _decrypt.Split(' ');
                                if (_decryptSplit[0] == _clientHandle[1])
                                {
                                    if (!string.IsNullOrEmpty(_decryptSplit[1]) && Clients.Contains(_decryptSplit[1]))
                                    {
                                        if (!string.IsNullOrEmpty(_decryptSplit[2]))
                                        {
                                            _responseMessage += SetKey(_id, _clientHandle);
                                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_decryptSplit[2]);
                                            if (_cInfo != null)
                                            {
                                                _response.StatusCode = 200;
                                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0}", _decryptSplit[2]), null);
                                                Writer(string.Format("Web panel client {0} has kicked steam id {1} entity {2} with IP {2}", _decryptSplit[1], _cInfo.playerId, _cInfo.entityId, _request.RemoteEndPoint.Address.ToString()));
                                            }
                                            else
                                            {
                                                _response.StatusCode = 202;
                                            }
                                        }
                                        else
                                        {
                                            _response.StatusCode = 401;
                                        }
                                    }
                                    else
                                    {
                                        _response.StatusCode = 401;
                                    }
                                }
                                else
                                {
                                    _response.StatusCode = 401;
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
                        string _pMR = Reverse(_postMessage);
                        string _id = _pMR.Substring(0, 8);
                        if (ValidAuth(_id))
                        {
                            Authorized.TryGetValue(_id, out string[] _clientHandle);
                            string _k = _clientHandle[0];
                            string _s = _pMR.Substring(8);
                            string _decrypt = Decrypt(_k, _s);
                            if (_decrypt.Contains(' '))
                            {
                                string[] _decryptSplit = _decrypt.Split(' ');
                                if (_decryptSplit[0] == _clientHandle[1])
                                {
                                    if (!string.IsNullOrEmpty(_decryptSplit[1]) && Clients.Contains(_decryptSplit[1]))
                                    {
                                        if (!string.IsNullOrEmpty(_decryptSplit[2]))
                                        {
                                            _responseMessage += SetKey(_id, _clientHandle);
                                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_decryptSplit[2]);
                                            if (_cInfo != null)
                                            {
                                                _response.StatusCode = 200;
                                                SdtdConsole.Instance.ExecuteSync(string.Format("ban {0} 1 year", _decryptSplit[2]), null);
                                                Writer(string.Format("Web panel client {0} has banned {1} from IP {2}", _decryptSplit[1], _decryptSplit[2], _request.RemoteEndPoint.Address.ToString()));
                                            }
                                            else
                                            {
                                                _response.StatusCode = 202;
                                            }
                                        }
                                        else
                                        {
                                            _response.StatusCode = 401;
                                        }
                                    }
                                    else
                                    {
                                        _response.StatusCode = 401;
                                    }
                                }
                                else
                                {
                                    _response.StatusCode = 401;
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
                        string _pMR = Reverse(_postMessage);
                        string _id = _pMR.Substring(0, 8);
                        if (ValidAuth(_id))
                        {
                            Authorized.TryGetValue(_id, out string[] _clientHandle);
                            string _k = _clientHandle[0];
                            string _s = _pMR.Substring(8);
                            string _decrypt = Decrypt(_k, _s);
                            if (_decrypt.Contains(' '))
                            {
                                string[] _decryptSplit = _decrypt.Split(' ');
                                if (_decryptSplit[0] == _clientHandle[1])
                                {
                                    if (!string.IsNullOrEmpty(_decryptSplit[1]) && Clients.Contains(_decryptSplit[1]))
                                    {
                                        if (!string.IsNullOrEmpty(_decryptSplit[2]))
                                        {
                                            _responseMessage += SetKey(_id, _clientHandle);
                                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_decryptSplit[2]);
                                            if (_cInfo != null)
                                            {
                                                _response.StatusCode = 200;
                                                if (Mute.Mutes.Contains(_cInfo.playerId))
                                                {
                                                    Mute.Mutes.Remove(_cInfo.playerId);
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteTime = 0;
                                                    Writer(string.Format("Web panel client {0} has unmuted {1} from IP {2}", _decryptSplit[1], _decryptSplit[2], _request.RemoteEndPoint.Address.ToString()));
                                                }
                                                else
                                                {
                                                    Mute.Mutes.Add(_cInfo.playerId);
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteTime = -1;
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteName = _cInfo.playerName;
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteDate = DateTime.Now;
                                                    Writer(string.Format("Web panel client {0} has muted {1} from IP {2}", _decryptSplit[1], _decryptSplit[2], _request.RemoteEndPoint.Address.ToString()));
                                                }

                                            }
                                            else
                                            {
                                                _response.StatusCode = 202;
                                            }
                                        }
                                        else
                                        {
                                            _response.StatusCode = 401;
                                        }
                                    }
                                    else
                                    {
                                        _response.StatusCode = 401;
                                    }
                                }
                                else
                                {
                                    _response.StatusCode = 401;
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
                        string _pMR = Reverse(_postMessage);
                        string _id = _pMR.Substring(0, 8);
                        if (ValidAuth(_id))
                        {
                            Authorized.TryGetValue(_id, out string[] _clientHandle);
                            string _k = _clientHandle[0];
                            string _s = _pMR.Substring(8);
                            string _decrypt = Decrypt(_k, _s);
                            if (_decrypt.Contains(' '))
                            {
                                string[] _decryptSplit = _decrypt.Split(' ');
                                if (_decryptSplit[0] == _clientHandle[1])
                                {
                                    if (!string.IsNullOrEmpty(_decryptSplit[1]) && Clients.Contains(_decryptSplit[1]))
                                    {
                                        if (!string.IsNullOrEmpty(_decryptSplit[2]))
                                        {
                                            _responseMessage += SetKey(_id, _clientHandle);
                                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_decryptSplit[2]);
                                            if (_cInfo != null)
                                            {
                                                _response.StatusCode = 200;
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
                                                        Writer(string.Format("Web panel client {0} has unjailed {1} from IP {2}", _decryptSplit[1], _decryptSplit[2], _request.RemoteEndPoint.Address.ToString()));
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
                                                    Writer(string.Format("Web panel client {0} has jailed {1} from IP {2}", _decryptSplit[1], _decryptSplit[2], _request.RemoteEndPoint.Address.ToString()));
                                                }
                                            }
                                            else
                                            {
                                                _response.StatusCode = 202;
                                            }
                                        }
                                        else
                                        {
                                            _response.StatusCode = 401;
                                        }
                                    }
                                    else
                                    {
                                        _response.StatusCode = 401;
                                    }
                                }
                                else
                                {
                                    _response.StatusCode = 401;
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
                        string _pMR = Reverse(_postMessage);
                        string _id = _pMR.Substring(0, 8);
                        if (ValidAuth(_id))
                        {
                            Authorized.TryGetValue(_id, out string[] _clientHandle);
                            string _k = _clientHandle[0];
                            string _s = _pMR.Substring(8);
                            string _decrypt = Decrypt(_k, _s);
                            if (_decrypt.Contains(' '))
                            {
                                string[] _decryptSplit = _decrypt.Split(' ');
                                if (_decryptSplit[0] == _clientHandle[1])
                                {
                                    if (!string.IsNullOrEmpty(_decryptSplit[1]) && Clients.Contains(_decryptSplit[1]))
                                    {
                                        if (!string.IsNullOrEmpty(_decryptSplit[2]))
                                        {
                                            _responseMessage += SetKey(_id, _clientHandle);
                                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_decryptSplit[2]);
                                            if (_cInfo != null)
                                            {
                                                _response.StatusCode = 200;
                                                VoteReward.ItemOrBlockCounter(_cInfo, VoteReward.Reward_Count);
                                                Writer(string.Format("Web panel client {0} has rewarded {1} from IP {2}", _decryptSplit[1], _decryptSplit[2], _request.RemoteEndPoint.Address.ToString()));
                                            }
                                            else
                                            {
                                                _response.StatusCode = 202;
                                            }
                                        }
                                        else
                                        {
                                            _response.StatusCode = 401;
                                        }
                                    }
                                    else
                                    {
                                        _response.StatusCode = 401;
                                    }
                                }
                                else
                                {
                                    _response.StatusCode = 401;
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
                        _response.ContentType = "text/plain; charset=utf-8";
                        using (Stream output = _response.OutputStream)
                        {
                            output.Write(_c, 0, _c.Length);
                        }
                    }
                }
                else
                {
                    BannedIP.Add(_request.RemoteEndPoint.Address.ToString());
                    Writer("Client with IP {0} has been banned from the web panel. Detected improper message format sent to server");
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("Error in WebsiteServer.POST: {0}" + e.Message));
            }
            _response.Close();
        }

        private static bool Expired(DateTime _expires)
        {
            TimeSpan varTime = DateTime.Now - _expires;
            double fractionalMinutes = varTime.TotalMinutes;
            int _timepassed = (int)fractionalMinutes;
            if (_timepassed >= 30)
            {
                return true;
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

        private static string Decrypt(string _k, string _s)
        {
            for (int j = 3984; j > 0; j -= 8)
            {
                _k = _k.Insert(j, " ");
                _s = _s.Insert(j, " ");
            }
            string[] _kA = _k.Split(' ');
            string[] _sA = _s.Split(' ');
            string _dec = "";
            int _cT = int.Parse(_sA[498].Substring(0, 3));
            for (int k = 0; k < _cT; k++)
            {
                char _ch = (char)BToD((int.Parse(_sA[k]) - int.Parse(_kA[_cT - 1 - k])).ToString().PadLeft(8, '0'));
                _dec += _ch;
            }
            return _dec;
        }

        private static int BToD(string _binary)
        {
            int _binaryLength = _binary.Length;
            double _decimal = 0;
            for (int i = 0; i < _binaryLength; ++i)
            {
                _decimal += ((byte)_binary[i] - 48) * Math.Pow(2, ((_binaryLength - i) - 1));
            }
            return (int)_decimal;
        }

        private static string Reverse(string _input)
        {
            string _output = "";
            for (int i = _input.Length - 1; i >= 0; i--)
            {
                _output += _input.ElementAt(i);
            }
            return _output;
        }

        private static bool ValidAuth(string _id)
        {
            if (Authorized.ContainsKey(_id))
            {
                AuthorizedTime.TryGetValue(_id, out DateTime _expires);
                if (Expired(_expires))
                {
                    Authorized.Remove(_id);
                    AuthorizedTime.Remove(_id);
                    return false;
                }
                return true;
            }
            return false;
        }

        private static bool ValidVisit(string _id)
        {
            if (Visitor.ContainsKey(_id))
            {
                return true;
            }
            return false;
        }

        private static string SetKey(string _id, string[] _clientHandle)
        {
            string _newKey = "";
            for (int i = 0; i < 499; i++)
            {
                _newKey += Rnd.Next(12345678, 87654321).ToString();
            }
            _clientHandle[0] = _newKey;
            Authorized[_id] = _clientHandle;
            AuthorizedTime[_id] = DateTime.Now;
            string _output = _newKey.Insert(0, _id);
            string _outputReversed = "";
            for (int j = _output.Length - 1; j >= 0; j--)
            {
                _outputReversed += _output.ElementAt(j);
            }
            return _outputReversed;
        }

        private static void Writer(string _input)
        {
            using (StreamWriter sw = new StreamWriter(FilePath, true))
            {
                sw.WriteLine(string.Format("{0}: {1}", DateTime.Now, _input));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }
    }
}
