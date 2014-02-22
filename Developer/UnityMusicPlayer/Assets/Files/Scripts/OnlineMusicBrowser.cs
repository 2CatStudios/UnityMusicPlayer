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
//THanks to Mike Talbot
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
	public Texture2D missingArtwork;
	
	internal bool showOnlineMusicBrowser = false;

	Vector2 scrollPosition;
	Vector2 horizontalScrollPosition;
	internal Rect onlineMusicBrowserPosition = new Rect(0, 0, 800, 600);
	internal string onlineMusicBrowserTitle;
	
	internal bool showDownloadList = false;

	#region Lists
	
	List<Song> allSongsList;
	List<Song> allRecentlyAddedList;
	List<Song> specificSort;
	List<Featured> featuredList;
	
	SortedDictionary<string, Album> albums = new SortedDictionary<string, Album>();
	SortedDictionary<string, Artist> artists = new SortedDictionary<string, Artist>();
	SortedDictionary<string, Genre> genres = new SortedDictionary<string, Genre>();
	
	#endregion
	
	int sortBy = 0;
	string currentPlace;

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
		
		specificSort = allRecentlyAddedList;
		currentPlace = "Recently Added";
		
		paneManager.loading = false;	
		if ( paneManager.currentPane == PaneManager.pane.onlineMusicBrowser )
		{
			
			musicViewer.tempEnableOMB = 1.0F;
			startupManager.ombEnabled = true;
		}
		
		downloadArtwork = true;
	}
	
	
/*	IEnumerator DownloadFeatured ()
	{
		
		while ( downloadArtwork == false ) {}
		
		foreach ( Song song in allSongsList )
		{
			
			if ( song.featured == "true" )
			{
			
				WWW featuredArtworkWWW = new WWW ( song.smallArtworkURL );
				yield return featuredArtworkWWW;
				
				Texture2D tempArtwork = new Texture2D ( 256, 256 );
				featuredArtworkWWW.LoadImageIntoTexture ( tempArtwork );
				
				Featured tempFeatured = new Featured ();
				tempFeatured.song = song;
				tempFeatured.artwork = tempArtwork;
	
				featuredList.Add ( tempFeatured );
			}
		}
		
		UnityEngine.Debug.Log ( featuredList.Count );
		downloadArtwork = false;
	}
*/
	
	
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
			
			if ( GUILayout.Button ( "Featured" ))
			{
				
				sortBy = 1;
				currentPlace = "Featured";
			}
			
			if (GUILayout.Button ("Recent"))
			{
	
				sortBy = 2;
				currentPlace = "Recently Added";
			}
	
			if (GUILayout.Button ("Name"))
			{
	
				sortBy = 3;
				currentPlace = "Name";
			}
	
			if (GUILayout.Button ("Albums"))
			{
	
				sortBy = 4;
				currentPlace = "Albums";
			}
	
			if (GUILayout.Button ("Artists"))
			{
	
				sortBy = 5;
				currentPlace = "Artists";
			}
	
			if (GUILayout.Button ("Genres"))
			{
	
				sortBy = 6;
				currentPlace = "Genres";
			}
	
			GUILayout.EndHorizontal ();
			GUILayout.Space ( 5 );
			GUILayout.BeginHorizontal ();
			GUILayout.Space ( onlineMusicBrowserPosition.width / 2 - 300  );
	
			if ( sortBy != 1 )
				scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  onlineMusicBrowserPosition.height - ( onlineMusicBrowserPosition.height / 4 + 53 )));
			
			GUILayout.Box ( "Current Sort: " + currentPlace, GUILayout.MaxWidth ( 600 ));
				
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
				horizontalScrollPosition = GUILayout.BeginScrollView ( horizontalScrollPosition, GUILayout.Width ( onlineMusicBrowserPosition.width - 20 ), GUILayout.Height( 360 ));
				GUILayout.BeginHorizontal ();
				foreach ( Featured featured in featuredList )
				{
					
					if ( GUILayout.Button ( featured.artwork, GUILayout.MaxWidth ( 512 ), GUILayout.MaxHeight ( 512 )))
					{
						
						specificSort = new List<Song>();
						specificSort.Add ( featured.song );
						sortBy = 0;
					}
				}
				GUILayout.EndHorizontal ();
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
						currentPlace = "Albums > " + album.name;
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
						currentPlace = "Artists > " + artist.name;
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
						currentPlace = "Genres > " + genre.name;
						sortBy = 0;
					}
				}
				break;
				
				case 7:
				break;
					
				default:
				break;
			}
			
			guiSkin.button.normal.background = null;
			guiSkin.button.hover.background = guiHover;
			
			GUILayout.EndScrollView ();
			if ( sortBy != 1 )
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
