using System;
using System.Net;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

public class SocketsManager : MonoBehaviour
{
	
	StartupManager startupManager;
	List<string> messages = new List<string> ();
	
	
	void Start ()
	{
		startupManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent <StartupManager> ();
		
		InvokeRepeating ( "SendUDPMessage", 0, 5 );
	}
	
	
	internal void PrepareUDPMessage ( string messageToSend )
	{
		
		messages.Add ( messageToSend );
	}
	

	void SendUDPMessage ()
	{
		
		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( "There are: " + messages.Count + " messages in the queue." );
		
		if ( messages.Count > 0 )
		{
		
			IPHostEntry host = Dns.GetHostEntry ( Dns.GetHostName ());;
			string localIP = null;
	
			foreach ( IPAddress ip in host.AddressList )
			{
			
				if ( ip.AddressFamily == AddressFamily.InterNetwork )
				{
				
					localIP = ip.ToString ();
					break;
				}
			}
		
		
			UdpClient udpClient = new UdpClient( localIP, 11011 );
			Byte[] sendBytes = Encoding.ASCII.GetBytes ( "UMP" + messages[0] );
			try
			{
			
		  		udpClient.Send ( sendBytes, sendBytes.Length );
				messages.RemoveAt ( 0 );
			} catch ( Exception e )
			{
				
				UnityEngine.Debug.Log ( e.ToString ());
			}
		}
	}
}
