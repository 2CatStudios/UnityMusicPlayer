using System;
using System.IO;
using System.Net;
using UnityEngine;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
//Written by GibsonBethke
//Jesus, you are awesome!
public class DownloadManager : MonoBehaviour
{

	public GUISkin guiskin;

	StartupManager startupManager;
	MusicViewer musicViewer;
	OnlineMusicBrowser onlineMusicBrowser;
	LoadingImage loadingImage;
	PaneManager paneManager;

	string currentDownloadSize;
	string currentDownloadPercentage;

	int downloadWindowY = 300;
	int downloadWindowX = 300;

	internal bool showSongInformation = false;
	bool downloading = false;
	bool getDownloadSize = true;
	
	internal Uri url;
	internal Song song;
	internal String downloadButtonText;

	bool cancelInvoke = false;

	void Start()
	{

		startupManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent<StartupManager>();
		musicViewer = GameObject.FindGameObjectWithTag ( "MusicViewer" ).GetComponent<MusicViewer>();
		onlineMusicBrowser = GameObject.FindGameObjectWithTag ("OnlineMusicBrowser").GetComponent<OnlineMusicBrowser>();
		loadingImage = GameObject.FindGameObjectWithTag ( "LoadingImage" ).GetComponent<LoadingImage>();
		paneManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent<PaneManager>();
	}

	void GetInfo ()
	{

		currentDownloadPercentage = "0";

		Thread getInfoThread = new Thread (GetInfoThread);
		getInfoThread.Priority = System.Threading.ThreadPriority.AboveNormal;
		getInfoThread.Start();
	}

	void GetInfoThread ()
	{

		if(getDownloadSize == true)
		{
			
			try
			{
				
				System.Net.WebRequest req = System.Net.HttpWebRequest.Create(url);
				req.Method = "HEAD";
				System.Net.WebResponse resp = req.GetResponse();
				currentDownloadSize = Math.Round(float.Parse(resp.Headers.Get("Content-Length")) / 1024 / 1024, 2).ToString () + "MB";
			}catch{}
			getDownloadSize = false;
		}

		if ( song.supportLink == "NONE" )
			downloadWindowY = 270;
		else
			downloadWindowY = 300;

		onlineMusicBrowser.showUnderlay = true;
		showSongInformation = true;
		cancelInvoke = true;
	}

	void OnGUI ()
	{

		GUI.skin = guiskin;

		if ( showSongInformation == true )
		{

			GUI.Window ( 2, new Rect ( onlineMusicBrowser.onlineMusicBrowserPosition.x + onlineMusicBrowser.onlineMusicBrowserPosition.width/2 - downloadWindowX/2, onlineMusicBrowser.onlineMusicBrowserPosition.y + onlineMusicBrowser.onlineMusicBrowserPosition.height/2 - downloadWindowY/2, downloadWindowX, downloadWindowY ), DownloadInfo, song.name );
			GUI.BringWindowToFront ( 2 );
			GUI.FocusWindow ( 2 );
		}
	}

	void DownloadInfo (int wid)
	{

		if ( cancelInvoke == true )
		{

			loadingImage.showLoadingImages = false;
			cancelInvoke = false;
		}
		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();
		GUILayout.Space (10);

		if(downloading == false)
		{

			if(GUILayout.Button (downloadButtonText) && url != null)
			{
				
				Thread downloadThread = new Thread (Download);
				downloadThread.Priority = System.Threading.ThreadPriority.Highest;
				downloadThread.Start();

				downloading = true;
				if ( song.supportLink == "NONE" )
					downloadWindowY = 220;
				else
					downloadWindowY = 250;
			}
		}
			
		if(downloading == false)
		{

			if (GUILayout.Button ("Close"))
			{

				onlineMusicBrowser.showUnderlay = false;
				showSongInformation = false;
				onlineMusicBrowser.songInfoWindowOpen = false;
				paneManager.popupBlocking = false;
				getDownloadSize = true;
				GUI.FocusWindow ( 1 );
				GUI.BringWindowToFront ( 1 );
			}
		}
			
		GUILayout.Label ("Download size: ~" + currentDownloadSize + " - " + currentDownloadPercentage + "% Complete");
		GUILayout.Space (31);

		GUI.skin.label.alignment = TextAnchor.MiddleLeft;

		GUILayout.Label ("Name: " + song.name);
		GUILayout.Label ("Artist: " + song.artist.name);
		GUILayout.Label ("Album: " + song.album.name);
		GUILayout.Label ("Genre: " + song.genre.name);
		GUILayout.Label ("Format: " + song.format);

		GUI.skin.label.alignment = TextAnchor.MiddleCenter;

		if(song.supportLink != "NONE")
		{
			
			if (GUILayout.Button ("Support Artist"))
				Process.Start (song.supportLink);
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
			
			client.DownloadProgressChanged += (start, end) => {  currentDownloadPercentage = end.ProgressPercentage.ToString(); };
			
			client.DownloadFileAsync (url, startupManager.tempPath + Path.DirectorySeparatorChar + song.name + "." + song.format);
		} catch (Exception error)
		{
			
			UnityEngine.Debug.Log (error);
		}
	}
	
	void DownloadFileCompleted(object sender, AsyncCompletedEventArgs end)
	{

		File.Move ( startupManager.tempPath + Path.DirectorySeparatorChar + song.name + "." + song.format, musicViewer.mediaPath + Path.DirectorySeparatorChar + song.name + "." + song.format );
		currentDownloadPercentage = "0%";
		onlineMusicBrowser.showUnderlay = false;
		showSongInformation = false;
		downloading = false;
		onlineMusicBrowser.songInfoWindowOpen = false;
		paneManager.popupBlocking = false;
		getDownloadSize = true;
		GUI.FocusWindow ( 1 );
		GUI.BringWindowToFront ( 1 );
	}	
}