using System;

namespace One_Man_Army
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (One_Man_Army_Game game = new One_Man_Army_Game())
            {
                game.Run();
            }
        }
    }
}

