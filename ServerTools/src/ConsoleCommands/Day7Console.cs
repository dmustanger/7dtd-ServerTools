using System;
using System.Collections.Generic;

namespace ServerTools
{
    class Day7Console : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools]- Enable or disable day7.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-d7 off\n" +
                   "  2. st-d7 on\n" +
                   "  3. st-d7\n" +
                   "1. Turn off day 7 alert\n" +
                   "2. Turn on day 7 alert\n" +
                   "3. Shows the current server fps, player, zombie, animal, days remaining before horde night, vehicle and supply crate totals\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-Day7", "d7", "st-d7" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 0 && _params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 0 or 1, found '{0}'", _params.Count));
                    return;
                }
                if (_params.Count == 0)
                {
                    int _zombies = 0, _animals = 0, _bicycles = 0, _miniBikes = 0, _motorcycles = 0, _4x4 = 0, _gyros = 0, _supplyCrates = 0;
                    int _daysRemaining = Day7.DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()));
                    List<Entity> _entities = GameManager.Instance.World.Entities.list;
                    foreach (Entity _e in _entities)
                    {
                        if (_e.IsClientControlled())
                        {
                            continue;
                        }
                        string _tags = _e.EntityClass.Tags.ToString();
                        if (_tags.Contains("zombie") && _e.IsAlive())
                        {
                            _zombies++;
                        }
                        else if (_tags.Contains("animal") && _e.IsAlive())
                        {
                            _animals++;
                        }
                        else
                        {
                            string _name = EntityClass.list[_e.entityClass].entityClassName;
                            if (_name == "vehicleBicycle")
                            {
                                _bicycles++;
                            }
                            else if (_name == "vehicleMinibike")
                            {
                                _miniBikes++;
                            }
                            else if (_name == "vehicleMotorcycle")
                            {
                                _motorcycles++;
                            }
                            else if (_name == "vehicle4x4Truck")
                            {
                                _4x4++;
                            }
                            else if (_name == "vehicleGyrocopter")
                            {
                                _gyros++;
                            }
                            else if (_name == "sc_General")
                            {
                                _supplyCrates++;
                            }
                        }
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Server FPS '{0}'", GameManager.Instance.fps.Counter));
                        if (_daysRemaining == 0 && !GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
                        {
                            SdtdConsole.Instance.Output("Next horde night is today");
                        }
                        else if (GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
                        {
                            SdtdConsole.Instance.Output("The horde is here!");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Next horde night in '{0}' days", _daysRemaining));
                        }
                        SdtdConsole.Instance.Output(string.Format("Players '{0}' Zombies '{1}' Animals '{2}'", ConnectionManager.Instance.ClientCount(), _zombies, _animals));
                        SdtdConsole.Instance.Output(string.Format("Bicycles '{0}' Minibikes '{1}' Motorcycles '{2}' 4x4 '{3}' Gyros '{4}'", _bicycles, _miniBikes, _motorcycles, _4x4, _gyros));
                        SdtdConsole.Instance.Output(string.Format("Supply crates '{0}'", _supplyCrates));
                    }
                }
                else
                {
                    if (_params[0].ToLower().Equals("off"))
                    {
                        if (Day7.IsEnabled)
                        {
                            Day7.IsEnabled = false;
                            Config.WriteXml();
                            Config.LoadXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Day7 has been set to off"));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Day7 is already off"));
                            return;
                        }
                    }
                    else if (_params[0].ToLower().Equals("on"))
                    {
                        if (!Day7.IsEnabled)
                        {
                            Day7.IsEnabled = true;
                            Config.WriteXml();
                            Config.LoadXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Day7 has been set to on"));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Day7 is already on"));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Day7Console.Execute: {0}", e.Message));
            }
        }
    }
}