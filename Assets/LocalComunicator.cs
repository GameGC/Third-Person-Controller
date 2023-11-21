using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace DefaultNamespace
{
    // This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class LocalComunicator : MonoBehaviour
    {
        private TcpListener server;
        private TcpClient client;
        private NetworkStream ns;

        private Thread tcpListenerThread;
        public void Start()
        {
            tcpListenerThread = new Thread(new ThreadStart(Perform));
            tcpListenerThread.IsBackground = true; 		
            tcpListenerThread.Start(); 	
        }
        //we wait for a connection

        public void Perform()
        {
            server = new TcpListener(IPAddress.Any, 9999);  
            // we set our IP address as server's address, and we also set the port: 9999

            server.Start();  // this will start the server
            client = server.AcceptTcpClient();
           // while (true)   //we wait for a connection
            {
               //if a connection exists, the server will accept it

                NetworkStream ns = client.GetStream(); //networkstream is used to send/receive messages

                byte[] hello = new byte[100];   //any message must be serialized (converted to byte array)
                hello = Encoding.Default.GetBytes("hello world");  //conversion string => byte array

                ns.Write(hello, 0, hello.Length);     //sending the message

                while (client.Connected)  //while the client is connected, we look for incoming messages
                {
                    byte[] msg = new byte[1024];     //the messages arrive as byte array
                    ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client
                    this.msg = Encoding.Default.GetString(msg);
                }
            }
        }

        private string msg;
        public void Update()
        {
            Debug.Log(msg);
        }
    }
}