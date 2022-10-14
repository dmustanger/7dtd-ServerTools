using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class MarketConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable market.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-mkt off\n" +
                   "  2. st-mkt on\n" +
                   "  3. st-mkt set\n" +
                   "1. Turn off the market\n" +
                   "2. Turn on the market\n" +
                   "3. Sets the market position to your current location\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Market", "mkt", "st-mkt" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Market.IsEnabled)
                    {
                        Market.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Market has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Market is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Market.IsEnabled)
                    {
                        Market.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Market has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Market is already on"));
                        return;
                    }
                }
                else if (_params[0] == "set")
                {
                    ClientInfo cInfo = _senderInfo.RemoteClientInfo;
                    EntityPlayer player = GameManager.Instance.World.Players.dict[cInfo.entityId];
                    Vector3 position = player.GetPosition();
                    int x = (int)position.x;
                    int y = (int)position.y;
                    int z = (int)position.z;
                    string lposition = x + "," + y + "," + z;
                    Market.MarketBounds.center = new Vector3(x, y, z);
                    int size = Market.Market_Size * 2;
                    Market.MarketBounds.size = new Vector3(size, size, size);
                    Market.Market_Position = lposition;
                    Phrases.Dict.TryGetValue("Market6", out string phrase);
                    phrase = phrase.Replace("{PlayerName}", cInfo.playerName);
                    phrase = phrase.Replace("{MarketPosition}", lposition);
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] {0}", phrase));
                    Config.WriteXml();
                    Config.LoadXml();
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in MarketConsole.Execute: {0}", e.Message));
            }
        }
    }
}