namespace Cab360Driver.EnumsConstants
{
    public class IntegerConstants
    {
        private static int UPDATE_INTERVAL = 5; //Seconds
        private static int FASTEST_INTERVAL = 5; //Seconds
        private static int DISPLACEMENT = 1; //METRES;

        public static int GetUpdateInterval()
        {
            return UPDATE_INTERVAL;
        }

        public static int GetFastestInterval()
        {
            return FASTEST_INTERVAL;
        }

        public static int GetDisplacement()
        {
            return DISPLACEMENT;
        }
    }
}