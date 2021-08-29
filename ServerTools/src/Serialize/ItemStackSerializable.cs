using System;


namespace ServerTools
{
    [Serializable]

    public class ItemDataSerializable
    {
        public ItemDataSerializable()
        {
            this.name = "";
            this.count = 0;
            this.useTimes = 0f;
            this.quality = 0;
            this.modSlots = 0;
            this.cosmeticSlots = 0;
        }

        public string name;

        public int count;

        public float useTimes;

        public int quality;

        public int modSlots;

        public int cosmeticSlots;
    }
}
