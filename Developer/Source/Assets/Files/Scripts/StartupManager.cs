using System;
using System.IO;
using System.Net;
using UnityEngine;
using System.Text;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
//Written by GibsonBethke
//Thank you for your ∞ mercy, Jesus!
public class StartupManager : MonoBehaviour
{

	public bool developmentMode = false;
	public bool musicMakerEnabled = true;

	public float runningVersion;
	float newestVersion;

	public GUIText connectionInformation;
	bool connctingToInternet = true;
	bool errorInInternetConnection = false;

	OnlineMusicBrowser onlineMusicBrowser;
	internal string[] allSongs;
	
	internal bool onMac;
	static string mac = "/Users/" + Environment.UserName + "/Library/Application Support/2Cat Studios/UnityMusicPlayer";
	static string windows = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\2Cat Studios\\UnityMusicPlayer";

	internal bool showFileTypes;

	string path;
	internal string supportPath;
	internal string mediaPath;

	int linesInPrefs = 19;

	string[] applicationDownloads;

	bool updateAvailable = false;

	string macVersionLink;
	string windowsVersionLink;
	
	void Start ()
	{    
		
		if(developmentMode == true)
			UnityEngine.Debug.Log("Development Mode is ON");

		connectionInformation.text = "Connecting to the OnlineMusicDatabase...";
		onlineMusicBrowser = GameObject.FindGameObjectWithTag ("OnlineMusicBrowser").GetComponent<OnlineMusicBrowser>();

		if(Environment.OSVersion.ToString().Substring (0, 4) == "Unix")
		{

			path = mac;
			onMac = true;
		} else
		{

			path = windows;
			onMac = false;
		}
		mediaPath = path + Path.DirectorySeparatorChar + "Media";

		supportPath = path + Path.DirectorySeparatorChar + "Support" + Path.DirectorySeparatorChar;

		if(!Directory.Exists (mediaPath))
			Directory.CreateDirectory(mediaPath);

		if(!Directory.Exists (supportPath))
			Directory.CreateDirectory(supportPath);

		Thread internetConnectionsThread = new Thread (InternetConnections);
		internetConnectionsThread.Priority = System.Threading.ThreadPriority.Highest;
		internetConnectionsThread.Start();

		if(!File.Exists (supportPath + "Preferences.umpp") || File.ReadAllLines ( supportPath + "Preferences.umpp" ).Length != linesInPrefs )
		{

			using (FileStream createPrefs = File.Create(supportPath + "Preferences.umpp"))
			{
				
				Byte[] preferences = new UTF8Encoding(true).GetBytes("False\nFalse\nFalse\nFalse\nFalse\n1.0\n0.373\n0.569\n1.000\nFalse\nFalse\nFalse\n0\n0\n0\n0\n0\n0\n0");
				createPrefs.Write(preferences, 0, preferences.Length);
			}
		}

		if(!File.Exists (supportPath + "FAQ & Tutorial.txt") | !File.Exists (supportPath + "ReadMe.txt"))
		{

			File.Copy (Application.streamingAssetsPath + Path.DirectorySeparatorChar + "FAQ & Tutorial.txt", supportPath + "FAQ & Tutorial.txt");
			File.Copy (Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ReadMe.txt", supportPath + "ReadMe.txt");

			Process.Start (supportPath + "FAQ & Tutorial.txt");
		} else if(developmentMode == false)
		{

			string faqVersion = "0.0";
			string readmeVersion = "0.0";
			
			StreamReader faq = new StreamReader(supportPath + "FAQ & Tutorial.txt");
			StreamReader readme = new StreamReader(supportPath + "ReadMe.txt");
			
			int lineCheck = 0;
			while(lineCheck < 2)
			{
				
				faqVersion = faq.ReadLine();
				readmeVersion = readme.ReadLine();
				lineCheck++;
			}
			faq.Close();
			readme.Close ();
			
			if(faqVersion != runningVersion.ToString ())
			{
				
				File.Delete (supportPath + "FAQ & Tutorial.txt");
				File.Copy (Application.streamingAssetsPath + Path.DirectorySeparatorChar + "FAQ & Tutorial.txt", supportPath + "FAQ & Tutorial.txt");
				Process.Start (supportPath + "FAQ & Tutorial.txt");
			}
			if(readmeVersion != runningVersion.ToString ())
			{
				
				File.Delete (supportPath + "ReadMe.txt");
				File.Copy (Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ReadMe.txt", supportPath + "ReadMe.txt");
				Process.Start (supportPath + "ReadMe.txt");
			}
		}
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
				applicationDownloads = wClient.DownloadString ("http://raw.github.com/2CatStudios/UnityMusicPlayer/master/ApplicationDownloads.txt").Split ('\n');

			} else {
				allSongs = wClient.DownloadString ("http://raw.github.com/2CatStudios/UnityMusicPlayer/master/Developer/AllSongs.txt").Split ('\n');
				applicationDownloads = wClient.DownloadString ("http://raw.github.com/2CatStudios/UnityMusicPlayer/master/Developer/ApplicationDownloads.txt").Split ('\n');
			}

			macVersionLink = applicationDownloads[5];
			windowsVersionLink = applicationDownloads[8];
			
			newestVersion = (float)Convert.ToDouble(applicationDownloads [1]);
			if(runningVersion != newestVersion)
			{
				
				updateAvailable = true;
			}
		} catch (Exception errorText)
		{
			
			UnityEngine.Debug.Log (errorText);
			errorInInternetConnection = true;
		}
		connctingToInternet = false;
		onlineMusicBrowser.startOMB = true;
	}

	void OnGUI ()
	{

		GUI.skin.label.alignment = TextAnchor.MiddleCenter;

		if (updateAvailable == true)
		{

			GUI.Window(3, new Rect (Screen.width / 2 - 142.5F, Screen.height / 2 - 85, 300, 100), NewVersion, "An Update is Available");
			GUI.FocusWindow (3);
			GUI.BringWindowToFront ( 3 );
		}

		if(connctingToInternet == false)
			connectionInformation.text = "";
		
		if(errorInInternetConnection == true)
			connectionInformation.text = "Unable to connect to the OnlineMusicDatabase!";

		if(developmentMode == true && Application.isEditor == false)
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
		}
	}

	void NewVersion (int pwid)
	{
		
		GUI.Label (new Rect (0, 15, 300, 40), applicationDownloads[2]);
		GUI.Label (new Rect (0, 50, 300, 40), "Download now?");
		if (GUI.Button (new Rect (20, 60, 70, 30), "No"))
			updateAvailable = false;
		
		if (GUI.Button (new Rect (210, 60, 70, 30), "Yes"))
		{

			if(onMac == true)
				Process.Start (macVersionLink);
			else
				Process.Start (windowsVersionLink);
			Application.Quit();
		}
	}
}
