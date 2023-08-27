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
        public static bool IsEnabled = false, Inside_Claim = false, Player_Transfers = false, Direct_Deposit = false, Direct_Payment = false,
            Deposit_Message = false;
        public static string Command_bank = "bank", Command_deposit = "deposit", Command_withdraw = "withdraw", Command_transfer = "transfer";
        public static int Deposit_Fee_Percent = 5;

        public static Dictionary<string, int> TransferId = new Dictionary<string, int>();
        public static Dictionary<string, int> AddToBank = new Dictionary<string, int>();

        private static readonly string file = string.Format("Bank_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string Filepath = string.Format("{0}/Logs/BankLogs/{1}", API.ConfigPath, file);

        public static int GetCurrency(string _id)
        {
            int bankValue = 0;
            if (GeneralOperations.No_Currency)
            {
                return bankValue;
            }
            bankValue = PersistentContainer.Instance.Players[_id].Bank;
            if (bankValue < 0)
            {
                PersistentContainer.Instance.Players[_id].Bank = 0;
                PersistentContainer.DataChange = true;
                bankValue = 0;
            }
            return bankValue;
        }

        public static void AddCurrencyToBank(string _id, int _amount)
        {
            if (GeneralOperations.No_Currency || _amount < 1)
            {
                return;
            }
            PersistentContainer.Instance.Players[_id].Bank += _amount;
            PersistentContainer.DataChange = true;
            ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_id);
            if (cInfo != null)
            {
                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: Bank addition for '{1}' '{2}' named '{3}' of '{4}' currency. Total = '{5}'", DateTime.Now, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, _amount, PersistentContainer.Instance.Players[_id].Bank));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                if (Deposit_Message)
                {
                    Phrases.Dict.TryGetValue("Bank17", out string phrase);
                    phrase = phrase.Replace("{Value}", _amount.ToString());
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>().Setup(phrase, string.Empty));
                }
            }
        }

        public static bool SubtractCurrencyFromBank(string _id, int _amount)
        {
            if (GeneralOperations.No_Currency || _amount < 1)
            {
                return false;
            }
            if (PersistentContainer.Instance.Players[_id].Bank >= _amount)
            {
                PersistentContainer.Instance.Players[_id].Bank -= _amount;
                PersistentContainer.DataChange = true;
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_id);
                if (cInfo != null)
                {
                    using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                    {
                        sw.WriteLine(string.Format("{0}: Bank reduction for '{1}' '{2}' named '{3}' of '{4}' currency. Total = '{5}'", DateTime.Now, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, _amount, PersistentContainer.Instance.Players[_id].Bank));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                    Phrases.Dict.TryGetValue("Bank19", out string phrase);
                    phrase = phrase.Replace("{Value}", _amount.ToString());
                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>().Setup(phrase, string.Empty));
                }
                return true;
            }
            else
            {
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_id);
                if (cInfo != null)
                {
                    Phrases.Dict.TryGetValue("Bank10", out string phrase);
                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                return false;
            }
        }

        public static void ClearBank(ClientInfo _cInfo)
        {
            if (!GeneralOperations.No_Currency)
            {
                int oldValue = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank;
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank = 0;
                PersistentContainer.DataChange = true;
                using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("{0}: Bank for '{1}' '{2}' named '{3}' was cleared of '{4}' currency", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, oldValue));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public static void CurrentBankAndId(ClientInfo _cInfo)
        {
            try
            {
                if (!GeneralOperations.No_Currency)
                {
                    int bank = GetCurrency(_cInfo.CrossplatformId.CombinedString);
                    if (Player_Transfers)
                    {
                        if (TransferId.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                        {
                            TransferId.TryGetValue(_cInfo.CrossplatformId.CombinedString, out int id);
                            Phrases.Dict.TryGetValue("Bank1", out string phrase);
                            phrase = phrase.Replace("{Value}", bank.ToString());
                            phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                            phrase = phrase.Replace("{Id}", id.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            AddId(_cInfo);
                            CurrentBankAndId(_cInfo);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Bank18", out string phrase);
                        phrase = phrase.Replace("{Value}", bank.ToString());
                        phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Bank.CurrentBankAndId: {0}", e.Message);
            }
        }

        public static void AddId(ClientInfo _cInfo)
        {
            if (!GeneralOperations.No_Currency)
            {
                int tranferId = GenerateTransferId();
                if (tranferId != 0)
                {
                    TransferId.Add(_cInfo.CrossplatformId.CombinedString, tranferId);
                }
            }
        }

        private static int GenerateTransferId()
        {
            if (!GeneralOperations.No_Currency)
            {
                for (int i = 0; i < 10; i++)
                {
                    int id = new System.Random().Next(1000, 8001);
                    if (!TransferId.ContainsValue(id))
                    {
                        return id;
                    }
                }
            }
            return 0;
        }

        public static void CheckLocation(ClientInfo _cInfo, string _amount, bool _deposit)
        {
            try
            {
                if (GeneralOperations.No_Currency)
                {
                    return;
                }
                if (Inside_Claim)
                {
                    EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                    if (player == null)
                    {
                        return;
                    }
                    Vector3 position = player.GetPosition();
                    Vector3i vec3i = new Vector3i((int)position.x, (int)position.y, (int)position.z);

                    if (GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, vec3i) != EnumLandClaimOwner.Self && 
                        GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, vec3i) != EnumLandClaimOwner.Ally)
                    {
                        Phrases.Dict.TryGetValue("Bank2", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
                if (_deposit)
                {
                    BagToBank(_cInfo, _amount);
                    return;
                }
                BankToBag(_cInfo, _amount);
                return;
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Bank.CheckLocation: {0}", e.Message);
            }
        }

        public static void BagToBank(ClientInfo _cInfo, string _amount)
        {
            try
            {
                if (GeneralOperations.No_Currency)
                {
                    return;
                }
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player == null)
                {
                    return;
                }
                if (!int.TryParse(_amount, out int value) || value < 1)
                {
                    Phrases.Dict.TryGetValue("Bank6", out string phrase);
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{Command_deposit}", Command_deposit);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                if (Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString) < value)
                {
                    Phrases.Dict.TryGetValue("Bank5", out string phrase1);
                    phrase1 = phrase1.Replace("{Name}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, value);
                if (Deposit_Fee_Percent > 0)
                {
                    float fee = value * ((float)Deposit_Fee_Percent / 100);
                    int adjustedDeposit = value - (int)fee;
                    if (!AddToBank.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                    {
                        AddToBank.Add(_cInfo.CrossplatformId.CombinedString, adjustedDeposit);
                        //AddCurrencyToBank(_cInfo.CrossplatformId.CombinedString, adjustedDeposit);
                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("{0}: '{1}' '{2}' added '{3}' to their bank", DateTime.Now, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, adjustedDeposit));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Phrases.Dict.TryGetValue("Bank3", out string phrase2);
                        phrase2 = phrase2.Replace("{Value}", adjustedDeposit.ToString());
                        phrase2 = phrase2.Replace("{Name}", Wallet.Currency_Name);
                        phrase2 = phrase2.Replace("{Percent}", Deposit_Fee_Percent.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase2 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    if (!AddToBank.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                    {
                        AddToBank.Add(_cInfo.CrossplatformId.CombinedString, value);
                        //AddCurrencyToBank(_cInfo.CrossplatformId.CombinedString, value);
                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("{0}: '{1}' '{2}' added '{3}' to their bank", DateTime.Now, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, value));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Phrases.Dict.TryGetValue("Bank4", out string phrase3);
                        phrase3 = phrase3.Replace("{Value}", value.ToString());
                        phrase3 = phrase3.Replace("{Name}", Wallet.Currency_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase3 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Bank.BagToBank: {0}", e.Message);
            }
        }

        public static void BankToBag(ClientInfo _cInfo, string _amount)
        {
            try
            {
                if (GeneralOperations.No_Currency)
                {
                    return;
                }
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player == null || !player.IsSpawned())
                {
                    return;
                }
                ItemValue itemValue = ItemClass.GetItem(GeneralOperations.Currency_Item, false);
                if (itemValue == null)
                {
                    Phrases.Dict.TryGetValue("Bank7", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    Log.Out(string.Format("[SERVERTOOLS] Bank operation failed. Unable to find item {0}. Check the Wallet Item_Name option matches an existing item", Wallet.Currency_Name));
                    return;
                }
                if (!int.TryParse(_amount, out int value) || value < 1)
                {
                    Phrases.Dict.TryGetValue("Bank6", out string phrase1);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                int maxAllowed = 1000;
                if (itemValue.ItemClass.Stacknumber != null)
                {
                    maxAllowed = itemValue.ItemClass.Stacknumber.Value;
                }
                if (value > maxAllowed)
                {
                    Phrases.Dict.TryGetValue("Bank9", out string phrase3);
                    phrase3 = phrase3.Replace("{Max}", maxAllowed.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase3 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                ItemStack itemStack = new ItemStack(itemValue, value);
                if (itemStack == null)
                {
                    return;
                }
                if (SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, value))
                {
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
                    Phrases.Dict.TryGetValue("Bank8", out string phrase4);
                    phrase4 = phrase4.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase4 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Bank.BankToBag: {0}", e.Message);
            }
        }

        public static void Transfer(ClientInfo _cInfo, string _transferIdAndAmount)
        {
            try
            {
                if (GeneralOperations.No_Currency)
                {
                    return;
                }
                string[] idAndAmount = { };
                if (!_transferIdAndAmount.Contains(" "))
                {
                    Phrases.Dict.TryGetValue("Bank12", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                idAndAmount = _transferIdAndAmount.Split(' ').ToArray();
                if (!int.TryParse(idAndAmount[0], out int id))
                {
                    Phrases.Dict.TryGetValue("Bank11", out string phrase1);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                if (!int.TryParse(idAndAmount[1], out int value) || value < 1)
                {
                    Phrases.Dict.TryGetValue("Bank6", out string phrase2);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase2 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                if (!TransferId.ContainsValue(id))
                {
                    Phrases.Dict.TryGetValue("Bank13", out string phrase3);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase3 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                if (GetCurrency(_cInfo.CrossplatformId.CombinedString) < value)
                {
                    Phrases.Dict.TryGetValue("Bank10", out string phrase4);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase4 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                foreach (KeyValuePair<string, int> bankData in TransferId)
                {
                    if (bankData.Value != id)
                    {
                        continue;
                    }
                    ClientInfo cInfo2 = GeneralOperations.GetClientInfoFromNameOrId(bankData.Key);
                    if (cInfo2 == null || cInfo2 == _cInfo)
                    {
                        Phrases.Dict.TryGetValue("Bank14", out string phrase5);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase5 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                    TransferId.Remove(bankData.Key);
                    if (SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, value))
                    {
                        AddCurrencyToBank(cInfo2.CrossplatformId.CombinedString, value);
                        using (StreamWriter sw = new StreamWriter(Filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("{0}: Bank transfer '{1}' '{2}' named '{3}' to '{4}' '{5}' named '{6}' of '{7}' currency", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, cInfo2.PlatformId.CombinedString, cInfo2.CrossplatformId.CombinedString, cInfo2.playerName, value));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                        Phrases.Dict.TryGetValue("Bank15", out string phrase6);
                        phrase6 = phrase6.Replace("{Value}", value.ToString());
                        phrase6 = phrase6.Replace("{PlayerName}", cInfo2.playerName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase6 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        Phrases.Dict.TryGetValue("Bank16", out string phrase7);
                        phrase7 = phrase7.Replace("{Value}", value.ToString());
                        phrase7 = phrase7.Replace("{PlayerName}", _cInfo.playerName);
                        ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + phrase7 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Bank.Transfer: {0}", e.Message);
            }
        }
    }
}
