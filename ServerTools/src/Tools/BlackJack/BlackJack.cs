using System;
using System.Collections.Generic;

namespace ServerTools
{
    class BlackJack
    {
        public static bool IsEnabled = false;
        public static int Buy_In = 300;
        public static string Directory = "", Command_blackjack = "blackjack";

        public static Dictionary<string, string[]> Player = new Dictionary<string, string[]>();
        public static Dictionary<string, List<string[]>> PlayerDeck = new Dictionary<string, List<string[]>>();

        private static List<string[]> Deck = new List<string[]>();
        private static readonly Random Random = new Random();

        public static void BuildDeck()
        {
            Deck.Add(new string[] { "1", "AceClub" });
            Deck.Add(new string[] { "1", "AceDiamond" });
            Deck.Add(new string[] { "1", "AceHeart" });
            Deck.Add(new string[] { "1", "AceSpade" });
            Deck.Add(new string[] { "2", "TwoClub" });
            Deck.Add(new string[] { "2", "TwoDiamond" });
            Deck.Add(new string[] { "2", "TwoHeart" });
            Deck.Add(new string[] { "2", "TwoSpade" });
            Deck.Add(new string[] { "3", "ThreeClub" });
            Deck.Add(new string[] { "3", "ThreeDiamond" });
            Deck.Add(new string[] { "3", "ThreeHeart" });
            Deck.Add(new string[] { "3", "ThreeSpade" });
            Deck.Add(new string[] { "4", "FourClub" });
            Deck.Add(new string[] { "4", "FourDiamond" });
            Deck.Add(new string[] { "4", "FourHeart" });
            Deck.Add(new string[] { "4", "FourSpade" });
            Deck.Add(new string[] { "5", "FiveClub" });
            Deck.Add(new string[] { "5", "FiveDiamond" });
            Deck.Add(new string[] { "5", "FiveHeart" });
            Deck.Add(new string[] { "5", "FiveSpade" });
            Deck.Add(new string[] { "6", "SixClub" });
            Deck.Add(new string[] { "6", "SixDiamond" });
            Deck.Add(new string[] { "6", "SixHeart" });
            Deck.Add(new string[] { "6", "SixSpade" });
            Deck.Add(new string[] { "7", "SevenClub" });
            Deck.Add(new string[] { "7", "SevenDiamond" });
            Deck.Add(new string[] { "7", "SevenHeart" });
            Deck.Add(new string[] { "7", "SevenSpade" });
            Deck.Add(new string[] { "8", "EightClub" });
            Deck.Add(new string[] { "8", "EightDiamond" });
            Deck.Add(new string[] { "8", "EightHeart" });
            Deck.Add(new string[] { "8", "EightSpade" });
            Deck.Add(new string[] { "9", "NineClub" });
            Deck.Add(new string[] { "9", "NineDiamond" });
            Deck.Add(new string[] { "9", "NineHeart" });
            Deck.Add(new string[] { "9", "NineSpade" });
            Deck.Add(new string[] { "10", "TenClub" });
            Deck.Add(new string[] { "10", "TenDiamond" });
            Deck.Add(new string[] { "10", "TenHeart" });
            Deck.Add(new string[] { "10", "TenSpade" });
            Deck.Add(new string[] { "10", "JackClub" });
            Deck.Add(new string[] { "10", "JackDiamond" });
            Deck.Add(new string[] { "10", "JackHeart" });
            Deck.Add(new string[] { "10", "JackSpade" });
            Deck.Add(new string[] { "10", "QueenClub" });
            Deck.Add(new string[] { "10", "QueenDiamond" });
            Deck.Add(new string[] { "10", "QueenHeart" });
            Deck.Add(new string[] { "10", "QueenSpade" });
            Deck.Add(new string[] { "10", "KingClub" });
            Deck.Add(new string[] { "10", "KingDiamond" });
            Deck.Add(new string[] { "10", "KingHeart" });
            Deck.Add(new string[] { "10", "KingSpade" });
        }

        public static void Exec(ClientInfo _cInfo)
        {
            if (Player.ContainsKey(_cInfo.ip) && Player[_cInfo.ip][0] == _cInfo.CrossplatformId.CombinedString)
            {
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserBlackJack", true));
                return;
            }
            Player.Add(_cInfo.ip, new string[] { _cInfo.CrossplatformId.CombinedString, "" });
            Timers.Blackjack_SingleUseTimer(_cInfo.ip);
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserBlackJack", true));
        }

        public static void RemovePlayer(string _ip)
        {
            if (Player.ContainsKey(_ip))
            {
                Player.Remove(_ip);
            }
        }

        public static void NewGame()
        {

        }
    }
}
