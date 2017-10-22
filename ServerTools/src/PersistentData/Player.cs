using System;
using System.Collections.Generic;
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
        private string homeposition;  // eventually once multihome support is widespread and data is migrated, swap the decorator to [NonSerialized].  Once that's widespread, remove this prop.
        [OptionalField]
        private Dictionary<string, string> homepositions;
        [OptionalField]
        private DateTime lastsethome;
        [OptionalField]
        private string clanname;
        [OptionalField]
        private bool isclanowner;
        [OptionalField]
        private bool isclanofficer;
        [OptionalField]
        private string invitedtoclan;
        [OptionalField]
        private string lastwhisper;
        [OptionalField]
        private bool ismuted;
        [OptionalField]
        private bool isjailed;
        [OptionalField]
        private bool isremovedfromjail = true;
        [OptionalField]
        private bool newspawntele;

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

        public Dictionary<string, string> HomePositions
        {
            get
            {
                if(homepositions == null)
                {
                    homepositions = new Dictionary<string, string>();
                }
                if(homeposition != null)
                {
                    homepositions["default"] = homeposition;
                    homeposition = null; 
                }
                return homepositions;
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

        public string ClanName
        {
            get
            {
                return clanname;
            }
            set
            {
                clanname = value;
            }
        }

        public bool IsClanOwner
        {
            get
            {
                return isclanowner;
            }
            set
            {
                isclanowner = value;
            }
        }

        public bool IsClanOfficer
        {
            get
            {
                return isclanofficer;
            }
            set
            {
                isclanofficer = value;
            }
        }

        public string InvitedToClan
        {
            get
            {
                return invitedtoclan;
            }
            set
            {
                invitedtoclan = value;
            }
        }

        public string LastWhisper
        {
            get
            {
                return lastwhisper;
            }
            set
            {
                lastwhisper = value;
            }
        }

        public bool IsMuted
        {
            get
            {
                return ismuted;
            }
            set
            {
                ismuted = value;
            }
        }

        public bool IsJailed
        {
            get
            {
                return isjailed;
            }
            set
            {
                isjailed = value;
            }
        }
        
        public bool IsRemovedFromJail
        {
            get
            {
                return isremovedfromjail;
            }
            set
            {
                isremovedfromjail = value;
            }
        }

        public bool NewSpawnTele
        {
            get
            {
                return newspawntele;
            }
            set
            {
                newspawntele = value;
            }
        }

        public Player(string steamId)
        {
            this.steamId = steamId;
        }
    }
}