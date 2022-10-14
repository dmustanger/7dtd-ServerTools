using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ShopConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable, disable, add, remove and list items from shop.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-shop off\n" +
                   "  2. st-shop on\n" +
                   "  3. st-shop add <ItemName> <SecondaryName> <Count> <Quality> <Price> <Category>\n" +
                   "  4. st-shop add <ItemName> <SecondaryName> <Count> <Quality> <Price> <Category> <PanelMessage>\n" +
                   "  5. st-shop remove <number>\n" +
                   "  6. st-shop list\n" +
                   "  7. st-shop log\n" +
                   "  8. st-shop log all\n" +
                   "  9. st-shop log player <EOS/EntityId/PlayerName>\n" +
                   "1. Turn off shop\n" +
                   "2. Turn on shop\n" +
                   "3. Add a item to Shop.xml with no panel message\n" +
                   "4. Add a item to Shop.xml with a panel message\n" +
                   "5. Remove item from Shop.xml\n" +
                   "6. Show a list of the shop items\n" +
                   "7. Shows a list and creates a text file of the shop sales. Does not include player info\n" +
                   "8. Shows a list and creates a text file of the shop sales for all players\n" +
                   "9. Shows a list and creates a text file of the shop sales for a specific player\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Shop", "shop" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 2 && _params.Count != 7 && _params.Count != 8)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, 2, 7 or 8, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (Shop.IsEnabled)
                    {
                        Shop.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Shop has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Shop is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (!Shop.IsEnabled)
                    {
                        Shop.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Shop has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Shop is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 7 && _params.Count != 8)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 7 or 8, found '{0}'", _params.Count));
                        return;
                    }
                    else
                    {
                        if (Shop.IsEnabled)
                        {
                            ItemValue _itemValue = ItemClass.GetItem(_params[1], false);
                            if (_itemValue.type == ItemValue.None.type)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Item could not be found '{0}'", _params[1]));
                                return;
                            }
                            else
                            {
                                if (!int.TryParse(_params[3], out int _count))
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid item count '{0}'", _params[3]));
                                    return;
                                }
                                else
                                {
                                    if (_count > _itemValue.ItemClass.Stacknumber.Value)
                                    {
                                        _count = _itemValue.ItemClass.Stacknumber.Value;
                                    }
                                    if (!int.TryParse(_params[4], out int _quality))
                                    {
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid item quality '{0}'", _params[4]));
                                        return;
                                    }
                                    else
                                    {
                                        if (!int.TryParse(_params[5], out int _price))
                                        {
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid item price '{0}'", _params[5]));
                                            return;
                                        }
                                        else
                                        {
                                            if (_quality < 1)
                                            {
                                                _quality = 1;
                                            }
                                            int _id = Shop.Dict.Count + 1;
                                            string panelMessage = "";
                                            if (_params.Count == 8)
                                            {
                                                panelMessage = _params[7];
                                            }
                                            string[] _item = new string[] { _id.ToString(), _params[1], _params[2], _params[3], _params[4], _params[5], _params[6], panelMessage };
                                            if (!Shop.Dict.Contains(_item))
                                            {
                                                if (!Shop.Categories.Contains(_params[6]))
                                                {
                                                    Shop.Categories.Add(_params[6]);
                                                }
                                                Shop.Dict.Add(_item);
                                                Shop.UpdateXml();
                                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Item '{0}' has been added to the shop", _params[1]));
                                                return;
                                            }
                                            else
                                            {
                                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to add item to Shop.xml. Entry is already on the list"));
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Shop is not enabled. Unable to write to the xml file"));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    else if (_params[1] != "0")
                    {
                        if (!int.TryParse(_params[1], out int _number))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid id '{0}'", _params[1]));
                            return;
                        }
                        if (Shop.Dict.Count >= _number - 1)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' not found on the shop list", _params[1]));
                            return;
                        }
                        else
                        {
                            Shop.Dict.RemoveAt(_number - 1);
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' has been removed from the shop list", _number));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    else
                    {
                        if (Shop.Dict.Count > 0)
                        {
                            for (int i = 0; i < Shop.Dict.Count; i++)
                            {
                                string[] _item = Shop.Dict[i];
                                if (int.Parse(_item[4]) > 1)
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] # {0}: '{1}' '{2}' at '{3}' quality for '{4}' '{5}'", _item[0], _item[3], _item[2], _item[4], _item[5], Wallet.Currency_Name));
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] # {0}: '{1}' '{2}' for '{3}' '{4}'", _item[0], _item[3], _item[2], _item[5], Wallet.Currency_Name));
                                }
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Shop list is empty"));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("log"))
                {
                    if (_params.Count < 1 || _params.Count > 3)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 to 3, found '{0}'", _params.Count));
                        return;
                    }
                    else if (_params.Count == 1)
                    {
                        List<string[]> shopLog = PersistentContainer.Instance.ShopLog;
                        if (shopLog.Count > 0)
                        {
                            Dictionary<string, int> entries = new Dictionary<string, int>();
                            for (int i = 0; i < shopLog.Count; i++)
                            {
                                int.TryParse(shopLog[i][1], out int amountSold);
                                if (!entries.ContainsKey(shopLog[i][0]))
                                {
                                    entries.Add(shopLog[i][0], amountSold);
                                }
                                else
                                {
                                    entries[shopLog[i][0]] += amountSold;
                                }
                            }
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Shop records:"));
                            foreach (var entry in entries)
                            {
                                string logEntry = string.Format("{0} '{1}'", entry.Value, entry.Key);
                                Shop.Writer(logEntry);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(logEntry);
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] There are no records of sale from the Shop yet"));
                        }
                        return;
                    }
                    else if (_params.Count == 2 && _params[1].ToLower().Equals("all"))
                    {
                        List<string[]> shopLog = PersistentContainer.Instance.ShopLog;
                        if (shopLog.Count > 0)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Shop records:"));
                            string playerId = "";
                            List<string> entries = new List<string>();
                            for (int i = 0; i < shopLog.Count; i++)
                            {
                                if (!entries.Contains(shopLog[i][3]))
                                {
                                    entries.Add(shopLog[i][3]);
                                    playerId = shopLog[i][3];
                                    for (int j = i; j < shopLog.Count; j++)
                                    {
                                        if (shopLog[j][3] == playerId)
                                        {
                                            string logEntry = string.Format("[SERVERTOOLS] '{0}' '{1}' named '{2}' purchased {3} '{4}' on '{5}'", shopLog[j][2], shopLog[j][3], shopLog[j][4], shopLog[j][1], shopLog[j][0], shopLog[j][5]);
                                            Shop.Writer(logEntry);
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(logEntry);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] There are no records of sale from the Shop yet"));
                        }
                        return;
                    }
                    else if (_params[1].ToLower().Equals("player"))
                    {
                        List<string[]> shopLog = PersistentContainer.Instance.ShopLog;
                        if (shopLog.Count > 0)
                        {
                            ClientInfo cInfo = GeneralFunction.GetClientInfoFromNameOrId(_params[2]);
                            if (cInfo != null)
                            {
                                EntityPlayer player = GeneralFunction.GetEntityPlayer(cInfo.entityId);
                                if (player != null)
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Shop records:"));
                                    string playerId = cInfo.CrossplatformId.CombinedString;
                                    for (int i = 0; i < shopLog.Count; i++)
                                    {
                                        if (shopLog[i][3] == playerId)
                                        {
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] '{0}' '{1}' named '{2}' purchased {3} '{4}' on '{5}'", shopLog[i][2], shopLog[i][3], shopLog[i][4], shopLog[i][1], shopLog[i][0], shopLog[i][5]));
                                        }
                                    }
                                    return;
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Shop records:"));
                                bool found = false;
                                string playerId = _params[2];
                                for (int i = 0; i < shopLog.Count; i++)
                                {
                                    if (shopLog[i][3] == playerId)
                                    {
                                        found = true;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] '{0}' '{1}' named '{2}' purchased {3} '{4}' on '{5}'", shopLog[i][2], shopLog[i][3], shopLog[i][4], shopLog[i][1], shopLog[i][0], shopLog[i][5]));
                                    }
                                }
                                if (!found)
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' not found", _params[0]));
                                }
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] There are no records of sale from the Shop yet"));
                        }
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ShopConsole.Execute: {0}", e.Message));
            }
        }
    }
}