using System;

namespace ServerTools
{
    class NewPlayerProtection
    {
        public static bool IsEnabled = false;
        public static int Level = 5;

        public static bool IsProtected(EntityPlayer _victim)
        {
            if (_victim.Progression != null && _victim.Progression.Level <= Level)
            {
                return true;
            }
            return false;
        }
    }
}
