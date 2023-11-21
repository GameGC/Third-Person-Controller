// This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class DotNetTcpTestClient : MonoBehaviour
{
	#region private members

	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	
	public Action<BinaryReader> packetReceived;
	#endregion


	private void OnEnable()
	{
		packetReceived += TestReadMessage;
	}

	private void OnDisable()
	{
		packetReceived -= TestReadMessage;
	}

	private void Start()
	{
		ConnectToTcpServer();
	}

	private void Update()
	{
		if (Keyboard.current.spaceKey.wasReleasedThisFrame)
		{
			TestSendMessage();
		}
	}

	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToTcpServer()
	{
		try
		{
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}

	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private async void ListenForData()
	{
		try
		{
			socketConnection = new TcpClient("localhost", 8052);
			Byte[] bytes = new Byte[1024];
			while (true)
			{
				if (socketConnection.Available > 0)
				{
					var reader = await BeginReadMessage();
					packetReceived.Invoke(reader);
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

	public async Task TestSendMessage()
	{
		var writer = await BeginSendMessage();
		writer.Write("hello world from client " +DateTime.Now);
		writer.Flush();
	}

	public void TestReadMessage(BinaryReader reader)
	{
		Debug.Log(reader.ReadString());
	}

	private async Task<BinaryWriter> BeginSendMessage()
	{
		while (socketConnection == null)
		{
			await Task.Delay(10);
		}

		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			return new BinaryWriter(stream, Encoding.ASCII);
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}

		return null;
	}

	private async Task<BinaryReader> BeginReadMessage()
	{
		while (!socketConnection.Connected)
		{
			await Task.Delay(10);
		}
		
		return new BinaryReader(socketConnection.GetStream(), Encoding.ASCII);
	}
}