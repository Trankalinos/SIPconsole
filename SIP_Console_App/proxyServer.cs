using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace SIP_Console_App
{
    class proxyServer
    {
        private locationServer locationServer;
        private redirectServer redirectServer;
        private registrarServer registrarServer;
        private const int listenPort = 5060;
        public proxyServer()
        {
            registrarServer = new registrarServer();
            redirectServer = new redirectServer();
            locationServer = new locationServer();
        }
        
        //once the server has started this is the main function that recieves the messages and figures out which server gets it
        public void Start()
        {
            bool done = false;
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            string received_data;
            byte[] receive_byte_array;
            try
            {
                Console.WriteLine("Server Listening");
                while (!done)
                {
                    receive_byte_array = listener.Receive(ref groupEP);
                    Console.WriteLine("Received a broadcast from {0}", groupEP.ToString());
                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                    //Console.WriteLine("data follows \n{0}\n\n", received_data);
                   sortMessage(received_data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            listener.Close();
        }

        public void sortMessage(String msgRecieved)
        {
            //Figure out who to send it to, note we only need to figure out the first line of the message to send to 
            // know who to send it to. Then we can just forward it and let the other server deal with the rest.
            // parse the first part of the meesage here and then call sipProtocol when you know the message
            int firstSpace = msgRecieved.IndexOf(" ");
            if (firstSpace > 0)
            {
                String sipMsg = msgRecieved.Substring(0, firstSpace);
                sipProtocol(sipMsg, msgRecieved);
            }
            else
            {
                return;
            }
        }
        public void sipProtocol(String sipMsg, String fullMsg)
        {
            // key, value pairs
            // SWITCH CASE STATEMENTS
                // INVITE, REGISTER
                // (OK) CANCEL BYE ACK
            switch (sipMsg)
            {
                case ("REGISTER"):
                    registrarServer.recieveMsg(fullMsg);
                    break;
                case ("INVITE"):
                    // do something
                    // This.forward(INVITE)
                case ("OK"):
                    // do something
                    // OK
                case ("CANCEL"):
                    // do something
                    // Cancel transaction
                case ("BYE"):
                    // do something
                    // This.connection(BYE);
                case ("ACK"):
                    // do something

                default: return; // do nothing
            }
        }

        public String createResponseMsg(int code, String sipMsg, String from, String to, String callID, String via, String callSeq, String contact, int contentLength, String contentType)
        {
            String protocol = "SIP/2.0/UDP";
            String message = "";

            message += protocol + " " + code + " " + sipMsg + "\n";
            message += "Via: " + protocol + " " + via + "\n";
            message += "To: " + to + "\n";
            message += "Call-ID: " + callID + "\n";
            message += "CSeq: " + callSeq + "\n";
            message += "Contact: " + contact + "\n";
            message += "Content-Type: " + contentType + "\n";
            message += "Content-Length: " + contentLength + "\n";
            
            return message;
        }

        // Index of
        // Substring

        // 
        public ArrayList processInviteMsg(String header, String msg)
        {
            KeyValuePair<String, String> kvp;
            String[] headers = {"INVITE", "Via:", "To:", "From:", "Call-ID:", "CSeq:", 
                                   "Contact:", "Content-Type:", "Content-Length:"};
            ArrayList myList = new ArrayList();
            String temp = "";
            
            int start = 0;
            int end = 0;
            start = msg.IndexOf(header, 0);
            end = msg.IndexOf("\r\n", start);
            temp = msg.Substring(start + 4, end - (start + 4));

            kvp.Key("INVITE");

            return myList;
        }

        public String getHeaderData(String msg, String header, String delimiter)
        {
            String temp = "";
            
            int start = 0;
            int end = 0;
            start = msg.IndexOf(header, 0);
            end = msg.IndexOf(delimiter, start);
            temp = msg.Substring(start + header.Length, end - (start + header.Length));
            
            return temp;
        }
    }
}
