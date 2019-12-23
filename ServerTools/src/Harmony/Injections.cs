using ServerTools;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class Injections
{

    public static bool DamageResponse_Prefix(EntityAlive __instance, ref DamageResponse _dmResponse)
    {
        try
        {
            return DamageDetector.ProcessPlayerDamage(__instance, _dmResponse);
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.DamageResponse_Prefix: {0}.", e.Message));
        }
        return true;
    }

    public static bool PlayerLoginRPC_Prefix(GameManager __instance, ClientInfo _cInfo, string _playerName, string _playerId, string _compatibilityVersion)
    {
        try
        {
            if (ReservedSlots.IsEnabled && (_playerId == null || _playerId.Length < 17))
            {
                return true;
            }
            else if (ConnectionManager.Instance.ClientCount() > PersistentOperations.MaxPlayers)
            {
                return ReservedSlots.FullServer(_playerId, _playerName, _compatibilityVersion);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.PlayerLoginRPC_Prefix: {0}.", e.Message));
        }
        return true;
    }

    public static bool ChangeBlocks_Prefix(GameManager __instance, string persistentPlayerId, ref List<BlockChangeInfo> _blocksToChange)
    {
        try
        {
            Log.Out(string.Format("[SERVERTOOLS] ChangeBlocks_Prefix executed."));
            return DamageDetector.ProcessBlockDamage(__instance, persistentPlayerId, _blocksToChange);
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ChangeBlocks_Prefix: {0}.", e.Message));
        }
        return true;
    }

    public static bool ExplosionServer_Prefix(GameManager __instance, Vector3 _worldPos, Vector3i _blockPos, int _playerId)
    {
        try
        {
            if (_playerId > 0)
            {
                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_playerId.ToString());
                if (Admin.PermissionLevel > DamageDetector.Admin_Level)
                {
                    return ProtectedSpace.AllowExplosion(_worldPos);
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ExplosionServer_Prefix: {0}.", e.Message));
        }
        return true;
    }
}
