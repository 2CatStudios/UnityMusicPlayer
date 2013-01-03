using System;
using System.IO;
using System.Net;
using UnityEngine;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Net.Security;
using System.Globalization;
using System.IO.Compression;
using System.ComponentModel;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
//Written by Gibson Bethke
//Thanks you Jesus; you are awesome!
public class WebManager : MonoBehaviour
{

	#region Variables

	public GUISkin skin;

	Uri url;
	string[] allSongs;

	static string path;
	public bool showDownloadList = false;

	bool newVersion = false;
	string[] applicationDownloads;
	public float newestVersion;
	string macVersionLink;
	string windowsVersionLink;

	Vector2 scrollPosition;

	string currentDownloadSize;
	string currentDownloadPercentage;

	Rect downloadInfoRect = new Rect(Screen.width/2 - 150, Screen.height/2 - 87, 300, 260);
	bool showDownloadInfo = false;
	bool currentlyDownloading = false;
	bool downloadError = false;
	Exception downloadErrorInfo;
	
	Song file;
	List<Song> allSongsList = new List<Song>();

	Rect windowRect = new Rect(100, 50, 600, 500);

	#endregion

	void Start ()
	{
		
		path = GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager> ().publicPath;
		ServicePointManager.ServerCertificateValidationCallback += delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

		using (WebClient wClient = new WebClient())
		try
		{

			applicationDownloads = wClient.DownloadString ("http://raw.github.com/2CatStudios/UnityMusicPlayer/master/Developer/ApplicationDownloads.txt").Split ('\n');
			allSongs = wClient.DownloadString ("http://raw.github.com/2CatStudios/UnityMusicPlayer/master/Developer/AllSongs.txt").Split ('\n');
		} catch (Exception error)
		{
		
			UnityEngine.Debug.Log (error);
			downloadErrorInfo = error;
			downloadError = true;
		}

		macVersionLink = applicationDownloads[4];
		windowsVersionLink = applicationDownloads[7];

/*		if (!Directory.Exists(path + Path.DirectorySeparatorChar + "AutomaticUpdater.app") && GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager>().onMac == true)
//		{
//
//
//			url = new Uri (applicationDownloads[10]);
//			Thread downloadUpdaterThread = new Thread (DownloadAutoUpdater);
//			downloadUpdaterThread.Start();
//		}
*/

		newestVersion = (float)Convert.ToDouble(applicationDownloads [1]);
		if(GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager>().currentVersion != newestVersion)
		{
			
			UnityEngine.Debug.LogWarning ("Versions Do NOT Match!");
			newVersion = true;
		}

		int i = 0;
		while (i < allSongs.Length)
		{

			i += 8;
			Song song = new Song();
			song.name = allSongs [i - 7];
			song.artist = allSongs [i - 6];
			song.album = allSongs [i - 5];
			song.genre = allSongs [i - 4];
			song.format = allSongs [i - 3];
			song.downloadLink = allSongs [i - 2];
			song.supportLink = allSongs [i - 1];
			
			allSongsList.Add(song);
		}

	}
	
	void OnGUI ()
	{

		GUI.skin = skin;

		//GUI.Window 1 doesn't exist

		//GUI.Window 2 is the InfoBar

		if(downloadError == true)
			GUI.Window (3, new Rect(0, 0, Screen.width, Screen.height), Error, "An Error Occurred, Please Restart!");

		if(showDownloadInfo == true)
		{
			
			downloadInfoRect = GUI.Window (4, downloadInfoRect, DownloadInfo, "Song Information");
			GUI.BringWindowToFront(4);
		}
			
		if (newVersion == true)
			GUI.Window (5, new Rect (Screen.width / 2 - 142.5F, Screen.height / 2 - 85, 300, 100), NewVersion, "An Update is Available");

		if (showDownloadList == true)
			windowRect = GUI.Window (6, windowRect, AllDownloads, "OnlineMusicBrowser");
	}

/*	void FeaturedDownloads (int wid)
	{

		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();
		GUILayout.Space (20);

		if (GUILayout.Button ("Close"))
		{

			showFeaturedDlL = false;
			GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager> ().clipList = null;
			GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager> ().clipList = Directory.GetFiles(path + Path.DirectorySeparatorChar + "Media" + Path.DirectorySeparatorChar, "*.wav");
			GameObject.FindGameObjectWithTag ("InfoBar").GetComponent<InfoBar>().songCount = GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager> ().clipList.Length;
			GameObject.FindGameObjectWithTag ("InfoBar").GetComponent<InfoBar> ().hideSongs = false;
			GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager> ().showGUI = true;
			GUI.BringWindowToFront(1);
		}

		scrollPosition = GUILayout.BeginScrollView (scrollPosition, GUILayout.Width (198), GUILayout.Height (250));

		int i = 0;
		file = 0;
		while (i < featuredSongs.Length)
		{

			i += 8;
			Song song = new Song();
			song.name = featuredSongs [i - 7];
			song.artist = featuredSongs [i - 6];
			song.album = featuredSongs [i - 5];
			song.genre = featuredSongs [i - 4];
			song.format = featuredSongs [i - 3];
			song.downloadLink = featuredSongs [i - 2];
			song.supportLink = featuredSongs [i - 1];
			
			featuredSongsList.Add(song);
			if (GUILayout.Button (featuredSongsList[file].name))
			{

				if(currentlyDownloading == false)
				{

					showDownloadInfo = true;
					url = new Uri (featuredSongsList[file].downloadLink);
				}
			}

			file ++;
		}

		GUILayout.FlexibleSpace ();

		if (GUILayout.Button ("Browse All Songs"))
		{

			showFeaturedDlL = false;
			showAllDlL = true;
		}

		GUI.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}
*/

	void AllDownloads(int wid)
	{
		
		GUILayout.BeginVertical ();
		GUILayout.Space (10);
		GUILayout.BeginHorizontal ();
		
		if (GUILayout.Button ("Artists"))
		{
			
			UnityEngine.Debug.Log ("Artists");
		}
		if (GUILayout.Button ("Albums"))
		{
			
			UnityEngine.Debug.Log ("Albums");
		}
		if (GUILayout.Button ("Genres"))
		{
			
			UnityEngine.Debug.Log ("Genres");
		}
		if (GUILayout.Button ("Close"))
		{
			
			showDownloadList = false;
			GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager> ().clipList = null;
			GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager> ().clipList = Directory.GetFiles(path + Path.DirectorySeparatorChar + "Media" + Path.DirectorySeparatorChar, "*.wav");
			GameObject.FindGameObjectWithTag ("InfoBar").GetComponent<InfoBar>().songCount = GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager> ().clipList.Length;
			GameObject.FindGameObjectWithTag ("InfoBar").GetComponent<InfoBar> ().hideSongs = false;
			GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager> ().showGUI = true;
			windowRect = new Rect(100, 50, 600, 500);
		}

		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
		GUILayout.Space (30);

		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();

		foreach (Song song in allSongsList)
		{
			
			if (GUILayout.Button (song.name))
			{
				
				if(currentlyDownloading == false)
				{

					file = song;
					showDownloadInfo = true;
					url = new Uri (song.downloadLink);
				}
			}
		}

		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();

		GUI.DragWindow ();
	}


	void DownloadInfo (int wid)
	{
					
		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();
		GUILayout.Space (10);

		if(currentlyDownloading == false)
		{

			if(GUILayout.Button ("Download"))
			{

				currentlyDownloading = true;
				try
				{

					System.Net.WebRequest req = System.Net.HttpWebRequest.Create(url);
					req.Method = "HEAD";
					System.Net.WebResponse resp = req.GetResponse();
					currentDownloadSize = Math.Round(float.Parse(resp.Headers.Get("Content-Length")) / 1024 / 1024, 2).ToString () + "MB";
				} catch
				{

					try
					{

						System.Net.WebRequest req = System.Net.HttpWebRequest.Create(url);
						req.Method = "HEAD";
						System.Net.WebResponse resp = req.GetResponse();
						currentDownloadSize = Math.Round(float.Parse(resp.Headers.Get("Content-Length")) / 1024 / 1024, 2).ToString () + "MB";
					} catch (Exception error)
					{

						downloadErrorInfo = error;
						downloadError = true;
					}
				}

				Thread downloadThread = new Thread (Download);
				downloadThread.Start();
			}

			if (GUILayout.Button ("Close"))
				showDownloadInfo = false;
				
		} else {

			GUILayout.Label ("   Download size: ~" + currentDownloadSize + " - " + currentDownloadPercentage + " Complete");
			GUILayout.Space (31);
		}
					
		GUILayout.Space (10);
					
		GUILayout.Label ("Name: " + file.name);
		GUILayout.Label ("Artist: " + file.artist);
		GUILayout.Label ("Album: " + file.album);
		GUILayout.Label ("Genre: " + file.genre);
		GUILayout.Label ("Format: " + file.format);
		if(file.supportLink != "NONE")
		{

			if (GUILayout.Button ("Support Artist"))
			{

				Process.Start (file.supportLink);
			}
		}
					
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
					
		GUI.DragWindow();
	}

	void Download ()
	{

		using (WebClient client = new WebClient())
		try
		{

			client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);

			client.DownloadProgressChanged += (start, end) => {  currentDownloadPercentage = end.ProgressPercentage.ToString() + "%"; };

			client.DownloadFileAsync (url, path + Path.DirectorySeparatorChar + "Media" + Path.DirectorySeparatorChar + file.name + "." + file.format);
		} catch (Exception error)
		{

			UnityEngine.Debug.Log (error);
			downloadErrorInfo = error;
			downloadError = true;
		}
	}

	void DownloadFileCompleted(object sender, AsyncCompletedEventArgs end)
	{

		currentDownloadPercentage = "0%";
		currentlyDownloading = false;
		showDownloadInfo = false;
	}

	void DownloadAutoUpdater ()
	{

		using (WebClient client = new WebClient())
		try
		{
			
			UnityEngine.Debug.Log ("Starting AutoUpdater Download");
			client.DownloadFile (url, path + Path.DirectorySeparatorChar + "AutomaticUpdater.zip");
			UnityEngine.Debug.Log ("Download Finished... Unzipping");

			
			using (Stream createFile = File.Create(path + Path.DirectorySeparatorChar + "AutomaticUpdater.app"))
			using (Stream openRead = File.OpenRead(path + Path.DirectorySeparatorChar + "AutomaticUpdater.zip"))
			using (Stream csStream = new GZipStream(openRead, CompressionMode.Decompress))
			{

				byte[] buffer = new byte[1024];
				int nRead;
				while ((nRead = csStream.Read(buffer, 0, buffer.Length)) > 0)
				{

					createFile.Write(buffer, 0, nRead);
				}
			}


		} catch (Exception error)
		{
			
			UnityEngine.Debug.Log (error);
			downloadErrorInfo = error;
			downloadError = true;
		}
	}

	void NewVersion (int pwid)
	{

		GUI.Label (new Rect (50, 30, 300, 40), "              Download Now");
		if (GUI.Button (new Rect (20, 60, 70, 30), "No"))
			newVersion = false;

		if (GUI.Button (new Rect (210, 60, 70, 30), "Yes"))
		{
		
			if(GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager>().onMac == true)
			{

/*				ProcessStartInfo processStartInfo = new ProcessStartInfo (path + Path.DirectorySeparatorChar + "AutomaticUpdater.app");
//				processStartInfo.Arguments = "--args -" + GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager>().appLocation + " -" + macVersionLink;
//				UnityEngine.Debug.Log (processStartInfo.Arguments);
//				Process.Start(processStartInfo);
*/

				Process.Start (macVersionLink);
			} else
			{

				Process.Start (windowsVersionLink);
			}
			Application.Quit();
		}
	}

	void Error (int pwid)
	{

		GUI.BringWindowToFront(4);
		GUI.FocusWindow (4);
		GUI.Label (new Rect(Screen.width/2 - 350, Screen.height/2 - 250, 700, 510), "Fatal Error: " + downloadErrorInfo);
	}
}

public class Song
{

	public String name;
	public String artist;
	public String album;
	public String genre;
	public String format;
	public String downloadLink;
	public String supportLink;
}