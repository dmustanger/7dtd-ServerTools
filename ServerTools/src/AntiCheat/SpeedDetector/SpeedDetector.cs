using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class SpeedDetector
    {
        public static bool IsEnabled = false;
        public static int Speed_Admin_Level = 0, Total_Flags = 4;

        public static Dictionary<int, int> Flags = new Dictionary<int, int>();

        private static string file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, file);

        public static bool TravelTooFar(Vector3 _oldPosition, Vector3 _newPosition)
        {
            float distance = Vector3.Distance(_oldPosition, _newPosition);
            if (distance > 25)
            {
                return true;
            }
            return false;
        }

        public static void Detected(ClientInfo _cInfo)
        {
            if (Flags.ContainsKey(_cInfo.entityId))
            {
                Flags.TryGetValue(_cInfo.entityId, out int flags);
                flags++;
                Flags[_cInfo.entityId] = flags;
                if (flags == Total_Flags)
                {
                    Timers.Speed_SingleUseTimer(_cInfo);
                }
            }
            else
            {
                Flags.Add(_cInfo.entityId, 1);
            }
        }

        public static void TimerExpired(ClientInfo _cInfo)
        {
            if (Flags.ContainsKey(_cInfo.entityId))
            {
                Flags.TryGetValue(_cInfo.entityId, out int flags);
                if (flags >= Total_Flags)
                {
                    Flags.Remove(_cInfo.entityId);
                    Phrases.Dict.TryGetValue("AntiCheat1", out string phrase);
                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.playerId, phrase), null);
                    using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
                    {
                        sw.WriteLine(string.Format("Detected '{0}' with steam id '{1}' using a speed hack", _cInfo.playerName, _cInfo.playerId));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                    Log.Warning("[SERVERTOOLS] Detected '{0}' with steam id '{1}' using a speed hack", _cInfo.playerName, _cInfo.playerId);
                    Phrases.Dict.TryGetValue("AntiCheat3", out phrase);
                    phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
            }
        }
    }
}
