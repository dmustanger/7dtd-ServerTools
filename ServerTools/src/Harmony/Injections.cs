using ServerTools;
using System;
using System.Collections.Generic;
using ServerTools.AntiCheat;

public static class Injections
{
    public static bool DamageResponse_Prefix(EntityAlive __instance, DamageResponse _dmResponse)
    {
        try
        {
            if (ProcessDamage.Damage_Detector || Zones.IsEnabled || Lobby.IsEnabled || Market.IsEnabled)
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

    public static void AddFallingBlock_Postfix(Vector3i _block)
    {
        try
        {
            if (FallingBlocks.IsEnabled)
            {
                FallingBlocks.Exec(_block);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.AddFallingBlock_Postfix: {0}", e.Message));
        }
    }
}