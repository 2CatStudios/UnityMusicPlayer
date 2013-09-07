using System;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
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

//-------
	
	bool isPaused;	
	float pausePoint;
	int songTime;
	int minutes;
	float seconds;
	int rtMinutes;
	float rtSeconds;

	internal bool wasPlaying = false;

	float tempPreciseTimemark;
	internal bool preciseTimemark;
	
	internal GUIText timemark;
	float timebarTime;

	float betweenSongDelay = 0.5F;

	internal String [ ] clipList;
	internal bool clipListEmpty;
	int currentSongNumber = -1;
	int i;
	
	Vector2 scrollPosition;
	Vector2 mousePos;
	
//-------
	
	public string songLocation;

	string rawCurrentSong;
	public GUIText currentSong;

//-------

	public Texture2D underlay;

	bool streaming = false;
	bool showStreamingWindow = false;
	Rect streamingWindowRect = new Rect ( 0, 0, 350, 70 );
	bool streamingConnectionError = false;
	string streamingConnectionErrorText = "";
	string streamingLink = "";
	bool dispose = false;

	bool hideGUI = false;
	public GUISkin GuiSkin;
	public Texture2D popupWindowTexture;
	
	int [ ] previousSongs = new int  [ 7 ] { 0, 0, 0, 0, 0, 0, 0 };
	int psPlace = 6;
	
	bool loop = false;
	bool shuffle = false;
	bool continuous = false;
	
	float volumeBarValue = 1.0F;
	
	bool echo = false;
	string tempEchoDelay = "100";
	string tempEchoDecayRate = "0.3";
	string tempEchoWetMix = "0.8";
	string tempEchoDryMix = "0.6";

	bool showOptionsWindow = false;
	Rect optionsWindowRect = new Rect ( 0, 0, 350, 250 );
	bool showTypes;
	float tempShowTypes;
	float avcR = 0.9886364F;
	float tempAVCR = 0.9886364F;
	float avcG = 0.5227273F;
	float tempAVCG = 0.5227273F;
	float avcB = 0.1704545F;
	float tempAVCB = 0.1704545F;
	
	float tempBloom = 0.0F;
	bool bloom = false;
	
	float tempMotionBlur = 0.0F;
	bool motionBlur = false;
	
	float tempSunShafts = 0.0F;
	bool sunShafts = false;

	float tempSlideshow = 0;
	internal bool slideshow = false;
	
	float tempShowQuickManage = 0.0F;
	bool showQuickManage = false;


	string[] slideshowImageLocations;
	GUITexture currentSlideshowImage;
	Texture2D newSlideshowImage;
	float fadeVelocity = 0.0F;
	bool fadeIn = false;
	bool fadeOut = false;

	int slideshowImage = 0;


//	String audioInput;
//	bool pickInput;

//-------

	internal string mediaPath;
	string prefsLocation;

	string musicViewerTitle;
	internal Rect musicViewerPosition = new Rect ( 0, 0, 800, 600 );

	bool showVisualizer = false;
	bool halfSpeed = false;
	bool doubleSpeed = false;

//-------

	public Texture2D timebarMarker;
	float tempShowTimebar;
	internal bool showTimebar;

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
		prefsLocation = startupManager.supportPath + "Preferences.umpp";
		currentSlideshowImage = GameObject.FindGameObjectWithTag ( "SlideshowImage" ).GetComponent<GUITexture>();

		musicViewerPosition.width = Screen.width;
		musicViewerPosition.height = Screen.height;

		streamingWindowRect.x = musicViewerPosition.width/2 - streamingWindowRect.width/2;
		streamingWindowRect.y = musicViewerPosition.height/2 - streamingWindowRect.height/2;

		optionsWindowRect.x = musicViewerPosition.width/2 - optionsWindowRect.width/2;
		optionsWindowRect.y = musicViewerPosition.height/2 - optionsWindowRect.height/2;

		audioVisualizerR = GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizerR> ();
		audioVisualizerL = GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizerL> ();

		timemark = GameObject.FindGameObjectWithTag ( "Timemark" ).GetComponent<GUIText> ();
		GameObject.FindGameObjectWithTag ( "TimebarImage" ).guiTexture.pixelInset = new Rect ( -musicViewerPosition.width/2, musicViewerPosition.height/2 - 3, musicViewerPosition.width, 6 );

		clipList = Directory.GetFiles ( mediaPath, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();

		if ( clipList.Length == 0 || clipList == null )
			clipListEmpty = true;
		else
			clipListEmpty = false;
		
		string [ ] prefs = File.ReadAllLines ( prefsLocation );
		
		loop = Convert.ToBoolean ( prefs [ 1 ] );
		shuffle = Convert.ToBoolean ( prefs [ 2 ] );
		continuous = Convert.ToBoolean ( prefs [ 3 ] );

		showTypes = Convert.ToBoolean ( prefs [ 4 ] );
		
		showTimebar = Convert.ToBoolean ( prefs [ 5 ] );
		tempShowTimebar = Convert.ToSingle ( showTimebar );
		
		showQuickManage = Convert.ToBoolean ( prefs [ 6 ] );
		tempShowQuickManage = Convert.ToSingle ( showQuickManage );
		
		preciseTimemark = Convert.ToBoolean ( prefs [ 7 ] );
		tempPreciseTimemark = Convert.ToSingle ( preciseTimemark );
		
		volumeBarValue = Convert.ToSingle ( prefs [ 8 ] );

		avcR = float.Parse ( prefs [ 9 ] );
		tempAVCR = avcR;
		
		avcG = float.Parse ( prefs [ 10 ] );
		tempAVCG = avcG;
		
		avcB = float.Parse ( prefs [ 11 ] );
		tempAVCB = avcB;

		bloom = Convert.ToBoolean ( prefs [ 12 ] );	
		tempBloom = Convert.ToSingle ( bloom );
		
		motionBlur = Convert.ToBoolean ( prefs [ 13 ] );
		tempMotionBlur = Convert.ToSingle ( motionBlur );
		
		sunShafts = Convert.ToBoolean ( prefs [ 14 ] );
		tempSunShafts = Convert.ToSingle ( sunShafts );

		tempEchoDelay = prefs [ 15 ];
		tempEchoDecayRate = prefs [ 16 ];
		tempEchoWetMix = prefs [ 17 ];
		tempEchoDryMix = prefs [ 18 ];

		previousSongs [ 0 ] = Convert.ToInt32 ( prefs [ 19 ] );
		if ( previousSongs [ 0 ] > clipList.Length )
			previousSongs [ 0 ] = clipList.Length;
		
		previousSongs [ 1 ] = Convert.ToInt32 ( prefs [ 20 ] );
		if ( previousSongs [ 1 ] > clipList.Length )
			previousSongs [ 1 ] = clipList.Length;
		
		previousSongs [ 2 ] = Convert.ToInt32 ( prefs [ 21 ] );
		if ( previousSongs [ 2 ] > clipList.Length )
			previousSongs [ 2 ] = clipList.Length;
		
		previousSongs [ 3 ] = Convert.ToInt32 ( prefs [ 22 ] );
		if ( previousSongs [ 3 ] > clipList.Length )
			previousSongs [ 3 ] = clipList.Length;
		
		previousSongs [ 4 ] = Convert.ToInt32 ( prefs [ 23 ] );
		if ( previousSongs [ 4 ] > clipList.Length )
			previousSongs [ 4 ] = clipList.Length;
		
		previousSongs [ 5 ] = Convert.ToInt32 ( prefs [ 24 ] );
		if ( previousSongs [ 5 ] > clipList.Length )
			previousSongs [ 5 ] = clipList.Length;
		
		previousSongs [ 6 ] = Convert.ToInt32 ( prefs [ 25 ] );
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


		TextWriter savePrefs = new StreamWriter ( prefsLocation );
		savePrefs.WriteLine ( mediaPath + "\n" + loop + "\n" + shuffle + "\n" + continuous + "\n" + showTypes + "\n" + showTimebar + "\n" + showQuickManage + "\n" + preciseTimemark + "\n" + volumeBarValue + "\n" + avcR + "\n" + avcG + "\n" + avcB + "\n" + bloom + "\n" + motionBlur + "\n" + sunShafts + 
		                     "\n" + tempEchoDelay + "\n" + tempEchoDecayRate + "\n" + tempEchoWetMix + "\n" + tempEchoDryMix + "\n" + previousSongs [ 0 ] + "\n" + previousSongs [ 1 ] + "\n" + previousSongs [ 2 ] + "\n" + previousSongs [ 3 ] + "\n" + previousSongs [ 4 ] + "\n" + previousSongs [ 5 ] + "\n" + previousSongs [ 6 ] );
		savePrefs.Close ();
		
		InvokeRepeating ( "Refresh", 0, 2 );
	}
	
	
	void Refresh ()
	{

		if ( paneManager.currentPane == PaneManager.pane.musicViewer )
		{
			
			clipList = Directory.GetFiles ( mediaPath, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();

			if ( clipList.Length == 0 || clipList == null )
				clipListEmpty = true;
			else
				clipListEmpty = false;
		}
	}


	void OptionsWindow ( int wid )
	{

		GUI.FocusWindow ( 6 );
		GUI.BringWindowToFront ( 6 );

		if ( showStreamingWindow == true )
		{

			GUI.FocusWindow ( 0 );
			GUI.BringWindowToFront ( 0 );
			paneManager.popupBlocking = false;
			streamingWindowRect.height = 70;
			streamingConnectionError = false;
			streamingConnectionErrorText = "";
			showStreamingWindow = false;
		}

		if ( GUI.Button ( new Rect ( 290, 20, 50, 20 ), "Close" ))
		{

			if ( tempEchoDelay.Trim () == "" )
				tempEchoDelay = "20";

			if ( tempEchoDecayRate.Trim () == "" )
				tempEchoDecayRate = "0.1";

			if ( tempEchoWetMix.Trim () == "" )
				tempEchoWetMix = "0.1";

			if ( tempEchoDryMix.Trim () == "" )
				tempEchoDryMix = "0.1";

			manager.GetComponent<AudioEchoFilter> ().delay = Convert.ToSingle ( tempEchoDelay );
			manager.GetComponent<AudioEchoFilter> ().decayRatio = Convert.ToSingle ( tempEchoDecayRate );
			manager.GetComponent<AudioEchoFilter> ().wetMix = Convert.ToSingle ( tempEchoWetMix );
			manager.GetComponent<AudioEchoFilter> ().dryMix = Convert.ToSingle ( tempEchoDryMix );

			avcR = tempAVCR;
			avcG = tempAVCG;
			avcB = tempAVCB;
			
			bloom = Convert.ToBoolean ( tempBloom );
			motionBlur = Convert.ToBoolean ( tempMotionBlur );
			sunShafts = Convert.ToBoolean ( tempSunShafts );

			showTypes = Convert.ToBoolean ( tempShowTypes );
			
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

			showQuickManage = Convert.ToBoolean ( tempShowQuickManage );
			
			preciseTimemark = Convert.ToBoolean ( tempPreciseTimemark );

			slideshow = Convert.ToBoolean ( tempSlideshow );
			if ( slideshow == true )
			{

				if ( showTimebar == false )
					musicViewerTitle = "";
				
				timemark.enabled = false;
				currentSong.text = "";
				hideGUI = true;
				
				StartCoroutine ( "SlideshowIN" );
			}

			GUI.FocusWindow ( 0 );
			GUI.BringWindowToFront ( 0 );
			paneManager.popupBlocking = false;
			showOptionsWindow = false;
		}

#region AudioVisualizerSettings

		GUI.Box ( new Rect ( 10, 20, 150, 198 ), "AudioVisualizer Settings" );

		GUI.Label ( new Rect ( 50, 39, 40, 30 ), "Red" );
		tempAVCR = GUI.HorizontalSlider ( new Rect ( 25, 62, 100, 15), tempAVCR, 0.0F, 1.000F );

		GUI.Label ( new Rect ( 50, 69, 40, 30 ), "Green" );
		tempAVCG = GUI.HorizontalSlider ( new Rect ( 25, 92, 100, 15), tempAVCG, 0.0F, 1.000F );

		GUI.Label ( new Rect ( 50, 99, 40, 30 ), "Blue" );
		tempAVCB = GUI.HorizontalSlider ( new Rect ( 25, 122, 100, 15), tempAVCB, 0.0F, 1.000F );

		GUI.contentColor = new Color ( tempAVCR, tempAVCG, tempAVCB, 1.000F );
		GUI.Label ( new Rect ( 35, 137, 80, 20 ), "Sample Color");
		GUI.contentColor = Color.white;
	
		tempBloom = GUI.HorizontalSlider ( new Rect ( 20, 165, 20, 14 ), UnityEngine.Mathf.Round ( tempBloom ), 0, 1 );
		GUI.Label ( new Rect ( 43, 157, 80, 22 ), "Toggle Bloom" );

		tempMotionBlur = GUI.HorizontalSlider ( new Rect ( 20, 182, 20, 14 ), UnityEngine.Mathf.Round ( tempMotionBlur ), 0, 1 );
		GUI.Label ( new Rect ( 36, 175, 125, 22 ), "Toggle Motion Blur" );

		tempSunShafts = GUI.HorizontalSlider ( new Rect ( 20, 199, 20, 14 ), UnityEngine.Mathf.Round ( tempSunShafts ), 0, 1 );
		GUI.Label ( new Rect ( 38, 193, 120, 22 ), "Toggle Sun Shafts" );

#endregion

		tempSlideshow = GUI.HorizontalSlider ( new Rect ( 170, 22, 20, 14 ), UnityEngine.Mathf.Round ( tempSlideshow ), 0, 1 );
		GUI.Label ( new Rect ( 174, 14, 100, 22 ), "Slideshow" );
		
		tempShowTypes = GUI.HorizontalSlider ( new Rect ( 170, 39, 20, 14 ), UnityEngine.Mathf.Round ( tempShowTypes ), 0, 1 );
		GUI.Label ( new Rect ( 180, 31, 100, 22 ), "Show Types" );
		
		tempShowTimebar = GUI.HorizontalSlider ( new Rect ( 170, 56, 20, 14 ), UnityEngine.Mathf.Round ( tempShowTimebar ), 0, 1 );
		GUI.Label ( new Rect ( 186, 49, 100, 22 ), "Show Timebar" );
		
		tempShowQuickManage = GUI.HorizontalSlider ( new Rect ( 170, 73, 20, 14 ), UnityEngine.Mathf.Round ( tempShowQuickManage ), 0, 1 );
		GUI.Label ( new Rect ( 195, 67, 116, 22 ), "Show QuickManage" );
		
		tempPreciseTimemark = GUI.HorizontalSlider ( new Rect ( 170, 90, 20, 22), UnityEngine.Mathf.Round ( tempPreciseTimemark ), 0, 1 );
		GUI.Label ( new Rect ( 192, 85, 145, 22 ), "Show Precise Timemark" );

#region AudioSettings

		GUI.Box ( new Rect ( 170, 108, 170, 110 ), "Audio Echo Settings" );

		GUI.Label ( new Rect ( 200, 128, 80, 20 ), "Echo Delay" );
		tempEchoDelay = GUI.TextField ( new Rect ( 175, 128, 30, 20 ), tempEchoDelay, 3 );

		GUI.Label ( new Rect ( 204, 150, 110, 20 ), "Echo Decay Rate" );
		tempEchoDecayRate = GUI.TextField ( new Rect ( 175, 150, 30, 20 ), tempEchoDecayRate, 3 );

		GUI.Label ( new Rect ( 198, 172, 100, 20 ), "Echo Wet Mix" );
		tempEchoWetMix = GUI.TextField ( new Rect ( 175, 172, 30, 20 ), tempEchoWetMix, 3 );

		GUI.Label ( new Rect ( 196, 194, 100, 20 ), "Echo Dry Mix" );
		tempEchoDryMix = GUI.TextField ( new Rect ( 175, 194, 30, 20 ), tempEchoDryMix, 3 );

#endregion

		GUI.Box ( new Rect ( 10, 222, 330, 22 ), "UnityMusicPlayer Version " + startupManager.runningVersion );
	}
	

	IEnumerator SlideshowIN ()
	{

		slideshowImageLocations = Directory.GetFiles ( startupManager.slideshowPath, "*.*" ).Where ( s => s.EndsWith ( ".png" ) || s.EndsWith ( ".jpg" ) || s.EndsWith ( ".jpeg" )).ToArray ();

		WWW wWw = new WWW ( "file://" + slideshowImageLocations [ slideshowImage ] );
		yield return wWw;

		newSlideshowImage = new Texture2D (( int ) musicViewerPosition.height, ( int ) musicViewerPosition.height, TextureFormat.ARGB32, false );
		wWw.LoadImageIntoTexture ( newSlideshowImage );
		currentSlideshowImage.texture = newSlideshowImage;

		fadeIn = true;

		yield return new WaitForSeconds ( 7 );

		slideshowImage += 1;
		if ( slideshowImage == slideshowImageLocations.Length )
			slideshowImage = 0;

		fadeOut = true;
	}


	void OnGUI ()
	{
		
		if ( showMusicViewer == true )
		{

			if ( manager.audio.clip != null && showTimebar == true )
				GUI.DrawTexture ( new Rect ( manager.audio.time * ( musicViewerPosition.width/manager.audio.clip.length ), -3, 10, 6 ), timebarMarker );

			if ( showStreamingWindow == true )
			{

				paneManager.popupBlocking = true;
				GUI.skin.window.normal.background = popupWindowTexture;
				GUI.Window ( 5, streamingWindowRect, StreamingWindow, "Web and Disk Streaming" );
			}

			if ( showOptionsWindow == true )
			{

				paneManager.popupBlocking = true;
				GUI.skin.window.normal.background = popupWindowTexture;
				GUI.Window ( 6, optionsWindowRect, OptionsWindow, "Options and Settings" );
			}
		}
		
		if ( slideshow == true )
		{
			
			GUI.Box ( new Rect ( musicViewerPosition.width - 100, musicViewerPosition.height - 65, 90, 20 ), ""  );
			slideshow = GUI.Toggle ( new Rect ( musicViewerPosition.width - 95, musicViewerPosition.height - 65, 80, 20 ), slideshow, "Slideshow" );
			
			if ( slideshow == false )
			{
				
				tempSlideshow = Convert.ToSingle ( slideshow );
				if ( showTimebar == false )
					musicViewerTitle = "MusicViewer";
				
				timemark.enabled = true;
				hideGUI = false;
				
				if ( manager.audio.clip != null )
				{
					
					currentSong.text = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1, rawCurrentSong.Length -4  );
					
				} else {
					
					currentSong.text = "UnityMusicPlayer";
				}
				
				StopCoroutine ( "SlideshowIN" );
				newSlideshowImage = null;
				currentSlideshowImage.texture = null;
				currentSlideshowImage.color = new Color ( 0.5F, 0.5F, 0.5F, 0.0F );
				fadeIn = false;
			}
		}
		
		GUI.skin = GuiSkin;
		musicViewerPosition = GUI.Window ( 0, musicViewerPosition, MusicViewerPane, musicViewerTitle );
		
		if ( GUI.Button ( new Rect ( musicViewerPosition.width - 75, musicViewerPosition.height - 40, 60, 30 ), "Quit" ))
			Quit ();
	}


	void StreamingWindow ( int wid )
	{
		
		GUI.FocusWindow ( 5 );
		GUI.BringWindowToFront ( 5 );
		
		if ( GUI.Button ( new Rect ( 275, 20, 55, 20 ), "Close" ))
		{

			GUI.FocusWindow ( 0 );
			GUI.BringWindowToFront ( 0 );
			dispose = true;
			loadingImage.showLoadingImages = false;
			paneManager.popupBlocking = false;
			streamingWindowRect.height = 70;
			streamingConnectionError = false;
			streamingConnectionErrorText = "";
			streamingLink = "";
			showStreamingWindow = false;
		}
		
		GUI.Label ( new Rect ( -36, 17, 340, 25 ), "Input the link to an audio file in the textfield." );
		
		streamingLink = GUI.TextField ( new Rect ( 45, 45, 300, 20 ), streamingLink.Trim ());
		
		if ( GUI.Button ( new Rect ( 6, 45, 35, 20 ), "Go" ))
		{

			minutes = 0;
			seconds = 0;
			rtMinutes = 0;
			rtSeconds = 0;
			if ( !streamingLink.StartsWith ( "http://" ))
				streamingLink = streamingLink.Insert ( 0, "file://" );

			dispose = false;
			StartCoroutine ( GetStreamingClip ( streamingLink ));
		}

		if ( streamingConnectionError == true )
			GUI.Label ( new Rect ( 0, 65, 300, 25 ), streamingConnectionErrorText );
	}


	IEnumerator GetStreamingClip ( string link )
	{

		loadingImage.showLoadingImages = true;
		loadingImage.InvokeRepeating ("LoadingImages", 0.25F, 0.25F);

		WWW www = new WWW ( link );
		AudioClip clip = www.GetAudioClip ( false, true, AudioType.WAV );
		yield return www;

		if ( dispose == false )
		{

			if ( www.error != null )
			{

				loadingImage.showLoadingImages = false;

				UnityEngine.Debug.Log ( www.error );
				streamingWindowRect.height = 90;

				if ( !String.IsNullOrEmpty ( www.error ))
				{

					streamingConnectionErrorText = "Error: " + www.error;
					UnityEngine.Debug.Log ( www.error );
				} else
					streamingConnectionErrorText = "Error: There was an error streaming your file.";

				streamingConnectionError = true;
			} else {

				loadingImage.showLoadingImages = false;
				showStreamingWindow = false;
				streamingWindowRect.height = 70;
				streamingConnectionError = false;
				streamingConnectionErrorText = "";
				dispose = false;
			
				if ( manager.audio.clip.isReadyToPlay )
				{
					
					currentSong.text = "UnityMusicPlayer";
					timemark.text = "Streaming][Streaming";
				
					manager.audio.clip = clip;

					minutes = 0;
					seconds = 0;
					rtMinutes = 00;
					rtSeconds = 00;

					streaming = true;
					manager.audio.Play ();
					wasPlaying = true;
					isPaused = false;
					
					Resources.UnloadUnusedAssets ();
				
					if ( startupManager.developmentMode == true )
						UnityEngine.Debug.Log ( "Playing audio" );
				}
			
			if ( www.error != null )
				UnityEngine.Debug.Log ( www.error );
			}
		} else {

			www.Dispose();
			Resources.UnloadUnusedAssets ();
			dispose = false;
		}
	}


	IEnumerator LoadAssetBundle ( string assetBundleToOpen, string absongName )
	{

		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( assetBundleToOpen + " | " + absongName );

		WWW wwwClient = WWW.LoadFromCacheOrDownload ( assetBundleToOpen, 1 );
		yield return wwwClient;
		
		AssetBundle bundle = wwwClient.assetBundle;
		
		AssetBundleRequest request = bundle.LoadAsync ( absongName, typeof ( AudioClip ));
		yield return request;

		AudioClip aClip = request.asset as AudioClip;
		bundle.Unload ( false );

		manager.audio.clip = aClip;
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
			scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  musicViewerPosition.height - ( musicViewerPosition.height / 4 + 56 )));
	
			if ( clipListEmpty == false )
			{
	
				if ( hideGUI == false )
				{
				
					for ( i = 0; i < clipList.Length; i ++ )
					{
					
						string pathToFile = clipList [ i ];
						string clipToPlay = clipList [ i ].Substring ( mediaPath.Length + 1 );
						string songName;
	
						bool isAssetBundle;
						if ( clipToPlay.Length > 8 )
						{
	
							if ( clipToPlay.Substring ( clipToPlay.Length - 7 ) == "unity3d" )
							{
	
								isAssetBundle = true;
								songName = clipToPlay.Substring ( 0, clipToPlay.Length - 8 );
							} else {
	
								isAssetBundle = false;
								songName = clipToPlay.Substring ( 0, clipToPlay.Length - 4 );
							}
						} else {
							isAssetBundle = false;
							songName = clipToPlay.Substring ( 0, clipToPlay.Length - 4 );
						}
	
						if ( showTypes == false )
							clipToPlay = songName;
	
						if ( GUILayout.Button ( clipToPlay ))
						{
	
							streaming = false;
	
							if ( isAssetBundle == true )
							{
	
								StartCoroutine ( LoadAssetBundle ( "file://" + pathToFile, songName));
								loadingImage.showLoadingImages = true;
								loadingImage.InvokeRepeating ("LoadingImages", 0.25F, 0.25F);
	
							} else {
	
								currentSongNumber = i;
								previousSongs [ 0 ] = previousSongs [ 1 ];
								previousSongs [ 1 ] = previousSongs [ 2 ];
								previousSongs [ 2 ] = previousSongs [ 3 ];
								previousSongs [ 3 ] = previousSongs [ 4 ];
								previousSongs [ 4 ] = previousSongs [ 5 ];
								previousSongs [ 5 ] = previousSongs [ 6 ];
								previousSongs [ 6 ] = i;
								psPlace = 6;
								
								Resources.UnloadUnusedAssets ();
								wasPlaying = false;
								StartCoroutine ( PlayAudio());
							}
						}
					}
	
				}
			} else
			{
	
				GUI.skin.label.alignment = TextAnchor.MiddleCenter;
				GUILayout.Label ( "\nYou don't have any music to play!\n\nIf you have some music (.wav or .ogg), navigate\nto the MusicManager (press the left arrow key)." +
					"\n\nYou can also download music by navigating to the OnlineMusicBrowser (press the right arrow key),\nor stream music by clicking the 'Streaming' button bellow.\n" );
				GUI.skin.label.alignment = TextAnchor.UpperLeft;
			}
	
			if ( hideGUI == false )
			{
	
				GUILayout.Box ( "System Commands" );
				
				if ( showQuickManage == true )
					if ( GUILayout.Button ( "Open Media Folder" ))
						Process.Start ( mediaPath );
				
				if ( GUILayout.Button ( "Streaming" ))
					showStreamingWindow = true;
	
				if ( GUILayout.Button ( "Options" ))
					showOptionsWindow = true;
			}
			
			GUI.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			
			
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
				manager.GetComponent<MotionBlur>().enabled = Convert.ToBoolean ( motionBlur );
				manager.GetComponent<SunShafts>().enabled = Convert.ToBoolean ( sunShafts );
			} else {
	
				audioVisualizerR.showAV = showVisualizer;
				audioVisualizerL.showAV = showVisualizer;
	
				manager.GetComponent<BloomAndLensFlares>().enabled = false;
				manager.GetComponent<MotionBlur>().enabled = false;
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
	
			if ( showOptionsWindow == true || showStreamingWindow == true || startupManager.showUnderlay == true )
				GUI.DrawTexture ( new Rect ( 0, 0, musicViewerPosition.width, musicViewerPosition.height ), underlay );
		}
	}


	void NextSong ()
	{

		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( "Next Song" );

		if ( clipListEmpty == false )
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

		if ( clipListEmpty == false )
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

					string songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length );
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
		currentSong.text = rawCurrentSong.Substring ( 0, rawCurrentSong.Length -4 );
		
		songLocation = clipList [ currentSongNumber ];
		
		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( "Preparing to play " + rawCurrentSong.Substring ( 0, rawCurrentSong.Length -4 ));
		
		WWW www = new WWW ( "file://" + songLocation );
		yield return www;

		manager.audio.clip = www.GetAudioClip ( false );
		Resources.UnloadUnusedAssets ();

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

				if ( streaming == false )
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

					timemark.text = "Streaming][Streaming";
				}
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

		if ( slideshow == false )
		{

			if ( Input.GetKeyUp ( KeyCode.DownArrow ))
				NextSong ();

			if ( Input.GetKeyUp ( KeyCode.UpArrow ))
				PreviousSong ();

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
		
			manager.audio.volume = volumeBarValue;
			
			if ( manager.audio.clip != null )
			{
				
				if ( streaming == false )
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
				}
			} else {
				
				if ( preciseTimemark == true )
				{
					
					timemark.text = "0:00.000][0:00.000";
				} else {
					
					timemark.text = "0:00][0:00";
				}
			}
			
			
			if ( manager.audio.isPlaying == true )
			{
				
				if ( streaming == false )
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
			
		} else {

			if ( fadeIn == true )
			{

				float smoothDampIn = Mathf.SmoothDamp ( currentSlideshowImage.color.a, 1.0F, ref fadeVelocity, 2, 4000 );
				currentSlideshowImage.color = new Color ( 0.5F, 0.5F, 0.5F, smoothDampIn );

				if ( currentSlideshowImage.color.a > 0.98F )
				{

					currentSlideshowImage.color = new Color ( 0.5F, 0.5F, 0.5F, 1.0F );
					fadeIn = false;
				}
			}

			if ( fadeOut == true )
			{

				float smoothDampOut = Mathf.SmoothDamp ( currentSlideshowImage.color.a, 0.0F, ref fadeVelocity, 2, 4000 );
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

				string songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length );
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

						string songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length);
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

					currentSong.text = "UnityMusicPlayer";
					
					if ( preciseTimemark == true )
						timemark.text = "0:00.000][0:00.000";
					else
						timemark.text = "0:00][0:00";
				}
			}
		}
	}


	void Quit ()
	{

		wasPlaying = false;
		manager.audio.Stop ();
		
		Resources.UnloadUnusedAssets ();

		TextWriter savePrefs = new StreamWriter ( prefsLocation );
		savePrefs.WriteLine ( mediaPath + "\n" + loop + "\n" + shuffle + "\n" + continuous + "\n" + showTypes + "\n" + showTimebar + "\n" + showQuickManage + "\n" + preciseTimemark + "\n" + volumeBarValue + "\n" + avcR + "\n" + avcG + "\n" + avcB + "\n" + bloom + "\n" + motionBlur + "\n" + sunShafts + 
		                     "\n" + tempEchoDelay + "\n" + tempEchoDecayRate + "\n" + tempEchoWetMix + "\n" + tempEchoDryMix + "\n" + previousSongs [ 0 ] + "\n" + previousSongs [ 1 ] + "\n" + previousSongs [ 2 ] + "\n" + previousSongs [ 3 ] + "\n" + previousSongs [ 4 ] + "\n" + previousSongs [ 5 ] + "\n" + previousSongs [ 6 ] );
		savePrefs.Close ();

		if ( Application.isEditor == true )
			UnityEngine.Debug.Log ( "Quit has been called" );
		else
			Application.Quit ();
	}
}
