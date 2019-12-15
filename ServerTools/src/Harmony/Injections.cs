using ServerTools;
using System;
using System.Collections.Generic;

public static class Injections
{
    

    public static void ProcessDamageResponse_Postfix(EntityAlive __instance, DamageResponse _dmResponse)
    {
        DamageDetector.ProcessEntityDamage(__instance, _dmResponse);
    }

    public static bool PlayerLoginRPC_Prefix(GameManager __instance, ClientInfo _cInfo, string _playerName, string _playerId, string _token, string _compatibilityVersion)
    {
        try
        {
            if (ReservedSlots.IsEnabled && _playerId == null || _playerId.Length < 17 || (ConnectionManager.Instance.ClientCount() > PersistentOperations.MaxPlayers && ReservedSlots.FullServer(_playerId, _playerName, _compatibilityVersion)))
            {
                return true;
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ProcessPlayerLoginRPC: {0}.", e.Message));
        }
        return true;
    }

    public static bool ChangeBlocks_Prefix(GameManager __instance, string persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
    {
        if (DamageDetector.ProcessBlockDamage(__instance, persistentPlayerId, _blocksToChange))
        {
            return true;
        }
        return true;
    }
}
