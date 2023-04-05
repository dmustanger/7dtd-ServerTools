using Platform;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class AutoRestart
    {
        public static bool IsEnabled = false;

        public static void Exec()
        {
            Log.Out(string.Format("[SERVERTOOLS] Auto restart initialized"));
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            string text = commandLineArgs[0];
            IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
            if (antiCheatClient != null && antiCheatClient.ClientAntiCheatEnabled())
            {
                RuntimePlatform platform = Application.platform;
                if (((int)platform) != 1)
                {
                    if ((int)platform != 2)
                    {
                        if ((int)platform != 13)
                        {
                            Log.Error(string.Format("[SERVERTOOLS] Restarting the game is not supported on this platform '{0}'", Application.platform));
                            return;
                        }
                        text = Path.GetDirectoryName(text);
                        text = ((text.Length > 0) ? (text + "/") : "") + "7DaysToDie_EAC";
                    }
                    else
                    {
                        text = Path.GetDirectoryName(text);
                        text = ((text.Length > 0) ? (text + "/") : "") + "7DaysToDie_EAC.exe";
                    }
                }
                else
                {
                    text = Path.GetDirectoryName(text);
                    text = ((text.Length > 0) ? (text + "/") : "") + "7DaysToDie_EAC";
                }
            }
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 1; i < commandLineArgs.Length; i++)
            {
                if (i > 1)
                {
                    stringBuilder.Append(' ');
                }
                string text2 = commandLineArgs[i];
                if (text2.IndexOf(' ') >= 0)
                {
                    stringBuilder.Append('"');
                    stringBuilder.Append(text2);
                    stringBuilder.Append('"');
                }
                else
                {
                    stringBuilder.Append(text2);
                }
            }
            Process.Start(new ProcessStartInfo(text)
            {
                UseShellExecute = true,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                Arguments = stringBuilder.ToString()
            });
            //Application.Quit();
        }
    }
}
