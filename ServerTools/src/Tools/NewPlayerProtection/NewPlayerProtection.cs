using System;

namespace ServerTools
{
    class NewPlayerProtection
    {
        public static bool IsEnabled = false;
        public static int Level = 5;

        public static void AddHealing(EntityAlive _entityAlive, DamageResponse _dmResponse)
        {
            EntityPlayer player = (EntityPlayer)_entityAlive;
            if (player != null && player.IsAlive() && player.Progression.Level <= Level && !player.Buffs.HasBuff("buffHealHealth"))
            {
                Entity entity = PersistentOperations.GetEntity(_dmResponse.Source.getEntityId());
                if (entity != null && (entity is EntityZombie || entity is EntityAnimal))
                {
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromEntityId(_entityAlive.entityId);
                    if (cInfo != null)
                    {
                        _entityAlive.SetCVar("medicalRegHealthAmount", _dmResponse.Strength);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("buffplayer {0} buffHealHealth", cInfo.CrossplatformId.CombinedString), null);
                    }
                }
            }
        }
    }
}
