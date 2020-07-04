using ServerTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using ServerTools.AntiCheat;

public static class Injections
{

    public static bool DamageResponse_Prefix(EntityAlive __instance, DamageResponse _dmResponse)
    {
        try
        {
            if (ProcessDamage.Damage_Detector || Zones.IsEnabled || Lobby.IsEnabled)
            {
                return ProcessDamage.ProcessPlayerDamage(__instance, _dmResponse);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.DamageResponse_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static bool PlayerLoginRPC_Prefix(GameManager __instance, ClientInfo _cInfo, string _playerName, string _playerId, string _compatibilityVersion)
    {
        try
        {
            if (ReservedSlots.IsEnabled && _playerId != null && _playerId.Length == 17 && ConnectionManager.Instance.ClientCount() > PersistentOperations.MaxPlayers)
            {
                return ReservedSlots.FullServer(_playerId, _playerName, _compatibilityVersion);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.PlayerLoginRPC_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static bool ChangeBlocks_Prefix(GameManager __instance, string persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
    {
        try
        {
            return ProcessDamage.ProcessBlockDamage(__instance, persistentPlayerId, _blocksToChange);
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ChangeBlocks_Prefix: {0}", e.Message));
        }
        return true;
    }

    public static bool ExplosionServer_Prefix(GameManager __instance, Vector3 _worldPos, Vector3i _blockPos, int _playerId)
    {
        try
        {
            if (_playerId > 0)
            {
                ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_playerId);
                if (_cInfo != null)
                {
                    GameManager.Instance.adminTools.GetAdmins().TryGetValue(_cInfo.playerId, out AdminToolsClientInfo Admin);
                    if (Admin.PermissionLevel > ProcessDamage.Admin_Level)
                    {
                        return ProtectedSpaces.AllowExplosion(_worldPos);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ExplosionServer_Prefix: {0}", e.Message));
        }
        return true;
    }
}