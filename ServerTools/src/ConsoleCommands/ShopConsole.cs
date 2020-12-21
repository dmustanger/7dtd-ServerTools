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
                   "  1. Shop off\n" +
                   "  2. Shop on\n" +
                   "  3. Shop add {itemName} {secondaryName} {count} {quality} {price} {category} \n" +
                   "  4. Shop remove {number}\n" +
                   "  5. Shop list\n" +
                   "1. Turn off shop\n" +
                   "2. Turn on shop\n" +
                   "3. Add item to Shop.xml\n" +
                   "4. Remove item from Shop.xml\n" +
                   "5. Show a list of the shop items\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Shop", "shp", "st-shp" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 2 && _params.Count != 7)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, 2 or 7, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Shop.IsEnabled)
                    {
                        Shop.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Shop has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Shop is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Shop.IsEnabled)
                    {
                        Shop.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Shop has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Shop is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 7)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 7, found {0}", _params.Count));
                        return;
                    }
                    else
                    {
                        if (Shop.IsEnabled)
                        {
                            ItemValue _itemValue = ItemClass.GetItem(_params[1], false);
                            if (_itemValue.type == ItemValue.None.type)
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Item could not be found: {0}", _params[1]));
                                return;
                            }
                            else
                            {
                                if (!int.TryParse(_params[3], out int _count))
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid item count: {0}", _params[3]));
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
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid item quality: {0}", _params[4]));
                                        return;
                                    }
                                    else
                                    {
                                        if (!int.TryParse(_params[5], out int _price))
                                        {
                                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid item price: {0}", _params[5]));
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
                                            foreach (var _item in Shop.Dict)
                                            {
                                                if (_item.Value[0] == _params[1])
                                                {
                                                    Shop.Dict1.TryGetValue(_item.Key, out int[] _values);
                                                    if (_values[0] == _count && _values[1] == _quality)
                                                    {
                                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to add item {0}. It is already on the shop list", _params[1]));
                                                        return;
                                                    }
                                                }
                                            }
                                            if (!Shop.Categories.Contains(_params[6]))
                                            {
                                                Shop.Categories.Add(_params[6]);
                                            }
                                            string[] _strings = new string[] { _params[1], _params[2], _params[6] };
                                            int[] _integers = new int[] { _count, _quality, _price };
                                            if (!Shop.Dict.ContainsKey(Shop.Dict.Count + 1))
                                            {
                                                Shop.Dict.Add(Shop.Dict.Count + 1, _strings);
                                                Shop.Dict1.Add(Shop.Dict1.Count + 1, _integers);
                                                Shop.UpdateXml();
                                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Item {0} has been added to the shop", _params[1]));
                                                return;
                                            }
                                            else if (!Shop.Dict.ContainsKey(Shop.Dict.Count + 2))
                                            {
                                                Shop.Dict.Add(Shop.Dict.Count + 2, _strings);
                                                Shop.Dict1.Add(Shop.Dict1.Count + 2, _integers);
                                                Shop.UpdateXml();
                                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Item {0} has been added to the shop", _params[1]));
                                                return;
                                            }
                                            else
                                            {
                                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to add item {0}. Shop id are out of order. Check the xml file", _params[1]));
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Shop is not enabled. Unable to write to the xml file"));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    else
                    {
                        if (!int.TryParse(_params[1], out int _number))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid id: {0}", _params[1]));
                            return;
                        }
                        else
                        {
                            if (!Shop.Dict.ContainsKey(_number - 1))
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Id not found on the shop list: {0}", _params[1]));
                                return;
                            }
                            else
                            {
                                Shop.Dict.Remove(_number - 1);
                                Shop.Dict1.Remove(_number - 1);
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Id {0} has been removed from the shop list", _number));
                                return;
                            }
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    else
                    {
                        if (Shop.Dict.Count > 0)
                        {
                            foreach (var _item in Shop.Dict)
                            {
                                if (Shop.Dict1.TryGetValue(_item.Key, out int[] _values))
                                {
                                    if (_values[1] > 1)
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] # {0}: {1} {2} {3} quality for {4} {5}", _item.Key, _values[0], _item.Value[1], _values[1], _values[2], Wallet.Coin_Name));
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] # {0}: {1} {2} for {3} {4}", _item.Key, _values[0], _item.Value[1], _values[2], Wallet.Coin_Name));
                                    }
                                }
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Shop list is empty"));
                            return;
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
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