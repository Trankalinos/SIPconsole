using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SIP_Console_App
{
    class locationServer
    {
        //Add any setup for this server here
        public locationServer()
        {
        }
        //Recieve the message and then do something about it
        public void recieveMsg(String msgRecieved)
        {
          
        }
        public String getData(String msgRecieved, String headertoFind, String delimitedtoStopAt)
        {
            int start = 0;
            int end = 0;
            String returnString;
            start = msgRecieved.IndexOf(headertoFind, 0);
            end = msgRecieved.IndexOf(delimitedtoStopAt, start);
            returnString = msgRecieved.Substring(start + headertoFind.Length, end - (start + headertoFind.Length));
            return returnString;
        }
        public void StoreUser(ArrayList clientInfo)
        {
            String userName = null; ;
            String userIPAddress = null;
            String expiry = null;



            foreach(KeyValuePair<String, String> dt in clientInfo) {
                if (dt.Key == "clientIP")
                {
                    userIPAddress = dt.Value;
                }
                else if (dt.Key == "expires")
                {
                    expiry = dt.Value;
                } else if (dt.Key == "to") {
                    userName = getData(dt.Value, ":", "@");
                }
            }
            using (StreamWriter wr = File.AppendText("clients.xml"))
            {
                wr.WriteLine(userName + "," + userIPAddress + "," + expiry);
            }
        }
    }
}
