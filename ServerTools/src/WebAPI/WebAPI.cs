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

namespace ServerTools
{
    public class WebAPI
    {
        public static bool IsEnabled = false, IsRunning = false, Shutdown = false;
        public static int Port = 8084;
        public static string Directory = "", Panel_Address = "";
        public static AesCryptoServiceProvider AESProvider = new AesCryptoServiceProvider();
        public static Dictionary<string, string[]> AuthorizedIvKey = new Dictionary<string, string[]>();
        public static Dictionary<string, DateTime> AuthorizedTime = new Dictionary<string, DateTime>();
        public static Dictionary<string, string[]> Visitor = new Dictionary<string, string[]>();
        public static Dictionary<string, string> POSTFollowUp = new Dictionary<string, string>();

        private static string Redirect = "", BaseAddress = "";
        private static HttpListener Listener = new HttpListener();
        private static Thread Thread;
        private static readonly Version HttpVersion = new Version(1, 1);

        public static void Load()
        {
            IsRunning = true;
            BuildLists();
            Start();
        }

        public static void Unload()
        {
            IsRunning = false;
            if (Thread != null && Thread.IsAlive)
            {
                Thread.Abort();
            }
            if (Listener != null && Listener.IsListening)
            {
                Listener.Stop();
            }
        }

        private static void Start()
        {
            if (Listener == null)
            {
                Listener = new HttpListener();
            }
            Thread = new Thread(new ThreadStart(Exec))
            {
                IsBackground = true
            };
            Thread.Start();
        }

        public static bool SetBaseAddress()
        {
            try
            {
                string _ip = GamePrefs.GetString(EnumGamePrefs.ServerIP);
                if (!string.IsNullOrEmpty(_ip) && _ip != "")
                {
                    BaseAddress = _ip;
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.SetBaseAddress: {0}", e.Message));
            }
            Log.Out("[SERVERTOOLS] Host IP could not be verified. Web API has been disabled");
            return false;
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
                    WebPanel.TimeOut = _timeoutList;
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.BuildLists: {0}", e.Message));
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
                    Log.Out("[SERVERTOOLS] Web_API port was set identically to the server control panel or telnet port. You must use a unique and unused port that is open to transmission. API has been disabled");
                    return;
                }
                if (Port > 1000 && Port < 65536)
                {
                    if (HttpListener.IsSupported)
                    {
                        Listener.Prefixes.Add(string.Format("http://*:{0}/", Port));
                        Listener.Start();
                        if (SetBaseAddress())
                        {
                            Redirect = "http://" + BaseAddress + ":" + Port;
                            Log.Out(string.Format("[SERVERTOOLS] ServerTools web api has opened @ {0}", Redirect));
                            Panel_Address = "http://" + BaseAddress + ":" + Port + "/st.html";
                            if (WebPanel.IsEnabled)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] ServerTools web panel is available @ {0}", Panel_Address));
                            }
                            while (Listener != null && Listener.IsListening)
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
                                            WebPanel.Writer(string.Format("URI request was too long. Request denied for IP: {0}", _ip));
                                            _response.StatusCode = 414;
                                        }
                                        else if (GameManager.Instance.World != null)
                                        {
                                            if (_uri.Contains("<script>"))
                                            {
                                                return;
                                            }
                                            if (!WebPanel.TimeOut.ContainsKey(_ip))
                                            {
                                                IsAllowed(_request, _response, _ip, _uri);
                                            }
                                            else
                                            {
                                                WebPanel.TimeOut.TryGetValue(_ip, out DateTime _timeout);
                                                if (DateTime.Now >= _timeout)
                                                {
                                                    WebPanel.TimeOut.Remove(_ip);
                                                    PersistentContainer.Instance.WebPanelTimeoutList.Remove(_ip);
                                                    PersistentContainer.DataChange = true;
                                                    IsAllowed(_request, _response, _ip, _uri);
                                                }
                                                else
                                                {
                                                    WebPanel.Writer(string.Format("Request denied for IP: {0} on timeout until: {1}", _ip, _timeout));
                                                    _response.StatusCode = 403;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            WebPanel.Writer(string.Format("World data has not loaded. Request denied for IP: {0}", _ip));
                                            _response.StatusCode = 409;
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            WebPanel.Writer(string.Format("The host ip could not be determined. Web API is disabled"));
                            Log.Out(string.Format("[SERVERTOOLS] The host ip could not be determined. Web API is disabled"));
                        }
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] This host can not support the web panel. Panel has been disabled"));
                    }
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] The port is set to an invalid number. It must be between 1001-65531 for ServerTools web api to function. Web api has been disabled"));
                }
            }
            catch (Exception e)
            {
                if (e.Message.Length > 0)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.Exec: {0}", e.Message));
                }
            }
            if (Thread != null && Thread.IsAlive)
            {
                Thread.Abort();
            }
            if (Listener != null && Listener.IsListening)
            {
                Listener.Stop();
            }
            if (IsEnabled && IsRunning && !PersistentOperations.Shutdown_Initiated)
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
                    if (_request.HttpMethod == "GET")
                    {
                        _uri = _uri.Remove(0, _uri.IndexOf(Port.ToString()) + Port.ToString().Length + 1);
                        if (WebPanel.IsEnabled && _uri.Contains("st.html"))
                        {
                            if (_uri.EndsWith("/"))
                            {
                                _response.Redirect("http://" + BaseAddress + ":" + Port + "/st.html");
                                _response.StatusCode = 308;
                            }
                            else if (WebPanel.PageHits.ContainsKey(_ip))
                            {
                                WebPanel.PageHits.TryGetValue(_ip, out int _count);
                                if (_count++ >= 6)
                                {
                                    WebPanel.PageHits.Remove(_ip);
                                    if (!_request.IsLocal)
                                    {
                                        WebPanel.TimeOut.Add(_ip, DateTime.Now.AddMinutes(5));
                                        PersistentContainer.Instance.WebPanelTimeoutList.Add(_ip, DateTime.Now.AddMinutes(5));
                                        PersistentContainer.DataChange = true;
                                        WebPanel.Writer(string.Format("Homepage request denied for IP {0}. Client is now in time out for five minutes", _ip));
                                        _response.StatusCode = 403;
                                    }
                                }
                                else
                                {
                                    WebPanel.PageHits[_ip] = _count + 1;
                                    WebPanel.Writer(string.Format("Homepage request granted for IP {0}", _ip));
                                    GET(_response, _uri, _ip);
                                }
                            }
                            else
                            {
                                WebPanel.PageHits.Add(_ip, 1);
                                WebPanel.Writer(string.Format("Homepage request granted for IP {0}", _ip));
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
                            _uri = _uri.Remove(0, _uri.IndexOf(Port.ToString()) + Port.ToString().Length + 1);
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
                    }
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.IsAllowed: {0}", e.Message));
                if (IsEnabled)
                {
                    Start();
                }
            }
        }

        private static void GET(HttpListenerResponse _response, string _uri, string _ip)
        {
            try
            {
                using (_response)
                {
                    string _location = WebAPI.Directory + _uri;
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
                            _response.AddHeader("Keep-Alive", "timeout=300, max=100");
                            _response.ContentType = MimeType.GetMimeType(Path.GetExtension(_fileInfo.Extension));
                            _response.ContentLength64 = (long)_c.Length;
                            _response.ContentEncoding = Encoding.UTF8;
                            _response.OutputStream.Write(_c, 0, _c.Length);
                        }
                        else
                        {
                            WebPanel.Writer(string.Format("Requested file was found but unable to form: {0}", _location));
                            _response.StatusCode = 404;
                        }
                    }
                    else
                    {
                        WebPanel.Writer(string.Format("Received get request for missing file at {0} from IP {1}", _location, _ip));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.GET: {0}", e.Message));
                if (IsEnabled)
                {
                    Start();
                }
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
                        if (DiscordBot.IsEnabled)
                        {
                            if (_uri == "DiscordHandshake")
                            {
                                if (!Visitor.ContainsKey(_clientId) && !POSTFollowUp.ContainsKey(_clientId))
                                {
                                    POSTFollowUp.Add(_clientId, "DiscordHandshake");
                                    AESProvider.GenerateIV();
                                    Visitor.Add(_clientId, new string[] { Convert.ToBase64String(AESProvider.IV) });
                                    _responseMessage += Convert.ToBase64String(AESProvider.IV);
                                    _response.StatusCode = 200;
                                }
                                else if (POSTFollowUp.ContainsKey(_clientId) && POSTFollowUp[_clientId] == "DiscordHandshake")
                                {
                                    POSTFollowUp.Remove(_clientId);
                                    Visitor.TryGetValue(_clientId, out string[] _IVKey);
                                    AESProvider.Key = DiscordBot.TokenBytes;
                                    AESProvider.IV = Convert.FromBase64String(_IVKey[0]);
                                    if (DiscordSecurityDecrypt(_postMessage.Substring(16)) == DiscordBot.TokenKey)
                                    {
                                        Visitor.Remove(_clientId);
                                        AESProvider.GenerateIV();
                                        AuthorizedIvKey.Add(_clientId, new string[]
                                        {
                                            Convert.ToBase64String(AESProvider.IV)
                                        });
                                        _responseMessage += Convert.ToBase64String(AESProvider.IV);
                                        _response.StatusCode = 200;
                                    }
                                    else
                                    {
                                        Visitor.Remove(_clientId);
                                        _response.StatusCode = 403;
                                    }
                                }
                                else
                                {
                                    _response.StatusCode = 409;
                                }
                            }
                            else if (_uri == "DiscordPost")
                            {
                                if (AuthorizedIvKey.ContainsKey(_clientId))
                                {
                                    if (!POSTFollowUp.ContainsKey(_clientId))
                                    {
                                        AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                        AESProvider.Key = DiscordBot.TokenBytes;
                                        AESProvider.IV = Convert.FromBase64String(_IVKey[0]);
                                        if (DiscordSecurityDecrypt(_postMessage.Substring(16)) == DiscordBot.TokenKey)
                                        {
                                            POSTFollowUp.Add(_clientId, "DiscordPost");
                                            AESProvider.GenerateIV();
                                            AuthorizedIvKey[_clientId] = new string[] { Convert.ToBase64String(AESProvider.IV) };
                                            if (DiscordBot.Queue.Count > 0)
                                            {
                                                _responseMessage = _responseMessage + Convert.ToBase64String(AESProvider.IV) + "§";
                                                int count = DiscordBot.Queue.Count;
                                                for (int i = 0; i < count; i++)
                                                {
                                                    _responseMessage = _responseMessage + DiscordBot.Queue[0] + "σ";
                                                    DiscordBot.Queue.RemoveAt(0);
                                                }
                                                _responseMessage = _responseMessage.TrimEnd('σ');
                                            }
                                            else
                                            {
                                                _responseMessage += Convert.ToBase64String(AESProvider.IV);
                                            }
                                            _response.StatusCode = 200;
                                        }
                                        else
                                        {
                                            AuthorizedIvKey.Remove(_clientId);
                                            _response.StatusCode = 403;
                                        }
                                    }
                                    else if (POSTFollowUp[_clientId] == "DiscordPost")
                                    {
                                        POSTFollowUp.Remove(_clientId);
                                        string[] decryted = DiscordMessageDecrypt(_postMessage.Substring(16)).Split('☼');
                                        int.TryParse(decryted[0], out int id);
                                        GameManager.Instance.ChatMessageServer(null, EChatType.Global, id, DiscordBot.Message_Color + decryted[2] + "[-]", DiscordBot.Prefix_Color + DiscordBot.Prefix + "[-] " + DiscordBot.Name_Color + decryted[1] + "[-]", false, null);
                                        AESProvider.GenerateIV();
                                        AuthorizedIvKey[_clientId] = new string[] { Convert.ToBase64String(AESProvider.IV) };
                                        _responseMessage += Convert.ToBase64String(AESProvider.IV);
                                        _response.StatusCode = 200;
                                    }
                                }
                                else
                                {
                                    _response.StatusCode = 409;
                                }
                            }
                            else if (_uri == "DiscordUpdate")
                            {
                                if (DiscordBot.Queue.Count > 0)
                                {
                                    if (AuthorizedIvKey.ContainsKey(_clientId))
                                    {
                                        AuthorizedIvKey.TryGetValue(_clientId, out string[] _IVKey);
                                        AESProvider.Key = DiscordBot.TokenBytes;
                                        AESProvider.IV = Convert.FromBase64String(_IVKey[0]);
                                        if (DiscordSecurityDecrypt(_postMessage.Substring(16)) == DiscordBot.TokenKey)
                                        {
                                            _responseMessage = _responseMessage + Convert.ToBase64String(AESProvider.IV) + "§";
                                            int count2 = DiscordBot.Queue.Count;
                                            for (int j = 0; j < count2; j++)
                                            {
                                                _responseMessage = _responseMessage + DiscordBot.Queue[0] + "σ";
                                                DiscordBot.Queue.RemoveAt(0);
                                            }
                                            _responseMessage = _responseMessage.TrimEnd('σ');
                                            _response.StatusCode = 200;
                                        }
                                        else
                                        {
                                            AuthorizedIvKey.Remove(_clientId);
                                            _response.StatusCode = 403;
                                        }
                                    }
                                }
                                else
                                {
                                    _response.StatusCode = 204;
                                }
                            }
                        }
                        if (WebPanel.IsEnabled)
                        {
                            if (_uri == "Handshake")
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
                                        _response.Redirect(Redirect);
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
                                            _response.Redirect(Redirect);
                                            _response.StatusCode = 200;
                                            WebPanel.Writer(string.Format("Client {0} at IP {1} has signed out", _IVKey[0], _ip));
                                        }
                                        else
                                        {
                                            _response.StatusCode = 403;
                                        }
                                    }
                                }
                                else
                                {
                                    _response.Redirect("http://" + BaseAddress + ":" + Port + "/st.html");
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
                                            _response.Redirect(Redirect);
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
                                        WebPanel.Writer(string.Format("Client {0} at IP {1} has set a new password", _IVKey[0], _ip));
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
                                            _response.Redirect(Redirect);
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
                                            _response.Redirect(Redirect);
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
                                                ClientInfo _cInfo = new ClientInfo
                                                {
                                                    playerId = "-Web_Panel- " + _IVKey[0],
                                                    entityId = -1
                                                };
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
                                                WebPanel.Writer(string.Format("Executed console command '{0}' from Client: {1} IP: {2}", _command, _IVKey[0], _ip));
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
                                            _response.Redirect(Redirect);
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
                                        _response.Redirect(Redirect);
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
                                            _response.Redirect(Redirect);
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
                                            _xmlDoc.Load(Config.ConfigFilePath);
                                        }
                                        catch (XmlException e)
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", Config.ConfigFilePath, e.Message));
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
                                                _xmlDoc.Save(Config.ConfigFilePath);
                                                WebPanel.Writer(string.Format("Client {0} at IP {1} has updated the Config ", _IVKey[0], _ip));
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
                                        _response.Redirect(Redirect);
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
                                                WebPanel.Writer(string.Format("Client {0} at IP {1} has kicked {2}", _clientId, _ip, _cInfo.playerId));
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
                                        _response.Redirect(Redirect);
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
                                                WebPanel.Writer(string.Format("Client {0} at IP {1} has banned id {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
                                                _response.StatusCode = 200;
                                            }
                                            else
                                            {
                                                PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_commandSplit[1]);
                                                if (_pdf != null)
                                                {
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 year", _pdf.ecd.belongsPlayerId), null);
                                                    WebPanel.Writer(string.Format("Client {0} at IP {1} has banned id {2} named {3}", _IVKey[0], _ip, _pdf.ecd.belongsPlayerId, _pdf.ecd.entityName));
                                                    _response.StatusCode = 406;
                                                }
                                                else
                                                {
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 year", _commandSplit[1]), null);
                                                    WebPanel.Writer(string.Format("Client {0} at IP {1} has banned id {2}", _IVKey[0], _ip, _commandSplit[1]));
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
                                        _response.Redirect(Redirect);
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
                                                WebPanel.Writer(string.Format("Client {0} at IP {1} has banned id {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
                                                _response.StatusCode = 200;
                                            }
                                            else
                                            {
                                                PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_commandSplit[1]);
                                                if (_pdf != null)
                                                {
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 year", _pdf.ecd.belongsPlayerId), null);
                                                    WebPanel.Writer(string.Format("Client {0} at IP {1} has banned id {2} named {3}", _IVKey[0], _ip, _pdf.ecd.belongsPlayerId, _pdf.ecd.entityName));
                                                    _response.StatusCode = 406;
                                                }
                                                else
                                                {
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 1 year", _commandSplit[1]), null);
                                                    WebPanel.Writer(string.Format("Client {0} at IP {1} has banned id {2}", _IVKey[0], _ip, _commandSplit[1]));
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
                                        _response.Redirect(Redirect);
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
                                                        WebPanel.Writer(string.Format("Client {0} at IP {1} has unmuted id {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
                                                        _response.StatusCode = 202;
                                                    }
                                                    else
                                                    {
                                                        Mute.Mutes.Add(_cInfo.playerId);
                                                        PersistentContainer.Instance.Players[_cInfo.playerId].MuteTime = -1;
                                                        PersistentContainer.Instance.Players[_cInfo.playerId].MuteName = _cInfo.playerName;
                                                        PersistentContainer.Instance.Players[_cInfo.playerId].MuteDate = DateTime.Now;
                                                        WebPanel.Writer(string.Format("Client {0} at IP {1} has muted id {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
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
                                                        WebPanel.Writer(string.Format("Client {0} at IP {1} has unmuted id {2}", _IVKey[0], _ip, _commandSplit[1]));
                                                        _response.StatusCode = 202;
                                                    }
                                                    else
                                                    {
                                                        Mute.Mutes.Add(_commandSplit[1]);
                                                        PersistentContainer.Instance.Players[_commandSplit[1]].MuteTime = -1;
                                                        PersistentContainer.Instance.Players[_commandSplit[1]].MuteName = "-Unknown-";
                                                        PersistentContainer.Instance.Players[_commandSplit[1]].MuteDate = DateTime.Now;
                                                        WebPanel.Writer(string.Format("Client {0} at IP {1} has muted id {2}", _IVKey[0], _ip, _commandSplit[1]));
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
                                        _response.Redirect(Redirect);
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
                                                            WebPanel.Writer(string.Format("Client {0} at IP {1} has unjailed {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
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
                                                        WebPanel.Writer(string.Format("Client {0} at IP {1} has jailed {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
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
                                                        WebPanel.Writer(string.Format("Client {0} at IP {1} has unjailed {2}", _IVKey[0], _ip, _commandSplit[1]));
                                                    }
                                                    else if (Jail.Jail_Position != "")
                                                    {
                                                        Jail.Jailed.Add(_commandSplit[1]);
                                                        PersistentContainer.Instance.Players[_commandSplit[1]].JailTime = -1;
                                                        PersistentContainer.Instance.Players[_commandSplit[1]].JailName = "-Unknown-";
                                                        PersistentContainer.Instance.Players[_commandSplit[1]].JailDate = DateTime.Now;
                                                        WebPanel.Writer(string.Format("Client {0} at IP {1} has jailed {2}", _IVKey[0], _ip, _commandSplit[1]));
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
                                        _response.Redirect(Redirect);
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
                                                    WebPanel.Writer(string.Format("Client {0} at IP {1} has rewarded {2} named {3}", _IVKey[0], _ip, _cInfo.playerId, _cInfo.playerName));
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
                        }
                        byte[] _c = Encoding.UTF8.GetBytes(_responseMessage);
                        if (_c != null)
                        {
                            _response.SendChunked = false;
                            _response.ProtocolVersion = HttpVersion;
                            _response.KeepAlive = false;
                            _response.AddHeader("Keep-Alive", "timeout=300, max=100");
                            _response.ContentLength64 = (long)_c.Length;
                            _response.ContentEncoding = Encoding.UTF8;
                            _response.ContentType = "text/html; charset=utf-8";
                            using (Stream output = _response.OutputStream)
                            {
                                output.Write(_c, 0, _c.Length);
                            }
                        }
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.POST: {0}", e.Message));
                if (IsEnabled)
                {
                    Start();
                }
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

        //private static string DiscordEncrypt(string _target)
        //{
        //    string result = "";
        //    try
        //    {
        //        ICryptoTransform cryptoTransform = WebAPI.AESProvider.CreateEncryptor();
        //        byte[] array = Convert.FromBase64String(_target);
        //        result = Convert.ToBase64String(cryptoTransform.TransformFinalBlock(array, 0, array.Length));
        //        cryptoTransform.Dispose();
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.DiscordEncrypt: {0}", e.Message));
        //    }
        //    return result;
        //}

        private static string DiscordSecurityDecrypt(string _target)
        {
            string result = "";
            try
            {
                ICryptoTransform cryptoTransform = WebAPI.AESProvider.CreateDecryptor();
                byte[] array = Convert.FromBase64String(_target);
                result = Convert.ToBase64String(cryptoTransform.TransformFinalBlock(array, 0, array.Length));
                cryptoTransform.Dispose();
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.DiscordDecrypt: {0}", e.Message));
            }
            return result;
        }

        private static string DiscordMessageDecrypt(string _target)
        {
            string result = "";
            try
            {
                ICryptoTransform cryptoTransform = WebAPI.AESProvider.CreateDecryptor();
                byte[] array = Convert.FromBase64String(_target);
                byte[] bytes = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
                result = Encoding.UTF8.GetString(bytes);
                cryptoTransform.Dispose();
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.DiscordDecrypt: {0}", e.Message));
            }
            return result;
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.Expired: {0}", e.Message));
            }
            return false;
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.PanelDecrypt: {0}", e.Message));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.PanelEncrypt: {0}", e.Message));
            }
            return _encrypted;
        }
    }
}
