namespace ServerTools
{
    public class Mods
    {
        public static void Init()
        {
            GameItems.LoadGameItems();

            if (CustomCommands.IsRunning && !CustomCommands.IsEnabled)
            {
                CustomCommands._fileWatcher.Dispose();
                CustomCommands.IsRunning = false;
            }
            if (!CustomCommands.IsRunning && CustomCommands.IsEnabled)
            {
                CustomCommands.Init();
            }

            if (KillMe.IsEnabled)
            {
                KillMe.Init();
            }

            if (Gimme.IsRunning && !Gimme.IsEnabled)
            {
                Gimme._fileWatcher.Dispose();
                Gimme.IsRunning = false;
            }
            if (!Gimme.IsRunning && Gimme.IsEnabled)
            {
                Gimme.Init();
            }

            if (HighPingKicker.IsRunning && !HighPingKicker.IsEnabled)
            {
                HighPingKicker._fileWatcher.Dispose();
                HighPingKicker.IsRunning = false;
            }
            if (!HighPingKicker.IsRunning && HighPingKicker.IsEnabled)
            {
                HighPingKicker.Init();
            }

            if (InventoryCheck.IsRunning && !InventoryCheck.IsEnabled)
            {
                InventoryCheck._fileWatcher.Dispose();
                InventoryCheck.IsRunning = false;
            }
            if (!InventoryCheck.IsRunning && InventoryCheck.IsEnabled)
            {
                InventoryCheck.Init();
            }

            if (TeleportHome.IsEnabled)
            {
                TeleportHome.Init();
            }

            if (Badwords.IsRunning && !Badwords.IsEnabled)
            {
                Badwords._fileWatcher.Dispose();
                Badwords.IsRunning = false;
            }
            if (!Badwords.IsRunning && Badwords.IsEnabled)
            {
                Badwords.Init();
            }

            if (SaveWorld.IsRunning && !SaveWorld.IsEnabled)
            {
                SaveWorld.th.Abort();
                SaveWorld.IsRunning = false;
            }
            if (!SaveWorld.IsRunning && SaveWorld.IsEnabled)
            {
                SaveWorld.Init();
            }

            if (InfoTicker.IsRunning && !InfoTicker.IsEnabled)
            {
                InfoTicker.th.Abort();
                InfoTicker.IsRunning = false;
            }
            if (!InfoTicker.IsEnabled && !Motd.IsEnabled && InfoTicker.IsConfigLoaded)
            {
                InfoTicker._fileWatcher.Dispose();
                InfoTicker.IsConfigLoaded = false;
            }
            if (!InfoTicker.IsRunning && InfoTicker.IsEnabled)
            {
                InfoTicker.Init();
            }

            if (!InfoTicker.IsConfigLoaded && Motd.IsEnabled)
            {
                InfoTicker.Init();
            }
        }
    }
}