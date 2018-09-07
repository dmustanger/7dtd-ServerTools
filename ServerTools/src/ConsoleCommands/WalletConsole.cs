using System;
using System.Collections.Generic;
using System.Data;

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
                    SdtdConsole.Instance.Output(string.Format("Wallet has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Wallet.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Wallet has been set to on"));
                    return;
                }
                else if (_params.Count == 2)
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
                        if (_result.Rows.Count != 0)
                        {
                            Wallet.AddCoinsToWallet(_steamid, _adjustCoins);
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
                else if (_params.Count == 1)
                {
                    string _steamid = SQL.EscapeString(_params[0]);
                    string _sql = string.Format("SELECT playerSpentCoins, zkills, kills, deaths FROM Players WHERE steamid = '{0}'", _steamid);
                    DataTable _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count != 0)
                    {
                        int currentCoins;
                        World world = GameManager.Instance.World;
                        int _playerSpentCoins;
                        int _zkills;
                        int _kills;
                        int _deaths;
                        int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
                        int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _zkills);
                        int.TryParse(_result.Rows[0].ItemArray.GetValue(2).ToString(), out _kills);
                        int.TryParse(_result.Rows[0].ItemArray.GetValue(3).ToString(), out _deaths);
                        int gameMode = world.GetGameMode();
                        if (gameMode == 7)
                        {
                            currentCoins = (_zkills * Wallet.Zombie_Kills) + (_kills * Wallet.Player_Kills) - (_deaths * Wallet.Deaths) + _playerSpentCoins;
                        }
                        else
                        {
                            currentCoins = (_zkills * Wallet.Zombie_Kills) - (_deaths * Wallet.Deaths) + _playerSpentCoins;
                        }
                        SdtdConsole.Instance.Output(string.Format("Wallet for id {0}: {1} {2}", _params[0], currentCoins, Wallet.Coin_Name));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player id not found: {0}", _steamid));
                    }
                    _result.Dispose();
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