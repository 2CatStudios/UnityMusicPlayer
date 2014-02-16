using System;
using System.IO;
using System.Xml;
using System.Net;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections.Generic;
//Written by GibsonBethke
//THanks to Mike Talbot
[XmlRoot("Songs")]
public class SongCollection
{
	
	[XmlElement("Song")]
	public Song[] songs;
}


public class Song
{
	
	public String name;

	public Album album;
	public Artist artist;
	public Genre genre;

	public String format;
	public String downloadLink;
	[XmlElement("Link")]
         public Link[] Links;
	
	public String releaseDate;
}


public class Link
{
	
	[XmlAttribute]
	public string name;
	[XmlText]
	public string link;
}


public class Album
{
	
	public String name;
	public List<Song> songs;
	
	public Album () {}
}

public class Artist
{
	
	public String name;
	public List<Song> songs;
	
	public Artist () {}
}

public class Genre
{

	public String name;
	public List<Song> songs;
	
	public Genre () {}
}

public class OnlineMusicBrowser : MonoBehaviour
{
	
	StartupManager startupManager;
	MusicViewer musicViewer;
	PaneManager paneManager;

	#region OMBVariables

	public GUISkin guiSkin;
	GUIStyle labelStyle;
	GUIStyle infoLabelStyle;
	GUIStyle buttonStyle;
	GUIStyle boxStyle;
	
	public Texture2D guiHover;
	public Texture2D guiActiveHover;
	
	internal bool showOnlineMusicBrowser = false;

	Vector2 scrollPosition;
	internal Rect onlineMusicBrowserPosition = new Rect(0, 0, 800, 600);
	internal string onlineMusicBrowserTitle;
	
	internal bool showDownloadList = false;

	#region Lists
	
	List<Song> allRecentList;
	List<Song> allSongsList;
	List<Album> allAlbumsList;
	List<Artist> allArtistsList;
	List<Genre> allGenresList;
	List<Song> specificSort;
	
	#endregion
	
	int sortBy = 5;
	string currentPlace = "Recent";

	#endregion
	
	#region DownloadInformation
	
	Song songInfoOwner;
	
	public WebClient client;
	
	Uri url;
//	Song song;
	string downloadButtonText;
	
	string currentDownloadSize;
	string currentDownloadPercentage;
	
	bool showSongInformation = false;
	bool downloading = false;
	
	Song downloadingSong;
	
	#endregion
	

	void Start ()
	{

		startupManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent<StartupManager>();
		musicViewer = GameObject.FindGameObjectWithTag ( "MusicViewer" ).GetComponent<MusicViewer>();
		paneManager = GameObject.FindGameObjectWithTag ("Manager").GetComponent<PaneManager>();

		onlineMusicBrowserPosition.width = Screen.width;
		onlineMusicBrowserPosition.height = Screen.height;
		onlineMusicBrowserPosition.x = onlineMusicBrowserPosition.width + onlineMusicBrowserPosition.width / 4;
		
		labelStyle = new GUIStyle ();
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.wordWrap = true;
		
		infoLabelStyle = new GUIStyle ();
		infoLabelStyle.alignment = TextAnchor.MiddleLeft;
		infoLabelStyle.fontSize = 16;
		
		buttonStyle = new GUIStyle ();
		buttonStyle.fontSize = 16;
		buttonStyle.alignment = TextAnchor.MiddleCenter;
		buttonStyle.border = new RectOffset ( 6, 6, 4, 4 );
		buttonStyle.hover.background = guiHover;
	}
	

	void StartOMB ()
	{
		
//		allSongs = null;
		allRecentList = new List<Song> ();
		allSongsList = new List<Song> ();
		allAlbumsList = new List<Album> ();
		allArtistsList = new List<Artist> ();
		allGenresList = new List<Genre> ();
		specificSort = new List<Song> ();
		
		Thread refreshThread = new Thread ( SortAvailableDownloads );
		refreshThread.Start();
	}
	
	
	void SortAvailableDownloads()
	{

		System.IO.StreamReader streamReader = new System.IO.StreamReader ( startupManager.supportPath + Path.DirectorySeparatorChar + "Downloads.xml" );
		string xml = streamReader.ReadToEnd();
		streamReader.Close();
		
		SongCollection songCollection = xml.DeserializeXml<SongCollection>();
			
		UnityEngine.Debug.Log ( "XML Deserialized" );
		
		int i = 0;
		while ( i <= songCollection.songs.Length )
		{
			
			
			UnityEngine.Debug.Log ( "Song Name " + i + " is " + songCollection.songs[i].name + "."  );
			UnityEngine.Debug.Log ( "Song Album " + i + " is " + songCollection.songs[i].album + "."  );
			UnityEngine.Debug.Log ( "Song Artist " + i + " is " + songCollection.songs[i].artist + "."  );
			UnityEngine.Debug.Log ( "Song Genre " + i + " is " + songCollection.songs[i].genre + "."  );
			UnityEngine.Debug.Log ( "Song Format " + i + " is " + songCollection.songs[i].format + "."  );
			UnityEngine.Debug.Log ( "Song Download " + i + " is " + songCollection.songs[i].downloadLink + "."  );
			UnityEngine.Debug.Log ( "Song Release " + i + " is " + songCollection.songs[i].releaseDate + "."  );
			i+= 1;
		}
		
		
		
		
		specificSort = allRecentList;
		currentPlace = "Recent";
		
		paneManager.loading = false;
		
		if ( paneManager.currentPane == PaneManager.pane.onlineMusicBrowser )
		{
			
			musicViewer.tempEnableOMB = 1.0F;
			startupManager.ombEnabled = true;
		}
	}


	void OnGUI ()
	{

		if ( showOnlineMusicBrowser == true )
		{
		
			GUI.skin = guiSkin;
		
			if ( paneManager.loading == false )
				onlineMusicBrowserPosition = GUI.Window ( 1, onlineMusicBrowserPosition, OnlineMusicBrowserPane, onlineMusicBrowserTitle );
		}
	}


	void OnlineMusicBrowserPane ( int wid )
	{
		
		if ( startupManager.ombEnabled == true )
		{
	
			GUILayout.Space ( onlineMusicBrowserPosition.width / 8 );
			GUILayout.BeginHorizontal ();
	
			if (GUILayout.Button ("Name"))
			{
	
				sortBy = 0;
				currentPlace = "Name";
			}
	
			if (GUILayout.Button ("Albums"))
			{
	
				sortBy = 1;
				currentPlace = "Albums";
			}
	
			if (GUILayout.Button ("Artists"))
			{
	
				sortBy = 2;
				currentPlace = "Artists";
			}
	
			if (GUILayout.Button ("Genres"))
			{
	
				sortBy = 3;
				currentPlace = "Genres";
			}
	
			if (GUILayout.Button ("Recent"))
			{
	
				sortBy = 4;
				currentPlace = "Recent";
			}
	
			GUILayout.EndHorizontal ();
			GUILayout.Space ( 5 );
			GUILayout.BeginHorizontal ();
			GUILayout.Space ( onlineMusicBrowserPosition.width / 2 - 300  );
	
			scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  onlineMusicBrowserPosition.height - ( onlineMusicBrowserPosition.height / 4 + 53 )));
			GUILayout.Box ( "Current Sort: " + currentPlace );
			
			switch (sortBy)
			{		
				
				case 0:
				specificSort = allSongsList;
				sortBy = 5;
				break;
				
				case 1:
				foreach ( Album album in allAlbumsList )
				{
	
					if ( GUILayout.Button ( album.name ))
					{
	
						specificSort = album.songs;
						currentPlace = "Albums > " + album.name;
						sortBy = 5;
					}
				}
				break;
	
				case 2:
				foreach ( Artist artist in allArtistsList )
				{
					
					if ( GUILayout.Button ( artist.name ))
					{
	
						specificSort = artist.songs;
						currentPlace = "Artists > " + artist.name;
						sortBy = 5;
					}
				}
				break;
	
				case 3:
				foreach ( Genre genre in allGenresList )
				{
					
					if ( GUILayout.Button ( genre.name ))
					{
	
						specificSort = genre.songs;
						currentPlace = "Genres > " + genre.name;
						sortBy = 5;
					}
				}
				break;
				
				case 4:
				specificSort = allRecentList;
				sortBy = 5;
				break;
				
				case 5:
				foreach ( Song song in specificSort )
				{
					
					if ( songInfoOwner == song )
					{
						
						guiSkin.button.normal.background = guiHover;
						guiSkin.button.hover.background = guiActiveHover;
					} else {
						
						guiSkin.button.normal.background = null;
						guiSkin.button.hover.background = guiHover;
					}
					
					if ( GUILayout.Button ( song.name ))
					{
						
						if ( showSongInformation == false || songInfoOwner != song )
						{
							
							if ( songInfoOwner != song )
							{
								
								showSongInformation = false;
								songInfoOwner = null;
							}
							
							if ( song.downloadLink.StartsWith ( "|" ) == true )
							{
									
								url = null;
								downloadButtonText = song.downloadLink.Substring ( 1 );
								
								currentDownloadPercentage = "";
								currentDownloadSize = "Unreleased";
							} else if ( song.downloadLink.StartsWith ( "h" ) == true )
							{
								
								url = new Uri ( song.downloadLink );
								downloadButtonText = "Download";
								
								currentDownloadPercentage = "";
								currentDownloadSize = "Loading";
									
								Thread getInfoThread = new Thread ( GetInfoThread );
								getInfoThread.Priority = System.Threading.ThreadPriority.AboveNormal;
								getInfoThread.Start ();
								}
								
								showSongInformation = true;
								songInfoOwner = song;
							} else {
							
							showSongInformation = false;
							songInfoOwner = null;
						}
					}
					
					if ( showSongInformation == true )
					{
						
						if ( songInfoOwner == song )
						{
						
							if ( downloading == false )
							{
					
								if ( GUILayout.Button ( downloadButtonText, buttonStyle ) && url != null )
								{
									
									downloadingSong = song;
									
									currentDownloadPercentage = " - Processing";
									
									try
									{
										
										using ( client = new WebClient ())
										{
						 
							        		client.DownloadFileCompleted += new AsyncCompletedEventHandler ( DownloadFileCompleted );
							
							        		client.DownloadProgressChanged += new DownloadProgressChangedEventHandler( DownloadProgressCallback );
											
							        		client.DownloadFileAsync ( url, startupManager.tempPath + Path.DirectorySeparatorChar + song.name + "." + song.format );
										}
									} catch ( Exception error ) {
										
										UnityEngine.Debug.Log ( error );
									}
												
									downloading = true;

								}
							} else {
									
								GUILayout.Label ( "Downloading '" + downloadingSong.name + "'", labelStyle );

								if ( GUILayout.Button ( "Cancel Download", buttonStyle ))
								{
										
									client.CancelAsync ();
								}
							}
							
							if ( downloadingSong == songInfoOwner )
								GUILayout.Label ( "Download size: ~" + currentDownloadSize + currentDownloadPercentage );
							else
								GUILayout.Label ( "Download size: ~" + currentDownloadSize );
					
							GUILayout.Label ( "Name: " + song.name, infoLabelStyle );
							GUILayout.Label ( "Artist: " + song.artist.name, infoLabelStyle );
							GUILayout.Label ( "Album: " + song.album.name, infoLabelStyle );
							GUILayout.Label ( "Genre: " + song.genre.name, infoLabelStyle );
							GUILayout.Label ( "Format: " + song.format, infoLabelStyle );
							GUILayout.Label ( "Released: " + song.releaseDate, infoLabelStyle );
/*							if ( song.supportLink != "NONE" )
							{
								
								if ( GUILayout.Button ( "Support " + song.artist.name, buttonStyle ))
									Process.Start ( song.supportLink );
							}
						*/	GUILayout.Label ( "" );
						}
					}
				}		
				
				break;
					
				default:
				break;
			}
			
			guiSkin.button.normal.background = null;
			guiSkin.button.hover.background = guiHover;
			
			GUILayout.EndScrollView ();
			GUILayout.EndHorizontal ();
			
		} else {
			
			GUI.Label ( new Rect ( 10, onlineMusicBrowserPosition.height / 4, onlineMusicBrowserPosition.width - 20, 128 ), "The OnlineMusicBrowser has been disabled!", labelStyle );
			if ( GUI.Button ( new Rect ( onlineMusicBrowserPosition.width/2 - 160, onlineMusicBrowserPosition.height / 2, 320, 64 ), "Enable OnlineMusicBrowser" ))
			{
				
				startupManager.SendMessage ( "RefreshOMB" );
			}
		}
	}
	
	void GetInfoThread ()
	{
	
		try
		{
					
			System.Net.WebRequest req = System.Net.HttpWebRequest.Create ( url );
			req.Method = "HEAD";
			System.Net.WebResponse resp = req.GetResponse();
			currentDownloadSize = Math.Round ( float.Parse ( resp.Headers.Get ( "Content-Length" )) / 1024 / 1024, 2 ).ToString () + "MB";
		} catch ( Exception e ) {
			
			UnityEngine.Debug.Log ( e );
		}
	}
	
	
	void DownloadFileCompleted ( object sender, AsyncCompletedEventArgs end )
	{
		
		if ( downloading == true )
		{
			
			if ( end.Cancelled == true )
			{
				
				File.Delete ( startupManager.tempPath + Path.DirectorySeparatorChar + downloadingSong.name + "." + downloadingSong.format );
			} else {
				
				if ( File.Exists ( musicViewer.mediaPath + Path.DirectorySeparatorChar + downloadingSong.name + "." + downloadingSong.format ))
					File.Delete ( musicViewer.mediaPath + Path.DirectorySeparatorChar + downloadingSong.name + "." + downloadingSong.format );
				
				File.Move ( startupManager.tempPath + Path.DirectorySeparatorChar + downloadingSong.name + "." + downloadingSong.format, musicViewer.mediaPath + Path.DirectorySeparatorChar + downloadingSong.name + "." + downloadingSong.format );
				musicViewer.clipList = Directory.GetFiles ( musicViewer.mediaPath, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
			}

			songInfoOwner = null;
			currentDownloadPercentage = "";
			showSongInformation = false;
			downloading = false;
		}
	}	
	
	
	void DownloadProgressCallback ( object sender, DownloadProgressChangedEventArgs arg )
	{
	
		currentDownloadPercentage = " - " + arg.ProgressPercentage.ToString () + "% Complete";
	}
}
