using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ServerTools
{
    public class DiscordBot
    {
        public static bool IsEnabled = false, TokenLoaded = false;
        public static string TokenKey, Webhook = "", LastEntry = "", Prefix = "[Discord]",
            Prefix_Color = "[FFFFFF]", Name_Color = "[FFFFFF]", Message_Color = "[FFFFFF]";
        public static int LastPlayer;
        public static List<string> Queue = new List<string>();

        private const string file = "ServerToolsToken.txt";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);

        public static void BuildToken()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    TokenKey = GeneralOperations.CreatePassword(22);
                    using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                    {
                        sw.WriteLine(TokenKey);
                    }
                    TokenLoaded = true;
                    Log.Out(string.Format("[SERVERTOOLS] Created and loaded security token for Discordian bot"));
                }
                else if (!TokenLoaded)
                {
                    using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            TokenKey = sr.ReadToEnd().RemoveLineBreaks().Trim();
                        }
                    }
                    TokenLoaded = true;
                    Log.Out(string.Format("[SERVERTOOLS] Loaded security token for Discordian bot"));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DiscordBot.BuildToken: {0}", e.Message));
            }
        }

        public static void WebHook()
        {
            try
            {
                if (Badwords.IsEnabled)
                {
                    for (int i = 0; i < Badwords.Dict.Count; i++)
                    {
                        if (Queue[0].ToLower().Contains(Badwords.Dict[i]))
                        {
                            Queue[0] = Queue[0].ToLower().Replace(Badwords.Dict[i], "***");
                        }
                    }
                }
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.UploadString(Webhook, "{\"content\":\"" + Queue[0].Replace("\"", "") + "\"}");
                    client.Dispose();
                    Queue.RemoveAt(0);
                }
                return;
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
                        Log.Out(string.Format("[SERVERTOOLS] The Discord webhook you have provided is not authorized. Check the token you have provided in the ServerToolsConfig.xml"));
                    }
                    else if (e.Message.Contains("(403)"))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] The Discord network has returned an error to the webhook request. Unable to send message"));
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Error in DiscordBot.WebHook: {0}", e.Message));
                    }
                }
            }
        }
    }
}
