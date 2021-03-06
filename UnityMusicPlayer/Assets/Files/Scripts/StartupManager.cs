using System;
using System.IO;
using System.Xml;
using System.Net;
using UnityEngine;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections;
using System.Diagnostics;
using System.Net.Security;
using System.Globalization;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Security.Cryptography.X509Certificates;
//Written by Michael Bethke
//Thank you for your ∞ mercy, Jesus!
[XmlRoot("TwoCatSettings")]
public class TwoCatSettings
{
	
	[XmlElement("LocalPort")]
	public int localPort = 35143;
}

[XmlRoot ( "Preferences" )]
public class Preferences
{
	
	[XmlElement ( "LastDirectory" )]
	public string lastDirectory;
	
	[XmlElement ( "CheckForUpdate" )]
	public bool checkForUpdate = true;
	
	[XmlElement ( "EnableUpdateNotifications" )]
	public bool updateNotifications = true;
	
	[XmlElement ( "EnableOMB" )]
	public bool enableOMB = true;
	
	[XmlElement ( "EnableTutorials" )]
	public bool enableTutorials = true;
	
	[XmlElement ( "Loop" )]
	public bool loop = false;
	
	[XmlElement ( "Shuffle" )]
	public bool shuffle = false;
	
	[XmlElement ( "Continuous")]
	public bool continuous = false;
	
	[XmlElement ( "EnableTypes" )]
	public bool enableTypes = false;
	
	[XmlElement ( "EnableArrows" )]
	public bool enableArrows = true;
	
	[XmlElement ( "EnableTimebar" )]
	public bool enableTimebar = false;
	
	[XmlElement ( "EnableArtwork" )]
	public bool enableArtwork = true;
	
	[XmlElement ( "EnableKeybinds" )]
	public bool enableKeybinds = true;
	
	[XmlElement ( "EnableDeepSearch" )]
	public bool enableDeepSearch = true;
	
	[XmlElement ( "EnableQuickManage" )]
	public bool enableQuickManage = true;
	
	[XmlElement ( "EnablePreciseTimemark" )]
	public bool enablePreciseTimemark = false;
	
	[XmlElement ( "EnableHideGUINotifications" )]
	public bool enableHideGUINotifications = true;
	
	[XmlElement ( "VolumebarValue" )]
	public float volumebarValue = 1.0f;
	
	[XmlElement ( "SlideshowDisplayTime" )]
	public float slideshowDisplayTime = 2.0f;
	
	[XmlElement ( "AVcR" )]
	public float avcR = 1.0f;
	
	[XmlElement ( "AVcG" )]
	public float avcG = 0.5f;
	
	[XmlElement ( "AVcB" )]
	public float avcB = 0.2f;
	
	[XmlElement ( "YScale" )]
	public float yScale = 100.0f;
	
	[XmlElement ( "Bloom" )]
	public bool bloom = false;
	
	[XmlElement ( "SunShafts" )]
	public bool sunShafts = true;
	
	[XmlElement ( "Blur" )]
	public bool blur = false;

	[XmlElement ( "BlurIterations" )]
	public int blurIterations = 2;
	
	[XmlElement ( "IterateEffects" )]
	public bool iterateEffects = false;
	
	[XmlElement ( "Vignetting" )]
	public bool vignetting = true;
	
	[XmlElement ( "AutoAVBlur" )]
	public bool autoAVBlur = true;
	
	[XmlElement ( "AutoAVOff" )]
	public bool autoAVOff = false;
	
	[XmlElement ( "EchoDelay" )]
	public float echoDelay = 110.0f;
	
	[XmlElement ( "EchoDecayRate" )]
	public float echoDecayRate = 0.3f;
	
	[XmlElement ( "EchoWetMix" )]
	public float echoWetMix = 0.8f;
	
	[XmlElement ( "EchoDryMix" )]
	public float echoDryMix = 0.6f;
}


public class StartupManager : MonoBehaviour
{

	public bool developmentMode = false;

	public string runningVersion;
	float newestVersion;
	float devVersion;

	public GUIText connectionInformation;
	bool errorInConnectionToInternet = false;
	internal bool startOMB = false;

	public GUISkin guiskin;

	internal bool showUnderlay = false;
	public Texture2D underlay;

	MusicViewer musicViewer;
	PaneManager paneManager;
	OnlineMusicBrowser onlineMusicBrowser;
	internal string[] allSongs;
	
	static string mac = Path.DirectorySeparatorChar + "Users" + Path.DirectorySeparatorChar  + Environment.UserName + Path.DirectorySeparatorChar + "Music" + Path.DirectorySeparatorChar  + "UnityMusicPlayer" + Path.DirectorySeparatorChar;
	static string windows = Environment.GetFolderPath ( Environment.SpecialFolder.MyMusic ) + Path.DirectorySeparatorChar  + "UnityMusicPlayer" + Path.DirectorySeparatorChar;
	internal string directoryBrowser;

	internal string twoCatStudiosPath;
	internal string path;
	internal string mediaPath;
	internal string downloadedPath;
	internal string supportPath;
	internal string helpPath;
	internal string prefsLocation;
	internal string	slideshowPath;
	internal string tempPath;

	internal TwoCatSettings twoCatSettings = new TwoCatSettings ();
	internal Preferences preferences = new Preferences ();

	string[] applicationDownloads;
	string[] devApplicationDownloads;
	public WebClient client;

	bool updateAvailable = false;
	bool clearConnectionInformation = false;

	string websiteLink;
	
	float startTime;
	float endTime;
	
	
	void Start ()
	{
		
		ServicePointManager.ServerCertificateValidationCallback += delegate ( object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors ) { return true; };
		
		if ( developmentMode == true )
			UnityEngine.Debug.Log ( "Development Mode is ON" );
			
		onlineMusicBrowser = GameObject.FindGameObjectWithTag ( "OnlineMusicBrowser" ).GetComponent<OnlineMusicBrowser>();
		musicViewer = GameObject.FindGameObjectWithTag ( "MusicViewer" ).GetComponent<MusicViewer>();
		paneManager = gameObject.GetComponent<PaneManager>();

		if ( Environment.OSVersion.ToString ().Substring ( 0, 4 ) == "Unix" )
		{

			path = mac;
			twoCatStudiosPath = Path.DirectorySeparatorChar + "Users" + Path.DirectorySeparatorChar  + Environment.UserName + Path.DirectorySeparatorChar + "Library" + Path.DirectorySeparatorChar  + "Application Support" + Path.DirectorySeparatorChar + "2Cat Studios" + Path.DirectorySeparatorChar;
			directoryBrowser = "Finder";
		} else
		{

			path = windows;
			twoCatStudiosPath = Environment.GetFolderPath ( Environment.SpecialFolder.CommonApplicationData ) + Path.DirectorySeparatorChar + "2Cat Studios" + Path.DirectorySeparatorChar;
			directoryBrowser = "File Explorer";
		}

		mediaPath = path + "Media" + Path.DirectorySeparatorChar;
		supportPath = path + "Support" + Path.DirectorySeparatorChar;
		downloadedPath = mediaPath + "Downloaded" + Path.DirectorySeparatorChar;
		helpPath = supportPath + Path.DirectorySeparatorChar + "FAQ & Tutorial.txt" + Path.DirectorySeparatorChar;
		slideshowPath = path + "Slideshow" + Path.DirectorySeparatorChar;
		tempPath = supportPath + "Temp" + Path.DirectorySeparatorChar;
		
		preferences.lastDirectory = mediaPath.Substring ( 0, mediaPath.Length - 1 );
		
		if ( !Directory.Exists ( twoCatStudiosPath ))
		{
			
			UnityEngine.Debug.Log ( twoCatStudiosPath + " does not exist!" );
			Directory.CreateDirectory ( twoCatStudiosPath );
		}
		
		if ( ReadTwoCatSettings () == true )
		{
		
			if ( WriteTwoCatSettings () == true )
			{
				
				if ( developmentMode == true )
				{
					
					UnityEngine.Debug.Log ( "TwoCat Settings Loaded Successfully" );
				}
			}
		} else {
			
			if ( WriteTwoCatSettings () != true )
			{
				
				UnityEngine.Debug.LogError ( "Unable to Write TwoCat Settings!" );
			}
		}
		
		if ( !Directory.Exists ( mediaPath ))
			Directory.CreateDirectory ( mediaPath );
			
		if ( !Directory.Exists ( mediaPath + "Albums" ))
			Directory.CreateDirectory ( mediaPath + "Albums" );
			
		if ( !Directory.Exists ( mediaPath + "Artists" ))
			Directory.CreateDirectory ( mediaPath + "Artists" );
			
		if ( !Directory.Exists ( mediaPath + "Genres" ))
			Directory.CreateDirectory ( mediaPath + "Genres" );
			
		if ( !Directory.Exists ( mediaPath + "Playlists" ))
			Directory.CreateDirectory ( mediaPath + "Playlists" );
		
		if ( !Directory.Exists ( mediaPath + "Downloaded" ))
			Directory.CreateDirectory ( mediaPath + "Downloaded" );

		if ( !Directory.Exists ( supportPath ))
			Directory.CreateDirectory(supportPath );

		if ( !Directory.Exists ( slideshowPath ))
		{
			
			Directory.CreateDirectory ( slideshowPath );
			File.Copy ( Application.streamingAssetsPath + Path.DirectorySeparatorChar + "UnityMusicPlayerIcon.png", slideshowPath + "UnityMusicPlayerIcon.png", true );
		}
		
		if ( !File.Exists ( slideshowPath + "UnityMusicPlayerIcon.png" ))
			File.Copy ( Application.streamingAssetsPath + Path.DirectorySeparatorChar + "UnityMusicPlayerIcon.png", slideshowPath + "UnityMusicPlayerIcon.png", true );
		

		if ( !Directory.Exists ( tempPath ))
		{

			Directory.CreateDirectory ( tempPath );
		} else if ( Directory.GetFiles ( tempPath ).Length > 0 )
		{
			
			DirectoryInfo tempDirectory = new DirectoryInfo ( tempPath );
			tempDirectory.Delete ( true );

			Directory.CreateDirectory ( tempPath );
		}
		
		if ( ReadPreferences () == true )
		{
		
			if ( WritePreferences () == true )
			{
				
				if ( developmentMode == true )
				{
					
					UnityEngine.Debug.Log ( "Universal Settings Loaded Successfully" );
				}
			}
		} else {
			
			if ( WritePreferences () != true )
			{
				
				UnityEngine.Debug.LogError ( "Unable to Write Universal Settings!" );
			}
		}

		if ( !Directory.Exists ( preferences.lastDirectory ))
		{
			
			preferences.lastDirectory = mediaPath.Substring ( 0, mediaPath.Length - 1 );
		}
		
		if ( !File.Exists ( supportPath + "FAQ & Tutorial.txt" ) || !File.Exists ( supportPath + "ReadMe.txt" ))
		{

			File.Copy ( Application.streamingAssetsPath + Path.DirectorySeparatorChar + "FAQ & Tutorial.txt", supportPath + "FAQ & Tutorial.txt", true );
			File.Copy ( Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ReadMe.txt", supportPath + "ReadMe.txt",true  );

		} else if ( developmentMode == false )
		{

			try
			{

				TextReader faq = File.OpenText ( supportPath + "FAQ & Tutorial.txt" );
				TextReader readme = File.OpenText ( supportPath + "ReadMe.txt" );

				string faqVersion = faq.ReadLine ().Substring ( 17 );
				string readmeVersion = readme.ReadLine ().Substring ( 17 );

				faq.Close ();
				readme.Close ();

                try
                {

                    if (float.Parse(faqVersion, CultureInfo.InvariantCulture.NumberFormat) < float.Parse(runningVersion))
                    {

                        File.Delete(supportPath + "FAQ & Tutorial.txt");
                        File.Copy(Application.streamingAssetsPath + Path.DirectorySeparatorChar + "FAQ & Tutorial.txt", supportPath + "FAQ & Tutorial.txt");
                    }
                    if (float.Parse(readmeVersion, CultureInfo.InvariantCulture.NumberFormat) < float.Parse(runningVersion))
                    {

                        File.Delete(supportPath + "ReadMe.txt");
                        File.Copy(Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ReadMe.txt", supportPath + "ReadMe.txt");
                    }
                }
                catch (Exception e)
                {

                    UnityEngine.Debug.Log ( "Unable to Read FAQ and Tutorial/ReadMe, rewritting, " + e );

                    File.Delete(supportPath + "FAQ & Tutorial.txt");
                    File.Copy(Application.streamingAssetsPath + Path.DirectorySeparatorChar + "FAQ & Tutorial.txt", supportPath + "FAQ & Tutorial.txt");

                    File.Delete(supportPath + "ReadMe.txt");
                    File.Copy(Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ReadMe.txt", supportPath + "ReadMe.txt");
                }
			}
			catch ( ArgumentOutOfRangeException error ) 
			{

				UnityEngine.Debug.Log ( "FAQ or ReadMe is not formatted properly! " + error );

				File.Delete ( supportPath + "FAQ & Tutorial.txt" );
				File.Copy ( Application.streamingAssetsPath + Path.DirectorySeparatorChar + "FAQ & Tutorial.txt", supportPath + "FAQ & Tutorial.txt" );

				File.Delete ( supportPath + "ReadMe.txt" );
				File.Copy ( Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ReadMe.txt", supportPath + "ReadMe.txt" );
			}
		}
		
		prefsLocation = supportPath + "Preferences.umpp";
		
		if ( preferences.checkForUpdate == true || preferences.enableOMB == true )
		{
			
			Thread internetConnectionsThread = new Thread (() => InternetConnections ( preferences.checkForUpdate, preferences.enableOMB ));
			internetConnectionsThread.Start ();
			
			if ( preferences.enableOMB == true )
			{
				
				paneManager.loading = true;
				connectionInformation.text = "Connecting to the OnlineMusicDatabase";
				startTime = Time.realtimeSinceStartup;
				InvokeRepeating ( "CheckStartOnlineMusicBrowser", 0, 0.2F );
			}
		}
	}

	
	void InternetConnections ( bool updateVersion, bool updateOMB )
	{
		
		using ( WebClient wClient = new WebClient ())
		try
		{
			
			if ( updateOMB == true )
			{
			
				if ( developmentMode == false )
				{
					
					if ( File.Exists ( supportPath + Path.DirectorySeparatorChar + "Downloads.xml" ))
						File.Delete ( supportPath + Path.DirectorySeparatorChar + "Downloads.xml" );
						
					Uri url = new Uri ( "http://2catstudios.github.io/UnityMusicPlayer/Stable/OnlineMusicBrowser.xml" );
					using ( client = new WebClient ())
					{
							
						client.DownloadFile ( url, supportPath + Path.DirectorySeparatorChar + "Downloads.xml" );
						startOMB = true;
					}
				} else {
					
					try
					{
						
						if ( File.Exists ( supportPath + Path.DirectorySeparatorChar + "Downloads.xml" ))
							File.Delete ( supportPath + Path.DirectorySeparatorChar + "Downloads.xml" );
					
						Uri url = new Uri ( "http://71.63.239.44/shares/USB_Storage/Storage/UnityMusicPlayer/Developer/OnlineMusicBrowser.xml" );
						using ( client = new WebClient ())
						{
						
							client.DownloadFile ( url, supportPath + Path.DirectorySeparatorChar + "Downloads.xml" );
							startOMB = true;
						}
					} catch {
				
						UnityEngine.Debug.LogWarning ( "Unable to download XML file! Downloading regular file instead." );
						Uri url = new Uri ( "http://2catstudios.github.io/UnityMusicPlayer/Stable/OnlineMusicBrowser.xml" );
						using ( client = new WebClient ())
						{
						
							client.DownloadFile ( url, supportPath + Path.DirectorySeparatorChar + "Downloads.xml" );
							startOMB = true;
						}
					}
				}
			}
			
			if ( updateVersion == true )
			{
				
				applicationDownloads = wClient.DownloadString ( "http://2catstudios.github.io/UnityMusicPlayer/Stable/VersionInfo.txt" ).Split ( '\n' );

				websiteLink = applicationDownloads [4];
					
				newestVersion = Convert.ToSingle(applicationDownloads [1]);
				if( Single.Parse ( runningVersion ) < newestVersion)
				{
						
					updateAvailable = true;
				}
				
				if ( developmentMode == true )
				{
					
					devApplicationDownloads = wClient.DownloadString ( "http://2catstudios.github.io/UnityMusicPlayer/Developer/VersionInfo.txt" ).Split ( '\n' );
					
					newestVersion = Convert.ToSingle ( applicationDownloads [1]);
					devVersion = Convert.ToSingle ( devApplicationDownloads [1]);
					UnityEngine.Debug.Log ( "Running version is: " + runningVersion + ". Dev-release release is: " + devVersion + ". Stable release is: " + newestVersion + "." );
				}
			}
		} catch ( Exception errorText )
		{
			
			if ( developmentMode == true )
				UnityEngine.Debug.Log ( errorText );
				
			errorInConnectionToInternet = true;
		}
			
		if ( updateOMB == true && errorInConnectionToInternet == false )
			clearConnectionInformation = true;
		
		updateVersion = false;
	}
	
	
	void CheckStartOnlineMusicBrowser ()
	{
		
		if ( startOMB == true )
		{
			
			endTime = Time.realtimeSinceStartup;
			if ( developmentMode == true )
				UnityEngine.Debug.Log ( "Connecting to the OMB successful! Took " + ( endTime - startTime ) + " seconds." );
			
			onlineMusicBrowser.SendMessage ( "StartOMB" );
			CancelInvoke ( "CheckStartOnlineMusicBrowser" );
		}
	}
	

	void OnGUI ()
	{
		
		GUI.skin = guiskin;
		
		if ( developmentMode == true && Application.isEditor == false )
		{

			showUnderlay = true;
			GUI.Window ( 5, new Rect ( Screen.width / 2 - 150F, Screen.height / 2 - 85, 300, 100 ), DeveloperMode, "" );
			GUI.BringWindowToFront ( 5 );
			GUI.FocusWindow ( 5 );
		} else {
			
			if ( updateAvailable == true )
			{
				
				if ( preferences.updateNotifications == true )
				{
        	
					showUnderlay = true;
					paneManager.popupBlocking = true;
					GUI.Window ( 3, new Rect (Screen.width / 2 - 150, Screen.height / 2 - 85, 300, 200), NewVersion, "" );
					GUI.FocusWindow ( 3 );
					GUI.BringWindowToFront ( 3 );
				}
			}
		}

		if ( errorInConnectionToInternet == true )
		{

			StartCoroutine ( "UnableToConnectToOMB" );
			errorInConnectionToInternet = false;
		}

		if ( clearConnectionInformation == true )
		{

			connectionInformation.text = "";
			clearConnectionInformation = false;
		}
	}
	
	
	void NewVersion ( int pwid )
	{
		
		GUILayout.BeginVertical ();
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		
		GUILayout.Label ( "Update " + applicationDownloads [1] + " is Available!" );
		
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();

		GUILayout.FlexibleSpace ();
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUILayout.Label ( applicationDownloads[2] );
		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		GUILayout.FlexibleSpace ();
		
		GUI.skin.button.fontSize = 16;
		if ( GUILayout.Button ( "Ignore" ))
		{
			
			updateAvailable = false;
			showUnderlay = false;
			paneManager.popupBlocking = false;
			GUI.FocusWindow ( 0 );
			GUI.BringWindowToFront ( 0 );
		}
		GUI.skin.button.fontSize = 22;
		
		if ( GUILayout.Button ( "Download Now" ))
		{
			
			Process.Start ( websiteLink );
			musicViewer.SendMessage ( "Quit" );
		}
		
		GUILayout.EndVertical ();
	}

	
	void DeveloperMode ( int pwid )
	{
		
		GUILayout.BeginVertical ();
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		
		GUILayout.Label ( "DeveloperMode is Enabled!" );
		
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		GUILayout.FlexibleSpace ();
		
		GUI.skin.button.fontSize = 16;
		if ( GUILayout.Button ( "Get Stable Version" ))
		{
			
			Process.Start ( websiteLink );
			musicViewer.SendMessage ( "Quit" );
			
		}
		GUI.skin.button.fontSize = 22;
		
		if ( GUILayout.Button ( "Okay" ))
		{
			
			developmentMode = false;
			showUnderlay = false;
			paneManager.popupBlocking = false;
			GUI.FocusWindow ( 0 );
			GUI.BringWindowToFront ( 0 );
		}
		
		GUILayout.EndVertical ();
	}
	

	IEnumerator UnableToConnectToOMB ()
	{

		connectionInformation.text = "Unable to connect to the OnlineMusicBrowser!";

		yield return new WaitForSeconds ( 5 );
		connectionInformation.text = "";
	}
	
	
	void CheckForUpdate ()
	{
		
		Thread internetConnectionsThread = new Thread (() => InternetConnections ( true, false ));
		internetConnectionsThread.Priority = System.Threading.ThreadPriority.Highest;
		internetConnectionsThread.Start ();
	}
	
	
	void RefreshOMB ()
	{
		
		startTime = Time.realtimeSinceStartup;
		
		allSongs = null;
		startOMB = false;
		
		connectionInformation.text = "Connecting to the OnlineMusicDatabase";
		InvokeRepeating ( "CheckStartOnlineMusicBrowser", 0, 0.2F );
		
		Thread internetConnectionsThread = new Thread (() => InternetConnections ( false, true ));
		internetConnectionsThread.Priority = System.Threading.ThreadPriority.Highest;
		internetConnectionsThread.Start ();
	}
	
	
	bool ReadTwoCatSettings ()
	{
		
		try {
			
			System.IO.StreamReader unisetReader = new System.IO.StreamReader ( twoCatStudiosPath + "TwoCatSettings.xml" );
			string unisetXML = unisetReader.ReadToEnd();
			unisetReader.Close();
			
			twoCatSettings = unisetXML.DeserializeXml<TwoCatSettings> ();
		} catch ( Exception error )
		{
			
			UnityEngine.Debug.LogWarning ( "Unable to Read TwoCat Settings: " + error );
			return false;
		}
		
		return ( true );
	}
	
	
	bool WriteTwoCatSettings ()
	{
		
		try {
			
			XmlSerializer unisetSerializer = new XmlSerializer ( twoCatSettings.GetType ());
			StreamWriter unisetWriter = new StreamWriter ( twoCatStudiosPath + "TwoCatSettings.xml" );
			unisetSerializer.Serialize ( unisetWriter.BaseStream, twoCatSettings );
		} catch ( Exception error ) {
			
			UnityEngine.Debug.LogError ( "Unable to Write TwoCat Settings: " + error );
			return false;
		}
		
		return ( true );
	}
	
	
	bool ReadPreferences ()
	{
		
		try {
			
			System.IO.StreamReader preferencesReader = new System.IO.StreamReader ( supportPath + "Preferences.umpp" );
			string preferencesXML = preferencesReader.ReadToEnd();
			preferencesReader.Close();
		
			preferences = preferencesXML.DeserializeXml<Preferences> ();
		} catch ( Exception error )
		{
			
			UnityEngine.Debug.LogWarning ( "Unable to Read Preferences: " + error );
			return false;
		}
		
		return ( true );
	}
	
	
	internal bool WritePreferences ()
	{
		
		try {
			
			XmlSerializer serializer = new XmlSerializer ( preferences.GetType ());
			StreamWriter writer = new StreamWriter ( supportPath + "Preferences.umpp" );
			serializer.Serialize ( writer.BaseStream, preferences );
			writer.Close ();
		} catch ( Exception error )
		{
			
			UnityEngine.Debug.LogError ( "Unable to Write Preferences: " + error );
			return false;
		}

		return ( true );
	}
}
