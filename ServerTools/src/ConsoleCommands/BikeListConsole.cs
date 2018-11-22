using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class BikeListConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Lists the currently assembled minibikes.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. BikeList" +
                "1. Shows a list of each bike, entity id, location and player riding the bike if applicable\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Bikelist", "bikelist", "bl" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                int _counter = 1;
                SdtdConsole.Instance.Output("Minibike List:");
                List<Entity> Entities = GameManager.Instance.World.Entities.list;
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    string _name = EntityClass.list[_entity.entityClass].entityClassName;
                    if (_name == "minibike")
                    {
                        Vector3 _pos = _entity.position;
                        int x = (int)_pos.x;
                        int y = (int)_pos.y;
                        int z = (int)_pos.z;
                        Entity _attachedPlayer = _entity.AttachedMainEntity;
                        if (_attachedPlayer != null)
                        {
                            List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
                            for (int j = 0; j < _cInfoList.Count; j++)
                            {
                                ClientInfo _cInfo = _cInfoList[j];
                                if (_cInfo != null)
                                {
                                    if (_attachedPlayer.entityId == _cInfo.entityId)
                                    {
                                        SdtdConsole.Instance.Output(string.Format("#{0}: Id {1}, Located at x {2}  y {3} z {4}, Player {5} is riding this bike.", _counter, _entity.entityId, x, y, z, _cInfo.playerName));
                                    }
                                }
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("#{0}: Id {1}, Located at x {2}  y {3} z {4}.", _counter, _entity.entityId, x, y, z));
                        }
                        _counter++;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BikeListConsole.Run: {0}.", e));
            }
        }
    }
}
