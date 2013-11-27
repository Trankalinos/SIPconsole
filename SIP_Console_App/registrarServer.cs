using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIP_Console_App
{
    class registrarServer
    {
        //Add any setup for this server here
        public registrarServer()
        {
        }
        //Recieve the message and then do something about it
        public void recieveMsg(String msgRecieved)
        {
            String clientIP;
            String branch;
            String from;
            String to;
            String tag;
            int callID;
            int cSeq;
            String sSeqType;
            String contact;
            int maxForwards;
            String userAgent;
            int expires;
            String contentLength;

           /* start = msgRecieved.IndexOf("sip:", 0);
            end = msgRecieved.IndexOf(" ", start);
            clientIP = msgRecieved.Substring(start + 4, end - (start + 4));
            Console.WriteLine(clientIP);*/
            branch = getHeaderData(msgRecieved, "branch=", "\r\n");

        }
        public void RegisterClient(String clientMsgToReg)
        {

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
    }
}
