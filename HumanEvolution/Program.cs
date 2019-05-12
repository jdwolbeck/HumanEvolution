using System;

namespace MGTemplate
{
#if WINDOWS || LINUX
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            //Josh Test
            using (var game = new Game1())
                game.Run();
        }
    }
#endif
}
