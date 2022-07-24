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
        public static bool IsEnabled = false, IsRunning = false, Shutdown = false, Connected = false;
        public static int Port = 8084;
        public static string Directory = "", Panel_Address = "", Icon_Folder = ".../DedicatedServer/Data/ItemIcons";

        public static Dictionary<string, string> Authorized = new Dictionary<string, string>();
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

        private static readonly string file = string.Format("WebAPILog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string FilePath = string.Format("{0}/Logs/WebAPILogs/{1}", API.ConfigPath, file);

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
                else
                {
                    ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
                    if (!string.IsNullOrEmpty(ip))
                    {
                        BaseAddress = ip;
                        return true;
                    }
                    Writer(string.Format("The host ip could not be determined. Web_API and Discordian will not function without this"));
                    Log.Out(string.Format("[SERVERTOOLS] The host ip could not be determined. Web_API and Discordian will not function without this"));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WebAPI.SetBaseAddress: {0}", e.Message));
            }
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
                Dictionary<string, string> AuthorizedList = PersistentContainer.Instance.WebAuthorizedList;
                if (authorizedTimeList != null && AuthorizedList != null && authorizedTimeList.Count > 0 && AuthorizedList.Count > 0)
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
                            AuthorizedList.Remove(expires.Key);
                        }
                    }
                    AuthorizedTime = authorizedTimeList;
                    Authorized = AuthorizedList;
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
                        "This means the potential to use Discordian and the Web_Panel have been disabled");
                    return;
                }
                if (Port > 1000 && Port < 65536)
                {
                    if (HttpListener.IsSupported)
                    {
                        if (Listener != null && !Listener.IsListening)
                        {
                            Listener.Prefixes.Clear();
                            if (!Listener.Prefixes.Contains(string.Format("http://*:{0}/", Port)))
                            {
                                Listener.Prefixes.Add(string.Format("http://*:{0}/", Port));
                                Listener.Start();
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] ServerTools web panel was unable to connect due to the prefix already in us @ {0}", Panel_Address));
                            }
                        }
                        if (SetBaseAddress())
                        {
                            Redirect = "http://" + BaseAddress + ":" + Port;
                            Log.Out(string.Format("[SERVERTOOLS] ServerTools web api has opened @ {0}", Redirect));
                            Panel_Address = "http://" + BaseAddress + ":" + Port + "/st.html";
                            Connected = true;
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
                                            Writer(string.Format("Request denied for banned IP: {0}", ip));
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
                                                Writer(string.Format("Request denied for IP '{0}' on timeout until '{1}'", ip, timeout));
                                                Allowed = false;
                                            }
                                        }
                                        if (uri.Length > 111)
                                        {
                                            Writer(string.Format("URI request was too long. Request denied for IP '{0}'", ip));
                                            Allowed = false;
                                            response.StatusCode = 414;
                                        }
                                        else if (uri.Contains("script") && !uri.Contains("JS/scripts.js") && !uri.Contains("JS/shopscripts.js") &&
                                            !uri.Contains("JS/blackjackscripts.js") && !uri.Contains("JS/auctionscripts.js"))
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
                                            Writer(string.Format("Banned IP '{0}'. Detected attempting to run a script against the server", ip));
                                            Allowed = false;
                                        }
                                        if (Allowed)
                                        {
                                            IsAllowed(request, response, ip, uri);
                                        }
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
            Connected = false;
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
                        if (WebPanel.IsEnabled || BlackJack.IsEnabled || Shop.IsEnabled || Auction.IsEnabled)
                        {
                            if (_uri.EndsWith("/"))
                            {
                                _uri = _uri.Remove(_uri.Length - 1);
                            }
                            if (_uri.EndsWith("st.html") || _uri.EndsWith("blackJack.html") || _uri.EndsWith("shop.html") || _uri.EndsWith("auction.html"))
                            {
                                _uri = _uri.Remove(0, _uri.IndexOf(Port.ToString()) + Port.ToString().Length + 1);
                                if (PageHits.ContainsKey(_ip))
                                {
                                    PageHits[_ip] += 1;
                                    if (PageHits[_ip] >= 8)
                                    {
                                        PageHits.Remove(_ip);
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
                                        Writer(string.Format("Request denied for IP '{0}' to '{1}'. Client is now in time out for five minutes", _ip, _uri));
                                    }
                                    else
                                    {
                                        GET(_response, Directory + _uri, _ip);
                                        Writer(string.Format("Request granted for IP '{0}' to '{1}'", _ip, _uri));
                                    }
                                }
                                else
                                {
                                    PageHits.Add(_ip, 1);
                                    GET(_response, Directory + _uri, _ip);
                                    Writer(string.Format("Request granted for IP '{0}' to '{1}'", _ip, _uri));
                                }
                            }
                            else if ((_uri.Contains("CSS/") && _uri.EndsWith(".css")) ||
                                (_uri.Contains("Font/") && _uri.EndsWith(".woff2")) || (_uri.Contains("Font/") && _uri.EndsWith(".woff")) ||
                                (_uri.Contains("JS/") && _uri.EndsWith(".js")) ||
                                (_uri.Contains("Img/") && _uri.EndsWith(".webp")) || (_uri.Contains("Img/") && _uri.EndsWith(".png")) ||
                                _uri.EndsWith("favicon.ico"))
                            {
                                _uri = _uri.Remove(0, _uri.IndexOf(Port.ToString()) + Port.ToString().Length + 1);
                                GET(_response, Directory + _uri, _ip);
                            }
                            else if (_uri.EndsWith("Config") && PassThrough.ContainsKey(_ip) && PassThrough[_ip] == "Config")
                            {
                                _uri = _uri.Remove(0, _uri.IndexOf(Port.ToString()) + Port.ToString().Length + 1);
                                PassThrough.Remove(_ip);
                                _uri = API.ConfigPath + "/ServerToolsConfig.xml";
                                GET(_response, _uri, _ip);
                            }
                            else if (_uri.Contains("Icon/") && _uri.EndsWith(".png"))
                            {
                                _uri = _uri.Remove(0, _uri.IndexOf(Port.ToString()) + Port.ToString().Length + 1);
                                _uri = Icon_Folder + "/" + _uri.Replace("Icon/", "");
                                GET(_response, _uri, _ip);
                            }
                            else
                            {
                                Ban.Add(_ip);
                                if (PersistentContainer.Instance.WebBanList != null)
                                {
                                    PersistentContainer.Instance.WebBanList.Add(_ip);
                                }
                                else
                                {
                                    List<string> bannedIP = new List<string>();
                                    bannedIP.Add(_ip);
                                    PersistentContainer.Instance.WebBanList = bannedIP;
                                }
                                PersistentContainer.DataChange = true;
                                Writer(string.Format("Banned IP '{0}' for requesting invalid file '{1}'", _ip, _uri));
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
                            Writer(string.Format("Requested file was found but unable to form from uri '{0}", _uri));
                        }
                    }
                    else
                    {
                        _response.StatusCode = 404;
                        Writer(string.Format("Received get request for missing file at '{0}' from IP '{1}'", _uri, _ip));
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
                            if (DiscordBot.IsEnabled && (_uri == "DiscordHandShake" || _uri == "DiscordPost" || _uri == "DiscordSync"))
                            {
                                string responseMessage = "", postMessage = "";
                                using (StreamReader read = new StreamReader(body, Encoding.UTF8))
                                {
                                    postMessage = read.ReadToEnd();
                                }
                                switch (_uri)
                                {
                                    case "DiscordHandShake":
                                        if (postMessage.Contains('☼'))
                                        {
                                            string[] clientData = postMessage.Split('☼');
                                            if (!Authorized.ContainsKey(clientData[0]))
                                            {
                                                using (SHA512 sha512 = SHA512.Create())
                                                {
                                                    byte[] bytes = Encoding.UTF8.GetBytes(DiscordBot.TokenKey);
                                                    byte[] hashBytes = sha512.ComputeHash(bytes);
                                                    string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                    if (clientData[1] == keyHash)
                                                    {
                                                        for (int i = 0; i < 10; i++)
                                                        {
                                                            string salt = PersistentOperations.CreatePassword(4);
                                                            if (!Authorized.ContainsValue(DiscordBot.TokenKey + salt))
                                                            {
                                                                Authorized.Add(clientData[0], DiscordBot.TokenKey + salt);
                                                                AuthorizedTime.Add(clientData[0], DateTime.Now.AddMinutes(5));
                                                                responseMessage += salt;
                                                                _response.StatusCode = 200;
                                                                break;
                                                            }
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
                                        break;
                                    case "DiscordPost":
                                        if (postMessage.Contains('☼'))
                                        {
                                            string[] clientData = postMessage.Split('☼');
                                            if (Authorized.ContainsKey(clientData[0]))
                                            {
                                                Authorized.TryGetValue(clientData[0], out string pass);
                                                using (SHA512 sha512 = SHA512.Create())
                                                {
                                                    byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                    byte[] hashBytes = sha512.ComputeHash(bytes);
                                                    string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                    if (clientData[1] == keyHash)
                                                    {
                                                        for (int i = 0; i < 10; i++)
                                                        {
                                                            string salt = PersistentOperations.CreatePassword(4);
                                                            if (!Authorized.ContainsValue(pass + salt))
                                                            {
                                                                Authorized[clientData[0]] = pass + salt;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(5);
                                                                if (int.TryParse(clientData[2], out int id))
                                                                {
                                                                    GameManager.Instance.ChatMessageServer(null, EChatType.Global, id, DiscordBot.Message_Color + clientData[4] + "[-]", DiscordBot.Prefix_Color + DiscordBot.Prefix + "[-] " + DiscordBot.Name_Color + clientData[3] + "[-]", false, null);
                                                                }
                                                                responseMessage += salt;
                                                                _response.StatusCode = 200;
                                                                break;
                                                            }
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
                                        break;
                                    case "DiscordSync":
                                        if (postMessage.Contains('☼'))
                                        {
                                            string[] clientData = postMessage.Split('☼');
                                            if (Authorized.ContainsKey(clientData[0]))
                                            {
                                                Authorized.TryGetValue(clientData[0], out string pass);
                                                using (SHA512 sha512 = SHA512.Create())
                                                {
                                                    byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                    byte[] hashBytes = sha512.ComputeHash(bytes);
                                                    string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                    if (clientData[1] == keyHash)
                                                    {
                                                        for (int i = 0; i < 10; i++)
                                                        {
                                                            string salt = PersistentOperations.CreatePassword(4);
                                                            if (!Authorized.ContainsValue(pass + salt))
                                                            {
                                                                Authorized[clientData[0]] = pass + salt;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(5);
                                                                responseMessage += salt;
                                                                _response.StatusCode = 200;
                                                                break;
                                                            }
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
                            else if (_uri == "SignIn" || _uri == "SignOut" || _uri == "NewPass" || _uri == "Console" ||
                            _uri == "Command" || _uri == "Players" || _uri == "Config" || _uri == "SaveConfig" || _uri == "Kick" ||
                            _uri == "Ban" || _uri == "Mute" || _uri == "Jail" || _uri == "Reward" || _uri == "EnterShop" || _uri == "ExitShop" || 
                            _uri == "ShopPurchase" || _uri == "EnterAuction" || _uri == "ExitAuction" || _uri == "AuctionPurchase" || _uri == "AuctionCancel")
                            {
                                if (WebPanel.IsEnabled || (Shop.IsEnabled && Shop.Panel) || (Auction.IsEnabled && Auction.Panel))
                                {
                                    string responseMessage = "", postMessage = "";
                                    using (StreamReader read = new StreamReader(body, Encoding.UTF8))
                                    {
                                        postMessage = read.ReadToEnd();
                                    }
                                    switch (_uri)
                                    {
                                        case "SignIn":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] clientData = postMessage.Split('☼');
                                                if (PersistentContainer.Instance.Players[clientData[0]] != null &&
                                                    PersistentContainer.Instance.Players[clientData[0]].WebPass != null &&
                                                    PersistentContainer.Instance.Players[clientData[0]].WebPass != "")
                                                {
                                                    string pass = PersistentContainer.Instance.Players[clientData[0]].WebPass;
                                                    using (SHA512 sha512 = SHA512.Create())
                                                    {
                                                        byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                        byte[] hashBytes = sha512.ComputeHash(bytes);
                                                        string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                        if (clientData[1].ToUpper() == keyHash)
                                                        {
                                                            string salt = PersistentOperations.CreatePassword(4);
                                                            pass += salt;
                                                            if (!Authorized.ContainsKey(clientData[0]))
                                                            {
                                                                Authorized.Add(clientData[0], pass);
                                                                AuthorizedTime.Add(clientData[0], DateTime.Now.AddMinutes(WebPanel.Timeout));
                                                                responseMessage += salt;
                                                                _response.StatusCode = 200;
                                                            }
                                                            else
                                                            {
                                                                Authorized[clientData[0]] = pass;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(WebPanel.Timeout);
                                                                responseMessage += salt;
                                                                _response.StatusCode = 200;
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
                                            break;
                                        case "SignOut":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] clientData = postMessage.Split('☼');
                                                if (Authorized.ContainsKey(clientData[0]))
                                                {
                                                    AuthorizedTime.TryGetValue(clientData[0], out DateTime time);
                                                    if (DateTime.Now <= time)
                                                    {
                                                        Authorized.TryGetValue(clientData[0], out string pass);
                                                        using (SHA512 sha512 = SHA512.Create())
                                                        {
                                                            byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                            byte[] hashBytes = sha512.ComputeHash(bytes);
                                                            string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            if (clientData[1].ToUpper() == keyHash)
                                                            {
                                                                Authorized.Remove(clientData[0]);
                                                                AuthorizedTime.Remove(clientData[0]);
                                                                _response.Redirect(Panel_Address);
                                                                _response.StatusCode = 200;
                                                                Writer(string.Format("Client {0} at IP {1} has signed out", clientData[0], _ip));
                                                            }
                                                            else
                                                            {
                                                                _response.Redirect(Panel_Address);
                                                                _response.StatusCode = 401;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.Remove(clientData[0]);
                                                        AuthorizedTime.Remove(clientData[0]);
                                                        _response.Redirect(Panel_Address);
                                                        _response.StatusCode = 401;
                                                        Writer(string.Format("Client {0} has been logged out", clientData[0]));
                                                    }
                                                }
                                                else
                                                {
                                                    _response.Redirect(Panel_Address);
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            break;
                                        case "NewPass":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] clientData = postMessage.Split('☼');
                                                if (Authorized.ContainsKey(clientData[0]))
                                                {
                                                    AuthorizedTime.TryGetValue(clientData[0], out DateTime time);
                                                    if (DateTime.Now <= time)
                                                    {
                                                        Authorized.TryGetValue(clientData[0], out string pass);
                                                        using (SHA512 sha512 = SHA512.Create())
                                                        {
                                                            byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                            byte[] hashBytes = sha512.ComputeHash(bytes);
                                                            string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            if (clientData[1].ToUpper() == keyHash)
                                                            {
                                                                string passCut = pass.Substring(0, 4);
                                                                string newPass = DecryptStringAES(clientData[2], passCut + passCut + passCut + passCut,
                                                                    passCut + passCut + passCut + passCut);
                                                                PersistentContainer.Instance.Players[clientData[0]].WebPass = newPass;
                                                                PersistentContainer.DataChange = true;
                                                                string salt = PersistentOperations.CreatePassword(4);
                                                                newPass += salt;
                                                                Authorized[clientData[0]] = newPass;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(WebPanel.Timeout);
                                                                responseMessage += salt;
                                                                _response.StatusCode = 200;
                                                                Writer(string.Format("Client {0} at IP {1} has set a new password", clientData[0], _ip));
                                                            }
                                                            else
                                                            {
                                                                _response.Redirect(Panel_Address);
                                                                _response.StatusCode = 401;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.Remove(clientData[0]);
                                                        AuthorizedTime.Remove(clientData[0]);
                                                        _response.Redirect(Panel_Address);
                                                        _response.StatusCode = 401;
                                                        Writer(string.Format("Client {0} has been logged out", clientData[0]));
                                                    }
                                                }
                                                else
                                                {
                                                    _response.Redirect(Panel_Address);
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            break;
                                        case "Console":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] clientData = postMessage.Split('☼');
                                                if (Authorized.ContainsKey(clientData[0]))
                                                {
                                                    AuthorizedTime.TryGetValue(clientData[0], out DateTime time);
                                                    if (DateTime.Now <= time)
                                                    {
                                                        Authorized.TryGetValue(clientData[0], out string pass);
                                                        using (SHA512 sha512 = SHA512.Create())
                                                        {
                                                            byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                            byte[] hashBytes = sha512.ComputeHash(bytes);
                                                            string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            if (clientData[1].ToUpper() == keyHash)
                                                            {
                                                                string salt = PersistentOperations.CreatePassword(4);
                                                                pass += salt;
                                                                Authorized[clientData[0]] = pass;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(WebPanel.Timeout);
                                                                int.TryParse(clientData[2], out int lineNumber);
                                                                int logCount = OutputLog.ActiveLog.Count;
                                                                if (logCount >= lineNumber + 1)
                                                                {
                                                                    for (int i = lineNumber; i < logCount; i++)
                                                                    {
                                                                        responseMessage += OutputLog.ActiveLog[i] + "\n";
                                                                    }
                                                                    responseMessage += "☼" + logCount;
                                                                }
                                                                responseMessage += "☼" + salt;
                                                                _response.StatusCode = 200;
                                                            }
                                                            else
                                                            {
                                                                _response.Redirect(Panel_Address);
                                                                _response.StatusCode = 401;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.Remove(clientData[0]);
                                                        AuthorizedTime.Remove(clientData[0]);
                                                        _response.Redirect(Panel_Address);
                                                        _response.StatusCode = 401;
                                                        Writer(string.Format("Client {0} has been logged out", clientData[0]));
                                                    }
                                                }
                                                else
                                                {
                                                    _response.Redirect(Panel_Address);
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            break;
                                        case "Command":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] clientData = postMessage.Split('☼');
                                                if (Authorized.ContainsKey(clientData[0]))
                                                {
                                                    AuthorizedTime.TryGetValue(clientData[0], out DateTime time);
                                                    if (DateTime.Now <= time)
                                                    {
                                                        Authorized.TryGetValue(clientData[0], out string pass);
                                                        using (SHA512 sha512 = SHA512.Create())
                                                        {
                                                            byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                            byte[] hashBytes = sha512.ComputeHash(bytes);
                                                            string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            if (clientData[1].ToUpper() == keyHash)
                                                            {
                                                                string salt = PersistentOperations.CreatePassword(4);
                                                                pass += salt;
                                                                Authorized[clientData[0]] = pass;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(WebPanel.Timeout);
                                                                IConsoleCommand commandValid = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(clientData[3], false);
                                                                if (commandValid == null)
                                                                {
                                                                    responseMessage += salt;
                                                                    _response.StatusCode = 402;
                                                                }
                                                                else
                                                                {
                                                                    ClientInfo cInfo = new ClientInfo
                                                                    {
                                                                        playerName = "-Web_Panel- " + clientData[0],
                                                                        entityId = -1
                                                                    };
                                                                    List<string> cmdReponse = SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(clientData[3], cInfo);
                                                                    int logCount = OutputLog.ActiveLog.Count;
                                                                    int.TryParse(clientData[2], out int lineNumber);
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
                                                                    responseMessage += "☼" + logCount + "☼" + salt;
                                                                    Log.Out(string.Format("[SERVERTOOLS] Executed console command '{0}' from web panel client '{1}' at IP '{2}'", clientData[3], clientData[0], _ip));
                                                                    Writer(string.Format("Executed console command '{0}' from Client {1} at IP {2}", clientData[3], clientData[0], _ip));
                                                                    _response.StatusCode = 200;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                _response.Redirect(Panel_Address);
                                                                _response.StatusCode = 401;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.Remove(clientData[0]);
                                                        AuthorizedTime.Remove(clientData[0]);
                                                        _response.Redirect(Panel_Address);
                                                        _response.StatusCode = 401;
                                                        Writer(string.Format("Client {0} has been logged out", clientData[0]));
                                                    }
                                                }
                                                else
                                                {
                                                    _response.Redirect(Panel_Address);
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            break;
                                        case "Players":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] clientData = postMessage.Split('☼');
                                                if (Authorized.ContainsKey(clientData[0]))
                                                {
                                                    AuthorizedTime.TryGetValue(clientData[0], out DateTime time);
                                                    if (DateTime.Now <= time)
                                                    {
                                                        Authorized.TryGetValue(clientData[0], out string pass);
                                                        using (SHA512 sha512 = SHA512.Create())
                                                        {
                                                            byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                            byte[] hashBytes = sha512.ComputeHash(bytes);
                                                            string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            if (clientData[1].ToUpper() == keyHash)
                                                            {
                                                                string salt = PersistentOperations.CreatePassword(4);
                                                                pass += salt;
                                                                Authorized[clientData[0]] = pass;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(WebPanel.Timeout);
                                                                List<ClientInfo> clientList = PersistentOperations.ClientList();
                                                                if (clientList != null)
                                                                {
                                                                    for (int i = 0; i < clientList.Count; i++)
                                                                    {
                                                                        ClientInfo cInfo = clientList[i];
                                                                        if (cInfo != null)
                                                                        {
                                                                            EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                                                                            if (player != null && player.Progression != null)
                                                                            {
                                                                                if (cInfo.playerName.Contains("☼") || cInfo.playerName.Contains("§"))
                                                                                {
                                                                                    responseMessage += cInfo.CrossplatformId.CombinedString + "/" + cInfo.entityId + "§" + "<Invalid Chars>" + "§"
                                                                                        + player.Health + "/" + (int)player.Stamina + "§" + player.Progression.Level + "§"
                                                                                        + (int)player.position.x + "," + (int)player.position.y + "," + (int)player.position.z;
                                                                                }
                                                                                else
                                                                                {
                                                                                    responseMessage += cInfo.CrossplatformId.CombinedString + "/" + cInfo.entityId + "§" + cInfo.playerName + "§"
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
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                if (responseMessage.Length > 0)
                                                                {
                                                                    responseMessage = responseMessage.TrimEnd('☼') + "╚" + salt;
                                                                }
                                                                else
                                                                {
                                                                    responseMessage += salt;
                                                                }
                                                                _response.StatusCode = 200;
                                                            }
                                                            else
                                                            {
                                                                _response.Redirect(Panel_Address);
                                                                _response.StatusCode = 401;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.Remove(clientData[0]);
                                                        AuthorizedTime.Remove(clientData[0]);
                                                        _response.Redirect(Panel_Address);
                                                        _response.StatusCode = 401;
                                                        Writer(string.Format("Client {0} has been logged out", clientData[0]));
                                                    }
                                                }
                                                else
                                                {
                                                    _response.Redirect(Panel_Address);
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            break;
                                        case "Config":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] clientData = postMessage.Split('☼');
                                                if (Authorized.ContainsKey(clientData[0]))
                                                {
                                                    AuthorizedTime.TryGetValue(clientData[0], out DateTime time);
                                                    if (DateTime.Now <= time)
                                                    {
                                                        Authorized.TryGetValue(clientData[0], out string pass);
                                                        using (SHA512 sha512 = SHA512.Create())
                                                        {
                                                            byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                            byte[] hashBytes = sha512.ComputeHash(bytes);
                                                            string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            if (clientData[1].ToUpper() == keyHash)
                                                            {
                                                                string salt = PersistentOperations.CreatePassword(4);
                                                                pass += salt;
                                                                Authorized[clientData[0]] = pass;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(WebPanel.Timeout);
                                                                if (!PassThrough.ContainsKey(_ip))
                                                                {
                                                                    PassThrough.Add(_ip, "Config");
                                                                }
                                                                else
                                                                {
                                                                    PassThrough[_ip] = "Config";
                                                                }
                                                                responseMessage += salt;
                                                                _response.StatusCode = 200;
                                                            }
                                                            else
                                                            {
                                                                _response.Redirect(Panel_Address);
                                                                _response.StatusCode = 401;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.Remove(clientData[0]);
                                                        AuthorizedTime.Remove(clientData[0]);
                                                        _response.Redirect(Panel_Address);
                                                        _response.StatusCode = 401;
                                                        Writer(string.Format("Client {0} has been logged out", clientData[0]));
                                                    }
                                                }
                                                else
                                                {
                                                    _response.Redirect(Panel_Address);
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            break;
                                        case "SaveConfig":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] clientData = postMessage.Split('☼');
                                                if (Authorized.ContainsKey(clientData[0]))
                                                {
                                                    AuthorizedTime.TryGetValue(clientData[0], out DateTime time);
                                                    if (DateTime.Now <= time)
                                                    {
                                                        Authorized.TryGetValue(clientData[0], out string pass);
                                                        using (SHA512 sha512 = SHA512.Create())
                                                        {
                                                            byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                            byte[] hashBytes = sha512.ComputeHash(bytes);
                                                            string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            if (clientData[1].ToUpper() == keyHash)
                                                            {
                                                                string salt = PersistentOperations.CreatePassword(4);
                                                                pass += salt;
                                                                Authorized[clientData[0]] = pass;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(WebPanel.Timeout);
                                                                responseMessage += salt;
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
                                                                    bool changed = false;
                                                                    XmlNodeList nodes = xmlDoc.GetElementsByTagName("Tool");
                                                                    string[] tools = clientData[2].Split('☼');
                                                                    for (int i = 0; i < tools.Length; i++)
                                                                    {
                                                                        XmlNode node = nodes[i];
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
                                                                        Writer(string.Format("Client {0} at IP {1} has updated the ServerToolsConfig.xml", clientData[0], _ip));
                                                                        _response.StatusCode = 200;
                                                                    }
                                                                    else
                                                                    {
                                                                        _response.StatusCode = 406;
                                                                    }
                                                                }

                                                            }
                                                            else
                                                            {
                                                                _response.Redirect(Panel_Address);
                                                                _response.StatusCode = 401;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.Remove(clientData[0]);
                                                        AuthorizedTime.Remove(clientData[0]);
                                                        _response.Redirect(Panel_Address);
                                                        _response.StatusCode = 401;
                                                        Writer(string.Format("Client {0} has been logged out", clientData[0]));
                                                    }
                                                }
                                                else
                                                {
                                                    _response.Redirect(Panel_Address);
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            break;
                                        case "Kick":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] clientData = postMessage.Split('☼');
                                                if (Authorized.ContainsKey(clientData[0]))
                                                {
                                                    AuthorizedTime.TryGetValue(clientData[0], out DateTime time);
                                                    if (DateTime.Now <= time)
                                                    {
                                                        Authorized.TryGetValue(clientData[0], out string pass);
                                                        using (SHA512 sha512 = SHA512.Create())
                                                        {
                                                            byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                            byte[] hashBytes = sha512.ComputeHash(bytes);
                                                            string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            if (clientData[1].ToUpper() == keyHash)
                                                            {
                                                                string salt = PersistentOperations.CreatePassword(4);
                                                                pass += salt;
                                                                Authorized[clientData[0]] = pass;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(WebPanel.Timeout);
                                                                responseMessage += salt;
                                                                ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(clientData[2]);
                                                                if (cInfo != null)
                                                                {
                                                                    SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0}", cInfo.CrossplatformId.CombinedString), null);
                                                                    Writer(string.Format("Client '{0}' at IP '{1}' has kicked '{2}' '{3}'", clientData[0], _ip, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString));
                                                                    _response.StatusCode = 200;
                                                                }
                                                                else
                                                                {
                                                                    _response.StatusCode = 406;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                _response.Redirect(Panel_Address);
                                                                _response.StatusCode = 401;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.Remove(clientData[0]);
                                                        AuthorizedTime.Remove(clientData[0]);
                                                        _response.Redirect(Panel_Address);
                                                        _response.StatusCode = 401;
                                                        Writer(string.Format("Client {0} has been logged out", clientData[0]));
                                                    }
                                                }
                                                else
                                                {
                                                    _response.Redirect(Panel_Address);
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            break;
                                        case "Ban":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] clientData = postMessage.Split('☼');
                                                if (Authorized.ContainsKey(clientData[0]))
                                                {
                                                    AuthorizedTime.TryGetValue(clientData[0], out DateTime time);
                                                    if (DateTime.Now <= time)
                                                    {
                                                        Authorized.TryGetValue(clientData[0], out string pass);
                                                        byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                        using (SHA512 sha512 = SHA512.Create())
                                                        {
                                                            byte[] hashBytes = sha512.ComputeHash(bytes);
                                                            string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            if (clientData[1].ToUpper() == keyHash)
                                                            {
                                                                string salt = PersistentOperations.CreatePassword(4);
                                                                pass += salt;
                                                                Authorized[clientData[0]] = pass;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(WebPanel.Timeout);
                                                                responseMessage += salt;
                                                                ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(clientData[2]);
                                                                if (cInfo != null)
                                                                {
                                                                    SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 1 year", cInfo.CrossplatformId.CombinedString), null);
                                                                    Writer(string.Format("Client '{0}' at IP '{1}' has banned ID '{2}' '{3}' named '{4}'", clientData[0], _ip, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                                                    _response.StatusCode = 200;
                                                                }
                                                                else
                                                                {
                                                                    PersistentPlayerData ppd = PersistentOperations.GetPersistentPlayerDataFromId(clientData[2]);
                                                                    if (ppd != null)
                                                                    {
                                                                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 1 year", ppd.UserIdentifier.CombinedString), null);
                                                                        Writer(string.Format("Client '{0}' at IP '{1}' has banned ID '{2}' named '{3}'", clientData[0], _ip, ppd.UserIdentifier.CombinedString, ppd.PlayerName));
                                                                        _response.StatusCode = 406;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                _response.Redirect(Panel_Address);
                                                                _response.StatusCode = 401;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.Remove(clientData[0]);
                                                        AuthorizedTime.Remove(clientData[0]);
                                                        _response.Redirect(Panel_Address);
                                                        _response.StatusCode = 401;
                                                        Writer(string.Format("Client {0} has been logged out", clientData[0]));
                                                    }
                                                }
                                                else
                                                {
                                                    _response.Redirect(Panel_Address);
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            break;
                                        case "Mute":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] clientData = postMessage.Split('☼');
                                                if (Authorized.ContainsKey(clientData[0]))
                                                {
                                                    AuthorizedTime.TryGetValue(clientData[0], out DateTime time);
                                                    if (DateTime.Now <= time)
                                                    {
                                                        Authorized.TryGetValue(clientData[0], out string pass);
                                                        byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                        using (SHA512 sha512 = SHA512.Create())
                                                        {
                                                            byte[] hashBytes = sha512.ComputeHash(bytes);
                                                            string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            if (clientData[1].ToUpper() == keyHash)
                                                            {
                                                                string salt = PersistentOperations.CreatePassword(4);
                                                                pass += salt;
                                                                Authorized[clientData[0]] = pass;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(WebPanel.Timeout);
                                                                responseMessage += salt;
                                                                if (Mute.IsEnabled)
                                                                {
                                                                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(clientData[2]);
                                                                    if (cInfo != null)
                                                                    {
                                                                        if (Mute.Mutes.Contains(cInfo.CrossplatformId.CombinedString))
                                                                        {
                                                                            Mute.Mutes.Remove(cInfo.CrossplatformId.CombinedString);
                                                                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteTime = 0;
                                                                            Writer(string.Format("Client {0} at IP {1} has unmuted ID {2} named {3}", clientData[0], _ip, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                                                            _response.StatusCode = 202;
                                                                        }
                                                                        else
                                                                        {
                                                                            Mute.Mutes.Add(cInfo.CrossplatformId.CombinedString);
                                                                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteTime = -1;
                                                                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteName = cInfo.playerName;
                                                                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].MuteDate = DateTime.Now;
                                                                            Writer(string.Format("Client {0} at IP {1} has muted ID {2} named {3}", clientData[0], _ip, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                                                            _response.StatusCode = 200;
                                                                        }
                                                                        PersistentContainer.DataChange = true;
                                                                    }
                                                                    else
                                                                    {
                                                                        PersistentPlayerData ppd = PersistentOperations.GetPersistentPlayerDataFromId(clientData[2]);
                                                                        if (ppd != null)
                                                                        {
                                                                            if (Mute.Mutes.Contains(ppd.UserIdentifier.CombinedString))
                                                                            {
                                                                                Mute.Mutes.Remove(ppd.UserIdentifier.CombinedString);
                                                                                PersistentContainer.Instance.Players[ppd.UserIdentifier.CombinedString].MuteTime = 0;
                                                                                Writer(string.Format("Client {0} at IP {1} has unmuted ID {2}", clientData[0], _ip, ppd.UserIdentifier.CombinedString));
                                                                                _response.StatusCode = 202;
                                                                            }
                                                                            else
                                                                            {
                                                                                Mute.Mutes.Add(ppd.UserIdentifier.CombinedString);
                                                                                PersistentContainer.Instance.Players[ppd.UserIdentifier.CombinedString].MuteTime = -1;
                                                                                PersistentContainer.Instance.Players[ppd.UserIdentifier.CombinedString].MuteName = "-Unknown-";
                                                                                PersistentContainer.Instance.Players[ppd.UserIdentifier.CombinedString].MuteDate = DateTime.Now;
                                                                                Writer(string.Format("Client {0} at IP {1} has muted ID {2}", clientData[0], _ip, ppd.UserIdentifier.CombinedString));
                                                                                _response.StatusCode = 200;
                                                                            }
                                                                            PersistentContainer.DataChange = true;
                                                                        }
                                                                        else
                                                                        {
                                                                            _response.StatusCode = 406;
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
                                                                _response.Redirect(Panel_Address);
                                                                _response.StatusCode = 401;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.Remove(clientData[0]);
                                                        AuthorizedTime.Remove(clientData[0]);
                                                        _response.Redirect(Panel_Address);
                                                        _response.StatusCode = 401;
                                                        Writer(string.Format("Client {0} has been logged out", clientData[0]));
                                                    }
                                                }
                                                else
                                                {
                                                    _response.Redirect(Panel_Address);
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            break;
                                        case "Jail":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] clientData = postMessage.Split('☼');
                                                if (Authorized.ContainsKey(clientData[0]))
                                                {
                                                    AuthorizedTime.TryGetValue(clientData[0], out DateTime time);
                                                    if (DateTime.Now <= time)
                                                    {
                                                        Authorized.TryGetValue(clientData[0], out string pass);
                                                        byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                        using (SHA512 sha512 = SHA512.Create())
                                                        {
                                                            byte[] hashBytes = sha512.ComputeHash(bytes);
                                                            string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            if (clientData[1].ToUpper() == keyHash)
                                                            {
                                                                string salt = PersistentOperations.CreatePassword(4);
                                                                pass += salt;
                                                                Authorized[clientData[0]] = pass;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(WebPanel.Timeout);
                                                                responseMessage += salt;
                                                                if (Jail.IsEnabled)
                                                                {
                                                                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(clientData[2]);
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
                                                                                Writer(string.Format("Client '{0}' at IP '{1}' has unjailed '{2}' '{3}' named '{4}'", clientData[0], _ip, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
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
                                                                            Writer(string.Format("Client '{0}' at IP '{1}' has jailed '{2}' '{3}' named '{4}'", clientData[0], _ip, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                                                            _response.StatusCode = 200;
                                                                        }
                                                                        else
                                                                        {
                                                                            _response.StatusCode = 409;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        PersistentPlayerData ppd = PersistentOperations.GetPersistentPlayerDataFromId(clientData[2]);
                                                                        if (ppd != null)
                                                                        {
                                                                            if (Jail.Jailed.Contains(ppd.UserIdentifier.CombinedString))
                                                                            {
                                                                                Jail.Jailed.Remove(ppd.UserIdentifier.CombinedString);
                                                                                PersistentContainer.Instance.Players[ppd.UserIdentifier.CombinedString].JailTime = 0;
                                                                                Writer(string.Format("Client {0} at IP {1} has unjailed {2}", clientData[0], _ip, ppd.UserIdentifier.CombinedString));
                                                                            }
                                                                            else if (Jail.Jail_Position != "")
                                                                            {
                                                                                Jail.Jailed.Add(ppd.UserIdentifier.CombinedString);
                                                                                PersistentContainer.Instance.Players[ppd.UserIdentifier.CombinedString].JailTime = -1;
                                                                                PersistentContainer.Instance.Players[ppd.UserIdentifier.CombinedString].JailName = "-Unknown-";
                                                                                PersistentContainer.Instance.Players[ppd.UserIdentifier.CombinedString].JailDate = DateTime.Now;
                                                                                Writer(string.Format("Client {0} at IP {1} has jailed {2}", clientData[0], _ip, ppd.UserIdentifier.CombinedString));
                                                                            }
                                                                            PersistentContainer.DataChange = true;
                                                                            _response.StatusCode = 200;
                                                                        }
                                                                        else
                                                                        {
                                                                            _response.StatusCode = 406;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    _response.StatusCode = 500;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                _response.Redirect(Panel_Address);
                                                                _response.StatusCode = 401;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.Remove(clientData[0]);
                                                        AuthorizedTime.Remove(clientData[0]);
                                                        _response.Redirect(Panel_Address);
                                                        _response.StatusCode = 401;
                                                        Writer(string.Format("Client {0} has been logged out", clientData[0]));
                                                    }
                                                }
                                                else
                                                {
                                                    _response.Redirect(Panel_Address);
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            break;
                                        case "Reward":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] clientData = postMessage.Split('☼');
                                                if (Authorized.ContainsKey(clientData[0]))
                                                {
                                                    AuthorizedTime.TryGetValue(clientData[0], out DateTime time);
                                                    if (DateTime.Now <= time)
                                                    {
                                                        Authorized.TryGetValue(clientData[0], out string pass);
                                                        byte[] bytes = Encoding.UTF8.GetBytes(pass);
                                                        using (SHA512 sha512 = SHA512.Create())
                                                        {
                                                            byte[] hashBytes = sha512.ComputeHash(bytes);
                                                            string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            if (clientData[1].ToUpper() == keyHash)
                                                            {
                                                                string salt = PersistentOperations.CreatePassword(4);
                                                                pass += salt;
                                                                Authorized[clientData[0]] = pass;
                                                                AuthorizedTime[clientData[0]] = DateTime.Now.AddMinutes(WebPanel.Timeout);
                                                                responseMessage += salt;
                                                                if (Voting.IsEnabled)
                                                                {
                                                                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(clientData[2]);
                                                                    if (cInfo != null)
                                                                    {
                                                                        Voting.ItemOrBlockCounter(cInfo, Voting.Reward_Count);
                                                                        Writer(string.Format("Client {0} at IP {1} has rewarded {2} named {3}", clientData[0], _ip, cInfo.PlatformId.CombinedString, cInfo.playerName));
                                                                        _response.StatusCode = 200;
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
                                                                _response.Redirect(Panel_Address);
                                                                _response.StatusCode = 401;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.Remove(clientData[0]);
                                                        AuthorizedTime.Remove(clientData[0]);
                                                        _response.Redirect(Panel_Address);
                                                        _response.StatusCode = 401;
                                                        Writer(string.Format("Client {0} has been logged out", clientData[0]));
                                                    }
                                                }
                                                else
                                                {
                                                    _response.Redirect(Panel_Address);
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            break;
                                        case "EnterShop":
                                            if (postMessage.Length > 127)
                                            {
                                                bool Found = false;
                                                var customers = Shop.PanelAccess.ToArray();
                                                for (int i = 0; i < customers.Length; i++)
                                                {
                                                    byte[] bytes = Encoding.UTF8.GetBytes(customers[i].Key);
                                                    using (SHA512 sha512_1 = SHA512.Create())
                                                    {
                                                        byte[] hashBytes = sha512_1.ComputeHash(bytes);
                                                        string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                        if (postMessage.ToUpper() == keyHash)
                                                        {
                                                            Found = true;
                                                            AuthorizedTime.TryGetValue(customers[i].Key, out DateTime remainingTime);
                                                            if (DateTime.Now <= remainingTime)
                                                            {
                                                                Shop.PanelAccess.TryGetValue(customers[i].Key, out int entityId);
                                                                ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                                if (cInfo != null)
                                                                {
                                                                    if (PageHits.ContainsKey(_ip))
                                                                    {
                                                                        PageHits.Remove(_ip);
                                                                    }
                                                                    AuthorizedTime.Remove(customers[i].Key);
                                                                    for (int j = 0; j < 10; j++)
                                                                    {
                                                                        string salt = PersistentOperations.CreatePassword(4);
                                                                        bytes = Encoding.UTF8.GetBytes(customers[i].Key + salt);
                                                                        using (SHA512 sha512 = SHA512.Create())
                                                                        {
                                                                            hashBytes = sha512.ComputeHash(bytes);
                                                                            keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                                        }
                                                                        if (!Authorized.ContainsKey(keyHash))
                                                                        {
                                                                            Authorized.Add(keyHash, customers[i].Key);
                                                                            AuthorizedTime.Add(keyHash, DateTime.Now.AddMinutes(5));
                                                                            int currency = Wallet.GetCurrency(cInfo.CrossplatformId.CombinedString);
                                                                            if (Bank.IsEnabled && Bank.Payments)
                                                                            {
                                                                                currency += PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Bank;
                                                                            }
                                                                            responseMessage += cInfo.playerName + "☼" + "Balance: " + currency + "☼" + Wallet.Currency_Name +
                                                                                "☼" + Shop.Panel_Name + "☼" + Shop.PanelItems + "☼" + Shop.CategoryString + "☼" + salt;
                                                                            _response.StatusCode = 200;
                                                                            break;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    Shop.PanelAccess.Remove(customers[i].Key);
                                                                    AuthorizedTime.Remove(customers[i].Key);
                                                                    _response.StatusCode = 403;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Shop.PanelAccess.Remove(customers[i].Key);
                                                                AuthorizedTime.Remove(customers[i].Key);
                                                                _response.StatusCode = 402;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (!Found)
                                                {
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            else if (postMessage == "DBUG")
                                            {
                                                if (PageHits.ContainsKey(_ip))
                                                {
                                                    PageHits.Remove(_ip);
                                                }
                                                responseMessage += "DBUG" + "☼" + "Balance: " + 0 + "☼" + Wallet.Currency_Name +
                                                    "☼" + Shop.Panel_Name + "☼" + Shop.PanelItems + "☼" + Shop.CategoryString + "☼" + "";
                                                _response.StatusCode = 200;
                                            }
                                            break;
                                        case "ExitShop":
                                            if (postMessage.Length > 127)
                                            {
                                                string postUppercase = postMessage.ToUpper();
                                                if (Authorized.ContainsKey(postUppercase))
                                                {
                                                    Authorized.TryGetValue(postUppercase, out string id);
                                                    Shop.PanelAccess.TryGetValue(id, out int entityId);
                                                    Shop.PanelAccess.Remove(id);
                                                    Authorized.Remove(postUppercase);
                                                    AuthorizedTime.Remove(postUppercase);
                                                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                    if (cInfo != null)
                                                    {
                                                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close browserShop", true));
                                                    }
                                                    _response.StatusCode = 200;
                                                }
                                                else
                                                {
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            else if (postMessage == "DBUG")
                                            {
                                                _response.StatusCode = 200;
                                            }
                                            break;
                                        case "ShopPurchase":
                                            if (postMessage.Length > 127 && postMessage.Contains('☼'))
                                            {
                                                string[] purchaseData = postMessage.Split('☼');
                                                string purchaseDataUppercase = purchaseData[0].ToUpper();
                                                if (Authorized.ContainsKey(purchaseDataUppercase))
                                                {
                                                    AuthorizedTime.TryGetValue(purchaseDataUppercase, out DateTime remainingTime);
                                                    if (DateTime.Now <= remainingTime)
                                                    {
                                                        Authorized.TryGetValue(purchaseDataUppercase, out string id);
                                                        string keyHash = "", salt = "";
                                                        for (int i = 0; i < 10; i++)
                                                        {
                                                            salt = PersistentOperations.CreatePassword(4);
                                                            byte[] bytes = Encoding.UTF8.GetBytes(id + salt);
                                                            using (SHA512 sha512 = SHA512.Create())
                                                            {
                                                                byte[] hashBytes = sha512.ComputeHash(bytes);
                                                                keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            }
                                                            if (!Authorized.ContainsKey(keyHash))
                                                            {
                                                                Authorized.Remove(purchaseDataUppercase);
                                                                AuthorizedTime.Remove(purchaseDataUppercase);
                                                                Authorized.Add(keyHash, id);
                                                                AuthorizedTime.Add(keyHash, DateTime.Now.AddMinutes(5));
                                                                break;
                                                            }
                                                        }
                                                        int shopNumber = int.Parse(purchaseData[1]);
                                                        if (shopNumber < Shop.Dict.Count)
                                                        {
                                                            string[] item = Shop.Dict.ElementAt(shopNumber);
                                                            Shop.PanelAccess.TryGetValue(id, out int entityId);
                                                            ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                            if (cInfo != null)
                                                            {
                                                                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                                                                if (player != null)
                                                                {
                                                                    int currency = Wallet.GetCurrency(cInfo.CrossplatformId.CombinedString);
                                                                    if (Bank.IsEnabled && Bank.Payments)
                                                                    {
                                                                        currency += PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Bank;
                                                                    }
                                                                    int quantity = int.Parse(purchaseData[2]);
                                                                    int count = int.Parse(item[3]);
                                                                    int price = int.Parse(item[5]);
                                                                    int total = price * quantity;
                                                                    if (currency >= total)
                                                                    {
                                                                        if (Bank.IsEnabled && Bank.Payments)
                                                                        {
                                                                            Wallet.RemoveCurrency(cInfo.CrossplatformId.CombinedString, total, true);
                                                                        }
                                                                        else
                                                                        {
                                                                            Wallet.RemoveCurrency(cInfo.CrossplatformId.CombinedString, total, false);
                                                                        }
                                                                        ItemValue itemValue = new ItemValue(ItemClass.GetItem(item[1], false).type);
                                                                        int quality = int.Parse(item[4]);
                                                                        if (itemValue.HasQuality)
                                                                        {
                                                                            itemValue.Quality = 1;
                                                                            if (quality > 1)
                                                                            {
                                                                                itemValue.Quality = quality;
                                                                            }
                                                                            itemValue.Modifications = new ItemValue[(int)EffectManager.GetValue(PassiveEffects.ModSlots, itemValue, itemValue.Quality - 1)];
                                                                            itemValue.CosmeticMods = new ItemValue[itemValue.ItemClass.HasAnyTags(ItemClassModifier.CosmeticItemTags) ? 1 : 0];
                                                                        }
                                                                        World world = GameManager.Instance.World;
                                                                        EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                                                        {
                                                                            entityClass = EntityClass.FromString("item"),
                                                                            id = EntityFactory.nextEntityID++,
                                                                            itemStack = new ItemStack(itemValue, count * quantity),
                                                                            pos = world.Players.dict[cInfo.entityId].position,
                                                                            rot = new Vector3(20f, 0f, 20f),
                                                                            lifetime = 60f,
                                                                            belongsPlayerId = cInfo.entityId
                                                                        });
                                                                        world.SpawnEntityInWorld(entityItem);
                                                                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, cInfo.entityId));
                                                                        world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                                                        Log.Out(string.Format("Sold '{0}' to '{1}' '{2}' named '{3}' through the shop", itemValue.ItemClass.Name, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                                                        Phrases.Dict.TryGetValue("Shop16", out string phrase);
                                                                        phrase = phrase.Replace("{Count}", (count * quantity).ToString());
                                                                        if (item[2] != "")
                                                                        {
                                                                            phrase = phrase.Replace("{Item}", item[2]);
                                                                        }
                                                                        else
                                                                        {
                                                                            phrase = phrase.Replace("{Item}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.GetItemName());
                                                                        }
                                                                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                                        responseMessage += currency - total + "§" + Wallet.Currency_Name + "§" + salt;
                                                                        _response.StatusCode = 200;
                                                                    }
                                                                    else
                                                                    {
                                                                        responseMessage += currency + "§" + Wallet.Currency_Name + "§" + salt;
                                                                        _response.StatusCode = 402;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            responseMessage += salt;
                                                            _response.StatusCode = 401;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.TryGetValue(purchaseDataUppercase, out string id);
                                                        Shop.PanelAccess.Remove(id);
                                                        Authorized.Remove(purchaseDataUppercase);
                                                        AuthorizedTime.Remove(purchaseDataUppercase);
                                                        _response.StatusCode = 400;
                                                    }
                                                }
                                                else
                                                {
                                                    _response.StatusCode = 400;
                                                }
                                            }
                                            break;
                                        case "EnterAuction":
                                            if (postMessage.Length > 127)
                                            {
                                                bool Found = false;
                                                var customers = Auction.PanelAccess.ToArray();
                                                for (int i = 0; i < customers.Length; i++)
                                                {
                                                    byte[] bytes = Encoding.UTF8.GetBytes(customers[i].Key);
                                                    using (SHA512 sha512_1 = SHA512.Create())
                                                    {
                                                        byte[] hashBytes = sha512_1.ComputeHash(bytes);
                                                        string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                        if (postMessage.ToUpper() == keyHash)
                                                        {
                                                            Found = true;
                                                            AuthorizedTime.TryGetValue(customers[i].Key, out DateTime remainingTime);
                                                            if (DateTime.Now <= remainingTime)
                                                            {
                                                                Auction.PanelAccess.TryGetValue(customers[i].Key, out int entityId);
                                                                ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                                if (cInfo != null)
                                                                {
                                                                    if (PageHits.ContainsKey(_ip))
                                                                    {
                                                                        PageHits.Remove(_ip);
                                                                    }
                                                                    AuthorizedTime.Remove(customers[i].Key);
                                                                    for (int j = 0; j < 10; j++)
                                                                    {
                                                                        string salt = PersistentOperations.CreatePassword(4);
                                                                        bytes = Encoding.UTF8.GetBytes(customers[i].Key + salt);
                                                                        using (SHA512 sha512_2 = SHA512.Create())
                                                                        {
                                                                            hashBytes = sha512_2.ComputeHash(bytes);
                                                                            keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                                        }
                                                                        if (!Authorized.ContainsKey(keyHash))
                                                                        {
                                                                            Authorized.Add(keyHash, customers[i].Key);
                                                                            AuthorizedTime.Add(keyHash, DateTime.Now.AddMinutes(5));
                                                                            int currency = Wallet.GetCurrency(cInfo.CrossplatformId.CombinedString);
                                                                            if (Bank.IsEnabled && Bank.Payments)
                                                                            {
                                                                                currency += PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Bank;
                                                                            }
                                                                            responseMessage += cInfo.playerName + "☼" + "Balance: " + currency + "☼" + Wallet.Currency_Name +
                                                                                "☼" + Auction.Panel_Name + "☼" + Auction.GetItems(cInfo.CrossplatformId.CombinedString) + "☼" + salt;
                                                                            _response.StatusCode = 200;
                                                                            break;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    Auction.PanelAccess.Remove(customers[i].Key);
                                                                    AuthorizedTime.Remove(customers[i].Key);
                                                                    _response.StatusCode = 403;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Auction.PanelAccess.Remove(customers[i].Key);
                                                                AuthorizedTime.Remove(customers[i].Key);
                                                                _response.StatusCode = 402;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (!Found)
                                                {
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            else if (postMessage == "DBUG")
                                            {
                                                if (PageHits.ContainsKey(_ip))
                                                {
                                                    PageHits.Remove(_ip);
                                                }
                                                responseMessage += "DBUG" + "☼" + "Balance: " + 0 + "☼" + Wallet.Currency_Name +
                                                    "☼" + Auction.Panel_Name + "☼" + Auction.GetItems("DBUG") + "☼" + "";
                                                _response.StatusCode = 200;
                                            }
                                            break;
                                        case "ExitAuction":
                                            if (postMessage.Length > 127)
                                            {
                                                string postUppercase = postMessage.ToUpper();
                                                if (Authorized.ContainsKey(postUppercase))
                                                {
                                                    Authorized.TryGetValue(postUppercase, out string id);
                                                    Auction.PanelAccess.TryGetValue(id, out int entityId);
                                                    Auction.PanelAccess.Remove(id);
                                                    Authorized.Remove(postUppercase);
                                                    AuthorizedTime.Remove(postUppercase);
                                                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                    if (cInfo != null)
                                                    {
                                                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close browserAuction", true));
                                                    }
                                                    _response.StatusCode = 200;
                                                }
                                                else
                                                {
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            else if (postMessage == "DBUG")
                                            {
                                                _response.StatusCode = 200;
                                            }
                                            break;
                                        case "AuctionPurchase":
                                            if (postMessage.Length > 127 && postMessage.Contains('☼'))
                                            {
                                                string[] purchaseData = postMessage.Split('☼');
                                                string purchaseDataUppercase = purchaseData[0].ToUpper();
                                                if (Authorized.ContainsKey(purchaseDataUppercase))
                                                {
                                                    AuthorizedTime.TryGetValue(purchaseDataUppercase, out DateTime remainingTime);
                                                    if (DateTime.Now <= remainingTime)
                                                    {
                                                        Authorized.TryGetValue(purchaseDataUppercase, out string id);
                                                        string keyHash = "", salt = "";
                                                        for (int i = 0; i < 10; i++)
                                                        {
                                                            salt = PersistentOperations.CreatePassword(4);
                                                            byte[] bytes = Encoding.UTF8.GetBytes(id + salt);
                                                            using (SHA512 sha512 = SHA512.Create())
                                                            {
                                                                byte[] hashBytes = sha512.ComputeHash(bytes);
                                                                keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            }
                                                            if (!Authorized.ContainsKey(keyHash))
                                                            {
                                                                Authorized.Remove(purchaseDataUppercase);
                                                                AuthorizedTime.Remove(purchaseDataUppercase);
                                                                Authorized.Add(keyHash, id);
                                                                AuthorizedTime.Add(keyHash, DateTime.Now.AddMinutes(5));
                                                                break;
                                                            }
                                                        }
                                                        int.TryParse(purchaseData[1], out int itemId);
                                                        if (Auction.AuctionItems.ContainsKey(itemId))
                                                        {
                                                            Auction.AuctionItems.TryGetValue(itemId, out string playerId);
                                                            Auction.PanelAccess.TryGetValue(id, out int entityId);
                                                            ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                            if (cInfo != null)
                                                            {
                                                                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                                                                if (player != null)
                                                                {
                                                                    if (cInfo.CrossplatformId.CombinedString != playerId)
                                                                    {
                                                                        if (PersistentContainer.Instance.Players[playerId].Auction != null &&
                                                                            PersistentContainer.Instance.Players[playerId].Auction.ContainsKey(itemId))
                                                                        {
                                                                            PersistentContainer.Instance.Players[playerId].Auction.TryGetValue(itemId, out ItemDataSerializable itemData);
                                                                            int currency = Wallet.GetCurrency(cInfo.CrossplatformId.CombinedString);
                                                                            if (Bank.IsEnabled && Bank.Payments)
                                                                            {
                                                                                currency += PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Bank;
                                                                            }
                                                                            if (currency >= itemData.price)
                                                                            {
                                                                                if (Bank.IsEnabled && Bank.Payments)
                                                                                {
                                                                                    Wallet.RemoveCurrency(cInfo.CrossplatformId.CombinedString, itemData.price, true);
                                                                                }
                                                                                else
                                                                                {
                                                                                    Wallet.RemoveCurrency(cInfo.CrossplatformId.CombinedString, itemData.price, false);
                                                                                }
                                                                                ItemValue itemValue = new ItemValue(ItemClass.GetItem(itemData.name, false).type);
                                                                                if (itemValue != null)
                                                                                {
                                                                                    if (itemValue.HasQuality)
                                                                                    {
                                                                                        itemValue.Quality = 1;
                                                                                        if (itemData.quality > 1)
                                                                                        {
                                                                                            itemValue.Quality = itemData.quality;
                                                                                        }
                                                                                    }
                                                                                    itemValue.UseTimes = itemData.useTimes;
                                                                                    itemValue.Seed = itemData.seed;
                                                                                    if (itemData.modSlots > 0)
                                                                                    {
                                                                                        itemValue.Modifications = new ItemValue[itemData.modSlots];
                                                                                    }
                                                                                    if (itemData.cosmeticSlots > 0)
                                                                                    {
                                                                                        itemValue.CosmeticMods = new ItemValue[itemData.cosmeticSlots];
                                                                                    }
                                                                                    World world = GameManager.Instance.World;
                                                                                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                                                                    {
                                                                                        entityClass = EntityClass.FromString("item"),
                                                                                        id = EntityFactory.nextEntityID++,
                                                                                        itemStack = new ItemStack(itemValue, itemData.count),
                                                                                        pos = player.position,
                                                                                        rot = new Vector3(20f, 0f, 20f),
                                                                                        lifetime = 60f,
                                                                                        belongsPlayerId = cInfo.entityId
                                                                                    });
                                                                                    world.SpawnEntityInWorld(entityItem);
                                                                                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, cInfo.entityId));
                                                                                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                                                                    Auction.AuctionItems.Remove(itemId);
                                                                                    PersistentContainer.Instance.Players[playerId].Auction.Remove(itemId);
                                                                                    PersistentContainer.DataChange = true;
                                                                                    using (StreamWriter sw = new StreamWriter(Auction.Filepath, true, Encoding.UTF8))
                                                                                    {
                                                                                        sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' has purchased auction entry number '{4}'", DateTime.Now, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, itemId));
                                                                                        sw.WriteLine();
                                                                                        sw.Flush();
                                                                                        sw.Close();
                                                                                    }
                                                                                    Phrases.Dict.TryGetValue("Auction11", out string phrase);
                                                                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                                                }
                                                                                responseMessage += salt;
                                                                                _response.StatusCode = 200;
                                                                            }
                                                                            else
                                                                            {
                                                                                responseMessage += salt + "☼" + "You do not have enough to make this purchase";
                                                                                _response.StatusCode = 402;
                                                                            }
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        responseMessage += salt + "☼" + "You own this item. You can not purchase your own";
                                                                        _response.StatusCode = 402;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                responseMessage += salt + "☼" + "You are not in game. Login to the game and try again";
                                                                _response.StatusCode = 402;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            responseMessage += salt;
                                                            _response.StatusCode = 401;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.TryGetValue(purchaseDataUppercase, out string id);
                                                        Auction.PanelAccess.Remove(id);
                                                        Authorized.Remove(purchaseDataUppercase);
                                                        AuthorizedTime.Remove(purchaseDataUppercase);
                                                        _response.StatusCode = 400;
                                                    }
                                                }
                                            }
                                            break;
                                        case "AuctionCancel":
                                            if (postMessage.Length > 127 && postMessage.Contains('☼'))
                                            {
                                                string[] cancelData = postMessage.Split('☼');
                                                string cancelDataUppercase = cancelData[0].ToUpper();
                                                if (Authorized.ContainsKey(cancelDataUppercase))
                                                {
                                                    AuthorizedTime.TryGetValue(cancelDataUppercase, out DateTime remainingTime);
                                                    if (DateTime.Now <= remainingTime)
                                                    {
                                                        Authorized.TryGetValue(cancelDataUppercase, out string id);
                                                        string keyHash = "", salt = "";
                                                        for (int i = 0; i < 10; i++)
                                                        {
                                                            salt = PersistentOperations.CreatePassword(4);
                                                            byte[] bytes = Encoding.UTF8.GetBytes(id + salt);
                                                            using (SHA512 sha512 = SHA512.Create())
                                                            {
                                                                byte[] hashBytes = sha512.ComputeHash(bytes);
                                                                keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                            }
                                                            if (!Authorized.ContainsKey(keyHash))
                                                            {
                                                                Authorized.Remove(cancelDataUppercase);
                                                                AuthorizedTime.Remove(cancelDataUppercase);
                                                                Authorized.Add(keyHash, id);
                                                                AuthorizedTime.Add(keyHash, DateTime.Now.AddMinutes(5));
                                                                break;
                                                            }
                                                        }
                                                        int.TryParse(cancelData[1], out int itemId);
                                                        if (Auction.AuctionItems.ContainsKey(itemId))
                                                        {
                                                            Auction.AuctionItems.TryGetValue(itemId, out string playerId);
                                                            if (PersistentContainer.Instance.Players[playerId].Auction != null && 
                                                                PersistentContainer.Instance.Players[playerId].Auction.ContainsKey(itemId))
                                                            {
                                                                Auction.PanelAccess.TryGetValue(id, out int entityId);
                                                                ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                                if (cInfo != null)
                                                                {
                                                                    EntityPlayer player = PersistentOperations.GetEntityPlayer(entityId);
                                                                    if (player != null)
                                                                    {
                                                                        if (PersistentContainer.Instance.Players[playerId].Auction.TryGetValue(itemId, out ItemDataSerializable itemData))
                                                                        {
                                                                            ItemValue itemValue = new ItemValue(ItemClass.GetItem(itemData.name, false).type);
                                                                            if (itemValue != null)
                                                                            {
                                                                                if (itemValue.HasQuality)
                                                                                {
                                                                                    itemValue.Quality = 1;
                                                                                    if (itemData.quality > 1)
                                                                                    {
                                                                                        itemValue.Quality = itemData.quality;
                                                                                    }
                                                                                }
                                                                                itemValue.UseTimes = itemData.useTimes;
                                                                                itemValue.Seed = itemData.seed;
                                                                                if (itemData.modSlots > 0)
                                                                                {
                                                                                    itemValue.Modifications = new ItemValue[itemData.modSlots];
                                                                                }
                                                                                if (itemData.cosmeticSlots > 0)
                                                                                {
                                                                                    itemValue.CosmeticMods = new ItemValue[itemData.cosmeticSlots];
                                                                                }
                                                                                World world = GameManager.Instance.World;
                                                                                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                                                                {
                                                                                    entityClass = EntityClass.FromString("item"),
                                                                                    id = EntityFactory.nextEntityID++,
                                                                                    itemStack = new ItemStack(itemValue, itemData.count),
                                                                                    pos = player.position,
                                                                                    rot = new Vector3(20f, 0f, 20f),
                                                                                    lifetime = 60f,
                                                                                    belongsPlayerId = cInfo.entityId
                                                                                });
                                                                                world.SpawnEntityInWorld(entityItem);
                                                                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, cInfo.entityId));
                                                                                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                                                                Auction.AuctionItems.Remove(itemId);
                                                                                PersistentContainer.Instance.Players[playerId].Auction.Remove(itemId);
                                                                                PersistentContainer.DataChange = true;
                                                                                using (StreamWriter sw = new StreamWriter(Auction.Filepath, true, Encoding.UTF8))
                                                                                {
                                                                                    sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' has cancelled their auction entry number '{4}'", DateTime.Now, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, itemId));
                                                                                    sw.WriteLine();
                                                                                    sw.Flush();
                                                                                    sw.Close();
                                                                                }
                                                                                Phrases.Dict.TryGetValue("Auction11", out string phrase);
                                                                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                                            }
                                                                            responseMessage += salt;
                                                                            _response.StatusCode = 200;
                                                                        }
                                                                        
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    responseMessage += salt + "☼" + "You are not in game. Login to the game and try again";
                                                                    _response.StatusCode = 402;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                responseMessage += salt + "☼" + "Unable to cancel. Item data not found";
                                                                _response.StatusCode = 402;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            responseMessage += salt;
                                                            _response.StatusCode = 401;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.TryGetValue(cancelDataUppercase, out string id);
                                                        Auction.PanelAccess.Remove(id);
                                                        Authorized.Remove(cancelDataUppercase);
                                                        AuthorizedTime.Remove(cancelDataUppercase);
                                                        _response.StatusCode = 400;
                                                    }
                                                }
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
                                Ban.Add(_ip);
                                if (PersistentContainer.Instance.WebBanList != null)
                                {
                                    PersistentContainer.Instance.WebBanList.Add(_ip);
                                }
                                else
                                {
                                    List<string> bannedIP = new List<string>();
                                    bannedIP.Add(_ip);
                                    PersistentContainer.Instance.WebBanList = bannedIP;
                                }
                                PersistentContainer.DataChange = true;
                                Writer(string.Format("Banned IP '{0}'. Detected attempting to access an invalid address '{1}'", _ip, _uri));
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

        public static string DecryptStringAES(string cipher, string _key, string _iv)
        {
            byte[] key = Encoding.UTF8.GetBytes(_key);
            byte[] iv = Encoding.UTF8.GetBytes(_iv);

            byte[] encrypted = Convert.FromBase64String(cipher);
            string decrypted = DecryptStringFromBytes(encrypted, key, iv);
            return string.Format(decrypted);
        }

        private static string DecryptStringFromBytes(byte[] cipher, byte[] key, byte[] iv)
        {
            string decrypted = string.Empty;
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;
                rijAlg.Key = key;
                rijAlg.IV = iv;
                using (MemoryStream msDecrypt = new MemoryStream(cipher))
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

        public static void Writer(string _input)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(FilePath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: {1}", DateTime.Now, _input));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("Error in WebAPI.Writer: {0}", e.Message));
            }
        }
    }
}
