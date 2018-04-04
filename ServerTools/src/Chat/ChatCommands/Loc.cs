
using UnityEngine;

namespace ServerTools
{
    class Loc

    {
        public static bool IsEnabled = false;

        public static void Exec(ClientInfo _cInfo)
        {
            Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Vector3 _position = _player.position;
            int _x = (int)_position.x;
            int _y = (int)_position.y;
            int _z = (int)_position.z;
            string _phrase760;
            if (!Phrases.Dict.TryGetValue(760, out _phrase760))
            {
                _phrase760 = "Your current position is X  {X}, Y  {Y}, Z  {Z}.";
            }
            _phrase760 = _phrase760.Replace("{X}", _x.ToString());
            _phrase760 = _phrase760.Replace("{Y}", _y.ToString());
            _phrase760 = _phrase760.Replace("{Z}", _z.ToString());
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase760), Config.Server_Response_Name, false, "ServerTools", false));
        }
    }
}
