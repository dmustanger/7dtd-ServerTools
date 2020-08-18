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
        }

        public ItemDataSerializable(string _name, int _count, int _useTimes, int _quality)
        {
            this.name = _name;
            this.count = _count;
            this.useTimes = _useTimes;
            this.quality = _quality;
        }

        public string name;

        public int count;

        public float useTimes;

        public int quality;
    }
}
