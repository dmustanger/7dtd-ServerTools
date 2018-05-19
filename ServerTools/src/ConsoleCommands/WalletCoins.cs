using System;
using System.Collections.Generic;

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
                if (_params.Count == 2)
                {
                    if (_params[0].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not adjust wallet: Invalid Id {0}", _params[0]));
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
                        List<string> playerlist = PersistentContainer.Instance.Players.SteamIDs;
                        for (int i = 0; i < playerlist.Count; i++)
                        {
                            int _counter = 0;
                            string _steamId = playerlist[i];
                            if (_steamId == _params[0])
                            {
                                Player p = PersistentContainer.Instance.Players[_steamId, false];
                                int _spentCoins = p.PlayerSpentCoins;
                                int _newCoins = _spentCoins + _adjustCoins;
                                PersistentContainer.Instance.Players[_steamId, true].PlayerSpentCoins = _newCoins;
                                PersistentContainer.Instance.Save();
                                if (_adjustCoins >= 0)
                                {
                                    SdtdConsole.Instance.Output(string.Format("Added {0} {1} to player id {2} wallet", _params[1], Wallet.Coin_Name, _params[0]));
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("Subtracted {0} {1} from player id {2} wallet", _params[1], Wallet.Coin_Name, _params[0]));
                                    return;
                                }
                            }
                            else
                            {
                                _counter++;
                                if (_counter == playerlist.Count)
                                {
                                    SdtdConsole.Instance.Output(string.Format("Player with Id {0} is not online.", _params[0]));
                                    return;
                                }
                            }
                        }
                    }
                }
                if (_params.Count == 1)
                {
                    if (_params[0].Length == 17)
                    {
                        int _id;
                        if (!int.TryParse(_params[0], out _id))
                        {
                            int currentCoins;
                            World world = GameManager.Instance.World;
                            Player p = PersistentContainer.Instance.Players[_params[0], false];
                            if (p != null)
                            {
                                int gameMode = world.GetGameMode();
                                if (gameMode == 7)
                                {
                                    currentCoins = (p.ZKills * Wallet.Zombie_Kills) + (p.Kills * Wallet.Player_Kills) - (p.Deaths * Wallet.Deaths) + p.PlayerSpentCoins;
                                }
                                else
                                {
                                    currentCoins = (p.ZKills * Wallet.Zombie_Kills) - (p.Deaths * Wallet.Deaths) + p.PlayerSpentCoins;
                                }
                                SdtdConsole.Instance.Output(string.Format("Wallet for id {0}: {1} {2}", _params[0], currentCoins, Wallet.Coin_Name));
                                return;
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Player id not found: {0}", _id));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Can not check wallet: Invalid Id {0}", _params[0]));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not adjust wallet: Invalid Id {0}", _params[0]));
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