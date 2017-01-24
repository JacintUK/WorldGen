using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloTK
{
    class Program
    {
        static void Main(string[] args)
        {
            Game window = new Game(800, 600);
            window.Run(30.0);
        }
    }
}
