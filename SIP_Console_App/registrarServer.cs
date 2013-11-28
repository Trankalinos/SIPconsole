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
    class registrarServer
    {
        //Add any setup for this server here
        public registrarServer()
        {
        }
        //Recieve the message and then do something about it
        public ArrayList recieveMsg(String msgRecieved)
        {
            String clientIP;
            String branch;
            String from;
            String to;
            String tag;
            String callID;
            String cSeq;
            String cSeqType = "REGISTER";
            String contact;
            String maxForwards;
            String userAgent;
            String expires;
            String contentLength;
            ArrayList myList = new ArrayList();

            clientIP = getHeaderData(msgRecieved, "sip:", " ");
            branch = getHeaderData(msgRecieved, "branch=", "\r\n");
            from = getHeaderData(msgRecieved, "From: <", ">");
            to = getHeaderData(msgRecieved, "To: <", ">");
            tag = getHeaderData(msgRecieved, "tag=", "\r\n");
            callID = getHeaderData(msgRecieved, "Call-ID: ", "\r\n");
            cSeq = getHeaderData(msgRecieved, "CSeq: ", " REGISTER");
            contact = getHeaderData(msgRecieved, "Contact: <", ">");
            maxForwards = getHeaderData(msgRecieved, "Max-Forwards: ", "\r\n");
            userAgent = getHeaderData(msgRecieved, "User-Agent: ", "\r\n");
            expires = getHeaderData(msgRecieved, "Expires: ", "\r\n");
            contentLength = getHeaderData(msgRecieved, "Content-Length: ", "\r\n");

            myList.Add(new KeyValuePair<String, String>("clientIP", clientIP));
            myList.Add(new KeyValuePair<String, String>("branch", branch));
            myList.Add(new KeyValuePair<String, String>("from", from));
            myList.Add(new KeyValuePair<String, String>("to", to));
            myList.Add(new KeyValuePair<String, String>("tag", tag));
            myList.Add(new KeyValuePair<String, String>("callID", callID));
            myList.Add(new KeyValuePair<String, String>("cSeq", cSeq));
            myList.Add(new KeyValuePair<String, String>("cSeqType", cSeqType));
            myList.Add(new KeyValuePair<String, String>("contact", contact));
            myList.Add(new KeyValuePair<String, String>("maxForwards", maxForwards));
            myList.Add(new KeyValuePair<String, String>("userAgent", userAgent));
            myList.Add(new KeyValuePair<String, String>("expires", expires));
            myList.Add(new KeyValuePair<String, String>("contentLength", contentLength));

            return myList;
        }
        public String getHeaderData(String msgRecieved, String headertoFind, String delimitedtoStopAt)
        {
            int start = 0;
            int end = 0;
            String returnString;
            start = msgRecieved.IndexOf(headertoFind, 0);
            end = msgRecieved.IndexOf(delimitedtoStopAt, start);
            returnString = msgRecieved.Substring(start + headertoFind.Length, end - (start + headertoFind.Length));
            return returnString;
        }

        public Boolean userExists(String username, String address)
        {
            // check the location server
            // if they exist, return true, else return false
            return true;
        }
    }
}
