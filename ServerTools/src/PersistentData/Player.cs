using System;
using System.Runtime.Serialization;

namespace ServerTools
{
    [Serializable]
    public class Player
    {
        private readonly string steamId;
        [OptionalField]
        private DateTime lastgimme;
        [OptionalField]
        private DateTime lastkillme;
        [OptionalField]
        private string homeposition;
        [OptionalField]
        private DateTime lastsethome;

        public DateTime LastGimme
        {
            get
            {
                return lastgimme;
            }
            set
            {
                lastgimme = value;
            }
        }

        public DateTime LastKillme
        {
            get
            {
                return lastkillme;
            }
            set
            {
                lastkillme = value;
            }
        }

        public string HomePosition
        {
            get
            {
                return homeposition;
            }
            set
            {
                homeposition = value;
            }
        }

        public DateTime LastSetHome
        {
            get
            {
                return lastsethome;
            }
            set
            {
                lastsethome = value;
            }
        }

        public Player(string steamId)
        {
            this.steamId = steamId;
        }
    }
}