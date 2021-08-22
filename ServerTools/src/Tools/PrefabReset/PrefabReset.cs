using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class PrefabReset
    {
        public static bool IsEnabled = false, IsRunning = false;

        public static Dictionary<int, bool> PlayerInZone = new Dictionary<int, bool>();
        public static void Load()
        {
            string _file = GameUtils.GetWorldDir()+"/prefabs.xml";

            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(_file);
                ResetPrefabs(xmlDoc);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _file, e.Message));
            }
        }

        protected static void ResetPrefabs(XmlDocument prefabsXML)
        {
            World world = GameManager.Instance.World;

            int cnt = 0;
            foreach (XmlNode childNode in prefabsXML.DocumentElement.ChildNodes)
            {
                XmlElement _line = (XmlElement)childNode;

                string _position = _line.GetAttribute("position");
                string[] sArray = _position.Split(',');

                Vector3 vector = new Vector3(float.Parse(sArray[0]), float.Parse(sArray[1]), float.Parse(sArray[2]));
                try
                {
                    PrefabInstance prefab = world.GetPOIAtPosition(vector);

                    Bounds bounds = prefab.GetAABB();
                    Vector3 size = bounds.size;
                    // Only update larger POI
                    if ((size.x + size.z) > 4)
                    {
                        Vector3i areaStart = new Vector3i(vector.x, vector.y, vector.z);
                        Vector3i areaSize = new Vector3i(areaStart.x + size.x, 0, areaStart.z + size.z);

                        // Executing chunkreset command
                        SdtdConsole.Instance.ExecuteSync(string.Format("chunkreset {0} {1} {2} {3}", areaStart.x, areaStart.z, areaSize.x, areaSize.z), null);
                        cnt++;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(string.Format("[SERVERTOOLS] Failed resetting prefab: {0}", e.Message));
                }
            }

            Log.Out(string.Format("[SERVERTOOLS] Prefab_Preset : {0} prefabs reset.", cnt));
        }

        public static void PlayerCheck(ClientInfo _cInfo, EntityAlive _player)
        {
            World _world = GameManager.Instance.World;

            if (_world.IsPositionWithinPOI(_player.position, 5))
            {
                bool SendMessage = false;
                if (PlayerInZone.ContainsKey(_player.entityId))
                {
                    if (PlayerInZone[_player.entityId] == false)
                    {
                        SendMessage = true;
                    }
                }
                else
                {
                    SendMessage = true;
                }

                if (SendMessage)
                {
                    PlayerInZone[_player.entityId] = true;
                    Phrases.Dict.TryGetValue("PrefabReset1", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + "[-]" + _phrase, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                if (PlayerInZone.ContainsKey(_player.entityId) && PlayerInZone[_player.entityId] == true)
                {
                    PlayerInZone[_player.entityId] = false;
                }
            }
        }
    }
}
