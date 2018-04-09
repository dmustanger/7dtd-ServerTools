using System;
using System.Collections.Generic;

namespace ServerTools.src.ConsoleCommands
{
    class Underground : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "Check if a player is underground with no clip or stuck";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. uc <steam id / player name / entity id> \n" +
                "  2. uc \n\n" +
                "1. List 1 players underground check\n" +
                "2. List all players underground check\n";
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
                    if (_params.Count == 1)
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
                Log.Out("Error in UndergroundCheck.Run: " + e);
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
