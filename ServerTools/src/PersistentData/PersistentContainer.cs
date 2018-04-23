using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServerTools
{
    [Serializable]
    public class PersistentContainer
    {
        private static string filepath = string.Format("{0}/ServerTools.bin", GameUtils.GetSaveGameDir());
        private Players players;
        private DateTime pollTime;
        private bool pollOpen;
        private int eventTime;
        private int eventPlayerCount;
        private int eventTeams;
        private int eventTimeOld;
        private int eventPlayerCountOld;
        private int eventTeamsOld;
        private int pollHours;
        private int pollYes;
        private int pollNo;
        private int lastPollHours;
        private int lastPollYes;
        private int lastPollNo;
        private List<string> eventSpawn;
        private List<string> eventRespawn;
        private List<string> eventSpawnOld;
        private List<string> eventRespawnOld;
        private string pollMessage;
        private string lastPollMessage;
        private string eventName;
        private string eventInvite;
        private string eventNameOld;
        private string eventInviteOld;
        private static PersistentContainer instance;

        public Players Players
        {
            get
            {
                if (players == null)
                {
                    players = new Players();
                }
                return players;
            }
        }

        public static PersistentContainer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PersistentContainer();
                }
                return instance;
            }
        }

        private PersistentContainer()
        {
        }

        public void Save()
        {
            Stream stream = File.Open(filepath, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, this);
            stream.Close();
        }

        public static bool Load()
        {
            if (File.Exists(filepath))
            {
                try
                {
                    PersistentContainer obj;
                    Stream stream = File.Open(filepath, FileMode.Open);
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    obj = (PersistentContainer)bFormatter.Deserialize(stream);
                    stream.Close();
                    instance = obj;
                    return true;
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Exception in PersistentContainer.Load: {0}", e));
                }
            }
            return false;
        }

        public bool PollOpen
        {
            get
            {
                return pollOpen;
            }
            set
            {
                pollOpen = value;
            }
        }

        public DateTime PollTime
        {
            get
            {
                return pollTime;
            }
            set
            {
                pollTime = value;
            }
        }

        public int EventTime
        {
            get
            {
                return eventTime;
            }
            set
            {
                eventTime = value;
            }
        }

        public int EventPlayerCount
        {
            get
            {
                return eventPlayerCount;
            }
            set
            {
                eventPlayerCount = value;
            }
        }

        public int EventTeams
        {
            get
            {
                return eventTeams;
            }
            set
            {
                eventTeams = value;
            }
        }

        public int EventTimeOld
        {
            get
            {
                return eventTimeOld;
            }
            set
            {
                eventTimeOld = value;
            }
        }

        public int EventPlayerCountOld
        {
            get
            {
                return eventPlayerCountOld;
            }
            set
            {
                eventPlayerCountOld = value;
            }
        }

        public int EventTeamsOld
        {
            get
            {
                return eventTeamsOld;
            }
            set
            {
                eventTeamsOld = value;
            }
        }

        public int PollHours
        {
            get
            {
                return pollHours;
            }
            set
            {
                pollHours = value;
            }
        }

        public int PollYes
        {
            get
            {
                return pollYes;
            }
            set
            {
                pollYes = value;
            }
        }

        public int PollNo
        {
            get
            {
                return pollNo;
            }
            set
            {
                pollNo = value;
            }
        }

        public int LastPollHours
        {
            get
            {
                return lastPollHours;
            }
            set
            {
                lastPollHours = value;
            }
        }

        public int LastPollYes
        {
            get
            {
                return lastPollYes;
            }
            set
            {
                lastPollYes = value;
            }
        }

        public int LastPollNo
        {
            get
            {
                return lastPollNo;
            }
            set
            {
                lastPollNo = value;
            }
        }

        public List<string> EventSpawn
        {
            get
            {
                return eventSpawn;
            }
            set
            {
                eventSpawn = value;
            }
        }

        public List<string> EventRespawn
        {
            get
            {
                return eventRespawn;
            }
            set
            {
                eventRespawn = value;
            }
        }

        public List<string> EventSpawnOld
        {
            get
            {
                return eventSpawnOld;
            }
            set
            {
                eventSpawnOld = value;
            }
        }

        public List<string> EventRespawnOld
        {
            get
            {
                return eventRespawnOld;
            }
            set
            {
                eventRespawnOld = value;
            }
        }

        public string PollMessage
        {
            get
            {
                return pollMessage;
            }
            set
            {
                pollMessage = value;
            }
        }

        public string LastPollMessage
        {
            get
            {
                return lastPollMessage;
            }
            set
            {
                lastPollMessage = value;
            }
        }

        public string EventName
        {
            get
            {
                return eventName;
            }
            set
            {
                eventName = value;
            }
        }

        public string EventInvite
        {
            get
            {
                return eventInvite;
            }
            set
            {
                eventInvite = value;
            }
        }

        public string EventNameOld
        {
            get
            {
                return eventNameOld;
            }
            set
            {
                eventNameOld = value;
            }
        }

        public string EventInviteOld
        {
            get
            {
                return eventInviteOld;
            }
            set
            {
                eventInviteOld = value;
            }
        }
    }
}