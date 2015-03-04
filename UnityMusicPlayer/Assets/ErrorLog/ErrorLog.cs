using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//Written by Michael Bethke
[RequireComponent (typeof (ErrorUI))]
public class ErrorLog : MonoBehaviour
{
	
	StartupManager startupManager;
	ErrorUI userInterface;
	
	public bool startupOnlyWriteToScreen = true;
	public bool startupOnlyWriteToDisk = true;
	
	bool debugLogActive = false;
	bool writeLogToScreen = false;
	bool writeLogToDisk = false;
	string writePath = null;
	
	internal List<String> log = new List<String> ();
	

	void Start ()
	{
		
		startupManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent<StartupManager> ();
		userInterface = gameObject.GetComponent<ErrorUI>();
		
		writeLogToScreen = startupOnlyWriteToScreen;
		writeLogToDisk = startupOnlyWriteToDisk;
		writePath = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent<StartupManager> ().supportPath;
	
		if ( writeLogToScreen == true || writeLogToDisk == true )
		{
			
			debugLogActive = true;
		} else {
			
			debugLogActive = false;
		}
	
		if ( debugLogActive == true )
		{
			
			if ( writeLogToScreen == true )
			{
				
				userInterface.enabled = true;
			} else {
				
				userInterface.enabled = false;
			}
			
			log.Add ( "Error Log Active" );
			
			if ( writeLogToDisk == true )
			{
				
				string writingWarning = "Writing to disk";
				
				if ( String.IsNullOrEmpty ( writePath.Trim ()) == true )
				{
					
					writingWarning = writingWarning + ", no destination given! Writing to default directory, Desktop.";
					writePath = Environment.GetFolderPath ( Environment.SpecialFolder.Desktop );
				}
				
				writePath = writePath + Path.DirectorySeparatorChar + "ErrorLog" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + ".txt";
				
				//log.Add ( writingWarning );
			}
		} else {
			
			UnityEngine.Debug.LogWarning ( "ErrorLog disabled." );
		}
	}
	
	
	public void Post ( string logString )
	{
		
		if ( AddMessage ( logString + " (Manual)\n" ) == false )
		{
			
			UnityEngine.Debug.LogError ( "Unable to AddMessage from Post, " + logString );
		}
	}
	
	
	void OnEnable ()
	{
		
		Application.RegisterLogCallback ( HandleLog );
	}
	
	
	void OnDisable ()
	{
		
		Application.RegisterLogCallback ( null );
	}
	
	
	void HandleLog ( string logString, string stackTrace, LogType type )
	{
		
		UnityEngine.Debug.Log ( "MESSAGE" );
		
		if ( AddMessage ( logString + " (" + type + ") " + stackTrace + "\n" ) == false )
		{
			
			UnityEngine.Debug.LogError ( "Unable to AddMessage from CallBack, " + logString );
		}
	}
	
	
	bool AddMessage ( string messageToAdd )
	{
		
		try
		{
			
			messageToAdd = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " - " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "." + DateTime.Now.Millisecond + " || " + startupManager.runningVersion + " || " + messageToAdd;
			log.Add ( messageToAdd );
			
			if ( writeLogToScreen == true )
			{
				
				userInterface.debugScrollPosition.y = Mathf.Infinity;
			}
			
			if ( writeLogToDisk == true )
			{
				using ( StreamWriter streamWriter = File.AppendText ( writePath )) 
				{
					
					streamWriter.WriteLine ( messageToAdd );
				}
			}
		} catch ( Exception e ) {
			
			UnityEngine.Debug.LogError ( e );
			return false;
		}
		
		return true;
	}
}