using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using UnityEngine;

namespace ServerTools.Website
{
    public partial class WebPanel
    {
        public static bool IsEnabled = false, IsRunning = false, DirFound = false;
        public static string SITE_DIR = "", ExternalIp = "", PostClient = "";
        public static int Port = 8084;
        public static AesCryptoServiceProvider AESProvider = new AesCryptoServiceProvider();
        public static Dictionary<string, string[]> AuthorizedIvKey = new Dictionary<string, string[]>();
        public static Dictionary<string, DateTime> AuthorizedTime = new Dictionary<string, DateTime>();
        public static Dictionary<string, int> LoginAttempts = new Dictionary<string, int>();
        public static Dictionary<string, int> PageHits = new Dictionary<string, int>();
        public static Dictionary<string, DateTime> TimeOut = new Dictionary<string, DateTime>();
        public static Dictionary<string, string[]> Visitor = new Dictionary<string, string[]>();
        public static Dictionary<string, string> POSTFollowUp = new Dictionary<string, string>();

        private static string file = string.Format("WebPanelLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy")), filePath = string.Format("{0}/Logs/WebPanelLogs/{1}", API.ConfigPath, file);
        private static HttpListener Listener = new HttpListener();
        private static readonly Version HttpVersion = new Version(1, 1);
        private static System.Random Rnd = new System.Random();
        private static Thread Thread;

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
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.Directory: {0}", e.Message));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.SetExternalIP: {0}", e.Message));
            }
        }


        private static void BuildLists()
        {
            try
            {
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
                Dictionary<string, DateTime> _authorizedTimeList = PersistentContainer.Instance.WebPanelAuthorizedTimeList;
                Dictionary<string, string[]> _authorizedIVKeyList = PersistentContainer.Instance.WebPanelAuthorizedIVKeyList;
                if (_authorizedTimeList != null && _authorizedIVKeyList != null && _authorizedTimeList.Count > 0 && _authorizedIVKeyList.Count > 0)
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
                            _authorizedIVKeyList.Remove(_expires.Key);
                        }
                    }
                    AuthorizedTime = _authorizedTimeList;
                    AuthorizedIvKey = _authorizedIVKeyList;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.BuildLists: {0}", e.Message));
            }
        }

        public static void Exec()
        {
            try
            {
                AESProvider.BlockSize = 128;
                AESProvider.KeySize = 256;
                AESProvider.Mode = CipherMode.CBC;
                AESProvider.Padding = PaddingMode.PKCS7;

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
                                using (HttpListenerResponse _response = _context.Response)
                                {
                                    _response.StatusCode = 403;
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
                                            return;
                                        }
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
                                                PersistentContainer.DataChange = true;
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
                                        Writer(string.Format("World data has not loaded. Request denied for IP: {0}", _ip));
                                        _response.StatusCode = 409;
                                    }
                                }
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.Exec: {0}", e.Message));
            }
            if (Listener.IsListening)
            {
                Listener.Stop();
                Listener.Close();
                Listener = new HttpListener();
            }
            else
            {
                Listener = new HttpListener();
            }
            if (Thread.IsAlive)
            {
                Thread.Abort();
            }
            if (IsEnabled)
            {
                Start();
            }
        }

        private static void IsAllowed(HttpListenerRequest _request, HttpListenerResponse _response, string _ip, string _uri)
        {
            try
            {
                using (_response)
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
                                        PersistentContainer.DataChange = true;
                                        Writer(string.Format("Homepage request denied for IP {0}. Client is now in time out for ten minutes", _ip));
                                        _response.StatusCode = 403;
                                    }
                                }
                                else
                                {
                                    PageHits[_ip] = _count + 1;
                                    Writer(string.Format("Homepage request granted for IP {0}", _ip));
                                    GET(_response, _uri, _ip);
                                }
                            }
                            else
                            {
                                PageHits.Add(_ip, 1);
                                Writer(string.Format("Homepage request granted for IP {0}", _ip));
                                GET(_response, _uri, _ip);
                            }
                        }
                        else
                        {
                            GET(_response, _uri, _ip);
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
                                }
                            }
                            POST(_response, _uri, _postMessage, _ip);
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
            }
            catch (Exception e)
            {
                if (Listener.IsListening)
                {
                    Listener.Stop();
                    Listener.Close();
                    Listener = new HttpListener();
                }
                else
                {
                    Listener = new HttpListener();
                }
                if (Thread.IsAlive)
                {
                    Thread.Abort();
                }
                Start();
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.IsAllowed: {0}", e.Message));
            }
        }

        private static void GET(HttpListenerResponse _response, string _uri, string _ip)
        {
            try
            {
                using (_response)
                {
                    string _location = SITE_DIR + _uri;
                    if (_uri == "Config" && POSTFollowUp.ContainsKey(_ip) && POSTFollowUp[_ip] == "Config")
                    {
                        POSTFollowUp.Remove(_ip);
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
                        Writer(string.Format("Received get request for missing file at {0} from IP {1}", _location, _ip));
                        _response.StatusCode = 404;
                    }
                }
            }
            catch (Exception e)
            {
                if (Listener.IsListening)
                {
                    Listener.Stop();
                    Listener.Close();
                    Listener = new HttpListener();
                }
                else
                {
                    Listener = new HttpListener();
                }
                if (Thread.IsAlive)
                {
                    Thread.Abort();
                }
                Start();
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.GET: {0}", e.Message));
            }
        }

        private static void POST(HttpListenerResponse _response, string _uri, string _postMessage, string _ip)
        {
            try
            {
                using (_response)
                {
                    if (_postMessage.Length >= 16)
                    {
                        string _responseMessage = "";
                        string _clientId = _postMessage.Substring(0, 16);
                        if (_uri == "DiscordianHandshake")
                        {
                            if (DiscordBot.IsEnabled)
                            {
                                if (!Visitor.ContainsKey(_clientId))
                                {
                                    if (!POSTFollowUp.ContainsKey(_clientId))
                                    {
                                        POSTFollowUp.Add(_clientId, "DiscordianHandshake");
                                        AESProvider.GenerateIV();
                                        string[] _iVKey = new string[] { Convert.ToBase64String(AESProvider.IV) };
                                        Visitor.Add(_clientId, _iVKey);
                                        _responseMessage += _iVKey[0];
                                        _response.StatusCode = 200;
                                    }
                                    else if (POSTFollowUp[_clientId] == "DiscordianHandshake")
                                    {
                                        POSTFollowUp.Remove(_clientId);
                                        Visitor.TryGetValue(_clientId, out string[] _iV);
                                        AESProvider.Key = DiscordBot.Token;
                                        AESProvider.IV = Convert.FromBase64String(_iV[0]);
                                        if (DiscordDecrypt(_postMessage.Substring(16)) == DiscordBot.TokenS)
                                        {
                                            Visitor.Remove(_clientId);
                                            AESProvider.GenerateIV();
                                            AuthorizedIvKey.Add(_clientId, new string[] { Convert.ToBase64String(AESProvider.IV) });
                                            AuthorizedTime.Add(_clientId, DateTime.Now);
                                            _responseMessage += Convert.ToBase64String(AESProvider.IV);
                                            _response.StatusCode = 200;
                                        }
                                        else
                                        {
                                            _response.StatusCode = 403;
                                        }
                                    }
                                }
                                else
                                {
                                    _response.StatusCode = 202;
                                }
                            }
                        }
                        else if (_uri == "Handshake")
                        {
                            if (_postMessage.Length == 16)
                            {
                                if (!Visitor.ContainsKey(_postMessage))
                                {
                                    Visitor.Add(_postMessage, null);
                                    _response.StatusCode = 200;
                                }
                                else
                                {
                                    _response.StatusCode = 401;
                                }
                            }
                            else
                            {
                                _response.StatusCode = 403;
                            }
                        }
                        else if (_uri == "SignIn")
                        {
                            if (Visitor.ContainsKey(_clientId))
                            {
                                if (!POSTFollowUp.ContainsKey(_clientId))
                                {
                                    string _login = _postMessage.Substring(16);
                                    if (!string.IsNullOrEmpty(PersistentContainer.Instance.Players[_login].WebPass) && PersistentContainer.Instance.Players[_login].WebPass != "")
                                    {
                                        string _key = "";
                                        string _kChop = PersistentContainer.Instance.Players[_login].WebPass;
                                        if (_kChop.Length >= 16)
                                        {
                                            _key += _kChop.Substring(0, 16);
                                        }
                                        else if (_kChop.Length >= 8)
                                        {
                                            _kChop = _kChop.Substring(0, 8);
                                            _key += _kChop + _kChop;
                                        }
                                        else
                                        {
                                            _kChop = _kChop.Substring(0, 4);
                                            _key += _kChop + _kChop + _kChop + _kChop;
                                        }
                                        string _iv = PersistentOperations.CreatePassword(16);
                                        Visitor[_clientId] = new string[] { _login, _key, _iv };
                                        POSTFollowUp.Add(_clientId, "SignIn");
                                        _responseMessage += _iv;
                                        _response.StatusCode = 200;
                                    }
                                    else
                                    {
                                        _response.StatusCode = 403;
                                    }
                                }
                                else if (POSTFollowUp[_clientId] == "SignIn")
                                {
                                    POSTFollowUp.Remove(_clientId);
                                    Visitor.TryGetValue(_clientId, out string[] _IVKey);
                                    string _decrypted = PanelDecrypt(_postMessage.Substring(16), Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                    if (_decrypted == PersistentContainer.Instance.Players[_IVKey[0]].WebPass)
                                    {
                                        Visitor.Remove(_clientId);
                                        string _newIv = PersistentOperations.CreatePassword(16);
                                        AuthorizedIvKey.Add(_clientId, new string[] { _IVKey[0], _IVKey[1], _newIv });
                                        AuthorizedTime.Add(_clientId, DateTime.Now);
                                        _responseMessage += _newIv;
                                        _response.StatusCode = 200;
                                    }
                                    else
                                    {
                                        _response.StatusCode = 403;
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
                            if (AuthorizedIvKey.ContainsKey(_clientId))
                            {
                                AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                if (Expired(_expires))
                                {
                                    AuthorizedIvKey.Remove(_clientId);
                                    AuthorizedTime.Remove(_clientId);
                                    _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                    _response.StatusCode = 401;
                                }
                                else
                                {
                                    AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                    string _decrypted = PanelDecrypt(_postMessage.Substring(16), Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                    if (_decrypted == _clientId)
                                    {
                                        AuthorizedIvKey.Remove(_clientId);
                                        AuthorizedTime.Remove(_clientId);
                                        _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                        _response.StatusCode = 200;
                                        Writer(string.Format("Client {0} at IP {1} has signed out", _IVKey[0], _ip));
                                    }
                                    else
                                    {
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
                            if (AuthorizedIvKey.ContainsKey(_clientId))
                            {
                                if (!POSTFollowUp.ContainsKey(_clientId))
                                {
                                    AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                    if (Expired(_expires))
                                    {
                                        AuthorizedIvKey.Remove(_clientId);
                                        AuthorizedTime.Remove(_clientId);
                                        _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                        _response.StatusCode = 401;
                                    }
                                    else
                                    {
                                        AuthorizedTime[_clientId] = DateTime.Now;
                                        AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                        string _decrypted = PanelDecrypt(_postMessage.Substring(16), Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                        if (_decrypted == _clientId)
                                        {
                                            string _newIv = PersistentOperations.CreatePassword(16);
                                            AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                            POSTFollowUp.Add(_clientId, "NewPass");
                                            _responseMessage += _newIv;
                                            _response.StatusCode = 200;
                                        }
                                        else
                                        {
                                            _response.StatusCode = 403;
                                        }
                                    }
                                }
                                else if (POSTFollowUp[_clientId] == "NewPass")
                                {
                                    POSTFollowUp.Remove(_clientId);
                                    AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                    string _decrypted = PanelDecrypt(_postMessage.Substring(16), Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                    PersistentContainer.Instance.Players[_IVKey[0]].WebPass = _decrypted;
                                    PersistentContainer.DataChange = true;
                                    string _newIv = PersistentOperations.CreatePassword(16);
                                    AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                    _responseMessage += _newIv;
                                    _response.StatusCode = 200;
                                    Writer(string.Format("Client {0} at IP {1} has set a new password", _IVKey[0], _ip));
                                }
                            }
                            else
                            {
                                _response.StatusCode = 401;
                            }
                        }
                        else if (_uri == "Console")
                        {
                            if (AuthorizedIvKey.ContainsKey(_clientId))
                            {
                                if (!POSTFollowUp.ContainsKey(_clientId))
                                {
                                    AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                    if (Expired(_expires))
                                    {
                                        AuthorizedIvKey.Remove(_clientId);
                                        AuthorizedTime.Remove(_clientId);
                                        _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                        _response.StatusCode = 401;
                                    }
                                    else
                                    {
                                        AuthorizedTime[_clientId] = DateTime.Now;
                                        AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                        string _decrypted = PanelDecrypt(_postMessage.Substring(16), Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                        if (_decrypted == _clientId)
                                        {
                                            string _newIv = PersistentOperations.CreatePassword(16);
                                            AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                            POSTFollowUp.Add(_clientId, "Console");
                                            _responseMessage += _newIv;
                                            _response.StatusCode = 200;
                                        }
                                        else
                                        {
                                            _response.StatusCode = 403;
                                        }
                                    }
                                }
                                else if (POSTFollowUp[_clientId] == "Console")
                                {
                                    POSTFollowUp.Remove(_clientId);
                                    int.TryParse(_postMessage.Substring(16), out int _lineNumber);
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
                            else
                            {
                                _response.StatusCode = 401;
                            }
                        }
                        else if (_uri == "Command")
                        {
                            if (AuthorizedIvKey.ContainsKey(_clientId))
                            {
                                if (!POSTFollowUp.ContainsKey(_clientId))
                                {
                                    AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                    if (Expired(_expires))
                                    {
                                        AuthorizedIvKey.Remove(_clientId);
                                        AuthorizedTime.Remove(_clientId);
                                        _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                        _response.StatusCode = 401;
                                    }
                                    else
                                    {
                                        AuthorizedTime[_clientId] = DateTime.Now;
                                        AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                        string _decrypted = PanelDecrypt(_postMessage.Substring(16), Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                        if (_decrypted == _clientId)
                                        {
                                            string _newIv = PersistentOperations.CreatePassword(16);
                                            AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                            POSTFollowUp.Add(_clientId, "Command");
                                            _responseMessage += _newIv;
                                            _response.StatusCode = 200;
                                        }
                                        else
                                        {
                                            _response.StatusCode = 403;
                                        }
                                    }
                                }
                                else if (POSTFollowUp[_clientId] == "Command")
                                {
                                    POSTFollowUp.Remove(_clientId);
                                    AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                    string[] idLineCountandCommand = _postMessage.Split(new[] { '☼' }, 3);
                                    string _decrypted = PanelDecrypt(idLineCountandCommand[2], Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                    string _newIv = PersistentOperations.CreatePassword(16);
                                    AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                    string _command = _decrypted;
                                    if (_command.Length > 300)
                                    {
                                        _responseMessage += _newIv;
                                        _response.StatusCode = 400;
                                    }
                                    else
                                    {
                                        IConsoleCommand _commandValid = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(_command, false);
                                        if (_commandValid == null)
                                        {
                                            _responseMessage += _newIv;
                                            _response.StatusCode = 406;
                                        }
                                        else
                                        {
                                            ClientInfo _cInfo = new ClientInfo();
                                            _cInfo.playerId = "-Web_Panel- " + _IVKey[0];
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
                                            _responseMessage += "☼" + _logCount + "☼" + _newIv;
                                            Writer(string.Format("Executed console command '{0}' from Client: {1} IP: {2}", _command, _IVKey[0], _ip));
                                            _response.StatusCode = 200;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _response.StatusCode = 401;
                            }
                        }
                        else if (_uri == "Players")
                        {
                            if (AuthorizedIvKey.ContainsKey(_clientId))
                            {
                                if (!POSTFollowUp.ContainsKey(_clientId))
                                {
                                    AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                    if (Expired(_expires))
                                    {
                                        AuthorizedIvKey.Remove(_clientId);
                                        AuthorizedTime.Remove(_clientId);
                                        _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                        _response.StatusCode = 401;
                                    }
                                    else
                                    {
                                        AuthorizedTime[_clientId] = DateTime.Now;
                                        AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                        string _decrypted = PanelDecrypt(_postMessage.Substring(16), Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                        if (_decrypted == _clientId)
                                        {
                                            string _newIv = PersistentOperations.CreatePassword(16);
                                            AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                            POSTFollowUp.Add(_clientId, "Players");
                                            _responseMessage += _newIv;
                                            _response.StatusCode = 200;
                                        }
                                        else
                                        {
                                            _response.StatusCode = 403;
                                        }
                                    }
                                }
                                else if (POSTFollowUp[_clientId] == "Players")
                                {
                                    POSTFollowUp.Remove(_clientId);
                                    List<ClientInfo> _clientList = PersistentOperations.ClientList();
                                    if (_clientList != null && _clientList.Count > 0)
                                    {
                                        int _count = 0;
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
                                                    _count++;
                                                }
                                            }
                                        }
                                        if (_responseMessage != "")
                                        {
                                            _responseMessage = _responseMessage.TrimEnd('☼');
                                            AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                            string _encrypted = PanelEncrypt(_responseMessage, Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                            string _decrypted = PanelDecrypt(_encrypted, Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                            string _newIv = PersistentOperations.CreatePassword(16);
                                            AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                            _responseMessage = _encrypted + "☼" + _newIv + "☼" + _count;
                                        }
                                    }
                                    _response.StatusCode = 200;
                                }
                            }
                        }
                        else if (_uri == "Config")
                        {
                            if (AuthorizedIvKey.ContainsKey(_clientId))
                            {
                                AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                if (Expired(_expires))
                                {
                                    AuthorizedIvKey.Remove(_clientId);
                                    AuthorizedTime.Remove(_clientId);
                                    _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                    _response.StatusCode = 401;
                                }
                                else
                                {
                                    AuthorizedTime[_clientId] = DateTime.Now;
                                    AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                    string _decrypted = PanelDecrypt(_postMessage.Substring(16), Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                    if (_decrypted == _clientId)
                                    {
                                        string _newIv = PersistentOperations.CreatePassword(16);
                                        AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                        POSTFollowUp.Add(_ip, "Config");
                                        _responseMessage += _newIv;
                                        _response.StatusCode = 200;
                                    }
                                    else
                                    {
                                        _response.StatusCode = 403;
                                    }
                                }
                            }
                            else
                            {
                                _response.StatusCode = 401;
                            }
                        }
                        else if (_uri == "SaveConfig")
                        {
                            if (AuthorizedIvKey.ContainsKey(_clientId))
                            {
                                if (!POSTFollowUp.ContainsKey(_clientId))
                                {
                                    AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                    if (Expired(_expires))
                                    {
                                        AuthorizedIvKey.Remove(_clientId);
                                        AuthorizedTime.Remove(_clientId);
                                        _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                        _response.StatusCode = 401;
                                    }
                                    else
                                    {
                                        AuthorizedTime[_clientId] = DateTime.Now;
                                        AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                        string _decrypted = PanelDecrypt(_postMessage.Substring(16), Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                        if (_decrypted == _clientId)
                                        {
                                            string _newIv = PersistentOperations.CreatePassword(16);
                                            AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                            POSTFollowUp.Add(_clientId, "SaveConfig");
                                            _responseMessage += _newIv;
                                            _response.StatusCode = 200;
                                        }
                                        else
                                        {
                                            _response.StatusCode = 403;
                                        }
                                    }
                                }
                                else if (POSTFollowUp[_clientId] == "SaveConfig")
                                {
                                    POSTFollowUp.Remove(_clientId);
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
                                        AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                        string _decrypted = PanelDecrypt(_postMessage.Substring(16), Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                        string _newIv = PersistentOperations.CreatePassword(16);
                                        AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                        _responseMessage += _newIv;
                                        bool _changed = false;
                                        XmlNodeList _nodes = _xmlDoc.GetElementsByTagName("Tool");
                                        _decrypted = _decrypted.TrimEnd('☼');
                                        string[] _tools = _decrypted.Split('☼');
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
                            else
                            {
                                _response.StatusCode = 403;
                            }
                        }
                        else if (_uri == "Kick")
                        {
                            if (AuthorizedIvKey.ContainsKey(_clientId))
                            {
                                AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                if (Expired(_expires))
                                {
                                    AuthorizedIvKey.Remove(_clientId);
                                    AuthorizedTime.Remove(_clientId);
                                    _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                    _response.StatusCode = 401;
                                }
                                else
                                {
                                    AuthorizedTime[_clientId] = DateTime.Now;
                                    AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                    string[] _commandSplit = _postMessage.Substring(16).Split('☼');
                                    string _decrypted = PanelDecrypt(_commandSplit[0], Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                    if (_decrypted == _clientId)
                                    {
                                        string _newIv = PersistentOperations.CreatePassword(16);
                                        AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                        _responseMessage += _newIv;
                                        ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_commandSplit[1]);
                                        if (_cInfo != null)
                                        {
                                            SdtdConsole.Instance.ExecuteSync(string.Format("kick {0}", _cInfo.playerId), null);
                                            Writer(string.Format("Client {0} at IP {1} has kicked {2}", _clientId, _ip, _cInfo.playerId));
                                            _response.StatusCode = 200;
                                        }
                                        else
                                        {
                                            _response.StatusCode = 406;
                                        }
                                    }
                                    else
                                    {
                                        _response.StatusCode = 403;
                                    }
                                }
                            }
                            else
                            {
                                _response.StatusCode = 403;
                            }
                        }
                        else if (_uri == "Ban")
                        {
                            if (AuthorizedIvKey.ContainsKey(_clientId))
                            {
                                AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                if (Expired(_expires))
                                {
                                    AuthorizedIvKey.Remove(_clientId);
                                    AuthorizedTime.Remove(_clientId);
                                    _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                    _response.StatusCode = 401;
                                }
                                else
                                {
                                    AuthorizedTime[_clientId] = DateTime.Now;
                                    AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                    string[] _commandSplit = _postMessage.Substring(16).Split('☼');
                                    string _decrypted = PanelDecrypt(_commandSplit[0], Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                    if (_decrypted == _clientId)
                                    {
                                        string _newIv = PersistentOperations.CreatePassword(16);
                                        AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                        _responseMessage += _newIv;
                                        ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_commandSplit[1]);
                                        if (_cInfo != null)
                                        {
                                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 year", _cInfo.playerId), null);
                                            Writer(string.Format("Client {0} at IP {1} has banned id {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
                                            _response.StatusCode = 200;
                                        }
                                        else
                                        {
                                            PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_commandSplit[1]);
                                            if (_pdf != null)
                                            {
                                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 year", _pdf.ecd.belongsPlayerId), null);
                                                Writer(string.Format("Client {0} at IP {1} has banned id {2} named {3}", _IVKey[0], _ip, _pdf.ecd.belongsPlayerId, _pdf.ecd.entityName));
                                                _response.StatusCode = 406;
                                            }
                                            else
                                            {
                                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 year", _commandSplit[1]), null);
                                                Writer(string.Format("Client {0} at IP {1} has banned id {2}", _IVKey[0], _ip, _commandSplit[1]));
                                                _response.StatusCode = 406;

                                            }
                                        }
                                    }
                                    else
                                    {
                                        _response.StatusCode = 403;
                                    }
                                }
                            }
                            else
                            {
                                _response.StatusCode = 403;
                            }

                            if (AuthorizedIvKey.ContainsKey(_clientId))
                            {
                                AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                if (Expired(_expires))
                                {
                                    AuthorizedIvKey.Remove(_clientId);
                                    AuthorizedTime.Remove(_clientId);
                                    _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                    _response.StatusCode = 401;
                                }
                                else
                                {
                                    AuthorizedTime[_clientId] = DateTime.Now;
                                    AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                    string[] _commandSplit = _postMessage.Substring(16).Split('☼');
                                    string _decrypted = PanelDecrypt(_commandSplit[0], Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                    if (_decrypted == _clientId)
                                    {
                                        string _newIv = PersistentOperations.CreatePassword(16);
                                        AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                        _responseMessage += _newIv;
                                        ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_commandSplit[1]);
                                        if (_cInfo != null)
                                        {
                                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 year", _cInfo.playerId), null);
                                            Writer(string.Format("Client {0} at IP {1} has banned id {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
                                            _response.StatusCode = 200;
                                        }
                                        else
                                        {
                                            PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_commandSplit[1]);
                                            if (_pdf != null)
                                            {
                                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 year", _pdf.ecd.belongsPlayerId), null);
                                                Writer(string.Format("Client {0} at IP {1} has banned id {2} named {3}", _IVKey[0], _ip, _pdf.ecd.belongsPlayerId, _pdf.ecd.entityName));
                                                _response.StatusCode = 406;
                                            }
                                            else
                                            {
                                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 year", _commandSplit[1]), null);
                                                Writer(string.Format("Client {0} at IP {1} has banned id {2}", _IVKey[0], _ip, _commandSplit[1]));
                                                _response.StatusCode = 406;
                                            }
                                        }
                                    }
                                    else
                                    {
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
                            if (AuthorizedIvKey.ContainsKey(_clientId))
                            {
                                AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                if (Expired(_expires))
                                {
                                    AuthorizedIvKey.Remove(_clientId);
                                    AuthorizedTime.Remove(_clientId);
                                    _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                    _response.StatusCode = 401;
                                }
                                else
                                {
                                    AuthorizedTime[_clientId] = DateTime.Now;
                                    AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                    string[] _commandSplit = _postMessage.Substring(16).Split('☼');
                                    string _decrypted = PanelDecrypt(_commandSplit[0], Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                    if (_decrypted == _clientId)
                                    {
                                        string _newIv = PersistentOperations.CreatePassword(16);
                                        AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                        _responseMessage += _newIv;
                                        if (Mute.IsEnabled)
                                        {
                                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_commandSplit[1]);
                                            if (_cInfo != null)
                                            {
                                                if (Mute.Mutes.Contains(_cInfo.playerId))
                                                {
                                                    Mute.Mutes.Remove(_cInfo.playerId);
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteTime = 0;
                                                    Writer(string.Format("Client {0} at IP {1} has unmuted id {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
                                                    _response.StatusCode = 202;
                                                }
                                                else
                                                {
                                                    Mute.Mutes.Add(_cInfo.playerId);
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteTime = -1;
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteName = _cInfo.playerName;
                                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteDate = DateTime.Now;
                                                    Writer(string.Format("Client {0} at IP {1} has muted id {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
                                                    _response.StatusCode = 200;
                                                }
                                                PersistentContainer.DataChange = true;
                                            }
                                            else
                                            {
                                                if (Mute.Mutes.Contains(_commandSplit[1]))
                                                {
                                                    Mute.Mutes.Remove(_commandSplit[1]);
                                                    PersistentContainer.Instance.Players[_commandSplit[1]].MuteTime = 0;
                                                    Writer(string.Format("Client {0} at IP {1} has unmuted id {2}", _IVKey[0], _ip, _commandSplit[1]));
                                                    _response.StatusCode = 202;
                                                }
                                                else
                                                {
                                                    Mute.Mutes.Add(_commandSplit[1]);
                                                    PersistentContainer.Instance.Players[_commandSplit[1]].MuteTime = -1;
                                                    PersistentContainer.Instance.Players[_commandSplit[1]].MuteName = "-Unknown-";
                                                    PersistentContainer.Instance.Players[_commandSplit[1]].MuteDate = DateTime.Now;
                                                    Writer(string.Format("Client {0} at IP {1} has muted id {2}", _IVKey[0], _ip, _commandSplit[1]));
                                                    _response.StatusCode = 200;
                                                }
                                                PersistentContainer.DataChange = true;
                                            }
                                        }
                                        else
                                        {
                                            _response.StatusCode = 406;
                                        }
                                    }
                                    else
                                    {
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
                            if (AuthorizedIvKey.ContainsKey(_clientId))
                            {
                                AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                if (Expired(_expires))
                                {
                                    AuthorizedIvKey.Remove(_clientId);
                                    AuthorizedTime.Remove(_clientId);
                                    _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                    _response.StatusCode = 401;
                                }
                                else
                                {
                                    AuthorizedTime[_clientId] = DateTime.Now;
                                    AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                    string[] _commandSplit = _postMessage.Substring(16).Split('☼');
                                    string _decrypted = PanelDecrypt(_commandSplit[0], Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                    if (_decrypted == _clientId)
                                    {
                                        string _newIv = PersistentOperations.CreatePassword(16);
                                        AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                        _responseMessage += _newIv;
                                        if (Jail.IsEnabled)
                                        {
                                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_commandSplit[1]);
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
                                                        PersistentContainer.DataChange = true;
                                                        if (_position != null && _position.Count > 0)
                                                        {
                                                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_position[0].x, -1, _position[0].z), null, false));
                                                        }
                                                        else
                                                        {
                                                            Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_pos[0].x, -1, _pos[0].z), null, false));
                                                        }
                                                        Writer(string.Format("Client {0} at IP {1} has unjailed {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
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
                                                    PersistentContainer.DataChange = true;
                                                    Writer(string.Format("Client {0} at IP {1} has jailed {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
                                                    _response.StatusCode = 200;
                                                }
                                                else
                                                {
                                                    _response.StatusCode = 409;
                                                }
                                            }
                                            else
                                            {
                                                if (Jail.Jailed.Contains(_commandSplit[1]))
                                                {
                                                    Jail.Jailed.Remove(_commandSplit[1]);
                                                    PersistentContainer.Instance.Players[_commandSplit[1]].JailTime = 0;
                                                    Writer(string.Format("Client {0} at IP {1} has unjailed {2}", _IVKey[0], _ip, _commandSplit[1]));
                                                }
                                                else if (Jail.Jail_Position != "")
                                                {
                                                    Jail.Jailed.Add(_commandSplit[1]);
                                                    PersistentContainer.Instance.Players[_commandSplit[1]].JailTime = -1;
                                                    PersistentContainer.Instance.Players[_commandSplit[1]].JailName = "-Unknown-";
                                                    PersistentContainer.Instance.Players[_commandSplit[1]].JailDate = DateTime.Now;
                                                    Writer(string.Format("Client {0} at IP {1} has jailed {2}", _IVKey[0], _ip, _commandSplit[1]));
                                                }
                                                PersistentContainer.DataChange = true;
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
                                        AuthorizedIvKey.Remove(_clientId);
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
                            if (AuthorizedIvKey.ContainsKey(_clientId))
                            {
                                AuthorizedTime.TryGetValue(_clientId, out DateTime _expires);
                                if (Expired(_expires))
                                {
                                    AuthorizedIvKey.Remove(_clientId);
                                    AuthorizedTime.Remove(_clientId);
                                    _response.Redirect("http://" + ExternalIp + ":" + Port + "/st.html");
                                    _response.StatusCode = 401;
                                }
                                else
                                {
                                    AuthorizedTime[_clientId] = DateTime.Now;
                                    AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                    string[] _commandSplit = _postMessage.Substring(16).Split('☼');
                                    string _decrypted = PanelDecrypt(_commandSplit[0], Encoding.UTF8.GetBytes(_IVKey[1]), Encoding.UTF8.GetBytes(_IVKey[2]));
                                    if (_decrypted == _clientId)
                                    {
                                        string _newIv = PersistentOperations.CreatePassword(16);
                                        AuthorizedIvKey[_clientId] = new string[] { _IVKey[0], _IVKey[1], _newIv };
                                        _responseMessage += _newIv;
                                        if (VoteReward.IsEnabled)
                                        {
                                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_commandSplit[1]);
                                            if (_cInfo != null)
                                            {
                                                VoteReward.ItemOrBlockCounter(_cInfo, VoteReward.Reward_Count);
                                                Writer(string.Format("Client {0} at IP {1} has rewarded {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
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
                        _response.StatusCode = 403;
                    }
                }
            }
            catch (Exception e)
            {
                if (Listener.IsListening)
                {
                    Listener.Stop();
                    Listener.Close();
                    Listener = new HttpListener();
                }
                else
                {
                    Listener = new HttpListener();
                }
                if (Thread.IsAlive)
                {
                    Thread.Abort();
                }
                Start();
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.POST: {0}", e.Message));
            }
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            string decrypted = null;
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;
                rijAlg.Key = key;
                rijAlg.IV = iv;
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            decrypted = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return decrypted;
        }

        private static byte[] EncryptStringToBytes(string target, byte[] key, byte[] iv)
        {
            byte[] encrypted;
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;
                rijAlg.Key = key;
                rijAlg.IV = iv;
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(target);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.Expired: {0}", e.Message));
            }
            return false;
        }

        private static string DiscordEncrypt(string _target)
        {
            ICryptoTransform _iCT = AESProvider.CreateEncryptor();
            byte[] _bArr1 = Convert.FromBase64String(_target);
            byte[] _bArr2 = _iCT.TransformFinalBlock(_bArr1, 0, _bArr1.Length);
            string _encrypted = Convert.ToBase64String(_bArr2);
            _iCT.Dispose();
            return _encrypted;
        }

        private static string DiscordDecrypt(string _target)
        {
            string _decrypted = "";
            try
            {
                ICryptoTransform _iCT = AESProvider.CreateDecryptor();
                byte[] _bArr1 = Convert.FromBase64String(_target);
                byte[] _bArr2 = _iCT.TransformFinalBlock(_bArr1, 0, _bArr1.Length);
                _decrypted = Convert.ToBase64String(_bArr2);
                _iCT.Dispose();
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebPanel.DiscordDecrypt: {0}", e.Message));
            }
            return _decrypted;
        }

        private static string PanelDecrypt(string _target, byte[] _key, byte[] _iv)
        {
            string _decrypt = "";
            try
            {
                var encrypted = Convert.FromBase64String(_target);
                _decrypt = DecryptStringFromBytes(encrypted, _key, _iv);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.PanelDecrypt: {0}", e.Message));
            }
            return _decrypt;
        }

        private static string PanelEncrypt(string _target, byte[] _key, byte[] _iv)
        {
            string _encrypted = "";
            try
            {
                byte[] encryptStringToBytes = EncryptStringToBytes(_target, _key, _iv);
                _encrypted = Convert.ToBase64String(encryptStringToBytes);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in API.PanelEncrypt: {0}", e.Message));
            }
            return _encrypted;
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
                Log.Out(string.Format("Error in WebPanel.Writer: {0}", e.Message));
            }
        }
    }
}
