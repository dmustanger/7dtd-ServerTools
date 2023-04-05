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
                   "  3. st-vault set <EOS/EntityId/PlayerName> <slots> <lines>\n" +
                   "  4. st-vault reset <EOS/EntityId/PlayerName>\n" +
                   "  5. st-vault max <EOS/EntityId/PlayerName>\n" +
                   "  6. st-vault clear <EOS/EntityId/PlayerName>\n" +
                   "1. Turn off Vault\n" +
                   "2. Turn on Vault\n" +
                   "3. Set the Vault size for a specific player to an exact number of slots and lines\n" +
                   "4. Reset the Vault size for a specific player to the default slots and lines from the main config\n" +
                   "5. Set the Vault size for a specific player to the maximum size of 8 slots and 6 lines\n" +
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
                else if (_params[0].ToLower().Equals("set"))
                {
                    if (_params.Count != 4)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 4, found '{0}'", _params.Count));
                        return;
                    }
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        int[] vaultSize = PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize;
                        if (vaultSize != null && vaultSize.Length == 2 && vaultSize[0] > 0 && vaultSize[1] > 0)
                        {
                            if (!int.TryParse(_params[2], out int slots))
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer provided for slots. '{0}'", _params[2]));
                                return;
                            }
                            if (!int.TryParse(_params[3], out int lines))
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer provided for lines. '{0}'", _params[3]));
                                return;
                            }
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize = new int[] { slots, lines };
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' has been set to '{3}' slots '{4}' lines", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, slots, lines));
                            PersistentContainer.DataChange = true;
                            return;
                        }
                    }
                    else if (_params[1].Contains("_") && PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        int[] vaultSize = PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize;
                        if (vaultSize != null && vaultSize.Length == 2 && vaultSize[0] > 0 && vaultSize[1] > 0)
                        {
                            if (!int.TryParse(_params[2], out int slots))
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer provided for slots. '{0}'", _params[2]));
                                return;
                            }
                            if (!int.TryParse(_params[3], out int lines))
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer provided for lines. '{0}'", _params[3]));
                                return;
                            }
                            PersistentContainer.Instance.Players[_params[1]].VaultSize = new int[] { slots, lines };
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' named '{1}' has been set to '{2}' slots '{3}' lines", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName, slots, lines));
                            PersistentContainer.DataChange = true;
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize = new int[] { Vault.Slots, Vault.Lines };
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' has been reset to '{3}' slots '{4}' lines", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName, Vault.Slots, Vault.Lines));
                        PersistentContainer.DataChange = true;
                        return;
                    }
                    else if (_params[1].Contains("_") && PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        PersistentContainer.Instance.Players[_params[1]].VaultSize = new int[] { Vault.Slots, Vault.Lines };
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' named '{1}' has been reset to '{2}' slots '{3}' lines", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName, Vault.Slots, Vault.Lines));
                        PersistentContainer.DataChange = true;
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("max"))
                {
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize = new int[] { 8, 6 };
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' has been set to the maximum size. 8 x 6", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                        PersistentContainer.DataChange = true;
                        return;
                    }
                    else if (_params[1].Contains("_") && PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        PersistentContainer.Instance.Players[_params[1]].VaultSize = new int[] { 8, 6 };
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' named '{1}' has been set to the maximum size. 8 x 6", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName));
                        PersistentContainer.DataChange = true;
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("clear"))
                {
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Vault = new ItemDataSerializable[48];
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' '{1}' named '{2}' has been cleared out", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                    }
                    else if (_params[1].Contains("_") && PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        PersistentContainer.Instance.Players[_params[1]].Vault = new ItemDataSerializable[48];
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The vault for '{0}' named '{1}' has been cleared out", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName));
                    }
                    PersistentContainer.DataChange = true;
                    return;
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