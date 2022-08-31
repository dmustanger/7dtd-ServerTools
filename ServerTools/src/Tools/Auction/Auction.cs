using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Auction
    {
        public static bool IsEnabled = false, IsRunning = false, No_Admins = false, Panel = false;
        public static int Admin_Level = 0, Total_Items = 1, Tax = 0;
        public static string Command_auction = "auction", Command_auction_cancel = "auction cancel", Command_auction_buy = "auction buy",
            Command_auction_sell = "auction sell", Panel_Name = "Awesome Auction";

        public static Dictionary<int, string> AuctionItems = new Dictionary<int, string>();
        public static Dictionary<string, int> PanelAccess = new Dictionary<string, int>();
        public static List<ushort> DroppedSeed = new List<ushort>();

        private static readonly string file = string.Format("Auction_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static readonly string Filepath = string.Format("{0}/Logs/AuctionLogs/{1}", API.ConfigPath, file);
        private static readonly string AlphaNumSet = "JKQRLWBYZMPSNODHEFXTACGIUV";

        public static void SetLink()
        {
            try
            {
                if (File.Exists(PersistentOperations.XPathDir + "XUi/windows.xml"))
                {
                    string link = string.Format("http://{0}:{1}/auction.html", WebAPI.BaseAddress, WebAPI.Port);
                    List<string> lines = File.ReadAllLines(PersistentOperations.XPathDir + "XUi/windows.xml").ToList();
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Contains("browserAuction"))
                        {
                            if (!lines[i + 7].Contains(link))
                            {
                                lines[i + 7] = string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", link);
                                File.WriteAllLines(PersistentOperations.XPathDir + "XUi/windows.xml", lines.ToArray());
                            }
                            return;
                        }
                    }
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Contains("/append"))
                        {
                            lines.RemoveRange(i, 3);
                            lines.Add("  <window name=\"browserAuction\" controller=\"ServerInfo\">");
                            lines.Add("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                            lines.Add("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Auction\" />");
                            lines.Add("      </panel>");
                            lines.Add("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                            lines.Add("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                            lines.Add("          <label name=\"ServerDescription\" />");
                            lines.Add(string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", link));
                            lines.Add("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                            lines.Add("          <sprite depth=\"4\" name=\"shoppingCartIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_shopping_cart\" />");
                            lines.Add("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                            lines.Add("          <!-- Change the text IP and Port to the one needed by ServerTools web api -->");
                            lines.Add("      </panel>");
                            lines.Add("  </window>");
                            lines.Add("");
                            lines.Add("</append>");
                            lines.Add("");
                            lines.Add("</configs>");
                            File.WriteAllLines(PersistentOperations.XPathDir + "XUi/windows.xml", lines.ToArray());
                            return;
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", PersistentOperations.XPathDir + "XUi/windows.xml", e.Message));
            }
        }

        public static void SellItem(ClientInfo _cInfo, string _price)
        {
            try
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (int.TryParse(_price, out int auctionPrice))
                    {
                        if (auctionPrice > 0)
                        {
                            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction == null)
                            {
                                Dictionary<int, ItemDataSerializable> auctionItems = new Dictionary<int, ItemDataSerializable>();
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction = auctionItems;
                                PersistentContainer.DataChange = true;
                            }
                            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction.Count < Total_Items + PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].AuctionCount)
                            {
                                List<Chunk> chunks = new List<Chunk>();
                                DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
                                Vector3 position = player.position;
                                Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x - 5, (int)position.y, (int)position.z);
                                if (chunk != null && !chunks.Contains(chunk))
                                {
                                    chunks.Add(chunk);
                                }
                                chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x + 5, (int)position.y, (int)position.z);
                                if (chunk != null && !chunks.Contains(chunk))
                                {
                                    chunks.Add(chunk);
                                }
                                chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x, (int)position.y, (int)position.z - 5);
                                if (chunk != null && !chunks.Contains(chunk))
                                {
                                    chunks.Add(chunk);
                                }
                                chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x + 5, (int)position.y, (int)position.z + 5);
                                if (chunk != null && !chunks.Contains(chunk))
                                {
                                    chunks.Add(chunk);
                                }
                                for (int i = 0; i < chunks.Count; i++)
                                {
                                    tiles = chunks[i].GetTileEntities();
                                    foreach (TileEntity tile in tiles.dict.Values)
                                    {
                                        if (tile is TileEntitySecureLootContainer)
                                        {
                                            TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)tile;
                                            Vector3i vec3i = SecureLoot.ToWorldPos();
                                            if ((vec3i.x - player.position.x) * (vec3i.x - player.position.x) + (vec3i.z - player.position.z) * (vec3i.z - player.position.z) <= 3 * 3)
                                            {
                                                if (vec3i.y >= (int)player.position.y - 3 && vec3i.y <= (int)player.position.y + 3)
                                                {
                                                    if ((SecureLoot.IsUserAllowed(_cInfo.PlatformId) || SecureLoot.IsUserAllowed(_cInfo.CrossplatformId)) && !SecureLoot.IsUserAccessing())
                                                    {
                                                        ItemStack[] items = SecureLoot.items;
                                                        ItemStack item = items[0];
                                                        if (item != null && !item.itemValue.IsEmpty())
                                                        {
                                                            if (item.itemValue.Modifications.Length > 0)
                                                            {
                                                                for (int j = 0; j < item.itemValue.Modifications.Length; j++)
                                                                {
                                                                    if (!item.itemValue.Modifications[j].IsEmpty())
                                                                    {
                                                                        Phrases.Dict.TryGetValue("Auction18", out string phrase);
                                                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                                        return;
                                                                    }
                                                                }
                                                            }
                                                            if (item.itemValue.CosmeticMods.Length > 0)
                                                            {
                                                                for (int j = 0; j < item.itemValue.CosmeticMods.Length; j++)
                                                                {
                                                                    if (!item.itemValue.CosmeticMods[j].IsEmpty())
                                                                    {
                                                                        Phrases.Dict.TryGetValue("Auction18", out string _phrase);
                                                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                                        return;
                                                                    }
                                                                }
                                                            }
                                                            int id = GenerateAuctionId();
                                                            if (id > 0)
                                                            {
                                                                AuctionItems.Add(id, _cInfo.CrossplatformId.CombinedString);
                                                                items[0] = ItemStack.Empty.Clone();
                                                                ItemDataSerializable itemData = new ItemDataSerializable();
                                                                {
                                                                    itemData.name = item.itemValue.ItemClass.GetItemName();
                                                                    itemData.localName = item.itemValue.ItemClass.GetLocalizedItemName() ?? item.itemValue.ItemClass.GetItemName();
                                                                    itemData.iconName = item.itemValue.ItemClass.GetIconName();
                                                                    itemData.count = item.count;
                                                                    itemData.useTimes = item.itemValue.UseTimes;
                                                                    itemData.maxUseTimes = item.itemValue.MaxUseTimes;
                                                                    itemData.quality = item.itemValue.Quality;
                                                                    itemData.price = auctionPrice;
                                                                    itemData.seed = item.itemValue.Seed;
                                                                    itemData.hasQuality = item.itemValue.HasQuality;
                                                                    if (item.itemValue.Modifications.Length > 0)
                                                                    {
                                                                        itemData.modSlots = item.itemValue.Modifications.Length;
                                                                    }
                                                                    else
                                                                    {
                                                                        itemData.modSlots = 0;
                                                                    }
                                                                    if (item.itemValue.CosmeticMods.Length > 0)
                                                                    {
                                                                        itemData.cosmeticSlots = item.itemValue.CosmeticMods.Length;
                                                                    }
                                                                    else
                                                                    {
                                                                        itemData.cosmeticSlots = 0;
                                                                    }
                                                                }
                                                                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction != null)
                                                                {
                                                                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction.Add(id, itemData);
                                                                }
                                                                else
                                                                {
                                                                    Dictionary<int, ItemDataSerializable> auctionItems = new Dictionary<int, ItemDataSerializable> { { id, itemData } };
                                                                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction = auctionItems;
                                                                }
                                                                tile.SetModified();
                                                                PersistentContainer.DataChange = true;
                                                                if (item.itemValue.HasQuality)
                                                                {
                                                                    using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                                                    {
                                                                        sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' has added '{4}' '{5}', '{6}' quality, '{7}' percent durability for '{8}' '{9}'", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, item.count, item.itemValue.ItemClass.GetItemName(), item.itemValue.Quality, item.itemValue.UseTimes / item.itemValue.MaxUseTimes * 100, _price, Wallet.Currency_Name));
                                                                        sw.WriteLine();
                                                                        sw.Flush();
                                                                        sw.Close();
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                                                    {
                                                                        sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' has added '{4}' '{5}' for '{6}' '{7}'", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, item.count, item.itemValue.ItemClass.GetItemName(), _price, Wallet.Currency_Name));
                                                                        sw.WriteLine();
                                                                        sw.Flush();
                                                                        sw.Close();
                                                                    }
                                                                }
                                                                Phrases.Dict.TryGetValue("Auction1", out string phrase);
                                                                phrase = phrase.Replace("{Name}", item.itemValue.ItemClass.GetLocalizedItemName() ?? item.itemValue.ItemClass.GetItemName());
                                                                phrase = phrase.Replace("{Value}", id.ToString());
                                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                                return;
                                                            }
                                                            else
                                                            {
                                                                Phrases.Dict.TryGetValue("Auction16", out string phrase);
                                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                                return;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Auction2", out string phrase);
                                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                phrase = phrase.Replace("{Command_auction_cancel}", Command_auction_cancel);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                return;
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Auction3", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Auction4", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                    Phrases.Dict.TryGetValue("Auction21", out string phrase1);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Auction.CheckBox: {0}", e.Message));
            }
        }

        public static void AuctionList(ClientInfo _cInfo)
        {
            try
            {
                if (AuctionItems.Count > 0)
                {
                    if (Panel && WebAPI.IsEnabled && WebAPI.Connected)
                    {
                        if (!PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Overlay)
                        {
                            string securityId = "";
                            for (int i = 0; i < 10; i++)
                            {
                                string pass = CreatePassword(4);
                                if (pass != "DBUG" && !PanelAccess.ContainsKey(pass))
                                {
                                    securityId = pass;
                                    if (!PanelAccess.ContainsValue(_cInfo.entityId))
                                    {
                                        PanelAccess.Add(securityId, _cInfo.entityId);
                                        WebAPI.AuthorizedTime.Add(securityId, DateTime.Now.AddMinutes(5));
                                    }
                                    else
                                    {
                                        if (PanelAccess.Count > 0)
                                        {
                                            foreach (var client in PanelAccess)
                                            {
                                                if (client.Value == _cInfo.entityId)
                                                {
                                                    PanelAccess.Remove(client.Key);
                                                    WebAPI.AuthorizedTime.Remove(client.Key);
                                                    break;
                                                }
                                            }
                                        }
                                        PanelAccess.Add(securityId, _cInfo.entityId);
                                        WebAPI.AuthorizedTime.Add(securityId, DateTime.Now.AddMinutes(5));
                                    }
                                    break;
                                }
                            }
                            Phrases.Dict.TryGetValue("Auction20", out string phrase);
                            phrase = phrase.Replace("{Value}", securityId);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserAuction", true));
                            return;
                        }
                    }
                    List<string> IDs = new List<string>();
                    var auctionEntries = AuctionItems.ToArray();
                    for (int i = 0; i < auctionEntries.Length; i++)
                    {
                        string id = auctionEntries[i].Value;
                        if (!IDs.Contains(id) && PersistentContainer.Instance.Players[id].Auction != null && PersistentContainer.Instance.Players[id].Auction.Count > 0)
                        {
                            IDs.Add(id);
                            if (_cInfo.CrossplatformId.CombinedString == id)
                            {
                                Phrases.Dict.TryGetValue("Auction19", out string phrase);
                                phrase = phrase.Replace("{Value}", PersistentContainer.Instance.Players[id].Auction.Count.ToString());
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            var items = PersistentContainer.Instance.Players[id].Auction.ToArray();
                            for (int j = 0; j < items.Length; j++)
                            {
                                if (items[j].Value.hasQuality)
                                {
                                    Phrases.Dict.TryGetValue("Auction5", out string phrase);
                                    phrase = phrase.Replace("{Id}", items[j].Key.ToString());
                                    phrase = phrase.Replace("{Count}", items[j].Value.count.ToString());
                                    phrase = phrase.Replace("{Item}", items[j].Value.localName);
                                    phrase = phrase.Replace("{Quality}", items[j].Value.quality.ToString());
                                    phrase = phrase.Replace("{Durability}", ((items[j].Value.maxUseTimes - items[j].Value.useTimes) / items[j].Value.maxUseTimes * 100).ToString());
                                    phrase = phrase.Replace("{Price}", items[j].Value.price.ToString());
                                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Auction17", out string phrase);
                                    phrase = phrase.Replace("{Id}", items[j].Key.ToString());
                                    phrase = phrase.Replace("{Count}", items[j].Value.count.ToString());
                                    phrase = phrase.Replace("{Item}", items[j].Value.localName);
                                    phrase = phrase.Replace("{Price}", items[j].Value.price.ToString());
                                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Auction6", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Auction.AuctionList: {0}", e.Message));
            }
        }

        public static void WalletCheck(ClientInfo _cInfo, int _purchase)
        {
            if (AuctionItems.ContainsKey(_purchase))
            {
                AuctionItems.TryGetValue(_purchase, out string id);
                if (PersistentContainer.Instance.Players[id].Auction != null && PersistentContainer.Instance.Players[id].Auction.Count > 0)
                {
                    PersistentContainer.Instance.Players[id].Auction.TryGetValue(_purchase, out ItemDataSerializable itemData);
                    int currency = 0;
                    int bankValue = 0;
                    if (Wallet.IsEnabled)
                    {
                        currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                    }
                    if (Bank.IsEnabled && Bank.Payments)
                    {
                        bankValue = Bank.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                    }
                    if (currency + bankValue >= itemData.price)
                    {
                        BuyAuction(_cInfo, _purchase, id, itemData);
                    }
                    else
                    {
                        int missing = itemData.price - (currency + bankValue);
                        Phrases.Dict.TryGetValue("Auction7", out string phrase);
                        phrase = phrase.Replace("{Value}", missing.ToString());
                        phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Auction8", out string phrase);
                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                phrase = phrase.Replace("{Command_auction}", Command_auction);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void BuyAuction(ClientInfo _cInfo, int _purchase, string _id, ItemDataSerializable _itemData)
        {
            ItemValue itemValue = new ItemValue(ItemClass.GetItem(_itemData.name, false).type);
            if (itemValue != null)
            {
                if (itemValue.ItemClass.HasQuality)
                {
                    itemValue.Quality = 1;
                    if (_itemData.quality > 0)
                    {
                        itemValue.Quality = _itemData.quality;
                    }
                }
                itemValue.UseTimes = _itemData.useTimes;
                itemValue.Seed = _itemData.seed;
                if (_itemData.modSlots > 0)
                {
                    itemValue.Modifications = new ItemValue[_itemData.modSlots];
                }
                if (_itemData.cosmeticSlots > 0)
                {
                    itemValue.CosmeticMods = new ItemValue[_itemData.cosmeticSlots];
                }
                World world = GameManager.Instance.World;
                EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                {
                    entityClass = EntityClass.FromString("item"),
                    id = EntityFactory.nextEntityID++,
                    itemStack = new ItemStack(itemValue, _itemData.count),
                    pos = world.Players.dict[_cInfo.entityId].position,
                    rot = new Vector3(20f, 0f, 20f),
                    lifetime = 60f,
                    belongsPlayerId = _cInfo.entityId
                });
                world.SpawnEntityInWorld(entityItem);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                AuctionItems.Remove(_purchase);
                PersistentContainer.Instance.Players[_id].Auction.Remove(_purchase);
                PersistentContainer.DataChange = true;
                if (_itemData.price >= 1)
                {
                    if (Bank.IsEnabled && Bank.Payments)
                    {
                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, _itemData.price, true);
                    }
                    else
                    {
                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, _itemData.price, false);
                    }
                }
                float fee = _itemData.price * ((float)Tax / 100);
                int adjustedPrice = _itemData.price - (int)fee;
                Wallet.AddCurrency(_id, adjustedPrice);
                string playerName = PersistentOperations.GetPlayerDataFileFromId(_id).ecd.entityName;
                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' has purchased auction entry '{3}', profits went to ID '{4}' named '{5}'", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _purchase, _id, playerName));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Phrases.Dict.TryGetValue("Auction9", out string phrase);
                phrase = phrase.Replace("{Count}", _itemData.count.ToString());
                phrase = phrase.Replace("{ItemName}", _itemData.name);
                phrase = phrase.Replace("{Value}", _itemData.price.ToString());
                phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                ClientInfo cInfo2 = PersistentOperations.GetClientInfoFromNameOrId(_id);
                if (cInfo2 != null)
                {
                    Phrases.Dict.TryGetValue("Auction10", out string phrase1);
                    ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void CancelAuction(ClientInfo _cInfo, string _auctionId)
        {
            if (int.TryParse(_auctionId, out int itemId))
            {
                if (AuctionItems.ContainsKey(itemId))
                {
                    EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                    if (player != null)
                    {
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction.Count > 0)
                        {
                            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction.TryGetValue(itemId, out ItemDataSerializable itemData))
                            {
                                ItemValue itemValue = new ItemValue(ItemClass.GetItem(itemData.name, false).type);
                                if (itemValue != null)
                                {
                                    if (itemValue.HasQuality)
                                    {
                                        itemValue.Quality = 1;
                                        if (itemData.quality > 1)
                                        {
                                            itemValue.Quality = itemData.quality;
                                        }
                                    }
                                    itemValue.UseTimes = itemData.useTimes;
                                    itemValue.Seed = itemData.seed;
                                    if (itemData.modSlots > 0)
                                    {
                                        itemValue.Modifications = new ItemValue[itemData.modSlots];
                                    }
                                    if (itemData.cosmeticSlots > 0)
                                    {
                                        itemValue.CosmeticMods = new ItemValue[itemData.cosmeticSlots];
                                    }
                                    World world = GameManager.Instance.World;
                                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                    {
                                        entityClass = EntityClass.FromString("item"),
                                        id = EntityFactory.nextEntityID++,
                                        itemStack = new ItemStack(itemValue, itemData.count),
                                        pos = player.position,
                                        rot = new Vector3(20f, 0f, 20f),
                                        lifetime = 60f,
                                        belongsPlayerId = _cInfo.entityId
                                    });
                                    world.SpawnEntityInWorld(entityItem);
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                    AuctionItems.Remove(itemId);
                                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction.Remove(itemId);
                                    PersistentContainer.DataChange = true;
                                    using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                    {
                                        sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' has cancelled their auction entry number '{4}'", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, itemId));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                    Phrases.Dict.TryGetValue("Auction11", out string phrase);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Auction12", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void AuctionList()
        {
            List<string> playerlist = PersistentContainer.Instance.Players.IDs;
            for (int i = 0; i < playerlist.Count; i++)
            {
                string id = playerlist[i];
                PersistentPlayer p = PersistentContainer.Instance.Players[id];
                if (p != null)
                {
                    if (p.Auction != null && p.Auction.Count > 0)
                    {
                        foreach (var auctionItem in p.Auction)
                        {
                            AuctionItems.Add(auctionItem.Key, id);
                        }
                    }
                }
            }
        }

        private static int GenerateAuctionId()
        {
            for (int i = 0; i < 20; i++)
            {
                int id = new System.Random().Next(1000, 5001);
                if (!AuctionItems.ContainsKey(id))
                {
                    return id;
                }
            }
            return 0;
        }

        public static string CreatePassword(int _length)
        {
            string pass = string.Empty;
            System.Random rnd = new System.Random();
            for (int i = 0; i < _length; i++)
            {
                pass += AlphaNumSet.ElementAt(rnd.Next(0, 26));
            }
            return pass;
        }

        public static string GetItems(string _crossplatformId)
        {
            string auctionItems = "";
            if (AuctionItems.Count > 0)
            {
                List<string> IDs = new List<string>();
                var auctionEntries = AuctionItems.ToArray();
                for (int i = 0; i < auctionEntries.Length; i++)
                {
                    string id = auctionEntries[i].Value;
                    if (!IDs.Contains(id) && PersistentContainer.Instance.Players[id].Auction != null && PersistentContainer.Instance.Players[id].Auction.Count > 0)
                    {
                        IDs.Add(id);
                        var items = PersistentContainer.Instance.Players[id].Auction.ToArray();
                        for (int j = 0; j < items.Length; j++)
                        {
                            auctionItems += items[j].Key + "§" + items[j].Value.count + "§" + items[j].Value.name + "§" + items[j].Value.localName + "§" +
                                items[j].Value.quality + "§" + (items[j].Value.maxUseTimes - items[j].Value.useTimes) / items[j].Value.maxUseTimes * 100 + "§" +
                                items[j].Value.price + "§" + items[j].Value.iconName + "§";
                            if (_crossplatformId == id)
                            {
                                auctionItems += "true" + "╚";
                            }
                            else
                            {
                                auctionItems += "false" + "╚";
                            }
                        }
                    }
                }
            }
            if (auctionItems.Length > 0)
            {
                auctionItems = auctionItems.Remove(auctionItems.Length - 1);
            }
            return auctionItems;
        }
    }
}
