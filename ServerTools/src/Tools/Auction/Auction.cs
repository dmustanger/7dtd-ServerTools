using HarmonyLib;
using Platform;
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
        public static int Admin_Level = 0, Max_Items = 1, Tax = 0;
        public static string Command_auction = "auction", Command_auction_cancel = "auction cancel", Command_auction_buy = "auction buy",
            Command_auction_sell = "auction sell", Panel_Name = "Awesome Auction";

        public static Dictionary<int, string> AuctionItems = new Dictionary<int, string>();
        public static Dictionary<string, int> PanelAccess = new Dictionary<string, int>();
        public static List<ushort> DroppedSeed = new List<ushort>();

        public static readonly string AuctionFile = string.Format("Auction_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        public static readonly string Filepath = string.Format("{0}/Logs/AuctionLogs/{1}", API.ConfigPath, AuctionFile);

        private static AccessTools.FieldRef<GameManager, Dictionary<TileEntity, int>> lockedTileEntities = AccessTools.FieldRefAccess<GameManager, Dictionary<TileEntity, int>>("lockedTileEntities");

        public static void SellItem(ClientInfo _cInfo, string _price)
        {
            try
            {
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (int.TryParse(_price, out int auctionPrice))
                    {
                        if (auctionPrice < 1)
                        {
                            Phrases.Dict.TryGetValue("Auction3", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction == null)
                        {
                            Dictionary<int, ItemDataSerializable> auctionItems = new Dictionary<int, ItemDataSerializable>();
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction = auctionItems;
                            PersistentContainer.DataChange = true;
                        }
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction.Count >= Max_Items)
                        {
                            Phrases.Dict.TryGetValue("Auction2", out string phrase);
                            phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            phrase = phrase.Replace("{Command_auction_cancel}", Command_auction_cancel);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                        Vector3i position = new Vector3i(player.serverPos.ToVector3() / 32f);
                        TileEntitySecureLootContainerSigned signedContainer;
                        List<Chunk> surroundingChunks = GeneralOperations.GetSurroundingChunks(position);
                        if (surroundingChunks == null || surroundingChunks.Count == 0)
                        {
                            return;
                        }
                        Dictionary<TileEntity, int> lockedTiles = lockedTileEntities(GameManager.Instance);
                        DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
                        for (int i = 0; i < surroundingChunks.Count; i++)
                        {
                            tiles = surroundingChunks[i].GetTileEntities();
                            foreach (var tile in tiles.dict)
                            {
                                if (!(tile.Value is TileEntitySecureLootContainerSigned))
                                {
                                    continue;
                                }
                                signedContainer = tile.Value as TileEntitySecureLootContainerSigned;
                                if (signedContainer != null && signedContainer.GetText().ToLower() == "auction" && !lockedTiles.ContainsKey(signedContainer))
                                {
                                    EnumLandClaimOwner claimOwner = GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, signedContainer.ToWorldPos());
                                    if (claimOwner == EnumLandClaimOwner.Self || claimOwner == EnumLandClaimOwner.Ally)
                                    {
                                        AddToAuction(_cInfo, signedContainer, auctionPrice);
                                        return;
                                    }
                                }
                            }
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

        public static void AddToAuction(ClientInfo _cInfo, TileEntitySecureLootContainerSigned _container, int _price)
        {
            ItemStack[] items = _container.items;
            ItemStack item = items[0];
            if (item != null && !item.itemValue.IsEmpty())
            {
                if (item.itemValue.Modifications.Length > 0)
                {
                    for (int j = 0; j < item.itemValue.Modifications.Length; j++)
                    {
                        if (!item.itemValue.Modifications[j].IsEmpty())
                        {
                            Phrases.Dict.TryGetValue("Auction18", out string phraseOne);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phraseOne + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                            Phrases.Dict.TryGetValue("Auction18", out string phraseTwo);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phraseTwo + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                }
                int id = GenerateAuctionId();
                if (id == 0)
                {
                    Phrases.Dict.TryGetValue("Auction16", out string phraseThree);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phraseThree + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
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
                    itemData.price = _price;
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
                _container.SetModified();
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
                Phrases.Dict.TryGetValue("Auction1", out string phraseFour);
                phraseFour = phraseFour.Replace("{Name}", item.itemValue.ItemClass.GetLocalizedItemName() ?? item.itemValue.ItemClass.GetItemName());
                phraseFour = phraseFour.Replace("{Value}", id.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phraseFour + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
        }

        public static void AuctionList(ClientInfo _cInfo)
        {
            try
            {
                if (AuctionItems.Count > 0)
                {
                    if (Panel && WebAPI.IsEnabled && WebAPI.IsRunning && !PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Overlay)
                    {
                        string ip = _cInfo.ip;
                        bool duplicate = false;
                        List<ClientInfo> clientList = GeneralOperations.ClientList();
                        if (clientList != null && clientList.Count > 1)
                        {
                            for (int i = 0; i < clientList.Count; i++)
                            {
                                ClientInfo cInfoTwo = clientList[i];
                                if (cInfoTwo != null && cInfoTwo.entityId != _cInfo.entityId && ip == cInfoTwo.ip)
                                {
                                    duplicate = true;
                                    break;
                                }
                            }
                        }
                        uint ipInt = NetworkUtils.ToInt(_cInfo.ip);
                        if (duplicate || (ipInt >= NetworkUtils.ToInt("10.0.0.0") && ipInt <= NetworkUtils.ToInt("10.255.255.255")) ||
                            (ipInt >= NetworkUtils.ToInt("172.16.0.0") && ipInt <= NetworkUtils.ToInt("172.31.255.255")) ||
                            (ipInt >= NetworkUtils.ToInt("192.168.0.0") && ipInt <= NetworkUtils.ToInt("192.168.255.255")) ||
                            _cInfo.ip == "127.0.0.1")
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
                                                    break;
                                                }
                                            }
                                        }
                                        PanelAccess.Add(securityId, _cInfo.entityId);
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
                        else
                        {
                            if (PanelAccess.Count > 0 && PanelAccess.ContainsValue(_cInfo.entityId))
                            {
                                var clients = PanelAccess.ToArray();
                                for (int i = 0; i < clients.Length; i++)
                                {
                                    if (clients[i].Value == _cInfo.entityId && clients[i].Key != ip)
                                    {
                                        PanelAccess.Remove(clients[i].Key);
                                        PanelAccess.Add(ip, _cInfo.entityId);
                                        break;
                                    }
                                }
                            }
                            else if (PanelAccess.ContainsKey(ip))
                            {
                                PanelAccess[ip] = _cInfo.entityId;
                            }
                            else
                            {
                                PanelAccess.Add(ip, _cInfo.entityId);
                            }
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserAuction", true));
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
                    int currency = 0, bankCurrency = 0, cost = itemData.price;
                    if (Wallet.IsEnabled)
                    {
                        currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                    }
                    if (Bank.IsEnabled && Bank.Direct_Payment)
                    {
                        bankCurrency = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank;
                    }
                    if (currency + bankCurrency >= cost)
                    {
                        if (currency > 0)
                        {
                            if (currency < cost)
                            {
                                Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, currency);
                                cost -= currency;
                                Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                            }
                            else
                            {
                                Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, cost);
                            }
                        }
                        else
                        {
                            Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                        }
                        BuyAuction(_cInfo, _purchase, id, itemData);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Auction7", out string phrase);
                        phrase = phrase.Replace("{Value}", (itemData.price - currency).ToString());
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
                itemValue.Quality = 0;
                itemValue.Modifications = new ItemValue[0];
                itemValue.CosmeticMods = new ItemValue[0];
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
                float fee = _itemData.price * ((float)Tax / 100);
                int adjustedPrice = _itemData.price - (int)fee;
                Wallet.AddCurrency(_id, adjustedPrice, true);
                string playerName = GeneralOperations.GetPlayerDataFileFromId(_id).ecd.entityName;
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
                ClientInfo cInfo2 = GeneralOperations.GetClientInfoFromNameOrId(_id);
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
                    EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
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
                pass += GeneralOperations.NumSet.ElementAt(rnd.Next(0, 10));
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
