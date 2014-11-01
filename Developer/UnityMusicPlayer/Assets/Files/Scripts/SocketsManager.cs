using System;
using System.Net;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
//Written by Michael Bethke
public class SocketsManager : MonoBehaviour
{
	
	StartupManager startupManager;
	//List<string> messages = new List<string> ();
	string message = null;
	int port;
	
	
	void Start ()
	{
		startupManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent <StartupManager> ();
		
		port = startupManager.twoCatSettings.localPort;
		
		InvokeRepeating ( "SendUDPMessage", 0, 5 );
		PrepareUDPMessage ( "UMP is Running" );
	}
	
	
	internal void PrepareUDPMessage ( string messageToSend )
	{
		
		//messages.Add ( messageToSend );
		message = messageToSend;
	}
	

	internal void SendUDPMessage ()
	{
		
		//if ( messages.Count > 0 )
		if ( String.IsNullOrEmpty ( message ) == false )
		{
		
			IPHostEntry host = Dns.GetHostEntry ( Dns.GetHostName ());;
			string localIP = null;
			//int port = 35143;
	
			foreach ( IPAddress ip in host.AddressList )
			{
			
				if ( ip.AddressFamily == AddressFamily.InterNetwork )
				{
				
					localIP = ip.ToString ();
					break;
				}
			}
			
			
			if ( startupManager.developmentMode == true )
			{
			
				//UnityEngine.Debug.Log ( "There are [" + messages.Count + "] messages in the queue." );
				UnityEngine.Debug.Log ( "'" + message + "' will be sent on " + localIP + " via " + port + "." );
			}
		
		
			UdpClient udpClient = new UdpClient( localIP, port );
			Byte[] sendBytes = Encoding.Unicode.GetBytes ( "[2CatStudios:UMP]" + message /*messages[0]*/ );
			try
			{
			
		  		udpClient.Send ( sendBytes, sendBytes.Length );
				//messages.RemoveAt ( 0 );
			} catch ( Exception e )
			{
				
				UnityEngine.Debug.Log ( e.ToString ());
			}
		}
	}
}
