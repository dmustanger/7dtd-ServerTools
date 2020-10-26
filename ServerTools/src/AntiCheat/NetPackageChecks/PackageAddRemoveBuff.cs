using HarmonyLib;
using System;

namespace ServerTools
{
    class PackageAddRemoveBuff
    {
        static AccessTools.FieldRef<NetPackageAddRemoveBuff, int> _entityId = AccessTools.FieldRefAccess<NetPackageAddRemoveBuff, int>("m_entityId");
        static AccessTools.FieldRef<NetPackageAddRemoveBuff, string> _buffName = AccessTools.FieldRefAccess<NetPackageAddRemoveBuff, string>("buffName");

        public static bool PackageAddRemoveBuff_ProcessPackage_Prefix(NetPackageAddRemoveBuff __instance, World _world)
        {
            if (__instance.Sender != null)
            {
                ClientInfo _cInfo = __instance.Sender;
                EntityAlive _entityAlive = _world.GetEntity(_entityId(__instance)) as EntityAlive;
                if (_entityAlive != null)
                {
                    if (!GameManager.Instance.adminTools.IsAdmin(_cInfo))
                    {
                        string _buff = _buffName(__instance).ToLower();
                        if (_buff.Contains("god"))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageAddRemoveBuff uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempting to apply or remove 'god' buff without permission to entity id {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityAlive.entityId));
                            Packages.Ban(_cInfo);
                            Packages.Writer(_cInfo, string.Format("Attempting to apply or remove 'god' buff without permission to entity id {0}", _entityAlive.entityId));
                            return false;
                        }
                        else if (_buff.Contains("megadamage"))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageAddRemoveBuff uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempting to apply or remove 'megadamage' buff without permission to entity id {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityAlive.entityId));
                            Packages.Ban(_cInfo);
                            Packages.Writer(_cInfo, string.Format("Attempting to apply or remove 'megadamage' buff without permission to entity id {0}", _entityAlive.entityId));
                            return false;
                        }
                        else if (_buff.Contains("buffme"))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageAddRemoveBuff uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempting to apply or remove 'buffme' buff without permission to entity id {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityAlive.entityId));
                            Packages.Ban(_cInfo);
                            Packages.Writer(_cInfo, string.Format("Attempting to apply or remove 'buffme' buff without permission to entity id {0}", _entityAlive.entityId));
                            return false;
                        }
                        else if (_buff.Contains("nerfme"))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageAddRemoveBuff uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempting to apply or remove 'nerfme' buff without permission to entity id {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityAlive.entityId));
                            Packages.Ban(_cInfo);
                            Packages.Writer(_cInfo, string.Format("Attempting to apply or remove 'nerfme' buff without permission to entity id {0}", _entityAlive.entityId));
                            return false;
                        }
                        else if (_buff.Contains("maxedout"))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageAddRemoveBuff uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempting to apply or remove 'maxedout' buff without permission to entity id {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityAlive.entityId));
                            Packages.Ban(_cInfo);
                            Packages.Writer(_cInfo, string.Format("Attempting to apply or remove 'maxedout' buff without permission to entity id {0}", _entityAlive.entityId));
                            return false;
                        }
                        else if (_buff.Contains("pegasus"))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Detected erroneous data NetPackageAddRemoveBuff uploaded by steam id {0}, owner id {1}, entity id {2} name {3}. Attempting to apply or remove 'pegasus' buff without permission to entity id {4}", _cInfo.playerId, _cInfo.ownerId, _cInfo.entityId, _cInfo.playerName, _entityAlive.entityId));
                            Packages.Ban(_cInfo);
                            Packages.Writer(_cInfo, string.Format("Attempting to apply or remove 'pegasus' buff without permission to entity id {0}", _entityAlive.entityId));
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
