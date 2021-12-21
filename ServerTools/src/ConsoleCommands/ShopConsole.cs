﻿using System;
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
                   "  3. st-shop add {itemName} {secondaryName} {count} {quality} {price} {category} \n" +
                   "  4. st-shop remove {number}\n" +
                   "  5. st-shop list\n" +
                   "1. Turn off shop\n" +
                   "2. Turn on shop\n" +
                   "3. Add item to Shop.xml\n" +
                   "4. Remove item from Shop.xml\n" +
                   "5. Show a list of the shop items\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Shop", "shop" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 2 && _params.Count != 7)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, 2 or 7, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
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
                    if (_params.Count != 7)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 7, found '{0}'", _params.Count));
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
                                            else if (_quality > 600)
                                            {
                                                _quality = 600;
                                            }
                                            int _id = Shop.Dict.Count + 1;
                                            string[] _item = new string[] { _id.ToString(), _params[1], _params[2], _params[3], _params[4], _params[5], _params[6] };
                                            if (!Shop.Dict.Contains(_item))
                                            {
                                                if (!Shop.Categories.Contains(_params[6]))
                                                {
                                                    Shop.Categories.Add(_params[6]);
                                                }
                                                Shop.Dict.Add(_item);
                                                Shop.UpdateXml();
                                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Item {0} has been added to the shop", _params[1]));
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