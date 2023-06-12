using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class RemoveEntityConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Removes an entity from the game";
        }

        protected override string getHelp()
        {
            return "Removes an entity from the game\n" +
                "Usage: st-rem <EntityId>\n" +
                "Usage: rem <EntityId>";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-RemoveEntity", "rem", "st-rem" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                int entityId = 0;
                if (!int.TryParse(_params[0], out entityId))
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid entityId '{0}'", entityId));
                    return;
                }
                else if (GameManager.Instance.World.Entities.dict.ContainsKey(entityId) && GameManager.Instance.World.Entities.dict.TryGetValue(entityId, out Entity entity) &&
                    entity != null)
                {
                    if (!entity.IsMarkedForUnload())
                    {
                        entity.MarkToUnload();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Marked entity '{0}' for unload", entityId));
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Entity '{0}' is already marked for unload", entityId));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Entity '{0}' not found. Unable to remove", entityId));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RemoveEntityConsole.Execute: {0}", e.Message));
            }
        }
    }
}
