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

        public static bool DisableResetPrefabs = false;

        public static Dictionary<int,Vector3i> UpdatedChunks = new Dictionary<int, Vector3i>();


        private static readonly string file = "ResetChunks.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);


        public static void Load()
        {
                ResetPrefabs();
        }

        public static void InitXML() {
            using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");

                sw.WriteLine("<Chunks>");
                sw.WriteLine("</Chunks>");
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }
        }

        public static bool LoadXML() {
            XmlDocument xmlDoc = new XmlDocument();
            if (Utils.FileExists(FilePath))
            {
                try
                {
                    xmlDoc.Load(FilePath);

                    // UpdatedChunks
                    XmlNodeList _childNodes = xmlDoc.DocumentElement.ChildNodes;
                    for (int i = 0; i < _childNodes.Count; i++)
                    {
                        XmlElement _line = (XmlElement)_childNodes[i];
                        if (_line.HasAttribute("hash") && _line.HasAttribute("position")) {
                            int hash = int.Parse(_line.GetAttribute("hash"));

                            string[] posArr = _line.GetAttribute("position").Split(',');

                            UpdatedChunks[hash] = new Vector3i(int.Parse(posArr[0]), int.Parse(posArr[1]), int.Parse(posArr[2]));
                        }
                    }
                    return true;
                }
                catch (XmlException e)
                {
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", FilePath, e.Message));
                    return false;
                }
            }
            return false;
        }


        public static void SaveChunk(int hashCode,Vector3i position) {

            if (!Utils.FileExists(FilePath)) {
                InitXML();
            }

            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(FilePath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", FilePath, e.Message));
                return;
            }
            XmlElement _root = xmlDoc.DocumentElement;

            XmlElement _child = xmlDoc.CreateElement("Chunk");
            _child.SetAttribute("hash", hashCode.ToString());
            _child.SetAttribute("position", position.x + "," + position.y + "," + position.z);
            _root.AppendChild(_child);

            xmlDoc.Save(FilePath);
        }

        protected static void ResetPrefabs()
        {
            if (!DisableResetPrefabs && LoadXML())
            {
                World world = GameManager.Instance.World;

                int cnt = 0;
                if (UpdatedChunks.Count() > 0)
                {
                    foreach (KeyValuePair<int, Vector3i> chunk in UpdatedChunks)
                    {
                            Vector3i areaStart = new Vector3i(chunk.Value.x, chunk.Value.y, chunk.Value.z);
                            SdtdConsole.Instance.ExecuteSync(string.Format("chunkreset {0} {1}", areaStart.x, areaStart.z), null);
                            cnt++;
                    }
                }
                Log.Out(string.Format("[SERVERTOOLS] Prefab_Preset : {0} prefab chunks reset.", cnt));


                File.Delete(FilePath);
                UpdatedChunks = new Dictionary<int, Vector3i>();
            }
        }

        public static void SetChunkUpdated(Vector3i position)
        {
            World _world = GameManager.Instance.World;
            if (_world.IsPositionWithinPOI(position.ToVector3(), POIProtection.Offset)) {

                IChunk chunk = _world.GetChunkFromWorldPos(position);

                int hashCode = chunk.GetHashCode();

                Vector3i chunkPos = chunk.GetWorldPos();

                if (!UpdatedChunks.ContainsKey(hashCode)) {
                    UpdatedChunks[hashCode] = chunkPos;
                    SaveChunk(hashCode,chunkPos);
                }
            }
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
