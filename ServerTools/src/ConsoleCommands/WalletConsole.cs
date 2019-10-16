using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

namespace ServerTools
{
    class WalletConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable, Disable, Add, Reduce, Check Wallet.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Wallet off\n" +
                   "  2. Wallet on\n" +
                   "  3. Wallet <steamId> <value>\n" +
                   "  4. Wallet <steamId>\n" +
                   "1. Turn off wallet\n" +
                   "2. Turn on wallet\n" +
                   "3. Add to or reduce a player's wallet value.\n" +
                   "4. Check player's wallet value.\n";
                   
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Wallet", "wallet" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 2)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    Wallet.IsEnabled = false;
                    LoadConfig.WriteXml();
                    SdtdConsole.Instance.Output(string.Format("Wallet has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Wallet.IsEnabled = true;
                    LoadConfig.WriteXml();
                    SdtdConsole.Instance.Output(string.Format("Wallet has been set to on"));
                    return;
                }
                else if (_params.Count == 2)
                {
                    if (_params[0].Length < 1 || _params[0].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not adjust wallet: Invalid Id {0}", _params[1]));
                        return;
                    }
                    if (_params[1].Length < 1 || _params[1].Length > 5)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not adjust wallet. Value {0} is invalid", _params[1]));
                        return;
                    }
                    int _adjustCoins;
                    if (!int.TryParse(_params[1], out _adjustCoins))
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not adjust wallet. Value {0} is invalid", _params[1]));
                        return;
                    }
                    else
                    {
                        PersistentPlayer p = PersistentContainer.Instance.Players[_params[0]];
                        if (p != null)
                        {
                            Wallet.AddCoinsToWallet(_params[0], _adjustCoins);
                            if (_adjustCoins >= 0)
                            {
                                SdtdConsole.Instance.Output(string.Format("Added {0} {1} to player id {2} wallet", _params[1], Wallet.Coin_Name, _params[0]));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Subtracted {0} {1} from player id {2} wallet", _params[1], Wallet.Coin_Name, _params[0]));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player id not found: {0}", _params[0]));
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WalletConsole.Run: {0}.", e));
            }
        }
    }
}