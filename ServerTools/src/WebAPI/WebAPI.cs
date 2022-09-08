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
        public static string Directory = "", Panel_Address = "", BaseAddress = "", Icon_Folder = "../Data/ItemIcons";

        public static Dictionary<string, string> Authorized = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> AuthorizedTime = new Dictionary<string, DateTime>();
        public static Dictionary<string, string[]> Visitor = new Dictionary<string, string[]>();
        public static Dictionary<string, string> PassThrough = new Dictionary<string, string>();
        public static Dictionary<string, int> PageHits = new Dictionary<string, int>();
        public static Dictionary<string, int> LoginAttempts = new Dictionary<string, int>();
        public static Dictionary<string, DateTime> TimeOut = new Dictionary<string, DateTime>();
        public static List<string> Ban = new List<string>();
        

        private static string Redirect = "";
        private static List<string> PostURI = new List<string>();
        private static HttpListener Listener;
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
                string externalIpString = new WebClient().DownloadString("https://ipinfo.io/ip?token=c31843916e5fd7").Trim();
                if (!string.IsNullOrEmpty(externalIpString))
                {
                    BaseAddress = externalIpString;
                    return true;
                }
                else
                {
                    externalIpString = new WebClient().DownloadString("https://api.ipify.org").Trim();
                    if (!string.IsNullOrEmpty(externalIpString))
                    {
                        BaseAddress = externalIpString;
                        return true;
                    }
                }
                Writer(string.Format("The host ip could not be determined. Web_API and Discordian will not function without this"));
                Log.Out(string.Format("[SERVERTOOLS] The host ip could not be determined. Web_API and Discordian will not function without this"));
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
                if (!PostURI.Contains("SignIn"))
                {
                    PostURI.Add("SignIn");
                    PostURI.Add("SignOut");
                    PostURI.Add("NewPass");
                    PostURI.Add("Console");
                    PostURI.Add("Command");
                    PostURI.Add("Players");
                    PostURI.Add("Config");
                    PostURI.Add("SaveConfig");
                    PostURI.Add("Kick");
                    PostURI.Add("Ban");
                    PostURI.Add("Mute");
                    PostURI.Add("Jail");
                    PostURI.Add("Reward");
                    PostURI.Add("EnterShop");
                    PostURI.Add("ExitShop");
                    PostURI.Add("ShopPurchase");
                    PostURI.Add("EnterAuction");
                    PostURI.Add("ExitAuction");
                    PostURI.Add("AuctionPurchase");
                    PostURI.Add("AuctionCancel");
                    PostURI.Add("EnterRio");
                    PostURI.Add("UpdateRio");
                    PostURI.Add("ExitRio");
                    PostURI.Add("StartGameRio");
                    PostURI.Add("ClosedRio");
                    PostURI.Add("RollRio");
                    PostURI.Add("ClaimRio");
                    PostURI.Add("EndTurnRio");
                    PostURI.Add("AddAIRio");
                    PostURI.Add("RemoveAIRio");
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
                        if (SetBaseAddress())
                        {
                            Redirect = "http://" + BaseAddress + ":" + Port;
                            Panel_Address = "http://" + BaseAddress + ":" + Port + "/st.html";
                            if (Listener != null && !Listener.IsListening)
                            {
                                Listener.Prefixes.Clear();
                                if (!Listener.Prefixes.Contains(string.Format("http://*:{0}/", Port)))
                                {
                                    Listener.Prefixes.Add(string.Format("http://*:{0}/", Port));
                                    Listener.Start();
                                    Connected = true;
                                    Log.Out(string.Format("[SERVERTOOLS] ServerTools web api has opened @ {0}", Redirect));
                                }
                                else
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] ServerTools web panel was unable to connect due to the prefix already in use @ '{0}'", Panel_Address));
                                    return;
                                }
                            }
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
                                        else if (uri.ToLower().Contains("script") && !uri.Contains("JS/scripts.js") && !uri.Contains("JS/shopscripts.js") &&
                                            !uri.Contains("JS/rioscripts.js") && !uri.Contains("JS/auctionscripts.js"))
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
        }

        private static void IsAllowed(HttpListenerRequest _request, HttpListenerResponse _response, string _ip, string _uri)
        {
            try
            {
                using (_response)
                {
                    if (_request.HttpMethod == "GET")
                    {
                        if (WebPanel.IsEnabled || RIO.IsEnabled || (Shop.IsEnabled && Shop.Panel) || (Auction.IsEnabled && Shop.Panel))
                        {
                            if (_uri.EndsWith("/"))
                            {
                                _uri = _uri.Remove(_uri.Length - 1);
                            }
                            if (_uri.ToLower().EndsWith("st.html") && WebPanel.IsEnabled ||
                                _uri.ToLower().EndsWith("rio.html") && RIO.IsEnabled ||
                                _uri.ToLower().EndsWith("shop.html") && Shop.IsEnabled ||
                                _uri.ToLower().EndsWith("auction.html") && Auction.IsEnabled)
                            {
                                _uri = _uri.Remove(0, _uri.IndexOf(Port.ToString()) + Port.ToString().Length + 1);
                                if (PageHits.ContainsKey(_ip))
                                {
                                    PageHits[_ip] += 1;
                                    if (PageHits[_ip] >= 8)
                                    {
                                        PageHits.Remove(_ip);
                                        if (!TimeOut.ContainsKey(_ip))
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
                                        }
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
                                (_uri.Contains("Audio/") && _uri.EndsWith(".mp3")) || _uri.EndsWith("favicon.ico"))
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
                            else if (_uri.EndsWith("undefined"))
                            {
                                Writer(string.Format("IP '{0}' requested undefined file '{1}'", _ip, _uri));
                                _response.StatusCode = 400;
                            }
                            else
                            {
                                if (!TimeOut.ContainsKey(_ip))
                                {
                                    TimeOut.Add(_ip, DateTime.Now.AddMinutes(30));
                                    if (PersistentContainer.Instance.WebTimeoutList != null)
                                    {
                                        PersistentContainer.Instance.WebTimeoutList.Add(_ip, DateTime.Now.AddMinutes(30));
                                    }
                                    else
                                    {
                                        Dictionary<string, DateTime> timeouts = new Dictionary<string, DateTime>();
                                        timeouts.Add(_ip, DateTime.Now.AddMinutes(30));
                                        PersistentContainer.Instance.WebTimeoutList = timeouts;
                                    }
                                    PersistentContainer.DataChange = true;
                                }
                                Writer(string.Format("Request denied for IP '{0}' to '{1}'. File could not be found. Client is now in time out for 30 minutes", _ip, _uri));
                            }
                        }
                    }
                    else if (_request.HttpMethod == "POST" && (WebPanel.IsEnabled || DiscordBot.IsEnabled || Auction.Panel || Shop.Panel || RIO.IsEnabled))
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
                            else if (PostURI.Contains(_uri))
                            {
                                if (WebPanel.IsEnabled || (Shop.IsEnabled && Shop.Panel) || (Auction.IsEnabled && Auction.Panel) || RIO.IsEnabled)
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
                                                                    int quantity = int.Parse(purchaseData[2]);
                                                                    int count = int.Parse(item[3]);
                                                                    int price = int.Parse(item[5]);
                                                                    int total = price * quantity;
                                                                    if (currency >= total)
                                                                    {
                                                                        Wallet.RemoveCurrency(cInfo.CrossplatformId.CombinedString, total);
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
                                                                            if (currency >= itemData.price)
                                                                            {
                                                                                Wallet.RemoveCurrency(cInfo.CrossplatformId.CombinedString, itemData.price);
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
                                        case "EnterRio":
                                            if (postMessage.Length > 127)
                                            {
                                                var allPlayers = RIO.Access.ToArray();
                                                for (int i = 0; i < allPlayers.Length; i++)
                                                {
                                                    byte[] bytes = Encoding.UTF8.GetBytes(allPlayers[i].Key);
                                                    using (SHA512 sha512_1 = SHA512.Create())
                                                    {
                                                        byte[] hashBytes = sha512_1.ComputeHash(bytes);
                                                        string keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                        if (postMessage.ToUpper() == keyHash)
                                                        {
                                                            AuthorizedTime.TryGetValue(allPlayers[i].Key, out DateTime remainingTime);
                                                            if (DateTime.Now <= remainingTime)
                                                            {
                                                                if (RIO.Access.TryGetValue(allPlayers[i].Key, out int entityId))
                                                                {
                                                                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                                    if (cInfo != null)
                                                                    {
                                                                        bool valid = true;
                                                                        if (PageHits.ContainsKey(_ip))
                                                                        {
                                                                            PageHits.Remove(_ip);
                                                                        }
                                                                        if (Wallet.IsEnabled && RIO.Bet > 0)
                                                                        {
                                                                            int currency = Wallet.GetCurrency(cInfo.CrossplatformId.CombinedString);
                                                                            if (currency >= RIO.Bet)
                                                                            {
                                                                                Wallet.RemoveCurrency(cInfo.CrossplatformId.CombinedString, RIO.Bet);
                                                                            }
                                                                            else
                                                                            {
                                                                                valid = false;
                                                                                RIO.Access.Remove(allPlayers[i].Key);
                                                                                AuthorizedTime.Remove(allPlayers[i].Key);
                                                                                responseMessage += "You do not have enough to play";
                                                                                _response.StatusCode = 401;
                                                                            }
                                                                        }
                                                                        if (valid)
                                                                        {
                                                                            AuthorizedTime.Remove(allPlayers[i].Key);
                                                                            for (int j = 0; j < 10; j++)
                                                                            {
                                                                                string salt = RIO.CreatePassword(4);
                                                                                bytes = Encoding.UTF8.GetBytes(allPlayers[i].Key + salt);
                                                                                using (SHA512 sha512_2 = SHA512.Create())
                                                                                {
                                                                                    hashBytes = sha512_2.ComputeHash(bytes);
                                                                                    keyHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                                                                                }
                                                                                if (!Authorized.ContainsKey(keyHash))
                                                                                {
                                                                                    Authorized.Add(keyHash, allPlayers[i].Key);
                                                                                    AuthorizedTime.Add(keyHash, DateTime.Now.AddMinutes(2));
                                                                                    responseMessage += salt + "☼";
                                                                                    break;
                                                                                }
                                                                            }
                                                                            bool sitting = false;
                                                                            if (RIO.Tables.Count > 0)
                                                                            {
                                                                                var tables = RIO.Tables.ToArray();//get all tables
                                                                                for (int j = 0; j < tables.Length; j++)
                                                                                {
                                                                                    bool started = false;
                                                                                    var table = tables[j];
                                                                                    if (RIO.Tables.TryGetValue(table.Key, out int[] players))//get player ids
                                                                                    {
                                                                                        if (RIO.Events.TryGetValue(table.Key, out Dictionary<int, string> events))//get table events
                                                                                        {
                                                                                            var eventArray = events.ToArray();
                                                                                            for (int k = 0; k < eventArray.Length; k++)
                                                                                            {
                                                                                                if (eventArray[k].Value.Contains("Start"))//game has started
                                                                                                {
                                                                                                    started = true;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            int playerCount = 0;
                                                                                            for (int k = 0; k < players.Length; k++)
                                                                                            {
                                                                                                if (players[k] == entityId)//if player id match
                                                                                                {
                                                                                                    if (k == 0)//Player is the host
                                                                                                    {
                                                                                                        if (!started)
                                                                                                        {
                                                                                                            for (int l = 0; l < players.Length; l++)//remove all players from table
                                                                                                            {
                                                                                                                players[l] = -1;
                                                                                                            }
                                                                                                            RIO.Tables[table.Key] = players;
                                                                                                            events.Add(events.Count, "HostLeft");
                                                                                                            RIO.Events[table.Key] = events;
                                                                                                        }
                                                                                                        else
                                                                                                        {
                                                                                                            players[k] = -1;//remove player from table
                                                                                                            RIO.Tables[table.Key] = players;
                                                                                                            int playerNumber = k + 1;
                                                                                                            events.Add(events.Count, "Left╚" + playerNumber);
                                                                                                            RIO.Events[table.Key] = events;
                                                                                                        }
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        if (!started)
                                                                                                        {
                                                                                                            players[k] = 0;//remove player and open slot
                                                                                                            RIO.Tables[table.Key] = players;
                                                                                                        }
                                                                                                        else
                                                                                                        {
                                                                                                            players[k] = -1;//remove player and close slot
                                                                                                            RIO.Tables[table.Key] = players;
                                                                                                        }
                                                                                                        int playerNumber = k + 1;
                                                                                                        events.Add(events.Count, "Left╚" + playerNumber);
                                                                                                        RIO.Events[table.Key] = events;
                                                                                                    }
                                                                                                }
                                                                                                else if (players[k] != -1 && players[k] != 0)//Slot is not closed or empty
                                                                                                {
                                                                                                    playerCount += 1;
                                                                                                }
                                                                                            }
                                                                                            if (playerCount == 0)//No players remain
                                                                                            {
                                                                                                RIO.Tables.Remove(table.Key);
                                                                                                RIO.Events.Remove(table.Key);
                                                                                                RIO.Claims.Remove(table.Key);
                                                                                            }
                                                                                            else if (playerCount == 1 && started)//One player remains and game started
                                                                                            {
                                                                                                int winnerId = 0;
                                                                                                int count = 1;
                                                                                                for (int k = 0; k < 4; k++)
                                                                                                {
                                                                                                    if (players[k] != -1 && players[k] != -2)
                                                                                                    {
                                                                                                        winnerId = players[k];
                                                                                                        int playerNumber = k + 1;
                                                                                                        events.Add(events.Count, "Win╚" + playerNumber);
                                                                                                        RIO.Events[table.Key] = events;
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        count += 1;
                                                                                                    }
                                                                                                }
                                                                                                if (RIO.Bet > 0)
                                                                                                {
                                                                                                    ClientInfo cInfoWinner = PersistentOperations.GetClientInfoFromEntityId(winnerId);
                                                                                                    if (cInfoWinner != null)
                                                                                                    {
                                                                                                        Wallet.AddCurrency(cInfoWinner.CrossplatformId.CombinedString, RIO.Bet * count, true);
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                            if (RIO.Tables.Count > 0)
                                                                            {
                                                                                var tables = RIO.Tables.ToArray();//get all tables
                                                                                for (int j = 0; j < tables.Length; j++)
                                                                                {
                                                                                    if (!sitting)
                                                                                    {
                                                                                        bool started = false;
                                                                                        var table = tables[j];
                                                                                        if (RIO.Tables.TryGetValue(table.Key, out int[] players))//get player ids
                                                                                        {
                                                                                            if (RIO.Events.TryGetValue(table.Key, out Dictionary<int, string> events))//get table events
                                                                                            {
                                                                                                var eventArray = events.ToArray();
                                                                                                for (int k = 0; k < eventArray.Length; k++)
                                                                                                {
                                                                                                    if (eventArray[k].Value.Contains("Start"))//game has started
                                                                                                    {
                                                                                                        started = true;
                                                                                                        break;
                                                                                                    }
                                                                                                }
                                                                                                if (!started)
                                                                                                {
                                                                                                    for (int k = 0; k < players.Length; k++)
                                                                                                    {
                                                                                                        if (players[k] == 0)
                                                                                                        {
                                                                                                            sitting = true;
                                                                                                            int playerNumber = k + 1;
                                                                                                            events.Add(events.Count, "Join╚" + playerNumber + "╚" + cInfo.playerName);
                                                                                                            RIO.Events[table.Key] = events;
                                                                                                            responseMessage += table.Key + "☼" + playerNumber;
                                                                                                            _response.StatusCode = 200;
                                                                                                            break;
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                            if (!sitting)
                                                                            {
                                                                                string tableId = "";
                                                                                for (int k = 0; k < 15; k++)
                                                                                {
                                                                                    tableId = RIO.CreateTable();
                                                                                    if (!RIO.Tables.ContainsKey(tableId) && !RIO.Events.ContainsKey(tableId))
                                                                                    {
                                                                                        break;
                                                                                    }
                                                                                }
                                                                                Dictionary<int, string> events = new Dictionary<int, string>();
                                                                                RIO.Events.Add(tableId, events);
                                                                                Dictionary<string, int> claims = new Dictionary<string, int>();
                                                                                RIO.Claims.Add(tableId, claims);
                                                                                RIO.Tables.Add(tableId, new int[] { entityId, 0, 0, 0 });
                                                                                events.Add(0, "Host╚" + cInfo.playerName);
                                                                                RIO.Events[tableId] = events;
                                                                                responseMessage += tableId + "☼" + 1;
                                                                                _response.StatusCode = 202;
                                                                            }
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        RIO.Access.Remove(allPlayers[i].Key);
                                                                        AuthorizedTime.Remove(allPlayers[i].Key);
                                                                        _response.StatusCode = 402;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                RIO.Access.Remove(allPlayers[i].Key);
                                                                AuthorizedTime.Remove(allPlayers[i].Key);
                                                                _response.StatusCode = 401;
                                                            }
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            _response.StatusCode = 400;
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        case "UpdateRio":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] updateData = postMessage.Split('☼');
                                                string updateDataUppercase = updateData[0].ToUpper();
                                                if (Authorized.ContainsKey(updateDataUppercase))
                                                {
                                                    AuthorizedTime.TryGetValue(updateDataUppercase, out DateTime remainingTime);
                                                    if (DateTime.Now <= remainingTime)
                                                    {
                                                        Authorized.TryGetValue(updateDataUppercase, out string id);
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
                                                                Authorized.Remove(updateDataUppercase);
                                                                AuthorizedTime.Remove(updateDataUppercase);
                                                                Authorized.Add(keyHash, id);
                                                                AuthorizedTime.Add(keyHash, DateTime.Now.AddMinutes(2));
                                                                break;
                                                            }
                                                        }
                                                        if (int.TryParse(updateData[2], out int eventCount))
                                                        {
                                                            if (RIO.Events.TryGetValue(updateData[1], out Dictionary<int, string> events))
                                                            {
                                                                if (events.Count > eventCount)
                                                                {
                                                                    responseMessage += salt + "☼";
                                                                    var eventArray = events.ToArray();
                                                                    for (int j = eventCount; j < eventArray.Length; j++)
                                                                    {
                                                                        responseMessage += eventArray[j].Value + "§";
                                                                    }
                                                                    if (responseMessage.Length > 4)
                                                                    {
                                                                        responseMessage = responseMessage.Remove(responseMessage.Length - 1);
                                                                    }
                                                                    _response.StatusCode = 200;
                                                                }
                                                                else
                                                                {
                                                                    responseMessage += salt;
                                                                    _response.StatusCode = 202;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.TryGetValue(updateDataUppercase, out string id);
                                                        RIO.Access.TryGetValue(id, out int entityId);
                                                        RIO.Access.Remove(id);
                                                        Authorized.Remove(updateDataUppercase);
                                                        AuthorizedTime.Remove(updateDataUppercase);
                                                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                        if (cInfo != null)
                                                        {
                                                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close browserRio", true));
                                                        }
                                                        _response.StatusCode = 401;
                                                    }
                                                }
                                            }
                                            break;
                                        case "ExitRio":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] exitRioData = postMessage.Split('☼');
                                                string exitRioDataUppercase = exitRioData[0].ToUpper();
                                                if (Authorized.ContainsKey(exitRioDataUppercase))
                                                {
                                                    Authorized.TryGetValue(exitRioDataUppercase, out string id);
                                                    RIO.Access.TryGetValue(id, out int entityId);
                                                    if (RIO.Tables.ContainsKey(exitRioData[1]))
                                                    {
                                                        if (RIO.Tables.TryGetValue(exitRioData[1], out int[] players))
                                                        {
                                                            if (RIO.Events.TryGetValue(exitRioData[1], out Dictionary<int, string> events))
                                                            {
                                                                int playerCount = 0;
                                                                int bets = 0;
                                                                bool started = false;
                                                                var eventArray = events.ToArray();
                                                                for (int i = 0; i < eventArray.Length; i++)
                                                                {
                                                                    if (eventArray[i].Value.Contains("Host") || eventArray[i].Value.Contains("Join"))
                                                                    {
                                                                        bets += 1;
                                                                    }
                                                                    else if (eventArray[i].Value.Contains("Left"))
                                                                    {
                                                                        bets -= 1;
                                                                    }
                                                                    else if (eventArray[i].Value.Contains("Start"))
                                                                    {
                                                                        started = true;
                                                                        break;
                                                                    }
                                                                }
                                                                if (started)
                                                                {
                                                                    for (int i = 0; i < players.Length; i++)
                                                                    {
                                                                        if (players[i] == entityId)
                                                                        {
                                                                            players[i] = -1;
                                                                            RIO.Tables[exitRioData[1]] = players;
                                                                            int playerNumber = i + 1;
                                                                            events.Add(events.Count, "Left╚" + playerNumber);
                                                                            RIO.Events[exitRioData[1]] = events;
                                                                        }
                                                                        else if (players[i] != -1)
                                                                        {
                                                                            playerCount += 1;
                                                                        }
                                                                    }
                                                                    if (playerCount == 0)
                                                                    {
                                                                        RIO.Tables.Remove(exitRioData[1]);
                                                                        RIO.Events.Remove(exitRioData[1]);
                                                                        RIO.Claims.Remove(exitRioData[1]);
                                                                    }
                                                                    else if (playerCount == 1)
                                                                    {
                                                                        int winnerId = 0;
                                                                        int count = 1;
                                                                        for (int i = 0; i < 4; i++)
                                                                        {
                                                                            if (players[i] != -1 && players[i] != -2)
                                                                            {
                                                                                winnerId = players[i];
                                                                                int playerNumber = i + 1;
                                                                                events.Add(events.Count, "Win╚" + playerNumber);
                                                                                RIO.Events[exitRioData[1]] = events;
                                                                            }
                                                                            else
                                                                            {
                                                                                count += 1;
                                                                            }
                                                                        }
                                                                        if (RIO.Bet > 0)
                                                                        {
                                                                            ClientInfo cInfoWinner = PersistentOperations.GetClientInfoFromEntityId(winnerId);
                                                                            if (cInfoWinner != null)
                                                                            {
                                                                                Wallet.AddCurrency(cInfoWinner.CrossplatformId.CombinedString, RIO.Bet * count, true);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    for (int i = 0; i < players.Length; i++)
                                                                    {
                                                                        if (players[i] == entityId)
                                                                        {
                                                                            if (i == 0)
                                                                            {
                                                                                players[i] = -1;
                                                                                RIO.Tables[exitRioData[1]] = players;
                                                                                events.Add(events.Count, "HostLeft");
                                                                                RIO.Events[exitRioData[1]] = events;
                                                                            }
                                                                            else
                                                                            {
                                                                                players[i] = 0;
                                                                                RIO.Tables[exitRioData[1]] = players;
                                                                                int playerNumber = i + 1;
                                                                                events.Add(events.Count, "Left╚" + playerNumber);
                                                                                RIO.Events[exitRioData[1]] = events;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    RIO.Access.Remove(id);
                                                    Authorized.Remove(exitRioDataUppercase);
                                                    AuthorizedTime.Remove(exitRioDataUppercase);
                                                    ClientInfo cInfo2 = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                    if (cInfo2 != null)
                                                    {
                                                        cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close browserRio", true));
                                                    }
                                                    _response.StatusCode = 200;
                                                }
                                                else
                                                {
                                                    _response.StatusCode = 401;
                                                }
                                            }
                                            break;
                                        case "StartGameRio":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] startGameData = postMessage.Split('☼');
                                                string startGameDataUppercase = startGameData[0].ToUpper();
                                                if (Authorized.ContainsKey(startGameDataUppercase))
                                                {
                                                    AuthorizedTime.TryGetValue(startGameDataUppercase, out DateTime remainingTime);
                                                    if (DateTime.Now <= remainingTime)
                                                    {
                                                        Authorized.TryGetValue(startGameDataUppercase, out string id);
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
                                                                Authorized.Remove(startGameDataUppercase);
                                                                AuthorizedTime.Remove(startGameDataUppercase);
                                                                Authorized.Add(keyHash, id);
                                                                AuthorizedTime.Add(keyHash, DateTime.Now.AddMinutes(2));
                                                                break;
                                                            }
                                                        }
                                                        if (RIO.Events.TryGetValue(startGameData[1], out Dictionary<int, string> events))
                                                        {
                                                            List<int> playerList = new List<int>();
                                                            if (RIO.Tables.TryGetValue(startGameData[1], out int[] players))
                                                            {
                                                                for (int j = 0; j < players.Length; j++)
                                                                {
                                                                    if (players[j] != 0)
                                                                    {
                                                                        playerList.Add(j);
                                                                    }
                                                                    else
                                                                    {
                                                                        players[j] = -1;
                                                                    }
                                                                }
                                                                playerList.RandomizeList();
                                                                events.Add(events.Count, "Start╚" + (playerList[0] + 1));
                                                                RIO.Events[startGameData[1]] = events;
                                                                responseMessage += salt;
                                                                _response.StatusCode = 200;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.TryGetValue(startGameDataUppercase, out string id);
                                                        RIO.Access.TryGetValue(id, out int entityId);
                                                        RIO.Access.Remove(id);
                                                        Authorized.Remove(startGameDataUppercase);
                                                        AuthorizedTime.Remove(startGameDataUppercase);
                                                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                        if (cInfo != null)
                                                        {
                                                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close browserRio", true));
                                                        }
                                                        _response.StatusCode = 401;
                                                    }
                                                }
                                            }
                                            break;
                                        case "RollRio":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] rollRioData = postMessage.Split('☼');
                                                string rollRioDataUppercase = rollRioData[0].ToUpper();
                                                if (Authorized.ContainsKey(rollRioDataUppercase))
                                                {
                                                    AuthorizedTime.TryGetValue(rollRioDataUppercase, out DateTime remainingTime);
                                                    if (DateTime.Now <= remainingTime)
                                                    {
                                                        Authorized.TryGetValue(rollRioDataUppercase, out string id);
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
                                                                Authorized.Remove(rollRioDataUppercase);
                                                                AuthorizedTime.Remove(rollRioDataUppercase);
                                                                Authorized.Add(keyHash, id);
                                                                AuthorizedTime.Add(keyHash, DateTime.Now.AddMinutes(2));
                                                                break;
                                                            }
                                                        }
                                                        if (RIO.Events.TryGetValue(rollRioData[1], out Dictionary<int, string> events))
                                                        {
                                                            string newDice = RIO.GetRoll();
                                                            string diceUpdate = rollRioData[2] + "," + newDice;
                                                            events.Add(events.Count, "Roll╚" + diceUpdate + "╚" + rollRioData[3]);
                                                            RIO.Events[rollRioData[1]] = events;
                                                            responseMessage += salt + "☼" + newDice;
                                                            _response.StatusCode = 200;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.TryGetValue(rollRioDataUppercase, out string id);
                                                        RIO.Access.TryGetValue(id, out int entityId);
                                                        RIO.Access.Remove(id);
                                                        Authorized.Remove(rollRioDataUppercase);
                                                        AuthorizedTime.Remove(rollRioDataUppercase);
                                                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                        if (cInfo != null)
                                                        {
                                                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close browserRio", true));
                                                        }
                                                        _response.StatusCode = 401;
                                                    }
                                                }
                                            }
                                            break;
                                        case "ClaimRio":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] claimRioData = postMessage.Split('☼');
                                                string claimRioDataUppercase = claimRioData[0].ToUpper();
                                                if (Authorized.ContainsKey(claimRioDataUppercase))
                                                {
                                                    AuthorizedTime.TryGetValue(claimRioDataUppercase, out DateTime remainingTime);
                                                    if (DateTime.Now <= remainingTime)
                                                    {
                                                        Authorized.TryGetValue(claimRioDataUppercase, out string id);
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
                                                                Authorized.Remove(claimRioDataUppercase);
                                                                AuthorizedTime.Remove(claimRioDataUppercase);
                                                                Authorized.Add(keyHash, id);
                                                                AuthorizedTime.Add(keyHash, DateTime.Now.AddMinutes(2));
                                                                break;
                                                            }
                                                        }
                                                        if (RIO.Events.TryGetValue(claimRioData[1], out Dictionary<int, string> events))
                                                        {
                                                            if (RIO.Claims.TryGetValue(claimRioData[1], out Dictionary<string, int> claims))
                                                            {
                                                                if (int.TryParse(claimRioData[3], out int playerNumber))
                                                                {
                                                                    claims.Add(claimRioData[2], playerNumber);
                                                                    RIO.Claims[claimRioData[1]] = claims;
                                                                    if (RIO.Tables.TryGetValue(claimRioData[1], out int[] players))
                                                                    {
                                                                        bool NextPlayer = false;
                                                                        for (int j = 0; j < 4; j++)
                                                                        {
                                                                            int number = j + 1;
                                                                            if (number != playerNumber && number > playerNumber && players[j] != -1 && players[j] != 0)
                                                                            {
                                                                                NextPlayer = true;
                                                                                events.Add(events.Count, "Claim╚" + claimRioData[2] + "╚" + playerNumber + "╚" + number + "╚" + claimRioData[4]);//square-playerNumber-nextPlayerNumber-bool(win or lose)
                                                                                RIO.Events[claimRioData[1]] = events;
                                                                                responseMessage += salt;
                                                                                _response.StatusCode = 200;
                                                                                break;
                                                                            }
                                                                        }
                                                                        if (!NextPlayer)
                                                                        {
                                                                            for (int j = 0; j < playerNumber; j++)
                                                                            {
                                                                                int number = j + 1;
                                                                                if (number != playerNumber && number < playerNumber && players[j] != -1 && players[j] != 0)
                                                                                {
                                                                                    events.Add(events.Count, "Claim╚" + claimRioData[2] + "╚" + playerNumber + "╚" + number + "╚" + claimRioData[4]);
                                                                                    RIO.Events[claimRioData[1]] = events;
                                                                                    responseMessage += salt;
                                                                                    _response.StatusCode = 200;
                                                                                    break;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.TryGetValue(claimRioDataUppercase, out string id);
                                                        RIO.Access.TryGetValue(id, out int entityId);
                                                        RIO.Access.Remove(id);
                                                        Authorized.Remove(claimRioDataUppercase);
                                                        AuthorizedTime.Remove(claimRioDataUppercase);
                                                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                        if (cInfo != null)
                                                        {
                                                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close browserRio", true));
                                                        }
                                                        _response.StatusCode = 401;
                                                    }
                                                }
                                            }
                                            break;
                                        case "EndTurnRio":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] endTurnData = postMessage.Split('☼');
                                                string endTurnDataUppercase = endTurnData[0].ToUpper();
                                                if (Authorized.ContainsKey(endTurnDataUppercase))
                                                {
                                                    AuthorizedTime.TryGetValue(endTurnDataUppercase, out DateTime remainingTime);
                                                    if (DateTime.Now <= remainingTime)
                                                    {
                                                        Authorized.TryGetValue(endTurnDataUppercase, out string id);
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
                                                                Authorized.Remove(endTurnDataUppercase);
                                                                AuthorizedTime.Remove(endTurnDataUppercase);
                                                                Authorized.Add(keyHash, id);
                                                                AuthorizedTime.Add(keyHash, DateTime.Now.AddMinutes(2));
                                                                break;
                                                            }
                                                        }
                                                        if (RIO.Events.TryGetValue(endTurnData[1], out Dictionary<int, string> events))
                                                        {
                                                            if (int.TryParse(endTurnData[2], out int playerNumber))
                                                            {
                                                                RIO.Tables.TryGetValue(endTurnData[1], out int[] players);
                                                                bool NextPlayer = false;
                                                                for (int j = playerNumber; j < players.Length; j++)
                                                                {
                                                                    if (j != playerNumber && j > playerNumber && players[j - 1] != -1 && players[j - 1] != 0)
                                                                    {
                                                                        NextPlayer = true;
                                                                        events.Add(events.Count, "Turn╚" + j);
                                                                        RIO.Events[endTurnData[1]] = events;
                                                                        responseMessage += salt;
                                                                        _response.StatusCode = 200;
                                                                        break;
                                                                    }
                                                                }
                                                                if (!NextPlayer)
                                                                {
                                                                    for (int j = 0; j < playerNumber; j++)
                                                                    {
                                                                        if (j != playerNumber && j < playerNumber && players[j] != -1 && players[j] != 0)
                                                                        {
                                                                            events.Add(events.Count, "Turn╚" + (j + 1));
                                                                            RIO.Events[endTurnData[1]] = events;
                                                                            responseMessage += salt;
                                                                            _response.StatusCode = 200;
                                                                            break;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.TryGetValue(endTurnDataUppercase, out string id);
                                                        RIO.Access.TryGetValue(id, out int entityId);
                                                        RIO.Access.Remove(id);
                                                        Authorized.Remove(endTurnDataUppercase);
                                                        AuthorizedTime.Remove(endTurnDataUppercase);
                                                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                        if (cInfo != null)
                                                        {
                                                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close browserRio", true));
                                                        }
                                                        _response.StatusCode = 401;
                                                    }
                                                }
                                            }
                                            break;
                                        case "AddAIRio":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] rollRioData = postMessage.Split('☼');
                                                string rollRioDataUppercase = rollRioData[0].ToUpper();
                                                if (Authorized.ContainsKey(rollRioDataUppercase))
                                                {
                                                    AuthorizedTime.TryGetValue(rollRioDataUppercase, out DateTime remainingTime);
                                                    if (DateTime.Now <= remainingTime)
                                                    {
                                                        Authorized.TryGetValue(rollRioDataUppercase, out string id);
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
                                                                Authorized.Remove(rollRioDataUppercase);
                                                                AuthorizedTime.Remove(rollRioDataUppercase);
                                                                Authorized.Add(keyHash, id);
                                                                AuthorizedTime.Add(keyHash, DateTime.Now.AddMinutes(2));
                                                                break;
                                                            }
                                                        }
                                                        if (RIO.Tables.TryGetValue(rollRioData[1], out int[] table))
                                                        {
                                                            for (int i = 0; i < table.Length; i++)
                                                            {
                                                                if (table[i] != 0 && table[i] != -1 && table[i] != -2)
                                                                {

                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Authorized.TryGetValue(rollRioDataUppercase, out string id);
                                                        RIO.Access.TryGetValue(id, out int entityId);
                                                        RIO.Access.Remove(id);
                                                        Authorized.Remove(rollRioDataUppercase);
                                                        AuthorizedTime.Remove(rollRioDataUppercase);
                                                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                        if (cInfo != null)
                                                        {
                                                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close browserRio", true));
                                                        }
                                                        _response.StatusCode = 401;
                                                    }
                                                }
                                            }
                                            break;
                                        case "RemoveAIRio":
                                            if (postMessage.Contains('☼'))
                                            {
                                                string[] rollRioData = postMessage.Split('☼');
                                                string rollRioDataUppercase = rollRioData[0].ToUpper();
                                                if (Authorized.ContainsKey(rollRioDataUppercase))
                                                {
                                                    AuthorizedTime.TryGetValue(rollRioDataUppercase, out DateTime remainingTime);
                                                    if (DateTime.Now <= remainingTime)
                                                    {
                                                        Authorized.TryGetValue(rollRioDataUppercase, out string id);
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
                                                                Authorized.Remove(rollRioDataUppercase);
                                                                AuthorizedTime.Remove(rollRioDataUppercase);
                                                                Authorized.Add(keyHash, id);
                                                                AuthorizedTime.Add(keyHash, DateTime.Now.AddMinutes(2));
                                                                break;
                                                            }
                                                        }

                                                    }
                                                    else
                                                    {
                                                        Authorized.TryGetValue(rollRioDataUppercase, out string id);
                                                        RIO.Access.TryGetValue(id, out int entityId);
                                                        RIO.Access.Remove(id);
                                                        Authorized.Remove(rollRioDataUppercase);
                                                        AuthorizedTime.Remove(rollRioDataUppercase);
                                                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(entityId);
                                                        if (cInfo != null)
                                                        {
                                                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close browserRio", true));
                                                        }
                                                        _response.StatusCode = 401;
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
