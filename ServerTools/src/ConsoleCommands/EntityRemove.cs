using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class RemoveEntity : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "Removes a entity from the game";
        }

        public override string GetHelp()
        {
            return "Removes a entity from the game.\n" +
                "Usage: entityremove <entityId>\n" +
                "Usage; er <entityid>";
        }

        public override string[] GetCommands()
        {
            return new string[] { "entityremove", "er" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                int _entityId = int.MinValue;
                if (!int.TryParse(_params[0], out _entityId))
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid entityId: {0}", _entityId));
                    return;
                }
                else
                {
                    GameManager.Instance.World.RemoveEntity(_entityId, EnumRemoveEntityReason.Despawned);
                    SdtdConsole.Instance.Output(string.Format("Removed entity {0} ", _entityId));
                }    
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EntityRemove.Run: {0}.", e));
            }
        }
    }
}
