using System;
using System.Collections.Generic;

namespace ServerTools
{
    class VaultConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable Vault.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-vault off\n" +
                   "  2. st-vault on\n" +
                   "  3. st-vault increase <EOS/EntityId/PlayerName>\n" +
                   "  4. st-vault decrease <EOS/EntityId/PlayerName>\n" +
                   "  5. st-vault max <EOS/EntityId/PlayerName>\n" +
                   "  6. st-vault clear <EOS/EntityId/PlayerName>\n" +
                   "1. Turn off Vault\n" +
                   "2. Turn on Vault\n" +
                   "3. Increase the Vault size for a specific player\n" +
                   "4. Decrease the Vault size for a specific player\n" +
                   "5. Increase or decrease the Vault size for a specific player to the maximum slot count of 48\n" +
                   "6. Clears all items from the specific player's vault\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Vault", "vault" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower().Equals("off"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (Vault.IsEnabled)
                    {
                        Vault.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Vault has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Vault is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (!Vault.IsEnabled)
                    {
                        Vault.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Vault has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Vault is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("increase"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    ClientInfo cInfo = GeneralFunction.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        int size = PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize;
                        if (size < 8)
                        {
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize += 1;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' has been increased to '{3}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' is set to the maximum size of 8", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            return;
                        }
                    }
                    else if (_params[1].Contains("_") && PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        int size = PersistentContainer.Instance.Players[_params[1]].VaultSize;
                        if (size < 8)
                        {
                            PersistentContainer.Instance.Players[_params[1]].VaultSize += 1;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' has been increased to '{3}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].PlayerName, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' is set to the maximum size of 8", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].PlayerName));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("decrease"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    ClientInfo cInfo = GeneralFunction.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        int size = PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize;
                        if (size > 1)
                        {
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize -= 1;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' has been decreased to '{3}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' is set to the minimum size of 1", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            return;
                        }
                    }
                    else if (_params[1].Contains("_") && PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        int size = PersistentContainer.Instance.Players[_params[1]].VaultSize;
                        if (size > 1)
                        {
                            PersistentContainer.Instance.Players[_params[1]].VaultSize -= 1;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' has been decreased to '{3}'", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].PlayerName, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' is set to the minimum size of 1", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].PlayerName));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("max"))
                {
                    ClientInfo cInfo = GeneralFunction.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        int size = PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize;
                        if (size != 48)
                        {
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize = 48;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' has been increased to 48", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' is set to the maximum size of 48", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            return;
                        }
                    }
                    else if (_params[1].Contains("_") && PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        int size = PersistentContainer.Instance.Players[_params[1]].VaultSize;
                        if (size != 48)
                        {
                            PersistentContainer.Instance.Players[_params[1]].VaultSize = 48;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' has been increased to 48", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].PlayerName));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' is already set to the maximum size of 48", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].PlayerName));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("clear"))
                {
                    ClientInfo cInfo = GeneralFunction.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Vault = new ItemDataSerializable[48];
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' has been cleared out", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                        return;
                    }
                    else if (_params[1].Contains("_") && PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Vault = new ItemDataSerializable[48];
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' has been cleared out", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].PlayerName));
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VaultConsole.Execute: {0}", e.Message));
            }
        }
    }
}