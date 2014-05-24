using System;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
//Written by Gibson Bethke
//Thank you for saving me, Jesus!
//Thank you for living in me, Spirit!
//Thank you for making me, Father!
public class MusicViewer : MonoBehaviour
{
	
#region Variables

	internal GameObject manager;
	SocketsManager socketsManager;
	StartupManager startupManager;
	OnlineMusicBrowser onlineMusicBrowser;
	LoadingImage loadingImage;
	PaneManager paneManager;
	AudioVisualizerR audioVisualizerR;
	AudioVisualizerL audioVisualizerL;
	
	GUIStyle fileStyle;
	GUIStyle folderStyle;
	GUIStyle fileBrowserFileStyle;
	public Texture2D folderIcon;
	public Texture2D musicNoteIcon;
	
	public Texture2D playIcon;
	public Texture2D pauseIcon;
	
	public Texture2D leftArrow;
	public Texture2D rightArrow;
	
	internal bool showMusicViewer = true;
	bool fileBrowser = false;

	string musicViewerTitle;
	internal Rect musicViewerPosition = new Rect ( 0, 0, 800, 600 );

	bool showVisualizer = false;
	bool halfSpeed = false;
	bool doubleSpeed = false;
	
	string songInfoOwner;
	bool showFolderMusic = false;
	
	internal string mediaPath;
	String[] currentDirectories;
	string songLocation;
	
	string tempCurrentDirectory;
	string currentDirectory;
	string [] currentDirectoryDirectories;
	string [] currentDirectoryFiles;
	string [] childDirectoryFiles;
	
	WWW wWw;
	
	public GUIText currentSong;	
	public Texture2D timebarMarker;
	internal GUIText timemark;
	float timebarTime;
	
	bool isPaused;	
	float pausePoint;
	int songTime;
	int minutes;
	float seconds;
	int rtMinutes;
	float rtSeconds;
	
	AudioType audioType;
	string audioTitle;

	internal bool wasPlaying = false;
	float betweenSongDelay = 0.5F;
	
	int currentSongNumber = -1;
	int i;
	
	Vector2 scrollPosition;
	Vector2 mousePos;
	
	bool hideGUI = false;
	public GUISkin guiSkin;
	GUIStyle centerStyle;
	GUIStyle labelStyle;
	GUIStyle songStyle;
	GUIStyle buttonStyle;
	public Texture2D guiHover;
	public Texture2D guiActiveHover;
	
	bool close = false;
	
	int[] previousSongs = new int  [ 7 ] { 0, 0, 0, 0, 0, 0, 0 };
	int psPlace = 6;
	
	string audioLocation;

#region EffectsSettings
	
	bool loop;
	bool shuffle;
	bool continuous;
	
	float volumeBarValue;
	
	bool echo;
	string echoDelay;
	string tempEchoDelay;
	string echoDecayRate;
	string tempEchoDecayRate;
	string echoWetMix;
	string tempEchoWetMix;
	string echoDryMix;
	string tempEchoDryMix;

	bool showOptionsWindow = false;
	Rect optionsWindowRect = new Rect ( 0, 0, 350, 410 );
	
#endregion
	
#region AVSettings
	
	float avcR;
	float tempAVCR;
	float avcG;
	float tempAVCG;
	float avcB;
	float tempAVCB;
	
	float tempBloom;
	bool bloom;
	
	float tempBlur;
	bool blur;
	
	float tempSunShafts;
	bool sunShafts;
	
	string blurIterations;
	string tempBlurIterations;
	
#endregion
	
#region GeneralSettings
	
	float tempShowTypes;
	bool showTypes;
	
	float tempShowTimebar;
	internal bool showTimebar;
	
	float tempShowArtwork;
	internal bool showArtwork;
	
	float tempShowQuickManage;
	bool showQuickManage;

	float tempPreciseTimemark;
	internal bool preciseTimemark;
	
	float tempShowArrows;
	internal bool showArrows;
	
	float tempCheckForUpdates;
	
	internal float tempEnableOMB;
	
	float tempDeepSearch;
	bool enableDeepSearch;

#endregion

#region SlideshowSettings

	float tempSlideshow = 0.0F;
	internal bool slideshow = false;
	
	float tempAutoAVBlur;
	bool autoAVBlur;
	
	float tempAutoAVOff;
	bool autoAVOff;

#endregion

#region SlideshowMechanics

	string[] slideshowImageLocations;
	internal GUITexture currentSlideshowImage;
	Texture2D newSlideshowImage;
	float fadeVelocity = 0.0F;
	string displayTime;
	bool fadeIn = false;
	bool fadeOut = false;

	int slideshowImage = 0;
	
#endregion	
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

		musicViewerPosition.width = Screen.width;
		musicViewerPosition.height = Screen.height;

		optionsWindowRect.x = musicViewerPosition.width/2 - optionsWindowRect.width/2;
		optionsWindowRect.y = musicViewerPosition.height/2 - optionsWindowRect.height/2;

		audioVisualizerR = GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizerR> ();
		audioVisualizerL = GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizerL> ();

		timemark = GameObject.FindGameObjectWithTag ( "Timemark" ).GetComponent<GUIText> ();
		GameObject.FindGameObjectWithTag ( "TimebarImage" ).guiTexture.pixelInset = new Rect ( -musicViewerPosition.width/2, musicViewerPosition.height/2 - 3, musicViewerPosition.width, 6 );

		currentDirectory = startupManager.lastDirectory;
				
		tempCheckForUpdates = Convert.ToSingle ( startupManager.checkForUpdate );
		
		tempEnableOMB = Convert.ToSingle ( startupManager.ombEnabled );
			
		loop = Convert.ToBoolean ( startupManager.prefs [ 4 ] );
		shuffle = Convert.ToBoolean ( startupManager.prefs [ 5 ] );
		continuous = Convert.ToBoolean ( startupManager.prefs [ 6 ] );

		showTypes = Convert.ToBoolean ( startupManager.prefs [ 7 ] );
		tempShowTypes = Convert.ToSingle ( showTypes );
		
		showArrows = Convert.ToBoolean ( startupManager.prefs [ 8 ] );
		tempShowArrows = Convert.ToSingle ( showArrows );
		
		showTimebar = Convert.ToBoolean ( startupManager.prefs [ 9 ] );
		tempShowTimebar = Convert.ToSingle ( showTimebar );
		
		showArtwork = Convert.ToBoolean ( startupManager.prefs [ 10 ] );
		tempShowArtwork = Convert.ToSingle ( showArtwork );
		
		enableDeepSearch = Convert.ToBoolean ( startupManager.prefs [ 11 ]);
		tempDeepSearch = Convert.ToSingle ( enableDeepSearch );
		
		showQuickManage = Convert.ToBoolean ( startupManager.prefs [ 12 ] );
		tempShowQuickManage = Convert.ToSingle ( showQuickManage );
		
		preciseTimemark = Convert.ToBoolean ( startupManager.prefs [ 13 ] );
		tempPreciseTimemark = Convert.ToSingle ( preciseTimemark );
		
		volumeBarValue = Convert.ToSingle ( startupManager.prefs [ 14 ] );

		avcR = float.Parse ( startupManager.prefs [ 15 ] );
		tempAVCR = avcR;
		
		avcG = float.Parse ( startupManager.prefs [ 16 ] );
		tempAVCG = avcG;
		
		avcB = float.Parse ( startupManager.prefs [ 17 ] );
		tempAVCB = avcB;

		bloom = Convert.ToBoolean ( startupManager.prefs [ 18 ] );	
		tempBloom = Convert.ToSingle ( bloom );
		
		blur = Convert.ToBoolean ( startupManager.prefs [ 19 ] );
		tempBlur = Convert.ToSingle ( blur );
		
		sunShafts = Convert.ToBoolean ( startupManager.prefs [ 20 ] );
		tempSunShafts = Convert.ToSingle ( sunShafts );
		
		blurIterations = Convert.ToString ( startupManager.prefs [ 21 ] );
		tempBlurIterations = blurIterations;
		
		manager.GetComponent<BlurEffect> ().iterations = Convert.ToInt16 ( blurIterations );

		echoDelay = startupManager.prefs [ 22 ];
		tempEchoDelay = echoDelay;
		echoDecayRate = startupManager.prefs [ 23 ];
		tempEchoDecayRate = echoDecayRate;
		echoWetMix = startupManager.prefs [ 24 ];
		tempEchoWetMix = echoWetMix;
		echoDryMix = startupManager.prefs [ 25 ];
		tempEchoDryMix = echoDryMix;
		
		manager.GetComponent<AudioEchoFilter> ().delay = Convert.ToSingle ( echoDelay );
		manager.GetComponent<AudioEchoFilter> ().decayRatio = Convert.ToSingle ( echoDecayRate );
		manager.GetComponent<AudioEchoFilter> ().wetMix = Convert.ToSingle ( echoWetMix );
		manager.GetComponent<AudioEchoFilter> ().dryMix = Convert.ToSingle ( echoDryMix );
		
		autoAVBlur = Convert.ToBoolean ( startupManager.prefs [ 26 ] );
		tempAutoAVBlur = Convert.ToSingle ( autoAVBlur );
		
		autoAVOff = Convert.ToBoolean ( startupManager.prefs [ 27 ] );
		tempAutoAVOff = Convert.ToSingle ( autoAVOff );
		
		displayTime = startupManager.prefs [ 28 ];
		
		currentSong.text = "UnityMusicPlayer";
		GameObject.FindGameObjectWithTag ( "TimebarImage" ).guiTexture.enabled = showTimebar;

		if ( showTimebar == true )
		{

			musicViewerTitle = "";
			onlineMusicBrowser.onlineMusicBrowserTitle = "";
		} else {

			musicViewerTitle = "MusicViewer";
			onlineMusicBrowser.onlineMusicBrowserTitle = "OnlineMusicBrowser";
		}

		if ( preciseTimemark == true )
			timemark.text = "0:00.000][0:00.000";
		else
			timemark.text = "0:00][0:00";

		TextWriter savePrefs = new StreamWriter ( startupManager.prefsLocation );
		savePrefs.WriteLine ( currentDirectory + "\n" + startupManager.checkForUpdate + "\n" + startupManager.ombEnabled + "\n" + startupManager.showTutorials + "\n" + loop + "\n" + shuffle + "\n" + continuous + "\n" + showTypes + "\n" + showArrows + "\n" + showTimebar + "\n" + showArtwork + "\n" + enableDeepSearch + "\n" + showQuickManage + "\n" + preciseTimemark + "\n" + volumeBarValue + "\n" + avcR + "\n" + avcG + "\n" + avcB + "\n" + bloom + "\n" + blur + "\n" + sunShafts + 
		                     "\n" + blurIterations + "\n" + echoDelay + "\n" + echoDecayRate + "\n" + echoWetMix + "\n" + echoDryMix + "\n" + autoAVBlur + "\n" + autoAVOff + "\n" + displayTime );
		savePrefs.Close ();
		
		centerStyle = new GUIStyle ();
		centerStyle.alignment = TextAnchor.MiddleCenter;
		
		labelStyle = new GUIStyle ();
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.wordWrap = true;
		
		folderStyle = new GUIStyle ();
		folderStyle.alignment = TextAnchor.MiddleLeft;
		folderStyle.border = new RectOffset ( 6, 6, 4, 4 );
		folderStyle.hover.background = guiHover;
		folderStyle.fontSize = 22;
		
		fileStyle = new GUIStyle ();
		fileStyle.alignment = TextAnchor.MiddleLeft;
		fileStyle.border = new RectOffset ( 6, 6, 6, 4 );
		fileStyle.padding = new RectOffset ( 6, 6, 3, 3 );
		fileStyle.margin = new RectOffset ( 4, 4, 4, 4 );
		fileStyle.hover.background = guiHover;
		fileStyle.fontSize = 22;
		
		fileBrowserFileStyle = new GUIStyle ();
		fileBrowserFileStyle.alignment = TextAnchor.MiddleLeft;
		
		songStyle = new GUIStyle ();
		songStyle.alignment = TextAnchor.MiddleLeft;
		songStyle.fontSize = 16;
		
		buttonStyle = new GUIStyle ();
		buttonStyle.fontSize = 16;
		buttonStyle.alignment = TextAnchor.MiddleCenter;
		buttonStyle.border = new RectOffset ( 6, 6, 4, 4 );
		buttonStyle.hover.background = guiHover;
		
		InvokeRepeating ( "Refresh", 0, 2 );
		StartCoroutine ( SetArtwork ());
	}
	
	
	void Refresh ()
	{

		if ( paneManager.currentPane == PaneManager.pane.musicViewer )
		{
			
			try
			{
				
				if ( fileBrowser == false )
				{
					
					currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
					currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
					try { childDirectoryFiles = Directory.GetFiles ( songInfoOwner, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray (); } catch { childDirectoryFiles = new string[0]; }
				} else {
						
					currentDirectoryDirectories = Directory.GetDirectories ( tempCurrentDirectory ).ToArray ();
					currentDirectoryFiles = Directory.GetFiles ( tempCurrentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
					try { childDirectoryFiles = Directory.GetFiles ( songInfoOwner, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray (); } catch { childDirectoryFiles = new string[0]; }
				}
			} catch ( Exception e ) {
			
				if ( startupManager.developmentMode == true )
					UnityEngine.Debug.LogWarning ( e );
				
				currentDirectory = startupManager.mediaPath;
			
				currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
				currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
				
			}
		}
	}


	void OptionsWindow ( int wid )
	{

		GUI.FocusWindow ( 5 );
		GUI.BringWindowToFront ( 5 );

		if ( GUI.Button ( new Rect ( 280, 20, 60, 20 ), "Close" ) || close == true)
		{

			if ( tempEchoDelay.Trim () == "" )
				tempEchoDelay = "100";
				
			echoDelay = tempEchoDelay;

			if ( tempEchoDecayRate.Trim () == "" )
				tempEchoDecayRate = "0.3";
				
			echoDecayRate = tempEchoDecayRate;

			if ( tempEchoWetMix.Trim () == "" )
				tempEchoWetMix = "0.8";
				
			echoWetMix = tempEchoWetMix;

			if ( tempEchoDryMix.Trim () == "" )
				tempEchoDryMix = "0.6";
				
			echoDryMix = tempEchoDryMix;

			manager.GetComponent<AudioEchoFilter> ().delay = Convert.ToSingle ( echoDelay );
			manager.GetComponent<AudioEchoFilter> ().decayRatio = Convert.ToSingle ( echoDecayRate );
			manager.GetComponent<AudioEchoFilter> ().wetMix = Convert.ToSingle ( echoWetMix );
			manager.GetComponent<AudioEchoFilter> ().dryMix = Convert.ToSingle ( echoDryMix );

			avcR = tempAVCR;
			avcG = tempAVCG;
			avcB = tempAVCB;
			
			sunShafts = Convert.ToBoolean ( tempSunShafts );
			bloom = Convert.ToBoolean ( tempBloom );
			blur = Convert.ToBoolean ( tempBlur );
			
			if ( tempBlurIterations.Trim () == "" )
				tempBlurIterations = "3";
				
			blurIterations = tempBlurIterations;
				
			manager.GetComponent<BlurEffect> ().iterations = Convert.ToInt16 ( blurIterations );
			
			showTypes = Convert.ToBoolean ( tempShowTypes );
			if ( manager.audio.clip != null )
			{

				if ( showTypes == true )
					currentSong.text = audioLocation.Substring ( audioLocation.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length );
				else
					currentSong.text = audioLocation.Substring ( audioLocation.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length, audioLocation.LastIndexOf ( "." ) - audioLocation.LastIndexOf ( Path.DirectorySeparatorChar ) - Path.DirectorySeparatorChar.ToString().Length );
			}

			showArrows = Convert.ToBoolean ( tempShowArrows );
			
			showTimebar = Convert.ToBoolean ( tempShowTimebar );
			if ( showTimebar == true )
			{

				GameObject.FindGameObjectWithTag ( "TimebarImage" ).guiTexture.enabled = true;
				musicViewerTitle = "";
				onlineMusicBrowser.onlineMusicBrowserTitle = "";
			} else {

				GameObject.FindGameObjectWithTag ( "TimebarImage" ).guiTexture.enabled = false;
				musicViewerTitle = "MusicViewer";
				onlineMusicBrowser.onlineMusicBrowserTitle = "OnlineMusicBrowser";
			}
			
			showArtwork = Convert.ToBoolean ( tempShowArtwork );
			
			if ( showArtwork == true )
				StartCoroutine ( SetArtwork ());
			else
				currentSlideshowImage.texture = null;
			
			enableDeepSearch = Convert.ToBoolean ( tempDeepSearch );

			showQuickManage = Convert.ToBoolean ( tempShowQuickManage );
			
			preciseTimemark = Convert.ToBoolean ( tempPreciseTimemark );
			
			startupManager.checkForUpdate = Convert.ToBoolean ( tempCheckForUpdates );
			
			if ( startupManager.ombEnabled == false && Convert.ToBoolean ( tempEnableOMB ) == true )
			{
				
				startupManager.SendMessage ( "RefreshOMB" );
				paneManager.loading = true;
			}
			
			startupManager.ombEnabled = Convert.ToBoolean ( tempEnableOMB );
			
			if ( displayTime.Trim () == "" )
				displayTime = "2.0";

			slideshow = Convert.ToBoolean ( tempSlideshow );
			autoAVBlur = Convert.ToBoolean ( tempAutoAVBlur );
			autoAVOff = Convert.ToBoolean ( tempAutoAVOff );
			if ( slideshow == true )
			{
				
				if ( autoAVOff == true )
				{
					
					showVisualizer = false;
					
					audioVisualizerR.showAV = false;
					audioVisualizerL.showAV = false;
	
					manager.GetComponent<BloomAndLensFlares>().enabled = false;
					manager.GetComponent<BlurEffect>().enabled = false;
					manager.GetComponent<SunShafts>().enabled = false;
				} else {
					
					audioVisualizerR.showAV = showVisualizer;
					audioVisualizerL.showAV = showVisualizer;
					audioVisualizerR.topLine.material.color = new Color ( avcR, avcG, avcB, 255 );
					audioVisualizerR.bottomLine.material.color = new Color ( avcR, avcG, avcB, 255 );
					audioVisualizerL.topLine.material.color = new Color ( avcR, avcG, avcB, 255 );
					audioVisualizerL.bottomLine.material.color = new Color ( avcR, avcG, avcB, 255 );
	
					manager.GetComponent<BloomAndLensFlares>().enabled = bloom;
					if ( autoAVBlur == false )
						manager.GetComponent<BlurEffect>().enabled = blur;
					else
						manager.GetComponent<BlurEffect>().enabled = true;
						
					manager.GetComponent<SunShafts>().enabled = sunShafts;
				}

				if ( showTimebar == false )
					musicViewerTitle = "";
				
				timemark.enabled = false;
				currentSong.text = "";
				hideGUI = true;
				
				StartCoroutine ( "LoadSlideshow", true );
			}

			GUI.FocusWindow ( 0 );
			GUI.BringWindowToFront ( 0 );
			paneManager.popupBlocking = false;
			close = false;
			showOptionsWindow = false;
		}
		
		GUI.Label ( new Rect ( 163, 22, 150, 22 ), "Off|On" );
		
		tempShowTypes = GUI.HorizontalSlider ( new Rect ( 170, 45, 20, 14 ), UnityEngine.Mathf.Round ( tempShowTypes ), 0, 1 );
		GUI.Label ( new Rect ( 195, 39, 100, 22 ), "Show Types" );
		
		tempShowArrows = GUI.HorizontalSlider ( new Rect ( 170, 61, 20, 14 ), UnityEngine.Mathf.Round ( tempShowArrows ), 0, 1 );
		GUI.Label ( new Rect ( 195, 55, 100, 22 ), "Enable Arrows" );
		
		tempShowArtwork = GUI.HorizontalSlider ( new Rect ( 170, 78, 20, 14 ), UnityEngine.Mathf.Round ( tempShowArtwork ), 0, 1 );
		GUI.Label ( new Rect ( 195, 72, 100, 22 ), "Enable Artwork" );
	
		tempShowTimebar = GUI.HorizontalSlider ( new Rect ( 170, 95, 20, 14 ), UnityEngine.Mathf.Round ( tempShowTimebar ), 0, 1 );
		GUI.Label ( new Rect ( 195, 89, 100, 22 ), "Enable Timebar" );
		
		tempDeepSearch = GUI.HorizontalSlider ( new Rect ( 170, 112, 20, 14 ), UnityEngine.Mathf.Round ( tempDeepSearch ), 0, 1 );
		GUI.Label ( new Rect ( 195, 106, 145, 22 ), "Enable Deep Search" );
		
		tempShowQuickManage = GUI.HorizontalSlider ( new Rect ( 170, 129, 20, 14 ), UnityEngine.Mathf.Round ( tempShowQuickManage ), 0, 1 );
		GUI.Label ( new Rect ( 195, 123, 124, 22 ), "Enable QuickManage" );
		
		tempPreciseTimemark = GUI.HorizontalSlider ( new Rect ( 170, 146, 20, 14 ), UnityEngine.Mathf.Round ( tempPreciseTimemark ), 0, 1 );
		GUI.Label ( new Rect ( 195, 140, 150, 22 ), "Show Precise Timemark" );
		

#region AudioVisualizerSettings

		GUI.Box ( new Rect ( 10, 24, 150, 224 ), "AudioVisualizer Settings" );

		GUI.Label ( new Rect ( 35, 46, 40, 30 ), "Red" );
		tempAVCR = GUI.HorizontalSlider ( new Rect ( 35, 66, 100, 15), tempAVCR, 0.0F, 1.000F );

		GUI.Label ( new Rect ( 35, 76, 40, 30 ), "Green" );
		tempAVCG = GUI.HorizontalSlider ( new Rect ( 35, 96, 100, 15), tempAVCG, 0.0F, 1.000F );

		GUI.Label ( new Rect ( 35, 106, 40, 30 ), "Blue" );
		tempAVCB = GUI.HorizontalSlider ( new Rect ( 35, 126, 100, 15), tempAVCB, 0.0F, 1.000F );

		GUI.contentColor = new Color ( tempAVCR, tempAVCG, tempAVCB, 1.000F );
		GUI.Label ( new Rect ( 45, 143, 80, 20 ), "Sample Color");
		GUI.contentColor = Color.white;
		
		tempSunShafts = GUI.HorizontalSlider ( new Rect ( 20, 169, 20, 14 ), UnityEngine.Mathf.Round ( tempSunShafts ), 0, 1 );
		GUI.Label ( new Rect ( 45, 162, 120, 22 ), "Toggle Sun Shafts" );
		
		tempBloom = GUI.HorizontalSlider ( new Rect ( 20, 186, 20, 14 ), UnityEngine.Mathf.Round ( tempBloom ), 0, 1 );
		GUI.Label ( new Rect ( 45, 180, 80, 22 ), "Toggle Bloom" );

		tempBlur = GUI.HorizontalSlider ( new Rect ( 20, 203, 20, 14 ), UnityEngine.Mathf.Round ( tempBlur ), 0, 1 );
		GUI.Label ( new Rect ( 45, 198, 80, 22 ), "Toggle Blur" );
		
		GUI.Label ( new Rect ( 35, 224, 80, 20 ), "Blur Amount" );
		tempBlurIterations = GUI.TextField ( new Rect ( 15, 224, 15, 20 ), tempBlurIterations, 1 );
		tempBlurIterations = RegexToString ( tempBlurIterations, false );

#endregion

#region AudioSettings

		GUI.Box ( new Rect ( 170, 166, 170, 110 ), "Audio Echo Settings" );

		GUI.Label ( new Rect ( 210, 186, 80, 20 ), "Echo Delay" );
		tempEchoDelay = GUI.TextField ( new Rect ( 175, 186, 30, 20 ), tempEchoDelay, 3 );
		tempEchoDelay = RegexToString ( tempEchoDelay, true );

		GUI.Label ( new Rect ( 210, 208, 110, 20 ), "Echo Decay Rate" );
		tempEchoDecayRate = GUI.TextField ( new Rect ( 175, 208, 30, 20 ), tempEchoDecayRate, 3 );
		tempEchoDecayRate = RegexToString ( tempEchoDecayRate, true );

		GUI.Label ( new Rect ( 210, 230, 100, 20 ), "Echo Wet Mix" );
		tempEchoWetMix = GUI.TextField ( new Rect ( 175, 230, 30, 20 ), tempEchoWetMix, 3 );
		tempEchoWetMix = RegexToString ( tempEchoWetMix, true );

		GUI.Label ( new Rect ( 210, 252, 100, 20 ), "Echo Dry Mix" );
		tempEchoDryMix = GUI.TextField ( new Rect ( 175, 252, 30, 20 ), tempEchoDryMix, 3 );
		tempEchoDryMix = RegexToString ( tempEchoDryMix, true );

#endregion

#region SlideshowSettings

		GUI.Box ( new Rect ( 10, 256, 150, 116 ), "Slideshow Settings" );
		
		tempSlideshow = GUI.HorizontalSlider ( new Rect ( 20, 284, 20, 14 ), UnityEngine.Mathf.Round ( tempSlideshow ), 0, 1 );
		GUI.Label ( new Rect ( 45, 278, 100, 22 ), "Slideshow" );
		
		tempAutoAVBlur = GUI.HorizontalSlider ( new Rect ( 20, 304, 20, 14 ), UnityEngine.Mathf.Round ( tempAutoAVBlur ), 0, 1 );
		GUI.Label ( new Rect ( 45, 298, 100, 22 ), "Force AV Blur" );
		
		tempAutoAVOff = GUI.HorizontalSlider ( new Rect ( 20, 324, 20, 14 ), UnityEngine.Mathf.Round ( tempAutoAVOff ), 0, 1 );
		GUI.Label ( new Rect ( 45, 318, 100, 22 ), "Force AV Off" );
		
		GUI.Label ( new Rect ( 45, 345, 80, 20 ), "Display Time" );
		displayTime = GUI.TextField ( new Rect ( 15, 346, 27, 20 ), displayTime, 3 );
		displayTime = RegexToString ( displayTime, true );

#endregion

#region Online Settings

		GUI.Box ( new Rect ( 170, 284, 170, 62 ), "Online Settings" );
		
		tempCheckForUpdates = GUI.HorizontalSlider ( new Rect ( 175, 309, 20, 14 ), UnityEngine.Mathf.Round ( tempCheckForUpdates ), 0, 1 );
		GUI.Label ( new Rect ( 200, 303, 115, 22 ), "Check For Updates" );
		
		tempEnableOMB = GUI.HorizontalSlider ( new Rect ( 175, 329, 20, 14 ), UnityEngine.Mathf.Round ( tempEnableOMB ), 0, 1 );
		GUI.Label ( new Rect ( 200, 322, 120, 22 ), "Enable Online Store" );
		
#endregion

		if ( GUI.Button ( new Rect ( 170, 350, 170, 22 ), "Reset Tutorials" ))
		{
			
			startupManager.showTutorials = true;
		}

		GUI.Box ( new Rect ( 10, 378, 330, 22 ), "UnityMusicPlayer Version " + startupManager.runningVersion );
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
		
			float tempDisplayTime = Convert.ToSingle ( displayTime );
			yield return new WaitForSeconds ( tempDisplayTime );
			
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
		
		if ( showArtwork == true )
		{
			
			string[] artworkImageLocations = new string[0];
			artworkImageLocations = Directory.GetFiles ( currentDirectory, "Artwork.*" ).Where ( s => s.EndsWith ( ".png" ) || s.EndsWith ( ".jpg" ) || s.EndsWith ( ".jpeg" )).ToArray ();
			
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
		
		if ( manager.audio.clip != null && showTimebar == true )
			GUI.DrawTexture ( new Rect ( manager.audio.time * ( musicViewerPosition.width/manager.audio.clip.length ), -3, 10, 6 ), timebarMarker );
		
		if ( showMusicViewer == true )
		{

			if ( showOptionsWindow == true )
			{

				paneManager.popupBlocking = true;
				GUI.Window ( 5, optionsWindowRect, OptionsWindow, "Options and Settings" );
			}
			
			GUI.skin = guiSkin;
			
			if ( showOptionsWindow == true )
			{
				
				fileStyle.hover.background = null;
				folderStyle.hover.background = null;
				buttonStyle.hover.background = null;
				guiSkin.button.hover.background = null;
			} else {
				
				fileStyle.hover.background = guiHover;
				folderStyle.hover.background = guiHover;
				buttonStyle.hover.background = guiHover;
				guiSkin.button.hover.background = guiHover;
			}
			
			musicViewerPosition = GUI.Window ( 0, musicViewerPosition, MusicViewerPane, musicViewerTitle );
		}
	}
	

	void MusicViewerPane ( int wid )
	{

		if ( slideshow == false )
		{
								
			hideGUI = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 190, musicViewerPosition.height - 20, 80, 20 ), hideGUI, "Hide Audio" );
			showVisualizer = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 110, musicViewerPosition.height - 20, 100, 20 ), showVisualizer, "AudioVisualizer" );
		
			if ( showVisualizer == true )
			{
		
				audioVisualizerR.showAV = showVisualizer;
				audioVisualizerL.showAV = showVisualizer;
				audioVisualizerR.topLine.material.color = new Color ( avcR, avcG, avcB, 255 );
				audioVisualizerR.bottomLine.material.color = new Color ( avcR, avcG, avcB, 255 );
				audioVisualizerL.topLine.material.color = new Color ( avcR, avcG, avcB, 255 );
				audioVisualizerL.bottomLine.material.color = new Color ( avcR, avcG, avcB, 255 );
		
				manager.GetComponent<BloomAndLensFlares>().enabled = Convert.ToBoolean ( bloom );
				manager.GetComponent<BlurEffect>().enabled = Convert.ToBoolean ( blur );
				manager.GetComponent<SunShafts>().enabled = Convert.ToBoolean ( sunShafts );
					
				manager.GetComponent<BlurEffect> ().iterations = Convert.ToInt16 ( blurIterations );
			} else {
		
				audioVisualizerR.showAV = showVisualizer;
				audioVisualizerL.showAV = showVisualizer;
		
				manager.GetComponent<BloomAndLensFlares>().enabled = false;
				manager.GetComponent<BlurEffect>().enabled = false;
				manager.GetComponent<SunShafts>().enabled = false;
			}
			
			if ( doubleSpeed = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 10, musicViewerPosition.height - 20, 95, 20 ), doubleSpeed, "Double Speed" ))
			{
					
				manager.audio.pitch = 2.0F;
			
				if ( doubleSpeed == true && halfSpeed == true )
					halfSpeed = false;
			}
			
			if ( halfSpeed = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 + 85, musicViewerPosition.height - 20, 80, 20 ), halfSpeed, "Half Speed" ))
			{
				
				manager.audio.pitch = 0.5F;
			
				if ( halfSpeed == true && doubleSpeed == true )
					doubleSpeed = false;
			}
				
			echo = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 + 165, musicViewerPosition.height - 20, 50, 20 ), echo,  "Echo" );
			if ( echo == true )
				manager.GetComponent<AudioEchoFilter> ().enabled = true;
			else
			    manager.GetComponent<AudioEchoFilter> ().enabled = false;
		
				
			if ( halfSpeed == false && doubleSpeed == false )
				manager.audio.pitch = 1.0F;
			
			if ( hideGUI == false )
			{
			
				if ( fileBrowser == false )
				{
					
					GUI.Label ( new Rect ( musicViewerPosition.width/2 - 100, musicViewerPosition.height/4 - 50, 100, 25 ), "Volume" );
					volumeBarValue = GUI.HorizontalSlider ( new Rect ( musicViewerPosition.width/2 - 118, musicViewerPosition.height/4 - 30, 100, 30 ), volumeBarValue, 0.0F, 1.0F );
		
					if ( GUI.Button ( new Rect ( musicViewerPosition.width/2 - 70, musicViewerPosition.height/4 - 15, 60, 30 ), "Next" ))
						NextSong ();

					if ( GUI.Button ( new Rect ( musicViewerPosition.width/2 - 130, musicViewerPosition.height/4 - 15, 60, 30 ), "Back" ))
						PreviousSong ();
				
					GUI.Label ( new Rect ( musicViewerPosition.width/2 + 10, musicViewerPosition.height/4 - 50, 120, 30 ), "Loop" );
				
					if ( loop = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 5, musicViewerPosition.height/4 - 45, 100, 20 ), loop, "" ))
					{
						
						if ( loop == true && shuffle == true || loop == true && continuous == true )
						{
					
							shuffle = false;
							continuous = false;
						}
					}
				
					GUI.Label ( new Rect ( musicViewerPosition.width/2 + 10, musicViewerPosition.height/4 - 30, 120, 30 ), "Shuffle" );
				
					if ( shuffle = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 5, musicViewerPosition.height/4 - 25, 100, 20 ), shuffle, "" ))
					{
						
						if ( shuffle == true && loop == true || shuffle == true && continuous == true )
						{
					
							loop = false;
							continuous = false;
						}
					}
				
					GUI.Label ( new Rect ( musicViewerPosition.width/2 + 10, musicViewerPosition.height/4 - 10, 120, 30 ), "Continuous" );
				
					if ( continuous = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 5, musicViewerPosition.height/4 - 5, 100, 20 ), continuous, "" ))
					{
	
						if ( continuous == true && shuffle == true || continuous == true && loop == true )
						{
					
							shuffle = false;
							loop = false;
						}
					}
						
					GUILayout.BeginHorizontal ();
					GUILayout.Space ( musicViewerPosition.width / 2 - 300 );
					GUILayout.BeginVertical ();
					GUILayout.Space ( musicViewerPosition.height / 4 + 25 );
					scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  musicViewerPosition.height - ( musicViewerPosition.height / 4 + 53 )));
					
					if ( enableDeepSearch == true )
					{
						
						if ( currentDirectoryDirectories.Any ())
						{
						
							for ( int directoryInt = 0; directoryInt < currentDirectoryDirectories.Length; directoryInt ++ )
							{
									
								if ( GUILayout.Button ( new GUIContent ( currentDirectoryDirectories[directoryInt].Substring ( currentDirectoryDirectories[directoryInt].LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length ), folderIcon), folderStyle ))
								{
									
									if ( showFolderMusic == false )
									{
										
										showFolderMusic = true;
										songInfoOwner = currentDirectoryDirectories[directoryInt];
										Refresh ();
									} else {
										
										if ( songInfoOwner != currentDirectoryDirectories[directoryInt] )
										{
											
											songInfoOwner = currentDirectoryDirectories[directoryInt];
											Refresh ();
										} else {
												
											showFolderMusic = false;
										}
									}
								}
								
								if ( showFolderMusic == true )
								{
							
									if ( songInfoOwner == currentDirectoryDirectories[directoryInt] )
									{
										
										if ( childDirectoryFiles.Any ())
										{
								
											for ( int childSongInt = 0; childSongInt < childDirectoryFiles.Length; childSongInt++ )
											{
											
												string childAudioTitle;
												if ( showTypes == true )				
													childAudioTitle = childDirectoryFiles[childSongInt].Substring ( childDirectoryFiles[childSongInt].LastIndexOf ( Path.DirectorySeparatorChar ));
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
											
													if ( childDirectoryFiles[childSongInt].Substring ( childDirectoryFiles [childSongInt].LastIndexOf ( "." )) == ".unity3d" )
													{
												
														loadingImage.showLoadingImages = true;
														loadingImage.InvokeRepeating ( "LoadingImages", 0.25F, 0.25F );
												
														StartCoroutine ( LoadAssetBundle ( "file://" + childDirectoryFiles [ currentSongNumber ]));
												
													} else {
												
														StartCoroutine ( PlayAudio ( childDirectoryFiles[childSongInt] ));
														loadingImage.showLoadingImages = false;
													}
												}
												GUILayout.EndHorizontal ();
											}
										} else {
											
											GUILayout.Label ( "This directory doesn't contain any music!" );
										}
									}
								}
							}
						}
					}
									
					if ( currentDirectoryFiles.Any ())
					{
					
						for ( int songInt = 0; songInt < currentDirectoryFiles.Length; songInt ++ )
						{
											
							string audioTitle;
							if ( showTypes == true )				
								audioTitle = currentDirectoryFiles[songInt].Substring ( currentDirectory.Length + Path.DirectorySeparatorChar.ToString().Length );
							else			
								audioTitle = currentDirectoryFiles[songInt].Substring ( currentDirectoryFiles[songInt].LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length, currentDirectoryFiles[songInt].LastIndexOf ( "." ) - currentDirectoryFiles[songInt].LastIndexOf ( Path.DirectorySeparatorChar ) - Path.DirectorySeparatorChar.ToString().Length );
							
							if ( GUILayout.Button ( new GUIContent ( audioTitle )))
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
											
								if ( currentDirectoryFiles[songInt].Substring ( currentDirectoryFiles [songInt].LastIndexOf ( "." )) == ".unity3d" )
								{
												
									loadingImage.showLoadingImages = true;
									loadingImage.InvokeRepeating ( "LoadingImages", 0.25F, 0.25F );
												
									StartCoroutine ( LoadAssetBundle ( "file://" + currentDirectoryFiles [ currentSongNumber ]));
												
								} else {
												
									StartCoroutine ( PlayAudio ( currentDirectoryFiles[songInt] ));
									loadingImage.showLoadingImages = false;
								}
							}
						}
					} else
					{
						
						if ( startupManager.showTutorials == true )
						{
								
							GUILayout.Label ( "\nYou don't have any music to play!\n\nIf you have some music (.wav, .ogg, or .aiff),\nclick 'Open File Browser' under the System Commands bar bellow." +
								"\n\nYou can also download music by navigating\nto the OnlineMusicBrowser (press the right arrow key).\n", centerStyle );
						
							if ( GUILayout.Button ( "Hide Tutorials"))
							{
							
								startupManager.showTutorials = false;
							}
							
							if ( GUILayout.Button ( "View Extended Help/Tutorial" ))
							{
											
								Process.Start ( startupManager.helpPath );
							}
						}
					} 
				} else {
					
					if ( tempCurrentDirectory.Substring ( 0, tempCurrentDirectory.LastIndexOf ( Path.DirectorySeparatorChar )).Length > 0 )
					{
						
						if ( GUI.Button ( new Rect ( musicViewerPosition.width/2 - 300, musicViewerPosition.height/4 - 15, 140, 30 ), new GUIContent ( "Previous", leftArrow )))
						{
						
							tempCurrentDirectory = tempCurrentDirectory.Substring ( 0, tempCurrentDirectory.LastIndexOf ( Path.DirectorySeparatorChar ));
							currentDirectoryDirectories = Directory.GetDirectories ( tempCurrentDirectory ).ToArray ();
							currentDirectoryFiles = Directory.GetFiles ( tempCurrentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
						}
					}
					
					if ( GUI.Button ( new Rect ( musicViewerPosition.width/2 - 150, musicViewerPosition.height/4 - 15, 200, 30 ), "Active Directory" ))
					{
						
						tempCurrentDirectory = currentDirectory;
						currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
						currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
					}
		
					if ( GUI.Button ( new Rect ( musicViewerPosition.width/2 + 60, musicViewerPosition.height/4 - 15, 240, 30 ), "Open in " + startupManager.directoryBrowser ))
						Process.Start ( tempCurrentDirectory );
					
					GUILayout.BeginHorizontal ();
					GUILayout.Space ( musicViewerPosition.width / 2 - 300 );
					GUILayout.BeginVertical ();
					GUILayout.Space ( musicViewerPosition.height / 4 + 25 );
					scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  musicViewerPosition.height - ( musicViewerPosition.height / 4 + 53 )));
			
					int cDDI = 0;
					if ( currentDirectoryDirectories.Any ())
					{
						
						do
						{
							
							if ( currentDirectoryDirectories[cDDI] == currentDirectory )
							{
                	
								folderStyle.fontStyle = FontStyle.Italic;
							} else {
									
								folderStyle.fontStyle = FontStyle.Normal;
							}
							
							if ( GUILayout.Button ( new GUIContent ( currentDirectoryDirectories[cDDI].Substring ( tempCurrentDirectory.Length + 1 ), folderIcon ), folderStyle ))
							{
					
								tempCurrentDirectory = currentDirectoryDirectories[cDDI];
								currentDirectoryDirectories = Directory.GetDirectories ( tempCurrentDirectory ).ToArray ();
								currentDirectoryFiles = Directory.GetFiles ( tempCurrentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".aif" ) || s.EndsWith ( ".aiff" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
							}
							
							cDDI += 1;
						}
						while ( cDDI < currentDirectoryDirectories.Length );
					}
					
					for ( int i = 0; i < currentDirectoryFiles.Length; i += 1 )
					{
			
						 GUILayout.Button ( new GUIContent ( currentDirectoryFiles[i].Substring ( currentDirectory.Length + 1 ), musicNoteIcon ), fileBrowserFileStyle );
					}
					
					if ( currentDirectoryFiles.Length == 0 && currentDirectoryDirectories.Length == 0 )
					{
						
						GUILayout.FlexibleSpace ();
						GUILayout.Label ( "This folder is empty!", labelStyle );
						GUILayout.FlexibleSpace ();
					}

					if ( tempCurrentDirectory != currentDirectory )
					{
						
						if ( GUILayout.Button ( "Set as Active Directory" ))
						{
						
							currentDirectory = tempCurrentDirectory;
							StartCoroutine ( SetArtwork ());
							fileBrowser = false;
							Refresh ();
						
							scrollPosition.y = 0;
						}
					}
				}
			
				GUILayout.Box ( "System Commands" );
			
				if ( fileBrowser == false )
				{
					
					if ( showQuickManage == true )
						if ( GUILayout.Button ( "Open Current Directory" ))
							Process.Start ( currentDirectory );
					
					if ( GUILayout.Button ( "Open File Browser" ))
					{
				
						tempCurrentDirectory = currentDirectory;
						fileBrowser = true;
						Refresh ();
					
						scrollPosition.y = 0;
					}
				} else {
					if ( GUILayout.Button ( "Close File Browser" ))
					{
				
						fileBrowser = false;
						Refresh ();
					
						scrollPosition.y = 0;
					}
				}
										
				if ( GUILayout.Button ( "Options" ))
					showOptionsWindow = true;
			
				GUI.EndScrollView();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
			
			if ( showOptionsWindow == true || startupManager.showUnderlay == true )
				GUI.DrawTexture ( new Rect ( 0, 0, musicViewerPosition.width, musicViewerPosition.height ), startupManager.underlay );
		}
	}


	void NextSong ()
	{
			
		if ( currentDirectoryFiles.Any () || showFolderMusic == true && childDirectoryFiles.Any () || manager.audio.isPlaying == true && childDirectoryFiles.Any ())
		{
			
			string[] searchDirectoryFiles = null;
			if ( currentDirectoryFiles.Any ())
			{
				
				searchDirectoryFiles = currentDirectoryFiles;
			} else if ( showFolderMusic == true && childDirectoryFiles.Any () || manager.audio.isPlaying == true && childDirectoryFiles.Any ()) {
				
				searchDirectoryFiles = childDirectoryFiles;
			}

			wasPlaying = false;
			if ( psPlace < 6 )
			{

				psPlace += 1;
				currentSongNumber = previousSongs [ psPlace ];
			
			} else {

				if ( continuous == true || loop == false && shuffle == false && continuous == false )
				{
						
					if ( currentSongNumber == searchDirectoryFiles.Length - 1 || showFolderMusic == true && currentSongNumber == searchDirectoryFiles.Length - 1 )
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

					if ( loop == true )
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

						if ( shuffle == true )
						{

							Resources.UnloadUnusedAssets ();
							
							int previousSongNumber = currentSongNumber;
							
								currentSongNumber = UnityEngine.Random.Range ( 0, searchDirectoryFiles.Length );
					
							if ( currentSongNumber == previousSongNumber && searchDirectoryFiles.Length > 1 )
							{
								
								bool shuffleOkay = false;
								while ( shuffleOkay == false )
								{
									
									currentSongNumber = UnityEngine.Random.Range ( 0, searchDirectoryFiles.Length );
									
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

			if ( searchDirectoryFiles [ currentSongNumber ].Substring ( searchDirectoryFiles [ currentSongNumber ].Length - 7 ) == "unity3d"  )
			{
				
				loadingImage.showLoadingImages = true;
				loadingImage.InvokeRepeating ( "LoadingImages", 0.25F, 0.25F );

				StartCoroutine ( LoadAssetBundle ( "file://" + searchDirectoryFiles [ currentSongNumber ]));
			} else {
									
				StartCoroutine ( PlayAudio ( searchDirectoryFiles [ currentSongNumber ] ));
			}
		}
	}


	void PreviousSong ()
	{

		if ( currentDirectoryFiles.Any () || showFolderMusic == true && childDirectoryFiles.Any () || manager.audio.isPlaying == true && childDirectoryFiles.Any ())
		{
			
			string[] searchDirectoryFiles = null;
			if ( currentDirectoryFiles.Any ())
			{
				
				searchDirectoryFiles = currentDirectoryFiles;
			} else if ( showFolderMusic == true && childDirectoryFiles.Any () || manager.audio.isPlaying == true && childDirectoryFiles.Any ()) {
				
				searchDirectoryFiles = childDirectoryFiles;
			}

			wasPlaying = false;
			if ( psPlace <= 0 )
			{

				currentSongNumber = UnityEngine.Random.Range ( 0, searchDirectoryFiles.Length );
			} else
			{
			
				psPlace -= 1;
				currentSongNumber = previousSongs [ psPlace ];
			}
			
			if ( currentSongNumber > searchDirectoryFiles.Length )
				currentSongNumber = 0;
			
			if ( searchDirectoryFiles [ currentSongNumber ].Substring ( searchDirectoryFiles [ currentSongNumber ].Length - 7 ) == "unity3d" )
			{

				loadingImage.showLoadingImages = true;
				loadingImage.InvokeRepeating ( "LoadingImages", 0.25F, 0.25F );

				StartCoroutine ( LoadAssetBundle ( "file://" + searchDirectoryFiles [ currentSongNumber ]));
			} else {
				
				StartCoroutine ( PlayAudio ( searchDirectoryFiles [ currentSongNumber ] ));
			}
		}
	}
	
	
	IEnumerator LoadAssetBundle ( string assetBundleToOpen )
	{
		
		timemark.text = "Loading][Loading";
		audioLocation = assetBundleToOpen;
		
		Caching.CleanCache ();
		
		songLocation = currentDirectoryFiles [ currentSongNumber ];
	
		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( assetBundleToOpen + " | " + songLocation.Substring ( songLocation.LastIndexOf ( "/" ) + 1 ));
			
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
				
				if ( showTypes == true )
					audioTitle = songLocation.Substring ( songLocation.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length );
				else
					audioTitle = songLocation.Substring ( songLocation.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length, songLocation.LastIndexOf ( "." ) - songLocation.LastIndexOf ( Path.DirectorySeparatorChar ) - Path.DirectorySeparatorChar.ToString().Length );
			
				currentSong.text = audioTitle;
			}
				
			if ( preciseTimemark == true )
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
			UnityEngine.Debug.Log ( wwwClient.error );
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
			
			if ( showTypes == true )
				audioTitle = songToLoad.Substring ( songToLoad.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length );
			else
				audioTitle = songToLoad.Substring ( songToLoad.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length, songToLoad.LastIndexOf ( "." ) - songToLoad.LastIndexOf ( Path.DirectorySeparatorChar ) - Path.DirectorySeparatorChar.ToString().Length );
			
			currentSong.text = audioTitle;
		}
		
		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( songToLoad.Substring ( songToLoad.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length ));
		
		WWW www = new WWW ( "file://" + songToLoad );
		yield return www;

		manager.audio.clip = www.GetAudioClip ( false, false, audioType );
		Resources.UnloadUnusedAssets ();

		if ( slideshow == false )
		{
			
			if ( preciseTimemark == true )
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
	
			if ( preciseTimemark == true )
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
				
			if ( startupManager.developmentMode == true )
				UnityEngine.Debug.Log ( "Playing audio" );
		}

		if ( www.error != null )
		{
			
			UnityEngine.Debug.Log ( www.error );
			
			manager.audio.clip = null;
			Resources.UnloadUnusedAssets ();
			
			rtMinutes = 0;
			rtSeconds = 00;
			minutes = 0;
			seconds = 00;
			
			if ( slideshow == false )
			{
										
				currentSong.text = "UnityMusicPlayer";
				
				if ( preciseTimemark == true )
					timemark.text = "0:00.000][0:00.000";
				else
					timemark.text = "0:00][0:00";
			}
		}
	}


	void Update ()
	{

		if ( Input.GetKeyUp ( KeyCode.DownArrow ))
		{
			
			NextSong ();
			loadingImage.showLoadingImages = true;
		}

		if ( Input.GetKeyUp ( KeyCode.UpArrow ))
		{
			
			PreviousSong ();
			loadingImage.showLoadingImages = true;
		}

		if ( Input.GetKeyUp ( KeyCode.Space ))
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
		
		if ( Input.GetKey ( KeyCode.Escape ) && slideshow == true )
		{
				
			slideshow = false;
			StartCoroutine ( "SetArtwork" );
			
			tempSlideshow = Convert.ToSingle ( slideshow );
			if ( showTimebar == false )
				musicViewerTitle = "MusicViewer";
			
			timemark.enabled = true;
			hideGUI = false;
			
			manager.GetComponent<BlurEffect> ().enabled = blur;
			
			if ( manager.audio.clip != null )
			{

				if ( showTypes == true )
					currentSong.text = audioLocation.Substring ( audioLocation.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length );
				else
					currentSong.text = audioLocation.Substring ( audioLocation.LastIndexOf ( Path.DirectorySeparatorChar ) + Path.DirectorySeparatorChar.ToString().Length, audioLocation.LastIndexOf ( "." ) - audioLocation.LastIndexOf ( Path.DirectorySeparatorChar ) - Path.DirectorySeparatorChar.ToString().Length );
				
			} else {
				
				currentSong.text = "UnityMusicPlayer";
				if ( preciseTimemark == true )
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
			
		if ( manager.audio.isPlaying == true )
		{
				
			if ( manager.audio.time >= manager.audio.clip.length )
			{
					
				if ( startupManager.developmentMode == true )
					UnityEngine.Debug.Log ( manager.audio.time + "  :  " + manager.audio.clip.length );

				wasPlaying = false;
				if ( continuous == true || loop == true )
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
					if ( continuous == true || loop == false && shuffle == false )
						Invoke ( "SongEnd", betweenSongDelay );
					else
						SongEnd ();
				}
			}
		}	
		
		if ( slideshow == false )
		{
		
			manager.audio.volume = volumeBarValue;
			
			if ( manager.audio.clip != null )
			{

				if ( manager.audio.isPlaying == true )
				{
						
					if ( preciseTimemark == true )
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
				
				if ( preciseTimemark == true )
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
	}


	void SongEnd ()
	{

		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( "AudioClips in Memory: " + Resources.FindObjectsOfTypeAll ( typeof ( AudioClip )).Length );
		
		if ( loop == true )
		{
			
			rtMinutes = new int ();
			rtSeconds = new int ();
				
			manager.audio.Play ();
			isPaused = false;
			wasPlaying = true;
				
			if ( startupManager.developmentMode == true )
				UnityEngine.Debug.Log ( "Playing audio" );
				
		} else {
			
			if ( currentDirectoryFiles.Any () || showFolderMusic == true && childDirectoryFiles.Any ())
			{		
			
				psPlace = 6;

				if ( continuous == true )
				{
				
					if ( currentSongNumber == currentDirectoryFiles.Length - 1 )
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
					
					if ( shuffle == true )
					{
						
						Resources.UnloadUnusedAssets ();
						
						int previousSongNumber = currentSongNumber;
						currentSongNumber = UnityEngine.Random.Range ( 0, currentDirectoryFiles.Length );
						
						if ( showFolderMusic == false )
							currentSongNumber = UnityEngine.Random.Range ( 0, currentDirectoryFiles.Length );
						else
							currentSongNumber = UnityEngine.Random.Range ( 0, childDirectoryFiles.Length );
				
						if ( currentSongNumber == previousSongNumber && currentDirectoryFiles.Length > 1 || currentSongNumber == previousSongNumber && childDirectoryFiles.Length > 1 )
						{
							
							bool shuffleOkay = false;
							while ( shuffleOkay == false )
							{
								
								if ( showFolderMusic== false )
									currentSongNumber = UnityEngine.Random.Range ( 0, currentDirectoryFiles.Length );
								else
									currentSongNumber = UnityEngine.Random.Range ( 0, childDirectoryFiles.Length );
								
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
						
							if ( preciseTimemark == true )
								timemark.text = "0:00.000][0:00.000";
							else
								timemark.text = "0:00][0:00";
						}
					}
				}
			}
			
			if ( continuous == true || shuffle == true )
			{
				
				if ( showFolderMusic == false )
				{
					if ( currentDirectoryFiles [ currentSongNumber ].Substring ( currentDirectoryFiles [ currentSongNumber ].Length - 7 ) == "unity3d"  )
					{
				
						loadingImage.showLoadingImages = true;
						loadingImage.InvokeRepeating ( "LoadingImages", 0.25F, 0.25F );

						StartCoroutine ( LoadAssetBundle ( "file://" + currentDirectoryFiles [ currentSongNumber ]));
					} else {
									
						StartCoroutine ( PlayAudio ( currentDirectoryFiles [ currentSongNumber ] ));
					}
				} else {
					
					if ( childDirectoryFiles [ currentSongNumber ].Substring ( childDirectoryFiles [ currentSongNumber ].Length - 7 ) == "unity3d"  )
					{
				
						loadingImage.showLoadingImages = true;
						loadingImage.InvokeRepeating ( "LoadingImages", 0.25F, 0.25F );

						StartCoroutine ( LoadAssetBundle ( "file://" + childDirectoryFiles [ currentSongNumber ]));
					} else {
									
						StartCoroutine ( PlayAudio ( childDirectoryFiles [ currentSongNumber ] ));
					}	
				}
			}
		}
	}


	void Quit ()
	{

		wasPlaying = false;
		manager.audio.Stop ();
		
		Resources.UnloadUnusedAssets ();
		Caching.CleanCache ();

		TextWriter savePrefs = new StreamWriter ( startupManager.prefsLocation );
		savePrefs.WriteLine ( currentDirectory + "\n" + startupManager.checkForUpdate + "\n" + startupManager.ombEnabled + "\n" + startupManager.showTutorials + "\n" + loop + "\n" + shuffle + "\n" + continuous + "\n" + showTypes + "\n" + showArrows + "\n" + showTimebar + "\n" + showArtwork + "\n" + enableDeepSearch + "\n" + showQuickManage + "\n" + preciseTimemark + "\n" + volumeBarValue + "\n" + avcR + "\n" + avcG + "\n" + avcB + "\n" + bloom + "\n" + blur + "\n" + sunShafts + 
		                     "\n" + blurIterations + "\n" + echoDelay + "\n" + echoDecayRate + "\n" + echoWetMix + "\n" + echoDryMix + "\n" + autoAVBlur + "\n" + autoAVOff + "\n" + displayTime );
		savePrefs.Close ();

		if ( Application.isEditor == true )
			UnityEngine.Debug.Log ( "Quit has been called" );
		else
			Application.Quit ();
	}
}
