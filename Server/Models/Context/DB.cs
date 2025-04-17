namespace Server.Models.Context
{
    internal class DB
    {
        
        static ElectronicDiaryContext? instance;
        public static ElectronicDiaryContext Instance
        {
            get
            {
                if (instance == null)
                    instance = new ElectronicDiaryContext();
                return instance;
            }
        }
        
    }
}
