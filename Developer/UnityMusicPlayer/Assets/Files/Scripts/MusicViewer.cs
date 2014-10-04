using System;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
//Written by Michael Bethke
//Thank you for saving me, Jesus!
//Thank you for living in me, Spirit!
//Thank you for making me, Father!
public class MusicViewer : MonoBehaviour
{
	
	public Font secretCode;
	public GUISkin guiSkin;
	public GUISkin optionsSkin;
	
#region Components

	GameObject manager;
	SocketsManager socketsManager;
	StartupManager startupManager;
	OnlineMusicBrowser onlineMusicBrowser;
	LoadingImage loadingImage;
	PaneManager paneManager;
	AudioVisualizer audioVisualizer;
	GUIText currentSong;
	GUIText timemark;
	
#endregion
	
#region MusicViewer
	
	internal bool showMusicViewer = true;
	
	string musicViewerTitle;
	internal Rect musicViewerPosition = new Rect ( 0, 0, 800, 600 );
	
	bool fileBrowser = false;
	bool showFolderMusic = false;
	string songInfoOwner;
	
	string browserCurrentDirectory;
	string [] browserCurrentDirectoryDirectories;
	string [] browserCurrentDirectoryFiles;
	
	internal string mediaPath;
	string songLocation;
	
	string parentDirectory;
	string [] parentDirectoryDirectories;
	string [] parentDirectoryFiles;
	
	string [] childDirectoryFiles;
	
	string activeDirectory;
	string [] activeDirectoryFiles;
	
	WWW wWw;

	float timebarTime;
	
	internal bool wasPlaying = false;
	
	string audioLocation;
	
	bool isPaused;	
	float pausePoint;
	
	int songTime;
	int minutes;
	float seconds;
	int rtMinutes;
	float rtSeconds;
	
	AudioType audioType;
	string audioTitle;

	float betweenSongDelay = 0.5F;
	
	int currentSongNumber = -1;
	int i;
	
	Vector2 scrollPosition;
	Vector2 mousePos;
	
	
	Vector2 optionsWindowScroll;
	
	
	Rect bottomBarPosition;
	private float bottomBarVelocity = 0.0F;
	
	int[] previousSongs = new int  [ 7 ] { 0, 0, 0, 0, 0, 0, 0 };
	int psPlace = 6;
	
#endregion
	
#region GUIStyles
	
	GUIStyle fileStyle;
	GUIStyle folderStyle;
	GUIStyle fileBrowserFileStyle;
	
	GUIStyle currentSongStyle;
	
	GUIStyle centerStyle;
	GUIStyle labelStyle;
	GUIStyle songStyle;
	GUIStyle buttonStyle;
	
	GUIStyle hideGUIStyle;
	GUIStyle showVisualizerStyle;
	GUIStyle doubleSpeedStyle;
	GUIStyle halfSpeedStyle;
	GUIStyle echoStyle;
	
#endregion
	
#region Textures
	
	public Texture2D folderIcon;
	public Texture2D musicNoteIcon;
	
	public Texture2D hideGUINormal;
	public Texture2D hideGUIHover;
	public Texture2D hideGUIOnNormal;
	public Texture2D hideGUIOnHover;
	
	public Texture2D showAudioVisualizerNormal;
	public Texture2D showAudioVisualizerHover;
	public Texture2D showAudioVisualizerOnNormal;
	public Texture2D showAudioVisualizerOnHover;
	
	public Texture2D audioSpeedDoubleNormal;
	public Texture2D audioSpeedDoubleHover;
	public Texture2D audioSpeedHalfNormal;
	public Texture2D audioSpeedHalfHover;
	public Texture2D audioSpeedNormalNormal;
	public Texture2D audioSpeedNormalHover;
	
	public Texture2D echoNormal;
	public Texture2D echoHover;
	public Texture2D echoOnNormal;
	public Texture2D echoOnHover;
	
	public Texture2D timebarMarker;
	
#endregion

#region EffectsSettings
	
	bool echo;
	internal bool hideGUI = false;
	internal bool showVisualizer = false;
	bool halfSpeed = false;
	bool doubleSpeed = false;
	
	string tempEchoDelay;
	string tempEchoDecayRate;
	string tempEchoWetMix;
	string tempEchoDryMix;

	bool showOptionsWindow = false;
	Rect optionsWindowRect = new Rect ( 0, 0, 370, 430 );
	
#endregion
	
#region AVSettings
	
	float tempAVcR;
	float tempAVcG;
	float tempAVcB;
	
	float tempYScale;
	
	bool tempBloom;
	bool tempSunShafts;
	bool tempBlur;
	string tempBlurIterations;
	bool tempVignetting;
	
	bool tempIterateEffects;
	//float tempIterateBloom;
	//float tempIterate
	
#endregion
	
#region GeneralSettings

	bool close = false;

	bool tempEnableTypes;
	bool tempEnableTimebar;
	bool tempEnableArtwork;
	bool tempEnableQuickManage;
	bool tempEnableHideGUINotifications;
	bool tempEnablePreciseTimemark;
	bool tempEnableDeepSearch;
	bool tempEnableArrows;
	bool tempUpdateNotifications;
	bool tempEnableKeybinds;

	internal bool tempEnableOMB;
	
	bool confirmSettingsReset = false;

#endregion

#region Slideshow

	internal bool slideshow = false;
	bool tempAutoAVBlur;
	bool tempAutoAVOff;

	string[] slideshowImageLocations;
	internal GUITexture currentSlideshowImage;
	Texture2D newSlideshowImage;
	float fadeVelocity = 0.0F;
	string tempSlideshowDisplayTime;
	
	bool fadeIn = false;
	bool fadeOut = false;

	int slideshowImage = 0;
	
#endregion	
	
	public static string RegexToString ( string key, bool isFloat )
	{
		
		if ( isFloat == true )
		{
			
			return Regex.Replace ( key, "[^0-9.]", "" );
		} else {		
			
			return Regex.Replace ( key, "[^0-9]", "" );
		}
	}

	
	void Start ()
	{

		manager = GameObject.FindGameObjectWithTag ( "Manager" );
		startupManager = manager.GetComponent<StartupManager> ();
		socketsManager = manager.GetComponent<SocketsManager> ();
		onlineMusicBrowser = GameObject.FindGameObjectWithTag ( "OnlineMusicBrowser" ).GetComponent <OnlineMusicBrowser>();
		paneManager = manager.GetComponent <PaneManager> ();
		loadingImage = GameObject.FindGameObjectWithTag ( "LoadingImage" ).GetComponent<LoadingImage>();
		currentSlideshowImage = GameObject.FindGameObjectWithTag ( "SlideshowImage" ).GetComponent<GUITexture>();
		currentSong = GameObject.FindGameObjectWithTag ( "CurrentSong" ).GetComponent<GUIText>();

		musicViewerPosition.width = Screen.width;
		musicViewerPosition.height = Screen.height;
		
		bottomBarPosition = new Rect (( musicViewerPosition.width - 240 ) / 2 , musicViewerPosition.height - 18, 240, 54 );

		optionsWindowRect.x = musicViewerPosition.width/2 - optionsWindowRect.width/2;
		optionsWindowRect.y = musicViewerPosition.height/2 - optionsWindowRect.height/2;

		audioVisualizer = GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizer> ();

		timemark = GameObject.FindGameObjectWithTag ( "Timemark" ).GetComponent<GUIText> ();
		GameObject.FindGameObjectWithTag ( "TimebarImage" ).guiTexture.pixelInset = new Rect ( -musicViewerPosition.width/2, musicViewerPosition.height/2 - 3, musicViewerPosition.width, 6 );
		
		
		
		LoadSettings ( false );
		
		
		
		centerStyle = new GUIStyle ();
		centerStyle.alignment = TextAnchor.MiddleCenter;
		
		labelStyle = new GUIStyle ();
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.wordWrap = true;
		
		folderStyle = new GUIStyle ();
		folderStyle.alignment = TextAnchor.MiddleLeft;
		folderStyle.border = new RectOffset ( 6, 6, 4, 4 );
		folderStyle.hover.background = guiSkin.button.hover.background;
		folderStyle.active.background = guiSkin.button.active.background;
		folderStyle.fontSize = 22;
		
		fileStyle = new GUIStyle ();
		fileStyle.alignment = TextAnchor.MiddleLeft;
		fileStyle.border = new RectOffset ( 6, 6, 6, 4 );
		fileStyle.padding = new RectOffset ( 6, 6, 3, 3 );
		fileStyle.margin = new RectOffset ( 4, 4, 4, 4 );
		fileStyle.hover.background = guiSkin.button.hover.background;
		fileStyle.active.background = guiSkin.button.active.background;
		fileStyle.fontSize = 22;
		
		fileBrowserFileStyle = new GUIStyle ();
		fileBrowserFileStyle.alignment = TextAnchor.MiddleLeft;
		
		currentSongStyle = new GUIStyle ();
		currentSongStyle.font = secretCode;
		currentSongStyle.fontSize = 31;
		
		songStyle = new GUIStyle ();
		songStyle.alignment = TextAnchor.MiddleLeft;
		songStyle.fontSize = 16;
		
		buttonStyle = new GUIStyle ();
		buttonStyle.fontSize = 22;
		buttonStyle.alignment = TextAnchor.MiddleCenter;
		buttonStyle.border = new RectOffset ( 6, 6, 6, 4 );
		buttonStyle.padding = new RectOffset ( 6, 6, 3, 3 );
		buttonStyle.margin = new RectOffset ( 4, 4, 4, 4 );
		buttonStyle.hover.background = guiSkin.button.hover.background;
		
		hideGUIStyle = new GUIStyle ();
		hideGUIStyle.normal.background = hideGUINormal;
		hideGUIStyle.hover.background = hideGUIHover;
		hideGUIStyle.onNormal.background = hideGUIOnNormal;
		hideGUIStyle.onHover.background = hideGUIOnHover;
		
		showVisualizerStyle = new GUIStyle ();
		showVisualizerStyle.normal.background = showAudioVisualizerNormal;
		showVisualizerStyle.hover.background = showAudioVisualizerHover;
		showVisualizerStyle.onNormal.background = showAudioVisualizerOnNormal;
		showVisualizerStyle.onHover.background = showAudioVisualizerOnHover;
		
		doubleSpeedStyle = new GUIStyle ();
		doubleSpeedStyle.normal.background = audioSpeedDoubleNormal;
		doubleSpeedStyle.hover.background = audioSpeedDoubleHover;
		doubleSpeedStyle.onNormal.background = audioSpeedNormalNormal;
		doubleSpeedStyle.onHover.background = audioSpeedNormalHover;
		
		halfSpeedStyle = new GUIStyle ();
		halfSpeedStyle.normal.background = audioSpeedHalfNormal;
		halfSpeedStyle.hover.background = audioSpeedHalfHover;
		halfSpeedStyle.onNormal.background = audioSpeedNormalNormal;
		halfSpeedStyle.onHover.background = audioSpeedNormalHover;
		
		echoStyle = new GUIStyle ();
		echoStyle.normal.background = echoNormal;
		echoStyle.hover.background = echoHover;
		echoStyle.onNormal.background = echoOnNormal;
		echoStyle.onHover.background = echoOnHover;
		
		InvokeRepeating ( "Refresh", 0, 2 );
		StartCoroutine ( SetArtwork ());
	}
	
	
	void LoadSettings ( bool reset )
	{
		
		if ( reset == true )
		{
			
			startupManager.preferences = new Preferences ();
			
			manager.audio.Stop ();
			manager.audio.clip = null;
			Resources.UnloadUnusedAssets ();
		}
		
		parentDirectory = startupManager.preferences.lastDirectory;
		activeDirectory = parentDirectory;
		
		tempEnableOMB = startupManager.preferences.enableOMB;

		tempEnableTypes = startupManager.preferences.enableTypes;	
		tempEnableArrows = startupManager.preferences.enableArrows;
		tempEnableTimebar = startupManager.preferences.enableTimebar;
		tempEnableArtwork = startupManager.preferences.enableArtwork;
		tempEnableKeybinds = startupManager.preferences.enableKeybinds;
		tempEnableDeepSearch = startupManager.preferences.enableDeepSearch;
		tempEnableQuickManage = startupManager.preferences.enableQuickManage;
		tempEnablePreciseTimemark = startupManager.preferences.enablePreciseTimemark;
		tempEnableHideGUINotifications = startupManager.preferences.enableHideGUINotifications;
		tempUpdateNotifications = startupManager.preferences.updateNotifications;

		tempAVcR = startupManager.preferences.avcR;
		tempAVcG = startupManager.preferences.avcG;
		tempAVcB = startupManager.preferences.avcB;
		
		tempYScale = startupManager.preferences.yScale;

		tempBloom = startupManager.preferences.bloom;
		tempBlur = startupManager.preferences.blur;
		tempSunShafts = startupManager.preferences.sunShafts;
		tempBlurIterations = startupManager.preferences.blurIterations.ToString ();
		tempVignetting = startupManager.preferences.vignetting;
		tempIterateEffects = startupManager.preferences.iterateEffects;
		
		manager.GetComponent<BlurEffect> ().iterations = startupManager.preferences.blurIterations;

		tempEchoDelay = startupManager.preferences.echoDelay.ToString ();
		tempEchoDecayRate = startupManager.preferences.echoDecayRate.ToString ();
		tempEchoWetMix = startupManager.preferences.echoWetMix.ToString ();
		tempEchoDryMix = startupManager.preferences.echoDryMix.ToString ();
		
		manager.GetComponent<AudioEchoFilter> ().delay = startupManager.preferences.echoDelay;
		manager.GetComponent<AudioEchoFilter> ().decayRatio = startupManager.preferences.echoDecayRate;
		manager.GetComponent<AudioEchoFilter> ().wetMix = startupManager.preferences.echoWetMix;
		manager.GetComponent<AudioEchoFilter> ().dryMix = startupManager.preferences.echoDryMix;
		
		tempAutoAVBlur = startupManager.preferences.autoAVBlur;
		tempAutoAVOff = startupManager.preferences.autoAVOff;

		tempSlideshowDisplayTime = startupManager.preferences.slideshowDisplayTime.ToString ();
		
		
		currentSong.text = "UnityMusicPlayer";
		GameObject.FindGameObjectWithTag ( "TimebarImage" ).guiTexture.enabled = startupManager.preferences.enableTimebar;

		if ( startupManager.preferences.enableTimebar == true )
		{

			musicViewerTitle = "";
			onlineMusicBrowser.onlineMusicBrowserTitle = "";
		} else {

			musicViewerTitle = "MusicViewer";
			onlineMusicBrowser.onlineMusicBrowserTitle = "OnlineMusicBrowser";
		}
		
		rtMinutes = 0;
		rtSeconds = 00;
		minutes = 0;
		seconds = 00;
			
		audioLocation = "";

		if ( startupManager.preferences.enablePreciseTimemark == true )
			timemark.text = "0:00.000][0:00.000";
		else
			timemark.text = "0:00][0:00";
	}
	
	
	void Refresh ()
	{
		
		if ( paneManager.currentPane == PaneManager.pane.musicViewer )
		{	
			
			try
			{
				
				if ( fileBrowser == false )
				{
					
					parentDirectoryDirectories = Directory.GetDirectories ( parentDirectory ).ToArray ();
					parentDirectoryFiles = Directory.GetFiles ( parentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
					try { childDirectoryFiles = Directory.GetFiles ( songInfoOwner, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray (); } catch { childDirectoryFiles = new string[0]; }
					
					activeDirectoryFiles = Directory.GetFiles ( activeDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
				} else {
					
					browserCurrentDirectoryDirectories = Directory.GetDirectories ( browserCurrentDirectory ).ToArray ();
					browserCurrentDirectoryFiles = Directory.GetFiles ( browserCurrentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
				}
			} catch ( Exception e ) {
			
				if ( startupManager.developmentMode == true )
					UnityEngine.Debug.LogWarning ( e );
				
				parentDirectory = startupManager.mediaPath.Substring ( 0, startupManager.mediaPath.Length - Path.DirectorySeparatorChar.ToString().Length );
			
				parentDirectoryDirectories = Directory.GetDirectories ( parentDirectory ).ToArray ();
				parentDirectoryFiles = Directory.GetFiles ( parentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
				
				browserCurrentDirectory = parentDirectory;
				browserCurrentDirectoryDirectories = Directory.GetDirectories ( browserCurrentDirectory ).ToArray ();
				browserCurrentDirectoryFiles = Directory.GetFiles ( browserCurrentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
				
				activeDirectory = parentDirectory;
				activeDirectoryFiles = parentDirectoryFiles;
			}
		}
	}


	void OptionsWindow ( int wid )
	{

		GUI.FocusWindow ( 5 );
		GUI.BringWindowToFront ( 5 );

		startupManager.showUnderlay = true;
		paneManager.popupBlocking = true;

		GUILayout.BeginVertical ();
		optionsWindowScroll = GUILayout.BeginScrollView ( optionsWindowScroll, false, true );
		
		
		GUILayout.Box ( "AudioVisualizer" );
		
		GUILayout.BeginHorizontal ();
		GUILayout.Label ( "Red", GUILayout.MaxWidth ( 50 ));
		tempAVcR = GUILayout.HorizontalSlider ( tempAVcR, 0.0F, 1.000F );
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal ();
		GUILayout.Label ( "Green", GUILayout.MaxWidth ( 50 ));
		tempAVcG = GUILayout.HorizontalSlider ( tempAVcG, 0.0F, 1.000F );
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal ();
		GUILayout.Label ( "Blue", GUILayout.MaxWidth ( 50 ));
		tempAVcB = GUILayout.HorizontalSlider ( tempAVcB, 0.0F, 1.000F );
		GUILayout.EndHorizontal ();
		
		
		GUI.contentColor = new Color ( tempAVcR, tempAVcG, tempAVcB, 1.000F );
		
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.Label ( guiSkin.toggle.normal.background, GUIStyle.none );
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		
		GUI.contentColor = Color.white;
		
		
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.Label ( "Sample Colour" );
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		
		
		tempSunShafts = GUILayout.Toggle ( tempSunShafts, "SunShafts" );
		tempBloom = GUILayout.Toggle ( tempBloom, "Bloom" );
		
		tempVignetting = GUILayout.Toggle ( tempVignetting, "Vignetting" );
		
		tempBlur = GUILayout.Toggle ( tempBlur, "Blur" );
		
		if ( tempBlur == true )
		{
			
			GUILayout.BeginHorizontal ();
			GUILayout.Space ( 10 );
			tempBlurIterations = GUILayout.TextField ( tempBlurIterations, 1, GUILayout.MaxWidth ( 16 ));
			tempBlurIterations = RegexToString ( tempBlurIterations, false );
			GUILayout.Label ( "Blur Amount (iterations)" );
			GUILayout.EndHorizontal ();
		}
		
		tempIterateEffects = GUILayout.Toggle ( tempIterateEffects, "Iterate Effects" );
		
		GUILayout.BeginHorizontal ();
		GUILayout.Label ( "Max Height", GUILayout.MaxWidth ( 80 ));
		tempYScale = GUILayout.HorizontalSlider ( tempYScale, 10.0F, 2000.0F );
		GUILayout.EndHorizontal ();
		


		GUILayout.Box ( "General" );
		
		tempEnableArrows = GUILayout.Toggle ( tempEnableArrows, "Show Arrows" );
		tempEnableArtwork = GUILayout.Toggle ( tempEnableArtwork, "Enable Artwork" );
		tempEnableTimebar = GUILayout.Toggle ( tempEnableTimebar, "Enable Timebar" );
		tempEnableKeybinds = GUILayout.Toggle ( tempEnableKeybinds, "Enable Keybinds" );
		tempEnableDeepSearch = GUILayout.Toggle ( tempEnableDeepSearch, "Enable DeepSearch" );
		tempEnableTypes = GUILayout.Toggle ( tempEnableTypes, "Show Audio Format" );
		tempEnableQuickManage = GUILayout.Toggle ( tempEnableQuickManage, "Enable QuickManage" );
		tempEnableHideGUINotifications = GUILayout.Toggle ( tempEnableHideGUINotifications, "GUI Notifications" );
		tempEnablePreciseTimemark = GUILayout.Toggle ( tempEnablePreciseTimemark, "Enable Precise Timemark" );
		tempEnableOMB = GUILayout.Toggle ( tempEnableOMB, "Enable OnlineMusicBrowser" );
		tempUpdateNotifications = GUILayout.Toggle ( tempUpdateNotifications, "Enable Update Notifications" );



		GUILayout.Box ( "Audio Echo" );
		
		GUILayout.BeginHorizontal ();
		GUILayout.Label ( "Echo Delay" );
		tempEchoDelay = GUILayout.TextField ( tempEchoDelay, 3, GUILayout.Width ( 100 ));
		tempEchoDelay = RegexToString ( tempEchoDelay, true );
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal ();
		GUILayout.Label ( "Echo Decay Rate" );
		tempEchoDecayRate = GUILayout.TextField ( tempEchoDecayRate, 3, GUILayout.Width ( 100 ));
		tempEchoDecayRate = RegexToString ( tempEchoDecayRate, true );
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal ();
		GUILayout.Label ( "Echo Wet Mix" );
		tempEchoWetMix = GUILayout.TextField (tempEchoWetMix, 3, GUILayout.Width ( 100 ));
		tempEchoWetMix = RegexToString ( tempEchoWetMix, true );
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal ();
		GUILayout.Label ( "Echo Dry Mix" );
		tempEchoDryMix = GUILayout.TextField ( tempEchoDryMix, 3, GUILayout.Width ( 100 ));
		tempEchoDryMix = RegexToString ( tempEchoDryMix, true );
		GUILayout.EndHorizontal ();


		
		GUILayout.Box ( "Slideshow" );
		
		GUI.skin.button.fontSize = 16;
		if ( GUILayout.Button ( "Start Slideshow" ))
		{
			
			slideshow = true;
			close = true;
		}
		GUI.skin.button.fontSize = 22;
		
		tempAutoAVOff = GUILayout.Toggle ( tempAutoAVOff, "Force AudioVisualizer OFF" );
		tempAutoAVBlur = GUILayout.Toggle ( tempAutoAVBlur, "Force Blur ON" );
		
		GUILayout.BeginHorizontal ();
		GUILayout.Label ( "Display Time (seconds)" );
		tempSlideshowDisplayTime = GUILayout.TextField ( tempSlideshowDisplayTime, 3, GUILayout.Width ( 100 ));
		tempSlideshowDisplayTime = RegexToString ( tempSlideshowDisplayTime, true );
		GUILayout.EndHorizontal ();
		
		
		
		GUILayout.Box ( "UnityMusicPlayer Version " + startupManager.runningVersion );
		
		GUI.skin.button.fontSize = 16;
		if ( GUILayout.Button ( "Reset Tutorials" ))
		{
			
			startupManager.preferences.enableTutorials = true;
		}
		
		if ( confirmSettingsReset == false )
		{
			
			if ( GUILayout.Button ( "Restore Default Settings" ))
			{
				
				confirmSettingsReset = true;
			}
		} else {
			
			if ( GUILayout.Button ( "Confirm Restore" ))
			{
				
				LoadSettings ( true );
				close = true;
			}
		}
		
		GUI.skin.button.fontSize = 22;
		
		
		if ( GUILayout.Button ( "Save & Close" ) || Input.GetKey ( KeyCode.Escape ))
		{
			
			close = true;
		}
		
		
		GUILayout.EndScrollView ();
		GUILayout.EndVertical ();


		
		if ( close == true )
		{

			if ( tempEchoDelay.Trim () == "" )
				tempEchoDelay = "100";
				
			startupManager.preferences.echoDelay = float.Parse ( tempEchoDelay );

			if ( tempEchoDecayRate.Trim () == "" )
				tempEchoDecayRate = "0.3";
				
			startupManager.preferences.echoDecayRate = float.Parse ( tempEchoDecayRate );

			if ( tempEchoWetMix.Trim () == "" )
				tempEchoWetMix = "0.8";
				
			startupManager.preferences.echoWetMix = float.Parse ( tempEchoWetMix );

			if ( tempEchoDryMix.Trim () == "" )
				tempEchoDryMix = "0.6";
				
			startupManager.preferences.echoDryMix = float.Parse ( tempEchoDryMix );

			manager.GetComponent<AudioEchoFilter> ().delay = Convert.ToSingle ( startupManager.preferences.echoDelay );
			manager.GetComponent<AudioEchoFilter> ().decayRatio = Convert.ToSingle ( startupManager.preferences.echoDecayRate );
			manager.GetComponent<AudioEchoFilter> ().wetMix = Convert.ToSingle ( startupManager.preferences.echoWetMix );
			manager.GetComponent<AudioEchoFilter> ().dryMix = Convert.ToSingle ( startupManager.preferences.echoDryMix );

			startupManager.preferences.avcR = tempAVcR;
			startupManager.preferences.avcG = tempAVcG;
			startupManager.preferences.avcB = tempAVcB;
			
			startupManager.preferences.yScale = tempYScale;
			
			startupManager.preferences.sunShafts = tempSunShafts;
			startupManager.preferences.bloom = tempBloom;
			startupManager.preferences.blur = tempBlur;
			startupManager.preferences.vignetting = tempVignetting;
			startupManager.preferences.iterateEffects = tempIterateEffects;
			
			if ( tempBlurIterations.Trim () == "" )
				tempBlurIterations = "3";
				
			startupManager.preferences.blurIterations = int.Parse ( tempBlurIterations );
			manager.GetComponent<BlurEffect> ().iterations = startupManager.preferences.blurIterations;
			
			startupManager.preferences.enableTypes = tempEnableTypes;
			if ( manager.audio.clip != null )
			{

				if ( startupManager.preferences.enableTypes == true )
					currentSong.text = audioLocation.Substring ( audioLocation.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length );
				else
					currentSong.text = audioLocation.Substring ( audioLocation.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length, audioLocation.LastIndexOf ( "." ) - audioLocation.LastIndexOf ( Path.DirectorySeparatorChar ) - Path.DirectorySeparatorChar.ToString().Length );
			}

			startupManager.preferences.enableArrows = tempEnableArrows;
			
			startupManager.preferences.enableTimebar = tempEnableTimebar;
			if ( startupManager.preferences.enableTimebar == true )
			{

				GameObject.FindGameObjectWithTag ( "TimebarImage" ).guiTexture.enabled = true;
				musicViewerTitle = "";
				onlineMusicBrowser.onlineMusicBrowserTitle = "";
			} else {

				GameObject.FindGameObjectWithTag ( "TimebarImage" ).guiTexture.enabled = false;
				musicViewerTitle = "MusicViewer";
				onlineMusicBrowser.onlineMusicBrowserTitle = "OnlineMusicBrowser";
			}
			
			startupManager.preferences.enableArtwork = tempEnableArtwork;
			
			if ( startupManager.preferences.enableArtwork == true )
				StartCoroutine ( SetArtwork ());
			else
				currentSlideshowImage.texture = null;
			
			startupManager.preferences.enableKeybinds = tempEnableKeybinds;
			startupManager.preferences.enableDeepSearch = tempEnableDeepSearch;
			startupManager.preferences.enableQuickManage = tempEnableQuickManage;
			startupManager.preferences.enablePreciseTimemark = tempEnablePreciseTimemark;
			startupManager.preferences.enableHideGUINotifications = tempEnableHideGUINotifications;
			startupManager.preferences.updateNotifications = tempUpdateNotifications;
			
			if ( startupManager.preferences.enableOMB == false && Convert.ToBoolean ( tempEnableOMB ) == true )
			{
				
				startupManager.SendMessage ( "RefreshOMB" );
				paneManager.loading = true;
			}
			
			startupManager.preferences.enableOMB = tempEnableOMB;
			
			if ( tempSlideshowDisplayTime.Trim () == "" )
				tempSlideshowDisplayTime = "2.0";

			startupManager.preferences.autoAVBlur = tempAutoAVBlur;
			startupManager.preferences.autoAVOff = tempAutoAVOff;
			if ( slideshow == true )
			{
				
				if ( startupManager.preferences.autoAVOff == true || showVisualizer == false )
				{
					
					showVisualizer = false;
	
					manager.GetComponent<BloomAndLensFlares>().enabled = false;
					manager.GetComponent<BlurEffect>().enabled = false;
					manager.GetComponent<SunShafts>().enabled = false;
					manager.GetComponent<Vignetting>().enabled = false;
	
				} else {
					
					audioVisualizer.topLeftLine.material.color = new Color ( startupManager.preferences.avcR, startupManager.preferences.avcG, startupManager.preferences.avcB, 255 );
					audioVisualizer.bottomLeftLine.material.color = new Color ( startupManager.preferences.avcR, startupManager.preferences.avcG, startupManager.preferences.avcB, 255 );
					audioVisualizer.topRightLine.material.color = new Color ( startupManager.preferences.avcR, startupManager.preferences.avcG, startupManager.preferences.avcB, 255 );
					audioVisualizer.bottomRightLine.material.color = new Color ( startupManager.preferences.avcR, startupManager.preferences.avcG, startupManager.preferences.avcB, 255 );
	
					manager.GetComponent<BloomAndLensFlares>().enabled = startupManager.preferences.bloom;
					if ( startupManager.preferences.autoAVBlur == false )
					{
						
						manager.GetComponent<BlurEffect>().enabled = startupManager.preferences.blur;
					} else {
						
						manager.GetComponent<BlurEffect>().enabled = true;
					}
						
					manager.GetComponent<SunShafts>().enabled = startupManager.preferences.sunShafts;
					manager.GetComponent<Vignetting>().enabled = startupManager.preferences.vignetting;
				}

				if ( startupManager.preferences.enableTimebar == false )
					musicViewerTitle = "";
				
				timemark.enabled = false;
				currentSong.text = "";
				hideGUI = true;
				
				StartCoroutine ( "LoadSlideshow", true );
			}

			bool preferencesSaved = false;
			preferencesSaved = startupManager.SavePreferences ();
			while ( preferencesSaved == false ) {}

			confirmSettingsReset = false;

			startupManager.showUnderlay = false;
			paneManager.popupBlocking = false;
			showOptionsWindow = false;
			optionsWindowScroll = new Vector2 ( 0, 0 );
			
			GUI.FocusWindow ( 0 );
			GUI.BringWindowToFront ( 0 );
			
			close = false;
		}
	}
	
	
	IEnumerator LoadSlideshow ( bool slideshowStart )
	{
		
		slideshowImageLocations = Directory.GetFiles ( startupManager.slideshowPath, "*.*" ).Where ( s => s.EndsWith ( ".png" ) || s.EndsWith ( ".jpg" ) || s.EndsWith ( ".jpeg" )).ToArray ();
		if ( slideshowImageLocations.Length > 0 )
		{
			
			wWw = new WWW ( "file://" + slideshowImageLocations [ slideshowImage ] );
			yield return wWw;
			
			slideshowImage += 1;
			if ( slideshowImage >= slideshowImageLocations.Length )
				slideshowImage = 0;
		}
		
		if ( slideshowStart == false )
		{
		
			float slideshowDisplayTime = Convert.ToSingle ( tempSlideshowDisplayTime );
			yield return new WaitForSeconds ( slideshowDisplayTime );
			
			fadeIn = false;
			fadeOut = true;
		} else {
			
			currentSlideshowImage.color = new Color ( 0.5F, 0.5F, 0.5F, 0.0F );
			StartCoroutine ( "SlideshowIN" );
		}
	}
	

	IEnumerator SlideshowIN ()
	{
		
		/*Thanks to http://andrew.hedges.name/experiments/aspect_ratio*/
		
		currentSlideshowImage.texture = null;
		Resources.UnloadUnusedAssets ();
		
		Vector2 tempImageSize = new Vector2 ( wWw.texture.width, wWw.texture.height );
		
		if ( tempImageSize.x > musicViewerPosition.width )
		{
			
			float tempSizeDifference = tempImageSize.x - musicViewerPosition.width;
			tempImageSize = new Vector2 ( tempImageSize.x - tempSizeDifference, tempImageSize.y / tempImageSize.x * ( tempImageSize.x - tempSizeDifference ));
		}
		
		if ( tempImageSize.y > musicViewerPosition.height )
		{
			
			float tempSizeDifference = tempImageSize.y - musicViewerPosition.height;
			tempImageSize = new Vector2 ( tempImageSize.x / tempImageSize.y * ( tempImageSize.y - tempSizeDifference ), tempImageSize.y - tempSizeDifference );
		}
		
		currentSlideshowImage.pixelInset = new Rect (( tempImageSize.x / 2 ) * -1, ( tempImageSize.y / 2 ) * -1 ,  tempImageSize.x , tempImageSize.y );
		
		newSlideshowImage = new Texture2D (( int ) tempImageSize.x, ( int ) tempImageSize.y, TextureFormat.ARGB32, false );
		wWw.LoadImageIntoTexture ( newSlideshowImage );
		currentSlideshowImage.texture = newSlideshowImage;

		yield return new WaitForSeconds ( 1.0F );
		fadeIn = true;
	}
	
	
	IEnumerator SetArtwork ()
	{
		
		if ( startupManager.preferences.enableArtwork == true )
		{
			
			string[] artworkImageLocations = new string[0];
			artworkImageLocations = Directory.GetFiles ( activeDirectory, "Artwork.*" ).Where ( s => s.EndsWith ( ".png" ) || s.EndsWith ( ".jpg" ) || s.EndsWith ( ".jpeg" )).ToArray ();
			
			if ( artworkImageLocations.Any ())
			{
	
				WWW wWw = new WWW ( "file://" + artworkImageLocations [ 0 ] );
				yield return wWw;
				
				currentSlideshowImage.texture = wWw.texture;
			} else {
				currentSlideshowImage.texture = null;
			}
		} else {
			
			if ( slideshow == false )
				currentSlideshowImage.texture = null;
		}
	}


	void OnGUI ()
	{
		
		if ( manager.audio.clip != null && startupManager.preferences.enableTimebar == true )
		{
			
			GUI.DrawTexture ( new Rect ( manager.audio.time * ( musicViewerPosition.width/manager.audio.clip.length ), -3, 10, 6 ), timebarMarker );
		}
			
		
		if ( showMusicViewer == true )
		{
			
			GUI.skin = optionsSkin;

			if ( showOptionsWindow == true )
			{
				
				GUI.Window ( 5, optionsWindowRect, OptionsWindow, "" );
			}
			
			GUI.skin = guiSkin;
			
			if ( startupManager.showUnderlay == true )
			{
				
				fileStyle.hover.background = null;
				folderStyle.hover.background = null;
				buttonStyle.hover.background = null;
			} else {
				
				fileStyle.hover.background = guiSkin.button.hover.background;
				folderStyle.hover.background = guiSkin.button.hover.background;
				buttonStyle.hover.background = guiSkin.button.hover.background;
			}
			
			musicViewerPosition = GUI.Window ( 0, musicViewerPosition, MusicViewerPane, musicViewerTitle );
		}
	}
	

	void MusicViewerPane ( int wid )
	{

		if ( slideshow == false )
		{
			
			if ( hideGUI == false )
			{
			
				if ( fileBrowser == false )
				{
					
					GUI.Label ( new Rect ( musicViewerPosition.width/2 - 100, musicViewerPosition.height/4 - 50, 100, 25 ), "Volume" );
					startupManager.preferences.volumebarValue = GUI.HorizontalSlider ( new Rect ( musicViewerPosition.width/2 - 118, musicViewerPosition.height/4 - 30, 100, 15 ), startupManager.preferences.volumebarValue, 0.0F, 1.0F );
		
					if ( GUI.Button ( new Rect ( musicViewerPosition.width/2 - 70, musicViewerPosition.height/4 - 15, 60, 30 ), "Next", buttonStyle ))
						NextSong ();

					if ( GUI.Button ( new Rect ( musicViewerPosition.width/2 - 130, musicViewerPosition.height/4 - 15, 60, 30 ), "Back", buttonStyle ))
						PreviousSong ();
				
					GUI.Label ( new Rect ( musicViewerPosition.width/2 + 10, musicViewerPosition.height/4 - 50, 120, 30 ), "Loop" );
				
					if ( startupManager.preferences.loop = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 5, musicViewerPosition.height/4 - 45, 100, 20 ), startupManager.preferences.loop, "" ))
					{
						
						if ( startupManager.preferences.loop == true && startupManager.preferences.shuffle == true || startupManager.preferences.loop == true && startupManager.preferences.continuous == true )
						{
					
							startupManager.preferences.shuffle = false;
							startupManager.preferences.continuous = false;
						}
					}
				
					GUI.Label ( new Rect ( musicViewerPosition.width/2 + 10, musicViewerPosition.height/4 - 30, 120, 30 ), "Shuffle" );
				
					if ( startupManager.preferences.shuffle = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 5, musicViewerPosition.height/4 - 25, 100, 20 ), startupManager.preferences.shuffle, "" ))
					{
						
						if ( startupManager.preferences.shuffle == true && startupManager.preferences.loop == true || startupManager.preferences.shuffle == true && startupManager.preferences.continuous == true )
						{
					
							startupManager.preferences.loop = false;
							startupManager.preferences.continuous = false;
						}
					}
				
					GUI.Label ( new Rect ( musicViewerPosition.width/2 + 10, musicViewerPosition.height/4 - 10, 120, 30 ), "Continuous" );
				
					if ( startupManager.preferences.continuous = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 5, musicViewerPosition.height/4 - 5, 100, 20 ), startupManager.preferences.continuous, "" ))
					{
	
						if ( startupManager.preferences.continuous == true && startupManager.preferences.shuffle == true || startupManager.preferences.continuous == true && startupManager.preferences.loop == true )
						{
					
							startupManager.preferences.shuffle = false;
							startupManager.preferences.loop = false;
						}
					}
					
					GUILayout.BeginHorizontal ();
					GUILayout.Space ( musicViewerPosition.width/16 );
					GUILayout.BeginVertical ();
					GUILayout.Space ( musicViewerPosition.height / 4 );
					scrollPosition = GUILayout.BeginScrollView ( /*new Rect ( musicViewerPosition.width/16, musicViewerPosition.height/4, musicViewerPosition.width - ( musicViewerPosition.width/8 ), musicViewerPosition.height - ( musicViewerPosition.height/4 )),*/ scrollPosition, /*new Rect ( musicViewerPosition.width/16, musicViewerPosition.height/4, musicViewerPosition.width - ( musicViewerPosition.width/8 ), musicViewerPosition.height - ( musicViewerPosition.height/4 ))*/ GUILayout.Width ( musicViewerPosition.width - ( musicViewerPosition.width/8 )), GUILayout.Height (  musicViewerPosition.height - ( musicViewerPosition.height / 4 + 60 )));
					
					if ( parentDirectoryFiles.Any ())
					{
					
						for ( int songInt = 0; songInt < parentDirectoryFiles.Length; songInt ++ )
						{
											
							string audioTitle;
							if ( startupManager.preferences.enableTypes == true )
								audioTitle = parentDirectoryFiles[songInt].Substring ( parentDirectory.Length + Path.DirectorySeparatorChar.ToString().Length );
							else		
								audioTitle = parentDirectoryFiles[songInt].Substring ( parentDirectoryFiles[songInt].LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length, parentDirectoryFiles[songInt].LastIndexOf ( "." ) - parentDirectoryFiles[songInt].LastIndexOf ( Path.DirectorySeparatorChar ) - Path.DirectorySeparatorChar.ToString().Length );

							if ( GUILayout.Button ( new GUIContent ( audioTitle ), buttonStyle ))
							{
						
								Resources.UnloadUnusedAssets ();
												
								currentSongNumber = songInt;
								previousSongs [ 0 ] = previousSongs [ 1 ];
								previousSongs [ 1 ] = previousSongs [ 2 ];
								previousSongs [ 2 ] = previousSongs [ 3 ];
								previousSongs [ 3 ] = previousSongs [ 4 ];
								previousSongs [ 4 ] = previousSongs [ 5 ];
								previousSongs [ 5 ] = previousSongs [ 6 ];
								previousSongs [ 6 ] = songInt;
								psPlace = 6;
												
								wasPlaying = false;
								
								activeDirectory = parentDirectory;
								activeDirectoryFiles = parentDirectoryFiles;
											
								if ( activeDirectoryFiles[songInt].Substring ( activeDirectoryFiles [songInt].LastIndexOf ( "." )) == ".unity3d" )
								{
												
									loadingImage.showLoadingImages = true;
									loadingImage.InvokeRepeating ( "LoadingImages", 0.25F, 0.25F );
												
									StartCoroutine ( LoadAssetBundle ( "file://" + activeDirectoryFiles [ currentSongNumber ]));
												
								} else {
												
									StartCoroutine ( PlayAudio ( activeDirectoryFiles[songInt] ));
									loadingImage.showLoadingImages = false;
								}
							}
						}
					} else
					{
						
						if ( startupManager.preferences.enableTutorials == true )
						{
								
							GUILayout.Label ( "You don't have any audio to play!\n\nIf you have some music (.wav, .ogg, or .aiff),\nclick 'Open File Browser' under the System Commands bar bellow." +
								"\n\nYou can also download music by navigating\nto the OnlineMusicBrowser (press the right arrow key).\n", centerStyle );
						
							if ( GUILayout.Button ( "Hide Tutorials", buttonStyle ))
							{
							
								startupManager.preferences.enableTutorials = false;
							}
							
							if ( GUILayout.Button ( "View Extended Help/Tutorial", buttonStyle ))
							{
											
								Process.Start ( startupManager.helpPath );
							}
						}
					}
					
					if ( startupManager.preferences.enableDeepSearch == true )
					{
						
						if ( parentDirectoryDirectories.Any ())
						{
						
							for ( int directoryInt = 0; directoryInt < parentDirectoryDirectories.Length; directoryInt ++ )
							{
									
								if ( GUILayout.Button ( new GUIContent ( parentDirectoryDirectories[directoryInt].Substring ( parentDirectoryDirectories[directoryInt].LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length ), folderIcon), folderStyle ))
								{
									
									if ( showFolderMusic == false )
									{
										
										showFolderMusic = true;
										songInfoOwner = parentDirectoryDirectories[directoryInt];
										Refresh ();
									} else {
										
										if ( songInfoOwner != parentDirectoryDirectories[directoryInt] )
										{
											
											songInfoOwner = parentDirectoryDirectories[directoryInt];
											Refresh ();
										} else {
												
											showFolderMusic = false;
										}
									}
								}
								
								if ( showFolderMusic == true )
								{
							
									if ( songInfoOwner == parentDirectoryDirectories[directoryInt] )
									{
										
										if ( childDirectoryFiles.Any ())
										{
								
											for ( int childSongInt = 0; childSongInt < childDirectoryFiles.Length; childSongInt++ )
											{
											
												string childAudioTitle;
												if ( startupManager.preferences.enableTypes == true )				
													childAudioTitle = childDirectoryFiles[childSongInt].Substring ( childDirectoryFiles[childSongInt].LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length );
												else			
													childAudioTitle = childDirectoryFiles[childSongInt].Substring ( childDirectoryFiles[childSongInt].LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length, childDirectoryFiles[childSongInt].LastIndexOf ( "." ) - childDirectoryFiles[childSongInt].LastIndexOf ( Path.DirectorySeparatorChar ) - Path.DirectorySeparatorChar.ToString().Length );
										
												GUILayout.BeginHorizontal ();
												GUILayout.Space ( 20 );
												
												if ( GUILayout.Button ( new GUIContent ( childAudioTitle ), fileStyle ))
												{
													
													Resources.UnloadUnusedAssets ();
												
													currentSongNumber = childSongInt;
													previousSongs [ 0 ] = previousSongs [ 1 ];
													previousSongs [ 1 ] = previousSongs [ 2 ];
													previousSongs [ 2 ] = previousSongs [ 3 ];
													previousSongs [ 3 ] = previousSongs [ 4 ];
													previousSongs [ 4 ] = previousSongs [ 5 ];
													previousSongs [ 5 ] = previousSongs [ 6 ];
													previousSongs [ 6 ] = childSongInt;
													psPlace = 6;
												
													wasPlaying = false;
													activeDirectory = songInfoOwner;
													activeDirectoryFiles = childDirectoryFiles;

													if ( activeDirectoryFiles[childSongInt].Substring ( activeDirectoryFiles [childSongInt].LastIndexOf ( "." )) == ".unity3d" )
													{
												
														loadingImage.showLoadingImages = true;
														loadingImage.InvokeRepeating ( "LoadingImages", 0.25F, 0.25F );
												
														StartCoroutine ( LoadAssetBundle ( "file://" + activeDirectoryFiles [ childSongInt ]));
												
													} else {
												
														StartCoroutine ( PlayAudio ( activeDirectoryFiles [childSongInt] ));
														loadingImage.showLoadingImages = false;
													}
												}
												GUILayout.EndHorizontal ();
											}
										} else {
											
											GUILayout.Label ( "This folder doesn't contain any music!" );
										}
									}
								}
							}
						}
					}
				} else {	// FileBrowser == true
					
					if ( browserCurrentDirectory.Substring ( 0, browserCurrentDirectory.LastIndexOf ( Path.DirectorySeparatorChar )).Length > 0 )
					{
						
						if ( GUI.Button ( new Rect ( musicViewerPosition.width/2 - 300, musicViewerPosition.height/4 - 15, 140, 30 ), new GUIContent ( "Previous", paneManager.leftArrowNormal ), buttonStyle ))
						{
						
							browserCurrentDirectory = browserCurrentDirectory.Substring ( 0, browserCurrentDirectory.LastIndexOf ( Path.DirectorySeparatorChar ));
							browserCurrentDirectoryDirectories = Directory.GetDirectories ( browserCurrentDirectory ).ToArray ();
							browserCurrentDirectoryFiles = Directory.GetFiles ( browserCurrentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
						}
					}
					
					if ( GUI.Button ( new Rect ( musicViewerPosition.width/2 - 150, musicViewerPosition.height/4 - 15, 200, 30 ), "Active Directory", buttonStyle ))
					{
						
						browserCurrentDirectory = parentDirectory;
						browserCurrentDirectoryDirectories = parentDirectoryDirectories;
						browserCurrentDirectoryFiles = parentDirectoryFiles;
					}
		
					if ( GUI.Button ( new Rect ( musicViewerPosition.width/2 + 60, musicViewerPosition.height/4 - 15, 240, 30 ), "Open in " + startupManager.directoryBrowser, buttonStyle ))
						Process.Start ( browserCurrentDirectory );

					GUILayout.BeginHorizontal ();
					GUILayout.Space ( musicViewerPosition.width / 16 );
					GUILayout.BeginVertical ();
					GUILayout.Space ( musicViewerPosition.height / 4 );
					
					scrollPosition = GUILayout.BeginScrollView ( /*new Rect ( musicViewerPosition.width/16, musicViewerPosition.height/4, musicViewerPosition.width - ( musicViewerPosition.width/8 ), musicViewerPosition.height - ( musicViewerPosition.height/4 )),*/ scrollPosition, /*new Rect ( musicViewerPosition.width/16, musicViewerPosition.height/4, musicViewerPosition.width - ( musicViewerPosition.width/8 ), musicViewerPosition.height - ( musicViewerPosition.height/4 ))*/ GUILayout.Width ( musicViewerPosition.width - ( musicViewerPosition.width/8 )), GUILayout.Height (  musicViewerPosition.height - ( musicViewerPosition.height / 4 + 60 )));
			
					for ( int i = 0; i < browserCurrentDirectoryFiles.Length; i += 1 )
					{
			
						GUILayout.Button ( new GUIContent ( browserCurrentDirectoryFiles[i].Substring ( browserCurrentDirectory.Length + 1 ), musicNoteIcon ), fileBrowserFileStyle );
					}
			
					int pDDi = 0;
					if ( browserCurrentDirectoryDirectories.Any ())
					{
						
						do
						{
							
							if ( browserCurrentDirectoryDirectories[pDDi] == parentDirectory )
							{
                	
								folderStyle.fontStyle = FontStyle.Italic;
							} else {
									
								folderStyle.fontStyle = FontStyle.Normal;
							}
							
							if ( GUILayout.Button ( new GUIContent ( browserCurrentDirectoryDirectories[pDDi].Substring ( browserCurrentDirectory.Length + 1 ), folderIcon ), folderStyle ))
							{
					
								browserCurrentDirectory = browserCurrentDirectoryDirectories[pDDi];
								browserCurrentDirectoryDirectories = Directory.GetDirectories ( browserCurrentDirectory ).ToArray ();
								browserCurrentDirectoryFiles = Directory.GetFiles ( browserCurrentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
							}
							
							pDDi += 1;
						}
						
						while ( pDDi < browserCurrentDirectoryDirectories.Length );
					}
					
					if ( browserCurrentDirectoryFiles.Length == 0 && browserCurrentDirectoryDirectories.Length == 0 )
					{
						
						GUILayout.FlexibleSpace ();
						GUILayout.Label ( "This folder is empty!", labelStyle );
						GUILayout.FlexibleSpace ();
					}

					if ( browserCurrentDirectory != parentDirectory )
					{
						
						if ( GUILayout.Button ( "Set as Active Directory" ))
						{
						
							parentDirectory = browserCurrentDirectory;
							startupManager.preferences.lastDirectory = parentDirectory;
							StartCoroutine ( SetArtwork ());
							
							activeDirectory = parentDirectory;
							
							fileBrowser = false;
							Refresh ();
						
							scrollPosition.y = 0;
						}
					}
				}
			
				GUILayout.Box ( "System Commands" );
			
				if ( fileBrowser == false )
				{
					
					if ( startupManager.preferences.enableQuickManage == true )
						if ( GUILayout.Button ( "Open Current Directory", buttonStyle ))
							Process.Start ( parentDirectory );
					
					if ( GUILayout.Button ( "Open File Browser", buttonStyle ))
					{
						
						browserCurrentDirectory = parentDirectory;
						fileBrowser = true;
						Refresh ();
					
						scrollPosition.y = 0;
					}
				} else {
					if ( GUILayout.Button ( "Close File Browser", buttonStyle ))
					{
				
						fileBrowser = false;
						Refresh ();
					
						scrollPosition.y = 0;
						
						bool preferencesSaved = false;
						preferencesSaved = startupManager.SavePreferences ();
						while ( preferencesSaved == false ) {}
					}
				}
										
				if ( GUILayout.Button ( "Options", buttonStyle ))
				{
					
					showOptionsWindow = true;
				}
			
				GUILayout.EndScrollView();
				GUILayout.EndVertical();
				//GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginArea ( bottomBarPosition );
			GUILayout.BeginHorizontal ( );
			
			GUILayout.Space ( 10 );
			hideGUI = GUILayout.Toggle ( hideGUI, "", hideGUIStyle, GUILayout.Height ( 36 ));
			
			currentSong.enabled = !hideGUI;
			timemark.enabled = !hideGUI;
			
			GUILayout.Space ( 10 );
			showVisualizer = GUILayout.Toggle ( showVisualizer, "", showVisualizerStyle, GUILayout.Height ( 36 ));
			
			GUILayout.Space ( 10 );
			if ( doubleSpeed = GUILayout.Toggle ( doubleSpeed, "", doubleSpeedStyle, GUILayout.Height ( 36 )))
			{
					
				manager.audio.pitch = 2.0F;
			
				if ( doubleSpeed == true && halfSpeed == true )
					halfSpeed = false;
			}
			
			GUILayout.Space ( 10 );
			if ( halfSpeed = GUILayout.Toggle ( halfSpeed, "", halfSpeedStyle, GUILayout.Height ( 36 )))
			{
				
				manager.audio.pitch = 0.5F;
			
				if ( halfSpeed == true && doubleSpeed == true )
					doubleSpeed = false;
			}
			
			GUILayout.Space ( 10 );
			echo = GUILayout.Toggle ( echo, "", echoStyle, GUILayout.Height ( 36 ));
			GUILayout.Space ( 10 );
			
			GUILayout.EndHorizontal ();
			GUILayout.EndArea ();
			
			
			if ( echo == true )
				manager.GetComponent<AudioEchoFilter> ().enabled = true;
			else
			    manager.GetComponent<AudioEchoFilter> ().enabled = false;
		
				
			if ( halfSpeed == false && doubleSpeed == false )
				manager.audio.pitch = 1.0F;
			
			if ( showVisualizer == true )
			{
		
				audioVisualizer.topLeftLine.material.color = new Color ( startupManager.preferences.avcR, startupManager.preferences.avcG, startupManager.preferences.avcB, 255 );
				audioVisualizer.bottomLeftLine.material.color = new Color ( startupManager.preferences.avcR, startupManager.preferences.avcG, startupManager.preferences.avcB, 255 );
				audioVisualizer.topRightLine.material.color = new Color ( startupManager.preferences.avcR, startupManager.preferences.avcG, startupManager.preferences.avcB, 255 );
				audioVisualizer.bottomRightLine.material.color = new Color ( startupManager.preferences.avcR, startupManager.preferences.avcG, startupManager.preferences.avcB, 255 );
		
				manager.GetComponent<BloomAndLensFlares>().enabled = startupManager.preferences.bloom;
				manager.GetComponent<BlurEffect>().enabled = startupManager.preferences.blur;
				manager.GetComponent<SunShafts>().enabled = startupManager.preferences.sunShafts;
				manager.GetComponent<Vignetting>().enabled = startupManager.preferences.vignetting;
				
				manager.GetComponent<BlurEffect> ().iterations = startupManager.preferences.blurIterations;
			} else {
		
				manager.GetComponent<BloomAndLensFlares>().enabled = false;
				manager.GetComponent<BlurEffect>().enabled = false;
				manager.GetComponent<SunShafts>().enabled = false;
				manager.GetComponent<Vignetting>().enabled = false;
			}
			
			if ( startupManager.showUnderlay == true )
			{
				
				GUI.DrawTexture ( new Rect ( 0, 0, musicViewerPosition.width, musicViewerPosition.height ), startupManager.underlay );
			}
		}
	}


	void NextSong ()
	{
			
		if ( activeDirectoryFiles.Any ())
		{

			wasPlaying = false;
			if ( psPlace < 6 )
			{

				psPlace += 1;
				currentSongNumber = previousSongs [ psPlace ];
			
			} else {

				if ( startupManager.preferences.continuous == true || startupManager.preferences.loop == false && startupManager.preferences.shuffle == false && startupManager.preferences.continuous == false )
				{
						
					if ( currentSongNumber == activeDirectoryFiles.Length - 1 )
						currentSongNumber = 0;
					else
						currentSongNumber++;

					previousSongs [ 0 ] = previousSongs [ 1 ];
					previousSongs [ 1 ] = previousSongs [ 2 ];
					previousSongs [ 2 ] = previousSongs [ 3 ];
					previousSongs [ 3 ] = previousSongs [ 4 ];
					previousSongs [ 4 ] = previousSongs [ 5 ];
					previousSongs [ 5 ] = previousSongs [ 6 ];
					previousSongs [ 6 ] = currentSongNumber;
			
				} else {

					if ( startupManager.preferences.loop == true )
					{

						if ( manager.audio.clip == null )
						{

							currentSongNumber = 0;

							previousSongs [ 0 ] = previousSongs [ 1 ];
							previousSongs [ 1 ] = previousSongs [ 2 ];
							previousSongs [ 2 ] = previousSongs [ 3 ];
							previousSongs [ 3 ] = previousSongs [ 4 ];
							previousSongs [ 4 ] = previousSongs [ 5 ];
							previousSongs [ 5 ] = previousSongs [ 6 ];
							previousSongs [ 6 ] = currentSongNumber;
							
						} else {

							if ( manager.audio.isPlaying == true )
							{

								rtMinutes = new int ();
								rtSeconds = new int ();
								
								manager.audio.Play ();
								isPaused = false;
								wasPlaying = true;

								if ( startupManager.developmentMode == true )
									UnityEngine.Debug.Log ( "Playing audio" );
							}
						}
					} else {

						if ( startupManager.preferences.shuffle == true )
						{

							Resources.UnloadUnusedAssets ();
							
							int previousSongNumber = currentSongNumber;
							
								currentSongNumber = UnityEngine.Random.Range ( 0, activeDirectoryFiles.Length );
					
							if ( currentSongNumber == previousSongNumber && activeDirectoryFiles.Length > 1 )
							{
								
								bool shuffleOkay = false;
								while ( shuffleOkay == false )
								{
									
									currentSongNumber = UnityEngine.Random.Range ( 0, activeDirectoryFiles.Length );
									
									if ( currentSongNumber != previousSongNumber )
										shuffleOkay = true;
								}
							}

							previousSongs [ 0 ] = previousSongs [ 1 ];
							previousSongs [ 1 ] = previousSongs [ 2 ];
							previousSongs [ 2 ] = previousSongs [ 3 ];
							previousSongs [ 3 ] = previousSongs [ 4 ];
							previousSongs [ 4 ] = previousSongs [ 5 ];
							previousSongs [ 5 ] = previousSongs [ 6 ];
							previousSongs [ 6 ] = currentSongNumber;
							psPlace = 6;

						}
					}
				}
			}

			if ( activeDirectoryFiles [ currentSongNumber ].Substring ( activeDirectoryFiles [ currentSongNumber ].Length - 7 ) == "unity3d"  )
			{
				
				loadingImage.showLoadingImages = true;
				loadingImage.InvokeRepeating ( "LoadingImages", 0.25F, 0.25F );

				StartCoroutine ( LoadAssetBundle ( "file://" + activeDirectoryFiles [ currentSongNumber ]));
			} else {
									
				StartCoroutine ( PlayAudio ( activeDirectoryFiles [ currentSongNumber ] ));
			}
		}
	}


	void PreviousSong ()
	{

		if ( activeDirectoryFiles.Any ())
		{

			wasPlaying = false;
			if ( psPlace <= 0 )
			{

				currentSongNumber = UnityEngine.Random.Range ( 0, activeDirectoryFiles.Length );
			} else
			{
			
				psPlace -= 1;
				currentSongNumber = previousSongs [ psPlace ];
			}
			
			if ( currentSongNumber > activeDirectoryFiles.Length )
				currentSongNumber = 0;
			
			if ( activeDirectoryFiles [ currentSongNumber ].Substring ( activeDirectoryFiles [ currentSongNumber ].Length - 7 ) == "unity3d" )
			{

				loadingImage.showLoadingImages = true;
				loadingImage.InvokeRepeating ( "LoadingImages", 0.25F, 0.25F );

				StartCoroutine ( LoadAssetBundle ( "file://" + activeDirectoryFiles [ currentSongNumber ]));
			} else {
				
				StartCoroutine ( PlayAudio ( activeDirectoryFiles [ currentSongNumber ] ));
			}
		}
	}
	
	
	IEnumerator LoadAssetBundle ( string assetBundleToOpen )
	{
		
		timemark.text = "Loading][Loading";
		audioLocation = assetBundleToOpen;
		
		Caching.CleanCache ();
		
		songLocation = parentDirectoryFiles [ currentSongNumber ];
	
		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( assetBundleToOpen + " | " + songLocation.Substring ( songLocation.LastIndexOf ( "/" ) + 1 ));
		
		assetBundleToOpen = assetBundleToOpen.Replace ( " ", "!umpSPACE0" );
		assetBundleToOpen = WWW.EscapeURL ( assetBundleToOpen );
		assetBundleToOpen = assetBundleToOpen.Replace ( "%5c", @"\" );
		assetBundleToOpen = assetBundleToOpen.Replace ( "%2f", @"/" );
		assetBundleToOpen = assetBundleToOpen.Replace ( "%21umpSPACE0", " " );
		
		WWW wwwClient = WWW.LoadFromCacheOrDownload ( assetBundleToOpen, 0 );
		yield return wwwClient;
		
		AssetBundleRequest request = wwwClient.assetBundle.LoadAsync ( songLocation.Substring ( songLocation.LastIndexOf ( "/" ) + 1, songLocation.LastIndexOf ( "." ) - songLocation.LastIndexOf ( "/" ) - 1 ), typeof ( AudioClip ));
		yield return request;

		manager.audio.clip = request.asset as AudioClip;
		
		wwwClient.assetBundle.Unload ( false );
		Resources.UnloadUnusedAssets ();
		
		if ( manager.audio.clip.isReadyToPlay )
		{
			
			if ( slideshow == false )
			{
				
				if ( startupManager.preferences.enableTypes == true )
					audioTitle = songLocation.Substring ( songLocation.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length );
				else
					audioTitle = songLocation.Substring ( songLocation.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length, songLocation.LastIndexOf ( "." ) - songLocation.LastIndexOf ( Path.DirectorySeparatorChar ) - Path.DirectorySeparatorChar.ToString().Length );
			
				Vector2 titleSize = currentSongStyle.CalcSize ( new GUIContent ( audioTitle ));

				if ( titleSize.x > musicViewerPosition.width )
					currentSong.fontSize = Mathf.RoundToInt ( Mathf.Floor ( 32 / ( titleSize.x / musicViewerPosition.width )) - 2 );
				else
					currentSong.fontSize = 31;
			
				currentSong.text = audioTitle;
			}
				
			if ( startupManager.preferences.enablePreciseTimemark == true )
				seconds = manager.audio.clip.length;
			else
				seconds = ( int ) Math.Round ( manager.audio.clip.length );

			if ( seconds > 60 )
			{
				
				minutes = ( int ) Math.Round ( seconds )/60;
				seconds -= minutes*60;
			} else {
					
				minutes = 0;
			}
			
			rtMinutes = 00;
			rtSeconds = 00;
	
			loadingImage.showLoadingImages = false;
			manager.audio.Play ();
			isPaused = false;
			
			socketsManager.PrepareUDPMessage ( audioTitle );
			
			if ( startupManager.developmentMode == true )
				UnityEngine.Debug.Log ( "Playing audio" );
		}
		
		if ( wwwClient.error != null )
		{
			
			using ( StreamWriter writer = new StreamWriter ( startupManager.supportPath + Path.DirectorySeparatorChar + "ErrorLog.txt", true))
			{
				
				writer.WriteLine ( "[" + DateTime.Now + "] " + wwwClient.error );
			}
			
			if ( startupManager.developmentMode == true )
				UnityEngine.Debug.Log ( "Error loading! " + wwwClient.error );
			
			manager.audio.clip = null;
			Resources.UnloadUnusedAssets ();
			
			rtMinutes = 0;
			rtSeconds = 00;
			minutes = 0;
			seconds = 00;
			
			if ( slideshow == false )
			{
										
				currentSong.text = "Error loading song!";
				
				if ( startupManager.preferences.enablePreciseTimemark == true )
					timemark.text = "0:00.000][0:00.000";
				else
					timemark.text = "0:00][0:00";
			}
		}
	}


	IEnumerator PlayAudio ( string songToLoad )
	{
		
		timemark.text = "Loading][Loading";
		audioLocation = songToLoad;
	
		manager.audio.Stop ();
		
		switch ( songToLoad.Substring ( songToLoad.LastIndexOf ( "." )))
		{
			
			case ".wav" :
			audioType = AudioType.WAV;
			break;
			
			case ".WAV" :
			audioType = AudioType.WAV;
			break;
			
			case ".aif" :
			audioType = AudioType.AIFF;
			break;
			
			case ".aiff" :
			audioType = AudioType.AIFF;
			break;
			
			case ".AIF" :
			audioType = AudioType.AIFF;
			break;
			
			case ".AIFF" :
			audioType = AudioType.AIFF;
			break;
			
			case ".ogg" :
			audioType = AudioType.OGGVORBIS;
			break;
			
			case ".OGG" :
			audioType = AudioType.OGGVORBIS;
			break;
			
			default:
			audioType = AudioType.UNKNOWN;
			break;
		}
		
		if ( slideshow == false )
		{
			
			if ( startupManager.preferences.enableTypes == true )
				audioTitle = songToLoad.Substring ( songToLoad.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length );
			else
				audioTitle = songToLoad.Substring ( songToLoad.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length, songToLoad.LastIndexOf ( "." ) - songToLoad.LastIndexOf ( Path.DirectorySeparatorChar ) - Path.DirectorySeparatorChar.ToString().Length );
			
			Vector2 titleSize = currentSongStyle.CalcSize ( new GUIContent ( audioTitle ));

			if ( titleSize.x > musicViewerPosition.width )
				currentSong.fontSize = Mathf.RoundToInt ( Mathf.Floor ( 32 / ( titleSize.x / musicViewerPosition.width )) - 2 );
			else
				currentSong.fontSize = 31;
			
			currentSong.text = audioTitle;
		}
		
		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( "Loading: " + songToLoad.Substring ( songToLoad.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length ));
		
		songToLoad = songToLoad.Replace ( " ", "!umpSPACE0" );
		songToLoad = WWW.EscapeURL ( songToLoad );
		songToLoad = songToLoad.Replace ( "%5c", @"\" );
		songToLoad = songToLoad.Replace ( "%2f", @"/" );
		songToLoad = songToLoad.Replace ( "%21umpSPACE0", " " );

		WWW www = new WWW ( "file://" + songToLoad );
		yield return www;

		manager.audio.clip = www.GetAudioClip ( false, false, audioType );
		
		Resources.UnloadUnusedAssets ();

		if ( slideshow == false )
		{
			
			if ( startupManager.preferences.enablePreciseTimemark == true )
				seconds = manager.audio.clip.length;
			else
				seconds = ( int ) Math.Round ( manager.audio.clip.length );
			
			if ( seconds > 60 )
			{
	
				minutes = ( int ) Math.Round ( seconds )/60;
				seconds -= minutes*60;
			} else
			{
				
				minutes = 0;
			}
	
			if ( startupManager.preferences.enablePreciseTimemark == true )
			{
	
				if ( manager.audio.isPlaying == true )
				{
	
					rtSeconds = manager.audio.time;
					seconds = manager.audio.clip.length;
	
					if ( seconds >= 60 )
					{
		
						minutes = ( int ) Math.Round ( seconds )/60;
						seconds -= minutes*60;
					}
	
					timemark.text = rtMinutes + ":" + String.Format ( "{0:00.000}", rtSeconds ) + "][" + minutes + ":" + String.Format ( "{0:00.000}", seconds );
				} else
				{
	
					timemark.text = "0:00.000][0:00.000";
				}
			} else
			{
	
				if ( manager.audio.isPlaying == true )
				{
	
					rtSeconds = ( int ) Math.Round ( manager.audio.time );
					seconds = ( int ) Math.Round ( manager.audio.clip.length );
	
					if ( seconds >= 60 )
					{
	
						minutes = ( int ) Math.Round ( seconds )/60;
						seconds -= minutes*60;
					}
	
					timemark.text = rtMinutes + ":" + String.Format ( "{0:00}", rtSeconds ) + "][" + minutes + ":" + String.Format ( "{0:00}", seconds );
				} else
				{
	
					timemark.text = "0:00][0:00";
				}
			}
		}
	
		if ( manager.audio.clip.isReadyToPlay )
		{
				
			rtMinutes = 00;
			rtSeconds = 00;
	
			manager.audio.Play ();
			isPaused = false;
			wasPlaying = true;
			
			socketsManager.PrepareUDPMessage ( audioTitle );
			if ( hideGUI == true && startupManager.preferences.enableHideGUINotifications == true )
			{
				
				GameObject.FindGameObjectWithTag ( "NotificationManager" ).GetComponent<NotificationManager>().Message ( "Now Playing '" + audioTitle + "'", true, false, 3.0f, true );
			}
				
			if ( startupManager.developmentMode == true )
				UnityEngine.Debug.Log ( "Playing audio" );
		}

		if ( www.error != null )
		{
			
			using ( StreamWriter writer = new StreamWriter ( startupManager.supportPath + Path.DirectorySeparatorChar + "ErrorLog.txt", true))
			{
				
				writer.WriteLine ( "[" + DateTime.Now + "] " + www.error );
			}
			
			if ( startupManager.developmentMode == true )
				UnityEngine.Debug.Log ( "Error loading! " + www.error );
			
			manager.audio.clip = null;
			Resources.UnloadUnusedAssets ();
			
			rtMinutes = 0;
			rtSeconds = 00;
			minutes = 0;
			seconds = 00;
			
			if ( slideshow == false )
			{
										
				currentSong.text = "Error loading song!";
				
				if ( startupManager.preferences.enablePreciseTimemark == true )
					timemark.text = "0:00.000][0:00.000";
				else
					timemark.text = "0:00][0:00";
			}
		}
	}


	void Update ()
	{
		
		if ( new Rect (( musicViewerPosition.width - 240 ) / 2 , 0, 240, 36 ).Contains ( Input.mousePosition ) && startupManager.showUnderlay == false )
		{
			
			float bottomBarYUp = Mathf.SmoothDamp ( bottomBarPosition.y, musicViewerPosition.height - 36, ref bottomBarVelocity, 0.05f );
			bottomBarPosition = new Rect (( musicViewerPosition.width - 240 ) / 2 , bottomBarYUp, 240, 64 );
		} else {
			
			float bottomBarYDown = Mathf.SmoothDamp ( bottomBarPosition.y, musicViewerPosition.height - 18, ref bottomBarVelocity, 0.05f );
			bottomBarPosition = new Rect (( musicViewerPosition.width - 240 ) / 2 , bottomBarYDown, 240, 54 );
		}
		
		if ( Input.GetKeyDown ( KeyCode.RightArrow ))
		{
			
			paneManager.MoveToOMB ();
		}
		
		if ( Input.GetKeyDown ( KeyCode.LeftArrow ))
		{
			
			paneManager.MoveToMV ();
		}

		if ( Input.GetKeyDown ( KeyCode.DownArrow ))
		{
			
			NextSong ();
			loadingImage.showLoadingImages = true;
		}

		if ( Input.GetKeyDown ( KeyCode.UpArrow ))
		{
			
			PreviousSong ();
			loadingImage.showLoadingImages = true;
		}

		if ( Input.GetKeyDown ( KeyCode.Space ))
		{
			
			if (isPaused == false )
			{
				
				manager.audio.Pause ();
				pausePoint = manager.audio.time;
				isPaused = true;
				
			} else {
					
				manager.audio.Play ();
				manager.audio.time = pausePoint;
				isPaused = false;
			}
		}
		
		if ( Input.GetKeyDown ( KeyCode.Escape ))
		{
				
			if ( slideshow == true )
			{
				
				slideshow = false;
				StartCoroutine ( "SetArtwork" );
				
				if ( startupManager.preferences.enableTimebar == false )
					musicViewerTitle = "MusicViewer";
				
				timemark.enabled = true;
				hideGUI = false;
				
				manager.GetComponent<BlurEffect> ().enabled = startupManager.preferences.blur;
				
				if ( manager.audio.clip != null )
				{
            	
					if ( startupManager.preferences.enableTypes == true )
						currentSong.text = audioLocation.Substring ( audioLocation.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length );
					else
						currentSong.text = audioLocation.Substring ( audioLocation.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length, audioLocation.LastIndexOf ( "." ) - audioLocation.LastIndexOf ( Path.DirectorySeparatorChar ) - Path.DirectorySeparatorChar.ToString().Length );
					
				} else {
					
					currentSong.text = "UnityMusicPlayer";
					if ( startupManager.preferences.enablePreciseTimemark == true )
						timemark.text = "0:00.000][0:00.000";
					else
						timemark.text = "0:00][0:00";
				}
				
				StopCoroutine ( "LoadSlideshow" );
				newSlideshowImage = null;
				currentSlideshowImage.pixelInset = new Rect ( -300, -300, 600, 600 );
				currentSlideshowImage.texture = null;
				currentSlideshowImage.color = new Color ( 0.5f, 0.5f, 0.5f, 0.1f );
				slideshowImage = 0;
				fadeIn = false;
				
				Resources.UnloadUnusedAssets ();
			}
		}
		
		if ( startupManager.preferences.enableKeybinds == true )
		{
			
			if ( Input.GetKeyDown ( KeyCode.H ))
			{
				
				hideGUI = !hideGUI;
			}
			
			if ( Input.GetKeyDown ( KeyCode.A ))
			{
				
				showVisualizer = !showVisualizer;
				
				if ( showVisualizer == true )
				{
		
					audioVisualizer.topLeftLine.material.color = new Color ( startupManager.preferences.avcR, startupManager.preferences.avcG, startupManager.preferences.avcB, 255 );
					audioVisualizer.bottomLeftLine.material.color = new Color ( startupManager.preferences.avcR, startupManager.preferences.avcG, startupManager.preferences.avcB, 255 );
					audioVisualizer.topRightLine.material.color = new Color ( startupManager.preferences.avcR, startupManager.preferences.avcG, startupManager.preferences.avcB, 255 );
					audioVisualizer.bottomRightLine.material.color = new Color ( startupManager.preferences.avcR, startupManager.preferences.avcG, startupManager.preferences.avcB, 255 );
		
					manager.GetComponent<BloomAndLensFlares>().enabled = startupManager.preferences.bloom;
					manager.GetComponent<BlurEffect>().enabled = startupManager.preferences.blur;
					manager.GetComponent<SunShafts>().enabled = startupManager.preferences.sunShafts;
					manager.GetComponent<Vignetting>().enabled = startupManager.preferences.vignetting;
				
					manager.GetComponent<BlurEffect> ().iterations = startupManager.preferences.blurIterations;
				} else {
		
					manager.GetComponent<BloomAndLensFlares>().enabled = false;
					manager.GetComponent<BlurEffect>().enabled = false;
					manager.GetComponent<SunShafts>().enabled = false;
					manager.GetComponent<Vignetting>().enabled = false;
				}
			}
			
			if ( Input.GetKeyDown ( KeyCode.F ))
			{
				
				doubleSpeed = !doubleSpeed;
				
				if ( doubleSpeed == true )
				{
					
					manager.audio.pitch = 2.0F;
			    	
					if ( doubleSpeed == true && halfSpeed == true )
						halfSpeed = false;
				}
				
				if ( halfSpeed == false && doubleSpeed == false )
					manager.audio.pitch = 1.0F;
			}
			
			if ( Input.GetKeyDown ( KeyCode.S ))
			{
				
				halfSpeed = !halfSpeed;
				
				if ( halfSpeed == true )
				{
					
					manager.audio.pitch = 0.5F;
			    	
					if ( doubleSpeed == true && halfSpeed == true )
						doubleSpeed = false;
				}
				
				if ( halfSpeed == false && doubleSpeed == false )
					manager.audio.pitch = 1.0F;
			}
			
			if ( Input.GetKeyDown ( KeyCode.E ))
			{
				
				echo = !echo;
				
				if ( echo == true )
				{
					
					manager.GetComponent<AudioEchoFilter> ().enabled = true;
				} else {
				    manager.GetComponent<AudioEchoFilter> ().enabled = false;
				}
			}
			
			/*if ( Input.GetKeyDown ( KeyCode.O ))
			{
				
				paneManager.MoveToOMB ();
			}
			
			if ( Input.GetKeyDown ( KeyCode.M ))
			{
				
				paneManager.MoveToMV ();
			}*/
		}
			
		if ( manager.audio.isPlaying == true )
		{
				
			if ( manager.audio.time >= manager.audio.clip.length )
			{
					
				if ( startupManager.developmentMode == true )
					UnityEngine.Debug.Log ( manager.audio.time + "  :  " + manager.audio.clip.length );

				wasPlaying = false;
				if ( startupManager.preferences.continuous == true || startupManager.preferences.loop == true )
					SongEnd ();
				else
					Invoke ( "SongEnd", betweenSongDelay );
			}
		} else {
				
			if ( wasPlaying == true )
			{
					
				if ( isPaused == false )	
				{
					
					if ( startupManager.developmentMode == true )
						UnityEngine.Debug.Log ( "Is not playing, was playing, is not paused" );

					wasPlaying = false;
					if ( startupManager.preferences.continuous == true || startupManager.preferences.loop == false && startupManager.preferences.shuffle == false )
						Invoke ( "SongEnd", betweenSongDelay );
					else
						SongEnd ();
				}
			}
		}	
		
		if ( slideshow == false )
		{
		
			manager.audio.volume = startupManager.preferences.volumebarValue;
			
			if ( manager.audio.clip != null )
			{

				if ( manager.audio.isPlaying == true )
				{
						
					if ( startupManager.preferences.enablePreciseTimemark == true )
					{
		
						rtSeconds = manager.audio.time;

						if ( rtSeconds >= 60 )
						{
					
							rtMinutes = ( int ) Math.Round ( rtSeconds )/60;
							rtSeconds -= rtMinutes*60;
						}

						timemark.text = rtMinutes + ":" + String.Format ( "{0:00.000}", rtSeconds ) + "][" + minutes + ":" + String.Format ( "{0:00.000}", seconds );
					} else {
							
						rtSeconds = ( int ) Math.Round ( manager.audio.time );

						if ( rtSeconds >= 60 )
						{
				
							rtMinutes = ( int ) Math.Round ( rtSeconds )/60;
							rtSeconds -= rtMinutes*60;
						}

						timemark.text = rtMinutes + ":" + String.Format ( "{0:00}", rtSeconds ) + "][" + minutes + ":" + String.Format ( "{0:00}", seconds);
					}
				}
			} else {
				
				if ( startupManager.preferences.enablePreciseTimemark == true )
				{
					
					timemark.text = "0:00.000][0:00.000";
				} else {
					
					timemark.text = "0:00][0:00";
				}
			}
		} else {

			if ( fadeIn == true )
			{
	
				float smoothDampIn = Mathf.SmoothDamp ( currentSlideshowImage.color.a, 1.0F, ref fadeVelocity, 2 );
				currentSlideshowImage.color = new Color ( 0.5F, 0.5F, 0.5F, smoothDampIn );
		
				if ( currentSlideshowImage.color.a > 0.98F )
				{
	
					currentSlideshowImage.color = new Color ( 0.5F, 0.5F, 0.5F, 1.0F );
					fadeIn = false;
					
					StartCoroutine ( "LoadSlideshow", false );
				}
			}

			if ( fadeOut == true )
			{

				float smoothDampOut = Mathf.SmoothDamp ( currentSlideshowImage.color.a, 0.0F, ref fadeVelocity, 2 );
				currentSlideshowImage.color = new Color ( 0.5F, 0.5F, 0.5F, smoothDampOut );

				if ( currentSlideshowImage.color.a < 0.02F )
				{

					currentSlideshowImage.color = new Color ( 0.5F, 0.5F, 0.5F, 0.0F );
					fadeOut = false;
					StartCoroutine ( "SlideshowIN" );
				}
			}
		}
		
		if ( showVisualizer == true && isPaused == false )
		{
			
			if ( startupManager.preferences.iterateEffects == true )
			{
				
				float currentPerlin = Mathf.PerlinNoise ( Time.time * 1.0f, 0.0f );
				
				manager.GetComponent<SunShafts> ().maxRadius = currentPerlin;
				//manager.GetComponent<BloomAndLensFlares> ().bloomThreshhold = Mathf.PerlinNoise ( Time.time * 1.0f, 0.0f );
				//manager.GetComponent<BlurEffect> ().blurSpread = Mathf.PerlinNoise ( Time.time * 10.0f, 0.0f );
			} else {
				
				manager.GetComponent<SunShafts> ().maxRadius = 0.25f;
			}
		}
	}


	void SongEnd ()
	{

		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( "AudioClips in Memory: " + Resources.FindObjectsOfTypeAll ( typeof ( AudioClip )).Length );
		
		if ( startupManager.preferences.loop == true )
		{
			
			rtMinutes = new int ();
			rtSeconds = new int ();
				
			manager.audio.Play ();
			isPaused = false;
			wasPlaying = true;
				
			if ( startupManager.developmentMode == true )
				UnityEngine.Debug.Log ( "Playing audio" );
				
		} else {
			
			try {
			
				if ( activeDirectoryFiles.Any ())
				{		
				
					psPlace = 6;
        	
					if ( startupManager.preferences.continuous == true )
					{
					
						if ( currentSongNumber == activeDirectoryFiles.Length - 1 )
							currentSongNumber = 0;
						else
							currentSongNumber++;
					
						previousSongs [ 0 ] = previousSongs [ 1 ];
						previousSongs [ 1 ] = previousSongs [ 2 ];
						previousSongs [ 2 ] = previousSongs [ 3 ];
						previousSongs [ 3 ] = previousSongs [ 4 ];
						previousSongs [ 4 ] = previousSongs [ 5 ];
						previousSongs [ 5 ] = previousSongs [ 6 ];
						previousSongs [ 6 ] = currentSongNumber;
					} else {
						
						if ( startupManager.preferences.shuffle == true )
						{
							
							if ( activeDirectoryFiles.Length > 1 )
							{
							
								Resources.UnloadUnusedAssets ();
								
								int previousSongNumber = currentSongNumber;
								currentSongNumber = UnityEngine.Random.Range ( 0, activeDirectoryFiles.Length );
							
								if ( currentSongNumber == previousSongNumber && activeDirectoryFiles.Length > 1 )
								{
									
									bool shuffleOkay = false;
									while ( shuffleOkay == false )
									{
										
										currentSongNumber = UnityEngine.Random.Range ( 0, activeDirectoryFiles.Length );
										
										if ( currentSongNumber != previousSongNumber )
											shuffleOkay = true;
									}
								}
        	        		
								previousSongs [ 0 ] = previousSongs [ 1 ];
								previousSongs [ 1 ] = previousSongs [ 2 ];
								previousSongs [ 2 ] = previousSongs [ 3 ];
								previousSongs [ 3 ] = previousSongs [ 4 ];
								previousSongs [ 4 ] = previousSongs [ 5 ];
								previousSongs [ 5 ] = previousSongs [ 6 ];
								previousSongs [ 6 ] = currentSongNumber;
								psPlace = 6;
								} else {
									
									rtMinutes = new int ();
									rtSeconds = new int ();
					
									manager.audio.Play ();
									isPaused = false;
									wasPlaying = true;
					
									if ( startupManager.developmentMode == true )
										UnityEngine.Debug.Log ( "Playing audio" );
								}
						} else {
        	
							manager.audio.clip = null;
							Resources.UnloadUnusedAssets ();
						
							rtMinutes = 0;
							rtSeconds = 00;
							minutes = 0;
							seconds = 00;
							
							audioLocation = "";
        	
							if ( slideshow == false )
							{
								
								currentSong.text = "UnityMusicPlayer";
							
								if ( startupManager.preferences.enablePreciseTimemark == true )
									timemark.text = "0:00.000][0:00.000";
								else
									timemark.text = "0:00][0:00";
							}
						}
					}
				}
			
				if ( startupManager.preferences.continuous == true || startupManager.preferences.shuffle == true )
				{
				
					if ( activeDirectoryFiles [ currentSongNumber ].Substring ( activeDirectoryFiles [ currentSongNumber ].Length - 7 ) == "unity3d"  )
					{
			
						loadingImage.showLoadingImages = true;
						loadingImage.InvokeRepeating ( "LoadingImages", 0.25F, 0.25F );

						StartCoroutine ( LoadAssetBundle ( "file://" + activeDirectoryFiles [ currentSongNumber ]));
					} else {
									
						StartCoroutine ( PlayAudio ( activeDirectoryFiles [ currentSongNumber ] ));
					}
				}
			} catch ( Exception e )
			{
					
				UnityEngine.Debug.Log ( e );
					
				manager.audio.clip = null;
				Resources.UnloadUnusedAssets ();
				
				rtMinutes = 0;
				rtSeconds = 00;
				minutes = 0;
				seconds = 00;
					
				audioLocation = "";
						
				currentSong.text = "UnityMusicPlayer";
				
				if ( startupManager.preferences.enablePreciseTimemark == true )
					timemark.text = "0:00.000][0:00.000";
				else
					timemark.text = "0:00][0:00";
			}
		}
	}


	internal void Quit ()
	{

		wasPlaying = false;
		manager.audio.Stop ();
		
		socketsManager.PrepareUDPMessage ( "Shutting Down" );
		
		Resources.UnloadUnusedAssets ();
		Caching.CleanCache ();
		
		bool preferencesSaved = false;
		preferencesSaved = startupManager.SavePreferences ();
		while ( preferencesSaved == false ) {}
		
		if ( Application.isEditor == true )
			UnityEditor.EditorApplication.isPlaying = false;
		else
			Application.Quit ();
	}
}
