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
//Written by GibsonBethke
//Thank you for your grace, Jesus

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


public class OnlineMusicViewer : MonoBehaviour
{

	#region Variables

	public GUISkin skin;
	
	Uri url;
	string[] allSongs;
	string[] applicationDownloads;
	
	static string path;
	
	bool updateAvailable = false;
	public float newestVersion;
	string macVersionLink;
	string windowsVersionLink;
	
	Vector2 scrollPosition;
	
	string currentDownloadSize;
	string currentDownloadPercentage;
	
	Rect downloadInfoRect = new Rect(Screen.width/2 - 150, Screen.height/2 - 87, 300, 260);
	bool showDownloadInfo = false;
	public bool showDownloadList = false;
	bool currentlyDownloading = false;
	
	Song file;
	List<Song> allSongsList = new List<Song>();

	internal Rect onlineMusicViewerPosition = new Rect(1000, 0, 800, 600);

	int sortBy = 0;

	#endregion
	
	void Start ()
	{
		
		path = GameObject.FindGameObjectWithTag ("Manager").GetComponent<StartupManager>().publicMediaPath;
		ServicePointManager.ServerCertificateValidationCallback += delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

		bool error = false;
		using (WebClient wClient = new WebClient())
		try
		{
			
			applicationDownloads = wClient.DownloadString ("http://raw.github.com/2CatStudios/UnityMusicPlayer/master/Developer/ApplicationDownloads.txt").Split ('\n');
			allSongs = wClient.DownloadString ("http://raw.github.com/2CatStudios/UnityMusicPlayer/master/Developer/AllSongs.txt").Split ('\n');
		} catch (Exception errorText)
		{
			
			UnityEngine.Debug.Log (errorText);
			error = true;
		}

		if(error == false)
		{

			macVersionLink = applicationDownloads[4];
			windowsVersionLink = applicationDownloads[7];

			newestVersion = (float)Convert.ToDouble(applicationDownloads [1]);
			if(GameObject.FindGameObjectWithTag ("Manager").GetComponent<StartupManager>().runningVersion != newestVersion)
			{
				
				UnityEngine.Debug.LogWarning ("Versions Do NOT Match!");
				updateAvailable = true;
			}
		} else
		{

			UnityEngine.Debug.Log ("No internet connection could be established");
		}
	}
	
	void Refresh ()
	{

		Thread refreshThread = new Thread (RefreshThread);
		refreshThread.Start();
	}

	void RefreshThread ()
	{

		using (WebClient wClient = new WebClient())
		try
		{

			allSongs = wClient.DownloadString ("http://raw.github.com/2CatStudios/UnityMusicPlayer/master/Developer/AllSongs.txt").Split ('\n');
		} catch {}
		
		allSongsList.Clear();
		
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

		onlineMusicViewerPosition = GUI.Window (1, onlineMusicViewerPosition, OnlineMusicViewerPane, "");
		
		if(showDownloadInfo == true)
		{
			
			downloadInfoRect = GUI.Window (2, downloadInfoRect, DownloadInfo, "Song Information");
			GUI.BringWindowToFront(3);
		}
		
		if (updateAvailable == true)
			GUI.Window (3, new Rect (Screen.width / 2 - 142.5F, Screen.height / 2 - 85, 300, 100), NewVersion, "An Update is Available");
	}

	void OnlineMusicViewerPane (int wid)
	{

		GUILayout.BeginVertical ();
		GUILayout.Space (100);
		GUILayout.BeginHorizontal ();

		if (GUILayout.Button ("Name"))
			sortBy = 0;

		if (GUILayout.Button ("Artists"))
			sortBy = 1;

		if (GUILayout.Button ("Albums"))
			sortBy = 2;

		if (GUILayout.Button ("Genres"))
			sortBy = 3;
		
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
		GUILayout.Space (30);

		GUILayout.BeginHorizontal ();
		GUILayout.Space (100);
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(600), GUILayout.Height(400));

		switch (sortBy)
		{

			case 0:
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
			break;
			
			case 1:
			foreach (Song song in allSongsList)
			{
				
				if (GUILayout.Button (song.artist))
				{
					
					if(currentlyDownloading == false)
					{
						
						file = song;
						showDownloadInfo = true;
						url = new Uri (song.downloadLink);
					}
				}
			}
			break;

			case 2:
			foreach (Song song in allSongsList)
			{
				
				if (GUILayout.Button (song.album))
				{
					
					if(currentlyDownloading == false)
					{
						
						file = song;
						showDownloadInfo = true;
						url = new Uri (song.downloadLink);
					}
				}
			}
			break;

			case 3:
			foreach (Song song in allSongsList)
			{
				
				if (GUILayout.Button (song.genre))
				{
					
					if(currentlyDownloading == false)
					{
						
						file = song;
						showDownloadInfo = true;
						url = new Uri (song.downloadLink);
					}
				}
			}
			break;

			default:
			break;

		}

		GUILayout.EndScrollView ();
		GUILayout.EndHorizontal ();
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
						
						UnityEngine.Debug.Log (error);
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
			
			client.DownloadFileAsync (url, path + Path.DirectorySeparatorChar + file.name + "." + file.format);
		} catch (Exception error)
		{
			
			UnityEngine.Debug.Log (error);
		}
	}
	
	void DownloadFileCompleted(object sender, AsyncCompletedEventArgs end)
	{
		
		currentDownloadPercentage = "0%";
		currentlyDownloading = false;
		showDownloadInfo = false;
	}
	
	void NewVersion (int pwid)
	{
		
		GUI.Label (new Rect (50, 30, 300, 40), "Download Now");
		if (GUI.Button (new Rect (20, 60, 70, 30), "No"))
			updateAvailable = false;
		
		if (GUI.Button (new Rect (210, 60, 70, 30), "Yes"))
		{



			if(GameObject.FindGameObjectWithTag("StartupManager").GetComponent<StartupManager>().onMac == true)
				Process.Start (macVersionLink);
			else
				Process.Start (windowsVersionLink);
			Application.Quit();
		}
	}
}