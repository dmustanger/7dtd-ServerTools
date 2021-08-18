using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class DeleteEntityConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Deletes an entity from the game.";
        }

        public override string GetHelp()
        {
            return "Deletes an entity from the game.\n" +
                "Usage: st-de <EntityId>\n" +
                "Usage: de <EntityId>";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-DeleteEntity", "de", "st-de" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                int _entityId = int.MinValue;
                if (!int.TryParse(_params[0], out _entityId))
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid entityId: {0}", _entityId));
                    return;
                }
                else
                {
                    GameManager.Instance.World.RemoveEntity(_entityId, EnumRemoveEntityReason.Despawned);
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Deleted entity {0} ", _entityId));
                }    
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DeleteEntityConsole.Run: {0}.", e.Message));
            }
        }
    }
}
