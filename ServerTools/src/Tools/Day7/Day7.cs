using System.Collections.Generic;

namespace ServerTools
{
    public class Day7
    {
        public static bool IsEnabled = false;
        public static string Command16 = "day7", Command17 = "day";

        public static void GetInfo(ClientInfo _cInfo)
        {
            string _fps = GameManager.Instance.fps.Counter.ToString();
            int _playerCount = ConnectionManager.Instance.ClientCount();
            int _zombies = 0, _animals = 0, _bicycles = 0, _miniBikes = 0, _motorcycles = 0, _4x4 = 0, _gyros = 0, _supplyCrates = 0;
            int _daysRemaining = DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()));
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
            }
            Response(_cInfo, _fps, _daysRemaining, _playerCount, _zombies, _animals, _bicycles, _miniBikes, _motorcycles, _4x4, _gyros, _supplyCrates);
        }

        public static void Response(ClientInfo _cInfo, string _fps, int _daysRemaining, int _playerCount, int _zombies, int _animals, int _bicycles, int _miniBikes, int _motorcycles, int _4x4, int _gyros, int _supplyCrates)
        {
            Phrases.Dict.TryGetValue(481, out string _phrase481);
            _phrase481 = _phrase481.Replace("{Value}", _fps);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase481 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            if (_daysRemaining == 0 && !SkyManager.BloodMoon())
            {
                Phrases.Dict.TryGetValue(487, out string _phrase487);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase487 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else if (SkyManager.BloodMoon())
            {
                Phrases.Dict.TryGetValue(486, out string _phrase486);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase486 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue(482, out string _phrase482);
                _phrase482 = _phrase482.Replace("{Value}", _daysRemaining.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase482 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            Phrases.Dict.TryGetValue(483, out string _phrase483);
            _phrase483 = _phrase483.Replace("{Players}", _playerCount.ToString());
            _phrase483 = _phrase483.Replace("{Zombies}", _zombies.ToString());
            _phrase483 = _phrase483.Replace("{Animals}", _animals.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase483 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            Phrases.Dict.TryGetValue(484, out string _phrase484);
            _phrase484 = _phrase484.Replace("{Bicycles}", _bicycles.ToString());
            _phrase484 = _phrase484.Replace("{Minibikes}", _miniBikes.ToString());
            _phrase484 = _phrase484.Replace("{Motorcycles}", _motorcycles.ToString());
            _phrase484 = _phrase484.Replace("{4x4}", _4x4.ToString());
            _phrase484 = _phrase484.Replace("{Gyros}", _gyros.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase484 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            Phrases.Dict.TryGetValue(485, out string _phrase485);
            _phrase485 = _phrase485.Replace("{Value}", _supplyCrates.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase485 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static int DaysRemaining(int _daysUntilHorde)
        {
            int _bloodmoonFrequency = GamePrefs.GetInt(EnumGamePrefs.BloodMoonFrequency);
            if (_daysUntilHorde <= _bloodmoonFrequency)
            {
                int _daysLeft = _bloodmoonFrequency - _daysUntilHorde;
                return _daysLeft;
            }
            else
            {
                int _daysLeft = _daysUntilHorde - _bloodmoonFrequency;
                return DaysRemaining(_daysLeft);
            }
        }
    }
}