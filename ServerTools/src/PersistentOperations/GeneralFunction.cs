using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    public class GeneralFunction
    {
        public static bool AreaRunning = false, Shutdown_Initiated = false, No_Vehicle_Pickup = false, ThirtySeconds = false, 
            No_Currency = false, Net_Package_Detector = false, Debug = false, Allow_Bicycle = false;
        public static int Jail_Violation = 4, Kill_Violation = 6, Kick_Violation = 8, Ban_Violation = 10, Player_Killing_Mode = 0, 
            MeleeHandPlayer = 0;
        public static string AppPath, Currency_Item, XPathDir, Command_expire = "expire", Command_commands = "commands", Command_overlay = "overlay";

        public static Dictionary<string, DateTime> Session = new Dictionary<string, DateTime>();
        public static Dictionary<int, int> EntityId = new Dictionary<int, int>();
        public static Dictionary<int, int> PvEViolations = new Dictionary<int, int>();

        public static List<ClientInfo> NewPlayerQue = new List<ClientInfo>();
        public static List<ClientInfo> BlockChatCommands = new List<ClientInfo>();

        public static readonly string AlphaNumSet = "jJkqQr9Kl3wXAbyYz0ZLmFpPRsMn5NoO6dDe1EfStaBc2CgGhH7iITu4U8vWxV", NumSet = "1928374650";
        public static readonly char[] InvalidPrefix = new char[] { '!', '@', '#', '$', '%', '&', '/', '\\' };

        public static DateTime StartTime;

        public static void CreateCustomXUi()
        {
            if (!string.IsNullOrEmpty(XPathDir))
            {
                if (!File.Exists(XPathDir + "items.xml"))
                {
                    using (StreamWriter sw = new StreamWriter(XPathDir + "items.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("<set xpath=\"/items/item[@name='casinoCoin']/property[@name='Tags']/@value\">dukes,currency</set>");
                        sw.WriteLine("<!-- ..... Wallet and Bank currency ^ ..... -->");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                        sw.Flush();
                        sw.Close();
                    }
                    XPathAlert("items.xml");
                }
                if (File.Exists(XPathDir + "gameevents.xml"))
                {
                    List<string> eventContent = new List<string>();
                    using (FileStream fs = new FileStream(XPathDir + "gameevents.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            while (!sr.EndOfStream)
                            {
                                eventContent.Add(sr.ReadLine());
                            }
                        }
                    }
                    int count = 0;
                    for (int i = 0; i < eventContent.Count; i++)
                    {
                        string line = eventContent[i];
                        if (line.Contains("action_admin") || line.Contains("action_dukes") || line.Contains("action_currency") || line.Contains("action_eject"))
                        {
                            count += 1;
                        }
                    }
                    if (count != 4)
                    {
                        File.Delete(XPathDir + "gameevents.xml");
                    }
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
                        sw.WriteLine();
                        sw.WriteLine("      <action class=\"RemoveItems\">");
                        sw.WriteLine("          <property name=\"items_location\" value=\"Backpack\" />");
                        sw.WriteLine("          <property name=\"remove_item_tag\" value=\"admin\" />");
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
                        sw.WriteLine();
                        sw.WriteLine("      <action class=\"RemoveItems\">");
                        sw.WriteLine("          <property name=\"items_location\" value=\"Backpack\" />");
                        sw.WriteLine("          <property name=\"remove_item_tag\" value=\"dukes\" />");
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
                        sw.WriteLine();
                        sw.WriteLine("      <action class=\"RemoveItems\">");
                        sw.WriteLine("          <property name=\"items_location\" value=\"Backpack\" />");
                        sw.WriteLine("          <property name=\"remove_item_tag\" value=\"currency\" />");
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
                        sw.WriteLine();
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
                        sw.WriteLine("</append>");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                        sw.Flush();
                        sw.Close();
                    }
                    XPathAlert("gameevents.xml");
                }
                if (File.Exists(XPathDir + "XUi/windows.xml"))
                {
                    List<string> windowContent = new List<string>();
                    using (FileStream fs = new FileStream(XPathDir + "XUi/windows.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            while (!sr.EndOfStream)
                            {
                                windowContent.Add(sr.ReadLine());
                            }
                        }
                    }
                    int count = 0;
                    for (int i = 0; i < windowContent.Count; i++)
                    {
                        string line = windowContent[i];
                        if (line.Contains(AllocsMap.Link) || line.Contains(DiscordLink.Invitation_Link) || line.Contains(Voting.Link) ||
                            line.Contains("rio.html") || line.Contains("shop.html") || line.Contains("auction.html") ||
                            line.Contains("imap.html"))
                        {
                            count += 1;
                        }
                    }
                    if (count != 7)
                    {
                        File.Delete(XPathDir + "XUi/windows.xml");
                    }
                }
                if (!File.Exists(XPathDir + "XUi/windows.xml"))
                {
                    using (StreamWriter sw = new StreamWriter(XPathDir + "XUi/windows.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("<append xpath=\"/windows\">");
                        sw.WriteLine();
                        sw.WriteLine("  <window name=\"browserMap\" controller=\"ServerInfo\">");
                        sw.WriteLine("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                        sw.WriteLine("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"World Map\" />");
                        sw.WriteLine("      </panel>");
                        sw.WriteLine("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                        sw.WriteLine("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                        sw.WriteLine("          <label name=\"ServerDescription\" />");
                        sw.WriteLine(string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", AllocsMap.Link));
                        sw.WriteLine("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                        sw.WriteLine("          <sprite depth=\"4\" name=\"mapIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_map\" />");
                        sw.WriteLine("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                        sw.WriteLine("      </panel>");
                        sw.WriteLine("  </window>");
                        sw.WriteLine();
                        sw.WriteLine("  <window name=\"browserDiscord\" controller=\"ServerInfo\">");
                        sw.WriteLine("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                        sw.WriteLine("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Discord Link\" />");
                        sw.WriteLine("      </panel>");
                        sw.WriteLine("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                        sw.WriteLine("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                        sw.WriteLine("          <label name=\"ServerDescription\" />");
                        sw.WriteLine(string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", DiscordLink.Invitation_Link));
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
                        sw.WriteLine(string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", Voting.Link));
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
                        sw.WriteLine("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"http://0.0.0.0:8084/rio.html\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />");
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
                        sw.WriteLine("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"http://0.0.0.0:8084/shop.html\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />");
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
                        sw.WriteLine("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"http://0.0.0.0:8084/auction.html\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />");
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
                        sw.WriteLine("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"http://0.0.0.0:8084/imap.html\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />");
                        sw.WriteLine("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                        sw.WriteLine("          <sprite depth=\"4\" name=\"mapIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_map\" />");
                        sw.WriteLine("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                        sw.WriteLine("      </panel>");
                        sw.WriteLine("  </window>");
                        sw.WriteLine();
                        sw.WriteLine("</append>");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                    }
                    XPathAlert("windows.xml");
                }
                if (File.Exists(XPathDir + "buffs.xml"))
                {
                    List<string> buffContent = new List<string>();
                    using (FileStream fs = new FileStream(XPathDir + "buffs.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            while (!sr.EndOfStream)
                            {
                                buffContent.Add(sr.ReadLine());
                            }
                        }
                    }
                    int count = 0;
                    for (int i = 0; i < buffContent.Count; i++)
                    {
                        string line = buffContent[i];
                        if (line.Contains("PvE_Zone") || line.Contains("PvP_Ally_Zone") || line.Contains("PvP_Stranger_Zone") || 
                            line.Contains("PvP_Zone") || line.Contains("pvp_ally_damage") || line.Contains("pvp_stranger_damage") ||
                            line.Contains("pvp_damage") || line.Contains("region_reset"))
                        {
                            count += 1;
                        }
                    }
                    if (count != 18)
                    {
                        File.Delete(XPathDir + "buffs.xml");
                    }
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
                        sw.WriteLine("	<stack_type value=\"replace\"/>");
                        sw.WriteLine("	<effect_group>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfBuffStart\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_ally_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfBuffStart\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_stranger_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfBuffStart\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pve_zone\"/>");
                        sw.WriteLine("	</effect_group>");
                        sw.WriteLine("</buff>");
                        sw.WriteLine();
                        sw.WriteLine("<buff name=\"pvp_ally_zone\" name_key=\"PvP_Ally_Zone\" description_key=\"You are inside a area with active damage for allies\" tooltip_key=\"PvP_Ally_Zone\" icon=\"ui_game_symbol_twitch_no_ranged\" icon_color=\"204,204,0\">");
                        sw.WriteLine("	<stack_type value=\"replace\"/>");
                        sw.WriteLine("	<effect_group>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfBuffStart\" target=\"self\" action=\"RemoveBuff\" buff=\"pve_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfBuffStart\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_stranger_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfBuffStart\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_ally_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfPrimaryActionRayHit\" target=\"other\" action=\"AddBuff\" buff=\"pvp_ally_damage\">");
                        sw.WriteLine("			<requirement name=\"EntityTagCompare\" target_tags=\"ally\"/>");
                        sw.WriteLine("		</triggered_effect>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfSecondaryActionRayHit\" target=\"other\" action=\"AddBuff\" buff=\"pvp_ally_damage\">");
                        sw.WriteLine("			<requirement name=\"EntityTagCompare\" target_tags=\"ally\"/>");
                        sw.WriteLine("		</triggered_effect>");
                        sw.WriteLine("	</effect_group>");
                        sw.WriteLine("</buff>");
                        sw.WriteLine();
                        sw.WriteLine("<buff name=\"pvp_stranger_zone\" name_key=\"PvP_Stranger_Zone\" description_key=\"You are inside a area with active damage for strangers\" tooltip_key=\"PvP_Stranger_Zone\" icon=\"ui_game_symbol_twitch_no_ranged\" icon_color=\"255,153,0\">");
                        sw.WriteLine("	<stack_type value=\"replace\"/>");
                        sw.WriteLine("	<effect_group>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfBuffStart\" target=\"self\" action=\"RemoveBuff\" buff=\"pve_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfBuffStart\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_ally_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfBuffStart\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_stranger_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfPrimaryActionRayHit\" target=\"other\" action=\"AddBuff\" buff=\"pvp_stranger_damage\">");
                        sw.WriteLine("			<requirement name=\"EntityTagCompare\" tags=\"player\"/>");
                        sw.WriteLine("		</triggered_effect>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfSecondaryActionRayHit\" target=\"other\" action=\"AddBuff\" buff=\"pvp_stranger_damage\">");
                        sw.WriteLine("			<requirement name=\"EntityTagCompare\" tags=\"player\"/>");
                        sw.WriteLine("		</triggered_effect>");
                        sw.WriteLine("	</effect_group>");
                        sw.WriteLine("</buff>");
                        sw.WriteLine();
                        sw.WriteLine("<buff name=\"pvp_zone\" name_key=\"PvP_Zone\" description_key=\"You are inside a area with active damage for everyone\" tooltip_key=\"PvP_Zone\" icon=\"ui_game_symbol_twitch_no_ranged\" icon_color=\"204,0,0\">");
                        sw.WriteLine("	<stack_type value=\"replace\"/>");
                        sw.WriteLine("	<effect_group>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfBuffStart\" target=\"self\" action=\"RemoveBuff\" buff=\"pve_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfBuffStart\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_ally_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfBuffStart\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_stranger_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_zone\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfPrimaryActionRayHit\" target=\"other\" action=\"AddBuff\" buff=\"pvp_damage\">");
                        sw.WriteLine("			<requirement name=\"EntityTagCompare\" tags=\"player\"/>");
                        sw.WriteLine("		</triggered_effect>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfSecondaryActionRayHit\" target=\"other\" action=\"AddBuff\" buff=\"pvp_damage\">");
                        sw.WriteLine("			<requirement name=\"EntityTagCompare\" tags=\"player\"/>");
                        sw.WriteLine("		</triggered_effect>");
                        sw.WriteLine("	</effect_group>");
                        sw.WriteLine("</buff>");
                        sw.WriteLine();
                        sw.WriteLine("<buff name=\"pvp_ally_damage\" hidden=\"true\">");
                        sw.WriteLine("	<stack_type value=\"replace\"/>");
                        sw.WriteLine("	<duration value=\"1\"/>");
                        sw.WriteLine("	<effect_group>");
                        sw.WriteLine("      <requirements>");
                        sw.WriteLine("			<requirement name=\"!HasBuff\" buff=\"pvp_ally_zone\"/>");
                        sw.WriteLine("	    </requirements>");
                        sw.WriteLine("		<passive_effect name=\"HealthLoss\" operation=\"base_set\" value=\"0\"/>");
                        sw.WriteLine("		<passive_effect name=\"HealthLoss\" operation=\"perc_set\" value=\"0\"/>");
                        sw.WriteLine("		<passive_effect name=\"HealthLoss\" operation=\"perc_add\" value=\"-1\"/>");
                        sw.WriteLine("		<passive_effect name=\"ElementalDamageResist\" operation=\"base_add\" value=\"200\"/>");
                        sw.WriteLine("		<passive_effect name=\"PhysicalDamageResist\" operation=\"base_add\" value=\"200\"/>");
                        sw.WriteLine("		<passive_effect name=\"ExplosionIncomingDamage\" operation=\"base_set\" value=\"0\" tags=\"explosive\"/>");
                        sw.WriteLine("		<passive_effect name=\"ExplosionIncomingDamage\" operation=\"perc_set\" value=\"0\" tags=\"explosive\"/>");
                        sw.WriteLine("		<passive_effect name=\"ExplosionIncomingDamage\" operation=\"perc_add\" value=\"-1\" tags=\"explosive\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_ally_damage\"/>");
                        sw.WriteLine("	</effect_group>");
                        sw.WriteLine("</buff>");
                        sw.WriteLine();
                        sw.WriteLine("<buff name=\"pvp_stranger_damage\" hidden=\"true\">");
                        sw.WriteLine("	<stack_type value=\"replace\"/>");
                        sw.WriteLine("	<duration value=\"1\"/>");
                        sw.WriteLine("	<effect_group>");
                        sw.WriteLine("      <requirements>");
                        sw.WriteLine("			<requirement name=\"!HasBuff\" buff=\"pvp_ally_zone\"/>");
                        sw.WriteLine("	    </requirements>");
                        sw.WriteLine("		<passive_effect name=\"HealthLoss\" operation=\"base_set\" value=\"0\"/>");
                        sw.WriteLine("		<passive_effect name=\"HealthLoss\" operation=\"perc_set\" value=\"0\"/>");
                        sw.WriteLine("		<passive_effect name=\"HealthLoss\" operation=\"perc_add\" value=\"-1\"/>");
                        sw.WriteLine("		<passive_effect name=\"ElementalDamageResist\" operation=\"base_add\" value=\"200\"/>");
                        sw.WriteLine("		<passive_effect name=\"PhysicalDamageResist\" operation=\"base_add\" value=\"200\"/>");
                        sw.WriteLine("		<passive_effect name=\"ExplosionIncomingDamage\" operation=\"base_set\" value=\"0\" tags=\"explosive\"/>");
                        sw.WriteLine("		<passive_effect name=\"ExplosionIncomingDamage\" operation=\"perc_set\" value=\"0\" tags=\"explosive\"/>");
                        sw.WriteLine("		<passive_effect name=\"ExplosionIncomingDamage\" operation=\"perc_add\" value=\"-1\" tags=\"explosive\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_stranger_damage\"/>");
                        sw.WriteLine("	</effect_group>");
                        sw.WriteLine("</buff>");
                        sw.WriteLine();
                        sw.WriteLine("<buff name=\"pvp_damage\" hidden=\"true\">");
                        sw.WriteLine("	<stack_type value=\"replace\"/>");
                        sw.WriteLine("	<duration value=\"1\"/>");
                        sw.WriteLine("	<effect_group>");
                        sw.WriteLine("      <requirements>");
                        sw.WriteLine("			<requirement name=\"!HasBuff\" buff=\"pvp_ally_zone\"/>");
                        sw.WriteLine("	    </requirements>");
                        sw.WriteLine("		<passive_effect name=\"HealthLoss\" operation=\"base_set\" value=\"0\"/>");
                        sw.WriteLine("		<passive_effect name=\"HealthLoss\" operation=\"perc_set\" value=\"0\"/>");
                        sw.WriteLine("		<passive_effect name=\"HealthLoss\" operation=\"perc_add\" value=\"-1\"/>");
                        sw.WriteLine("		<passive_effect name=\"ElementalDamageResist\" operation=\"base_add\" value=\"200\"/>");
                        sw.WriteLine("		<passive_effect name=\"PhysicalDamageResist\" operation=\"base_add\" value=\"200\"/>");
                        sw.WriteLine("		<passive_effect name=\"ExplosionIncomingDamage\" operation=\"base_set\" value=\"0\" tags=\"explosive\"/>");
                        sw.WriteLine("		<passive_effect name=\"ExplosionIncomingDamage\" operation=\"perc_set\" value=\"0\" tags=\"explosive\"/>");
                        sw.WriteLine("		<passive_effect name=\"ExplosionIncomingDamage\" operation=\"perc_add\" value=\"-1\" tags=\"explosive\"/>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"pvp_damage\"/>");
                        sw.WriteLine("	</effect_group>");
                        sw.WriteLine("</buff>");
                        sw.WriteLine();
                        sw.WriteLine("<buff name=\"region_reset\" name_key=\"Region_reset\" description_key=\"The region you are in will reset. Building here is NOT recommended\" icon=\"ui_game_symbol_brick\" icon_color=\"255, 153, 51\">");
                        sw.WriteLine("	<stack_type value=\"replace\"/>");
                        sw.WriteLine("	<effect_group>");
                        sw.WriteLine("		<triggered_effect trigger=\"onSelfDied\" target=\"self\" action=\"RemoveBuff\" buff=\"region_reset\"/>");
                        sw.WriteLine("	</effect_group>");
                        sw.WriteLine("</buff>");
                        sw.WriteLine();
                        sw.WriteLine("</append>");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                        sw.Flush();
                        sw.Close();
                    }
                    XPathAlert("buffs.xml");
                }
                if (File.Exists(XPathDir + "XUi/xui.xml"))
                {
                    List<string> xuiContent = new List<string>();
                    using (FileStream fs = new FileStream(XPathDir + "XUi/xui.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            while (!sr.EndOfStream)
                            {
                                xuiContent.Add(sr.ReadLine());
                            }
                        }
                    }
                    int count = 0;
                    for (int i = 0; i < xuiContent.Count; i++)
                    {
                        string line = xuiContent[i];
                        if (line.Contains("browserMap") || line.Contains("browserDiscord") || line.Contains("browserVote") ||
                            line.Contains("browserRio") || line.Contains("browserShop") || line.Contains("browserAuction") ||
                            line.Contains("browserIMap"))
                        {
                            count += 1;
                        }
                    }
                    if (count != 14)
                    {
                        File.Delete(XPathDir + "XUi/xui.xml");
                    }
                }
                if (!File.Exists(XPathDir + "XUi/xui.xml"))
                {
                    using (StreamWriter sw = new StreamWriter(XPathDir + "XUi/xui.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("<append xpath=\"/xui/ruleset\">");
                        sw.WriteLine();
                        sw.WriteLine("  <window_group name=\"browserMap\">");
                        sw.WriteLine("      <window name=\"browserMap\" />");
                        sw.WriteLine("  </window_group>");
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
                        sw.WriteLine("  <window_group name=\"browserIMap\">");
                        sw.WriteLine("      <window name=\"browserIMap\" />");
                        sw.WriteLine("  </window_group>");
                        sw.WriteLine();
                        sw.WriteLine("</append>");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                    }
                    XPathAlert("xui.xml");
                }
                if (File.Exists(XPathDir + "blocks.xml"))
                {
                    List<string> windowContent = new List<string>();
                    using (FileStream fs = new FileStream(XPathDir + "blocks.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            while (!sr.EndOfStream)
                            {
                                windowContent.Add(sr.ReadLine());
                            }
                        }
                    }
                    int count = 0;
                    for (int i = 0; i < windowContent.Count; i++)
                    {
                        string line = windowContent[i];
                        if (line.Contains("VaultBox") || line.Contains("movement,melee,bullet,arrow,rocket"))
                        {
                            count += 1;
                        }
                    }
                    if (count != 2)
                    {
                        File.Delete(XPathDir + "blocks.xml");
                    }
                }
                if (!File.Exists(XPathDir + "blocks.xml"))
                {
                    using (StreamWriter sw = new StreamWriter(XPathDir + "blocks.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("<append xpath=\"/blocks\">");
                        sw.WriteLine();
                        sw.WriteLine("<block name=\"VaultBox\">");
                        sw.WriteLine("	<property name=\"CreativeMode\" value=\"Player\"/>");
                        sw.WriteLine("	<property name=\"Tags\" value=\"door\"/>");
                        sw.WriteLine("	<property name=\"Class\" value=\"SecureLoot\"/>");
                        sw.WriteLine("	<property name=\"CustomIcon\" value=\"cntChest02\"/>");
                        sw.WriteLine("	<property name=\"Material\" value=\"MwoodReinforced\"/>");
                        sw.WriteLine("	<property name=\"StabilitySupport\" value=\"false\"/>");
                        sw.WriteLine("	<property name=\"Shape\" value=\"Ext3dModel\"/>");
                        sw.WriteLine("	<property name=\"Texture\" value=\"293\"/>");
                        sw.WriteLine("	<property name=\"Mesh\" value=\"models\"/>");
                        sw.WriteLine("	<property name=\"IsTerrainDecoration\" value=\"true\"/>");
                        sw.WriteLine("	<property name=\"FuelValue\" value=\"300\"/>");
                        sw.WriteLine("	<property name=\"Model\" value=\"LootContainers/chest02\" param1=\"main_mesh\"/>");
                        sw.WriteLine("	<property name=\"ShowModelOnFall\" value=\"false\"/>");
                        sw.WriteLine("	<property name=\"HandleFace\" value=\"Bottom\"/>");
                        sw.WriteLine("	<property name=\"ImposterExchange\" value=\"imposterQuarter\" param1=\"154\"/>");
                        sw.WriteLine("	<property name=\"Collide\" value=\"movement,melee,bullet,arrow,rocket\"/>");
                        sw.WriteLine("	<property name=\"LootList\" value=\"playerStorage\"/>");
                        sw.WriteLine("	<property class=\"RepairItems\">");
                        sw.WriteLine("		<property name=\"resourceWood\" value=\"10\"/>");
                        sw.WriteLine("	</property>");
                        sw.WriteLine("	<drop event=\"Fall\" name=\"terrDestroyedWoodDebris\" count=\"1\" prob=\"1\" stick_chance=\"1\"/>");
                        sw.WriteLine("	<property name=\"LPHardnessScale\" value=\"8\"/>");
                        sw.WriteLine("	<property name=\"DescriptionKey\" value=\"cntSecureStorageChestDesc\"/>");
                        sw.WriteLine("	<property name=\"CanPickup\" value=\"true\"/>");
                        sw.WriteLine("	<property name=\"EconomicValue\" value=\"0\"/>");
                        sw.WriteLine("	<property name=\"EconomicBundleSize\" value=\"1\"/>");
                        sw.WriteLine("	<property name=\"FilterTags\" value=\"MC_playerBlocks,SC_decor\"/>");
                        sw.WriteLine("</block>");
                        sw.WriteLine();
                        sw.WriteLine("</append>");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                        sw.Flush();
                        sw.Close();
                    }
                }
                if (File.Exists(XPathDir + "recipes.xml"))
                {
                    List<string> windowContent = new List<string>();
                    using (FileStream fs = new FileStream(XPathDir + "recipes.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            while (!sr.EndOfStream)
                            {
                                windowContent.Add(sr.ReadLine());
                            }
                        }
                    }
                    int count = 0;
                    for (int i = 0; i < windowContent.Count; i++)
                    {
                        string line = windowContent[i];
                        if (line.Contains("VaultBox"))
                        {
                            count += 1;
                        }
                    }
                    if (count != 1)
                    {
                        File.Delete(XPathDir + "recipes.xml");
                    }
                }
                if (!File.Exists(XPathDir + "recipes.xml"))
                {
                    using (StreamWriter sw = new StreamWriter(XPathDir + "recipes.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("<append xpath=\"/recipes\">");
                        sw.WriteLine();
                        sw.WriteLine("<recipe name=\"VaultBox\" count=\"1\">");
                        sw.WriteLine("	<ingredient name=\"resourceWood\" count=\"10\"/>");
                        sw.WriteLine("</recipe>");
                        sw.WriteLine();
                        sw.WriteLine("</append>");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
        }

        public static void XPathAlert(string _fileName)
        {
            Log.Warning(string.Format("[SERVERTOOLS] The file named '{0}' has been created or updated and is required for full functionality of ServerTools. Server restart is recommended", _fileName));
        }

        public static void CheckArea()
        {
            if (!AreaRunning)
            {
                AreaRunning = true;
                List<Entity> entityList = GameManager.Instance.World.Entities.list;
                List<ClientInfo> clientList = ClientList();
                if (clientList != null)
                {
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        ClientInfo cInfo = clientList[i];
                        if (cInfo != null && !TeleportDetector.Ommissions.Contains(cInfo.entityId))
                        {
                            EntityPlayer player = GetEntityPlayer(cInfo.entityId);
                            if (player != null && player.IsSpawned() && !player.IsDead() && player.position != null)
                            {
                                if (Zones.IsEnabled && Zones.ZoneList.Count > 0)
                                {
                                    Zones.ZoneCheck(cInfo, player, entityList);
                                }
                                if (Lobby.IsEnabled)
                                {
                                    Lobby.IsLobby(player.position);
                                }
                                if (Market.IsEnabled)
                                {
                                    Market.IsMarket(player.position);
                                }
                                if (RegionReset.IsEnabled)
                                {
                                    RegionReset.IsResetRegion(cInfo, player);
                                }
                            }
                        }
                    }
                }
                if (entityList != null && entityList.Count > 0)
                {
                    for (int i = 0; i < entityList.Count; i++)
                    {
                        Entity entity = entityList[i];
                        if (entity != null & !(entity is EntityPlayer) && entity.IsSpawned())
                        {
                            if (Lobby.IsEnabled && Lobby.IsLobby(entity.position))
                            {
                                GameManager.Instance.World.RemoveEntity(entityList[i].entityId, EnumRemoveEntityReason.Despawned);
                                Log.Out(string.Format("[SERVERTOOLS] Removed a hostile entity from the lobby @ '{0}'", entityList[i].position));
                            }
                            else if (Market.IsEnabled && Market.IsMarket(entity.position))
                            {
                                GameManager.Instance.World.RemoveEntity(entityList[i].entityId, EnumRemoveEntityReason.Despawned);
                                Log.Out(string.Format("[SERVERTOOLS] Removed a hostile entity from the market @ '{0}'", entityList[i].position));
                            }
                        }
                    }
                }
                AreaRunning = false;
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
                return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List.ToList();
            }
            return null;
        }

        public static ClientInfo GetClientInfoFromNameOrId(string _id)
        {
            ClientInfo cInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.GetForNameOrId(_id);
            if (cInfo != null)
            {
                return cInfo;
            }
            else if (int.TryParse(_id, out int entityId))
            {
                cInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(entityId);
                if (cInfo != null)
                {
                    return cInfo;
                }
            }
            return null;
        }

        public static ClientInfo GetClientInfoFromEntityId(int _entityId)
        {
            return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(_entityId);
        }

        public static ClientInfo GetClientInfoFromUId(PlatformUserIdentifierAbs _uId)
        {
            return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(_uId);
        }

        public static ClientInfo GetClientInfoFromName(string _name)
        {
            return SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.GetForPlayerName(_name);
        }

        public static List<EntityPlayer> ListPlayers()
        {
            return GameManager.Instance.World.Players.list;
        }

        public static EntityPlayer GetEntityPlayer(int _id)
        {
            EntityPlayer entityPlayer;
            GameManager.Instance.World.Players.dict.TryGetValue(_id, out entityPlayer);
            return entityPlayer;
        }

        public static Entity GetEntity(int _id)
        {
            Entity entity;
            GameManager.Instance.World.Entities.dict.TryGetValue(_id, out entity);
            return entity;
        }

        public static EntityZombie GetZombie(int _id)
        {
            Entity entity;
            GameManager.Instance.World.Entities.dict.TryGetValue(_id, out entity);
            if (entity != null && entity is EntityZombie)
            {
                return entity as EntityZombie;
            }
            return null;
        }

        public static EntityAnimal GetAnimal(int _id)
        {
            Entity entity;
            GameManager.Instance.World.Entities.dict.TryGetValue(_id, out entity);
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
            PlatformUserIdentifierAbs uId = GetPlatformUserFromNameOrId(_id);
            if (uId != null)
            {
                PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
                if (persistentPlayerList != null)
                {
                    PersistentPlayerData ppd = persistentPlayerList.GetPlayerData(uId);
                    if (ppd != null)
                    {
                        return ppd;
                    }
                }
            }
            return null;
        }

        public static PersistentPlayerData GetPersistentPlayerDataFromUId(PlatformUserIdentifierAbs _uId)
        {
            PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
            if (persistentPlayerList != null)
            {
                PersistentPlayerData ppd = persistentPlayerList.GetPlayerData(_uId);
                if (ppd != null)
                {
                    return ppd;
                }
            }
            return null;
        }

        public static PersistentPlayerData GetPersistentPlayerDataFromEntityId(int _entityId)
        {
            PersistentPlayerList persistentPlayerList = GameManager.Instance.persistentPlayers;
            if (persistentPlayerList != null)
            {
                PersistentPlayerData persistentPlayerData = persistentPlayerList.GetPlayerDataFromEntityID(_entityId);
                if (persistentPlayerData != null)
                {
                    return persistentPlayerData;
                }
            }
            return null;
        }

        public static PlayerDataFile GetPlayerDataFileFromUId(PlatformUserIdentifierAbs _uId)
        {
            PlayerDataFile playerDatafile = new PlayerDataFile();
            playerDatafile.Load(GameIO.GetPlayerDataDir(), _uId.CombinedString);
            if (playerDatafile != null)
            {
                return playerDatafile;
            }
            return null;
        }

        public static PlayerDataFile GetPlayerDataFileFromEntityId(int _entityId)
        {
            PersistentPlayerData persistentPlayerData = GetPersistentPlayerDataFromEntityId(_entityId);
            if (persistentPlayerData != null)
            {
                PlayerDataFile playerDatafile = new PlayerDataFile();
                playerDatafile.Load(GameIO.GetPlayerDataDir(), persistentPlayerData.UserIdentifier.CombinedString.Trim());
                if (playerDatafile != null)
                {
                    return playerDatafile;
                }
            }
            return null;
        }

        public static PlayerDataFile GetPlayerDataFileFromId(string _id)
        {
            if (ConsoleHelper.ParseParamPartialNameOrId(_id, out PlatformUserIdentifierAbs platformUserIdentifierAbs, out ClientInfo clientInfo, true) == 1 && platformUserIdentifierAbs != null)
            {
                PlayerDataFile playerDatafile = GetPlayerDataFileFromUId(platformUserIdentifierAbs);
                if (playerDatafile != null)
                {
                    return playerDatafile;
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

        public static void RemoveAllClaims(string _id)
        {
            PersistentPlayerData persistentPlayerData = GetPersistentPlayerDataFromId(_id);
            if (persistentPlayerData != null)
            {
                List<Vector3i> landProtectionBlocks = persistentPlayerData.LPBlocks;
                if (landProtectionBlocks != null)
                {
                    for (int i = 0; i < landProtectionBlocks.Count; i++)
                    {
                        Vector3i position = landProtectionBlocks[i];
                        World world = GameManager.Instance.World;
                        BlockValue blockValue = world.GetBlock(position);
                        Block block = blockValue.Block;
                        if (block != null && block is BlockLandClaim)
                        {
                            world.SetBlockRPC(0, position, BlockValue.Air);
                            SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, position.ToVector3()), false, -1, -1, -1, -1);
                            world.ObjectOnMapRemove(EnumMapObjectType.LandClaim, position.ToVector3());
                            LandClaimBoundsHelper.RemoveBoundsHelper(position.ToVector3());
                        }
                        GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(position);
                        persistentPlayerData.LPBlocks.Remove(position);
                    }
                    SavePersistentPlayerDataXML();
                }
            }
        }

        public static void RemoveOneClaim(string _playerId, Vector3i _position)
        {
            PersistentPlayerData persistentPlayerData = GetPersistentPlayerDataFromId(_playerId);
            if (persistentPlayerData != null)
            {
                World world = GameManager.Instance.World;
                BlockValue blockValue = world.GetBlock(_position);
                Block block = blockValue.Block;
                if (block != null && block is BlockLandClaim)
                {
                    world.SetBlockRPC(0, _position, BlockValue.Air);
                    SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, _position.ToVector3()), false, -1, -1, -1, -1);
                    world.ObjectOnMapRemove(EnumMapObjectType.LandClaim, _position.ToVector3());
                    LandClaimBoundsHelper.RemoveBoundsHelper(_position.ToVector3());
                }
                GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(_position);
                persistentPlayerData.LPBlocks.Remove(_position);
                SavePersistentPlayerDataXML();
            }
        }

        public static void RemovePersistentPlayerData(string _id)
        {
            PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
            if (persistentPlayerList != null)
            {
                PlatformUserIdentifierAbs uId = GetPlatformUserFromNameOrId(_id);
                if (uId != null)
                {
                    if (persistentPlayerList.Players.ContainsKey(uId))
                    {
                        persistentPlayerList.Players.Remove(uId);
                        SavePersistentPlayerDataXML();
                    }
                }
            }
        }

        public static void RemoveAllACL(string _playerId)
        {
            PersistentPlayerData persistentPlayerData = GetPersistentPlayerDataFromId(_playerId);
            if (persistentPlayerData != null)
            {
                PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
                foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> persistentPlayerData2 in persistentPlayerList.Players)
                {
                    if (persistentPlayerData2.Key != persistentPlayerData.UserIdentifier)
                    {
                        if (persistentPlayerData2.Value.ACL != null && persistentPlayerData2.Value.ACL.Contains(persistentPlayerData.UserIdentifier))
                        {
                            persistentPlayerData2.Value.RemovePlayerFromACL(persistentPlayerData.UserIdentifier);
                            persistentPlayerData2.Value.Dispatch(persistentPlayerData, EnumPersistentPlayerDataReason.ACL_Removed);
                        }
                        if (persistentPlayerData.ACL != null && persistentPlayerData.ACL.Contains(persistentPlayerData2.Value.UserIdentifier))
                        {
                            persistentPlayerData.RemovePlayerFromACL(persistentPlayerData2.Key);
                            persistentPlayerData.Dispatch(persistentPlayerData2.Value, EnumPersistentPlayerDataReason.ACL_Removed);
                        }
                    }
                }
                SavePersistentPlayerDataXML();
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

        public static bool ClaimedByNone(Vector3i _position)
        {
            Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_position);
            if (chunk != null)
            {
                PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
                if (persistentPlayerList != null)
                {
                    int claimSize = GameStats.GetInt(EnumGameStats.LandClaimSize);
                    Dictionary<Vector3i, PersistentPlayerData> claims = persistentPlayerList.m_lpBlockMap;
                    foreach (var claim in claims)
                    {
                        float distance = (claim.Key.ToVector3() - _position.ToVector3()).magnitude;
                        if (distance <= claimSize / 2 && GameManager.Instance.World.IsLandProtectionValidForPlayer(claim.Value))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
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

        public static bool ClaimedBySelfOrAlly(ClientInfo _cInfo, Vector3i _position)
        {
            PersistentPlayerList persistentPlayerList = GetPersistentPlayerList();
            if (persistentPlayerList != null)
            {
                int claimSize = GameStats.GetInt(EnumGameStats.LandClaimSize);
                Dictionary<Vector3i, PersistentPlayerData> claims = persistentPlayerList.m_lpBlockMap;
                foreach (var claim in claims)
                {
                    float distance = (claim.Key.ToVector3() - _position.ToVector3()).magnitude;
                    if (distance <= claimSize / 2 && GameManager.Instance.World.IsLandProtectionValidForPlayer(claim.Value))
                    {
                        if (claim.Value.EntityId == _cInfo.entityId)
                        {
                            return true;
                        }
                        else if (claim.Value.ACL.Contains(_cInfo.PlatformId) || claim.Value.ACL.Contains(_cInfo.CrossplatformId))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        public static void EntityIdList()
        {
            if (EntityClass.list.Dict != null && EntityClass.list.Dict.Count > 0)
            {
                int count = 1;
                foreach (KeyValuePair<int, EntityClass> entityClass in EntityClass.list.Dict)
                {
                    if (entityClass.Value.bAllowUserInstantiate)
                    {
                        if (!EntityId.ContainsKey(count))
                        {
                            EntityId.Add(count, entityClass.Key);
                        }
                        count++;
                    }
                }
            }
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
            ItemValue itemValue = ItemClass.GetItem(itemName, false);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in GeneralFunctions.CreatePassword: {0}", e.Message));
            }
            return pass;
        }

        public static long ConvertIPToLong(string ipAddress)
        {
            try
            {
                if (IPAddress.TryParse(ipAddress, out IPAddress ip))
                {
                    byte[] bytes = ip.MapToIPv4().GetAddressBytes();
                    return 16777216L * bytes[0] +
                        65536 * bytes[1] +
                        256 * bytes[2] +
                        bytes[3];
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in GeneralFunctions.ConvertIPToLong: {0}", e.Message));
            }
            return 0;
        }

        public static void GetCurrencyName()
        {
            List<ItemClass> itemClassCurrency = ItemClass.GetItemsWithTag(FastTags.Parse("currency"));
            if (itemClassCurrency != null && itemClassCurrency.Count > 0)
            {
                Currency_Item = itemClassCurrency[0].Name;
                Log.Out(string.Format("[SERVERTOOLS] Game currency and exchange set to item named '{0}'", Currency_Item));
            }
            else
            {
                No_Currency = true;
                Log.Out(string.Format("[SERVERTOOLS] Unable to find an item with the tag 'currency' in the item list. Bank and Wallet tools have been disabled. Anything relying on the Wallet for exchange will also not work. If this is the first time running ServerTools or it was recently updated, you may need to restart one more time"));
            }
        }

        public static void GetMeleeHandPlayer()
        {
            ItemValue meleeHand = ItemClass.GetItem("meleeHandPlayer");
            if (meleeHand != null)
            {
                MeleeHandPlayer = meleeHand.GetItemId();
            }
        }

        public static void JailPlayer(ClientInfo _cInfoKiller)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("st-Jail add {0} 120", _cInfoKiller.CrossplatformId.CombinedString), null);
            Phrases.Dict.TryGetValue("Jail1", out string phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfoKiller.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void KillPlayer(ClientInfo _cInfo)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.CrossplatformId.CombinedString), null);
            Phrases.Dict.TryGetValue("Zones4", out string phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void KickPlayer(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue("Zones6", out string phrase);
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
            Phrases.Dict.TryGetValue("Zones5", out phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void BanPlayer(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue("Zones8", out string phrase);
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
            Phrases.Dict.TryGetValue("Zones7", out phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void CommandsList(ClientInfo _cInfo)
        {
            try
            {
                string commands = "";
                if (AdminList.IsEnabled && AdminList.Command_adminlist != "")
                {
                    if (CommandList.Dict.TryGetValue(AdminList.Command_adminlist, out string[] values))
                    {
                        if (bool.TryParse(values[1], out bool hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AdminList.Command_adminlist);
                            }
                        }
                    }
                }
                if (AllocsMap.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue(AllocsMap.Command_map, out string[] values))
                    {
                        if (bool.TryParse(values[1], out bool hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AllocsMap.Command_map);
                            }
                        }
                    }
                }
                if (AnimalTracking.IsEnabled)
                {
                    if (AnimalTracking.Command_trackanimal != "")
                    {
                        if (CommandList.Dict.TryGetValue(AnimalTracking.Command_trackanimal, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AnimalTracking.Command_trackanimal);
                                }
                            }
                        }
                    }
                    if (AnimalTracking.Command_track != "")
                    {
                        if (CommandList.Dict.TryGetValue(AnimalTracking.Command_track, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AnimalTracking.Command_track);
                                }
                            }
                        }
                    }
                }
                if (Auction.IsEnabled)
                {
                    if (Auction.Command_auction != "")
                    {
                        if (CommandList.Dict.TryGetValue(Auction.Command_auction, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction);
                                }
                            }
                        }
                    }
                    if (Auction.Command_auction_cancel != "")
                    {
                        if (CommandList.Dict.TryGetValue(Auction.Command_auction_cancel, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_cancel);
                                }
                            }
                        }
                    }
                    if (Auction.Command_auction_buy != "")
                    {
                        if (CommandList.Dict.TryGetValue(Auction.Command_auction_buy, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_buy);
                                }
                            }
                        }
                    }
                    if (Auction.Command_auction_sell != "")
                    {
                        if (CommandList.Dict.TryGetValue(Auction.Command_auction_sell, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_sell);
                                }
                            }
                        }
                    }
                }
                if (AutoPartyInvite.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue(AutoPartyInvite.Command_party_add, out string[] values1))
                    {
                        if (bool.TryParse(values1[1], out bool hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AutoPartyInvite.Command_party_add);
                            }
                        }
                    }
                    if (CommandList.Dict.TryGetValue(AutoPartyInvite.Command_party_remove, out string[] values2))
                    {
                        if (bool.TryParse(values2[1], out bool hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AutoPartyInvite.Command_party_remove);
                            }
                        }
                    }
                    if (CommandList.Dict.TryGetValue(AutoPartyInvite.Command_party, out string[] values3))
                    {
                        if (bool.TryParse(values3[1], out bool hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, AutoPartyInvite.Command_party);
                            }
                        }
                    }
                }
                if (Bank.IsEnabled)
                {
                    if (Bank.Command_bank != "")
                    {
                        if (CommandList.Dict.TryGetValue(Bank.Command_bank, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Bank.Command_bank);
                                }
                            }
                        }
                    }
                    if (Wallet.IsEnabled && Bank.Command_deposit != "")
                    {
                        if (CommandList.Dict.TryGetValue(Bank.Command_deposit, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Bank.Command_deposit);
                                }
                            }
                        }
                    }
                    if (Wallet.IsEnabled && Bank.Command_withdraw != "")
                    {
                        if (CommandList.Dict.TryGetValue(Bank.Command_withdraw, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Bank.Command_withdraw);
                                }
                            }
                        }
                    }
                    if (Bank.Player_Transfers)
                    {
                        if (Bank.Command_transfer != "")
                        {
                            if (CommandList.Dict.TryGetValue(Bank.Command_transfer, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Bank.Command_transfer);
                                    }
                                }
                            }
                        }
                    }
                }
                if (Bed.IsEnabled)
                {
                    if (Bed.Command_bed != "")
                    {
                        if (CommandList.Dict.TryGetValue(Bed.Command_bed, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Bed.Command_bed);
                                }
                            }
                        }
                    }
                }
                if (Bloodmoon.IsEnabled)
                {
                    if (Bloodmoon.Command_bloodmoon != "")
                    {
                        if (CommandList.Dict.TryGetValue(Bloodmoon.Command_bloodmoon, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Bloodmoon.Command_bloodmoon);
                                }
                            }
                        }
                    }
                }
                if (Bounties.IsEnabled)
                {
                    if (Bounties.Command_bounty != "")
                    {
                        if (CommandList.Dict.TryGetValue(Bounties.Command_bounty, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Bounties.Command_bounty);
                                }
                            }
                        }
                    }
                    if (Bounties.Command_bounty != "")
                    {
                        if (CommandList.Dict.TryGetValue(Bounties.Command_bounty, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Bounties.Command_bounty);
                                }
                            }
                        }
                    }
                }
                if (ChatColor.IsEnabled && ChatColor.Players.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    if (ChatColor.Command_ccc != "")
                    {
                        if (CommandList.Dict.TryGetValue(ChatColor.Command_ccc, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccc);
                                }
                            }
                        }
                    }
                    if (ChatColor.Rotate)
                    {
                        if (ChatColor.Command_ccpr != "")
                        {
                            if (CommandList.Dict.TryGetValue(ChatColor.Command_ccpr, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccpr);
                                    }
                                }
                            }
                        }
                        if (ChatColor.Command_ccnr != "")
                        {
                            if (CommandList.Dict.TryGetValue(ChatColor.Command_ccnr, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccnr);
                                    }
                                }
                            }
                        }
                    }
                    if (ChatColor.Custom_Color)
                    {
                        if (ChatColor.Command_ccpr != "")
                        {
                            if (CommandList.Dict.TryGetValue(ChatColor.Command_ccpr, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2} [******]", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccpr);
                                    }
                                }
                            }
                        }
                        if (ChatColor.Command_ccnr != "")
                        {
                            if (CommandList.Dict.TryGetValue(ChatColor.Command_ccnr, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2} [******]", commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccnr);
                                    }
                                }
                            }
                        }
                    }
                }
                if (ClanManager.IsEnabled)
                {
                    if (ClanManager.Command_chat != "")
                    {
                        if (CommandList.Dict.TryGetValue(ClanManager.Command_chat, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_chat);
                                }
                            }
                        }
                    }
                    if (ClanManager.Command_clan_list != "")
                    {
                        if (CommandList.Dict.TryGetValue(ClanManager.Command_clan_list, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_clan_list);
                                }
                            }
                        }
                    }
                    if (!ClanManager.ClanMember.Contains(_cInfo.CrossplatformId.CombinedString))
                    {
                        if (ClanManager.Command_add != "")
                        {
                            if (CommandList.Dict.TryGetValue(ClanManager.Command_add, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2} clanName", commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_add);
                                    }
                                }
                            }
                        }
                        if (ClanManager.Command_request != "")
                        {
                            if (CommandList.Dict.TryGetValue(ClanManager.Command_request, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2} clanName", commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_request);
                                    }
                                }
                            }
                        }
                    }
                }
                if (Day7.IsEnabled)
                {
                    if (Day7.Command_day7 != "")
                    {
                        if (CommandList.Dict.TryGetValue(Day7.Command_day7, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Day7.Command_day7);
                                }
                            }
                        }
                    }
                }
                if (Died.IsEnabled)
                {
                    if (Died.Command_died != "")
                    {
                        if (CommandList.Dict.TryGetValue(Died.Command_died, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Died.Command_died);
                                }
                            }
                        }
                    }
                }
                if (DiscordLink.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue(DiscordLink.Command_discord, out string[] values))
                    {
                        if (bool.TryParse(values[1], out bool hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, DiscordLink.Command_discord);
                            }
                        }
                    }
                }
                if (ExitCommand.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue(ExitCommand.Command_exit, out string[] values))
                    {
                        if (bool.TryParse(values[1], out bool hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, ExitCommand.Command_exit);
                            }
                        }
                    }
                }
                if (FirstClaimBlock.IsEnabled)
                {
                    if (FirstClaimBlock.Command_claim != "")
                    {
                        if (CommandList.Dict.TryGetValue(FirstClaimBlock.Command_claim, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, FirstClaimBlock.Command_claim);
                                }
                            }
                        }
                    }
                }
                if (Fps.IsEnabled)
                {
                    if (Fps.Command_fps != "")
                    {
                        if (CommandList.Dict.TryGetValue(Fps.Command_fps, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Fps.Command_fps);
                                }
                            }
                        }
                    }
                }
                if (FriendTeleport.IsEnabled)
                {
                    if (FriendTeleport.Command_friend != "")
                    {
                        if (CommandList.Dict.TryGetValue(FriendTeleport.Command_friend, out string[] values1))
                        {
                            if (bool.TryParse(values1[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_friend);
                                }
                            }
                        }
                        if (CommandList.Dict.TryGetValue(FriendTeleport.Command_friend, out string[] values2))
                        {
                            if (bool.TryParse(values2[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_friend);
                                }
                            }
                        }
                    }
                    if (FriendTeleport.Command_accept != "" && FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
                    {
                        if (CommandList.Dict.TryGetValue(FriendTeleport.Command_accept, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_accept);
                                }
                            }
                        }
                    }
                }
                if (Gimme.IsEnabled)
                {
                    if (Gimme.Command_gimme != "")
                    {
                        if (CommandList.Dict.TryGetValue(Gimme.Command_gimme, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Gimme.Command_gimme);
                                }
                            }
                        }
                    }
                }
                if (Hardcore.IsEnabled)
                {
                    if (!Hardcore.Optional)
                    {
                        if (Hardcore.Command_top3 != "")
                        {
                            if (CommandList.Dict.TryGetValue(Hardcore.Command_top3, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_top3);
                                    }
                                }
                            }
                        }
                        if (Hardcore.Command_score != "")
                        {
                            if (CommandList.Dict.TryGetValue(Hardcore.Command_score, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_score);
                                    }
                                }
                            }
                        }
                        if (Hardcore.Command_hardcore != "")
                        {
                            if (CommandList.Dict.TryGetValue(Hardcore.Command_hardcore, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore);
                                    }
                                }
                            }
                        }
                        if (Hardcore.Max_Extra_Lives > 0)
                        {
                            if (Hardcore.Command_buy_life != "")
                            {
                                if (CommandList.Dict.TryGetValue(Hardcore.Command_buy_life, out string[] values))
                                {
                                    if (bool.TryParse(values[1], out bool hidden))
                                    {
                                        if (!hidden)
                                        {
                                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_buy_life);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Hardcore.Command_top3 != "")
                        {
                            if (CommandList.Dict.TryGetValue(Hardcore.Command_top3, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_top3);
                                    }
                                }
                            }
                        }
                        if (Hardcore.Command_score != "")
                        {
                            if (CommandList.Dict.TryGetValue(Hardcore.Command_score, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_score);
                                    }
                                }
                            }
                        }
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreEnabled)
                        {
                            if (Hardcore.Command_hardcore != "")
                            {
                                if (CommandList.Dict.TryGetValue(Hardcore.Command_hardcore, out string[] values))
                                {
                                    if (bool.TryParse(values[1], out bool hidden))
                                    {
                                        if (!hidden)
                                        {
                                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore);
                                        }
                                    }
                                }
                            }
                            if (Hardcore.Max_Extra_Lives > 0)
                            {
                                if (Hardcore.Command_buy_life != "")
                                {
                                    if (CommandList.Dict.TryGetValue(Hardcore.Command_buy_life, out string[] values))
                                    {
                                        if (bool.TryParse(values[1], out bool hidden))
                                        {
                                            if (!hidden)
                                            {
                                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_buy_life);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Hardcore.Command_hardcore_on != "")
                            {
                                if (CommandList.Dict.TryGetValue(Hardcore.Command_hardcore_on, out string[] values))
                                {
                                    if (bool.TryParse(values[1], out bool hidden))
                                    {
                                        if (!hidden)
                                        {
                                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore_on);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (Homes.IsEnabled)
                {
                    if (Homes.Command_home != "")
                    {
                        if (CommandList.Dict.TryGetValue(Homes.Command_home, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_home);
                                }
                            }
                        }
                    }
                    if (Homes.Command_fhome != "")
                    {
                        if (CommandList.Dict.TryGetValue(Homes.Command_fhome, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_fhome);
                                }
                            }
                        }
                    }
                    if (Homes.Command_save != "")
                    {
                        if (CommandList.Dict.TryGetValue(Homes.Command_save, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_save);
                                }
                            }
                        }
                    }
                    if (Homes.Command_delete != "")
                    {
                        if (CommandList.Dict.TryGetValue(Homes.Command_delete, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_delete);
                                }
                            }
                        }
                    }
                    if (Homes.Command_go != "" && Homes.Invite.ContainsKey(_cInfo.entityId))
                    {
                        if (CommandList.Dict.TryGetValue(Homes.Command_go, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_go);
                                }
                            }
                        }
                    }
                    if (Homes.Command_set != "")
                    {
                        if (CommandList.Dict.TryGetValue(Homes.Command_set, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Homes.Command_set);
                                }
                            }
                        }
                    }
                }
                if (InfoTicker.IsEnabled)
                {
                    if (InfoTicker.Command_infoticker != "")
                    {
                        if (CommandList.Dict.TryGetValue(InfoTicker.Command_infoticker, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, InfoTicker.Command_infoticker);
                                }
                            }
                        }
                    }
                }
                if (KickVote.IsEnabled)
                {
                    if (KickVote.Command_kickvote != "")
                    {
                        if (CommandList.Dict.TryGetValue(KickVote.Command_kickvote, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, KickVote.Command_kickvote);
                                }
                            }
                        }
                    }
                }
                if (Lobby.IsEnabled)
                {
                    if (Lobby.Command_lobby != "")
                    {
                        if (CommandList.Dict.TryGetValue(Lobby.Command_lobby, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Lobby.Command_lobby);
                                }
                            }
                        }
                    }
                    if (Lobby.Return && Lobby.LobbyPlayers.Contains(_cInfo.entityId))
                    {
                        if (Lobby.Command_lobbyback != "")
                        {
                            if (CommandList.Dict.TryGetValue(Lobby.Command_lobbyback, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Lobby.Command_lobbyback);
                                    }
                                }
                            }
                        }
                    }
                }
                if (Loc.IsEnabled)
                {
                    if (Loc.Command_loc != "")
                    {
                        if (CommandList.Dict.TryGetValue(Loc.Command_loc, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Loc.Command_loc);
                                }
                            }
                        }
                    }
                }
                if (Lottery.IsEnabled)
                {
                    if (Lottery.Command_lottery != "")
                    {
                        if (CommandList.Dict.TryGetValue(Lottery.Command_lottery, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery);
                                }
                            }
                        }
                    }
                    if (Lottery.Command_lottery != "")
                    {
                        if (CommandList.Dict.TryGetValue(Lottery.Command_lottery, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} #", commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery);
                                }
                            }
                        }
                    }
                    if (Lottery.Command_lottery_enter != "")
                    {
                        if (CommandList.Dict.TryGetValue(Lottery.Command_lottery_enter, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery_enter);
                                }
                            }
                        }
                    }
                }
                if (Market.IsEnabled)
                {
                    if (Market.Command_market != "")
                    {
                        if (CommandList.Dict.TryGetValue(Market.Command_market, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Market.Command_market);
                                }
                            }
                        }
                    }
                    if (Market.Return && Market.MarketPlayers.Contains(_cInfo.entityId))
                    {
                        if (Market.Command_marketback != "")
                        {
                            if (CommandList.Dict.TryGetValue(Market.Command_marketback, out string[] values))
                            {
                                if (bool.TryParse(values[1], out bool hidden))
                                {
                                    if (!hidden)
                                    {
                                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Market.Command_marketback);
                                    }
                                }
                            }
                        }
                    }
                }
                if (Mute.IsEnabled)
                {
                    if (Mute.Command_mute != "")
                    {
                        if (CommandList.Dict.TryGetValue(Mute.Command_mute, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Mute.Command_mute);
                                }
                            }
                        }
                    }
                    if (Mute.Command_unmute != "")
                    {
                        if (CommandList.Dict.TryGetValue(Mute.Command_unmute, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Mute.Command_unmute);
                                }
                            }
                        }
                    }
                    if (Mute.Command_mutelist != "")
                    {
                        if (CommandList.Dict.TryGetValue(Mute.Command_mutelist, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Mute.Command_mutelist);
                                }
                            }
                        }
                    }
                }
                if (MuteVote.IsEnabled && Mute.IsEnabled)
                {
                    if (MuteVote.Command_mutevote != "")
                    {
                        if (CommandList.Dict.TryGetValue(MuteVote.Command_mutevote, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, MuteVote.Command_mutevote);
                                }
                            }
                        }
                    }
                }
                if (BlockPickup.IsEnabled)
                {
                    if (BlockPickup.Command_pickup != "")
                    {
                        if (CommandList.Dict.TryGetValue(BlockPickup.Command_pickup, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, BlockPickup.Command_pickup);
                                }
                            }
                        }
                    }
                }
                if (PlayerList.IsEnabled)
                {
                    if (PlayerList.Command_playerlist != "")
                    {
                        if (CommandList.Dict.TryGetValue(PlayerList.Command_playerlist, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, PlayerList.Command_playerlist);
                                }
                            }
                        }
                    }
                }
                if (Prayer.IsEnabled)
                {
                    if (Prayer.Command_pray != "")
                    {
                        if (CommandList.Dict.TryGetValue(Prayer.Command_pray, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Prayer.Command_pray);
                                }
                            }
                        }
                    }
                }
                if (Report.IsEnabled)
                {
                    if (Report.Command_report != "")
                    {
                        if (CommandList.Dict.TryGetValue(Report.Command_report, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Report.Command_report);
                                }
                            }
                        }
                    }
                }
                if (RestartVote.IsEnabled)
                {
                    if (RestartVote.Command_restartvote != "")
                    {
                        if (CommandList.Dict.TryGetValue(RestartVote.Command_restartvote, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, RestartVote.Command_restartvote);
                                }
                            }
                        }
                    }
                }
                if (Shop.IsEnabled)
                {
                    if (Shop.Command_shop != "")
                    {
                        if (CommandList.Dict.TryGetValue(Shop.Command_shop, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Shop.Command_shop);
                                }
                            }
                        }
                    }
                    if (Shop.Command_shop_buy != "")
                    {
                        if (CommandList.Dict.TryGetValue(Shop.Command_shop_buy, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Shop.Command_shop_buy);
                                }
                            }
                        }
                    }
                }
                if (Shutdown.IsEnabled)
                {
                    if (Shutdown.Command_shutdown != "")
                    {
                        if (CommandList.Dict.TryGetValue(Shutdown.Command_shutdown, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Shutdown.Command_shutdown);
                                }
                            }
                        }
                    }
                }
                if (Stuck.IsEnabled)
                {
                    if (Stuck.Command_stuck != "")
                    {
                        if (CommandList.Dict.TryGetValue(Stuck.Command_stuck, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Stuck.Command_stuck);
                                }
                            }
                        }
                    }
                }
                if (Suicide.IsEnabled)
                {
                    if (Suicide.Command_killme != "")
                    {
                        if (CommandList.Dict.TryGetValue(Suicide.Command_killme, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Suicide.Command_killme);
                                }
                            }
                        }
                    }
                    if (Suicide.Command_suicide != "")
                    {
                        if (CommandList.Dict.TryGetValue(Suicide.Command_suicide, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Suicide.Command_suicide);
                                }
                            }
                        }
                    }
                }
                if (Travel.IsEnabled)
                {
                    if (Travel.Command_travel != "")
                    {
                        if (CommandList.Dict.TryGetValue(Travel.Command_travel, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Travel.Command_travel);
                                }
                            }
                        }
                    }
                }
                if (VehicleRecall.IsEnabled)
                {
                    if (CommandList.Dict.TryGetValue(VehicleRecall.Command_vehicle, out string[] values))
                    {
                        if (bool.TryParse(values[1], out bool hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, VehicleRecall.Command_vehicle);
                            }
                        }
                    }
                }
                if (Voting.IsEnabled)
                {
                    if (Voting.Command_reward != "")
                    {
                        if (CommandList.Dict.TryGetValue(Voting.Command_reward, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Voting.Command_reward);
                                }
                            }
                        }
                    }
                    if (Voting.Command_vote != "")
                    {
                        if (CommandList.Dict.TryGetValue(Voting.Command_vote, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Voting.Command_vote);
                                }
                            }
                        }
                    }
                }
                if (Wall.IsEnabled)
                {
                    if (Wall.Command_wall != "")
                    {
                        if (CommandList.Dict.TryGetValue(Wall.Command_wall, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Wall.Command_wall);
                                }
                            }
                        }
                    }
                }
                if (Waypoints.IsEnabled)
                {
                    if (Waypoints.Command_waypoint != "")
                    {
                        if (CommandList.Dict.TryGetValue(Waypoints.Command_waypoint, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint);
                                }
                            }
                        }
                    }
                    if (Waypoints.Command_waypoint != "")
                    {
                        if (CommandList.Dict.TryGetValue(Waypoints.Command_waypoint, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint);
                                }
                            }
                        }
                    }
                    if (Waypoints.Command_waypoint_save != "")
                    {
                        if (CommandList.Dict.TryGetValue(Waypoints.Command_waypoint_save, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint_save);
                                }
                            }
                        }
                    }
                    if (Waypoints.Command_waypoint_del != "")
                    {
                        if (CommandList.Dict.TryGetValue(Waypoints.Command_waypoint_del, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint_del);
                                }
                            }
                        }
                    }
                    if (Waypoints.Command_fwaypoint != "")
                    {
                        if (CommandList.Dict.TryGetValue(Waypoints.Command_fwaypoint, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} 'name'", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_fwaypoint);
                                }
                            }
                        }
                    }
                    if (Waypoints.Command_go_way != "" && Waypoints.Invite.ContainsKey(_cInfo.entityId))
                    {
                        if (CommandList.Dict.TryGetValue(Waypoints.Command_go_way, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_go_way);
                                }
                            }
                        }
                    }
                }
                if (Whisper.IsEnabled)
                {
                    if (Whisper.Command_rmessage != "")
                    {
                        if (CommandList.Dict.TryGetValue(Whisper.Command_rmessage, out string[] values))
                        {
                            if (bool.TryParse(values[1], out bool hidden))
                            {
                                if (!hidden)
                                {
                                    commands = string.Format("{0} {1}{2} {3}{4}", commands, ChatHook.Chat_Command_Prefix1, Whisper.Command_pmessage, ChatHook.Chat_Command_Prefix1, Whisper.Command_rmessage);
                                }
                            }
                        }
                    }
                }
                
                if (ReservedSlots.IsEnabled && (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                {
                    if (CommandList.Dict.TryGetValue(Command_expire, out string[] values))
                    {
                        if (bool.TryParse(values[1], out bool hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Command_expire);
                            }
                        }
                    }
                }
                else if (ChatColor.IsEnabled && (ChatColor.Players.ContainsKey(_cInfo.PlatformId.CombinedString) || ChatColor.Players.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                {
                    if (CommandList.Dict.TryGetValue(Command_expire, out string[] values))
                    {
                        if (bool.TryParse(values[1], out bool hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Command_expire);
                            }
                        }
                    }
                }
                else if (LoginNotice.IsEnabled && (LoginNotice.Dict1.ContainsKey(_cInfo.PlatformId.CombinedString) || LoginNotice.Dict1.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                {
                    if (CommandList.Dict.TryGetValue(Command_expire, out string[] values))
                    {
                        if (bool.TryParse(values[1], out bool hidden))
                        {
                            if (!hidden)
                            {
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Command_expire);
                            }
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
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.CustomCommandList: {0}", e.Message));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.AdminCommandList: {0}", e.Message));
            }
        }

        public static void SetWindowLinks()
        {
            Auction.SetLink();
            AllocsMap.SetLink(AllocsMap.Link);
            DiscordLink.SetLink(DiscordLink.Invitation_Link);
            Shop.SetLink();
            RIO.SetLink();
            InteractiveMap.SetLink();
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

        public static bool SessionBonus(string _id)
        {
            ClientInfo cInfo = GetClientInfoFromNameOrId(_id);
            if (cInfo != null)
            {
                if (Wallet.IsEnabled && Wallet.Session_Bonus > 0)
                {
                    Wallet.AddCurrency(cInfo.CrossplatformId.CombinedString, Wallet.Session_Bonus, true);
                }
                return true;
            }
            return false;
        }
    }
}
