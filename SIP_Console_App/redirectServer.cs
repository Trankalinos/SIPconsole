using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIP_Console_App
{
    class redirectServer
    {
        // Add any setup for this server here
        public redirectServer()
        {
        }
        // Recieve the message and then do something about it

        /* For this assignment, since the capacity is only required to support 1-to-1,
         * the need for a redirect server is purely optional. In which case, we will
         * provide some pseudo code to reflect want we want to do.
         */
        public String recieveMsg(String msgRecieved)
        {
            // if the server is too busy
            // redirect the call to another server.
            String newServer = "Via: " + "a new SIP server";
            return newServer;
        }
    }
}
