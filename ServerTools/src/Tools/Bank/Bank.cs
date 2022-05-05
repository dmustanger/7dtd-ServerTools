using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class Bank
    {
        public static bool IsEnabled = false, Inside_Claim = false, Player_Transfers = false;
        public static string Command_bank = "bank", Command_deposit = "deposit", Command_withdraw = "withdraw", Command_transfer = "transfer";
        public static int Deposit_Fee_Percent = 5;

        public static Dictionary<string, int> TransferId = new Dictionary<string, int>();

        private static readonly string file = string.Format("Bank_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/BankLogs/{1}", API.ConfigPath, file);
        private static readonly System.Random Random = new System.Random();

        public static int GetCurrent(string _steamid)
        {
            int bankValue = PersistentContainer.Instance.Players[_steamid].Bank;
            if (bankValue < 0)
            {
                PersistentContainer.Instance.Players[_steamid].Bank = 0;
                PersistentContainer.DataChange = true;
                bankValue = 0;
            }
            return bankValue;
        }

        public static void AddCoinsToBank(string _steamid, int _amount)
        {
            PersistentContainer.Instance.Players[_steamid].Bank += _amount;
            PersistentContainer.DataChange = true;
        }

        public static void SubtractCoinsFromBank(string _steamid, int _amount)
        {
            PersistentContainer.Instance.Players[_steamid].Bank -= _amount;
            PersistentContainer.DataChange = true;
        }

        public static void ClearBank(ClientInfo _cInfo)
        {
            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank = 0;
            PersistentContainer.DataChange = true;
        }

        public static void CurrentBankAndId(ClientInfo _cInfo)
        {
            try
            {
                int bank = GetCurrent(_cInfo.CrossplatformId.CombinedString);
                if (TransferId.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    TransferId.TryGetValue(_cInfo.CrossplatformId.CombinedString, out int id);
                    Phrases.Dict.TryGetValue("Bank1", out string phrase);
                    phrase = phrase.Replace("{Value}", bank.ToString());
                    phrase = phrase.Replace("{Id}", id.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    AddId(_cInfo);
                    CurrentBankAndId(_cInfo);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.CurrentBankAndId: {0}", e.Message));
            }
        }

        public static void AddId(ClientInfo _cInfo)
        {
            int tranferId = GenerateTransferId();
            if (tranferId != 0)
            {
                TransferId.Add(_cInfo.CrossplatformId.CombinedString, tranferId);
            }
        }

        private static int GenerateTransferId()
        {
            int id = Random.Next(1000, 8001);
            if (!TransferId.ContainsValue(id))
            {
                return id;
            }
            else
            {
                return 0;
            }
        }

        public static void CheckLocation(ClientInfo _cInfo, string _amount, int _exec)
        {
            try
            {
                if (Inside_Claim)
                {
                    EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                    if (player != null)
                    {
                        Vector3 position = player.GetPosition();
                        Vector3i vec3i = new Vector3i((int)position.x, (int)position.y, (int)position.z);
                        
                        if (PersistentOperations.ClaimedByWho(_cInfo.CrossplatformId, vec3i) == EnumLandClaimOwner.None)
                        {
                            Phrases.Dict.TryGetValue("Bank2", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                }
                if (_exec == 1)
                {
                    BagToBank(_cInfo, _amount);
                }
                else if (_exec == 2)
                {
                    BankToBag(_cInfo, _amount);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.CheckLocation: {0}", e.Message));
            }
        }

        public static void BagToBank(ClientInfo _cInfo, string _amount)
        {
            try
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (int.TryParse(_amount, out int value))
                    {
                        if (Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString) >= value)
                        {
                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, value);
                            if (Deposit_Fee_Percent > 0)
                            {
                                float fee = value * ((float)Deposit_Fee_Percent / 100);
                                int adjustedDeposit = value - (int)fee;
                                AddCoinsToBank(_cInfo.CrossplatformId.CombinedString, adjustedDeposit);
                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: '{1}' '{2}' added '{3}' to their bank", DateTime.Now, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, adjustedDeposit));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                                Phrases.Dict.TryGetValue("Bank3", out string phrase);
                                phrase = phrase.Replace("{Value}", adjustedDeposit.ToString());
                                phrase = phrase.Replace("{Name}", Wallet.Currency_Name);
                                phrase = phrase.Replace("{Percent}", Deposit_Fee_Percent.ToString());
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                AddCoinsToBank(_cInfo.CrossplatformId.CombinedString, value);
                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: '{1}' '{2}' added '{3}' to their bank", DateTime.Now, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, value));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                                Phrases.Dict.TryGetValue("Bank4", out string phrase);
                                phrase = phrase.Replace("{Value}", value.ToString());
                                phrase = phrase.Replace("{Name}", Wallet.Currency_Name);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Bank5", out string phrase);
                            phrase = phrase.Replace("{Name}", Wallet.Currency_Name);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Bank6", out string _phrase);
                        _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                        _phrase = _phrase.Replace("{Command_deposit}", Command_deposit);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.ChestToBankDeposit: {0}", e.Message));
            }
        }

        public static void BankToBag(ClientInfo _cInfo, string _amount)
        {
            try
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null && player.IsSpawned())
                {
                    ItemValue itemValue = ItemClass.GetItem(PersistentOperations.Currency_Item, false);
                    if (itemValue != null)
                    {
                        if (int.TryParse(_amount, out int value))
                        {
                            if (GetCurrent(_cInfo.CrossplatformId.CombinedString) >= value)
                            {
                                int maxAllowed;
                                if (itemValue.ItemClass.Stacknumber != null)
                                {
                                    maxAllowed = itemValue.ItemClass.Stacknumber.Value;
                                }
                                else
                                {
                                    maxAllowed = 30000;
                                }
                                if (value <= maxAllowed)
                                {
                                    ItemStack itemStack = new ItemStack(itemValue, value);
                                    if (itemStack != null)
                                    {
                                        SubtractCoinsFromBank(_cInfo.CrossplatformId.CombinedString, value);
                                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                        {
                                            sw.WriteLine(string.Format("{0}: '{1}' removed '{2}' from their bank", DateTime.Now, _cInfo.playerName, value));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                        {
                                            entityClass = EntityClass.FromString("item"),
                                            id = EntityFactory.nextEntityID++,
                                            itemStack = itemStack,
                                            pos = player.position,
                                            rot = new Vector3(20f, 0f, 20f),
                                            lifetime = 60f,
                                            belongsPlayerId = _cInfo.entityId
                                        });
                                        GameManager.Instance.World.SpawnEntityInWorld(entityItem);
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                                        GameManager.Instance.World.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                        Phrases.Dict.TryGetValue("Bank8", out string phrase);
                                        phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Bank9", out string phrase);
                                    phrase = phrase.Replace("{Max}", maxAllowed.ToString());
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Bank10", out string phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Bank6", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Bank7", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        Log.Out(string.Format("[SERVERTOOLS] Bank operation failed. Unable to find item {0}. Check the Wallet currency target in .../Mods/ServerTools/Config/items.xml matches the target in the default items.xml", Wallet.Currency_Name));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.BankToBag: {0}", e.Message));
            }
        }

        public static void Transfer(ClientInfo _cInfo, string _transferIdAndAmount)
        {
            try
            {
                string[] idAndAmount = { };
                if (_transferIdAndAmount.Contains(" "))
                {
                    idAndAmount = _transferIdAndAmount.Split(' ').ToArray();
                    if (int.TryParse(idAndAmount[0], out int id))
                    {
                        if (int.TryParse(idAndAmount[1], out int value))
                        {
                            if (TransferId.ContainsValue(id))
                            {
                                if (GetCurrent(_cInfo.CrossplatformId.CombinedString) >= value)
                                {
                                    foreach (KeyValuePair<string, int> bankData in TransferId)
                                    {
                                        if (bankData.Value == id)
                                        {
                                            TransferId.Remove(bankData.Key);
                                            ClientInfo cInfo2 = PersistentOperations.GetClientInfoFromNameOrId(bankData.Key);
                                            if (cInfo2 != null)
                                            {
                                                SubtractCoinsFromBank(_cInfo.CrossplatformId.CombinedString, value);
                                                AddCoinsToBank(cInfo2.CrossplatformId.CombinedString, value);
                                                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine(string.Format("{0}: Bank transfer '{1}' '{2}' named '{3}' to '{4}' '{5}' named '{6}' of '{7}' currency", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, cInfo2.PlatformId.CombinedString, cInfo2.CrossplatformId.CombinedString, cInfo2.playerName, value));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Phrases.Dict.TryGetValue("Bank15", out string phrase);
                                                phrase = phrase.Replace("{Value}", value.ToString());
                                                phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                                                ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                Phrases.Dict.TryGetValue("Bank16", out phrase);
                                                phrase = phrase.Replace("{Value}", value.ToString());
                                                phrase = phrase.Replace("{PlayerName}", cInfo2.playerName);
                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                            else
                                            {
                                                Phrases.Dict.TryGetValue("Bank14", out string phrase);
                                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Bank10", out string phrase);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Bank13", out string phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Bank6", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Bank11", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Bank12", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bank.Transfer: {0}", e.Message));
            }
        }
    }
}
