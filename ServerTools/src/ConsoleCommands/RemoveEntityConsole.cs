using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class RemoveEntityConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Removes an entity from the game";
        }

        public override string GetHelp()
        {
            return "Removes an entity from the game\n" +
                "Usage: st-rem <EntityId>\n" +
                "Usage: rem <EntityId>";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-RemoveEntity", "rem", "st-rem" };
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
                int _entityId = int.MinValue;
                if (!int.TryParse(_params[0], out _entityId))
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid entityId '{0}'", _entityId));
                    return;
                }
                else
                {
                    GameManager.Instance.World.RemoveEntity(_entityId, EnumRemoveEntityReason.Despawned);
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed entity '{0}'", _entityId));
                }    
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RemoveEntityConsole.Execute: {0}", e.Message));
            }
        }
    }
}
