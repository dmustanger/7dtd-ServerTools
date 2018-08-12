using System;
using System.Collections.Generic;
using System.Data;

namespace ServerTools
{
    public class WalletCoins : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools]-Increase, decrease or check the value of a wallet.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. wallet <steamId> <value>\n" +
                "  2. wallet <steamId>\n" +
                "1. Add to or reduce a player's wallet value.\n" +
                "2. Check player's wallet value.\n";
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
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 2, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].Length != 17)
                {
                    SdtdConsole.Instance.Output(string.Format("Can not adjust wallet: Invalid Id {0}", _params[0]));
                    return;
                }
                if (_params.Count == 2)
                {
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
                        string _steamid = SQL.EscapeString(_params[0]);
                        string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _steamid);
                        DataTable _result = SQL.TQuery(_sql);
                        if (_result.Rows.Count !=0)
                        {
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out int _playerSpentCoins);
                            int _newCoins = _playerSpentCoins + _adjustCoins;
                            _sql = string.Format("UPDATE Players SET playerSpentCoins = {0} WHERE steamid = '{1}'", _playerSpentCoins + _adjustCoins, _steamid);
                            SQL.FastQuery(_sql);
                            if (_adjustCoins >= 0)
                            {
                                SdtdConsole.Instance.Output(string.Format("Added {0} {1} to player id {2} wallet", _params[1], Wallet.Coin_Name, _steamid));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Subtracted {0} {1} from player id {2} wallet", _params[1], Wallet.Coin_Name, _steamid));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player id not found: {0}", _steamid));
                        }
                        _result.Dispose();
                    }
                }
                if (_params.Count == 1)
                {
                    string _steamid = SQL.EscapeString(_params[0]);
                    string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _steamid);
                    DataTable _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count != 0)
                    {
                        int currentCoins;
                        World world = GameManager.Instance.World;
                        int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out int _playerSpentCoins);
                        Player p = PersistentContainer.Instance.Players[_params[0], false];
                        int gameMode = world.GetGameMode();
                        if (gameMode == 7)
                        {
                            currentCoins = (p.ZKills * Wallet.Zombie_Kills) + (p.Kills * Wallet.Player_Kills) - (p.Deaths * Wallet.Deaths) + _playerSpentCoins;
                        }
                        else
                        {
                            currentCoins = (p.ZKills * Wallet.Zombie_Kills) - (p.Deaths * Wallet.Deaths) + _playerSpentCoins;
                        }
                        SdtdConsole.Instance.Output(string.Format("Wallet for id {0}: {1} {2}", _params[0], currentCoins, Wallet.Coin_Name));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player id not found: {0}", _steamid));
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