using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class MarketConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable or disable market.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-mkt off\n" +
                   "  2. st-mkt on\n" +
                   "  3. st-mkt set\n" +
                   "  4. st-mkt set {X} {Y} {Z}" +
                   "1. Turn off the market\n" +
                   "2. Turn on the market\n" +
                   "3. Sets the market position to your current location\n" +
                   "4. Sets the market position to the specified x y z location\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-Market", "mkt", "st-mkt" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower().Equals("off"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (Market.IsEnabled)
                    {
                        Market.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Market has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Market is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (!Market.IsEnabled)
                    {
                        Market.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Market has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Market is already on"));
                        return;
                    }
                }
                else if (_params[0] == "set")
                {
                    if (_params.Count != 1 && _params.Count != 4)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 or 4, found '{0}'", _params.Count));
                        return;
                    }
                    else if (_params.Count == 1)
                    {
                        ClientInfo cInfo = _senderInfo.RemoteClientInfo;
                        if (cInfo != null)
                        {
                            EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                            if (cInfo != null)
                            {
                                Vector3 position = player.GetPosition();
                                int x = (int)position.x;
                                int y = (int)position.y;
                                int z = (int)position.z;
                                string mposition = x + "," + y + "," + z;
                                Market.Market_Position = mposition;
                                Bounds bounds = new Bounds();
                                bounds.center = new Vector3(x, y, z);
                                int size = Market.Market_Size * 2;
                                bounds.size = new Vector3(size, size, size);
                                Market.MarketBounds[0] = bounds.min.x;
                                Market.MarketBounds[1] = bounds.min.y;
                                Market.MarketBounds[2] = bounds.min.z;
                                Market.MarketBounds[3] = bounds.max.x;
                                Market.MarketBounds[4] = bounds.max.y;
                                Market.MarketBounds[5] = bounds.max.z;
                                Phrases.Dict.TryGetValue("Market6", out string phrase);
                                phrase = phrase.Replace("{Position}", mposition);
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] {0}", phrase));
                                Config.WriteXml();
                                Config.LoadXml();
                            }
                        }
                    }
                    else
                    {
                        if (int.TryParse(_params[1], out int x))
                        {
                            if (int.TryParse(_params[2], out int y))
                            {
                                if (int.TryParse(_params[3], out int z))
                                {
                                    string mposition = x + "," + y + "," + z;
                                    Market.Market_Position = mposition;
                                    Bounds bounds = new Bounds();
                                    bounds.center = new Vector3(x, y, z);
                                    int size = Market.Market_Size * 2;
                                    bounds.size = new Vector3(size, size, size);
                                    Market.MarketBounds[0] = bounds.min.x;
                                    Market.MarketBounds[1] = bounds.min.y;
                                    Market.MarketBounds[2] = bounds.min.z;
                                    Market.MarketBounds[3] = bounds.max.x;
                                    Market.MarketBounds[4] = bounds.max.y;
                                    Market.MarketBounds[5] = bounds.max.z;
                                    Phrases.Dict.TryGetValue("Market6", out string phrase);
                                    phrase = phrase.Replace("{Position}", mposition);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] {0}", phrase));
                                    Config.WriteXml();
                                    Config.LoadXml();
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[3]));
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[3]));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[3]));
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in MarketConsole.Execute: {0}", e.Message));
            }
        }
    }
}