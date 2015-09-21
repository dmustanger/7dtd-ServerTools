using System;

namespace ServerTools
{
    public class time
    {
        public static int GetMinutes(DateTime _datetime)
        {
            TimeSpan varTime = (DateTime)DateTime.Now - (DateTime)_datetime;
            double fractionalMinutes = varTime.TotalMinutes;
            int wholeMinutes = (int)fractionalMinutes;
            return wholeMinutes;
        }
    }
}