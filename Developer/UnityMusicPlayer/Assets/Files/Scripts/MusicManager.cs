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

	public GUISkin guiSkin;
	public Texture2D guiHover;
	StartupManager startupManager;
	PaneManager paneManager;
	
	internal bool showMusicManager = false;
	MusicViewer musicViewer;
	
	string currentDirectory;
	string [] currentDirectoryDirectories;
	string [] currentDirectoryFiles;
	internal bool checkForChanges = false;
	
	Vector2 scrollPosition;
	
	bool showNewFolderWindow = false;
	string folderName = "";

	Rect newFolderWindowRect = new Rect ( 0, 0, 350, 70 );
	internal Rect musicManagerPosition = new Rect (-1000, 0, 800, 600);
	internal string musicManagerTitle;
	
	List<string> availableSorts = new List<string>();
	
	string[] artworkImageLocations;
	
	bool startup;
	

	void Start ()
	{
		
		startup = true;

		startupManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent<StartupManager>();
		paneManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent<PaneManager>();
		musicViewer = GameObject.FindGameObjectWithTag ( "MusicViewer" ).GetComponent<MusicViewer>();
		
		musicManagerPosition.width = Screen.width;
		musicManagerPosition.height = Screen.height;
		musicManagerPosition.x = -musicManagerPosition.width + -musicManagerPosition.width / 4;
		
		newFolderWindowRect.x = musicManagerPosition.width/2 - newFolderWindowRect.width/2;
		newFolderWindowRect.y = musicManagerPosition.height/2 - newFolderWindowRect.height/2;
		
		currentDirectory = startupManager.lastDirectory;
		
		string[] tempAvailableSorts = Directory.GetDirectories ( startupManager.mediaPath );
		foreach ( string directoryName in tempAvailableSorts )
		{
			
			availableSorts.Add ( directoryName.Substring ( startupManager.mediaPath.Length ));
		}
		
		currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
		currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
		
		StartCoroutine ( "SetArtwork" );
		InvokeRepeating ( "UpdateDirectories", 0, 2 );
	}
	
	
	IEnumerator SetArtwork ()
	{
		
		if ( musicViewer.showArtwork == true )
		{
			
			artworkImageLocations = Directory.GetFiles ( currentDirectory, "Artwork.*" ).Where ( s => s.EndsWith ( ".png" ) || s.EndsWith ( ".jpg" ) || s.EndsWith ( ".jpeg" )).ToArray ();
			
			if ( artworkImageLocations.Length > 0 )
			{
	
				WWW wWw = new WWW ( "file://" + artworkImageLocations [ 0 ] );
				yield return wWw;
				
				musicViewer.currentSlideshowImage.texture = wWw.texture;
			} else {
				
				if ( startup == false )
					musicViewer.currentSlideshowImage.texture = null;
			}
			
			startup = false;
		}
	}
	
	
	void UpdateDirectories ()
	{
		
		if ( paneManager.currentPane == PaneManager.pane.musicManager )
		{
			
			try
			{
				
				currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
				currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
		
			} catch ( Exception e ) {
				
				if ( startupManager.developmentMode == true )
					UnityEngine.Debug.LogWarning ( e );
					
				currentDirectory = startupManager.mediaPath + "Albums";
				
				currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
				currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
			}
		}
		
		string[] tempAvailableSorts = Directory.GetDirectories ( startupManager.mediaPath );
		availableSorts.Clear ();
		foreach ( string directoryName in tempAvailableSorts )
		{
			
			availableSorts.Add ( directoryName.Substring ( startupManager.mediaPath.Length ));
		}
	}
	

	void OnGUI ()
	{
		
		if ( showNewFolderWindow == true )
		{
		
			GUI.Window ( 7, newFolderWindowRect, NewFolderWindow, "New Folder" );
		}
		
		if ( showMusicManager == true )
		{
			
			GUI.skin = guiSkin;
			
			if ( showNewFolderWindow == true )
				GUI.skin.button.hover.background = null;
			else
				GUI.skin.button.hover.background = guiHover;

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
		
		scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  musicManagerPosition.height - ( musicManagerPosition.height / 6 + 96 )));
		
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
			
			GUILayout.Box ( "All songs in " + currentDirectory.Substring ( startupManager.mediaPath.Length ));
			GUILayout.Space ( 10 );
			
			for ( int i = 0; i < currentDirectoryFiles.Length; i += 1 )
			{
				
				GUILayout.Label ( currentDirectoryFiles[i].Substring ( currentDirectory.Length + 1 ));
			}
		} else {
			
			if ( startupManager.showTutorials == true )
			{

				GUILayout.Box ( "There are no valid files in this directory!" );
				GUILayout.Label ( "If you want to add some music to this folder,\nclick the 'Open Current Directory' button bellow." +
					"\n\nDrop any .wav or .ogg files into the folder that will appear.\n\nThen, click 'Set as Active Directory' to see your songs in the MusicViewer.\n\nTo listen to any of your music, navigate to the MusicViewer\n(press the right arrow key), and click a song title." );
					
				if ( GUILayout.Button ( "Hide Tutorial Text" ))
				{
					
					startupManager.showTutorials = false;
				}
			}
		}
		
		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		GUILayout.Space ( 20 );
		
		if ( GUILayout.Button ( "Set as Active Directory" ))
			SetMusicViewerMedia ();
			
		if ( GUILayout.Button ( "Open Current Directory" ))
			Process.Start ( currentDirectory );
			
		if ( GUILayout.Button ( "New Folder" ))
			showNewFolderWindow = true;
			
		GUILayout.EndScrollView ();
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
		
		if ( showNewFolderWindow == true )
		{
			
			GUI.DrawTexture ( new Rect ( 0, 0, musicManagerPosition.width, musicManagerPosition.height ), startupManager.underlay );
		}
	}
	
	
	void NewFolderWindow ( int wid )
	{
		
		GUI.FocusWindow ( 7 );
		GUI.BringWindowToFront ( 7 );
		
		if ( GUI.Button ( new Rect ( 288, 20, 55, 20 ), "Cancel" ))
		{

			GUI.FocusWindow ( 4 );
			GUI.BringWindowToFront ( 4 );
			paneManager.popupBlocking = false;
			newFolderWindowRect.height = 70;
			showNewFolderWindow = false;
		}
		
		GUI.Label ( new Rect ( -25, 17, 340, 25 ), "Enter a folder name and press 'Create'." );
		
		folderName = GUI.TextField ( new Rect ( 65, 45, 280, 20 ), folderName );
		
		if ( GUI.Button ( new Rect ( 10, 45, 50, 20 ), "Create" ) && String.IsNullOrEmpty ( folderName.Trim ()) == false )
		{

			if ( !Directory.Exists ( currentDirectory + Path.DirectorySeparatorChar + folderName ))
			{
				
				Directory.CreateDirectory ( currentDirectory + Path.DirectorySeparatorChar + folderName );
				
				UpdateDirectories ();
				
				GUI.FocusWindow ( 4 );
				GUI.BringWindowToFront ( 4 );
				paneManager.popupBlocking = false;
				newFolderWindowRect.height = 70;
				showNewFolderWindow = false;
			}
		}
	}
	
	
	void SetMusicViewerMedia ()
	{
		
		musicViewer.mediaPath = currentDirectory;
		if ( currentDirectoryFiles.Any ())
		{
			
			musicViewer.clipList = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
		} else {
			
			musicViewer.clipList = new String[0];
		}
		
		StartCoroutine ( "SetArtwork" );
	}
}