namespace ServerTools
{
    public class Mods
    {
        public static void Load()
        {
            if (AutoSaveWorld.IsRunning && !AutoSaveWorld.IsEnabled)
            {
                AutoSaveWorld.Stop();
            }
            if (!AutoSaveWorld.IsRunning && AutoSaveWorld.IsEnabled)
            {
                AutoSaveWorld.Start();
            }
            if (Badwords.IsRunning && !Badwords.IsEnabled)
            {
                Badwords.Unload();
            }
            if (!Badwords.IsRunning && Badwords.IsEnabled)
            {
                Badwords.Load();
            }
            if (ClanManager.IsEnabled)
            {
                ClanData.Init();
            }
            if (CustomCommands.IsRunning && !CustomCommands.IsEnabled)
            {
                CustomCommands.Unload();
            }
            if (!CustomCommands.IsRunning && CustomCommands.IsEnabled)
            {
                CustomCommands.Load();
            }
            if (Gimme.IsRunning && !Gimme.IsEnabled)
            {
                Gimme.Unload();
            }
            if (!Gimme.IsRunning && Gimme.IsEnabled)
            {
                Gimme.Load();
            }
            if (HighPingKicker.IsRunning && !HighPingKicker.IsEnabled)
            {
                HighPingKicker.Unload();
            }
            if (!HighPingKicker.IsRunning && HighPingKicker.IsEnabled)
            {
                HighPingKicker.Load();
            }
            if (InfoTicker.IsRunning && !InfoTicker.IsEnabled)
            {
                InfoTicker.Unload();
            }
            if (!InfoTicker.IsRunning && InfoTicker.IsEnabled)
            {
                InfoTicker.Load();
            }
            if (InventoryCheck.IsRunning && !InventoryCheck.IsEnabled)
            {
                InventoryCheck.Unload();
            }
            if (!InventoryCheck.IsRunning && InventoryCheck.IsEnabled)
            {
                InventoryCheck.Load();
            }
            if (KillMe.IsEnabled)
            {
                KillMe.Load();
            }
        }
    }
}