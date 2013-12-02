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
        public ArrayList StoreUser(ArrayList clientInfo)
        {
            String userName = null; ;
            String userIPAddress = null;
            String expiry = null;
            String line = null;
            String uName = null;
            int lineCount = 0;
            int lineCountSaved = 0;
            Boolean usernameExists = false;
            ArrayList userList = new ArrayList();


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
          StreamReader file = new StreamReader(@"clients.txt");
          while ((line = file.ReadLine()) != null)
          {
              uName = line.Substring(0, line.IndexOf(",", 0));
              if (uName.Equals(userName))
              {
                  usernameExists = true;
                  lineCountSaved = lineCount;
              }
              lineCount++;
              userList.Add(line);
          }
          file.Close();
            using (StreamWriter wr = File.CreateText("clients.txt"))
            {
                if (!usernameExists)
                {
                    wr.WriteLine(userName + "," + userIPAddress + "," + expiry);
                }
                else
                {
                    userList.RemoveAt(lineCountSaved);
                    wr.Write("");
                    foreach (String a in userList)
                    {
                        wr.WriteLine(a);
                    }
                    wr.WriteLine(userName + "," + userIPAddress + "," + expiry);
                }
            }
            return clientInfo;
        }
    }
}
