using System.Collections.Generic;

namespace ServerTools
{
    public class Day7
    {
        public static bool IsEnabled = false;
        public static string Command_day7 = "day7", Command_day = "day";

        public static void GetInfo(ClientInfo _cInfo)
        {
            string fps = GameManager.Instance.fps.Counter.ToString();
            int playerCount = ConnectionManager.Instance.ClientCount();
            int zombies = 0, animals = 0, bicycles = 0, miniBikes = 0, motorcycles = 0, fourByFour = 0, gyros = 0, supplyCrates = 0;
            int daysRemaining = DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()));
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
                    zombies++;
                }
                else if (_tags.Contains("animal") && _e.IsAlive())
                {
                    animals++;
                }
                else
                {
                    string _name = EntityClass.list[_e.entityClass].entityClassName;
                    if (_name == "vehicleBicycle")
                    {
                        bicycles++;
                    }
                    else if (_name == "vehicleMinibike")
                    {
                        miniBikes++;
                    }
                    else if (_name == "vehicleMotorcycle")
                    {
                        motorcycles++;
                    }
                    else if (_name == "vehicle4x4Truck")
                    {
                        fourByFour++;
                    }
                    else if (_name == "vehicleGyrocopter")
                    {
                        gyros++;
                    }
                    else if (_name == "sc_General")
                    {
                        supplyCrates++;
                    }
                }
            }
            Response(_cInfo, fps, daysRemaining, playerCount, zombies, animals, bicycles, miniBikes, motorcycles, fourByFour, gyros, supplyCrates);
        }

        public static void Response(ClientInfo _cInfo, string _fps, int _daysRemaining, int _playerCount, int _zombies, int _animals, int _bicycles, int _miniBikes, int _motorcycles, int _4x4, int _gyros, int _supplyCrates)
        {
            Phrases.Dict.TryGetValue("Day7_1", out string phrase);
            phrase = phrase.Replace("{Value}", _fps);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            if (_daysRemaining == 0 && !GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
            {
                Phrases.Dict.TryGetValue("Day7_7", out phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else if (GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
            {
                Phrases.Dict.TryGetValue("Day7_6", out phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("Day7_2", out phrase);
                phrase = phrase.Replace("{Value}", _daysRemaining.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color +phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            Phrases.Dict.TryGetValue("Day7_3", out phrase);
            phrase = phrase.Replace("{Players}", _playerCount.ToString());
            phrase = phrase.Replace("{Zombies}", _zombies.ToString());
            phrase = phrase.Replace("{Animals}", _animals.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            Phrases.Dict.TryGetValue("Day7_4", out phrase);
            phrase = phrase.Replace("{Bicycles}", _bicycles.ToString());
            phrase = phrase.Replace("{Minibikes}", _miniBikes.ToString());
            phrase = phrase.Replace("{Motorcycles}", _motorcycles.ToString());
            phrase = phrase.Replace("{4x4}", _4x4.ToString());
            phrase = phrase.Replace("{Gyros}", _gyros.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            Phrases.Dict.TryGetValue("Day7_5", out phrase);
            phrase = phrase.Replace("{Value}", _supplyCrates.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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