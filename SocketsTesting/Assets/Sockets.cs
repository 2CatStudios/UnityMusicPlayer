using System;
using System.Net;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;
using System.Collections;

public class Sockets : MonoBehaviour
{
	
	
	string returnData = "";
	GUIStyle guistyle;
	public Font guifont;
	
	
	void Start ()
	{
		
		guistyle = new GUIStyle ();
		guistyle.font = guifont;
		guistyle.alignment = TextAnchor.MiddleCenter;
		
		Thread recieveThread = new Thread ( RecieveDatagram );
		recieveThread.Start();
	}
	
	
	void OnGUI ()
	{
		
		GUI.Label ( new Rect ( 0, 0, Screen.width, Screen.height ), returnData, guistyle );
	}
	
	
	void RecieveDatagram ()
	{
	
		UdpClient receivingUdpClient = new UdpClient ( 35143 );
		IPEndPoint RemoteIpEndPoint = new IPEndPoint ( IPAddress.Any, 0 );
		
		while ( true )
		{
		
			try {
			
				Byte[] receiveBytes = receivingUdpClient.Receive ( ref RemoteIpEndPoint ); 
				returnData = Encoding.Unicode.GetString ( receiveBytes );
				
				if ( returnData.Substring ( 0, 17 ) == "[2CatStudios:UMP]" )
				{
					
					UnityEngine.Debug.Log ( "Recieved '" + returnData.ToString () + "' This message was sent on " + RemoteIpEndPoint.Address.ToString() + " via the port " + RemoteIpEndPoint.Port.ToString ());
					returnData = returnData.Substring ( 17 );
				} else {
					
					UnityEngine.Debug.Log ( "Data was recieved, but it was not expected." );
				}
			}
			catch ( Exception e )
			{
		
				UnityEngine.Debug.Log ( e.ToString ());
				returnData = e.ToString ();
			}
		}
	}
}