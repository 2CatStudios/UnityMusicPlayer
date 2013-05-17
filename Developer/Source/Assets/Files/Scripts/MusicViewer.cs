using System;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
//Written by GibsonBethke
//Thank you for saving me, Jesus!
//Thank you for living in me, Spirit!
//Thank you for making me, Father!
public class MusicViewer : MonoBehaviour
{

#region Variables

	StartupManager startupManager;
	LoadingImage loadingImage;
	PaneManager paneManager;
	AudioVisualizerR audioVisualizerR;
	AudioVisualizerL audioVisualizerL;
	GUIText timebar;

//-------
	
	bool isPaused;	
	float pausePoint;
	int songTime;
	int minutes;
	float seconds;
	int rtMinutes;
	float rtSeconds;

	bool tempPreciseTimebar;
	bool preciseTimebar = false;
	
	public GUIText timemark;
	float timebarTime;

	float betweenSongDelay = 1.0F;

	internal String [ ] clipList;
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
	public Texture2D streamingWindowTexture;
	
	int [ ] previousSongs = new int  [ 7 ] { 0, 0, 0, 0, 0, 0, 0 };
	int psPlace = 6;
	
	bool loop = false;
	bool shuffle = false;
	bool continuous = false;
	
	float volumeBarValue = 1.0F;
	
	bool echo = false;
	string echoDelay = "200";
	string echoDecayRate = "0.3";
	string echoWetMix = "0.8";
	string echoDryMix = "0.6";

	bool showSettingsWindow = false;
	Rect settingsWindowRect = new Rect ( 0, 0, 350, 200 );
//	bool showAllDividers;
	bool showTypes;
	float avcR = 0.9886364F;
	float avcG = 0.5227273F;
	float avcB = 0.1704545F;
	bool bloom = false;
	bool motionBlur = false;
	bool sunShafts = false;

	bool tempDJMode = false;
	internal bool djMode = false;

	public Texture2D[] djModeImages = new Texture2D[3];
	public float djModeImagesShuffleDelay = 10.0F;

	String audioInput;
	bool pickInput;

//-------

	string mediaPath;
	string prefsLocation;
	GameObject manager;

	internal Rect musicViewerPosition = new Rect ( 0, 0, 800, 600 );

	bool showVisualizer = false;
	bool halfSpeed = false;
	bool doubleSpeed = false;

	bool clipListEmpty;

#endregion

	
	void Start ()
	{

		manager = GameObject.FindGameObjectWithTag ( "Manager" );
		startupManager = manager.GetComponent <StartupManager> ();
		paneManager = manager.GetComponent <PaneManager> ();
		loadingImage = GameObject.FindGameObjectWithTag ( "LoadingImage" ).GetComponent<LoadingImage>();
		mediaPath = startupManager.mediaPath;
		prefsLocation = startupManager.supportPath + "Preferences.umpp";

		musicViewerPosition.width = Screen.width;
		musicViewerPosition.height = Screen.height;

		streamingWindowRect.x = musicViewerPosition.width/2 - streamingWindowRect.width/2;
		streamingWindowRect.y = musicViewerPosition.height/2 - streamingWindowRect.height/2;

		settingsWindowRect.x = musicViewerPosition.width/2 - settingsWindowRect.width/2;
		settingsWindowRect.y = musicViewerPosition.height/2 - settingsWindowRect.height/2;

		audioVisualizerR = GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizerR> ();
		audioVisualizerL = GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizerL> ();

		timebar = GameObject.FindGameObjectWithTag ( "Timebar" ).GetComponent<GUIText> ();

		clipList = Directory.GetFiles ( mediaPath, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();

		if ( clipList.Length == 0 || clipList == null )
			clipListEmpty = true;

		string [ ] prefs = File.ReadAllLines ( prefsLocation );
		
		loop = Convert.ToBoolean ( prefs [ 0 ] );
		shuffle = Convert.ToBoolean ( prefs [ 1 ] );
		continuous = Convert.ToBoolean ( prefs [ 2 ] );

		showTypes = Convert.ToBoolean ( prefs [ 3 ] );
		preciseTimebar = Convert.ToBoolean ( prefs [ 4 ] );
		tempPreciseTimebar = preciseTimebar;
		volumeBarValue = Convert.ToSingle ( prefs [ 5 ] );


		avcR = float.Parse ( prefs [ 6 ] );
		avcG = float.Parse ( prefs [ 7 ] );
		avcB = float.Parse ( prefs [ 8 ] );


		bloom = Convert.ToBoolean ( prefs [ 9 ] );	
		motionBlur = Convert.ToBoolean ( prefs [ 10 ] );
		sunShafts = Convert.ToBoolean ( prefs [ 11 ] );


		previousSongs [ 0 ] = Convert.ToInt32 ( prefs [ 12 ] );
		if ( previousSongs [ 0 ] > clipList.Length )
			previousSongs [ 0 ] = clipList.Length;
		
		previousSongs [ 1 ] = Convert.ToInt32 ( prefs [ 13 ] );
		if ( previousSongs [ 1 ] > clipList.Length )
			previousSongs [ 1 ] = clipList.Length;
		
		previousSongs [ 2 ] = Convert.ToInt32 ( prefs [ 14 ] );
		if ( previousSongs [ 2 ] > clipList.Length )
			previousSongs [ 2 ] = clipList.Length;
		
		previousSongs [ 3 ] = Convert.ToInt32 ( prefs [ 15 ] );
		if ( previousSongs [ 3 ] > clipList.Length )
			previousSongs [ 3 ] = clipList.Length;
		
		previousSongs [ 4 ] = Convert.ToInt32 ( prefs [ 16 ] );
		if ( previousSongs [ 4 ] > clipList.Length )
			previousSongs [ 4 ] = clipList.Length;
		
		previousSongs [ 5 ] = Convert.ToInt32 ( prefs [ 17 ] );
		if ( previousSongs [ 5 ] > clipList.Length )
			previousSongs [ 5 ] = clipList.Length;
		
		previousSongs [ 6 ] = Convert.ToInt32 ( prefs [ 18 ] );
		if ( previousSongs [ 6 ] > clipList.Length )
			previousSongs [ 6 ] = clipList.Length;


		TextWriter savePrefs = new StreamWriter ( prefsLocation );
		savePrefs.WriteLine ( loop + "\n" + shuffle + "\n" + continuous + "\n" + showTypes + "\n" + preciseTimebar + "\n" + volumeBarValue + "\n" + avcR + "\n" + avcG + "\n" + avcB + "\n" + bloom + "\n" + motionBlur + "\n" + sunShafts + "\n" + previousSongs [ 0 ] + "\n" + previousSongs [ 1 ] + "\n" + previousSongs [ 2 ] + "\n" + previousSongs [ 3 ] + "\n" + previousSongs [ 4 ] + "\n" + previousSongs [ 5 ] + "\n" + previousSongs [ 6 ] );
		savePrefs.Close ();

		InvokeRepeating ( "Refresh", 0, 2 );
	}


	void Refresh ()
	{

		string [ ] tempClipList = Directory.GetFiles ( mediaPath, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
		if ( clipList != tempClipList )
			clipList = tempClipList;

		if ( clipList.Length == 0 || clipList == null )
			clipListEmpty = true;
		else
			clipListEmpty = false;
	}


	void SettingsWindow ( int wid )
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

			preciseTimebar = tempPreciseTimebar;

			if ( preciseTimebar == true )
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
					} else {
						timemark.text = "Streaming][Streaming";
					}
				} else {

					timemark.text = "0:00.000][0:00.000";
					}
				} else {

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
				} else {

					timemark.text = "0:00][0:00";
				}
			}

			if ( tempDJMode == true )
			{

				manager.audio.Stop ();

				djMode = true;
				timebar.enabled = false;
				currentSong.text = "Select Line-In";

				manager.GetComponent<BloomAndLensFlares>().enabled = bloom;
				manager.GetComponent<MotionBlur>().enabled = motionBlur;
				manager.GetComponent<SunShafts>().enabled = sunShafts;

				pickInput = true;
				UnityEngine.Debug.Log ( "DJ Mode is ON" );
			}

			GUI.FocusWindow ( 0 );
			GUI.BringWindowToFront ( 0 );
			paneManager.popupBlocking = false;
			showSettingsWindow = false;
		}

#region AudioVisualizerSettings

		GUI.Box ( new Rect ( 10, 20, 150, 170 ), "AudioVisualizer Settings" );

		GUI.Label ( new Rect ( 50, 30, 40, 30 ), "Red" );
		avcR = GUI.HorizontalSlider ( new Rect ( 25, 50, 100, 15), avcR, 0.0F, 1.000F );

		GUI.Label ( new Rect ( 50, 55, 40, 30 ), "Green" );
		avcG = GUI.HorizontalSlider ( new Rect ( 25, 75, 100, 15), avcG, 0.0F, 1.000F );

		GUI.Label ( new Rect ( 50, 80, 40, 30 ), "Blue" );
		avcB = GUI.HorizontalSlider ( new Rect ( 25, 100, 100, 15), avcB, 0.0F, 1.000F );

		GUI.contentColor = new Color ( avcR, avcG, avcB, 1.000F );
		GUI.Label ( new Rect ( 35, 110, 80, 20 ), "Sample Color");
		GUI.contentColor = Color.white;

		bloom = GUI.Toggle ( new Rect ( 20, 128, 95, 20 ), bloom, "Toggle Bloom" );

		motionBlur = GUI.Toggle ( new Rect ( 20, 148, 125, 20 ), motionBlur, "Toggle Motion Blur" );

		sunShafts = GUI.Toggle ( new Rect ( 20, 168, 120, 20 ), sunShafts, "Toggle Sun Shafts" );

#endregion

		GUI.Box ( new Rect ( 170, 20, 110, 22 ), "" );
		tempDJMode = GUI.Toggle ( new Rect ( 175, 20, 70, 20 ), tempDJMode, new GUIContent ( "DJ Mode", "Remains on until restart!" ));

		mousePos = Event.current.mousePosition;
		GUI.Label ( new Rect ( 175 - mousePos.x/20, 35 - mousePos.y/20, 200, 25 ), GUI.tooltip);


		showTypes = GUI.Toggle ( new Rect ( 170, 42, 120, 15 ), showTypes, "Show audio types" );

		tempPreciseTimebar = GUI.Toggle ( new Rect ( 170, 57, 115, 15 ), tempPreciseTimebar, "Precise Timebar" );

#region AudioSettings

		GUI.Box ( new Rect ( 170, 80, 170, 110 ), "Audio Echo Settings" );

		GUI.Label ( new Rect ( 200, 100, 80, 20 ), "Echo Delay" );
		echoDelay = GUI.TextField ( new Rect ( 175, 100, 30, 20 ), echoDelay, 3 );
		manager.GetComponent<AudioEchoFilter> ().delay = Convert.ToSingle ( echoDelay );

		GUI.Label ( new Rect ( 204, 122, 110, 20 ), "Echo Decay Rate" );
		echoDecayRate = GUI.TextField ( new Rect ( 175, 122, 30, 20 ), echoDecayRate, 3 );
		manager.GetComponent<AudioEchoFilter> ().decayRatio = Convert.ToSingle ( echoDecayRate );

		GUI.Label ( new Rect ( 198, 144, 100, 20 ), "Echo Wet Mix" );
		echoWetMix = GUI.TextField ( new Rect ( 175, 144, 30, 20 ), echoWetMix, 3 );
		manager.GetComponent<AudioEchoFilter> ().wetMix = Convert.ToSingle ( echoWetMix );

		GUI.Label ( new Rect ( 196, 166, 100, 20 ), "Echo Dry Mix" );
		echoDryMix = GUI.TextField ( new Rect ( 175, 166, 30, 20 ), echoDryMix, 3 );
		manager.GetComponent<AudioEchoFilter> ().dryMix = Convert.ToSingle ( echoDryMix );

#endregion
	}


	void OnGUI ()
	{

		if ( showStreamingWindow == true )
		{

			paneManager.popupBlocking = true;
			GUI.skin.window.normal.background = streamingWindowTexture;
			GUI.Window ( 5, streamingWindowRect, StreamingWindow, "Web and Disk Streaming" );
		}

		if ( showSettingsWindow == true )
		{

			paneManager.popupBlocking = true;
			GUI.skin.window.normal.background = streamingWindowTexture;
			GUI.Window ( 6, settingsWindowRect, SettingsWindow, "Settings" );
		}

		if ( djMode == false )
		{

			GUI.skin = GuiSkin;
			musicViewerPosition = GUI.Window ( 0, musicViewerPosition, MusicViewerPane, "MusicViewer" );

			if ( GUI.Button ( new Rect ( musicViewerPosition.width - 75, musicViewerPosition.height - 50, 60, 30), "Quit" ))
			{
			
				Resources.UnloadUnusedAssets ();
				
				TextWriter savePrefs = new StreamWriter ( prefsLocation );
				savePrefs.WriteLine ( loop + "\n" + shuffle + "\n" + continuous + "\n" + showTypes + "\n" + preciseTimebar + "\n" + volumeBarValue + "\n" + avcR + "\n" + avcG + "\n" + avcB + "\n" + bloom + "\n" + motionBlur + "\n" + sunShafts + "\n" + previousSongs [ 0 ] + "\n" + previousSongs [ 1 ] + "\n" + previousSongs [ 2 ] + "\n" + previousSongs [ 3 ] + "\n" + previousSongs [ 4 ] + "\n" + previousSongs [ 5 ] + "\n" + previousSongs [ 6 ] );
				savePrefs.Close ();
			
				if ( startupManager.developmentMode == true )
					UnityEngine.Debug.Log ( "Quit has been called" );
				else
					Application.Quit ();
			}
		} else if ( djMode == true )
		{

			if ( pickInput == true )
			{

				GUILayout.BeginHorizontal ();
				GUILayout.Space ( musicViewerPosition.width / 2 - 300 );
				GUILayout.BeginVertical ();
				GUILayout.Space ( musicViewerPosition.height / 4 + 25 );
	
				scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  musicViewerPosition.height - ( musicViewerPosition.height / 4 + 56 )));

				foreach (string device in Microphone.devices)
				{

					if ( GUILayout.Button ( device ))
					{

						audioInput = device;
						pickInput = false;

						loadingImage.showLoadingImages = true;
						loadingImage.InvokeRepeating ("LoadingImages", 0.25F, 0.25F);

						currentSong.text = "";

						showVisualizer = true;

						audioVisualizerR.showAV = showVisualizer;
						audioVisualizerL.showAV = showVisualizer;
						audioVisualizerR.topLine.material.color = new Color ( avcR, avcG, avcB, 255 );
						audioVisualizerR.bottomLine.material.color = new Color ( avcR, avcG, avcB, 255 );
						audioVisualizerL.topLine.material.color = new Color ( avcR, avcG, avcB, 255 );
						audioVisualizerL.bottomLine.material.color = new Color ( avcR, avcG, avcB, 255 );

						int minFreq = 20;
						int maxFreq = 20000;


						UnityEngine.Debug.Log ( "New input device: " + audioInput );

						Microphone.GetDeviceCaps ( audioInput, out minFreq, out maxFreq );
						UnityEngine.Debug.Log ( minFreq + " " + maxFreq  );

						loadingImage.showLoadingImages = false;

						manager.audio.clip = Microphone.Start ( audioInput, true, 2, 44100 );
						manager.audio.Play();
					}
				}

				GUI.EndScrollView();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
		}
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
		
		GUI.Label ( new Rect ( -36, 17, 340, 25 ), "Paste the link to an audio file in the textfield." );
		
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

				currentSong.text = "Streaming";
				
				manager.audio.clip = clip;
				Resources.UnloadUnusedAssets ();
			
				timemark.text = "Streaming][Streaming";
			
				if ( manager.audio.clip.isReadyToPlay )
				{

					minutes = 0;
					seconds = 0;
					rtMinutes = 00;
					rtSeconds = 00;

					streaming = true;
					manager.audio.Play ();
					isPaused = false;
				
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


	IEnumerator LoadAssetBundle ( string assetBundleToOpen, string songName )
	{

		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( assetBundleToOpen + " | " + songName );

		WWW wwwClient = WWW.LoadFromCacheOrDownload ( assetBundleToOpen, 1 );
		yield return wwwClient;
		
		AssetBundle bundle = wwwClient.assetBundle;
		
		AssetBundleRequest request = bundle.LoadAsync ( songName, typeof ( AudioClip ));
		yield return request;

		AudioClip aClip = request.asset as AudioClip;
		bundle.Unload ( false );

		manager.audio.clip = aClip;
		Resources.UnloadUnusedAssets ();

		currentSong.text = songName;

		if ( preciseTimebar == true )
			seconds = manager.audio.clip.length;
		else
			seconds = ( int ) Math.Round ( manager.audio.clip.length );

		if ( seconds > 60 )
		{
			
			minutes = ( int ) Math.Round ( seconds )/60;
			seconds -= minutes*60;
		} else
			
			minutes = 0;

		timemark.text = rtMinutes + ":" + String.Format ( "{0:00}", rtSeconds ) + "][" + minutes + ":" + String.Format ( "{0:00}", seconds );
		
		if ( manager.audio.clip.isReadyToPlay )
		{
			
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

		#region NextButton
		
		if ( GUI.Button ( new Rect ( musicViewerPosition.width/2 - 70, musicViewerPosition.height/4 - 7, 55, 30), "Next" ))
			NextSong ();
		
		#endregion
		
		#region BackButton
		
		if ( GUI.Button (new Rect ( musicViewerPosition.width/2 - 125, musicViewerPosition.height/4 - 7, 55, 30), "Back" ))
			PreviousSong ();
		
		#endregion
		
		#region VolumeBar
		
		GUI.Label ( new Rect ( musicViewerPosition.width/2 - 110, musicViewerPosition.height/4 - 42, 100, 25), "Volume" );
		volumeBarValue = GUI.HorizontalSlider ( new Rect ( musicViewerPosition.width/2 - 115, musicViewerPosition.height/4 - 20, 100, 30 ), volumeBarValue, 0.0F, 1.0F );
		
		#endregion
		
		#region LoopButton
		
		GUI.Label (new Rect ( musicViewerPosition.width/2 + 10, musicViewerPosition.height/4 - 42, 120, 30 ), "Loop" );
		
		if ( loop = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 5, musicViewerPosition.height/4 - 35, 100, 20 ), loop, "" ))
			if ( loop == true && shuffle == true || loop == true && continuous == true )
		{
			
			shuffle = false;
			continuous = false;
		}
		
		#endregion
		
		#region ShuffleButton
		
		GUI.Label ( new Rect ( musicViewerPosition.width/2 + 10, musicViewerPosition.height/4 - 19, 120, 30 ), "Shuffle" );
		
		if ( shuffle = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 5, musicViewerPosition.height/4 - 14, 100, 20 ), shuffle, "" ))
			if ( shuffle == true && loop == true || shuffle == true && continuous == true )
		{
			
			loop = false;
			continuous = false;
		}
		
		#endregion
		
		#region ContinuousPlay
		
		GUI.Label ( new Rect ( musicViewerPosition.width/2 + 10, musicViewerPosition.height/4 + 1, 120, 30 ), "Continuous" );
		
		if ( continuous = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 - 5, musicViewerPosition.height/4 + 6, 100, 20 ), continuous, "" ))
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

						Resources.UnloadUnusedAssets ();

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
							StartCoroutine ( PlayAudio());
						}
					}
				}

			}
		} else
		{

			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			GUILayout.Label ( "\nYou don't have any music to play!\n\nIf you have some songs on your computer\n(.wav or .ogg), click the 'Manage Audio' button bellow. Drop songs into the folder that appears.\n\nYou can also download music by navigating to the OnlineMusicBrowser (by pressing the right arrow key),\nor stream music by clicking the 'Streaming' button bellow.\n" );
			GUI.skin.label.alignment = TextAnchor.UpperLeft;
		}

		if ( hideGUI == false )
		{

			GUILayout.Box ( "System Commands" );

			if ( GUILayout.Button ( "Settings" ))
				showSettingsWindow = true;

			if ( GUILayout.Button ( "Streaming" ))
				showStreamingWindow = true;

			if ( GUILayout.Button ( "Manage Audio" ))
				OpenFolder ();
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

			manager.GetComponent<BloomAndLensFlares>().enabled = bloom;
			manager.GetComponent<MotionBlur>().enabled = motionBlur;
			manager.GetComponent<SunShafts>().enabled = sunShafts;
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

		echo = GUI.Toggle ( new Rect ( musicViewerPosition.width/2 + 165, musicViewerPosition.height - 20, 90, 20 ), echo,  "Echo" );

		if ( echo == true )
			manager.GetComponent<AudioEchoFilter> ().enabled = true;
		else
		    manager.GetComponent<AudioEchoFilter> ().enabled = false;

		
		if ( halfSpeed == false && doubleSpeed == false )
			manager.audio.pitch = 1.0F;

		if ( showSettingsWindow == true || showStreamingWindow == true || startupManager.showUnderlay == true )
			GUI.DrawTexture ( new Rect ( 0, 0, musicViewerPosition.width, musicViewerPosition.height ), underlay );
	}


	void NextSong ()
	{

		if ( psPlace != 6 )
		{
			
			psPlace += 1;
			currentSongNumber = previousSongs [ psPlace ];
			
			if ( clipList [ currentSongNumber ].Substring ( clipList [ currentSongNumber ].Length - 7 ) == "unity3d" )
			{

				string songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1 );
				StartCoroutine ( LoadAssetBundle ( "file://" + clipList [ currentSongNumber ], songName.Substring ( 0, songName.Length - 8 )));
			} else

				StartCoroutine ( PlayAudio() );
				
		} else {
			
			if ( shuffle == true )
			{
				
				Resources.UnloadUnusedAssets ();
				currentSongNumber = UnityEngine.Random.Range ( 0, clipList.Length );
				
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

			} else {
				
				Resources.UnloadUnusedAssets ();
				
				currentSongNumber += 1;
				previousSongs [ 0 ] = previousSongs [ 1 ];
				previousSongs [ 1 ] = previousSongs [ 2 ];
				previousSongs [ 2 ] = previousSongs [ 3 ];
				previousSongs [ 3 ] = previousSongs [ 4 ];
				previousSongs [ 4 ] = previousSongs [ 5 ];
				previousSongs [ 5 ] = previousSongs [ 6 ];
				previousSongs [ 6 ] = currentSongNumber;
				
				if ( currentSongNumber >= clipList.Length )
					currentSongNumber = 0;
				
				if ( clipList [ currentSongNumber ].Substring ( clipList [ currentSongNumber ].Length - 7 ) == "unity3d" )
				{

					string songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1 );
					StartCoroutine ( LoadAssetBundle ( "file://" + clipList [ currentSongNumber ], songName.Substring ( 0, songName.Length - 8 )));
				} else

					StartCoroutine ( PlayAudio() );
			}
		}
	}


	void PreviousSong ()
	{

		Resources.UnloadUnusedAssets ();
		
		if ( psPlace < 0 )
			currentSongNumber = UnityEngine.Random.Range ( 0, clipList.Length );
		else
		{
			
			psPlace -= 1;
			currentSongNumber = previousSongs [ psPlace ];
			
			if ( clipList [ currentSongNumber ].Substring ( clipList [ currentSongNumber ].Length - 7 ) == "unity3d" )
			{

				string songName = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1 );
				StartCoroutine ( LoadAssetBundle ( "file://" + clipList [ currentSongNumber ], songName.Substring ( 0, songName.Length - 8 )));	
			} else
				
				StartCoroutine ( PlayAudio() );
		}
	}


	IEnumerator PlayAudio ()
	{
	
		rawCurrentSong = clipList [ currentSongNumber ].Substring ( mediaPath.Length + 1 );
		currentSong.text = rawCurrentSong.Substring ( 0, rawCurrentSong.Length -4 );
		
		songLocation = clipList [ currentSongNumber ];
		
		if ( startupManager.developmentMode == true )
			UnityEngine.Debug.Log ( "Preparing to play " + rawCurrentSong.Substring ( 0, rawCurrentSong.Length -4 ));
		
		WWW www = new WWW ( "file://" + songLocation );
		yield return www;

		manager.audio.clip = www.GetAudioClip ( false );
		Resources.UnloadUnusedAssets ();

		if ( preciseTimebar == true )
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
		
		timemark.text = rtMinutes + ":" + String.Format ( "{0:00}", rtSeconds ) + "][" + minutes + ":" + String.Format ( "{0:00}", seconds );

		if ( manager.audio.clip.isReadyToPlay )
		{
			
			rtMinutes = 00;
			rtSeconds = 00;

			manager.audio.Play ();
			isPaused = false;
			
			if ( startupManager.developmentMode == true )
				UnityEngine.Debug.Log ( "Playing audio" );
		}

		if ( www.error != null )
			UnityEngine.Debug.Log ( www.error );
	}


	void Update ()
	{

		if ( djMode == false )
		{

			if ( Input.GetKeyDown ( KeyCode.DownArrow ))
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
				
					if (startupManager.developmentMode == true )
						UnityEngine.Debug.Log ( "Pausing audio at " + pausePoint );
				
				} else {
					
					manager.audio.Play ();
					manager.audio.time = pausePoint;
					isPaused = false;
				
					if ( startupManager.developmentMode == true )
						UnityEngine.Debug.Log ( "Resuming audio at " + pausePoint );
				}
			}
		
			manager.audio.volume = volumeBarValue;
		
			if ( manager.audio.isPlaying == true && streaming == false )
			{

				if ( preciseTimebar == true )
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

				if ( manager.audio.isPlaying == true )
				{

//					UnityEngine.Debug.Log ( manager.audio.time + "  :  " + manager.audio.clip.length );
				
					if ( manager.audio.time >= manager.audio.clip.length )
					{

						if ( continuous == true || loop == false && shuffle == false )
							Invoke ( "SongEnd", betweenSongDelay );
						else
							SongEnd ();

//						UnityEngine.Debug.Log ( "SongEnd" );
					}
				}
			}
		} else {

			if ( Input.GetKey ( KeyCode.Q ))
			    Application.Quit();

			if ( pickInput == false )
			{

				float[] spectrum = manager.audio.GetSpectrumData(1024, 0, FFTWindow.BlackmanHarris);
				int i = 1;
				while (i < 1023)
				{

					UnityEngine.Debug.DrawLine(new Vector3(i - 1, spectrum[i] + 10, 0), new Vector3(i, spectrum[i + 1] + 10, 0), Color.red);
					UnityEngine.Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrum[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(spectrum[i]) + 10, 2), Color.cyan);
					UnityEngine.Debug.DrawLine(new Vector3(Mathf.Log(i - 1), spectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), spectrum[i] - 10, 1), Color.green);
					UnityEngine.Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(spectrum[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(spectrum[i]), 3), Color.yellow);
					i++;
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
					
				if ( startupManager.developmentMode == true )
					UnityEngine.Debug.Log ( "Playing audio" );
					

			} else
			{
				
				if ( shuffle == true )
				{
					
					Resources.UnloadUnusedAssets ();
					currentSongNumber = UnityEngine.Random.Range ( 0, clipList.Length );

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
					
					timemark.text = "0:00][0:00";
					currentSong.text = "UnityMusicPlayer";
				}
			}
		}
	}


	void OpenFolder ()
	{
		
		Process.Start ( mediaPath );
	}
}
