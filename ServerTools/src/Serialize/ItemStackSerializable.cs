using System;


namespace ServerTools
{
    [Serializable]

    public class ItemDataSerializable
    {
        public ItemDataSerializable()
        {
            this.name = "";
            this.iconName = "";
            this.count = 0;
            this.useTimes = 0f;
            this.maxUseTimes = 0;
            this.quality = 0;
            this.price = 0;
            this.modSlots = 0;
            this.cosmeticSlots = 0;
            this.seed = 0;
            this.hasQuality = false;
        }

        public string name;

        public string iconName;

        public int count;

        public float useTimes;

        public int maxUseTimes;

        public int quality;

        public int price;

        public int modSlots;

        public int cosmeticSlots;

        public ushort seed;

        public bool hasQuality;
    }
}
