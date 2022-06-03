using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class Auction
    {
        public static bool IsEnabled = false, IsRunning = false, No_Admins = false;
        public static int Admin_Level = 0, Total_Items = 1, Tax;
        public static string Command_auction = "auction", Command_auction_cancel = "auction cancel", Command_auction_buy = "auction buy", Command_auction_sell = "auction sell";

        public static Dictionary<int, string> AuctionItems = new Dictionary<int, string>();

        private static readonly string file = string.Format("Auction_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/AuctionLogs/{1}", API.ConfigPath, file);
        private static readonly System.Random Random = new System.Random();

        public static void CheckBox(ClientInfo _cInfo, string _price)
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
                            else if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction.Count < Total_Items)
                            {
                                List<Chunk> chunks = new List<Chunk>();
                                DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
                                Vector3 _position = player.position;
                                Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)_position.x - 5, (int)_position.y, (int)_position.z);
                                if (chunk != null && !chunks.Contains(chunk))
                                {
                                    chunks.Add(chunk);
                                }
                                chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)_position.x + 5, (int)_position.y, (int)_position.z);
                                if (chunk != null && !chunks.Contains(chunk))
                                {
                                    chunks.Add(chunk);
                                }
                                chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)_position.x, (int)_position.y, (int)_position.z - 5);
                                if (chunk != null && !chunks.Contains(chunk))
                                {
                                    chunks.Add(chunk);
                                }
                                chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)_position.x + 5, (int)_position.y, (int)_position.z + 5);
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
                                                        if (item != null && !item.IsEmpty())
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
                                                                ItemDataSerializable serializedItemStack = new ItemDataSerializable();
                                                                {
                                                                    serializedItemStack.name = item.itemValue.ItemClass.GetItemName();
                                                                    serializedItemStack.count = item.count;
                                                                    serializedItemStack.useTimes = item.itemValue.UseTimes;
                                                                    serializedItemStack.quality = item.itemValue.Quality;
                                                                    serializedItemStack.seed = item.itemValue.Seed;
                                                                    if (item.itemValue.Modifications.Length > 0)
                                                                    {
                                                                        serializedItemStack.modSlots = item.itemValue.Modifications.Length;
                                                                    }
                                                                    else
                                                                    {
                                                                        serializedItemStack.modSlots = 0;
                                                                    }
                                                                    if (item.itemValue.CosmeticMods.Length > 0)
                                                                    {
                                                                        serializedItemStack.cosmeticSlots = item.itemValue.CosmeticMods.Length;
                                                                    }
                                                                    else
                                                                    {
                                                                        serializedItemStack.cosmeticSlots = 0;
                                                                    }
                                                                }
                                                                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction.Count > 0)
                                                                {
                                                                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction.Add(id, serializedItemStack);
                                                                }
                                                                else
                                                                {
                                                                    Dictionary<int, ItemDataSerializable> auctionItems = new Dictionary<int, ItemDataSerializable>
                                                                            {
                                                                                { id, serializedItemStack }
                                                                            };
                                                                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction = auctionItems;
                                                                }
                                                                if (PersistentContainer.Instance.AuctionPrices != null && PersistentContainer.Instance.AuctionPrices.Count > 0)
                                                                {
                                                                    PersistentContainer.Instance.AuctionPrices.Add(id, auctionPrice);
                                                                }
                                                                else
                                                                {
                                                                    Dictionary<int, int> _auctionPrices = new Dictionary<int, int>
                                                                            {
                                                                                { id, auctionPrice }
                                                                            };
                                                                    PersistentContainer.Instance.AuctionPrices = _auctionPrices;
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
                                                                Phrases.Dict.TryGetValue("Auction16", out string _phrase);
                                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Auction3", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Auction4", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
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
                    if (PersistentContainer.Instance.AuctionPrices != null)
                    {
                        Dictionary<int, int> auctionPrices = PersistentContainer.Instance.AuctionPrices;
                        List<string> playerlist = AuctionItems.Values.ToList();
                        if (playerlist != null && playerlist.Count > 0)
                        {
                            for (int i = 0; i < playerlist.Count; i++)
                            {
                                string id = playerlist[i];
                                if (PersistentContainer.Instance.Players[id].Auction != null && PersistentContainer.Instance.Players[id].Auction.Count > 0)
                                {
                                    if (_cInfo.CrossplatformId.CombinedString == id)
                                    {
                                        Phrases.Dict.TryGetValue("Auction19", out string phrase);
                                        phrase = phrase.Replace("{Value}", PersistentContainer.Instance.Players[id].Auction.Count.ToString());
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    Dictionary<int, ItemDataSerializable> auctionItems = PersistentContainer.Instance.Players[id].Auction;
                                    foreach (var item in auctionItems)
                                    {
                                        ItemValue itemValue = new ItemValue(ItemClass.GetItem(item.Value.name, false).type, false);
                                        if (itemValue != null)
                                        {
                                            if (auctionPrices.TryGetValue(item.Key, out int price))
                                            {
                                                if (itemValue.HasQuality)
                                                {
                                                    Phrases.Dict.TryGetValue("Auction5", out string phrase);
                                                    phrase = phrase.Replace("{Id}", item.Key.ToString());
                                                    phrase = phrase.Replace("{Count}", item.Value.count.ToString());
                                                    phrase = phrase.Replace("{Item}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.GetItemName());
                                                    phrase = phrase.Replace("{Quality}", item.Value.quality.ToString());
                                                    phrase = phrase.Replace("{Durability}", ((itemValue.MaxUseTimes - item.Value.useTimes) / itemValue.MaxUseTimes * 100).ToString());
                                                    phrase = phrase.Replace("{Price}", price.ToString());
                                                    phrase = phrase.Replace("{Coin}", Wallet.Currency_Name);
                                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                }
                                                else
                                                {
                                                    Phrases.Dict.TryGetValue("Auction17", out string phrase);
                                                    phrase = phrase.Replace("{Id}", item.Key.ToString());
                                                    phrase = phrase.Replace("{Count}", item.Value.count.ToString());
                                                    phrase = phrase.Replace("{Item}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.GetItemName());
                                                    phrase = phrase.Replace("{Price}", price.ToString());
                                                    phrase = phrase.Replace("{Coin}", Wallet.Currency_Name);
                                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                if (PersistentContainer.Instance.AuctionPrices != null)
                {
                    Dictionary<int, int> auctionPrices = PersistentContainer.Instance.AuctionPrices;
                    int price = 0;
                    if (auctionPrices.ContainsKey(_purchase))
                    {
                        auctionPrices.TryGetValue(_purchase, out price);
                    }
                    int currentCoins = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                    if (currentCoins >= price)
                    {
                        BuyAuction(_cInfo, _purchase, price);
                    }
                    else
                    {
                        int missing = price - currentCoins;
                        Phrases.Dict.TryGetValue("Auction7", out string phrase);
                        phrase = phrase.Replace("{Value}", missing.ToString());
                        phrase = phrase.Replace("{Name}", Wallet.Currency_Name);
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

        public static void BuyAuction(ClientInfo _cInfo, int _purchase, int _price)
        {
            if (AuctionItems.TryGetValue(_purchase, out string id))
            {
                if (PersistentContainer.Instance.Players[id].Auction != null && PersistentContainer.Instance.Players[id].Auction.Count > 0)
                {
                    PersistentContainer.Instance.Players[id].Auction.TryGetValue(_purchase, out ItemDataSerializable itemData);
                    ItemValue itemValue = new ItemValue(ItemClass.GetItem(itemData.name, false).type);
                    if (itemValue != null)
                    {
                        if (itemValue.ItemClass.HasQuality)
                        {
                            itemValue.Quality = 1;
                            if (itemData.quality > 0)
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
                        EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                        {
                            entityClass = EntityClass.FromString("item"),
                            id = EntityFactory.nextEntityID++,
                            itemStack = new ItemStack(itemValue, itemData.count),
                            pos = world.Players.dict[_cInfo.entityId].position,
                            rot = new Vector3(20f, 0f, 20f),
                            lifetime = 60f,
                            belongsPlayerId = _cInfo.entityId
                        });
                        world.SpawnEntityInWorld(entityItem);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                        world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                        AuctionItems.Remove(_purchase);
                        PersistentContainer.Instance.Players[id].Auction.Remove(_purchase);
                        PersistentContainer.Instance.AuctionPrices.Remove(_purchase);
                        PersistentContainer.DataChange = true;
                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, _price);
                        float fee = _price * ((float)Tax / 100);
                        int adjustedPrice = _price - (int)fee;
                        Wallet.AddCurrency(id, adjustedPrice);
                        string playerName = PersistentOperations.GetPlayerDataFileFromId(id).ecd.entityName;
                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' has purchased auction entry '{3}', profits went to id '{4}' named '{5}'", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _purchase, id, playerName));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Phrases.Dict.TryGetValue("Auction9", out string phrase);
                        phrase = phrase.Replace("{Count}", itemData.count.ToString());
                        phrase = phrase.Replace("{ItemName}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.GetItemName());
                        phrase = phrase.Replace("{Value}", _price.ToString());
                        phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        ClientInfo cInfo2 = PersistentOperations.GetClientInfoFromNameOrId(id);
                        if (cInfo2 != null)
                        {
                            Phrases.Dict.TryGetValue("Auction10", out string phrase1);
                            ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
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
                                ItemValue itemValue = new ItemValue(ItemClass.GetItem(itemData.name, false).type, itemData.quality, itemData.quality, false);
                                if (itemValue != null)
                                {
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
                                        pos = world.Players.dict[_cInfo.entityId].position,
                                        rot = new Vector3(20f, 0f, 20f),
                                        lifetime = 60f,
                                        belongsPlayerId = _cInfo.entityId
                                    });
                                    world.SpawnEntityInWorld(entityItem);
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                    AuctionItems.Remove(itemId);
                                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Auction.Remove(itemId);
                                    PersistentContainer.Instance.AuctionPrices.Remove(itemId);
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
            int id = 0;
            for (int i = 0; i < 20; i++)
            {
                id = Random.Next(1000, 5001);
                if (!AuctionItems.ContainsKey(id))
                {
                    break;
                }
            }
            return id;
        }
    }
}
