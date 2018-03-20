using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class WalletCoins : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools]-Increase or decrease the value of a wallet.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. wallet <steamId/entityId> <value>\n" +
                "1. Add to or reduce a player's wallet value.\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Wallet", "wallet" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                    return;
                }
                if (_params[0].Length < 1 || _params[0].Length > 17)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not adjust wallet connected to Id: Invalid Id {0}", _params[0]));
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
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (_cInfo != null)
                    {
                        Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                        int _spentCoins = p.PlayerSpentCoins;
                        int _newCoins = _spentCoins + _adjustCoins;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = _newCoins;
                        PersistentContainer.Instance.Save();
                        if (_adjustCoins >= 0)
                        {
                            SdtdConsole.Instance.Output(string.Format("Added {0} {1} to player id {2}'s wallet", _params[1], Wallet.Coin_Name, _params[0]));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Subtracted {0} {1} from player id {2}'s wallet", _params[1], Wallet.Coin_Name, _params[0]));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player with Id {0} is not online.", _params[0]));
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WalletCoins.Run: {0}.", e));
            }
        }
    }
}