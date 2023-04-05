using UnityEngine;

namespace ServerTools
{
    class Confetti
    {
        public static bool IsEnabled = false, Player = false, Zombie = false;

        public static void Exec(EntityAlive _entity)
        {
            float lightBrightness = GameManager.Instance.World.GetLightBrightness(new Vector3i(_entity.position));
            GameManager.Instance.SpawnParticleEffectServer(new ParticleEffect("confetti", _entity.position, lightBrightness, Color.white, null, null, false), _entity.entityId);
            GameManager.Instance.PlaySoundAtPositionServer(_entity.position, "twitch_celebrate", (AudioRolloffMode)1, 5);
        }
    }
}
