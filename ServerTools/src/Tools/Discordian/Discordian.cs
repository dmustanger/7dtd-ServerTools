using ServerTools.Website;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ServerTools
{
    class DiscordBot
    {
        public static bool IsEnabled = false, Loaded = false;
        private const string file = "DiscordToken.txt";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);

        public static byte[] Token { get; private set; }

        public static string TokenS { get; private set; }

        public static void BuildToken()
        {
            try
            {
                if (!Utils.FileExists(filePath))
                {
                    WebPanel.AESProvider.BlockSize = 128;
                    WebPanel.AESProvider.KeySize = 256;
                    WebPanel.AESProvider.Mode = CipherMode.CBC;
                    WebPanel.AESProvider.Padding = PaddingMode.PKCS7;
                    WebPanel.AESProvider.GenerateKey();
                    Token = WebPanel.AESProvider.Key;
                    TokenS = Convert.ToBase64String(WebPanel.AESProvider.Key);
                    using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        sw.WriteLine(TokenS);
                    }
                    Loaded = true;
                    Log.Out("[SERVERTOOLS] Created and loaded security token for the discord bot");
                }
                else
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            string _token = sr.ReadToEnd().RemoveLineBreaks().Trim();
                            Token = Convert.FromBase64String(_token);
                            TokenS = _token;
                            Log.Out("[SERVERTOOLS] Loaded security token for the discord bot");
                            Loaded = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Discordian.Token: {0}", e.Message));
            }
        }
    }
}
