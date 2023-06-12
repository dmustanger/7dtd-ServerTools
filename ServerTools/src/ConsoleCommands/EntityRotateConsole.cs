using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class RotateEntityConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Rotates an entity to face away from the command user";
        }

        protected override string getHelp()
        {
            return "Rotates an entity to face away from the command user. Entity being rotated can not be a player.\n" +
                "Usage: st-erot <EntityId>\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-EntityRotate", "erot", "st-erot" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_senderInfo.RemoteClientInfo == null)
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Invalid operation. This command must be used manually by an administrator in game");
                    return;
                }
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (!int.TryParse(_params[0], out int _entityId))
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid entityId value '{0}'", _entityId));
                    return;
                }
                Entity entity = GeneralOperations.GetEntity(_entityId);
                if (entity == null)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid entityId. Entity not found '{0}'", _entityId));
                    return;
                }
                if (entity is EntityPlayer)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Entity with id '{0}' can not be a player", _entityId));
                    return;
                }
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_senderInfo.RemoteClientInfo.entityId);
                if (player != null)
                {
                    ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                    {
                        ThreadManager.StartCoroutine(SetRot(entity, new Vector3(player.position.x, player.position.y, player.position.z)));
                    }, null);
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Rotated entity {0} to face away from you", _entityId));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EntityRotateConsole.Execute: {0}", e.Message));
            }
        }

        public static IEnumerator SetRot(Entity _entity, Vector3 _position)
        {
            try
            {
                if (_entity != null && _position != null)
                {
                    _entity.SetRotation(_position);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EntityRotateConsole.SetPos: {0}", e.StackTrace));
            }
            yield break;
        }
    }
}
