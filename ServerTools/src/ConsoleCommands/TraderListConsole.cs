using System;
using System.Collections.Generic;

namespace ServerTools
{
    class TraderListConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Lists the trader locations.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                "  1. st-tl\n" +
                "1. Shows a list of trader locations\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-Traderlist", "tl", "st-tl" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                int _counter = 1;
                List<TraderArea> _traderlist = GameManager.Instance.World.TraderAreas;
                for (int i = 0; i < _traderlist.Count; i++)
                {
                    TraderArea _traderArea = _traderlist[i];
                    Vector3i _position = _traderArea.Position;
                    bool _closed = _traderArea.IsClosed;
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("#{0}: Trader located at x {1} y {2} z {3}", _counter, _position.x, _position.y, _position.z));
                    if (!_closed)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("#{0}: Trader is open", _counter));
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("#{0}: Trader is closed", _counter));
                    }
                    _counter++;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in TraderListConsole.Execute: {0}", e.Message));
            }
        }
    }
}
