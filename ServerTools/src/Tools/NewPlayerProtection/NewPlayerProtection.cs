using System;

namespace ServerTools
{
    class NewPlayerProtection
    {
        public static bool IsEnabled = false;
        public static int Level = 5;

        public static void AddHealing(EntityAlive _entityAlive, DamageResponse _dmResponse)
        {
            EntityPlayer _player = (EntityPlayer)_entityAlive;
            if (_player != null && _player.IsAlive() && _player.Progression.Level < Level && !_player.Buffs.HasBuff("buffHealHealth"))
            {
                Entity _entity = PersistentOperations.GetEntity(_dmResponse.Source.getEntityId());
                if (_entity != null && (_entity is EntityZombie || _entity is EntityAnimal))
                {
                    ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_entityAlive.entityId);
                    if (_cInfo != null)
                    {
                        _entityAlive.SetCVar("medicalRegHealthAmount", _dmResponse.Strength);
                        SdtdConsole.Instance.ExecuteSync(string.Format("buffplayer {0} buffHealHealth", _cInfo.playerId), null);
                    }
                }
            }
        }
    }
}
