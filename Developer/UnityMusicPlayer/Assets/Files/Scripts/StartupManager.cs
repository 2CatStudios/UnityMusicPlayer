using System;
using System.IO;
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
using System.Security.Cryptography.X509Certificates;
//Written by GibsonBethke
//Thank you for your ∞ mercy, Jesus!
public class StartupManager : MonoBehaviour
{

	public bool developmentMode = false;

	public string runningVersion;
	float newestVersion;
	float devVersion;

	public GUIText connectionInformation;
	bool errorInConnectionToInternet = false;
	internal bool startOMB = false;

	internal bool showUnderlay = false;
	public Texture2D underlay;
	public Texture2D popupWindowTexture;

	MusicViewer musicViewer;
	PaneManager paneManager;
	LoadingImage loadingImage;
	OnlineMusicBrowser onlineMusicBrowser;
	internal string[] allSongs;
	
	static string mac = Path.DirectorySeparatorChar + "Users" + Path.DirectorySeparatorChar  + Environment.UserName + Path.DirectorySeparatorChar + "Music" + Path.DirectorySeparatorChar  + "UnityMusicPlayer" + Path.DirectorySeparatorChar;
	static string windows = Environment.GetFolderPath ( Environment.SpecialFolder.MyMusic ) + Path.DirectorySeparatorChar  + "UnityMusicPlayer" + Path.DirectorySeparatorChar;
	internal string directoryBrowser;
	
	internal bool showTutorials = true;

	internal bool showFileTypes;

	internal string path;
	internal string mediaPath;
	internal string downloadedPath;
	internal string lastDirectory;
	internal string supportPath;
	internal string helpPath;
	internal string prefsLocation;
	internal string	slideshowPath;
	internal string tempPath;

	internal string [] prefs;
	int linesInPrefs = 29;

	string[] applicationDownloads;
	string[] devApplicationDownloads;
	public WebClient client;

	bool updateAvailable = false;
	internal bool checkForUpdate = true;
	bool clearConnectionInformation = false;
	internal bool ombEnabled = true;

	string websiteLink;
	
	
	void Start ()
	{
		
		ServicePointManager.ServerCertificateValidationCallback += delegate ( object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors ) { return true; };
		
		if ( developmentMode == true )
			UnityEngine.Debug.Log ( "Development Mode is ON" );
			
		onlineMusicBrowser = GameObject.FindGameObjectWithTag ( "OnlineMusicBrowser" ).GetComponent<OnlineMusicBrowser>();
		loadingImage = GameObject.FindGameObjectWithTag ( "LoadingImage" ).GetComponent<LoadingImage>();
		musicViewer = GameObject.FindGameObjectWithTag ( "MusicViewer" ).GetComponent<MusicViewer>();
		paneManager = gameObject.GetComponent<PaneManager>();

		if ( Environment.OSVersion.ToString ().Substring ( 0, 4 ) == "Unix" )
		{

			path = mac;
			directoryBrowser = "Finder";
		} else
		{

			path = windows;
			directoryBrowser = "File Explorer";
		}

		mediaPath = path + "Media" + Path.DirectorySeparatorChar;
		supportPath = path + "Support" + Path.DirectorySeparatorChar;
		downloadedPath = mediaPath + "Downloaded" + Path.DirectorySeparatorChar;
		helpPath = supportPath + Path.DirectorySeparatorChar + "FAQ & Tutorial.txt" + Path.DirectorySeparatorChar;
		slideshowPath = path + "Slideshow" + Path.DirectorySeparatorChar;
		tempPath = supportPath + "Temp" + Path.DirectorySeparatorChar;

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
		
		if ( !File.Exists ( supportPath + "Preferences.umpp" ) || File.ReadAllLines ( supportPath + "Preferences.umpp" ).Length != linesInPrefs )
		{
			
			if ( developmentMode == true )
			{
				
				if ( !File.Exists ( supportPath + "Preferences.umpp" ))
					UnityEngine.Debug.LogWarning ( "Preference file does not exist!" );
				else
					UnityEngine.Debug.LogWarning ( "Preference file is outdated! There are " + File.ReadAllLines ( supportPath + "Preferences.umpp" ).Length + " lines. There should be " + linesInPrefs + " lines." );
			}
			
			using ( FileStream createPrefs = File.Create ( supportPath + "Preferences.umpp" ))
			{
					
				Byte[] preferences = new UTF8Encoding(true).GetBytes( mediaPath + "Albums\nTrue\nTrue\nTrue\nFalse\nFalse\nFalse\nFalse\nTrue\nFalse\nTrue\nFalse\nFalse\nFalse\n1.0\n0.373\n0.569\n1.000\nFalse\nFalse\nTrue\n3\n100\n0.3\n0.8\n0.6\nTrue\nFalse\n2.0\n0\n0\n0\n0\n0\n0\n0");
				createPrefs.Write ( preferences, 0, preferences.Length );
			}
		}
		
		lastDirectory = File.ReadAllLines ( supportPath + "Preferences.umpp" )[0];
		if ( !Directory.Exists ( lastDirectory ))
		{
			
			lastDirectory = mediaPath + "Albums";
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

				if ( float.Parse( faqVersion, CultureInfo.InvariantCulture.NumberFormat ) < float.Parse ( runningVersion ))
				{
				
					File.Delete ( supportPath + "FAQ & Tutorial.txt" );
					File.Copy ( Application.streamingAssetsPath + Path.DirectorySeparatorChar + "FAQ & Tutorial.txt", supportPath + "FAQ & Tutorial.txt" );
				}
				if ( float.Parse( readmeVersion, CultureInfo.InvariantCulture.NumberFormat ) < float.Parse ( runningVersion ))
				{
				
					File.Delete ( supportPath + "ReadMe.txt" );
					File.Copy ( Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ReadMe.txt", supportPath + "ReadMe.txt" );
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
		prefs = File.ReadAllLines ( prefsLocation );
		checkForUpdate = Convert.ToBoolean ( prefs [1] );
		ombEnabled = Convert.ToBoolean ( prefs [2] );
		showTutorials = Convert.ToBoolean ( prefs [3] );
		
		if ( checkForUpdate == true || ombEnabled == true )
		{
			
			Thread internetConnectionsThread = new Thread (() => InternetConnections ( checkForUpdate, ombEnabled ));
			internetConnectionsThread.Start ();
			
			if ( ombEnabled == true )
			{
				
				paneManager.loading = true;
				connectionInformation.text = "Connecting to the OnlineMusicDatabase";
				InvokeRepeating ( "CheckStartOnlineMusicBrowser", 0, 0.2F );
			}
		}
		
		gameObject.GetComponent<SocketsManager>().PrepareUDPMessage ( "UMP is Running" );
	}

	
	void InternetConnections ( bool updateVersion, bool updateOMB )
	{
		
		using ( WebClient wClient = new WebClient ())
		try
		{
			
			if ( developmentMode == false )
			{
				
				if ( updateOMB == true )
				{
					
					if ( File.Exists ( supportPath + Path.DirectorySeparatorChar + "Downloads.xml" ))
						File.Delete ( supportPath + Path.DirectorySeparatorChar + "Downloads.xml" );
						
					Uri url = new Uri ( "http://raw.github.com/2CatStudios/UnityMusicPlayer/master/Downloads.xml" );
					using ( client = new WebClient ())
					{
							
						client.DownloadFile ( url, supportPath + Path.DirectorySeparatorChar + "Downloads.xml" );
						startOMB = true;
					}
				}

			} else {
				
				if ( updateOMB == true )
				{
					
					try
					{
						
						if ( File.Exists ( supportPath + Path.DirectorySeparatorChar + "Downloads.xml" ))
							File.Delete ( supportPath + Path.DirectorySeparatorChar + "Downloads.xml" );
						
						Uri url = new Uri ( "http://raw.github.com/2CatStudios/UnityMusicPlayer/master/Developer/Downloads.xml" );
						using ( client = new WebClient ())
						{
							
							client.DownloadFile ( url, supportPath + Path.DirectorySeparatorChar + "Downloads.xml" );
							startOMB = true;
						}
					} catch {
					
						UnityEngine.Debug.Log ( "Unable to download XML file! Downloading regular file instead." );
						Uri url = new Uri ( "http://raw.github.com/2CatStudios/UnityMusicPlayer/master/Downloads.xml" );
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
				
				applicationDownloads = wClient.DownloadString ( "http://raw.github.com/2CatStudios/UnityMusicPlayer/master/VersionInfo.txt" ).Split ( '\n' );

				websiteLink = applicationDownloads [4];
					
				newestVersion = Convert.ToSingle(applicationDownloads [1]);
				if( Single.Parse ( runningVersion ) < newestVersion)
				{
						
					updateAvailable = true;
				}
				
				if ( developmentMode == true )
				{
					
					devApplicationDownloads = wClient.DownloadString ( "http://raw.github.com/2CatStudios/UnityMusicPlayer/master/Developer/VersionInfo.txt" ).Split ( '\n' );
					
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
		
		if ( updateVersion == true )
			loadingImage.showLoadingImages = false;
			
		if ( updateOMB == true && errorInConnectionToInternet == false )
			clearConnectionInformation = true;
		
		updateVersion = false;
	}
	
	
	void CheckStartOnlineMusicBrowser ()
	{
		
		if ( startOMB == true )
		{
			
			onlineMusicBrowser.SendMessage ( "StartOMB" );
			CancelInvoke ( "CheckStartOnlineMusicBrowser" );
		}
	}
	

	void OnGUI ()
	{

		if ( updateAvailable == true )
		{

			showUnderlay = true;
			paneManager.popupBlocking = true;
			GUI.Window ( 3, new Rect (Screen.width / 2 - 142.5F, Screen.height / 2 - 85, 300, 100), NewVersion, "An Update is Available" );
			GUI.FocusWindow ( 3 );
			GUI.BringWindowToFront ( 3 );
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

		if ( developmentMode == true && Application.isEditor == false )
		{

			showUnderlay = true;
			GUI.Window ( 5, new Rect ( Screen.width / 2 - 100F, Screen.height / 2 - 45, 200, 80 ), DeveloperMode, "Notice" );
			GUI.BringWindowToFront ( 5 );
			GUI.FocusWindow ( 5 );
		}
	}

	
	void DeveloperMode ( int pwid )
	{

		GUI.Label ( new Rect ( 30, 25, 200, 40 ), "developerMode is ON!" );
		
		if ( GUI.Button ( new Rect ( 10, 55, 120, 20 ), "Get Stable Version" ))
		{
			
			Process.Start ( websiteLink );
			musicViewer.SendMessage ( "Quit" );
		}
		
		if ( GUI.Button ( new Rect ( 140, 55, 50, 20 ), "Close" ))
		{

			showUnderlay = false;
		    developmentMode = false;
			GUI.FocusWindow ( 0 );
			GUI.BringWindowToFront ( 0 );
		}
	}

	
	void NewVersion ( int pwid )
	{
		
		GUI.Label (new Rect (0, 15, 300, 40), applicationDownloads[2]);
		GUI.Label (new Rect (0, 50, 300, 40), "Download now?");
		if (GUI.Button (new Rect (20, 60, 70, 30), "No"))
		{

			updateAvailable = false;
			showUnderlay = false;
			paneManager.popupBlocking = false;
			GUI.FocusWindow ( 0 );
			GUI.BringWindowToFront ( 0 );
		}
		
		if (GUI.Button (new Rect (210, 60, 70, 30), "Yes"))
		{

			Process.Start ( websiteLink );
			musicViewer.SendMessage ( "Quit" );
		}
	}
	

	IEnumerator UnableToConnectToOMB ()
	{

		connectionInformation.text = "Unable to connect to the OnlineMusicDatabase!";

		yield return new WaitForSeconds ( 10 );
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
		
		allSongs = null;
		
		startOMB = false;
		
		connectionInformation.text = "Connecting to the OnlineMusicDatabase";
		InvokeRepeating ( "CheckStartOnlineMusicBrowser", 0, 0.2F );
		
		Thread internetConnectionsThread = new Thread (() => InternetConnections ( false, true ));
		internetConnectionsThread.Priority = System.Threading.ThreadPriority.Highest;
		internetConnectionsThread.Start ();
	}
}
