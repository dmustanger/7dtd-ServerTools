using System.Collections.Generic;

namespace ServerTools
{
    class PlayerStatCheck
    {
        public static bool IsEnabled = false;
        public static int AdminLevel = 0;
        public static bool KickEnabled = false;
        public static bool BanEnabled = false;

        public static void PlayerStat()
        {
            int _playerCount = ConnectionManager.Instance.ClientCount();
            if (_playerCount > 0)
            {
                World world = GameManager.Instance.World;
                var enumerator = world.Players.list;
                foreach (var ent in enumerator)
                {
                    if (ent.IsClientControlled())
                    {
                        if (ent.IsSpawned() == true)
                        {
                            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                            foreach (var _cInfo in _cInfoList)
                            {
                                GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel > AdminLevel)
                                {
                                    if (ent.entityId == _cInfo.entityId)
                                    {
                                        int pointsPer = Progression.SkillPointsPerLevel;
                                        int maxPlayerLevel = Progression.MaxLevel;
                                        var p_speedForward = ent.speedForward;
                                        var p_Health = ent.Stats.Health.Value;
                                        var p_Stamina = ent.Stats.Stamina.Value;
                                        var p_ExpToNextLevel = ent.ExpToNextLevel;
                                        var p_jumpStrength = ent.jumpStrength;
                                        var p_Level = ent.Level;
                                        var p_SkillPoints = ent.SkillPoints;
                                        var p_height = ent.height;
                                        var maxPts = (p_Level * pointsPer + 42);

                                        if (p_Health > 250)
                                        {
                                            Log.Warning("Detected player {0} steamId {1} with health @ {2}. 250 is maximum", _cInfo.playerName, _cInfo.playerId, p_Health);
                                            if (KickEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat health[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat health\"", _cInfo.playerId), _cInfo);
                                            }
                                            if (BanEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and banned for illegal player stat health[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat health\"", _cInfo.playerId), _cInfo);
                                            }
                                        }
                                        if (p_Stamina > 250)
                                        {
                                            Log.Warning("Detected player {0} steamId {1} with stamina @ {2}. 250 is maximum", _cInfo.playerName, _cInfo.playerId, p_Stamina);
                                            if (KickEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat stamina[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat stamina\"", _cInfo.playerId), _cInfo);
                                            }
                                            if (BanEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat stamina[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat stamina\"", _cInfo.playerId), _cInfo);
                                            }
                                        }
                                        if (p_jumpStrength >= 1.2)
                                        {
                                            Log.Warning("Detected player {0} steamId {1} at jump strength {2}. 1.1 is default", _cInfo.playerName, _cInfo.playerId, p_jumpStrength);
                                            if (KickEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player jump ability[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat jump\"", _cInfo.playerId), _cInfo);
                                            }
                                            if (BanEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player jump ability[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat jump\"", _cInfo.playerId), _cInfo);
                                            }
                                        }
                                        if (p_Level > maxPlayerLevel)
                                        {
                                            Log.Warning("Detected player {0} steamId {1} at level {2}. Maximum level set is {3}", _cInfo.playerName, _cInfo.playerId, p_Level, maxPlayerLevel);
                                            if (KickEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat level[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat level\"", _cInfo.playerId), _cInfo);
                                            }
                                            if (BanEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat level[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat level\"", _cInfo.playerId), _cInfo);
                                            }
                                        }
                                        if (p_SkillPoints > maxPts)
                                        {
                                            Log.Warning("Detected player {0} steamId {1} with {2} skill points available, while only level {3}. Player can only have {4} skill points at this level.", _cInfo.playerName, _cInfo.playerId, p_SkillPoints, p_Level, maxPts);
                                            if (KickEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat skill points available[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat skill points\"", _cInfo.playerId), _cInfo);
                                            }
                                            if (BanEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player stat skill points available[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat skill points\"", _cInfo.playerId), _cInfo);
                                            }
                                        }
                                        if (p_height > 1.8 || p_height < 1.7)
                                        {
                                            Log.Warning("Detected player {0} steamId {1} with player height @ {2}", _cInfo.playerName, _cInfo.playerId, p_height);
                                            if (KickEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player height[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat height\"", _cInfo.playerId), _cInfo);
                                            }
                                            if (BanEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player height[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat height\"", _cInfo.playerId), _cInfo);
                                            }
                                        }
                                        if (p_speedForward > 14)
                                        {
                                            Log.Warning("Detected player {0} steamId {1} speed hacking", _cInfo.playerName, _cInfo.playerId);
                                            if (KickEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player speed[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for illegal player stat speed\"", _cInfo.playerId), _cInfo);
                                            }
                                            if (BanEnabled)
                                            {
                                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} was detected and kicked for illegal player speed[-]", _cInfo.playerName), "Server", false, "", false);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for illegal player stat speed\"", _cInfo.playerId), _cInfo);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
