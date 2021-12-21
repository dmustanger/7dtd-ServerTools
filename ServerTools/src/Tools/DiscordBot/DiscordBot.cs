using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ServerTools
{
    public class DiscordBot
    {
        public static bool IsEnabled = false, TokenLoaded = false;
        public static byte[] TokenBytes;
        public static string TokenKey, Webhook = "", LastEntry = "", LastPlayer = "", Prefix = "[Discord]",
            Prefix_Color = "[FFFFFF]", Name_Color = "[FFFFFF]", Message_Color = "[FFFFFF]";
        public static List<string> Queue = new List<string>();

        private const string file = "DiscordToken.txt";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);

        public static void BuildToken()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.BlockSize = 128;
                        aes.KeySize = 256;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        aes.GenerateKey();
                        TokenBytes = aes.Key;
                        TokenKey = Convert.ToBase64String(aes.Key);
                    }
                    using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                    {
                        sw.WriteLine(TokenKey);
                    }
                    TokenLoaded = true;
                    Log.Out("[SERVERTOOLS] Created and loaded security token for the discord bot");
                }
                else if (!TokenLoaded)
                {
                    using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            string token = sr.ReadToEnd().RemoveLineBreaks().Trim();
                            TokenBytes = Convert.FromBase64String(token);
                            TokenKey = Convert.ToBase64String(TokenBytes);
                        }
                    }
                    TokenLoaded = true;
                    Log.Out("[SERVERTOOLS] Loaded security token for the discord bot");
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DiscordBot.Token: {0}", e.Message));
            }
        }

        public static void WebHook()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.UploadString(Webhook, "{\"content\":\"" + Queue[0].Replace("\"", "") + "\"}");
                    client.Dispose();
                }
            }
            catch (Exception e)
            {
                if (e.Message != null)
                {
                    if (e.Message.Contains("(400)"))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Error in message sent to Discord using the webhook. Contains invalid symbols. Discord has rejected it"));
                    }
                    if (e.Message.Contains("(401)"))
                    {
                        Log.Out("[SERVERTOOLS] The Discord webhook you have provided is not authorized. Correct the token you have provided in the ServerToolsConfig.xml");
                    }
                    else if (e.Message.Contains("(403)"))
                    {
                        Log.Out("[SERVERTOOLS] The Discord network has returned an error to the webhook request. Unable to send message");
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Error in DiscordBot.WebHook: {0}", e.Message));
                    }
                }
            }
            Queue.RemoveAt(0);
        }
    }
}
