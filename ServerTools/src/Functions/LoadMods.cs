namespace ServerTools
{
    public class Mods
    {
        public static void Init()
        {
            GameItems.LoadGameItems();
            CustomCommands.Init();
            KillMe.Init();
            Gimme.Init();
            HighPingKicker.Init();
            InventoryCheck.Init();
            TeleportHome.Init();
            Badwords.Init();
            InfoTicker.Init();
            SaveWorld.Init();
        }
    }
}