using System.Collections.Generic;

namespace ServerTools
{
    public class Day7
    {
        public static bool IsEnabled = false;
        public static string Command_day7 = "day7", Command_day = "day";

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
            Phrases.Dict.TryGetValue("Day7_1", out string _phrase);
            _phrase = _phrase.Replace("{Value}", _fps);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            if (_daysRemaining == 0 && !SkyManager.BloodMoon())
            {
                Phrases.Dict.TryGetValue("Day7_7", out _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else if (SkyManager.BloodMoon())
            {
                Phrases.Dict.TryGetValue("Day7_6", out _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("Day7_2", out _phrase);
                _phrase = _phrase.Replace("{Value}", _daysRemaining.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            Phrases.Dict.TryGetValue("Day7_3", out _phrase);
            _phrase = _phrase.Replace("{Players}", _playerCount.ToString());
            _phrase = _phrase.Replace("{Zombies}", _zombies.ToString());
            _phrase = _phrase.Replace("{Animals}", _animals.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            Phrases.Dict.TryGetValue("Day7_4", out _phrase);
            _phrase = _phrase.Replace("{Bicycles}", _bicycles.ToString());
            _phrase = _phrase.Replace("{Minibikes}", _miniBikes.ToString());
            _phrase = _phrase.Replace("{Motorcycles}", _motorcycles.ToString());
            _phrase = _phrase.Replace("{4x4}", _4x4.ToString());
            _phrase = _phrase.Replace("{Gyros}", _gyros.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            Phrases.Dict.TryGetValue("Day7_5", out _phrase);
            _phrase = _phrase.Replace("{Value}", _supplyCrates.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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