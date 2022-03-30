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
        public static Dictionary<string, string[]> AuthorizedIvKey = new Dictionary<string, string[]>();
        public static Dictionary<string, DateTime> AuthorizedTime = new Dictionary<string, DateTime>();
        public static Dictionary<string, string[]> Visitor = new Dictionary<string, string[]>();
        public static Dictionary<string, string> PassThrough = new Dictionary<string, string>();
        public static Dictionary<string, int> PageHits = new Dictionary<string, int>();
        public static Dictionary<string, int> LoginAttempts = new Dictionary<string, int>();
        public static Dictionary<string, DateTime> TimeOut = new Dictionary<string, DateTime>();
        public static List<string> Ban = new List<string>();

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
                string ip = GamePrefs.GetString(EnumGamePrefs.ServerIP);
                if (!string.IsNullOrEmpty(ip))
                {
                    BaseAddress = ip;
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
                Dictionary<string, DateTime> timeoutList = PersistentContainer.Instance.WebTimeoutList;
                if (timeoutList != null && timeoutList.Count > 0)
                {
                    for (int i = 0; i < timeoutList.Count; i++)
                    {
                        KeyValuePair<string, DateTime> expires = timeoutList.ElementAt(i);
                        TimeSpan varTime = DateTime.Now - expires.Value;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int timepassed = (int)fractionalMinutes;
                        if (timepassed >= 10)
                        {
                            timeoutList.Remove(expires.Key);
                        }
                    }
                    TimeOut = timeoutList;
                }
                List<string> banList = PersistentContainer.Instance.WebBanList;
                if (banList != null && banList.Count > 0)
                {
                    Ban = banList;
                }
                Dictionary<string, DateTime> authorizedTimeList = PersistentContainer.Instance.WebAuthorizedTimeList;
                Dictionary<string, string[]> authorizedIVKeyList = PersistentContainer.Instance.WebAuthorizedIVKeyList;
                if (authorizedTimeList != null && authorizedIVKeyList != null && authorizedTimeList.Count > 0 && authorizedIVKeyList.Count > 0)
                {
                    for (int i = 0; i < authorizedTimeList.Count; i++)
                    {
                        KeyValuePair<string, DateTime> expires = authorizedTimeList.ElementAt(i);
                        TimeSpan varTime = DateTime.Now - expires.Value;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int timepassed = (int)fractionalMinutes;
                        if (timepassed >= 30)
                        {
                            authorizedTimeList.Remove(expires.Key);
                            authorizedIVKeyList.Remove(expires.Key);
                        }
                    }
                    AuthorizedTime = authorizedTimeList;
                    AuthorizedIvKey = authorizedIVKeyList;
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
                int controlPanelPort = GamePrefs.GetInt(EnumGamePrefs.ControlPanelPort);
                int telnetPanelPort = GamePrefs.GetInt(EnumGamePrefs.TelnetPort);
                if (Port == controlPanelPort || Port == telnetPanelPort)
                {
                    Log.Out("[SERVERTOOLS] Web_API port was set identically to the server control panel or telnet port. " +
                        "You must use a unique and unused port that is open to transmission. Web_API has been disabled. " +
                        "This means Discordian and the Web_Panel are also disabled");
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
                                HttpListenerContext context = Listener.GetContext();
                                if (context != null)
                                {
                                    bool Allowed = true;
                                    HttpListenerRequest request = context.Request;
                                    using (HttpListenerResponse response = context.Response)
                                    {
                                        response.StatusCode = 403;
                                        string ip = request.RemoteEndPoint.Address.ToString();
                                        string uri = request.Url.AbsoluteUri;
                                        if (Ban.Contains(ip))
                                        {
                                            WebPanel.Writer(string.Format("Request denied for banned IP: {0}", ip));
                                            Allowed = false;
                                        }
                                        else if (TimeOut.ContainsKey(ip))
                                        {
                                            TimeOut.TryGetValue(ip, out DateTime timeout);
                                            if (DateTime.Now >= timeout)
                                            {
                                                TimeOut.Remove(ip);
                                                PersistentContainer.Instance.WebTimeoutList.Remove(ip);
                                                PersistentContainer.DataChange = true;
                                            }
                                            else
                                            {
                                                WebPanel.Writer(string.Format("Request denied for IP '{0}' on timeout until '{1}'", ip, timeout));
                                                Allowed = false;
                                            }
                                        }
                                        if (uri.Length > 111)
                                        {
                                            WebPanel.Writer(string.Format("URI request was too long. Request denied for IP '{0}'", ip));
                                            Allowed = false;
                                            response.StatusCode = 414;
                                        }
                                        else if (uri.Contains("script") && !uri.Contains("JS/scripts.js"))
                                        {
                                            if (!request.IsLocal)
                                            {
                                                if (!Ban.Contains(ip))
                                                {
                                                    if (TimeOut.ContainsKey(ip))
                                                    {
                                                        TimeOut.Remove(ip);
                                                    }
                                                    if (PersistentContainer.Instance.WebTimeoutList != null && PersistentContainer.Instance.WebTimeoutList.ContainsKey(ip))
                                                    {
                                                        PersistentContainer.Instance.WebTimeoutList.Remove(ip);
                                                    }
                                                    Ban.Add(ip);
                                                    if (PersistentContainer.Instance.WebBanList != null)
                                                    {
                                                        PersistentContainer.Instance.WebBanList.Add(ip);
                                                    }
                                                    else
                                                    {
                                                        List<string> bannedIP = new List<string>();
                                                        bannedIP.Add(ip);
                                                        PersistentContainer.Instance.WebBanList = bannedIP;
                                                    }
                                                    PersistentContainer.DataChange = true;
                                                }
                                                WebPanel.Writer(string.Format("Banned IP '{0}'. Detected attempting to run a script against the server", ip));
                                                Allowed = false;
                                            }
                                            else
                                            {
                                                WebPanel.Writer(string.Format("Local IP '{0}'. Detected attempting to run a script against the server", ip));
                                                Allowed = false;
                                            }
                                        }
                                        if (Allowed)
                                        {
                                            IsAllowed(request, response, ip, uri);
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
                        if (WebPanel.IsEnabled || BlackJack.IsEnabled)
                        {
                            _uri = _uri.Remove(0, _uri.IndexOf(Port.ToString()) + Port.ToString().Length + 1);
                            if (_uri.Contains("st.html"))
                            {
                                if (_uri.EndsWith("/"))
                                {
                                    _response.Redirect("http://" + BaseAddress + ":" + Port + "/st.html");
                                    _response.StatusCode = 308;
                                }
                                else if (PageHits.ContainsKey(_ip))
                                {
                                    PageHits[_ip] += 1;
                                    if (PageHits[_ip] >= 6)
                                    {
                                        PageHits.Remove(_ip);
                                        if (!_request.IsLocal)
                                        {
                                            TimeOut.Add(_ip, DateTime.Now.AddMinutes(5));
                                            if (PersistentContainer.Instance.WebTimeoutList != null)
                                            {
                                                PersistentContainer.Instance.WebTimeoutList.Add(_ip, DateTime.Now.AddMinutes(5));
                                            }
                                            else
                                            {
                                                Dictionary<string, DateTime> timeouts = new Dictionary<string, DateTime>();
                                                timeouts.Add(_ip, DateTime.Now.AddMinutes(5));
                                                PersistentContainer.Instance.WebTimeoutList = timeouts;
                                            }
                                            PersistentContainer.DataChange = true;
                                            WebPanel.Writer(string.Format("Homepage request denied for IP '{0}'. Client is now in time out for five minutes", _ip));
                                        }
                                    }
                                    else
                                    {
                                        GET(_response, Directory + _uri, _ip);
                                        WebPanel.Writer(string.Format("Homepage request granted for IP '{0}'", _ip));
                                    }
                                }
                                else
                                {
                                    PageHits.Add(_ip, 1);
                                    GET(_response, Directory + _uri, _ip);
                                    WebPanel.Writer(string.Format("Homepage request granted for IP '{0}'", _ip));
                                }
                            }
                            else if (_uri.Contains("blackJack.html") && BlackJack.Player.ContainsKey(_ip))
                            {
                                if (PageHits.ContainsKey(_ip))
                                {
                                    PageHits[_ip] += 1;
                                    if (PageHits[_ip] >= 3)
                                    {
                                        TimeOut.Add(_ip, DateTime.Now.AddMinutes(5));
                                        if (PersistentContainer.Instance.WebTimeoutList != null)
                                        {
                                            PersistentContainer.Instance.WebTimeoutList.Add(_ip, DateTime.Now.AddMinutes(5));
                                        }
                                        else
                                        {
                                            Dictionary<string, DateTime> timeouts = new Dictionary<string, DateTime>();
                                            timeouts.Add(_ip, DateTime.Now.AddMinutes(5));
                                            PersistentContainer.Instance.WebTimeoutList = timeouts;
                                        }
                                        PersistentContainer.DataChange = true;
                                        WebPanel.Writer(string.Format("Black jack request denied for IP '{0}'. Client is now in time out for five minutes", _ip));
                                    }
                                    else
                                    {
                                        GET(_response, Directory + _uri, _ip);
                                        WebPanel.Writer(string.Format("Black jack request granted for IP '{0}'", _ip));
                                    }
                                }
                                else
                                {
                                    PageHits.Add(_ip, 1);
                                    GET(_response, Directory + _uri, _ip);
                                    WebPanel.Writer(string.Format("Black jack request granted for IP '{0}'", _ip));
                                }
                            }
                            else if (_uri == "CSS/styles.css" || _uri == "Font/7DaysLater.woff2" || _uri == "JS/aes.js" || _uri == "JS/cipher-core.js" ||
                                _uri == "JS/core.js" || _uri == "JS/crypto-js.js" || _uri == "JS/enc-base64.js" || _uri == "JS/enc-utf16.js" || _uri == "JS/evpkdf.js" || 
                                _uri == "JS/lib-typedarrays.js" || _uri == "JS/md5.js" || _uri == "JS/pad-pkcs7.js" || _uri == "JS/scripts.js" || _uri == "JS/x64-core.js" || 
                                _uri == "Img/BloodBorder.webp" || _uri == "Img/Lock.webp" || _uri == "Img/STLogo.webp" || _uri == "Img/UserIcon.webp" || 
                                _uri == "Img/ZombieDevs.webp" || _uri == "favicon.ico" || _uri == "JS/blackJackScripts.js" || _uri == "CSS/blackJackStyles.js")
                            {
                                GET(_response, Directory + _uri, _ip);
                            }
                            else if (_uri == "Config" && PassThrough.ContainsKey(_ip) && PassThrough[_ip] == "Config")
                            {
                                PassThrough.Remove(_ip);
                                _uri = API.ConfigPath + "/ServerToolsConfig.xml";
                                GET(_response, _uri, _ip);
                            }
                        }
                    }
                    else if (_request.HttpMethod == "POST" && (WebPanel.IsEnabled || DiscordBot.IsEnabled))
                    {
                        if (_request.HasEntityBody)
                        {
                            _uri = _uri.Remove(0, _uri.IndexOf(Port.ToString()) + Port.ToString().Length + 1);
                            POST(_request, _response, _uri, _ip);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.IsAllowed: {0}", e.Message));
            }
        }

        private static void GET(HttpListenerResponse _response, string _uri, string _ip)
        {
            try
            {
                using (_response)
                {
                    FileInfo fileInfo = new FileInfo(_uri);
                    if (fileInfo != null && fileInfo.Exists)
                    {
                        byte[] c = File.ReadAllBytes(_uri);
                        if (c != null)
                        {
                            _response.StatusCode = 200;
                            _response.SendChunked = false;
                            _response.ProtocolVersion = HttpVersion;
                            _response.KeepAlive = true;
                            _response.AddHeader("Keep-Alive", "timeout=300, max=100");
                            _response.ContentType = MimeType.GetMimeType(Path.GetExtension(fileInfo.Extension));
                            _response.ContentLength64 = (long)c.Length;
                            _response.ContentEncoding = Encoding.UTF8;
                            _response.OutputStream.Write(c, 0, c.Length);
                        }
                        else
                        {
                            _response.StatusCode = 404;
                            WebPanel.Writer(string.Format("Requested file was found but unable to form from uri '{0}", _uri));
                        }
                    }
                    else
                    {
                        _response.StatusCode = 404;
                        WebPanel.Writer(string.Format("Received get request for missing file at '{0}' from IP '{1}'", _uri, _ip));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.GET: {0}", e.Message));
            }
        }

        private static void POST(HttpListenerRequest _request, HttpListenerResponse _response, string _uri, string _ip)
        {
            try
            {
                using (_response)
                {
                    if (_request.InputStream != null)
                    {
                        using (Stream body = _request.InputStream)
                        {
                            if (DiscordBot.IsEnabled && (_uri == "DiscordHandShake" || _uri == "DiscordPost" || _uri == "Discord_Sync"))
                            {
                                switch (_uri)
                                {
                                    case "DiscordHandShake":
                                        string postMessage = "";
                                        using (StreamReader read = new StreamReader(body, Encoding.UTF8))
                                        {
                                            postMessage = read.ReadToEnd();
                                        }
                                        if (!PassThrough.ContainsKey(_ip))
                                        {
                                            if (!string.IsNullOrEmpty(postMessage))
                                            {
                                                if (postMessage.Length == 16 && !PersistentContainer.Instance.Connections.ContainsKey(postMessage))
                                                {
                                                    using (Aes aes = Aes.Create())
                                                    {
                                                        aes.BlockSize = 128;
                                                        aes.KeySize = 256;
                                                        aes.Mode = CipherMode.CBC;
                                                        aes.Padding = PaddingMode.PKCS7;
                                                        aes.GenerateIV();
                                                        byte[] iv = aes.IV;
                                                        if (iv != null)
                                                        {
                                                            PersistentContainer.Instance.Connections.Add(postMessage, iv);
                                                            PersistentContainer.Instance.ConnectionTimeOut.Add(postMessage, DateTime.Now.AddMinutes(1));
                                                            PersistentContainer.DataChange = true;
                                                            _response.StatusCode = 200;
                                                            _response.SendChunked = false;
                                                            _response.ProtocolVersion = HttpVersion;
                                                            _response.KeepAlive = true;
                                                            _response.AddHeader("Keep-Alive", "timeout=300, max=100");
                                                            _response.ContentLength64 = (long)iv.Length;
                                                            _response.ContentType = "application/octet-stream";
                                                            using (Stream output = _response.OutputStream)
                                                            {
                                                                output.Write(iv, 0, iv.Length);
                                                            }
                                                        }
                                                    }
                                                    PassThrough.Add(_ip, postMessage);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            PassThrough.TryGetValue(_ip, out string clientId);
                                            PassThrough.Remove(_ip);
                                            PersistentContainer.Instance.Connections.TryGetValue(clientId, out byte[] iv);
                                            string decrypted = DiscordDecrypt(postMessage, iv);
                                            if (decrypted == DiscordBot.TokenKey)
                                            {
                                                using (Aes aes = Aes.Create())
                                                {
                                                    aes.BlockSize = 128;
                                                    aes.KeySize = 256;
                                                    aes.Mode = CipherMode.CBC;
                                                    aes.Padding = PaddingMode.PKCS7;
                                                    aes.GenerateIV();
                                                    iv = aes.IV;
                                                    if (iv != null)
                                                    {
                                                        PersistentContainer.Instance.Connections[clientId] = iv;
                                                        PersistentContainer.Instance.ConnectionTimeOut[clientId] = DateTime.Now.AddMinutes(5);
                                                        PersistentContainer.DataChange = true;
                                                        _response.StatusCode = 200;
                                                        _response.SendChunked = false;
                                                        _response.ProtocolVersion = HttpVersion;
                                                        _response.KeepAlive = true;
                                                        _response.AddHeader("Keep-Alive", "timeout=300, max=100");
                                                        _response.ContentLength64 = (long)iv.Length;
                                                        _response.ContentType = "application/octet-stream";
                                                        using (Stream output = _response.OutputStream)
                                                        {
                                                            output.Write(iv, 0, iv.Length);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                PersistentContainer.Instance.Connections.Remove(clientId);
                                                PersistentContainer.Instance.ConnectionTimeOut.Remove(clientId);
                                                PersistentContainer.DataChange = true;
                                            }
                                        }
                                        break;
                                    case "DiscordPost":
                                        if (!PassThrough.ContainsKey(_ip))
                                        {
                                            using (StreamReader read = new StreamReader(body, Encoding.UTF8))
                                            {
                                                postMessage = read.ReadToEnd();
                                            }
                                            if (!string.IsNullOrEmpty(postMessage) && postMessage.Length > 16)
                                            {
                                                string clientId = postMessage.Substring(0, 16);
                                                if (PersistentContainer.Instance.Connections.ContainsKey(clientId))
                                                {
                                                    PersistentContainer.Instance.Connections.TryGetValue(clientId, out byte[] iv);
                                                    string decrypted = DiscordDecrypt(postMessage.Substring(16), iv);
                                                    if (decrypted == DiscordBot.TokenKey)
                                                    {
                                                        using (Aes aes = Aes.Create())
                                                        {
                                                            aes.BlockSize = 128;
                                                            aes.KeySize = 256;
                                                            aes.Mode = CipherMode.CBC;
                                                            aes.Padding = PaddingMode.PKCS7;
                                                            aes.GenerateIV();
                                                            iv = aes.IV;
                                                            if (iv != null)
                                                            {
                                                                PersistentContainer.Instance.Connections[clientId] = iv;
                                                                PersistentContainer.Instance.ConnectionTimeOut[clientId] = DateTime.Now.AddMinutes(1);
                                                                PersistentContainer.DataChange = true;
                                                                _response.StatusCode = 200;
                                                                _response.SendChunked = false;
                                                                _response.ProtocolVersion = HttpVersion;
                                                                _response.KeepAlive = true;
                                                                _response.AddHeader("Keep-Alive", "timeout=300, max=100");
                                                                _response.ContentLength64 = (long)iv.Length;
                                                                _response.ContentType = "application/octet-stream";
                                                                using (Stream output = _response.OutputStream)
                                                                {
                                                                    output.Write(iv, 0, iv.Length);
                                                                }
                                                            }
                                                        }
                                                        PassThrough.Add(_ip, postMessage);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            PassThrough.TryGetValue(_ip, out string clientId);
                                            PassThrough.Remove(_ip);
                                            PersistentContainer.Instance.ConnectionTimeOut[clientId] = DateTime.Now.AddMinutes(5);
                                            PersistentContainer.DataChange = true;
                                            using (StreamReader read = new StreamReader(body, Encoding.UTF32))
                                            {
                                                postMessage = read.ReadToEnd();
                                            }
                                            if (!string.IsNullOrEmpty(postMessage))
                                            {
                                                string[] message = postMessage.Split('☼');
                                                if (int.TryParse(message[0], out int id))
                                                {
                                                    GameManager.Instance.ChatMessageServer(null, EChatType.Global, id, DiscordBot.Message_Color + message[2] + "[-]", DiscordBot.Prefix_Color + DiscordBot.Prefix + "[-] " + DiscordBot.Name_Color + message[1] + "[-]", false, null);
                                                }
                                            }
                                        }
                                        break;
                                    case "Discord_Sync":
                                        using (StreamReader read = new StreamReader(body, Encoding.UTF8))
                                        {
                                            postMessage = read.ReadToEnd();
                                        }
                                        if (!string.IsNullOrEmpty(postMessage) && postMessage.Length > 16)
                                        {
                                            if (PersistentContainer.Instance.Connections.ContainsKey(postMessage))
                                            {
                                                string clientId = postMessage.Substring(0, 16);
                                                if (PersistentContainer.Instance.Connections.ContainsKey(clientId))
                                                {
                                                    PersistentContainer.Instance.Connections.TryGetValue(clientId, out byte[] iv);
                                                    string decrypted = DiscordDecrypt(postMessage.Substring(16), iv);
                                                    if (decrypted == DiscordBot.TokenKey)
                                                    {
                                                        using (Aes aes = Aes.Create())
                                                        {
                                                            aes.BlockSize = 128;
                                                            aes.KeySize = 256;
                                                            aes.Mode = CipherMode.CBC;
                                                            aes.Padding = PaddingMode.PKCS7;
                                                            aes.GenerateIV();
                                                            iv = aes.IV;
                                                            if (iv != null)
                                                            {
                                                                PersistentContainer.Instance.Connections[clientId] = iv;
                                                                PersistentContainer.Instance.ConnectionTimeOut[clientId] = DateTime.Now.AddMinutes(5);
                                                                PersistentContainer.DataChange = true;
                                                                _response.StatusCode = 200;
                                                                _response.SendChunked = false;
                                                                _response.ProtocolVersion = HttpVersion;
                                                                _response.KeepAlive = true;
                                                                _response.AddHeader("Keep-Alive", "timeout=300, max=100");
                                                                _response.ContentLength64 = (long)iv.Length;
                                                                _response.ContentType = "application/octet-stream";
                                                                using (Stream output = _response.OutputStream)
                                                                {
                                                                    output.Write(iv, 0, iv.Length);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                            else if (_uri == "Handshake" || _uri == "SignIn" || _uri == "SignOut" || _uri == "NewPass" || _uri == "Console" ||
                            _uri == "Command" || _uri == "Players" || _uri == "Config" || _uri == "SaveConfig" || _uri == "Kick" ||
                            _uri == "Ban" || _uri == "Mute" || _uri == "Jail" || _uri == "Reward")
                            {
                                if (WebPanel.IsEnabled)
                                {
                                    string responseMessage = "";
                                    string postMessage = "";
                                    using (StreamReader read = new StreamReader(body, Encoding.UTF8))
                                    {
                                        postMessage = read.ReadToEnd();
                                    }
                                    string clientId = postMessage.Substring(0, 16);
                                    switch (_uri)
                                    {
                                        case "Handshake":
                                            if (!Visitor.ContainsKey(postMessage))
                                            {
                                                Visitor.Add(postMessage, null);
                                                _response.StatusCode = 200;
                                            }
                                            else
                                            {
                                                _response.StatusCode = 401;
                                            }
                                            break;
                                        case "SignIn":
                                            if (Visitor.ContainsKey(clientId))
                                            {
                                                if (!PassThrough.ContainsKey(clientId))
                                                {
                                                    string login = postMessage.Substring(16);
                                                    if (!string.IsNullOrEmpty(PersistentContainer.Instance.Players[login].WebPass) && PersistentContainer.Instance.Players[login].WebPass != "")
                                                    {
                                                        string key = "";
                                                        string kChop = PersistentContainer.Instance.Players[login].WebPass;
                                                        if (kChop.Length >= 16)
                                                        {
                                                            key += kChop.Substring(0, 16);
                                                        }
                                                        else if (kChop.Length >= 8)
                                                        {
                                                            kChop = kChop.Substring(0, 8);
                                                            key += kChop + kChop;
                                                        }
                                                        else
                                                        {
                                                            kChop = kChop.Substring(0, 4);
                                                            key += kChop + kChop + kChop + kChop;
                                                        }
                                                        string iv = PersistentOperations.CreatePassword(16);
                                                        Visitor[clientId] = new string[] { login, key, iv };
                                                        PassThrough.Add(clientId, "SignIn");
                                                        responseMessage += iv;
                                                        _response.StatusCode = 200;
                                                    }
                                                }
                                                else if (PassThrough[clientId] == "SignIn")
                                                {
                                                    PassThrough.Remove(clientId);
                                                    Visitor.TryGetValue(clientId, out string[] IVKey);
                                                    string decrypted = PanelDecrypt(postMessage.Substring(16), Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                    if (decrypted == PersistentContainer.Instance.Players[IVKey[0]].WebPass)
                                                    {
                                                        Visitor.Remove(clientId);
                                                        string newIv = PersistentOperations.CreatePassword(16);
                                                        AuthorizedIvKey.Add(clientId, new string[] { IVKey[0], IVKey[1], newIv });
                                                        AuthorizedTime.Add(clientId, DateTime.Now);
                                                        responseMessage += newIv;
                                                        _response.StatusCode = 200;
                                                        WebPanel.Writer(string.Format("Client sign in success: {0} @ {1}", IVKey[0], _ip));
                                                    }
                                                    else
                                                    {
                                                        WebPanel.Writer(string.Format("Client sign in failed: {0} @ {1}", IVKey[0], _ip));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                _response.StatusCode = 401;
                                            }
                                            break;
                                        case "SignOut":
                                            if (AuthorizedIvKey.ContainsKey(clientId))
                                            {
                                                AuthorizedTime.TryGetValue(clientId, out DateTime expires);
                                                if (Expired(expires))
                                                {
                                                    AuthorizedIvKey.Remove(clientId);
                                                    AuthorizedTime.Remove(clientId);
                                                    _response.Redirect(Redirect);
                                                    _response.StatusCode = 401;
                                                }
                                                else
                                                {
                                                    AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                    string decrypted = PanelDecrypt(postMessage.Substring(16), Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                    if (decrypted == clientId)
                                                    {
                                                        AuthorizedIvKey.Remove(clientId);
                                                        AuthorizedTime.Remove(clientId);
                                                        _response.Redirect(Redirect);
                                                        _response.StatusCode = 200;
                                                        WebPanel.Writer(string.Format("Client {0} at IP {1} has signed out", IVKey[0], _ip));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                _response.Redirect(Panel_Address);
                                                _response.StatusCode = 401;
                                            }
                                            break;
                                        case "NewPass":
                                            if (AuthorizedIvKey.ContainsKey(clientId))
                                            {
                                                if (!PassThrough.ContainsKey(clientId))
                                                {
                                                    AuthorizedTime.TryGetValue(clientId, out DateTime expires);
                                                    if (Expired(expires))
                                                    {
                                                        AuthorizedIvKey.Remove(clientId);
                                                        AuthorizedTime.Remove(clientId);
                                                        _response.Redirect(Redirect);
                                                        _response.StatusCode = 401;
                                                    }
                                                    else
                                                    {
                                                        AuthorizedTime[clientId] = DateTime.Now;
                                                        AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                        string decrypted = PanelDecrypt(postMessage.Substring(16), Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                        if (decrypted == clientId)
                                                        {
                                                            string newIv = PersistentOperations.CreatePassword(16);
                                                            AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                            PassThrough.Add(clientId, "NewPass");
                                                            responseMessage += newIv;
                                                            _response.StatusCode = 200;
                                                        }
                                                    }
                                                }
                                                else if (PassThrough[clientId] == "NewPass")
                                                {
                                                    PassThrough.Remove(clientId);
                                                    AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                    string decrypted = PanelDecrypt(postMessage.Substring(16), Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                    PersistentContainer.Instance.Players[IVKey[0]].WebPass = decrypted;
                                                    PersistentContainer.DataChange = true;
                                                    string newIv = PersistentOperations.CreatePassword(16);
                                                    AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                    responseMessage += newIv;
                                                    _response.StatusCode = 200;
                                                    WebPanel.Writer(string.Format("Client {0} at IP {1} has set a new password", IVKey[0], _ip));
                                                }
                                            }
                                            else
                                            {
                                                _response.StatusCode = 401;
                                            }
                                            break;
                                        case "Console":
                                            if (AuthorizedIvKey.ContainsKey(clientId))
                                            {
                                                if (!PassThrough.ContainsKey(clientId))
                                                {
                                                    AuthorizedTime.TryGetValue(clientId, out DateTime expires);
                                                    if (Expired(expires))
                                                    {
                                                        AuthorizedIvKey.Remove(clientId);
                                                        AuthorizedTime.Remove(clientId);
                                                        _response.Redirect(Redirect);
                                                        _response.StatusCode = 401;
                                                    }
                                                    else
                                                    {
                                                        AuthorizedTime[clientId] = DateTime.Now;
                                                        AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                        string decrypted = PanelDecrypt(postMessage.Substring(16), Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                        if (decrypted == clientId)
                                                        {
                                                            string newIv = PersistentOperations.CreatePassword(16);
                                                            AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                            PassThrough.Add(clientId, "Console");
                                                            responseMessage += newIv;
                                                            _response.StatusCode = 200;
                                                        }
                                                    }
                                                }
                                                else if (PassThrough[clientId] == "Console")
                                                {
                                                    PassThrough.Remove(clientId);
                                                    int.TryParse(postMessage.Substring(16), out int _lineNumber);
                                                    int _logCount = OutputLog.ActiveLog.Count;
                                                    if (_logCount >= _lineNumber + 1)
                                                    {
                                                        for (int i = _lineNumber; i < _logCount; i++)
                                                        {
                                                            responseMessage += OutputLog.ActiveLog[i] + "\n";
                                                        }
                                                        responseMessage += "☼" + _logCount;
                                                    }
                                                    _response.StatusCode = 200;
                                                }
                                            }
                                            else
                                            {
                                                _response.StatusCode = 401;
                                            }
                                            break;
                                        case "Command":
                                            if (AuthorizedIvKey.ContainsKey(clientId))
                                            {
                                                if (!PassThrough.ContainsKey(clientId))
                                                {
                                                    AuthorizedTime.TryGetValue(clientId, out DateTime expires);
                                                    if (Expired(expires))
                                                    {
                                                        AuthorizedIvKey.Remove(clientId);
                                                        AuthorizedTime.Remove(clientId);
                                                        _response.Redirect(Redirect);
                                                        _response.StatusCode = 401;
                                                    }
                                                    else
                                                    {
                                                        AuthorizedTime[clientId] = DateTime.Now;
                                                        AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                        string decrypted = PanelDecrypt(postMessage.Substring(16), Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                        if (decrypted == clientId)
                                                        {
                                                            string newIv = PersistentOperations.CreatePassword(16);
                                                            AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                            PassThrough.Add(clientId, "Command");
                                                            responseMessage += newIv;
                                                            _response.StatusCode = 200;
                                                        }
                                                    }
                                                }
                                                else if (PassThrough[clientId] == "Command")
                                                {
                                                    PassThrough.Remove(clientId);
                                                    AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                    string[] idLineCountandCommand = postMessage.Split(new[] { '☼' }, 3);
                                                    string decrypted = PanelDecrypt(idLineCountandCommand[2], Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                    string newIv = PersistentOperations.CreatePassword(16);
                                                    AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                    string command = decrypted;
                                                    if (command.Length > 300)
                                                    {
                                                        responseMessage += newIv;
                                                        _response.StatusCode = 400;
                                                    }
                                                    else
                                                    {
                                                        IConsoleCommand commandValid = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(command, false);
                                                        if (commandValid == null)
                                                        {
                                                            responseMessage += newIv;
                                                            _response.StatusCode = 406;
                                                        }
                                                        else
                                                        {
                                                            ClientInfo cInfo = new ClientInfo
                                                            {
                                                                playerName = "-Web_Panel- " + IVKey[0],
                                                                entityId = -1
                                                            };
                                                            List<string> cmdReponse = SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(command, cInfo);
                                                            int logCount = OutputLog.ActiveLog.Count;
                                                            int.TryParse(idLineCountandCommand[1], out int lineNumber);
                                                            if (logCount >= lineNumber + 1)
                                                            {
                                                                for (int i = lineNumber; i < logCount; i++)
                                                                {
                                                                    responseMessage += OutputLog.ActiveLog[i] + "\n";
                                                                }
                                                            }
                                                            for (int i = 0; i < cmdReponse.Count; i++)
                                                            {
                                                                responseMessage += cmdReponse[i] + "\n";
                                                            }
                                                            responseMessage += "☼" + logCount + "☼" + newIv;
                                                            WebPanel.Writer(string.Format("Executed console command '{0}' from Client: {1} IP: {2}", command, IVKey[0], _ip));
                                                            _response.StatusCode = 200;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                _response.StatusCode = 401;
                                            }
                                            break;
                                        case "Players":
                                            if (AuthorizedIvKey.ContainsKey(clientId))
                                            {
                                                if (!PassThrough.ContainsKey(clientId))
                                                {
                                                    AuthorizedTime.TryGetValue(clientId, out DateTime expires);
                                                    if (Expired(expires))
                                                    {
                                                        AuthorizedIvKey.Remove(clientId);
                                                        AuthorizedTime.Remove(clientId);
                                                        _response.Redirect(Redirect);
                                                        _response.StatusCode = 401;
                                                    }
                                                    else
                                                    {
                                                        AuthorizedTime[clientId] = DateTime.Now;
                                                        AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                        string decrypted = PanelDecrypt(postMessage.Substring(16), Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                        if (decrypted == clientId)
                                                        {
                                                            string newIv = PersistentOperations.CreatePassword(16);
                                                            AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                            PassThrough.Add(clientId, "Players");
                                                            responseMessage += newIv;
                                                            _response.StatusCode = 200;
                                                        }
                                                    }
                                                }
                                                else if (PassThrough[clientId] == "Players")
                                                {
                                                    PassThrough.Remove(clientId);
                                                    List<ClientInfo> clientList = PersistentOperations.ClientList();
                                                    if (clientList != null)
                                                    {
                                                        int count = 0;
                                                        for (int i = 0; i < clientList.Count; i++)
                                                        {
                                                            ClientInfo cInfo = clientList[i];
                                                            if (cInfo != null)
                                                            {
                                                                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                                                                if (player != null && player.Progression != null)
                                                                {
                                                                    if (cInfo.playerName.Contains("☼") || cInfo.playerName.Contains("§") || cInfo.playerName.Contains("/"))
                                                                    {
                                                                        responseMessage += cInfo.PlatformId.CombinedString + "/" + cInfo.entityId + "§" + "<Invalid Chars>" + "§"
                                                                            + player.Health + "/" + (int)player.Stamina + "§" + player.Progression.Level + "§"
                                                                            + (int)player.position.x + "," + (int)player.position.y + "," + (int)player.position.z;
                                                                    }
                                                                    else
                                                                    {
                                                                        responseMessage += cInfo.PlatformId.CombinedString + "/" + cInfo.entityId + "§" + cInfo.playerName + "§"
                                                                            + player.Health + "/" + (int)player.Stamina + "§" + player.Progression.Level + "§"
                                                                            + (int)player.position.x + "," + (int)player.position.y + "," + (int)player.position.z;
                                                                    }
                                                                    if (Mute.IsEnabled && Mute.Mutes.Contains(cInfo.CrossplatformId.CombinedString))
                                                                    {
                                                                        responseMessage += "§" + "True";
                                                                    }
                                                                    else
                                                                    {
                                                                        responseMessage += "§" + "False";
                                                                    }
                                                                    if (Jail.IsEnabled && Jail.Jailed.Contains(cInfo.CrossplatformId.CombinedString))
                                                                    {
                                                                        responseMessage += "/" + "True" + "☼";
                                                                    }
                                                                    else
                                                                    {
                                                                        responseMessage += "/" + "False" + "☼";
                                                                    }
                                                                    count++;
                                                                }
                                                            }
                                                        }
                                                        if (responseMessage != "")
                                                        {
                                                            responseMessage = responseMessage.TrimEnd('☼');
                                                            AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                            string encrypted = PanelEncrypt(responseMessage, Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                            string decrypted = PanelDecrypt(encrypted, Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                            string newIv = PersistentOperations.CreatePassword(16);
                                                            AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                            responseMessage = encrypted + "☼" + newIv + "☼" + count;
                                                        }
                                                    }
                                                    _response.StatusCode = 200;
                                                }
                                            }
                                            break;
                                        case "Config":
                                            if (AuthorizedIvKey.ContainsKey(clientId))
                                            {
                                                AuthorizedTime.TryGetValue(clientId, out DateTime expires);
                                                if (Expired(expires))
                                                {
                                                    AuthorizedIvKey.Remove(clientId);
                                                    AuthorizedTime.Remove(clientId);
                                                    _response.Redirect(Redirect);
                                                    _response.StatusCode = 401;
                                                }
                                                else
                                                {
                                                    AuthorizedTime[clientId] = DateTime.Now;
                                                    AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                    string decrypted = PanelDecrypt(postMessage.Substring(16), Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                    if (decrypted == clientId)
                                                    {
                                                        string newIv = PersistentOperations.CreatePassword(16);
                                                        AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                        PassThrough.Add(_ip, "Config");
                                                        responseMessage += newIv;
                                                        _response.StatusCode = 200;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                _response.StatusCode = 401;
                                            }
                                            break;
                                        case "SaveConfig":
                                            if (AuthorizedIvKey.ContainsKey(clientId))
                                            {
                                                if (!PassThrough.ContainsKey(clientId))
                                                {
                                                    AuthorizedTime.TryGetValue(clientId, out DateTime expires);
                                                    if (Expired(expires))
                                                    {
                                                        AuthorizedIvKey.Remove(clientId);
                                                        AuthorizedTime.Remove(clientId);
                                                        _response.Redirect(Redirect);
                                                        _response.StatusCode = 401;
                                                    }
                                                    else
                                                    {
                                                        AuthorizedTime[clientId] = DateTime.Now;
                                                        AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                        string decrypted = PanelDecrypt(postMessage.Substring(16), Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                        if (decrypted == clientId)
                                                        {
                                                            string newIv = PersistentOperations.CreatePassword(16);
                                                            AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                            PassThrough.Add(clientId, "SaveConfig");
                                                            responseMessage += newIv;
                                                            _response.StatusCode = 200;
                                                        }
                                                    }
                                                }
                                                else if (PassThrough[clientId] == "SaveConfig")
                                                {
                                                    PassThrough.Remove(clientId);
                                                    XmlDocument xmlDoc = new XmlDocument();
                                                    try
                                                    {
                                                        xmlDoc.Load(Config.ConfigFilePath);
                                                    }
                                                    catch (XmlException e)
                                                    {
                                                        Log.Out(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", Config.ConfigFilePath, e.Message));
                                                    }
                                                    if (xmlDoc != null)
                                                    {
                                                        AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                        string decrypted = PanelDecrypt(postMessage.Substring(16), Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                        string newIv = PersistentOperations.CreatePassword(16);
                                                        AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                        responseMessage += newIv;
                                                        bool changed = false;
                                                        XmlNodeList _nodes = xmlDoc.GetElementsByTagName("Tool");
                                                        decrypted = decrypted.TrimEnd('☼');
                                                        string[] tools = decrypted.Split('☼');
                                                        for (int i = 0; i < tools.Length; i++)
                                                        {
                                                            XmlNode node = _nodes[i];
                                                            string[] nameAndOptions = tools[i].Split('§');
                                                            if (nameAndOptions[1].Contains("╚"))
                                                            {
                                                                string[] _options = nameAndOptions[1].Split('╚');
                                                                for (int j = 0; j < _options.Length; j++)
                                                                {
                                                                    string[] optionNameAndValue = _options[j].Split('σ');
                                                                    int nodePosition = j + 1;
                                                                    if (nameAndOptions[0] == node.Attributes[0].Value && optionNameAndValue[0] == node.Attributes[nodePosition].Name && optionNameAndValue[1] != node.Attributes[nodePosition].Value)
                                                                    {
                                                                        changed = true;
                                                                        node.Attributes[nodePosition].Value = optionNameAndValue[1];
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                string[] optionNameAndValue = nameAndOptions[1].Split('σ');
                                                                if (nameAndOptions[0] == node.Attributes[0].Value && optionNameAndValue[0] == node.Attributes[1].Name && optionNameAndValue[1] != node.Attributes[1].Value)
                                                                {
                                                                    changed = true;
                                                                    node.Attributes[1].Value = optionNameAndValue[1];
                                                                }
                                                            }
                                                        }
                                                        if (changed)
                                                        {
                                                            xmlDoc.Save(Config.ConfigFilePath);
                                                            WebPanel.Writer(string.Format("Client {0} at IP {1} has updated the Config ", IVKey[0], _ip));
                                                            _response.StatusCode = 200;
                                                        }
                                                        else
                                                        {
                                                            _response.StatusCode = 406;
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        case "Kick":
                                            if (AuthorizedIvKey.ContainsKey(clientId))
                                            {
                                                AuthorizedTime.TryGetValue(clientId, out DateTime expires);
                                                if (Expired(expires))
                                                {
                                                    AuthorizedIvKey.Remove(clientId);
                                                    AuthorizedTime.Remove(clientId);
                                                    _response.Redirect(Redirect);
                                                    _response.StatusCode = 401;
                                                }
                                                else
                                                {
                                                    AuthorizedTime[clientId] = DateTime.Now;
                                                    AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                    string[] commandSplit = postMessage.Substring(16).Split('☼');
                                                    string decrypted = PanelDecrypt(commandSplit[0], Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                    if (decrypted == clientId)
                                                    {
                                                        string newIv = PersistentOperations.CreatePassword(16);
                                                        AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                        responseMessage += newIv;
                                                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(commandSplit[1]);
                                                        if (cInfo != null)
                                                        {
                                                            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0}", cInfo.CrossplatformId.CombinedString), null);
                                                            WebPanel.Writer(string.Format("Client '{0}' at IP '{1}' has kicked '{2}' '{3}'", clientId, _ip, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString));
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
                                            break;
                                        case "Ban":
                                            if (AuthorizedIvKey.ContainsKey(clientId))
                                            {
                                                AuthorizedTime.TryGetValue(clientId, out DateTime expires);
                                                if (Expired(expires))
                                                {
                                                    AuthorizedIvKey.Remove(clientId);
                                                    AuthorizedTime.Remove(clientId);
                                                    _response.Redirect(Redirect);
                                                    _response.StatusCode = 401;
                                                }
                                                else
                                                {
                                                    AuthorizedTime[clientId] = DateTime.Now;
                                                    AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                    string[] commandSplit = postMessage.Substring(16).Split('☼');
                                                    string decrypted = PanelDecrypt(commandSplit[0], Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                    if (decrypted == clientId)
                                                    {
                                                        string newIv = PersistentOperations.CreatePassword(16);
                                                        AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                        responseMessage += newIv;
                                                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(commandSplit[1]);
                                                        if (cInfo != null)
                                                        {
                                                            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 1 year", cInfo.CrossplatformId.CombinedString), null);
                                                            WebPanel.Writer(string.Format("Client '{0}' at IP '{1}' has banned id '{2}' '{3}' named '{4}'", IVKey[0], _ip, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                                            _response.StatusCode = 200;
                                                        }
                                                        else
                                                        {
                                                            PersistentPlayerData ppd = PersistentOperations.GetPersistentPlayerDataFromId(commandSplit[1]);
                                                            if (ppd != null)
                                                            {
                                                                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 1 year", ppd.UserIdentifier.CombinedString), null);
                                                                WebPanel.Writer(string.Format("Client '{0}' at IP '{1}' has banned id '{2}' named '{3}'", IVKey[0], _ip, ppd.UserIdentifier.CombinedString, ppd.PlayerName));
                                                                _response.StatusCode = 406;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                _response.StatusCode = 401;
                                            }
                                            break;
                                        case "Mute":
                                            if (AuthorizedIvKey.ContainsKey(clientId))
                                            {
                                                AuthorizedTime.TryGetValue(clientId, out DateTime expires);
                                                if (Expired(expires))
                                                {
                                                    AuthorizedIvKey.Remove(clientId);
                                                    AuthorizedTime.Remove(clientId);
                                                    _response.Redirect(Redirect);
                                                    _response.StatusCode = 401;
                                                }
                                                else
                                                {
                                                    AuthorizedTime[clientId] = DateTime.Now;
                                                    AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                    string[] commandSplit = postMessage.Substring(16).Split('☼');
                                                    string decrypted = PanelDecrypt(commandSplit[0], Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                    if (decrypted == clientId)
                                                    {
                                                        string newIv = PersistentOperations.CreatePassword(16);
                                                        AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                        responseMessage += newIv;
                                                        if (Mute.IsEnabled)
                                                        {
                                                            ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(commandSplit[1]);
                                                            if (cInfo != null)
                                                            {
                                                                if (Mute.Mutes.Contains(cInfo.CrossplatformId.CombinedString))
                                                                {
                                                                    Mute.Mutes.Remove(cInfo.CrossplatformId.CombinedString);
                                                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteTime = 0;
                                                                    WebPanel.Writer(string.Format("Client {0} at IP {1} has unmuted id {2} named {3}", IVKey[0], _ip, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                                                    _response.StatusCode = 202;
                                                                }
                                                                else
                                                                {
                                                                    Mute.Mutes.Add(cInfo.CrossplatformId.CombinedString);
                                                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteTime = -1;
                                                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteName = cInfo.playerName;
                                                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteDate = DateTime.Now;
                                                                    WebPanel.Writer(string.Format("Client {0} at IP {1} has muted id {2} named {3}", IVKey[0], _ip, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                                                    _response.StatusCode = 200;
                                                                }
                                                                PersistentContainer.DataChange = true;
                                                            }
                                                            else
                                                            {
                                                                if (Mute.Mutes.Contains(commandSplit[1]))
                                                                {
                                                                    Mute.Mutes.Remove(commandSplit[1]);
                                                                    PersistentContainer.Instance.Players[commandSplit[1]].MuteTime = 0;
                                                                    WebPanel.Writer(string.Format("Client {0} at IP {1} has unmuted id {2}", IVKey[0], _ip, commandSplit[1]));
                                                                    _response.StatusCode = 202;
                                                                }
                                                                else
                                                                {
                                                                    Mute.Mutes.Add(commandSplit[1]);
                                                                    PersistentContainer.Instance.Players[commandSplit[1]].MuteTime = -1;
                                                                    PersistentContainer.Instance.Players[commandSplit[1]].MuteName = "-Unknown-";
                                                                    PersistentContainer.Instance.Players[commandSplit[1]].MuteDate = DateTime.Now;
                                                                    WebPanel.Writer(string.Format("Client {0} at IP {1} has muted id {2}", IVKey[0], _ip, commandSplit[1]));
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
                                                }
                                            }
                                            else
                                            {
                                                _response.StatusCode = 401;
                                            }
                                            break;
                                        case "Jail":
                                            if (AuthorizedIvKey.ContainsKey(clientId))
                                            {
                                                AuthorizedTime.TryGetValue(clientId, out DateTime expires);
                                                if (Expired(expires))
                                                {
                                                    AuthorizedIvKey.Remove(clientId);
                                                    AuthorizedTime.Remove(clientId);
                                                    _response.Redirect(Redirect);
                                                    _response.StatusCode = 401;
                                                }
                                                else
                                                {
                                                    AuthorizedTime[clientId] = DateTime.Now;
                                                    AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                    string[] commandSplit = postMessage.Substring(16).Split('☼');
                                                    string decrypted = PanelDecrypt(commandSplit[0], Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                    if (decrypted == clientId)
                                                    {
                                                        string newIv = PersistentOperations.CreatePassword(16);
                                                        AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], newIv };
                                                        responseMessage += newIv;
                                                        if (Jail.IsEnabled)
                                                        {
                                                            ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(commandSplit[1]);
                                                            if (cInfo != null)
                                                            {
                                                                if (Jail.Jailed.Contains(cInfo.CrossplatformId.CombinedString))
                                                                {
                                                                    EntityPlayer player = GameManager.Instance.World.Players.dict[cInfo.entityId];
                                                                    if (player != null)
                                                                    {
                                                                        EntityBedrollPositionList position = player.SpawnPoints;
                                                                        Jail.Jailed.Remove(cInfo.CrossplatformId.CombinedString);
                                                                        PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].JailTime = 0;
                                                                        PersistentContainer.DataChange = true;
                                                                        if (position != null && position.Count > 0)
                                                                        {
                                                                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(position[0].x, -1, position[0].z), null, false));
                                                                        }
                                                                        else
                                                                        {
                                                                            Vector3[] pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                                                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(pos[0].x, -1, pos[0].z), null, false));
                                                                        }
                                                                        WebPanel.Writer(string.Format("Client '{0}' at IP '{1}' has unjailed '{2}' '{3}' named '{4}'", IVKey[0], _ip, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                                                        _response.StatusCode = 200;
                                                                    }
                                                                }
                                                                else if (Jail.Jail_Position != "")
                                                                {
                                                                    EntityPlayer player = GameManager.Instance.World.Players.dict[cInfo.entityId];
                                                                    if (player != null && player.IsSpawned())
                                                                    {
                                                                        if (Jail.Jail_Position.Contains(","))
                                                                        {
                                                                            string[] cords = Jail.Jail_Position.Split(',');
                                                                            int.TryParse(cords[0], out int _x);
                                                                            int.TryParse(cords[1], out int _y);
                                                                            int.TryParse(cords[2], out int _z);
                                                                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                                                                        }
                                                                    }
                                                                    Jail.Jailed.Add(cInfo.CrossplatformId.CombinedString);
                                                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].JailTime = -1;
                                                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].JailName = cInfo.playerName;
                                                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].JailDate = DateTime.Now;
                                                                    PersistentContainer.DataChange = true;
                                                                    WebPanel.Writer(string.Format("Client '{0}' at IP '{1}' has jailed '{2}' '{3}' named '{4}'", IVKey[0], _ip, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                                                    _response.StatusCode = 200;
                                                                }
                                                                else
                                                                {
                                                                    _response.StatusCode = 409;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (Jail.Jailed.Contains(commandSplit[1]))
                                                                {
                                                                    Jail.Jailed.Remove(commandSplit[1]);
                                                                    PersistentContainer.Instance.Players[commandSplit[1]].JailTime = 0;
                                                                    WebPanel.Writer(string.Format("Client {0} at IP {1} has unjailed {2}", IVKey[0], _ip, commandSplit[1]));
                                                                }
                                                                else if (Jail.Jail_Position != "")
                                                                {
                                                                    Jail.Jailed.Add(commandSplit[1]);
                                                                    PersistentContainer.Instance.Players[commandSplit[1]].JailTime = -1;
                                                                    PersistentContainer.Instance.Players[commandSplit[1]].JailName = "-Unknown-";
                                                                    PersistentContainer.Instance.Players[commandSplit[1]].JailDate = DateTime.Now;
                                                                    WebPanel.Writer(string.Format("Client {0} at IP {1} has jailed {2}", IVKey[0], _ip, commandSplit[1]));
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
                                                        AuthorizedTime.Remove(clientId);
                                                        AuthorizedIvKey.Remove(clientId);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                _response.StatusCode = 401;
                                            }
                                            break;
                                        case "Reward":
                                            if (AuthorizedIvKey.ContainsKey(clientId))
                                            {
                                                AuthorizedTime.TryGetValue(clientId, out DateTime expires);
                                                if (Expired(expires))
                                                {
                                                    AuthorizedIvKey.Remove(clientId);
                                                    AuthorizedTime.Remove(clientId);
                                                    _response.Redirect(Redirect);
                                                    _response.StatusCode = 401;
                                                }
                                                else
                                                {
                                                    AuthorizedTime[clientId] = DateTime.Now;
                                                    AuthorizedIvKey.TryGetValue(clientId, out string[] IVKey);
                                                    string[] commandSplit = postMessage.Substring(16).Split('☼');
                                                    string decrypted = PanelDecrypt(commandSplit[0], Encoding.UTF8.GetBytes(IVKey[1]), Encoding.UTF8.GetBytes(IVKey[2]));
                                                    if (decrypted == clientId)
                                                    {
                                                        string _newIv = PersistentOperations.CreatePassword(16);
                                                        AuthorizedIvKey[clientId] = new string[] { IVKey[0], IVKey[1], _newIv };
                                                        responseMessage += _newIv;
                                                        if (VoteReward.IsEnabled)
                                                        {
                                                            ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(commandSplit[1]);
                                                            if (cInfo != null)
                                                            {
                                                                VoteReward.ItemOrBlockCounter(cInfo, VoteReward.Reward_Count);
                                                                WebPanel.Writer(string.Format("Client {0} at IP {1} has rewarded {2} named {3}", IVKey[0], _ip, cInfo.PlatformId.CombinedString, cInfo.playerName));
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
                                                }
                                            }
                                            else
                                            {
                                                _response.StatusCode = 401;
                                            }
                                            break;
                                    }
                                    byte[] c = Encoding.UTF8.GetBytes(responseMessage);
                                    if (c != null)
                                    {
                                        _response.SendChunked = false;
                                        _response.ProtocolVersion = HttpVersion;
                                        _response.KeepAlive = true;
                                        _response.AddHeader("Keep-Alive", "timeout=300, max=100");
                                        _response.ContentLength64 = (long)c.Length;
                                        _response.ContentEncoding = Encoding.UTF8;
                                        _response.ContentType = "text/html; charset=utf-8";
                                        using (Stream output = _response.OutputStream)
                                        {
                                            output.Write(c, 0, c.Length);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                WebPanel.Writer(string.Format("Detected {0} attempting to access an invalid address {1}", _ip, _uri));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.POST: {0}", e.Message));
            }
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

        private static string DiscordDecrypt(string _target, byte[] _iv)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.BlockSize = 128;
                    aes.KeySize = 256;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = DiscordBot.TokenBytes;
                    aes.IV = _iv;
                    byte[] array = Convert.FromBase64String(_target);
                    using (ICryptoTransform iCT = aes.CreateDecryptor())
                    {
                        _target = Convert.ToBase64String(iCT.TransformFinalBlock(array, 0, array.Length));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.DiscordDecrypt: {0}", e.Message));
            }
            return _target;
        }

        private static bool Expired(DateTime _expires)
        {
            try
            {
                TimeSpan varTime = DateTime.Now - _expires;
                double fractionalMinutes = varTime.TotalMinutes;
                int timepassed = (int)fractionalMinutes;
                if (timepassed >= 30)
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
            string decrypt = "";
            try
            {
                var encrypted = Convert.FromBase64String(_target);
                decrypt = DecryptStringFromBytes(encrypted, _key, _iv);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.PanelDecrypt: {0}", e.Message));
            }
            return decrypt;
        }

        private static string PanelEncrypt(string _target, byte[] _key, byte[] _iv)
        {
            try
            {
                byte[] encryptStringToBytes = EncryptStringToBytes(_target, _key, _iv);
                _target = Convert.ToBase64String(encryptStringToBytes);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.PanelEncrypt: {0}", e.Message));
            }
            return _target;
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            string decrypted = string.Empty;
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
    }
}
