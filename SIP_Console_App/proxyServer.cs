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
        private const String mytag = "DvF01HZK3K3QD";
        
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
        public void Start()
        {
            bool done = false;
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            string received_data;
            byte[] receive_byte_array;
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.WriteLine(ip.ToString());
                    break;
                }
            }
            try
            {
                Console.WriteLine("Server Listening");
                while (!done)
                {
                    receive_byte_array = listener.Receive(ref groupEP);
                    Console.WriteLine("Received a broadcast from {0}", groupEP.ToString());
                    clientIP = groupEP.ToString();
                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                    sortMessage(received_data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
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
            else {
                return;
            }
        }
        public void forwardResponseMsg(String msgRecieved, Boolean isResponse) {
            String from = null;
            String to = null;
            String recipientAddress = null;
            if (isResponse)
            {
                from = getHeaderData(msgRecieved, "From: <", ">");
                recipientAddress = getIPAddress(locationServer.getData(from, ":", "@"));
            }
            else
            {
                to = getHeaderData(msgRecieved, "To: <", ">");
                recipientAddress = getIPAddress(locationServer.getData(to, ":", "@"));
            }
            sendRes(new IPEndPoint(System.Net.IPAddress.Parse(recipientAddress), listenPort), msgRecieved);
        }
        public void sipProtocol(String sipMsg, String fullMsg)
        {
            switch (sipMsg)
            {
                case ("REGISTER"):
                    Console.WriteLine("data follows \n{0}\n\n", fullMsg);
                    ArrayList clientInfo = locationServer.StoreUser(registrarServer.recieveMsg(fullMsg));
                    sendRegisterResponse(clientInfo);
                    break;
                case ("INVITE"):
                    Console.WriteLine("data follows \n{0}\n\n", fullMsg);
                    forwardResponseMsg(fullMsg, false);
                    break;
                case ("OK"):
                    Console.WriteLine("data follows \n{0}\n\n", fullMsg);
                    forwardResponseMsg(fullMsg, true);
                    break;
                    // OK
                case ("CANCEL"):
                    Console.WriteLine("data follows \n{0}\n\n", fullMsg);
                    forwardResponseMsg(fullMsg, false);
                    break;
                case ("BYE"):
                    Console.WriteLine("data follows \n{0}\n\n", fullMsg);
                    forwardResponseMsg(fullMsg, false);
                    break;
                case ("ACK"):
                    Console.WriteLine("data follows \n{0}\n\n", fullMsg);
                    forwardResponseMsg(fullMsg, true);
                    break;
                case ("OPTIONS"):
                    Console.WriteLine("data follows \n{0}\n\n", fullMsg);
                    break;
                default:
                    Console.WriteLine("data follows \n{0}\n\n", fullMsg);
                    forwardResponseMsg(fullMsg, true); return; // do nothing
            }
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
        public void sendRes(IPEndPoint client, String response)
        {
            byte[] responseByteArray = Encoding.ASCII.GetBytes(response);
            listener.Send(responseByteArray, responseByteArray.Length, client);
        }
        public void sendRegisterResponse(ArrayList clientInfo)
        {
            String to = "";
            String from = "";
            String via = "";
            String branch = "";
            String tag = "";
            String callID = "";
            String callSeq = "";
            String contact = "";
            String expires = "";
            String contentLength = "";
            String clientIPAddress = "";

            foreach (KeyValuePair<String, String> kvp in clientInfo)
            {
                if (kvp.Key.Equals("to")) to = kvp.Value;
                else if (kvp.Key.Equals("from")) from = kvp.Value;
                else if (kvp.Key.Equals("via")) via = kvp.Value;
                else if (kvp.Key.Equals("branch")) branch = kvp.Value;
                else if (kvp.Key.Equals("tag")) tag = kvp.Value;
                else if (kvp.Key.Equals("callID")) callID = kvp.Value;
                else if (kvp.Key.Equals("cSeq")) callSeq = kvp.Value;
                else if (kvp.Key.Equals("contact")) contact = kvp.Value;
                else if (kvp.Key.Equals("contentLength")) contentLength = kvp.Value;
                else if (kvp.Key.Equals("expires")) expires = kvp.Value;
            }
            //clientIPAddress = contact.Substring(contact.IndexOf("@") + 1, (contact.IndexOf(":", contact.IndexOf("@")) - contact.IndexOf("@") - 1));
         clientIPAddress = contact.Substring(contact.IndexOf("@") + 1, (contact.IndexOf(";") - contact.IndexOf("@") - 1));
         String response = "SIP/2.0 200 Registration sucessful\r\n"
                + "Via: " + via + ";"
                + "rport=" + "5060" + ";received=" + clientIPAddress + ";"
                + "branch=" + branch + "\r\n"
                + "From: " + "<" + from + ">;tag=" + tag + "\r\n"
                + "To: " + "<" + to + ">;tag=" + mytag + "\r\n"
                + "Call-ID: " + callID + "\r\n"
                + "CSeq: " + callSeq + " REGISTER\r\n"
                + "Contact: <" + contact + ">\r\n"
                + "Expires: " + expires + "\r\n"
                + "Content-Length: " + contentLength + "\r\n\r\n"
                ;
         sendRes(new IPEndPoint(System.Net.IPAddress.Parse(clientIPAddress), listenPort), response);
        }
        public String getIPAddress(String userName)
        {
            String foundIPAddress = null;
            foundIPAddress = locationServer.getIPAddressFromDatabase(userName);
            return foundIPAddress;
        }
       
    }
}
