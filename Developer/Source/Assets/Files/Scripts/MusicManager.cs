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
	
	internal bool showMusicManager = false;
	MusicViewer musicViewer;
	
	string currentDirectory;
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
		
		currentDirectory = startupManager.lastDirectory;
		
		string[] tempAvailableSorts = Directory.GetDirectories ( startupManager.mediaPath );
		foreach ( string directoryName in tempAvailableSorts )
		{
			
			availableSorts.Add ( directoryName.Substring ( startupManager.mediaPath.Length ));
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

		if ( showMusicManager == true )
		{
			
			GUI.skin = guiskin;
			musicManagerPosition = GUI.Window ( 4, musicManagerPosition, MusicMakerPane, musicManagerTitle );
		}
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
				
				currentDirectory = startupManager.mediaPath + directoryName;
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
		
		GUILayout.Box ( "All songs in " + currentDirectory.Substring ( startupManager.mediaPath.Length ));
		GUILayout.Space ( 10 );
		
		if ( currentDirectoryFiles.Length > 0 )
		{
			
			for ( int i = 0; i < currentDirectoryFiles.Length; i += 1 )
			{
				
				GUILayout.Label ( currentDirectoryFiles[i].Substring ( currentDirectory.Length + 1 ));
			}
		} else {
			
			GUILayout.Label ( "This folder is empty!\n\nIf you want to add some music to this folder,\nclick the 'Open current directory' button bellow." +
				"\n\nThen, drop any .wav or .ogg files into the folder that will appear.\n\nTo listen to any of your music, navigate to the MusicViewer (press the right arrow key)." );
		}
		
		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		GUILayout.Space ( 20 );
		
		if ( GUILayout.Button ( "Set as active media directory" ))
			SetMusicViewerMedia ();
			
		if ( GUILayout.Button ( "Open current directory" ))
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
			musicViewer.clipList = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
			musicViewer.clipListEmpty = false;
		} else {
			
			Array.Clear( musicViewer.clipList, 0, musicViewer.clipList.Length);
			musicViewer.clipListEmpty = true;
		}
	}
}