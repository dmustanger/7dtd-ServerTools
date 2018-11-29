using System.Collections.Generic;

namespace ServerTools
{
    public class Day7
    {
        public static bool IsEnabled = false;
        public static int Days_Until_Horde = 7;

        public static void GetInfo(ClientInfo _cInfo, bool _announce)
        {
            string _fps = GameManager.Instance.fps.Counter.ToString();
            int _playerCount = ConnectionManager.Instance.ClientCount();
            int _zombies = 0;
            int _animals = 0;
            int _bicycles = 0;
            int _miniBikes = 0;
            int _motorcycles = 0;
            int _4x4 = 0;
            int _gyros = 0;
            int _supplyCrates = 0;
            int _daysUntilHorde = Days_Until_Horde - GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()) % Days_Until_Horde;
            List<Entity> _entities = GameManager.Instance.World.Entities.list;
            foreach (Entity _e in _entities)
            {
                if (_e.IsClientControlled())
                {
                    continue;
                }
                EntityType _type = _e.entityType;
                if (_type == EntityType.Zombie && _e.IsAlive())
                {
                    _zombies = _zombies + 1;
                }
                else if (_type == EntityType.Animal && _e.IsAlive())
                {
                    _animals = _animals + 1;
                }
                else
                {
                    string _name = EntityClass.list[_e.entityClass].entityClassName;
                    if (_name == "vehicleBicycle")
                    {
                        _bicycles = _bicycles + 1;
                    }
                    else if (_name == "vehicleMinibike")
                    {
                        _miniBikes = _miniBikes + 1;
                    }
                    else if (_name == "vehicleMotorcycle")
                    {
                        _motorcycles = _motorcycles + 1;
                    }
                    else if (_name == "vehicle4x4Truck")
                    {
                        _4x4 = _4x4 + 1;
                    }
                    else if (_name == "vehicleGyrocopter")
                    {
                        _gyros = _gyros + 1;
                    }
                    else if (_name == "sc_General")
                    {
                        _supplyCrates = _supplyCrates + 1;
                    }
                }  
            }
            string _phrase300;
            string _phrase301;
            string _phrase302;
            string _phrase303;
            string _phrase304;
            string _phrase306;
            if (!Phrases.Dict.TryGetValue(300, out _phrase300))
            {
                _phrase300 = "Server FPS: {Fps}";
            }
            if (!Phrases.Dict.TryGetValue(301, out _phrase301))
            {
                _phrase301 = "Next horde night is in {DaysUntilHorde} days";
            }
            if (!Phrases.Dict.TryGetValue(302, out _phrase302))
            {
                _phrase302 = "Total Players:{Players} Total Zombies:{Zombies} Total Animals:{Animals}";
            }
            if (!Phrases.Dict.TryGetValue(303, out _phrase303))
            {
                _phrase303 = "Bicycles:{Bicycles} Minibikes:{Minibikes} Motorcycles:{Motorcycles} 4x4:{4x4} Gyros:{Gyros}";
            }
            if (!Phrases.Dict.TryGetValue(304, out _phrase304))
            {
                _phrase304 = "Total Supply Crates:{SupplyCrates}";
            }
            if (!Phrases.Dict.TryGetValue(306, out _phrase306))
            {
                _phrase306 = "Next horde night is today";
            }
            _phrase300 = _phrase300.Replace("{Fps}", _fps);
            _phrase301 = _phrase301.Replace("{DaysUntilHorde}", _daysUntilHorde.ToString());
            _phrase302 = _phrase302.Replace("{Players}", _playerCount.ToString());
            _phrase302 = _phrase302.Replace("{Zombies}", _zombies.ToString());
            _phrase302 = _phrase302.Replace("{Animals}", _animals.ToString());
            _phrase303 = _phrase303.Replace("{Bicycles}", _bicycles.ToString());
            _phrase303 = _phrase303.Replace("{Minibikes}", _miniBikes.ToString());
            _phrase303 = _phrase303.Replace("{Motorcycles}", _motorcycles.ToString());
            _phrase303 = _phrase303.Replace("{4x4}", _4x4.ToString());
            _phrase303 = _phrase303.Replace("{Gyros}", _gyros.ToString());
            _phrase304 = _phrase304.Replace("{SupplyCrates}", _supplyCrates.ToString());
            if (_announce)
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase300 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                if (_daysUntilHorde == Days_Until_Horde)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase306 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase301 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase302 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase303 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase304 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase300 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                if (_daysUntilHorde == Days_Until_Horde)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase306 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase301 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase302 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase303 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase304 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}