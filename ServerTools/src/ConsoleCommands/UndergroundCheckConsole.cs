using System;
using System.Collections.Generic;

namespace ServerTools.src.ConsoleCommands
{
    class UndergroundCheckConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "Check if a player is underground with no clip or stuck";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. UndergroundCheck off \n" +
                "  2. UndergroundCheck on \n\n" +
                "  3. UndergroundCheck <steam id / player name / entity id> \n" +
                "  4. UndergroundCheck \n\n" +
                "1. Turn off underground check\n" +
                "2. Turn on underground check\n" +
                "3. List 1 players underground check\n" +
                "4. List all players underground check\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-UndergroundCheck", "undergroundcheck", "uc" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 1)
                {
                    SdtdConsole.Instance.Output("Wrong number of arguments, expected 0 or 1, found " + _params.Count + ".");
                    return;
                }
                else
                {
                    if (_params[0].ToLower().Equals("off"))
                    {
                        UndergroundCheck.IsEnabled = false;
                        SdtdConsole.Instance.Output(string.Format("Underground check has been set to off"));
                        return;
                    }
                    else if (_params[0].ToLower().Equals("on"))
                    {
                        UndergroundCheck.IsEnabled = true;
                        SdtdConsole.Instance.Output(string.Format("Underground check has been set to on"));
                        return;
                    }
                    else if (_params.Count == 1)
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                        if (_cInfo == null)
                        {
                            SdtdConsole.Instance.Output("Playername or entity/steamid id not found.");
                            return;
                        }
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        SdtdConsole.Instance.Output("UC: player name=" + _cInfo.playerName + " entity id=" + _cInfo.entityId + " underGround=" + getPlayerUnderground(_player));
                    }
                    else
                    {
                        World world = GameManager.Instance.World;
                        List<EntityPlayer> PlayerList = world.Players.list;
                        for (int i = 0; i < PlayerList.Count; i++)
                        {
                            EntityPlayer _player = PlayerList[i];
                            SdtdConsole.Instance.Output("UC: player name=" + _player.name + " entity id=" + _player.entityId + " underGround=" + getPlayerUnderground(_player));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in UndergroundCheckConsole.Run: {0}.", e));
            }
        }

        public bool getPlayerUnderground(EntityPlayer _player)
        {
            int x = (int)_player.position.x;
            int y = (int)_player.position.y;
            int z = (int)_player.position.z;
            for (int i = x - 2; i <= (x + 2); i++)
            {
                for (int j = z - 2; j <= (z + 2); j++)
                {
                    for (int k = y - 3; k <= (y + 2); k++)
                    {
                        BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        if (Block.type == BlockValue.Air.type || _player.IsInElevator() || _player.IsInWater())
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
