using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIP_Console_App
{
    class Program
    {
        //Main part of our program, it doesnt do anything other then start the server
        static void Main(string[] args)
        {
            proxyServer myServer = new proxyServer();
            myServer.Start();
        }
    }
}
