﻿using System;
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
                    Console.WriteLine("data follows \n{0}\n\n", received_data);
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
                    ArrayList clientInfo = locationServer.StoreUser(registrarServer.recieveMsg(fullMsg));
                    sendRegisterResponse(clientInfo);
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
                case ("OPTIONS"):
                // do something

                default: return; // do nothing
            }
        }

        public String createResponseMsg(int code, String sipMsg, String via, String from, String to, String callID, 
            String callSeq, String contact, String contentLength, String contentType)
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
            String from = "";
            String via = "";
            String branch = "";
            String maxFwd = "";
            String tag = "";
            String callID = "";
            String callSeq = "";
            String contact = "";
            String contentType = "";
            String contentLength = "";
            String clientIPAddress = null;

            foreach (KeyValuePair<String, String> kvp in kvpList)
            {
                if (kvp.Key.Equals("To: ")) username = kvp.Value;
                else if (kvp.Key.Equals("ToAddress: ")) recipientAddress = kvp.Value;
                else if (kvp.Key.Equals("From: ")) from = kvp.Value;
                else if (kvp.Key.Equals("Via: ")) via = kvp.Value;
                else if (kvp.Key.Equals("branch=")) branch = kvp.Value;
                else if (kvp.Key.Equals("Max Forwards: ")) maxFwd = kvp.Value;
                else if (kvp.Key.Equals("tag=")) tag = kvp.Value;
                else if (kvp.Key.Equals("callID: ")) callID = kvp.Value;
                else if (kvp.Key.Equals("CSeq: ")) callSeq = kvp.Value;
                else if (kvp.Key.Equals("Contact: ")) contact = kvp.Value;
                else if (kvp.Key.Equals("Content-Type: ")) contentType = kvp.Value;
                else if (kvp.Key.Equals("Content-Length: ")) contentLength = kvp.Value;
                else if (kvp.Key.Equals("clientIP")) clientIPAddress = kvp.Value;
            }

            Boolean cont = registrarServer.userExists(username, recipientAddress);
            if (cont)
            {
                // forward the request to the recipient of the call
                try
                {
                    IPEndPoint addyR = new IPEndPoint(System.Net.IPAddress.Parse(recipientAddress), listenPort);
                    byte[] byteArray = Encoding.ASCII.GetBytes(msg);
                    // create response message
                    String response = createResponseMsg(
                        200,                                            // code
                        "OK",                                           // sipMsg
                        (via + ";" + branch),                           // via 
                        (username + " <" + recipientAddress + ">"),     // to
                        from,                                       // from
                        callID,                                         // callID
                        callSeq + " INVITE",                            // call sequence
                        contact,                                        // contact
                        contentType,                                    // content-type
                        contentLength                                   // content-length
                    );
                    listener.Send(byteArray, byteArray.Length, addyR);
                    sendRes(new IPEndPoint(System.Net.IPAddress.Parse(clientIP), listenPort), response);
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

        public void sendRes(IPEndPoint client, String response)
        {
            byte[] responseByteArray = Encoding.ASCII.GetBytes(response);
            listener.Send(responseByteArray, responseByteArray.Length, client);
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

            kvpList.Add(new KeyValuePair<String, String>("Request: ", request));
            kvpList.Add(new KeyValuePair<String, String>("Via: ", via));
            kvpList.Add(new KeyValuePair<String, String>("branch=", branch));
            kvpList.Add(new KeyValuePair<String, String>("Max-Forwards: ", maxForwards));
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
        public void sendRegisterResponse(ArrayList clientInfo)
        {
            String to = "";
            String from = "";
            String via = "";
            String branch = "";
            String maxFwd = "";
            String tag = "";
            String callID = "";
            String callSeq = "";
            String contact = "";
            String contentLength = "";
            String clientIPAddress = null;

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
                else if (kvp.Key.Equals("clientIP")) clientIPAddress = kvp.Value;
            }

         String response = "SIP/2.0 200 Registration sucessful\r\n"
                + "Via: SIP/2.0/UDP " + clientIPAddress + ";"
                + "rport=" + "5060" + ";received=" + from + ";"
                + "branch=" + branch + "\r\n"
                + "From: " + "<sip:" + from + ">;tag=" + tag + "\r\n"
                + "To: " + "<sip:" + to + ">;tag=" + tag + "\r\n"
                + "Call-ID: " + callID + "\r\n"
                + "CSeq: " + Convert.ToString(Convert.ToInt32(callSeq) + 1) + " REGISTER\r\n"
                + "Contact: <sip:" + to + ":" + "5060" + ">\r\n"
                + "Content-Length: " + contentLength + "\r\n"
                + "Content-Type: application/sdp\r\n"
                ;
         sendRes(new IPEndPoint(System.Net.IPAddress.Parse(clientIPAddress), listenPort), response);
        }
    }
}
