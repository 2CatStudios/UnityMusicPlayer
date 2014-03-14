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
//using System.Collections.Specialized;
//Written by GibsonBethke
//Thanks to Mike Talbot
[XmlRoot("Songs")]
public class SongCollection
{
	
	[XmlElement("Song")]
	public Song[] songs;
}


public class Song
{
	
	[XmlAttribute]
	public String featured;
	
	public String name;
	
	public String album;
	public String artist;
	public String genre;
	public String format;
	public String downloadURL;
	[XmlElement("link")]
         public Link[] links;
	
	public String releaseDate;
	public String largeArtworkURL;
	public String smallArtworkURL;
}


public class Link
{
	
	[XmlAttribute]
	public string name;
	[XmlText]
	public string address;
}


public class Album
{
	
	public String name;
	public List<Song> songs = new List<Song>();
	
	public Album () {}
}

public class Artist
{
	
	public String name;
	public List<Song> songs = new List<Song>();
	
	public Artist () {}
}

public class Genre
{

	public String name;
	public List<Song> songs = new List<Song>();
	
	public Genre () {}
}

public class Featured
{
	
	public Song song;
	public Texture2D artwork;
}

public class Sort
{
	
	public String name;
	public int method;
	
	public Sort ( String name, int method )
	{
		
		this.name = name;
		this.method = method;
	}
}


public class OnlineMusicBrowser : MonoBehaviour
{
	
	StartupManager startupManager;
	MusicViewer musicViewer;
	PaneManager paneManager;

	public GUISkin guiSkin;
	GUIStyle labelStyle;
	GUIStyle infoLabelStyle;
	GUIStyle buttonStyle;
	GUIStyle boxStyle;
	
	public Texture2D guiHover;
	public Texture2D guiActiveHover;
	public Texture2D missingArtwork;
	
	internal bool showOnlineMusicBrowser = false;

	internal Vector2 scrollPosition;
	internal Vector2 horizontalScrollPosition;
	internal Rect onlineMusicBrowserPosition = new Rect(0, 0, 800, 600);
	internal string onlineMusicBrowserTitle;
	
	internal bool showDownloadList = false;
	
	int sortBy = 1;
	Sort currentSort;

	#region Lists
	
	List<Sort> availableSorts;
	
	List<Song> allSongsList;
	List<Song> allRecentlyAddedList;
	List<Song> specificSort;
	List<Featured> featuredList;
	
	SortedDictionary<string, Album> albums = new SortedDictionary<string, Album>();
	SortedDictionary<string, Artist> artists = new SortedDictionary<string, Artist>();
	SortedDictionary<string, Genre> genres = new SortedDictionary<string, Genre>();
	
	#endregion
	
	#region DownloadInformation
	
	public WebClient client;
	
	Uri url;
	string downloadButtonText;
	
	string currentDownloadSize;
	string currentDownloadPercentage;
	
	bool showSongInformation = false;
	bool downloading = false;
	bool downloadArtwork = false;
	
	Song songInfoOwner;
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

		availableSorts = new List<Sort> () { new Sort ( "Featured", 1), new Sort ( "Recent", 2 ), new Sort ( "Name", 3 ), new Sort ( "Album", 4 ), new Sort ( "Artist", 5 ), new Sort ( "Genre", 6 ) };
		currentSort = availableSorts[0];
		
		allSongsList = new List<Song> ();
		allRecentlyAddedList = new List<Song> ();
		specificSort = new List<Song> ();
		featuredList = new List<Featured> ();
		
		Thread refreshThread = new Thread ( SortAvailableDownloads );
		refreshThread.Start();
		
		downloadArtwork = false;
		StartCoroutine ( "DownloadFeatured" );
	}
	
	
	void SortAvailableDownloads()
	{

		System.IO.StreamReader streamReader = new System.IO.StreamReader ( startupManager.supportPath + Path.DirectorySeparatorChar + "Downloads.xml" );
		string xml = streamReader.ReadToEnd();
		streamReader.Close();
		
		SongCollection songCollection = xml.DeserializeXml<SongCollection>();
		allSongsList = songCollection.songs.ToList ();
		allSongsList.Sort (( a, b ) => a.name.CompareTo ( b.name ));
		
		allRecentlyAddedList = songCollection.songs.ToList ();;
		allRecentlyAddedList.Reverse ();
		
		
		Album tempAlbum;
		Artist tempArtist;
		Genre tempGenre;

		foreach ( Song song in songCollection.songs )
		{
			
			tempAlbum = new Album ();
			tempAlbum.name = song.album;
			tempAlbum.songs.Add ( song );
			
			if ( !albums.ContainsKey ( tempAlbum.name ))
			{

				albums[tempAlbum.name] = tempAlbum;
			} else {
			
				albums[song.album].songs.Add ( song );
			}

			tempArtist = new Artist ();
			tempArtist.name = song.artist;
			tempArtist.songs.Add ( song );
			
			if ( !artists.ContainsKey ( tempArtist.name ))
			{

				artists[tempArtist.name] = tempArtist;
			} else {
			
				artists[song.artist].songs.Add ( song );
			}
			
			
			tempGenre = new Genre ();
			tempGenre.name = song.genre;
			tempGenre.songs.Add ( song );
			
			if ( !genres.ContainsKey ( tempGenre.name ))
			{

				genres[tempGenre.name] = tempGenre;
			} else {
			
				genres[song.genre].songs.Add ( song );
			}
			
			if ( song.featured == "true" )
			{
			
				Featured tempFeatured = new Featured ();
				tempFeatured.song = song;
				tempFeatured.artwork = missingArtwork;
	
				featuredList.Add ( tempFeatured );
			}
		}
		
		paneManager.loading = false;	
		if ( paneManager.currentPane == PaneManager.pane.onlineMusicBrowser )
		{
			
			musicViewer.tempEnableOMB = 1.0F;
			startupManager.ombEnabled = true;
		}
		
		downloadArtwork = true;
	}
	
	
	IEnumerator DownloadFeatured ()
	{
		
		while ( downloadArtwork == false ) {}
		
		foreach ( Featured featuredSong in featuredList )
		{
			
			WWW featuredArtworkWWW = new WWW ( featuredSong.song.smallArtworkURL );
			yield return featuredArtworkWWW;
				
			Texture2D downloadedArtwork = new Texture2D ( 256, 256 );
			featuredArtworkWWW.LoadImageIntoTexture ( downloadedArtwork );
				
			featuredSong.artwork = downloadedArtwork;
		}
		downloadArtwork = false;
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
			
			foreach ( Sort sort in availableSorts )
			{
				
				if ( currentSort == sort )	
					guiSkin.button.normal.background = guiHover;
				
				if ( GUILayout.Button ( sort.name ))
				{
					
					currentSort = sort;
					sortBy = sort.method;
					scrollPosition = new Vector2 ( 0, 0 );
				}
				
				guiSkin.button.normal.background = null;
				guiSkin.button.hover.background = guiHover;
			}
	
			GUILayout.EndHorizontal ();
			GUILayout.Space ( 5 );
			GUILayout.BeginHorizontal ();
			GUILayout.Space ( onlineMusicBrowserPosition.width / 2 - 300  );
	
			if ( sortBy != 1 )
				scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  onlineMusicBrowserPosition.height - ( onlineMusicBrowserPosition.height / 4 + 50 )));
				
			switch ( sortBy )
			{
				
				case 0:
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
						
						int firstEquation = ( allRecentlyAddedList.Count - 1 ) - specificSort.IndexOf ( song );
						int secondEquation = ( allRecentlyAddedList.Count - 1 ) - firstEquation;
						
						scrollPosition.y = secondEquation * 36;
						
						if ( showSongInformation == false || songInfoOwner != song )
						{
							
							if ( songInfoOwner != song )
							{
								
								showSongInformation = false;
								songInfoOwner = null;
							}
							
							if ( song.downloadURL.StartsWith ( "|" ) == true )
							{

								url = null;
								downloadButtonText = song.downloadURL.Substring ( 1 );
								
								currentDownloadPercentage = "";
								currentDownloadSize = "Unreleased";
							} else if ( song.downloadURL.StartsWith ( "h" ) == true )
							{
								
								url = new Uri ( song.downloadURL );
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
					
					guiSkin.button.normal.background = null;
					guiSkin.button.hover.background = guiHover;
					
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
							GUILayout.Label ( "Album: " + song.album, infoLabelStyle );
							GUILayout.Label ( "Artist: " + song.artist, infoLabelStyle );
							GUILayout.Label ( "Genre: " + song.genre, infoLabelStyle );
							GUILayout.Label ( "Format: " + song.format, infoLabelStyle );
							GUILayout.Label ( "Released: " + song.releaseDate, infoLabelStyle );
							if ( song.links != null )
							{
								
								foreach ( Link currentLink in song.links )
								{
									
									if ( GUILayout.Button ( currentLink.name, buttonStyle ))
										Process.Start ( currentLink.address );
								}
							}
							GUILayout.Label ( "" );
						}
					}
				}
				break;
				
				case 1:
				GUILayout.EndHorizontal ();
				horizontalScrollPosition = GUILayout.BeginScrollView ( horizontalScrollPosition, GUILayout.Width ( onlineMusicBrowserPosition.width - 20 ), GUILayout.Height( 390 ));
				GUILayout.BeginHorizontal ();
				foreach ( Featured featured in featuredList )
				{
					
					if ( GUILayout.Button ( featured.artwork, GUILayout.MaxWidth ( 360 ), GUILayout.MaxHeight ( 360 )))
					{
						
						songInfoOwner = featured.song;
						showSongInformation = true;
						
						if ( featured.song.downloadURL.StartsWith ( "|" ) == true )
						{

							url = null;
							downloadButtonText = featured.song.downloadURL.Substring ( 1 );
							
							currentDownloadPercentage = "";
							currentDownloadSize = "Unreleased";
						} else if ( featured.song.downloadURL.StartsWith ( "h" ) == true )
						{
								
							url = new Uri ( featured.song.downloadURL );
							downloadButtonText = "Download";
							
							currentDownloadPercentage = "";
							currentDownloadSize = "Loading";
								
							Thread getInfoThread = new Thread ( GetInfoThread );
							getInfoThread.Priority = System.Threading.ThreadPriority.AboveNormal;
							getInfoThread.Start ();
						}
						
						int firstEquation = ( allRecentlyAddedList.Count - 1 ) - allRecentlyAddedList.IndexOf ( featured.song );
						int secondEquation = ( allRecentlyAddedList.Count - 1 ) - firstEquation;
						
						scrollPosition.y = secondEquation * 36;
						
						currentSort = availableSorts[1];
						specificSort = allRecentlyAddedList;
						sortBy = 0;
					}
				}
				break;
	
				case 2:
				specificSort = allRecentlyAddedList;
				sortBy = 0;
				break;
	
				case 3:
				specificSort = allSongsList;
				sortBy = 0;
				break;
				
				case 4:
				foreach ( Album album in albums.Values )
				{
	
					if ( GUILayout.Button ( album.name ))
					{
	
						specificSort = album.songs;
						sortBy = 0;
					}
				}
				break;
				
				case 5:
				foreach ( Artist artist in artists.Values )
				{
					
					if ( GUILayout.Button ( artist.name ))
					{
	
						specificSort = artist.songs;
						sortBy = 0;
					}
				}
				break;
				
				case 6:
				foreach ( Genre genre in genres.Values )
				{
					
					if ( GUILayout.Button ( genre.name ))
					{
	
						specificSort = genre.songs;
						sortBy = 0;
					}
				}
				break;
			}
			
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
				
//				File.Move ( startupManager.tempPath + Path.DirectorySeparatorChar + downloadingSong.name + "." + downloadingSong.format, musicViewer.parentDirectory + Path.DirectorySeparatorChar + downloadingSong.name + "." + downloadingSong.format );
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
