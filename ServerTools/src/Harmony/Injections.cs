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

    public static void PlayerLoginRPC_Prefix(string _playerId, out bool __state)
    {
        __state = false;
        try
        {
            int _maxCount = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
            if (ReservedSlots.IsEnabled && _playerId != null && _playerId.Length == 17 && ConnectionManager.Instance.ClientCount() > _maxCount)
            {
                if (ReservedSlots.FullServer(_playerId))
                {
                    GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, _maxCount + 1);
                    __state = true;
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.PlayerLoginRPC_Prefix: {0}", e.Message));
        }
    }

    public static void PlayerLoginRPC_Postfix(bool __state)
    {
        try
        {
            if (__state)
            {
                int _maxCount = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
                GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, _maxCount - 1);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.PlayerLoginRPC_Postfix: {0}", e.Message));
        }
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

    public static bool ExplosionServer_Prefix(Vector3 _worldPos, int _playerId)
    {
        try
        {
            if (_playerId > 0)
            {
                ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_playerId);
                if (_cInfo != null)
                {
                    if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > ProcessDamage.Admin_Level)
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

    public static void ServerConsoleCommand_Postfix(ClientInfo _cInfo, string _cmd)
    {
        try
        {
            ConsoleCommandLog.Exec(_cInfo, _cmd);
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ServerConsoleCommand_Postfix: {0}", e.Message));
        }
    }

    //public static bool IsWithinTraderArea_Prefix(World __instance, Vector3i _worldBlockPos, ref bool __result)
    //{
    //    try
    //    {
    //        if (ProtectedSpaces.IsEnabled && ProtectedSpaces.IsProtectedSpace(_worldBlockPos))
    //        {
    //            Log.Out(string.Format("[SERVERTOOLS] Inside protected space. Altering result for Location: {0}, {1}, {2}", _worldBlockPos.x, _worldBlockPos.y, _worldBlockPos.z));
    //            __result = true;
    //            return false;
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Log.Out(string.Format("[SERVERTOOLS] Error in Injections.IsWithinTraderArea_Prefix: {0}", e.Message));
    //    }
    //    return true;
    //}
}