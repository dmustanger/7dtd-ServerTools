using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class RotateEntityConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Rotates an entity to face away from the command user";
        }

        public override string GetHelp()
        {
            return "Rotates an entity to face away from the command user. Entity being rotated can not be a player.\n" +
                "Usage: EntityRotate <EntityId>\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-EntityRotate", "EntityRotate", "entityrotate", "erot" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_senderInfo.RemoteClientInfo == null)
                {
                    SdtdConsole.Instance.Output("Invalid operation. This command must be used manually by an administrator in game");
                    return;
                }
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                int _entityId;
                if (!int.TryParse(_params[0], out _entityId))
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid entityId value: {0}", _entityId));
                    return;
                }
                Entity Entity = GameManager.Instance.World.Entities.dict[_entityId];
                if (Entity == null)
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid entityId. Entity not found: {0}", _entityId));
                    return;
                }
                if (Entity.IsClientControlled())
                {
                    SdtdConsole.Instance.Output(string.Format("Entity with id {0} can not be a player", _entityId));
                    return;
                }
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                if (_player != null)
                {
                    Entity.SetRotation(new UnityEngine.Vector3(_player.position.x, _player.position.y, _player.position.z));
                    SdtdConsole.Instance.Output(string.Format("Rotated entity {0} to face away from you", _entityId));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EntityRotateConsole.Execute: {0}", e));
            }
        }
    }
}
