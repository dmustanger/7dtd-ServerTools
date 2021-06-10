using ServerTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class Injections
{
    public static bool DamageResponse_Prefix(EntityAlive __instance, DamageResponse _dmResponse)
    {
        try
        {
            if (EntityDamage.IsEnabled || Zones.IsEnabled || Lobby.IsEnabled || Market.IsEnabled)
            {
                return EntityDamage.ProcessEntityDamage(__instance, _dmResponse);
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
            if (_playerId != null && _playerId.Length == 17)
            {
                int __maxPlayers = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
                if (ReservedSlots.IsEnabled && ConnectionManager.Instance.ClientCount() > __maxPlayers)
                {
                    if (ReservedSlots.FullServer(_playerId))
                    {
                        GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, __maxPlayers + 1);
                        __state = true;
                        return;
                    }
                }
                if (NewPlayer.IsEnabled && NewPlayer.Block_During_Bloodmoon && SkyManager.BloodMoon())
                {
                    PersistentPlayerData _ppd = PersistentOperations.GetPersistentPlayerDataFromSteamId(_playerId);
                    if (_ppd == null)
                    {
                        ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_playerId);
                        if (_cInfo != null)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerDenied>().Setup(new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, 0, default, "[ServerTools] - New players are kicked during the bloodmoon. Please return after the bloodmoon is over")));
                            return;
                        }
                    }
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
                int _maxPlayers = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
                GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, _maxPlayers - 1);
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
            if (POIProtection.IsEnabled || BlockChange.IsEnabled)
            {
                return BlockChange.ProcessBlockChange(__instance, persistentPlayerId, _blocksToChange);
            }
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
                FallingBlocks.Single(_block);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.AddFallingBlock_Postfix: {0}", e.Message));
        }
    }

    public static void AddFallingBlocks_Postfix(IList<Vector3i> _list)
    {
        try
        {
            if (FallingBlocks.IsEnabled)
            {
                FallingBlocks.Multiple(_list);
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.AddFallingBlocks_Postfix: {0}", e.Message));
        }
    }

    public static void ChatMessageServer_Postfix(ClientInfo _cInfo, EChatType _chatType, int _senderEntityId, string _msg, string _mainName)
    {
        try
        {
            if (DiscordBot.IsEnabled && DiscordBot.Webhook != "" && DiscordBot.Webhook.StartsWith("https://discord.com/api/webhooks") &&
                _chatType == EChatType.Global && !string.IsNullOrWhiteSpace(_mainName) && !_mainName.Contains(DiscordBot.Prefix) &&
                !PersistentOperations.InvalidPrefix.Contains(_msg[0]))
            {
                if (_msg.Contains("[") && _msg.Contains("]"))
                {
                    _msg = Regex.Replace(_msg, @"\[.*?\]", "");
                }
                if (_mainName.Contains("[") && _mainName.Contains("]"))
                {
                    _mainName = Regex.Replace(_mainName, @"\[.*?\]", "");
                }
                if (_cInfo != null)
                {
                    if (DiscordBot.LastEntry != _msg)
                    {
                        DiscordBot.LastPlayer = _cInfo.playerId;
                        DiscordBot.LastEntry = _msg;
                        DiscordBot.Queue.Add("[Game] **" + _mainName + "**  " + DiscordBot.LastEntry);
                    }
                    else if (DiscordBot.LastPlayer != _cInfo.playerId)
                    {
                        DiscordBot.LastPlayer = _cInfo.playerId;
                        DiscordBot.LastEntry = _msg;
                        DiscordBot.Queue.Add("[Game] **" + _mainName + "**  " + DiscordBot.LastEntry);
                    }
                }
                else if (DiscordBot.LastEntry != _msg)
                {
                    DiscordBot.LastPlayer = "-1";
                    DiscordBot.LastEntry = _msg;
                    DiscordBot.Queue.Add("[Game] **" + _mainName + "**  " + DiscordBot.LastEntry);
                }
            }
        }
        catch (Exception e)
        {
            Log.Out(string.Format("[SERVERTOOLS] Error in Injections.ChatMessageServer_Postfix: {0}", e.Message));
        }
    }
}