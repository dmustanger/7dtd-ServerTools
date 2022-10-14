using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class RotateEntityConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Rotates an entity to face away from the command user";
        }

        public override string GetHelp()
        {
            return "Rotates an entity to face away from the command user. Entity being rotated can not be a player.\n" +
                "Usage: st-erot <EntityId>\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-EntityRotate", "erot", "st-erot" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_senderInfo.RemoteClientInfo == null)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Invalid operation. This command must be used manually by an administrator in game");
                    return;
                }
                if (_params.Count != 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (!int.TryParse(_params[0], out int _entityId))
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid entityId value '{0}'", _entityId));
                    return;
                }
                Entity Entity = GameManager.Instance.World.Entities.dict[_entityId];
                if (Entity == null)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid entityId. Entity not found '{0}'", _entityId));
                    return;
                }
                if (Entity is EntityPlayer)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Entity with id '{0}' can not be a player", _entityId));
                    return;
                }
                EntityPlayer _player = GeneralFunction.GetEntityPlayer(_senderInfo.RemoteClientInfo.entityId);
                if (_player != null)
                {
                    Entity.SetRotation(new UnityEngine.Vector3(_player.position.x, _player.position.y, _player.position.z));
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Rotated entity {0} to face away from you", _entityId));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EntityRotateConsole.Execute: {0}", e.Message));
            }
        }
    }
}
