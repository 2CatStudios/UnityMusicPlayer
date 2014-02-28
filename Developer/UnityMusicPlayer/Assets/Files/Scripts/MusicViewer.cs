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
	StartupManager startupManager;
	OnlineMusicBrowser onlineMusicBrowser;
	MusicManager musicManager;
	LoadingImage loadingImage;
	PaneManager paneManager;
	AudioVisualizerR audioVisualizerR;
	AudioVisualizerL audioVisualizerL;
	
	internal bool showMusicViewer = true;

	string musicViewerTitle;
	internal Rect musicViewerPosition = new Rect ( 0, 0, 800, 600 );

	bool showVisualizer = false;
	bool halfSpeed = false;
	bool doubleSpeed = false;
	
	internal string mediaPath;

//-------
	
	bool isPaused;	
	float pausePoint;
	int songTime;
	int minutes;
	float seconds;
	int rtMinutes;
	float rtSeconds;

	internal bool wasPlaying = false;
	
	public Texture2D timebarMarker;
	
	internal GUIText timemark;
	float timebarTime;

	float betweenSongDelay = 0.5F;

	internal String[] clipList;
	int currentSongNumber = -1;
	int i;
	
	Vector2 scrollPosition;
	Vector2 mousePos;
	
//-------
	
	public string songLocation;

	string rawCurrentSong;
	public GUIText currentSong;

//-------

	bool hideGUI = false;
	public GUISkin guiSkin;
	GUIStyle centerStyle;
	public Texture2D guiHover;
	
	bool close = false;
	
	int[] previousSongs = new int  [ 7 ] { 0, 0, 0, 0, 0, 0, 0 };
	int psPlace = 6;
	
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
	
#region General Settings
	
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

#endregion

#region Slideshow Settings

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

#region Online Settings

	float tempCheckForUpdates;
	internal float tempEnableOMB;

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
	
	WWW wWw;

#endregion

	
	void Start ()
	{

		manager = GameObject.FindGameObjectWithTag ( "Manager" );
		startupManager = manager.GetComponent <StartupManager> ();
		mediaPath = startupManager.lastDirectory;
		onlineMusicBrowser = GameObject.FindGameObjectWithTag ( "OnlineMusicBrowser" ).GetComponent <OnlineMusicBrowser>();
		musicManager = GameObject.FindGameObjectWithTag ( "MusicManager" ).GetComponent <MusicManager>();
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

		clipList = Directory.GetFiles ( mediaPath, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();

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
		
		showQuickManage = Convert.ToBoolean ( startupManager.prefs [ 11 ] );
		tempShowQuickManage = Convert.ToSingle ( showQuickManage );
		
		preciseTimemark = Convert.ToBoolean ( startupManager.prefs [ 12 ] );
		tempPreciseTimemark = Convert.ToSingle ( preciseTimemark );
		
		volumeBarValue = Convert.ToSingle ( startupManager.prefs [ 13 ] );

		avcR = float.Parse ( startupManager.prefs [ 14 ] );
		tempAVCR = avcR;
		
		avcG = float.Parse ( startupManager.prefs [ 15 ] );
		tempAVCG = avcG;
		
		avcB = float.Parse ( startupManager.prefs [ 16 ] );
		tempAVCB = avcB;

		bloom = Convert.ToBoolean ( startupManager.prefs [ 17 ] );	
		tempBloom = Convert.ToSingle ( bloom );
		
		blur = Convert.ToBoolean ( startupManager.prefs [ 18 ] );
		tempBlur = Convert.ToSingle ( blur );
		
		sunShafts = Convert.ToBoolean ( startupManager.prefs [ 19 ] );
		tempSunShafts = Convert.ToSingle ( sunShafts );
		
		blurIterations = Convert.ToString ( startupManager.prefs [ 20 ] );
		tempBlurIterations = blurIterations;
		
		manager.GetComponent<BlurEffect> ().iterations = Convert.ToInt16 ( blurIterations );

		echoDelay = startupManager.prefs [ 21 ];
		tempEchoDelay = echoDelay;
		echoDecayRate = startupManager.prefs [ 22 ];
		tempEchoDecayRate = echoDecayRate;
		echoWetMix = startupManager.prefs [ 23 ];
		tempEchoWetMix = echoWetMix;
		echoDryMix = startupManager.prefs [ 24 ];
		tempEchoDryMix = echoDryMix;
		
		manager.GetComponent<AudioEchoFilter> ().delay = Convert.ToSingle ( echoDelay );
		manager.GetComponent<AudioEchoFilter> ().decayRatio = Convert.ToSingle ( echoDecayRate );
		manager.GetComponent<AudioEchoFilter> ().wetMix = Convert.ToSingle ( echoWetMix );
		manager.GetComponent<AudioEchoFilter> ().dryMix = Convert.ToSingle ( echoDryMix );
		
		autoAVBlur = Convert.ToBoolean ( startupManager.prefs [ 25 ] );
		tempAutoAVBlur = Convert.ToSingle ( autoAVBlur );
		
		autoAVOff = Convert.ToBoolean ( startupManager.prefs [ 26 ] );
		tempAutoAVOff = Convert.ToSingle ( autoAVOff );
		
		displayTime = startupManager.prefs [ 27 ];

		previousSongs [ 0 ] = Convert.ToInt32 ( startupManager.prefs [ 28 ] );
		if ( previousSongs [ 0 ] > clipList.Length )
			previousSongs [ 0 ] = clipList.Length;
		
		previousSongs [ 1 ] = Convert.ToInt32 ( startupManager.prefs [ 29 ] );
		if ( previousSongs [ 1 ] > clipList.Length )
			previousSongs [ 1 ] = clipList.Length;
		
		previousSongs [ 2 ] = Convert.ToInt32 ( startupManager.prefs [ 30 ] );
		if ( previousSongs [ 2 ] > clipList.Length )
			previousSongs [ 2 ] = clipList.Length;
		
		previousSongs [ 3 ] = Convert.ToInt32 ( startupManager.prefs [ 31 ] );
		if ( previousSongs [ 3 ] > clipList.Length )
			previousSongs [ 3 ] = clipList.Length;
		
		previousSongs [ 4 ] = Convert.ToInt32 ( startupManager.prefs [ 32 ] );
		if ( previousSongs [ 4 ] > clipList.Length )
			previousSongs [ 4 ] = clipList.Length;
		
		previousSongs [ 5 ] = Convert.ToInt32 ( startupManager.prefs [ 33 ] );
		if ( previousSongs [ 5 ] > clipList.Length )
			previousSongs [ 5 ] = clipList.Length;
		
		previousSongs [ 6 ] = Convert.ToInt32 ( startupManager.prefs [ 34 ] );
		if ( previousSongs [ 6 ] > clipList.Length )
			previousSongs [ 6 ] = clipList.Length;


		currentSong.text = "UnityMusicPlayer";
		GameObject.FindGameObjectWithTag ( "TimebarImage" ).guiTexture.enabled = showTimebar;

		if ( showTimebar == true )
		{

			musicViewerTitle = "";
			onlineMusicBrowser.onlineMusicBrowserTitle = "";
			musicManager.musicManagerTitle = "";
		} else {

			musicViewerTitle = "MusicViewer";
			onlineMusicBrowser.onlineMusicBrowserTitle = "OnlineMusicBrowser";
			musicManager.musicManagerTitle = "MusicManager";
		}

		if ( preciseTimemark == true )
			timemark.text = "0:00.000][0:00.000";
		else
			timemark.text = "0:00][0:00";
			
		musicManager.SendMessage ( "SetArtwork" );

		TextWriter savePrefs = new StreamWriter ( startupManager.prefsLocation );
		savePrefs.WriteLine ( mediaPath + "\n" + startupManager.checkForUpdate + "\n" + startupManager.ombEnabled + "\n" + startupManager.showTutorials + "\n" + loop + "\n" + shuffle + "\n" + continuous + "\n" + showTypes + "\n" + showArrows + "\n" + showTimebar + "\n" + showArtwork + "\n" + showQuickManage + "\n" + preciseTimemark + "\n" + volumeBarValue + "\n" + avcR + "\n" + avcG + "\n" + avcB + "\n" + bloom + "\n" + blur + "\n" + sunShafts + 
		                     "\n" + blurIterations + "\n" + echoDelay + "\n" + echoDecayRate + "\n" + echoWetMix + "\n" + echoDryMix + "\n" + autoAVBlur + "\n" + autoAVOff + "\n" + displayTime + "\n" + previousSongs [ 0 ] + "\n" + previousSongs [ 1 ] + "\n" + previousSongs [ 2 ] + "\n" + previousSongs [ 3 ] + "\n" + previousSongs [ 4 ] + "\n" + previousSongs [ 5 ] + "\n" + previousSongs [ 6 ] );
		savePrefs.Close ();
		
		InvokeRepeating ( "Refresh", 0, 2 );
		
		centerStyle = new GUIStyle ();
		centerStyle.alignment = TextAnchor.MiddleCenter;
	}
	
	
	void Refresh ()
	{

		if ( paneManager.currentPane == PaneManager.pane.musicViewer )
		{
			
			if ( clipList.Any ())
			{
				
				clipList = Directory.GetFiles ( mediaPath, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
			}
		}
	}


	void OptionsWindow ( int wid )
	{

		GUI.FocusWindow ( 5 );
		GUI.BringWindowToFront ( 5 );

		if ( GUI.Button ( new Rect ( 290, 20, 50, 20 ), "Close" ) || close == true)
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

			showArrows = Convert.ToBoolean ( tempShowArrows );
			
			showTimebar = Convert.ToBoolean ( tempShowTimebar );
			if ( showTimebar == true )
			{

				GameObject.FindGameObjectWithTag ( "TimebarImage" ).guiTexture.enabled = true;
				musicViewerTitle = "";
				onlineMusicBrowser.onlineMusicBrowserTitle = "";
				musicManager.musicManagerTitle = "";
			} else {

				GameObject.FindGameObjectWithTag ( "TimebarImage" ).guiTexture.enabled = false;
				musicViewerTitle = "MusicViewer";
				onlineMusicBrowser.onlineMusicBrowserTitle = "OnlineMusicBrowser";
				musicManager.musicManagerTitle = "MusicManager";
			}
			
			showArtwork = Convert.ToBoolean ( tempShowArtwork );
			
			if ( showArtwork == true )
				musicManager.SendMessage ( "SetArtwork" );
			else
				currentSlideshowImage.texture = null;

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
		
		tempShowTypes = GUI.HorizontalSlider ( new Rect ( 170, 27, 20, 14 ), UnityEngine.Mathf.Round ( tempShowTypes ), 0, 1 );
		GUI.Label ( new Rect ( 195, 20, 100, 22 ), "Show Types" );
		
		tempShowArrows = GUI.HorizontalSlider ( new Rect ( 170, 44, 20, 14 ), UnityEngine.Mathf.Round ( tempShowArrows ), 0, 1 );
		GUI.Label ( new Rect ( 195, 38, 100, 22 ), "Show Arrows" );
		
		tempShowArtwork = GUI.HorizontalSlider ( new Rect ( 170, 61, 20, 14 ), UnityEngine.Mathf.Round ( tempShowArtwork ), 0, 1 );
		GUI.Label ( new Rect ( 195, 55, 100, 22 ), "Show Artwork" );
	
		tempShowTimebar = GUI.HorizontalSlider ( new Rect ( 170, 78, 20, 14 ), UnityEngine.Mathf.Round ( tempShowTimebar ), 0, 1 );
		GUI.Label ( new Rect ( 195, 72, 100, 22 ), "Show Timebar" );
		
		tempShowQuickManage = GUI.HorizontalSlider ( new Rect ( 170, 95, 20, 14 ), UnityEngine.Mathf.Round ( tempShowQuickManage ), 0, 1 );
		GUI.Label ( new Rect ( 195, 89, 116, 22 ), "Show QuickManage" );
		
		tempPreciseTimemark = GUI.HorizontalSlider ( new Rect ( 170, 112, 20, 14 ), UnityEngine.Mathf.Round ( tempPreciseTimemark ), 0, 1 );
		GUI.Label ( new Rect ( 195, 106, 145, 22 ), "Show Precise Timemark" );

#region AudioVisualizerSettings

		GUI.Box ( new Rect ( 10, 20, 150, 224 ), "AudioVisualizer Settings" );

		GUI.Label ( new Rect ( 35, 42, 40, 30 ), "Red" );
		tempAVCR = GUI.HorizontalSlider ( new Rect ( 35, 62, 100, 15), tempAVCR, 0.0F, 1.000F );

		GUI.Label ( new Rect ( 35, 72, 40, 30 ), "Green" );
		tempAVCG = GUI.HorizontalSlider ( new Rect ( 35, 92, 100, 15), tempAVCG, 0.0F, 1.000F );

		GUI.Label ( new Rect ( 35, 102, 40, 30 ), "Blue" );
		tempAVCB = GUI.HorizontalSlider ( new Rect ( 35, 122, 100, 15), tempAVCB, 0.0F, 1.000F );

		GUI.contentColor = new Color ( tempAVCR, tempAVCG, tempAVCB, 1.000F );
		GUI.Label ( new Rect ( 45, 137, 80, 20 ), "Sample Color");
		GUI.contentColor = Color.white;
		
		tempSunShafts = GUI.HorizontalSlider ( new Rect ( 20, 165, 20, 14 ), UnityEngine.Mathf.Round ( tempSunShafts ), 0, 1 );
		GUI.Label ( new Rect ( 45, 158, 120, 22 ), "Toggle Sun Shafts" );
		
		tempBloom = GUI.HorizontalSlider ( new Rect ( 20, 182, 20, 14 ), UnityEngine.Mathf.Round ( tempBloom ), 0, 1 );
		GUI.Label ( new Rect ( 45, 176, 80, 22 ), "Toggle Bloom" );

		tempBlur = GUI.HorizontalSlider ( new Rect ( 20, 199, 20, 14 ), UnityEngine.Mathf.Round ( tempBlur ), 0, 1 );
		GUI.Label ( new Rect ( 45, 194, 80, 22 ), "Toggle Blur" );
		
		GUI.Label ( new Rect ( 35, 220, 100, 20 ), "Blur Iterations" );
		tempBlurIterations = GUI.TextField ( new Rect ( 15, 220, 15, 20 ), tempBlurIterations, 1 );
		tempBlurIterations = RegexToString ( tempBlurIterations, false );

#endregion

#region AudioSettings

		GUI.Box ( new Rect ( 170, 134, 170, 110 ), "Audio Echo Settings" );

		GUI.Label ( new Rect ( 210, 154, 80, 20 ), "Echo Delay" );
		tempEchoDelay = GUI.TextField ( new Rect ( 175, 154, 30, 20 ), tempEchoDelay, 3 );
		tempEchoDelay = RegexToString ( tempEchoDelay, true );

		GUI.Label ( new Rect ( 210, 176, 110, 20 ), "Echo Decay Rate" );
		tempEchoDecayRate = GUI.TextField ( new Rect ( 175, 176, 30, 20 ), tempEchoDecayRate, 3 );
		tempEchoDecayRate = RegexToString ( tempEchoDecayRate, true );

		GUI.Label ( new Rect ( 210, 198, 100, 20 ), "Echo Wet Mix" );
		tempEchoWetMix = GUI.TextField ( new Rect ( 175, 198, 30, 20 ), tempEchoWetMix, 3 );
		tempEchoWetMix = RegexToString ( tempEchoWetMix, true );

		GUI.Label ( new Rect ( 210, 220, 100, 20 ), "Echo Dry Mix" );
		tempEchoDryMix = GUI.TextField ( new Rect ( 175, 220, 30, 20 ), tempEchoDryMix, 3 );
		tempEchoDryMix = RegexToString ( tempEchoDryMix, true );

#endregion

#region SlideshowSettings

		GUI.Box ( new Rect ( 10, 252, 150, 116 ), "Slideshow Settings" );
		
		tempSlideshow = GUI.HorizontalSlider ( new Rect ( 20, 280, 20, 14 ), UnityEngine.Mathf.Round ( tempSlideshow ), 0, 1 );
		GUI.Label ( new Rect ( 45, 274, 100, 22 ), "Slideshow" );
		
		tempAutoAVBlur = GUI.HorizontalSlider ( new Rect ( 20, 300, 20, 14 ), UnityEngine.Mathf.Round ( tempAutoAVBlur ), 0, 1 );
		GUI.Label ( new Rect ( 45, 294, 100, 22 ), "Force AV Blur" );
		
		tempAutoAVOff = GUI.HorizontalSlider ( new Rect ( 20, 320, 20, 14 ), UnityEngine.Mathf.Round ( tempAutoAVOff ), 0, 1 );
		GUI.Label ( new Rect ( 45, 314, 100, 22 ), "Force AV Off" );
		
		GUI.Label ( new Rect ( 45, 341, 80, 20 ), "Display Time" );
		displayTime = GUI.TextField ( new Rect ( 15, 342, 27, 20 ), displayTime, 3 );
		displayTime = RegexToString ( displayTime, true );

#endregion

#region Online Settings

		GUI.Box ( new Rect ( 170, 252, 170, 90 ), "Online Settings" );
		
		tempCheckForUpdates = GUI.HorizontalSlider ( new Rect ( 175, 277, 20, 14 ), UnityEngine.Mathf.Round ( tempCheckForUpdates ), 0, 1 );
		GUI.Label ( new Rect ( 200, 271, 115, 22 ), "Check For Updates" );
		
		tempEnableOMB = GUI.HorizontalSlider ( new Rect ( 175, 297, 20, 14 ), UnityEngine.Mathf.Round ( tempEnableOMB ), 0, 1 );
		GUI.Label ( new Rect ( 200, 290, 120, 22 ), "Enable Online Store" );
		
		if ( GUI.Button ( new Rect ( 175, 314, 160, 22 ), "Check For New Version" ))
		{
			
			startupManager.SendMessage ( "CheckForUpdate" );
			
			loadingImage.showLoadingImages = true;
			loadingImage.InvokeRepeating ("LoadingImages", 0, 0.25F);
			close = true;
		}

#endregion

		if ( GUI.Button ( new Rect ( 170, 346, 170, 22 ), "Reset Tutorials" ))
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
//		Thanks to http://andrew.hedges.name/experiments/aspect_ratio
		
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
				GUI.skin.button.hover.background = null;
			else
				GUI.skin.button.hover.background = guiHover;

			musicViewerPosition = GUI.Window ( 0, musicViewerPosition, MusicViewerPane, musicViewerTitle );
		}
	}


	IEnumerator LoadAssetBundle ( string assetBundleToOpen, string absongName )
	{
		
		Caching.CleanCache ();
	
		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( assetBundleToOpen + " | " + absongName );		
	
		WWW wwwClient = WWW.LoadFromCacheOrDownload ( assetBundleToOpen, 0 );
		yield return wwwClient;
		
		AssetBundleRequest request = wwwClient.assetBundle.LoadAsync ( absongName, typeof ( AudioClip ));
		yield return request;

		manager.audio.clip = request.asset as AudioClip;
		
		wwwClient.assetBundle.Unload ( false );
		Resources.UnloadUnusedAssets ();
		
		if ( manager.audio.clip.isReadyToPlay )	
		{
			
			currentSong.text = absongName;
				
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
			
			if ( startupManager.developmentMode == true )
				UnityEngine.Debug.Log ( "Playing audio" );
		}
		
		if ( wwwClient.error != null )
			UnityEngine.Debug.Log ( wwwClient.error );
	}


	void MusicViewerPane ( int wid )
	{

		if ( slideshow == false )
		{
			
			if ( hideGUI == false )
			{
				
				#region VolumeBar
				
				GUI.Label ( new Rect ( musicViewerPosition.width/2 - 100, musicViewerPosition.height/4 - 50, 100, 25), "Volume" );
				volumeBarValue = GUI.HorizontalSlider ( new Rect ( musicViewerPosition.width/2 - 118, musicViewerPosition.height/4 - 30, 100, 30 ), volumeBarValue, 0.0F, 1.0F );
				
				#endregion
				
		
				#region NextButton
		
				if ( GUI.Button ( new Rect ( musicViewerPosition.width/2 - 70, musicViewerPosition.height/4 - 15, 60, 30), "Next" ))
					NextSong ();
				
				#endregion
				
				
				#region BackButton
		
				if ( GUI.Button (new Rect ( musicViewerPosition.width/2 - 130, musicViewerPosition.height/4 - 15, 60, 30), "Back" ))
					PreviousSong ();
				
				#endregion
				
				
				#region LoopButton
				
				GUI.Label (new Rect ( musicViewerPosition.width/2 + 10, musicViewerPosition.height/4 - 50, 120, 30 ), "Loop" );
				
				if ( loop = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 5, musicViewerPosition.height/4 - 45, 100, 20 ), loop, "" ))
					if ( loop == true && shuffle == true || loop == true && continuous == true )
				{
					
					shuffle = false;
					continuous = false;
				}
				
				#endregion
				
				
				#region ShuffleButton
				
				GUI.Label ( new Rect ( musicViewerPosition.width/2 + 10, musicViewerPosition.height/4 - 30, 120, 30 ), "Shuffle" );
				
				if ( shuffle = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 5, musicViewerPosition.height/4 - 25, 100, 20 ), shuffle, "" ))
					if ( shuffle == true && loop == true || shuffle == true && continuous == true )
				{
					
					loop = false;
					continuous = false;
				}
				
				#endregion
				
				
				#region ContinuousPlay
				
				GUI.Label ( new Rect ( musicViewerPosition.width/2 + 10, musicViewerPosition.height/4 - 10, 120, 30 ), "Continuous" );
				
				if ( continuous = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 5, musicViewerPosition.height/4 - 5, 100, 20 ), continuous, "" ))
					if ( continuous == true && shuffle == true || continuous == true && loop == true )
					{
					
						shuffle = false;
						loop = false;
					}
				
				#endregion
				
		
				GUILayout.BeginHorizontal ();
				GUILayout.Space ( musicViewerPosition.width / 2 - 300 );
				GUILayout.BeginVertical ();
				GUILayout.Space ( musicViewerPosition.height / 4 + 25 );
				scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  musicViewerPosition.height - ( musicViewerPosition.height / 4 + 53 )));

				if ( clipList.Any ())
				{

					for ( i = 0; i < clipList.Length; i ++ )
					{
					
						string clipToPlay = clipList [ i ].Substring ( mediaPath.Length + 1 );
						string songName;
		
						bool isAssetBundle;
						if ( clipToPlay.Length > 8 )
						{
		
							if ( clipToPlay.Substring ( clipToPlay.Length - 7 ) == "unity3d" )
							{
		
								isAssetBundle = true;
								songName = clipToPlay.Substring ( 0, clipToPlay.Length - 8 );
							} else
							{
		
								isAssetBundle = false;
								songName = clipToPlay.Substring ( 0, clipToPlay.Length - 4 );
							}
						} else
						{
							
							isAssetBundle = false;
							songName = clipToPlay.Substring ( 0, clipToPlay.Length - 4 );
						}
		
						if ( showTypes == false )
							clipToPlay = songName;
		
						if ( GUILayout.Button ( clipToPlay ))
						{
	
							Resources.UnloadUnusedAssets ();
							
							currentSongNumber = i;
							previousSongs [ 0 ] = previousSongs [ 1 ];
							previousSongs [ 1 ] = previousSongs [ 2 ];
							previousSongs [ 2 ] = previousSongs [ 3 ];
							previousSongs [ 3 ] = previousSongs [ 4 ];
							previousSongs [ 4 ] = previousSongs [ 5 ];
							previousSongs [ 5 ] = previousSongs [ 6 ];
							previousSongs [ 6 ] = i;
							psPlace = 6;
							
							wasPlaying = false;
							
							if ( isAssetBundle == true )
							{
		
								loadingImage.showLoadingImages = true;
								loadingImage.InvokeRepeating ( "LoadingImages", 0.25F, 0.25F );
								
								songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1 );
								StartCoroutine ( LoadAssetBundle ( "file://" + clipList [ currentSongNumber ], songName.Substring ( 0, songName.Length - 8 )));	
		
							} else
							{
									
								StartCoroutine ( PlayAudio ());
								loadingImage.showLoadingImages = false;
							}
						}
					}
				} else
				{
		
					GUILayout.Label ( "\nYou don't have any music to play!\n\nIf you have some music (.wav or .ogg), navigate\nto the MusicManager (press the left arrow key)." +
						"\n\nYou can also download music by navigating\nto the OnlineMusicBrowser (press the right arrow key).\n", centerStyle );
						
					if ( GUILayout.Button ( "View Help/Tutorial" ))
					{
						
						Process.Start ( startupManager.helpPath );
					}
				}
	
				GUILayout.Box ( "System Commands" );
					
				if ( showQuickManage == true )
					if ( GUILayout.Button ( "Open Media Folder" ))
						Process.Start ( mediaPath );
							
				if ( GUILayout.Button ( "Options" ))
					showOptionsWindow = true;
				
				GUI.EndScrollView();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
			
			
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
	
			if ( showOptionsWindow == true || startupManager.showUnderlay == true )
				GUI.DrawTexture ( new Rect ( 0, 0, musicViewerPosition.width, musicViewerPosition.height ), startupManager.underlay );
		}
	}


	void NextSong ()
	{

		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( "Next Song" );

		if ( clipList.Any ())
		{

			wasPlaying = false;
			if ( psPlace < 6 )
			{

				psPlace += 1;
				currentSongNumber = previousSongs [ psPlace ];
			
				if ( clipList [ currentSongNumber ].Substring ( clipList [ currentSongNumber ].Length - 7 ) == "unity3d" )
				{
	
					string songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1 );
					StartCoroutine ( LoadAssetBundle ( "file://" + clipList [ currentSongNumber ], songName.Substring ( 0, songName.Length - 8 )));
					
					loadingImage.showLoadingImages = true;
					loadingImage.InvokeRepeating ("LoadingImages", 0.25F, 0.25F);
				} else {

					StartCoroutine ( PlayAudio() );
				}
			} else {

				if ( continuous == true || loop == false && shuffle == false && continuous == false )
				{

					if ( currentSongNumber == clipList.Length - 1 )
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
			
					if ( clipList [ currentSongNumber ].Substring ( clipList [ currentSongNumber ].Length - 7 ) == "unity3d" )
					{
				
						string songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1 );
						StartCoroutine ( LoadAssetBundle ( "file://" + clipList [ currentSongNumber ], songName.Substring ( 0, songName.Length - 8 )));
					
						loadingImage.showLoadingImages = true;
						loadingImage.InvokeRepeating ("LoadingImages", 0.25F, 0.25F);
					} else {
						
						StartCoroutine ( PlayAudio() );
					}
				} else
				{

					if ( loop == true )
					{

						if ( manager.audio.clip == null )
						{

							if ( currentSongNumber == clipList.Length - 1 )
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

							if ( clipList [ currentSongNumber ].Substring ( clipList [ currentSongNumber ].Length - 7 ) == "unity3d" )
							{

								string songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1 );
								StartCoroutine ( LoadAssetBundle ( "file://" + clipList [ currentSongNumber ], songName.Substring ( 0, songName.Length - 8 )));
					
								loadingImage.showLoadingImages = true;
								loadingImage.InvokeRepeating ("LoadingImages", 0.25F, 0.25F);
							} else {
							
								StartCoroutine ( PlayAudio() );
							}
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
					} else
					{

						if ( shuffle == true )
						{

							Resources.UnloadUnusedAssets ();
							
							int previousSongNumber = currentSongNumber;
							currentSongNumber = UnityEngine.Random.Range ( 0, clipList.Length );
					
							if ( currentSongNumber == previousSongNumber && clipList.Length > 1 )
							{
								
								bool shuffleOkay = false;
								while ( shuffleOkay == false )
								{
									
									currentSongNumber = UnityEngine.Random.Range ( 0, clipList.Length );
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

							if ( clipList [ currentSongNumber ].Substring ( clipList [ currentSongNumber ].Length - 7 ) == "unity3d" )
							{

								string songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1 );
								StartCoroutine ( LoadAssetBundle ( "file://" + clipList [ currentSongNumber ], songName.Substring ( 0, songName.Length - 8 )));
					
								loadingImage.showLoadingImages = true;
								loadingImage.InvokeRepeating ("LoadingImages", 0.25F, 0.25F);
							} else {
						
								StartCoroutine ( PlayAudio() );
							}
						}
					}
				}
			}
		}
	}


	void PreviousSong ()
	{

		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( "Previous Song" );

		if ( clipList.Any ())
		{

			wasPlaying = false;
			if ( psPlace <= 0 )
			{

				currentSongNumber = UnityEngine.Random.Range ( 0, clipList.Length );
				StartCoroutine ( PlayAudio() );
			} else
			{
			
				psPlace -= 1;
				currentSongNumber = previousSongs [ psPlace ];
			
				if ( clipList [ currentSongNumber ].Substring ( clipList [ currentSongNumber ].Length - 7 ) == "unity3d" )
				{

					string songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1 );
					StartCoroutine ( LoadAssetBundle ( "file://" + clipList [ currentSongNumber ], songName.Substring ( 0, songName.Length - 8 )));	
				} else {
				
					StartCoroutine ( PlayAudio() );
				}
			}
		}
	}


	IEnumerator PlayAudio ()
	{
	
		manager.audio.Stop ();

		rawCurrentSong = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1 );
		
		if ( slideshow == false )
			currentSong.text = rawCurrentSong.Substring ( 0, rawCurrentSong.Length -4 );
		
		songLocation = clipList [ currentSongNumber ];
		
		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( "Preparing to play " + rawCurrentSong.Substring ( 0, rawCurrentSong.Length -4 ));
		
		WWW www = new WWW ( "file://" + songLocation );
		yield return www;

		manager.audio.clip = www.GetAudioClip ( false );
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
				
			if ( startupManager.developmentMode == true )
				UnityEngine.Debug.Log ( "Playing audio" );
		}

		if ( www.error != null )
			UnityEngine.Debug.Log ( www.error );
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
			musicManager.StartCoroutine ( "SetArtwork" );
			
			tempSlideshow = Convert.ToSingle ( slideshow );
			if ( showTimebar == false )
				musicViewerTitle = "MusicViewer";
			
			timemark.enabled = true;
			hideGUI = false;
			
			manager.GetComponent<BlurEffect> ().enabled = blur;
			
			if ( manager.audio.clip != null )
			{
				
				currentSong.text = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1, rawCurrentSong.Length -4  );
				
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
				if ( continuous == true )
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
		
		psPlace = 6;

		if ( continuous == true )
		{
			
			if ( currentSongNumber == clipList.Length - 1 )
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

			if ( clipList [ currentSongNumber ].Substring ( clipList [ currentSongNumber ].Length - 7 ) == "unity3d" )
			{

				string songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1 );
				StartCoroutine ( LoadAssetBundle ( "file://" + clipList [ currentSongNumber ], songName.Substring ( 0, songName.Length - 8 )));
			} else
				StartCoroutine ( PlayAudio() );

		} else
		{
			
			if ( loop == true )
			{
					
				rtMinutes = new int ();
				rtSeconds = new int ();
					
				manager.audio.Play ();
				isPaused = false;
				wasPlaying = true;
					
				if ( startupManager.developmentMode == true )
					UnityEngine.Debug.Log ( "Playing audio" );
					
			} else
			{
				
				if ( shuffle == true )
				{
					
					Resources.UnloadUnusedAssets ();
					
					int previousSongNumber = currentSongNumber;
					currentSongNumber = UnityEngine.Random.Range ( 0, clipList.Length );
					
					if ( currentSongNumber == previousSongNumber && clipList.Length > 1 )
					{
						
						bool shuffleOkay = false;
						while ( shuffleOkay == false )
						{
							
							currentSongNumber = UnityEngine.Random.Range ( 0, clipList.Length );
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

					if ( clipList [ currentSongNumber ].Substring ( clipList [ currentSongNumber ].Length - 7 ) == "unity3d" )
					{

						string songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length+ 1 );
						StartCoroutine ( LoadAssetBundle ( "file://" + clipList [ currentSongNumber ], songName.Substring ( 0, songName.Length - 8 )));
					} else

						StartCoroutine ( PlayAudio() );

				} else
				{

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
		}
	}


	void Quit ()
	{

		wasPlaying = false;
		manager.audio.Stop ();
		
		Resources.UnloadUnusedAssets ();
		Caching.CleanCache ();

		TextWriter savePrefs = new StreamWriter ( startupManager.prefsLocation );
		savePrefs.WriteLine ( mediaPath + "\n" + startupManager.checkForUpdate + "\n" + startupManager.ombEnabled + "\n" + startupManager.showTutorials + "\n" + loop + "\n" + shuffle + "\n" + continuous + "\n" + showTypes + "\n" + showArrows + "\n" + showTimebar + "\n" + showArtwork + "\n" + showQuickManage + "\n" + preciseTimemark + "\n" + volumeBarValue + "\n" + avcR + "\n" + avcG + "\n" + avcB + "\n" + bloom + "\n" + blur + "\n" + sunShafts + 
		                     "\n" + blurIterations + "\n" + echoDelay + "\n" + echoDecayRate + "\n" + echoWetMix + "\n" + echoDryMix + "\n" + autoAVBlur + "\n" + autoAVOff + "\n" + displayTime + "\n" + previousSongs [ 0 ] + "\n" + previousSongs [ 1 ] + "\n" + previousSongs [ 2 ] + "\n" + previousSongs [ 3 ] + "\n" + previousSongs [ 4 ] + "\n" + previousSongs [ 5 ] + "\n" + previousSongs [ 6 ] );
		savePrefs.Close ();

		if ( Application.isEditor == true )
			UnityEngine.Debug.Log ( "Quit has been called" );
		else
			Application.Quit ();
	}
}
