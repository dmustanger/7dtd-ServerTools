using Platform.Steam;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    public class GeneralOperations
    {
        public static bool Running = false, Shutdown_Initiated = false, No_Vehicle_Pickup = false, ThirtySeconds = false, 
            No_Currency = false, Debug = false, Allow_Bicycle = false, Encryption_Off = false;
        public static int Jail_Violation = 4, Kill_Violation = 6, Kick_Violation = 8, Ban_Violation = 10, Player_Killing_Mode = 0;
        public static string AppPath, Currency_Item, XPathDir, Command_expire = "expire", Command_commands = "commands", Command_overlay = "overlay";
        public static string lastOutput = "";
        public static int LandClaimDurability = 0;

        public static List<string> ActiveLog = new List<string>();
        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        public static Dictionary<int, int> PvEViolations = new Dictionary<int, int>();
        public static List<ClientInfo> NewPlayerQue = new List<ClientInfo>();
        public static List<ClientInfo> BlockChatCommands = new List<ClientInfo>();

        public static readonly string AlphaNumSet = "jJkqQr9Kl3wXAbyYz0ZLmFpPRsMn5NoO6dDe1EfStaBc2CgGhH7iITu4U8vWxV", NumSet = "1928374650";
        public static readonly char[] InvalidPrefix = new char[] { '!', '@', '#', '$', '%', '&', '/', '\\' };

        public static DateTime StartTime;

        private static EntityPlayer player;
        private static Entity entity;
        private static PlatformUserIdentifierAbs uId;
        private static PersistentPlayerList persistentPlayerList, persistentPlayerListTwo, persistentPlayerListThree, persistentPlayerListFour;
        private static PersistentPlayerData persistentPlayerData, persistentPlayerDataTwo, ppdThree, ppdFour;
        private static PlayerDataFile playerDatafile, playerDatafileTwo, playerDatafileThree;
        private static ItemValue itemValue;

        private static List<ClientInfo> clientList;
        private static List<Entity> entityList;

        //private static string Debug = string.Format("{0}/{1}", API.ConfigPath, DebugFile);
        //private const string DebugFile = "Debug.txt";

        public static void CreateCustomXUi()
        {
            if (!string.IsNullOrEmpty(XPathDir))
            {
                if (File.Exists(XPathDir + "items.xml"))
                {
                    File.Delete(XPathDir + "items.xml");
                }
                if (!File.Exists(XPathDir + "items.xml"))
                {
                    using (StreamWriter sw = new StreamWriter(XPathDir + "items.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("  <set xpath=\"/items/item[@name='{0}']/property[@name='Tags']/@value\">dukes,currency</set>", Wallet.Item_Name);
                        sw.WriteLine("  <!-- ..... Wallet and Bank currency ^ ..... -->");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                        sw.Flush();
                        sw.Close();
                    }
                }
                if (File.Exists(XPathDir + "gameevents.xml"))
                {
                    File.Delete(XPathDir + "gameevents.xml");
                }
                if (!File.Exists(XPathDir + "gameevents.xml"))
                {
                    using (StreamWriter sw = new StreamWriter(XPathDir + "gameevents.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("<append xpath=\"/gameevents\">");
                        sw.WriteLine();
                        sw.WriteLine("  <action_sequence name=\"action_admin\">");
                        sw.WriteLine("      <action class=\"RemoveItems\">");
                        sw.WriteLine("          <property name=\"items_location\" value=\"Backpack\" />");
                        sw.WriteLine("          <property name=\"items_tags\" value=\"admin\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("      <action class=\"PlaySound\">");
                        sw.WriteLine("          <property name=\"sound\" value=\"ui_trader_purchase\" />");
                        sw.WriteLine("          <property name=\"inside_head\" value=\"true\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("  </action_sequence>");
                        sw.WriteLine();
                        sw.WriteLine("  <action_sequence name=\"action_dukes\">");
                        sw.WriteLine("      <action class=\"RemoveItems\">");
                        sw.WriteLine("          <property name=\"items_location\" value=\"Backpack\" />");
                        sw.WriteLine("          <property name=\"items_tags\" value=\"dukes\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("      <action class=\"PlaySound\">");
                        sw.WriteLine("          <property name=\"sound\" value=\"ui_trader_purchase\" />");
                        sw.WriteLine("          <property name=\"inside_head\" value=\"true\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("  </action_sequence>");
                        sw.WriteLine();
                        sw.WriteLine("  <action_sequence name=\"action_currency\">");
                        sw.WriteLine("      <action class=\"RemoveItems\">");
                        sw.WriteLine("          <property name=\"items_location\" value=\"Backpack\" />");
                        sw.WriteLine("          <property name=\"items_tags\" value=\"currency\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("      <action class=\"PlaySound\">");
                        sw.WriteLine("          <property name=\"sound\" value=\"ui_trader_purchase\" />");
                        sw.WriteLine("          <property name=\"inside_head\" value=\"true\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("  </action_sequence>");
                        sw.WriteLine();
                        sw.WriteLine("  <action_sequence name=\"action_eject\">");
                        sw.WriteLine("      <action class=\"EjectFromVehicle\">");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("      <action class=\"PlaySound\">");
                        sw.WriteLine("          <property name=\"sound\" value=\"twitch_vehicle_overlay\" />");
                        sw.WriteLine("          <property name=\"inside_head\" value=\"true\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("  </action_sequence>");
                        sw.WriteLine();
                        sw.WriteLine("  <action_sequence name=\"action_remove_bedrolls\">");
                        sw.WriteLine("      <action class=\"ResetPlayerData\">");
                        sw.WriteLine("          <property name=\"reset_levels\" value=\"false\" />");
                        sw.WriteLine("          <property name=\"reset_books\" value=\"false\" />");
                        sw.WriteLine("          <property name=\"reset_crafting\" value=\"false\" />");
                        sw.WriteLine("          <property name=\"reset_skills\" value=\"false\" />");
                        sw.WriteLine("          <property name=\"remove_landclaims\" value=\"false\" />");
                        sw.WriteLine("          <property name=\"remove_bedroll\" value=\"true\" />");
                        sw.WriteLine("      </action>");
                        sw.WriteLine();
                        sw.WriteLine("  </action_sequence>");
                        sw.WriteLine();
                        sw.WriteLine("</append>");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                        sw.Flush();
                        sw.Close();
                    }
                }
                SetWindows();
                SetBuffs();
                if (File.Exists(XPathDir + "XUi/xui.xml"))
                {
                    File.Delete(XPathDir + "XUi/xui.xml");
                }
                if (!File.Exists(XPathDir + "XUi/xui.xml"))
                {
                    using (StreamWriter sw = new StreamWriter(XPathDir + "XUi/xui.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("<append xpath=\"/xui/ruleset\">");
                        sw.WriteLine();
                        sw.WriteLine("  <window_group name=\"browserDiscord\">");
                        sw.WriteLine("      <window name=\"browserDiscord\" />");
                        sw.WriteLine("  </window_group>");
                        sw.WriteLine();
                        sw.WriteLine("  <window_group name=\"browserVote\">");
                        sw.WriteLine("      <window name=\"browserVote\" />");
                        sw.WriteLine("  </window_group>");
                        sw.WriteLine();
                        sw.WriteLine("  <window_group name=\"browserRio\">");
                        sw.WriteLine("      <window name=\"browserRio\" />");
                        sw.WriteLine("  </window_group>");
                        sw.WriteLine();
                        sw.WriteLine("  <window_group name=\"browserShop\">");
                        sw.WriteLine("      <window name=\"browserShop\" />");
                        sw.WriteLine("  </window_group>");
                        sw.WriteLine();
                        sw.WriteLine("  <window_group name=\"browserAuction\">");
                        sw.WriteLine("      <window name=\"browserAuction\" />");
                        sw.WriteLine("  </window_group>");
                        sw.WriteLine();
                        sw.WriteLine("  <window_group name=\"browserDonation\">");
                        sw.WriteLine("      <window name=\"browserDonation\" />");
                        sw.WriteLine("  </window_group>");
                        sw.WriteLine();
                        sw.WriteLine("</append>");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                    }
                }
                if (File.Exists(XPathDir + "blocks.xml"))
                {
                    File.Delete(XPathDir + "blocks.xml");
                }
                if (File.Exists(XPathDir + "recipes.xml"))
                {
                    File.Delete(XPathDir + "recipes.xml");
                }
            }
        }

        public static void SetWindows()
        {
            if (File.Exists(XPathDir + "XUi/windows.xml"))
            {
                File.Delete(XPathDir + "XUi/windows.xml");
            }
            if (!File.Exists(XPathDir + "XUi/windows.xml"))
            {
                using (StreamWriter sw = new StreamWriter(XPathDir + "XUi/windows.xml", false, Encoding.UTF8))
                {
                    sw.WriteLine("<configs>");
                    sw.WriteLine();
                    sw.WriteLine("<append xpath=\"/windows\">");
                    sw.WriteLine();
                    sw.WriteLine("  <window name=\"browserDiscord\" controller=\"ServerInfo\">");
                    sw.WriteLine("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                    sw.WriteLine("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Discord Link\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                    sw.WriteLine("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                    sw.WriteLine("          <label name=\"ServerDescription\" />");
                    sw.WriteLine("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", DiscordLink.Link);
                    sw.WriteLine("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                    sw.WriteLine("          <sprite depth=\"4\" name=\"microphoneIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_mic\" />");
                    sw.WriteLine("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("  </window>");
                    sw.WriteLine();
                    sw.WriteLine("  <window name=\"browserVote\" controller=\"ServerInfo\">");
                    sw.WriteLine("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                    sw.WriteLine("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Voting Site\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                    sw.WriteLine("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                    sw.WriteLine("          <label name=\"ServerDescription\" />");
                    sw.WriteLine("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", Voting.Link);
                    sw.WriteLine("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                    sw.WriteLine("          <sprite depth=\"4\" name=\"computerIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_computer\" />");
                    sw.WriteLine("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("  </window>");
                    sw.WriteLine();
                    sw.WriteLine("  <window name=\"browserRio\" controller=\"ServerInfo\">");
                    sw.WriteLine("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                    sw.WriteLine("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Roll It Out\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                    sw.WriteLine("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                    sw.WriteLine("          <label name=\"ServerDescription\" />");
                    sw.WriteLine("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"http://{0}:{1}/rio.html\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", WebAPI.BaseAddress, WebAPI.Port);
                    sw.WriteLine("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                    sw.WriteLine("          <sprite depth=\"4\" name=\"coinIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_coin\" />");
                    sw.WriteLine("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("  </window>");
                    sw.WriteLine();
                    sw.WriteLine("  <window name=\"browserShop\" controller=\"ServerInfo\">");
                    sw.WriteLine("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                    sw.WriteLine("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Shop\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                    sw.WriteLine("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                    sw.WriteLine("          <label name=\"ServerDescription\" />");
                    sw.WriteLine("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"http://{0}:{1}/shop.html\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", WebAPI.BaseAddress, WebAPI.Port);
                    sw.WriteLine("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                    sw.WriteLine("          <sprite depth=\"4\" name=\"shoppingCartIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_shopping_cart\" />");
                    sw.WriteLine("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("  </window>");
                    sw.WriteLine();
                    sw.WriteLine("  <window name=\"browserAuction\" controller=\"ServerInfo\">");
                    sw.WriteLine("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                    sw.WriteLine("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Auction\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                    sw.WriteLine("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                    sw.WriteLine("          <label name=\"ServerDescription\" />");
                    sw.WriteLine("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"http://{0}:{1}/auction.html\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", WebAPI.BaseAddress, WebAPI.Port);
                    sw.WriteLine("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                    sw.WriteLine("          <sprite depth=\"4\" name=\"shoppingCartIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_shopping_cart\" />");
                    sw.WriteLine("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("  </window>");
                    sw.WriteLine();
                    sw.WriteLine("  <window name=\"browserIMap\" controller=\"ServerInfo\">");
                    sw.WriteLine("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                    sw.WriteLine("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Interactive Map\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                    sw.WriteLine("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                    sw.WriteLine("          <label name=\"ServerDescription\" />");
                    sw.WriteLine("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"http://{0}:{1}/imap.html\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", WebAPI.BaseAddress, WebAPI.Port);
                    sw.WriteLine("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                    sw.WriteLine("          <sprite depth=\"4\" name=\"mapIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_map\" />");
                    sw.WriteLine("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("  </window>");
                    sw.WriteLine();
                    sw.WriteLine("  <window name=\"browserDonation\" controller=\"ServerInfo\">");
                    sw.WriteLine("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                    sw.WriteLine("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Donation Link\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                    sw.WriteLine("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                    sw.WriteLine("          <label name=\"ServerDescription\" />");
                    sw.WriteLine("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", DonationLink.Link);
                    sw.WriteLine("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                    sw.WriteLine("          <sprite depth=\"4\" name=\"coinIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_coin\" />");
                    sw.WriteLine("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                    sw.WriteLine("      </panel>");
                    sw.WriteLine("  </window>");
                    sw.WriteLine();
                    sw.WriteLine("</append>");
                    sw.WriteLine();
                    sw.WriteLine("</configs>");
                }
            }
        }

        public static void SetBuffs()
        {
            if (File.Exists(XPathDir + "buffs.xml"))
            {
                File.Delete(XPathDir + "buffs.xml");
            }
            if (!File.Exists(XPathDir + "buffs.xml"))
            {
                using (StreamWriter sw = new StreamWriter(XPathDir + "buffs.xml", false, Encoding.UTF8))
                {
                    sw.WriteLine("<configs>");
                    sw.WriteLine();
                    sw.WriteLine("<append xpath=\"/buffs\">");
                    sw.WriteLine();
                    sw.WriteLine("<buff name=\"pve_zone\" name_key=\"PvE_Zone\" description_key=\"You are inside a PvE area\" tooltip_key=\"PvE_Zone\" icon=\"ui_game_symbol_twitch_no_ranged\" icon_color=\"0,102,153\">");
                    sw.WriteLine("	<display_value_key value=\"PvE Zone\"/>");
                    sw.WriteLine("	<display_value value=\"xxx\"/>");
                    sw.WriteLine("	<stack_type value=\"replace\"/>");
                    sw.WriteLine("	<effect_group>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pve_zone\"/>");
                    sw.WriteLine("	</effect_group>");
                    sw.WriteLine("</buff>");
                    sw.WriteLine();
                    sw.WriteLine("<buff name=\"pvp_ally_zone\" name_key=\"PvP_Ally_Zone\" description_key=\"You are inside a area with active damage for allies\" tooltip_key=\"PvP_Ally_Zone\" icon=\"ui_game_symbol_twitch_no_ranged\" icon_color=\"204,204,0\">");
                    sw.WriteLine("	<display_value_key value=\"Ally PvP Zone\"/>");
                    sw.WriteLine("	<display_value value=\"xxx\"/>");
                    sw.WriteLine("	<stack_type value=\"replace\"/>");
                    sw.WriteLine("	<effect_group>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_ally_zone\"/>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfPrimaryActionRayHit\" target=\"other\" action=\"AddBuff\" buff=\"pvp_ally_damage\">");
                    sw.WriteLine("			<requirement name=\"EntityTagCompare\" target=\"other\" tags=\"player\"/>");
                    sw.WriteLine("		</triggered_effect>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfSecondaryActionRayHit\" target=\"other\" action=\"AddBuff\" buff=\"pvp_ally_damage\">");
                    sw.WriteLine("			<requirement name=\"EntityTagCompare\" target=\"other\" tags=\"player\"/>");
                    sw.WriteLine("		</triggered_effect>");
                    sw.WriteLine("	</effect_group>");
                    sw.WriteLine("</buff>");
                    sw.WriteLine();
                    sw.WriteLine("<buff name=\"pvp_stranger_zone\" name_key=\"PvP_Stranger_Zone\" description_key=\"You are inside a area with active damage for strangers\" tooltip_key=\"PvP_Stranger_Zone\" icon=\"ui_game_symbol_twitch_no_ranged\" icon_color=\"255,153,0\">");
                    sw.WriteLine("	<display_value_key value=\"Stranger PvP Zone\"/>");
                    sw.WriteLine("	<display_value value=\"xxx\"/>");
                    sw.WriteLine("	<stack_type value=\"replace\"/>");
                    sw.WriteLine("	<effect_group>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_stranger_zone\"/>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfPrimaryActionRayHit\" target=\"other\" action=\"AddBuff\" buff=\"pvp_stranger_damage\">");
                    sw.WriteLine("			<requirement name=\"EntityTagCompare\" target=\"other\" tags=\"player\"/>");
                    sw.WriteLine("		</triggered_effect>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfSecondaryActionRayHit\" target=\"other\" action=\"AddBuff\" buff=\"pvp_stranger_damage\">");
                    sw.WriteLine("			<requirement name=\"EntityTagCompare\" target=\"other\" tags=\"player\"/>");
                    sw.WriteLine("		</triggered_effect>");
                    sw.WriteLine("	</effect_group>");
                    sw.WriteLine("</buff>");
                    sw.WriteLine();
                    sw.WriteLine("<buff name=\"pvp_zone\" name_key=\"PvP_Zone\" description_key=\"You are inside a area with active damage for everyone\" tooltip_key=\"PvP_Zone\" icon=\"ui_game_symbol_twitch_no_ranged\" icon_color=\"204,0,0\">");
                    sw.WriteLine("	<display_value_key value=\"PvP Zone\"/>");
                    sw.WriteLine("	<display_value value=\"xxx\"/>");
                    sw.WriteLine("	<stack_type value=\"replace\"/>");
                    sw.WriteLine("	<effect_group>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_zone\"/>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfPrimaryActionRayHit\" target=\"other\" action=\"AddBuff\" buff=\"pvp_damage\">");
                    sw.WriteLine("			<requirement name=\"EntityTagCompare\" target=\"other\" tags=\"player\"/>");
                    sw.WriteLine("		</triggered_effect>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfSecondaryActionRayHit\" target=\"other\" action=\"AddBuff\" buff=\"pvp_damage\">");
                    sw.WriteLine("			<requirement name=\"EntityTagCompare\" target=\"other\" tags=\"player\"/>");
                    sw.WriteLine("		</triggered_effect>");
                    sw.WriteLine("	</effect_group>");
                    sw.WriteLine("</buff>");
                    sw.WriteLine();
                    sw.WriteLine("<buff name=\"pvp_ally_damage\" hidden=\"true\">");
                    sw.WriteLine("	<display_value_key value=\"PvP Ally Damage\"/>");
                    sw.WriteLine("	<stack_type value=\"replace\"/>");
                    sw.WriteLine("	<duration value=\"1\"/>");
                    sw.WriteLine("	<effect_group>");
                    sw.WriteLine("      <requirements>");
                    sw.WriteLine("			<requirement name=\"!HasBuff\" buff=\"pvp_ally_zone\"/>");
                    sw.WriteLine("	    </requirements>");
                    sw.WriteLine("		<passive_effect name=\"GeneralDamageResist\" operation=\"base_add\" value=\"1\"/>");
                    sw.WriteLine("		<passive_effect name=\"ElementalDamageResist\" operation=\"perc_add\" value=\"200\"/>");
                    sw.WriteLine("		<passive_effect name=\"PhysicalDamageResist\" operation=\"perc_add\" value=\"200\"/>");
                    sw.WriteLine("		<passive_effect name=\"ExplosionIncomingDamage\" operation=\"perc_add\" value=\"-1\"/>");
                    sw.WriteLine("		<passive_effect name=\"HealthLoss\" operation=\"perc_add\" value=\"-1\"/>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_ally_damage\"/>");
                    sw.WriteLine("	</effect_group>");
                    sw.WriteLine("</buff>");
                    sw.WriteLine();
                    sw.WriteLine("<buff name=\"pvp_stranger_damage\" hidden=\"true\">");
                    sw.WriteLine("	<display_value_key value=\"PvP Stranger Damage\"/>");
                    sw.WriteLine("	<stack_type value=\"replace\"/>");
                    sw.WriteLine("	<duration value=\"1\"/>");
                    sw.WriteLine("	<effect_group>");
                    sw.WriteLine("      <requirements>");
                    sw.WriteLine("			<requirement name=\"!HasBuff\" buff=\"pvp_stranger_zone\"/>");
                    sw.WriteLine("	    </requirements>");
                    sw.WriteLine("		<passive_effect name=\"GeneralDamageResist\" operation=\"base_add\" value=\"1\"/>");
                    sw.WriteLine("		<passive_effect name=\"ElementalDamageResist\" operation=\"base_add\" value=\"200\"/>");
                    sw.WriteLine("		<passive_effect name=\"PhysicalDamageResist\" operation=\"base_add\" value=\"200\"/>");
                    sw.WriteLine("		<passive_effect name=\"ExplosionIncomingDamage\" operation=\"perc_add\" value=\"-1\"/>");
                    sw.WriteLine("		<passive_effect name=\"HealthLoss\" operation=\"perc_add\" value=\"-1\"/>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_stranger_damage\"/>");
                    sw.WriteLine("	</effect_group>");
                    sw.WriteLine("</buff>");
                    sw.WriteLine();
                    sw.WriteLine("<buff name=\"pvp_damage\" hidden=\"true\">");
                    sw.WriteLine("	<display_value_key value=\"PvP Damage\"/>");
                    sw.WriteLine("	<stack_type value=\"replace\"/>");
                    sw.WriteLine("	<duration value=\"1\"/>");
                    sw.WriteLine("	<effect_group>");
                    sw.WriteLine("      <requirements>");
                    sw.WriteLine("			<requirement name=\"!HasBuff\" buff=\"pvp_zone\"/>");
                    sw.WriteLine("	    </requirements>");
                    sw.WriteLine("		<passive_effect name=\"GeneralDamageResist\" operation=\"base_add\" value=\"1\"/>");
                    sw.WriteLine("		<passive_effect name=\"ElementalDamageResist\" operation=\"base_add\" value=\"200\"/>");
                    sw.WriteLine("		<passive_effect name=\"PhysicalDamageResist\" operation=\"base_add\" value=\"200\"/>");
                    sw.WriteLine("		<passive_effect name=\"ExplosionIncomingDamage\" operation=\"perc_add\" value=\"-1\"/>");
                    sw.WriteLine("		<passive_effect name=\"HealthLoss\" operation=\"perc_add\" value=\"-1\"/>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_damage\"/>");
                    sw.WriteLine("	</effect_group>");
                    sw.WriteLine("</buff>");
                    sw.WriteLine();
                    sw.WriteLine(string.Format("<buff name=\"region_reset\" name_key=\"Region_reset\" description_key=\"The region you are in will reset. Building here is NOT recommended\" icon=\"{0}\" icon_color=\"255, 153, 51\">", RegionReset.Icon));
                    sw.WriteLine("	<display_value_key value=\"     Region will reset\"/>");
                    sw.WriteLine("	<display_value value=\"xxx\"/>");
                    sw.WriteLine("	<stack_type value=\"replace\"/>");
                    sw.WriteLine("	<effect_group>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"region_reset\"/>");
                    sw.WriteLine("	</effect_group>");
                    sw.WriteLine("</buff>");
                    sw.WriteLine();
                    sw.WriteLine(string.Format("<buff name=\"chunk_reset\" name_key=\"Chunk_reset\" description_key=\"The chunk you are in will reset. Building here is NOT recommended\" icon=\"{0}\" icon_color=\"204, 102, 0\">", ChunkReset.Icon));
                    sw.WriteLine("	<display_value_key value=\"     Chunk will reset\"/>");
                    sw.WriteLine("	<display_value value=\"xxx\"/>");
                    sw.WriteLine("	<stack_type value=\"replace\"/>");
                    sw.WriteLine("	<effect_group>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"chunk_reset\"/>");
                    sw.WriteLine("	</effect_group>");
                    sw.WriteLine("</buff>");
                    sw.WriteLine();
                    sw.WriteLine("<buff name=\"block_protection\" name_key=\"Block_protection\" hidden=\"true\" >");
                    sw.WriteLine("	<stack_type value=\"replace\"/>");
                    sw.WriteLine("	<effect_group>");
                    sw.WriteLine("		<passive_effect name=\"DisableItem\" operation=\"base_set\" value=\"1\" tags=\"melee\"/>");
                    sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"block_protection\"/>");
                    sw.WriteLine("	</effect_group>");
                    sw.WriteLine("</buff>");
                    sw.WriteLine();
                    sw.WriteLine("</append>");
                    sw.WriteLine();
                    sw.WriteLine("</configs>");
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public static void GetProtectionLevel()
        {
            LandClaimDurability = GamePrefs.GetInt(EnumGamePrefs.LandClaimOnlineDurabilityModifier);
        }

        public static void CheckArea()
        {
            if (!Running)
            {
                ThreadManager.AddSingleTask(delegate (ThreadManager.TaskInfo _taskInfo)
                {
                    Running = true;
                    clientList = ClientList();
                    if (clientList == null || clientList.Count < 1)
                    {
                        Running = false;
                        return;
                    }
                    ClientInfo cInfo;
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        cInfo = clientList[i];
                        if (cInfo == null || !cInfo.loginDone || TeleportDetector.Omissions.Contains(cInfo.entityId))
                        {
                            continue;
                        }
                        player = GetEntityPlayer(cInfo.entityId);
                        if (player == null || !player.IsSpawned() || player.IsDead() || player.position == null)
                        {
                            continue;
                        }
                        if (Zones.IsEnabled && Zones.ZoneList.Count > 0)
                        {
                            Zones.ZoneCheck(cInfo, player);
                        }
                        if (RegionReset.IsEnabled && RegionReset.Regions.Count > 0)
                        {
                            RegionReset.IsResetRegion(cInfo, player);
                        }
                        if (ChunkReset.IsEnabled && ChunkReset.Chunks.Count > 0)
                        {
                            ChunkReset.IsResetChunk(cInfo, player);
                        }
                        if (ProtectedZones.IsEnabled)
                        {
                            ProtectedZones.InsideProtectedZone(cInfo, player);
                        }
                    }

                    if (!IsBloodmoon())
                    {
                        entityList = GameManager.Instance.World.Entities.list;
                        if (entityList == null || entityList.Count < 1)
                        {
                            Running = false;
                            return;
                        }
                        for (int i = 0; i < entityList.Count; i++)
                        {
                            entity = entityList[i];
                            if (entity == null || !entity.IsSpawned() || entity.IsDead() || entity.IsMarkedForUnload())
                            {
                                continue;
                            }
                            if (entity is EntityZombie || entity is EntityEnemyAnimal)
                            {
                                if (Lobby.IsEnabled && Lobby.IsLobby(entity.position))
                                {
                                    entity.MarkToUnload();
                                }
                                else if (Market.IsEnabled && Market.IsMarket(entity.position))
                                {
                                    entity.MarkToUnload();
                                }
                            }
                        }
                    }
                    Running = false;
                });
            }
        }

        public static void SessionTime(ClientInfo _cInfo)
        {
            if (!Session.ContainsKey(_cInfo.CrossplatformId.CombinedString))
            {
                Session.Add(_cInfo.CrossplatformId.CombinedString, DateTime.Now);
            }
        }

        public static bool IsBloodmoon()
        {
            if (GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
            {
                return true;
            }
            return false;
        }

        public static List<ClientInfo> ClientList()
        {
            if (ConnectionManager.Instance.Clients != null && ConnectionManager.Instance.Clients.Count > 0)
            {
                return ConnectionManager.Instance.Clients.List.ToList();
            }
            return null;
        }

        public static ClientInfo GetClientInfoFromNameOrId(string _id)
        {
            ClientInfo cInfo = ConnectionManager.Instance.Clients.GetForNameOrId(_id);
            if (cInfo != null)
            {
                return cInfo;
            }
            else if (int.TryParse(_id, out int entityId))
            {
                cInfo = ConnectionManager.Instance.Clients.ForEntityId(entityId);
                if (cInfo != null)
                {
                    return cInfo;
                }
            }
            return null;
        }

        public static ClientInfo GetClientInfoFromEntityId(int _entityId)
        {
            return ConnectionManager.Instance.Clients.ForEntityId(_entityId);
        }

        public static ClientInfo GetClientInfoFromUId(PlatformUserIdentifierAbs _uId)
        {
            return ConnectionManager.Instance.Clients.ForUserId(_uId);
        }

        public static ClientInfo GetClientInfoFromName(string _name)
        {
            return ConnectionManager.Instance.Clients.GetForPlayerName(_name);
        }

        public static List<EntityPlayer> ListPlayers()
        {
            return GameManager.Instance.World.Players.list;
        }

        public static EntityPlayer GetEntityPlayer(int _id)
        {
            GameManager.Instance.World.Players.dict.TryGetValue(_id, out EntityPlayer entityPlayer);
            return entityPlayer;
        }

        public static Entity GetEntity(int _id)
        {
            GameManager.Instance.World.Entities.dict.TryGetValue(_id, out Entity entity);
            return entity;
        }

        public static EntityZombie GetZombie(int _id)
        {
            GameManager.Instance.World.Entities.dict.TryGetValue(_id, out Entity entity);
            if (entity != null && entity is EntityZombie)
            {
                return entity as EntityZombie;
            }
            return null;
        }

        public static EntityAnimal GetAnimal(int _id)
        {
            GameManager.Instance.World.Entities.dict.TryGetValue(_id, out Entity entity);
            if (entity != null && entity is EntityAnimal)
            {
                return entity as EntityAnimal;
            }
            return null;
        }

        public static PersistentPlayerList GetPersistentPlayerList()
        {
            return GameManager.Instance.persistentPlayers;
        }

        public static PersistentPlayerData GetPersistentPlayerDataFromId(string _id)
        {
            uId = GetPlatformUserFromNameOrId(_id);
            if (uId != null)
            {
                persistentPlayerList = GetPersistentPlayerList();
                if (persistentPlayerList != null)
                {
                    ppdThree = persistentPlayerList.GetPlayerData(uId);
                    if (ppdThree != null)
                    {
                        return ppdThree;
                    }
                }
            }
            return null;
        }

        public static PersistentPlayerData GetPersistentPlayerDataFromUId(PlatformUserIdentifierAbs _uId)
        {
            persistentPlayerListTwo = GetPersistentPlayerList();
            if (persistentPlayerListTwo != null)
            {
                ppdFour = persistentPlayerList.GetPlayerData(_uId);
                if (ppdFour != null)
                {
                    return ppdFour;
                }
            }
            return null;
        }

        public static PersistentPlayerData GetPersistentPlayerDataFromEntityId(int _entityId)
        {
            persistentPlayerListThree = GetPersistentPlayerList();
            if (persistentPlayerListThree != null)
            {
                persistentPlayerData = persistentPlayerList.GetPlayerDataFromEntityID(_entityId);
                if (persistentPlayerData != null)
                {
                    return persistentPlayerData;
                }
            }
            return null;
        }

        public static PlayerDataFile GetPlayerDataFileFromUId(PlatformUserIdentifierAbs _uId)
        {
            playerDatafile = new PlayerDataFile();
            playerDatafile.Load(GameIO.GetPlayerDataDir(), _uId.CombinedString);
            if (playerDatafile != null)
            {
                return playerDatafile;
            }
            return null;
        }

        public static PlayerDataFile GetPlayerDataFileFromEntityId(int _entityId)
        {
            persistentPlayerDataTwo = GetPersistentPlayerDataFromEntityId(_entityId);
            if (persistentPlayerDataTwo != null)
            {
                playerDatafileTwo = new PlayerDataFile();
                playerDatafileTwo.Load(GameIO.GetPlayerDataDir(), persistentPlayerDataTwo.UserIdentifier.CombinedString.Trim());
                if (playerDatafileTwo != null)
                {
                    return playerDatafileTwo;
                }
            }
            return null;
        }

        public static PlayerDataFile GetPlayerDataFileFromId(string _id)
        {
            if (ConsoleHelper.ParseParamPartialNameOrId(_id, out PlatformUserIdentifierAbs platformUserIdentifierAbs, out ClientInfo clientInfo, true) == 1 && platformUserIdentifierAbs != null)
            {
                playerDatafileThree = GetPlayerDataFileFromUId(platformUserIdentifierAbs);
                if (playerDatafileThree != null)
                {
                    return playerDatafileThree;
                }
            }
            return null;
        }

        public static PlatformUserIdentifierAbs GetPlatformUserFromNameOrId(string _id)
        {
            if (ConsoleHelper.ParseParamPartialNameOrId(_id, out PlatformUserIdentifierAbs platformUserIdentifierAbs, out ClientInfo clientInfo, true) == 1 && platformUserIdentifierAbs != null)
            {
                return platformUserIdentifierAbs;
            }
            return null;
        }

        public static void RemovePersistentPlayerData(string _id)
        {
            persistentPlayerListFour = GetPersistentPlayerList();
            if (persistentPlayerListFour != null)
            {
                PlatformUserIdentifierAbs uId = GetPlatformUserFromNameOrId(_id);
                if (uId != null)
                {
                    if (persistentPlayerListFour.Players.ContainsKey(uId))
                    {
                        persistentPlayerListFour.Players.Remove(uId);
                        SavePersistentPlayerDataXML();
                    }
                }
            }
        }

        public static void SavePlayerDataFile(string _id, PlayerDataFile _playerDataFile)
        {
            _playerDataFile.Save(GameIO.GetPlayerDataDir(), _id.Trim());
            ClientInfo cInfo = GetClientInfoFromNameOrId(_id);
            if (cInfo != null)
            {
                ModEvents.SavePlayerData.Invoke(cInfo, _playerDataFile);
            }
        }

        public static void SavePersistentPlayerDataXML()
        {
            if (GameManager.Instance.persistentPlayers != null)
            {
                GameManager.Instance.persistentPlayers.Write(GameIO.GetSaveGameDir(null, null) + "/players.xml");
            }
        }

        public static EnumLandClaimOwner ClaimedByWho(PlatformUserIdentifierAbs _uId, Vector3i _position)
        {
            PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
            if (persistentPlayerList != null)
            {
                PersistentPlayerData persistentPlayerData = persistentPlayerList.GetPlayerData(_uId);
                if (persistentPlayerData != null)
                {
                    return GameManager.Instance.World.GetLandClaimOwner(_position, persistentPlayerData);
                }
            }
            return EnumLandClaimOwner.None;
        }

        public static void ReturnBlock(ClientInfo _cInfo, string _blockName, int _quantity, string _phrase)
        {
            EntityPlayer player = GetEntityPlayer(_cInfo.entityId);
            if (player != null && player.IsSpawned() && !player.IsDead())
            {
                World world = GameManager.Instance.World;
                ItemValue itemValue = ItemClass.GetItem(_blockName, false);
                if (itemValue != null)
                {
                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                    {
                        entityClass = EntityClass.FromString("item"),
                        id = EntityFactory.nextEntityID++,
                        itemStack = new ItemStack(itemValue, _quantity),
                        pos = world.Players.dict[_cInfo.entityId].position,
                        rot = new Vector3(20f, 0f, 20f),
                        lifetime = 60f,
                        belongsPlayerId = _cInfo.entityId
                    });
                    world.SpawnEntityInWorld(entityItem);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                    Phrases.Dict.TryGetValue(_phrase, out string phrase);
                    phrase = phrase.Replace("{Value}", _quantity.ToString());
                    phrase = phrase.Replace("{ItemName}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.Name);
                    phrase = phrase.Replace("{BlockName}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static Dictionary<int, EntityPlayer> GetEntityPlayers()
        {
            return GameManager.Instance.World.Players.dict;
        }

        public static bool IsValidItem(string itemName)
        {
            itemValue = ItemClass.GetItem(itemName, false);
            if (itemValue.type != ItemValue.None.type)
            {
                return true;
            }
            return false;
        }

        public static string CreatePassword(int _length)
        {
            string pass = string.Empty;
            try
            {
                System.Random rnd = new System.Random();
                for (int i = 0; i < _length; i++)
                {
                    pass += AlphaNumSet.ElementAt(rnd.Next(0, 62));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in GeneralOperationss.CreatePassword: {0}", e.Message));
            }
            return pass;
        }

        public static void GetCurrencyName()
        {
            List<ItemClass> itemClassCurrency = ItemClass.GetItemsWithTag(FastTags.Parse("currency"));
            if (itemClassCurrency != null && itemClassCurrency.Count > 0)
            {
                Currency_Item = itemClassCurrency[0].Name;
                Log.Out("[SERVERTOOLS] Game currency and exchange set to item named '{0}'", Currency_Item);
                if (itemClassCurrency.Count > 1)
                {
                    for (int i = 1; i < itemClassCurrency.Count; i++)
                    {
                        Log.Out("[SERVERTOOLS] Alternative currency item named '{0}' was found with the Currency tag but is unused", itemClassCurrency[i].Name);
                    }
                }
            }
            else
            {
                No_Currency = true;
                Log.Out(string.Format("[SERVERTOOLS] Unable to find an item with the tag 'currency' in the item list. Bank and Wallet tools have been disabled. Anything relying on the Wallet for exchange will also not work. If this is the first time running ServerTools or it was recently updated, you may need to restart one more time"));
            }
        }

        public static List<Chunk> GetSurroundingChunks(Vector3i _position)
        {
            List<Chunk> chunks = new List<Chunk>();
            Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_position);
            if (chunk != null)
            {
                chunks.Add(chunk);
            }
            _position.x += 16;
            chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_position);
            if (chunk != null)
            {
                chunks.Add(chunk);
            }
            _position.z += 16;
            chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_position);
            if (chunk != null)
            {
                chunks.Add(chunk);
            }
            _position.x -= 16;
            chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_position);
            if (chunk != null)
            {
                chunks.Add(chunk);
            }
            _position.x -= 16;
            chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_position);
            if (chunk != null)
            {
                chunks.Add(chunk);
            }
            _position.z -= 16;
            chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_position);
            if (chunk != null)
            {
                chunks.Add(chunk);
            }
            _position.z -= 16;
            chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_position);
            if (chunk != null)
            {
                chunks.Add(chunk);
            }
            _position.x += 16;
            chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_position);
            if (chunk != null)
            {
                chunks.Add(chunk);
            }
            _position.x += 16;
            chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_position);
            if (chunk != null)
            {
                chunks.Add(chunk);
            }
            return chunks;
        }

        public static void JailPlayer(ClientInfo _cInfoKiller)
        {
            Phrases.Dict.TryGetValue("Jail1", out string phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfoKiller.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            SdtdConsole.Instance.ExecuteSync(string.Format("st-Jail add {0} 120", _cInfoKiller.CrossplatformId.CombinedString), null);
        }

        public static void KillPlayer(ClientInfo _cInfo, int _toolNumber)
        {
            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
            {
                string phrase = "";
                if (_toolNumber == 1)
                {
                    Phrases.Dict.TryGetValue("Lobby14", out phrase);
                }
                else if (_toolNumber == 2)
                {
                    Phrases.Dict.TryGetValue("Market14", out phrase);
                }
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.CrossplatformId.CombinedString), null);
            }, null);
        }

        public static void KickPlayer(ClientInfo _cInfo, string _reason)
        {
            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
            {
                GameUtils.KickPlayerForClientInfo(_cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, 0, default(DateTime), _reason));
            }, null);
        }

        public static void BanPlayer(ClientInfo _cInfo, string _reason)
        {
            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
            {
                if (_cInfo != null)
                {
                    DateTime time = DateTime.Now.AddYears(5);
                    string name = _cInfo.playerName;
                    if (_cInfo.PlatformId != null)
                    {
                        GameManager.Instance.adminTools.Blacklist.AddBan(name, _cInfo.PlatformId, time, _reason);
                        SdtdConsole.Instance.Output("{0} banned until {1}, reason: {2}.", _cInfo.PlatformId, time.ToCultureInvariantString(), _reason);

                        if (_cInfo.PlatformId is UserIdentifierSteam)
                        {
                            UserIdentifierSteam steamIdentifier = _cInfo.PlatformId as UserIdentifierSteam;
                            if (steamIdentifier != null && !steamIdentifier.OwnerId.Equals(steamIdentifier))
                            {
                                GameManager.Instance.adminTools.Blacklist.AddBan(name, steamIdentifier.OwnerId, time, _reason);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Steam Family Sharing license owner {0} banned until {1}, reason: {2}.", steamIdentifier.OwnerId, time.ToCultureInvariantString(), _reason);
                            }
                        }
                    }
                    if (_cInfo.CrossplatformId != null)
                    {
                        GameManager.Instance.adminTools.Blacklist.AddBan(name, _cInfo.CrossplatformId, time, _reason);
                        SdtdConsole.Instance.Output("{0} banned until {1}, reason: {2}.", _cInfo.CrossplatformId, time.ToCultureInvariantString(), _reason);
                    }
                    GameUtils.KickPlayerForClientInfo(_cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.Banned, 0, time, _reason));
                }
            }, null);
        }

        public static void CommandsList(ClientInfo _cInfo)
        {
            try
            {
                string commands = "";
                EntityPlayer player = GetEntityPlayer(_cInfo.entityId);
                if (player == null)
                {
                    return;
                }
                if (AdminList.IsEnabled && AdminList.Command_adminlist != "")
                {
                    if (CommandList.Dict.TryGetValue("adminlist", out bool hidden))
                    {
                        if (!hidden)
                        {

                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AdminList.Command_adminlist);
                        }
                    }
                }
                if (AnimalTracking.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("animal", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AnimalTracking.Command_animal);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("track", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AnimalTracking.Command_track);
                        }
                    }
                }
                if (Auction.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("auction", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("auction cancel", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_cancel);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("auction buy", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_buy);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("auction sell", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_sell);
                        }
                    }
                }
                if (AutoPartyInvite.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("party add", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AutoPartyInvite.Command_party_add);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("party remove", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AutoPartyInvite.Command_party_remove);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("party", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AutoPartyInvite.Command_party);
                        }
                    }
                }
                if (Bank.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("bank", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Bank.Command_bank);
                        }
                    }
                    if (Wallet.IsEnabled)
                    {
                        if (CommandList.Dict.TryGetValue("deposit", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Bank.Command_deposit);
                            }
                        }
                    }
                    if (Wallet.IsEnabled)
                    {
                        if (CommandList.Dict.TryGetValue("withdraw", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Bank.Command_withdraw);
                            }
                        }
                    }
                    if (Bank.Player_Transfers)
                    {
                        if (CommandList.Dict.TryGetValue("transfer", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Bank.Command_transfer);
                            }
                        }
                    }
                }
                if (Bed.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("bed", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Bed.Command_bed);
                        }
                    }
                }
                if (Bloodmoon.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("bloodmoon", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Bloodmoon.Command_bloodmoon);
                        }
                    }
                }
                if (Bounties.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("bounty", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Bounties.Command_bounty);
                            commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Bounties.Command_bounty);
                        }
                    }
                }
                if (ChatColor.IsEnabled && ChatColor.Players.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    if (CommandList.Dict.TryGetValue("ccc", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccc);
                        }
                    }
                    if (ChatColor.Rotate)
                    {
                        if (CommandList.Dict.TryGetValue("ccpr", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccpr);
                            }
                        }
                        if (CommandList.Dict.TryGetValue("ccnr", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccnr);
                            }
                        }
                    }
                    if (ChatColor.Custom_Color)
                    {
                        if (CommandList.Dict.TryGetValue("ccpr", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2} [******]", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccpr);
                            }
                        }
                        if (CommandList.Dict.TryGetValue("ccnr", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2} [******]", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccnr);
                            }
                        }
                    }
                }
                if (ClanManager.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("clan chat", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_chat);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("clan list", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_clan_list);
                        }
                    }
                    if (!ClanManager.ClanMember.Contains(_cInfo.CrossplatformId.CombinedString))
                    {
                        if (CommandList.Dict.TryGetValue("clan add", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2} clanName", commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_add);
                            }
                        }
                        if (CommandList.Dict.TryGetValue("clan request", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2} clanName", commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_request);
                            }
                        }
                    }
                }
                if (Day7.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("day7", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Day7.Command_day7);
                        }
                    }
                }
                if (Died.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("died", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Died.Command_died);
                        }
                    }
                }
                if (DiscordLink.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("discord", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, DiscordLink.Command_discord);
                        }
                    }
                }
                if (ExitCommand.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("exit", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ExitCommand.Command_exit);
                        }
                    }
                }
                if (FirstClaimBlock.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("claim", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, FirstClaimBlock.Command_claim);
                        }
                    }
                }
                if (Fps.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("fps", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Fps.Command_fps);
                        }
                    }
                }
                if (FriendTeleport.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("friend", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_friend);
                            commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_friend);
                        }
                    }
                    if (FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
                    {
                        if (CommandList.Dict.TryGetValue("accept", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_accept);
                            }
                        }
                    }
                }
                if (Gamble.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("gamble", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Gamble.Command_gamble);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("gamble bet", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Gamble.Command_gamble_bet);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("gamble payout", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Gamble.Command_gamble_payout);
                        }
                    }
                }
                if (Gimme.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("gimme", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Gimme.Command_gimme);
                        }
                    }
                }
                if (Hardcore.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("top3", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_top3);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("score", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_score);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("hardcore", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore);
                        }
                    }
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreEnabled)
                    {
                        if (Hardcore.Max_Extra_Lives > 0)
                        {
                            if (CommandList.Dict.TryGetValue("buy life", out hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_buy_life);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (CommandList.Dict.TryGetValue("hardcore on", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore_on);
                            }
                        }
                    }
                }
                if (Homes.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("home", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_home);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("fhome", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_fhome);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("home save", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_home_save);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("home delete", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_home_delete);
                        }
                    }
                    if (Homes.Invite.ContainsKey(_cInfo.entityId))
                    {
                        if (CommandList.Dict.TryGetValue("go home", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_go_home);
                            }
                        }
                    }
                    if (CommandList.Dict.TryGetValue("sethome", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_sethome);
                        }
                    }
                }
                if (InfoTicker.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("infoticker", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, InfoTicker.Command_infoticker);
                        }
                    }
                }
                if (KickVote.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("kickvote", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, KickVote.Command_kickvote);
                        }
                    }
                }
                if (Lobby.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("lobby", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Lobby.Command_lobby);
                        }
                    }
                    if (Lobby.Return && Lobby.IsLobby(player.position))
                    {
                        if (CommandList.Dict.TryGetValue("lobbyback", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Lobby.Command_lobbyback);
                            }
                        }
                    }
                }
                if (Loc.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("loc", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Loc.Command_loc);
                        }
                    }
                }
                if (Lottery.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("lottery", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery);
                            commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("lottery enter", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery_enter);
                        }
                    }
                }
                if (Market.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("market", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Market.Command_market);
                        }
                    }
                    if (Market.Return && Market.IsMarket(player.position))
                    {
                        if (CommandList.Dict.TryGetValue("marketback", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Market.Command_marketback);
                            }
                        }
                    }
                }
                if (Mute.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("mute", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Mute.Command_mute);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("unmute", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Mute.Command_unmute);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("mutelist", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Mute.Command_mutelist);
                        }
                    }
                }
                if (MuteVote.IsEnabled && Mute.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("mutevote", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, MuteVote.Command_mutevote);
                        }
                    }
                }
                if (BlockPickup.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("pickup", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, BlockPickup.Command_pickup);
                        }
                    }
                }
                if (PlayerList.IsEnabled)
                {
                    if (PlayerList.Command_playerlist != "")
                    {
                        if (CommandList.Dict.TryGetValue("playerlist", out bool hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, PlayerList.Command_playerlist);
                            }
                        }
                    }
                }
                if (Prayer.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("pray", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Prayer.Command_pray);
                        }
                    }
                }
                if (Report.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("report", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Report.Command_report);
                        }
                    }
                }
                if (RestartVote.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("restartvote", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, RestartVote.Command_restartvote);
                        }
                    }
                }
                if (Shop.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("shop", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Shop.Command_shop);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("shop buy", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Shop.Command_shop_buy);
                        }
                    }
                }
                if (Shutdown.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("shutdown", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Shutdown.Command_shutdown);
                        }
                    }
                }
                if (Stuck.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("stuck", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Stuck.Command_stuck);
                        }
                    }
                }
                if (Suicide.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("killme", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Suicide.Command_killme);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("suicide", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Suicide.Command_suicide);
                        }
                    }
                }
                if (Travel.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("travel", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Travel.Command_travel);
                        }
                    }
                }
                if (VehicleRecall.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("vehicle", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, VehicleRecall.Command_vehicle);
                        }
                    }
                }
                if (Voting.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("reward", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Voting.Command_reward);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("vote", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Voting.Command_vote);
                        }
                    }
                }
                if (Wall.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("wall", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Wall.Command_wall);
                        }
                    }
                }
                if (Waypoints.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("waypoint", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint);
                            commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("waypoint save", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint_save);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("waypoint delete", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint_delete);
                        }
                    }
                    if (CommandList.Dict.TryGetValue("fwaypoint", out hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_fwaypoint);
                        }
                    }
                    if (Waypoints.Invite.ContainsKey(_cInfo.entityId))
                    {
                        if (CommandList.Dict.TryGetValue("go way", out hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_go_way);
                            }
                        }
                    }
                }
                if (Whisper.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("rmessage", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2} {3}{4}", commands, ChatHook.Chat_Command_Prefix1, Whisper.Command_pmessage, ChatHook.Chat_Command_Prefix1, Whisper.Command_rmessage);
                        }
                    }
                }
                
                if (ReservedSlots.IsEnabled && (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                {
                    if (CommandList.Dict.TryGetValue("expire", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Command_expire);
                        }
                    }
                }
                else if (ChatColor.IsEnabled && (ChatColor.Players.ContainsKey(_cInfo.PlatformId.CombinedString) || ChatColor.Players.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                {
                    if (CommandList.Dict.TryGetValue("expire", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Command_expire);
                        }
                    }
                }
                else if (LoginNotice.IsEnabled && (LoginNotice.Dict1.ContainsKey(_cInfo.PlatformId.CombinedString) || LoginNotice.Dict1.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                {
                    if (CommandList.Dict.TryGetValue("expire", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Command_expire);
                        }
                    }
                }
                if (Sorter.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("sort", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Sorter.Command_sort);
                        }
                    }
                }
                if (Harvest.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue("harvest", out bool hidden))
                    {
                        if (!hidden)
                        {
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Harvest.Command_harvest);
                        }
                    }
                }
                if (commands.Length > 100)
                {
                    for (int i = 0; i < 10; i += 100)
                    {
                        commands = commands.Insert(i, "╚");
                        if (commands.Substring(i).Length < 100)
                        {
                            break;
                        }
                    }
                    string[] commandSplit = commands.Split('╚');
                    for (int i = 0; i < commandSplit.Length; i ++)
                    {
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + commandSplit[i], -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else if (commands.Length > 0)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in GeneralFunctions.CommandsList: {0}", e.Message);
            }
        }

        public static void AdminCommandList(ClientInfo _cInfo)
        {
            try
            {
                string commands = "";
                if (AdminChat.IsEnabled)
                {
                    commands = string.Format("{0} @" + AdminChat.Command_admin, commands);
                }
                if (commands.Length > 0)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in GeneralFunctions.AdminCommandList: {0}", e.Message);
            }
        }

        public static void Overlay(ClientInfo _cInfo)
        {
            if (!PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Overlay)
            {
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Overlay = true;
                Phrases.Dict.TryGetValue("Overlay2", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Overlay = false;
                Phrases.Dict.TryGetValue("Overlay3", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            PersistentContainer.DataChange = true;
        }

        public static void SessionBonus(string _id)
        {
            ClientInfo cInfo = GetClientInfoFromNameOrId(_id);
            if (cInfo == null)
            {
                return;
            }
            if ((Wallet.IsEnabled || Bank.IsEnabled && Bank.Direct_Payment) && Wallet.Session_Bonus > 0)
            {
                Wallet.AddCurrency(cInfo.CrossplatformId.CombinedString, Wallet.Session_Bonus, true);
            }
        }


        public static void StartLog()
        {
            try
            {
                Log.LogCallbacks += LogAction;
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in GeneralFunctions.StartLog: {0}", e.Message);
            }
        }

        public static void CloseLog()
        {
            try
            {
                Log.LogCallbacks -= LogAction;
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in GeneralFunctions.CloseLog: {0}", e.Message);
            }
        }

        private static void LogAction(string msg, string trace, LogType type)
        {
            try
            {
                ActiveLog.Add(msg);
                //using (StreamWriter sw = new StreamWriter(Debug, true, Encoding.UTF8))
                //{
                //    sw.WriteLine(string.Format("msg: {0} / trace {1} / type {2}", msg, trace, type));
                //    sw.WriteLine();
                //    sw.Flush();
                //    sw.Close();
                //    sw.Dispose();
                //}
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in GeneralFunctions.LogAction: {0}", e.Message);
            }
        }
    }
}
