using System;
using System.Collections.Generic;

namespace ServerTools
{
    class WalletConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable, disable, add, reduce, check wallet.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-wlt off\n" +
                   "  2. st-wlt on\n" +
                   "  3. st-wlt all <Value>\n" +
                   "  4. st-wlt <EOS> <Value>\n" +
                   "  5. st-wlt <EOS>\n" +
                   "1. Turn off wallet\n" +
                   "2. Turn on wallet\n" +
                   "3. Add to or reduce all online player's wallet value\n" +
                   "4. Add to or reduce a player's wallet value\n" +
                   "5. Check player's wallet value\n";
                   
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Wallet", "wlt", "st-wlt" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 2)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 or 2, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Wallet.IsEnabled)
                    {
                        Wallet.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wallet has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wallet is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Wallet.IsEnabled)
                    {
                        Wallet.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wallet has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wallet is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("all"))
                {
                    bool negative = false;
                    if (_params[1].Contains("-"))
                    {
                        _params[1].Replace("-", "");
                        negative = true;
                    }
                    if (!int.TryParse(_params[1], out int adjustCoins))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not adjust wallet. Value '{0}' is invalid", _params[1]));
                    }
                    else
                    {
                        List<ClientInfo> clientList = PersistentOperations.ClientList();
                        if (clientList != null)
                        {
                            for (int i = 0; i < clientList.Count; i++)
                            {
                                ClientInfo cInfo = clientList[i];
                                if (cInfo != null)
                                {
                                    if (negative)
                                    {
                                        if (Wallet.GetCurrency(cInfo.CrossplatformId.CombinedString) >= adjustCoins)
                                        {
                                            Wallet.RemoveCurrency(cInfo.CrossplatformId.CombinedString, adjustCoins, false);
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed '{0}' '{1}' from Id '{2}' named '{3}'", _params[1], Wallet.Currency_Name, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                        }
                                        else
                                        {
                                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to remove '{0}' '{1}'. Target '{2}' named '{3}' does not have enough", _params[1], Wallet.Currency_Name, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        Wallet.AddCurrency(cInfo.CrossplatformId.CombinedString, adjustCoins);
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added '{0}' '{1}' to id '{2}' named '{3}'", _params[1], Wallet.Currency_Name, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (_params.Count == 2)
                {
                    if (_params[1].Length < 1 || _params[1].Length > 6)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not adjust wallet. Value '{0}' is invalid", _params[1]));
                        return;
                    }
                    bool negative = false;
                    if (_params[1].Contains("-"))
                    {
                        _params[1].Replace("-", "");
                        negative = true;
                    }
                    if (!int.TryParse(_params[1], out int adjustCoins))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not adjust wallet. Value '{0}' is invalid", _params[1]));
                        return;
                    }
                    if (_params[0].Contains("_"))
                    {
                        PersistentPlayer p = PersistentContainer.Instance.Players[_params[0]];
                        if (p != null)
                        {
                            if (negative)
                            {
                                Wallet.RemoveCurrency(_params[0], adjustCoins, false);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed '{0}' '{1}' from Id '{2}'", _params[1], Wallet.Currency_Name, _params[0]));
                            }
                            else
                            {
                                Wallet.AddCurrency(_params[0], adjustCoins);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added '{0}' '{1}' to wallet '{2}'", _params[1], Wallet.Currency_Name, _params[0]));
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id not found '{0}'", _params[0]));
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid Id '{0}'", _params[0]));
                    }
                }
                else if (_params.Count == 1)
                {
                    if (_params[0].Contains("_"))
                    {
                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[0]);
                        if (cInfo != null)
                        {
                            int currentWallet = Wallet.GetCurrency(cInfo.CrossplatformId.CombinedString);
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' named '{1}' has '{2}' '{3}'", _params[0], cInfo.playerName, currentWallet, Wallet.Currency_Name));
                        }
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WalletConsole.Execute: {0}", e.Message));
            }
        }
    }
}