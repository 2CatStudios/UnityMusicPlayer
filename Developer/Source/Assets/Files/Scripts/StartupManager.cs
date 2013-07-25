using System;
using System.IO;
using System.Net;
using UnityEngine;
using System.Text;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Net.Security;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
//Written by GibsonBethke
//Thank you for your ∞ mercy, Jesus!
public class StartupManager : MonoBehaviour
{

	public bool developmentMode = false;

	public float runningVersion;
	float newestVersion;

	public GUIText connectionInformation;
	bool errorInConnectionToInternet = false;
	bool connectingToInternet = true;
	bool startOMB = false;
	bool u1 = true;

	internal bool showUnderlay = false;

	PaneManager paneManager;
	OnlineMusicBrowser onlineMusicBrowser;
	internal string[] allSongs;
	
	static string mac = "/Users/" + Environment.UserName + "/Library/Application Support/2Cat Studios/UnityMusicPlayer";
	static string windows = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\2Cat Studios\\UnityMusicPlayer";

	internal bool showFileTypes;

	internal string path;
	internal string mediaPath;
	internal string supportPath;
	internal string	slideshowPath;
	internal string tempPath;

	int linesInPrefs = 25;

	string[] applicationDownloads;

	bool updateAvailable = false;

	string websiteLink;
	
	
	void Start ()
	{    
		
		if(developmentMode == true)
			UnityEngine.Debug.Log("Development Mode is ON");

		connectionInformation.text = "Connecting to the OnlineMusicDatabase...";
		onlineMusicBrowser = GameObject.FindGameObjectWithTag ("OnlineMusicBrowser").GetComponent<OnlineMusicBrowser>();
		paneManager = gameObject.GetComponent<PaneManager>();

		if(Environment.OSVersion.ToString().Substring (0, 4) == "Unix")
		{

			path = mac;
		} else
		{

			path = windows;
		}

		supportPath = path + Path.DirectorySeparatorChar + "Support" + Path.DirectorySeparatorChar;
		slideshowPath = path + Path.DirectorySeparatorChar + "Slideshow";
		tempPath = supportPath + "Temp";

		if ( !Directory.Exists ( path + Path.DirectorySeparatorChar + "Media" ))
			Directory.CreateDirectory ( path + Path.DirectorySeparatorChar + "Media" );
			
		if ( !Directory.Exists ( path + Path.DirectorySeparatorChar + "Media" + Path.DirectorySeparatorChar + "Albums" ))
			Directory.CreateDirectory ( path + Path.DirectorySeparatorChar + "Media" + Path.DirectorySeparatorChar + "Albums" );
			
		if ( !Directory.Exists ( path + Path.DirectorySeparatorChar + "Media" + Path.DirectorySeparatorChar + "Artists" ))
			Directory.CreateDirectory ( path + Path.DirectorySeparatorChar + "Media" + Path.DirectorySeparatorChar + "Artists" );
			
		if ( !Directory.Exists ( path + Path.DirectorySeparatorChar + "Media" + Path.DirectorySeparatorChar + "Genres" ))
			Directory.CreateDirectory ( path + Path.DirectorySeparatorChar + "Media" + Path.DirectorySeparatorChar + "Genres" );
			
		if ( !Directory.Exists ( path + Path.DirectorySeparatorChar + "Media" + Path.DirectorySeparatorChar + "Playlists" ))
			Directory.CreateDirectory ( path + Path.DirectorySeparatorChar + "Media" + Path.DirectorySeparatorChar + "Playlists" );

		if ( !Directory.Exists ( supportPath ))
			Directory.CreateDirectory(supportPath);

		if ( !Directory.Exists ( slideshowPath ))
			Directory.CreateDirectory ( slideshowPath );

		if ( !Directory.Exists ( tempPath ))
		{

			Directory.CreateDirectory ( tempPath );
		} else if ( Directory.GetFiles ( tempPath ).Length > 0 )
		{
			
			DirectoryInfo tempDirectory = new DirectoryInfo ( tempPath );
			tempDirectory.Delete ( true );

			Directory.CreateDirectory ( tempPath );
		}
		
		if(!File.Exists (supportPath + "Preferences.umpp") || File.ReadAllLines ( supportPath + "Preferences.umpp" ).Length != linesInPrefs )
		{

			using (FileStream createPrefs = File.Create(supportPath + "Preferences.umpp"))
			{
				
				Byte[] preferences = new UTF8Encoding(true).GetBytes( path + Path.DirectorySeparatorChar + "Media\nFalse\nFalse\nFalse\nFalse\nFalse\nFalse\n1.0\n0.373\n0.569\n1.000\nFalse\nFalse\nTrue\n100\n0.3\n0.8\n0.6\n0\n0\n0\n0\n0\n0\n0");
				createPrefs.Write(preferences, 0, preferences.Length);
			}
		}
		
		mediaPath = File.ReadAllLines ( supportPath + "Preferences.umpp" )[0];
		if ( !Directory.Exists ( mediaPath ))
		{
			
			mediaPath = path + Path.DirectorySeparatorChar + "Media";
		}

		Thread internetConnectionsThread = new Thread (InternetConnections);
		internetConnectionsThread.Priority = System.Threading.ThreadPriority.Highest;
		internetConnectionsThread.Start();

		if(!File.Exists (supportPath + "FAQ & Tutorial.txt") || !File.Exists (supportPath + "ReadMe.txt"))
		{

			File.Copy (Application.streamingAssetsPath + Path.DirectorySeparatorChar + "FAQ & Tutorial.txt", supportPath + "FAQ & Tutorial.txt");
			File.Copy (Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ReadMe.txt", supportPath + "ReadMe.txt");

			if ( developmentMode == false )
				Process.Start (supportPath + "FAQ & Tutorial.txt");
		} else if(developmentMode == false)
		{

			try
			{

				TextReader faq = File.OpenText ( supportPath + "FAQ & Tutorial.txt" );
				TextReader readme = File.OpenText ( supportPath + "ReadMe.txt" );

				string faqVersion = faq.ReadLine ().Substring ( 17 );
				string readmeVersion = readme.ReadLine ().Substring ( 17 );

				faq.Close();
				readme.Close ();

				if( float.Parse( faqVersion, CultureInfo.InvariantCulture.NumberFormat ) < runningVersion )
				{
				
					File.Delete ( supportPath + "FAQ & Tutorial.txt" );
					File.Copy ( Application.streamingAssetsPath + Path.DirectorySeparatorChar + "FAQ & Tutorial.txt", supportPath + "FAQ & Tutorial.txt" );
					Process.Start ( supportPath + "FAQ & Tutorial.txt" );
				}
				if ( float.Parse( readmeVersion, CultureInfo.InvariantCulture.NumberFormat ) < runningVersion )
				{
				
					File.Delete ( supportPath + "ReadMe.txt" );
					File.Copy ( Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ReadMe.txt", supportPath + "ReadMe.txt" );
					Process.Start ( supportPath + "ReadMe.txt" );
				}
			}
			catch ( ArgumentOutOfRangeException error ) 
			{

				UnityEngine.Debug.Log ( "FAQ or ReadMe not formatted properly! " + error );

				File.Delete ( supportPath + "FAQ & Tutorial.txt" );
				File.Copy ( Application.streamingAssetsPath + Path.DirectorySeparatorChar + "FAQ & Tutorial.txt", supportPath + "FAQ & Tutorial.txt" );
				Process.Start ( supportPath + "FAQ & Tutorial.txt" );

				File.Delete ( supportPath + "ReadMe.txt" );
				File.Copy ( Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ReadMe.txt", supportPath + "ReadMe.txt" );
				Process.Start ( supportPath + "ReadMe.txt" );
			}
		}
		
		InvokeRepeating ( "CheckStartOnlineMusicBrowser", 0, 0.2F );
	}

	
	void InternetConnections ()
	{

		ServicePointManager.ServerCertificateValidationCallback += delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
		
		using (WebClient wClient = new WebClient())
		try
		{
			
			if(developmentMode == false)
			{
				allSongs = wClient.DownloadString ("http://raw.github.com/2CatStudios/UnityMusicPlayer/master/AllSongs.txt").Split ('\n');
				applicationDownloads = wClient.DownloadString ("https://raw.github.com/2CatStudios/UnityMusicPlayer/master/VersionInfo.txt").Split ('\n');

			} else {
				allSongs = wClient.DownloadString ("http://raw.github.com/2CatStudios/UnityMusicPlayer/master/Developer/AllSongs.txt").Split ('\n');
				applicationDownloads = wClient.DownloadString ("https://raw.github.com/2CatStudios/UnityMusicPlayer/master/Developer/VersionInfo.txt").Split ('\n');
			}

			websiteLink = applicationDownloads [4];
			
			newestVersion = Convert.ToSingle(applicationDownloads [1]);
			if(runningVersion < newestVersion)
			{
				
				updateAvailable = true;
			}
		} catch (Exception errorText)
		{
			
			UnityEngine.Debug.Log (errorText);
			errorInConnectionToInternet = true;
		}

		if ( errorInConnectionToInternet == false )
			startOMB = true;

		connectingToInternet = false;
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

		GUI.skin.label.alignment = TextAnchor.MiddleCenter;

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

		if ( u1 == true && connectingToInternet == false && errorInConnectionToInternet == false )
		{

			connectionInformation.text = "";
			u1 = false;
		}


		if ( developmentMode == true && Application.isEditor == false )
		{

			GUI.Window ( 5, new Rect ( Screen.width / 2 - 100F, Screen.height / 2 - 45, 200, 80 ), DeveloperMode, "Warning!" );
			GUI.BringWindowToFront ( 5 );
			GUI.FocusWindow ( 5 );
		}
	}

	
	void DeveloperMode ( int pwid )
	{

		GUI.Label ( new Rect ( -50, 10, 300, 40 ), "developerMode is On!" );
		if ( GUI.Button ( new Rect ( 75, 50, 50, 20 ), "Close" ))
		{

		    developmentMode = false;
			GUI.FocusWindow ( 0 );
			GUI.BringWindowToFront ( 0 );
		}
	}

	
	void NewVersion (int pwid)
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

			paneManager.popupBlocking = true;
			updateAvailable = false;
			showUnderlay = false;
			Application.Quit();
		}
	}

	IEnumerator UnableToConnectToOMB ()
	{

		connectionInformation.text = "Unable to connect to the OnlineMusicDatabase!";

		yield return new WaitForSeconds ( 10 );
		connectionInformation.text = "";
	}
}
