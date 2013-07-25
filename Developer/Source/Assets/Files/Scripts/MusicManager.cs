using System;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
//Written by M. Gibson Bethke
//Thank you for my sanity, Jesus!
public class MusicManager : MonoBehaviour
{

	public GUISkin guiskin;
	StartupManager startupManager;
	PaneManager paneManager;
	MusicViewer musicViewer;
	
	internal string currentDirectory;
	string [] currentDirectoryDirectories;
	string [] currentDirectoryFiles;
	internal bool checkForChanges = false;
	
	Vector2 scrollPosition;

	internal Rect musicManagerPosition = new Rect (-1000, 0, 800, 600);
	internal string musicManagerTitle;
	
	List<string> availableSorts = new List<string>();
	

	void Start ()
	{

		startupManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent<StartupManager>();
		paneManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent<PaneManager>();
		musicViewer = GameObject.FindGameObjectWithTag ( "MusicViewer" ).GetComponent<MusicViewer>();
		
		musicManagerPosition.width = Screen.width;
		musicManagerPosition.height = Screen.height;
		musicManagerPosition.x = -musicManagerPosition.width + -musicManagerPosition.width / 4;
		
		currentDirectory = startupManager.mediaPath;
		
		string[] tempAvailableSorts = Directory.GetDirectories ( startupManager.path + Path.DirectorySeparatorChar + "Media" );
		foreach ( string directoryName in tempAvailableSorts )
		{
			
			availableSorts.Add ( directoryName.Substring (( startupManager.path + Path.DirectorySeparatorChar + "Media" ).Length + 1 ));
		}
		
		currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
		currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
		
		InvokeRepeating ( "UpdateDirectories", 0, 2 );
	}
	
	
	void UpdateDirectories ()
	{
		
		if ( paneManager.currentPane == PaneManager.pane.musicManager )
		{
			
			currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
			currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
		}
	}
	

	void OnGUI ()
	{

		GUI.skin = guiskin;
		musicManagerPosition = GUI.Window ( 4, musicManagerPosition, MusicMakerPane, musicManagerTitle );
	}
	

	void MusicMakerPane ( int wid )
	{

		GUILayout.Space ( musicManagerPosition.height / 6);	

		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		
		foreach ( string directoryName in availableSorts )
		{
			
			if ( GUILayout.Button ( directoryName ))
			{
				
				currentDirectory = startupManager.path + Path.DirectorySeparatorChar + "Media" + Path.DirectorySeparatorChar + directoryName;
				currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
				currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
			}
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal ();
		GUILayout.Space ( musicManagerPosition.width / 2 - 300 );
		GUILayout.BeginVertical ();
		
		scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  musicManagerPosition.height - ( musicManagerPosition.height / 6 + 56 )));
		
		for ( int i = 0; i < currentDirectoryDirectories.Length; i += 1 )
		{
			
			if ( GUILayout.Button ( currentDirectoryDirectories[i].Substring ( currentDirectory.Length + 1 ) ))
			{
				
				currentDirectory = currentDirectoryDirectories[i];
				currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
				currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
			}
		}
		
		GUILayout.Space ( 20 );
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		
		if ( currentDirectoryFiles.Length > 0 )
		{
			
			GUILayout.Box ( "Songs in current directory" );
			GUILayout.Space ( 10 );
			for ( int i = 0; i < currentDirectoryFiles.Length; i += 1 )
			{
				
				GUILayout.Label ( currentDirectoryFiles[i].Substring ( currentDirectory.Length + 1 ));
			}
		} else if ( currentDirectoryDirectories.Length == 0 )
		{
			
			GUILayout.Label ( "This folder is empty" );
		}
		
		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		GUILayout.Space ( 20 );
		
		if ( GUILayout.Button ( "Set as active media directory" ))
			SetMusicViewerMedia ();
			
		if ( GUILayout.Button ( "Open directory in default file manager" ))
			Process.Start ( currentDirectory );
			
		GUILayout.EndScrollView ();
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
	}
	
	
	void SetMusicViewerMedia ()
	{
		
		musicViewer.mediaPath = currentDirectory;
		if ( currentDirectoryFiles.Length > 0 )
		{
			musicViewer.clipList = Directory.GetFiles ( startupManager.mediaPath, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
			musicViewer.clipListEmpty = false;
		} else {
			
			Array.Clear( musicViewer.clipList, 0, musicViewer.clipList.Length);
			musicViewer.clipListEmpty = true;
		}
	}
}