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
        protected UdpClient listener;
        protected String clientIP;

        public proxyServer()
        {
            listener = new UdpClient(listenPort);
            registrarServer = new registrarServer();
            redirectServer = new redirectServer();
            locationServer = new locationServer();
        }
        
        //once the server has started this is the main function that recieves the messages and figures out which server gets it
        public void Start()
        {
            bool done = false;
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
                    clientIP = groupEP.ToString();
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
            // Figure out who to send it to, note we only need to figure out the first line of the message to send to 
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
                    ArrayList kvpList = new ArrayList();
                    kvpList = this.receiveInviteMsg(fullMsg);
                    this.fwdReq(kvpList, fullMsg);
                    break;
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

            message += protocol + " " + code + " " + sipMsg + "\r\n";
            message += "Via: " + protocol + " " + via + "\r\n";
            message += "To: " + to + "\r\n";
            message += "Call-ID: " + callID + "\r\n";
            message += "CSeq: " + callSeq + "\r\n";
            message += "Contact: " + contact + "\r\n";
            message += "Content-Type: " + contentType + "\r\n";
            message += "Content-Length: " + contentLength + "\r\n";
            
            return message;
        }

        // Index of
        // Substring

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

        public void fwdReq(ArrayList kvpList, String msg)
        {
            // send our stuff to Registrar to check if user exists
            // if so, forward the request to the user and return the message to client
            // else, do nothing and terminate the session and send an error message to client
            String username = "";
            String recipientAddress = "";
            foreach (KeyValuePair<String, String> kvp in kvpList)
            {
                if (kvp.Key.Equals("To: ")) username = kvp.Value;
                else if (kvp.Key.Equals("ToAddress: ")) recipientAddress = kvp.Value;
            }

            Boolean cont = registrarServer.userExists(username, recipientAddress);
            if (cont)
            {
                // forward the request to the recipient of the call
                try
                {
                    IPEndPoint addyR = new IPEndPoint(Convert.ToInt32(recipientAddress), listenPort);
                    byte[] byteArray = Encoding.ASCII.GetBytes(msg);
                    // create response message
                    // response = createResponseMsg(200, "OK", blah blah blah)
                    listener.Send(byteArray, byteArray.Length, addyR);
                    // listener.Send(newByte, newByte.Length, clientIP);
                    
                }
                catch (Exception e) 
                {
                    Console.WriteLine(e.ToString());
                }
                
            }
            else
            {
                // else, terminate the session.
            }

        }

        public ArrayList receiveInviteMsg(String msg)
        {
            ArrayList kvpList = new ArrayList();

            String request = "INVITE";
            String via = getHeaderData(msg, "Via: ", ";");
            String branch = getHeaderData(msg, "branch=", " ");
            String maxForwards = getHeaderData(msg, "Max-Forwards: ", "\r\n");
            String to = getHeaderData(msg, "To: ", " <");
            String toAddress = getHeaderData(msg, ("To: " + to +"<"), ">");
            String from = getHeaderData(msg, "From: ", ";");
            String tag = getHeaderData(msg, "tag=", "\r\n");
            String callID = getHeaderData(msg, "Call-ID: ", "\r\n");
            String callSeq = getHeaderData(msg, "CSeq: ", "INVITE");
            String contact = getHeaderData(msg, "Contact: ", "\r\n");
            String contentType = getHeaderData(msg, "Content-Type: ", "\r\n");
            String contentLength = getHeaderData(msg, "Content-Length: ", "\r\n");

            /*String[] headers = {"Request", "Via: ", "branch=", "Max-Forwards: ", "To: ", "From: ", 
                               "tag=", "Call-ID: ", "CSeq: ", "Contact: ", "Content-Type: ",
                               "Content-Length: "};
            */

            kvpList.Add(new KeyValuePair<String, String>("Request: ", request));
            kvpList.Add(new KeyValuePair<String, String>("Via: ", via));
            kvpList.Add(new KeyValuePair<String, String>("branch=", branch));
            kvpList.Add(new KeyValuePair<String, String>("Max-Forwards:", maxForwards));
            kvpList.Add(new KeyValuePair<String, String>("To: ", to));
            kvpList.Add(new KeyValuePair<String, String>("ToAddress: ", toAddress));
            kvpList.Add(new KeyValuePair<String, String>("From: ", from));
            kvpList.Add(new KeyValuePair<String, String>("tag=", tag));
            kvpList.Add(new KeyValuePair<String, String>("Call-ID: ", callID));
            kvpList.Add(new KeyValuePair<String, String>("CSeq: ", callSeq));
            kvpList.Add(new KeyValuePair<String, String>("Contact: ", contact));
            kvpList.Add(new KeyValuePair<String, String>("Content-Type: ", contentType));
            kvpList.Add(new KeyValuePair<String, String>("Content-Length: ", contentLength));

            return kvpList;
        }
    }
}
