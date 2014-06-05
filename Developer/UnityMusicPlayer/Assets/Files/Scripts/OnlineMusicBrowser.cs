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
	public GUIStyle normalStyle;
	public GUIStyle activeStyle;
	
	public Sort ( String name, int method, GUIStyle normalStyle, GUIStyle activeStyle )
	{
		
		this.name = name;
		this.method = method;
		this.normalStyle = normalStyle;
		this.activeStyle = activeStyle;
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
	GUIStyle sortLabelStyle;
	
	GUIStyle buttonStyle;
	GUIStyle boxStyle;
	
	GUIStyle currentStyle;
	
	public Texture2D flagNormal;
	public Texture2D flagHover;
	public Texture2D flagOnNormal;
	public Texture2D flagOnHover;
	
	public Texture2D clockNormal;
	public Texture2D clockHover;
	public Texture2D clockOnNormal;
	public Texture2D clockOnHover;
	
	public Texture2D textNormal;
	public Texture2D textHover;
	public Texture2D textOnNormal;
	public Texture2D textOnHover;
	
	public Texture2D recordNormal;
	public Texture2D recordHover;
	public Texture2D recordOnNormal;
	public Texture2D recordOnHover;
	
	public Texture2D personNormal;
	public Texture2D personHover;
	public Texture2D personOnNormal;
	public Texture2D personOnHover;
	
	public Texture2D listNormal;
	public Texture2D listHover;
	public Texture2D listOnNormal;
	public Texture2D listOnHover;
	
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
	bool downloadFeaturedArtwork;
	internal bool downloadArtwork;
	
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
		
		downloadArtwork = Convert.ToBoolean ( startupManager.prefs [ 29 ]);
		
		labelStyle = new GUIStyle ();
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.wordWrap = true;
		
		infoLabelStyle = new GUIStyle ();
		infoLabelStyle.alignment = TextAnchor.MiddleLeft;
		infoLabelStyle.fontSize = 16;
		
		sortLabelStyle = new GUIStyle ();
		sortLabelStyle.alignment = TextAnchor.MiddleRight;
		sortLabelStyle.fontSize = 16;
		
		buttonStyle = new GUIStyle ();
		buttonStyle.fontSize = 16;
		buttonStyle.alignment = TextAnchor.MiddleCenter;
		buttonStyle.border = new RectOffset ( 6, 6, 4, 4 );
		buttonStyle.hover.background = guiHover;
		
		
		GUIStyle flagStyle = new GUIStyle ();
		flagStyle.normal.background = flagNormal;
		flagStyle.hover.background = flagHover;
		flagStyle.border = new RectOffset ( 0, 0, 0, 0 );
		
		GUIStyle flagStyleActive = new GUIStyle ();
		flagStyleActive.normal.background = flagOnNormal;
		flagStyleActive.hover.background = flagOnHover;
		flagStyleActive.border = new RectOffset ( 0, 0, 0, 0 );
		
		
		GUIStyle clockStyle = new GUIStyle ();
		clockStyle.normal.background = clockNormal;
		clockStyle.hover.background = clockHover;
		clockStyle.border = new RectOffset ( 0, 0, 0, 0 );
		
		GUIStyle clockStyleActive = new GUIStyle ();
		clockStyleActive.normal.background = clockOnNormal;
		clockStyleActive.hover.background = clockOnHover;
		clockStyleActive.border = new RectOffset ( 0, 0, 0, 0 );
		
		
		GUIStyle textStyle = new GUIStyle ();
		textStyle.normal.background = textNormal;
		textStyle.hover.background = textHover;
		textStyle.border = new RectOffset ( 0, 0, 0, 0 );
		
		GUIStyle textStyleActive = new GUIStyle ();
		textStyleActive.normal.background = textOnNormal;
		textStyleActive.hover.background = textOnHover;
		textStyleActive.border = new RectOffset ( 0, 0, 0, 0 );
		
		
		GUIStyle recordStyle = new GUIStyle ();
		recordStyle.normal.background = recordNormal;
		recordStyle.hover.background = recordHover;
		recordStyle.border = new RectOffset ( 0, 0, 0, 0 );
		
		GUIStyle recordStyleActive = new GUIStyle ();
		recordStyleActive.normal.background = recordOnNormal;
		recordStyleActive.hover.background = recordOnHover;
		recordStyleActive.border = new RectOffset ( 0, 0, 0, 0 );
		
		
		GUIStyle personStyle = new GUIStyle ();
		personStyle.normal.background = personNormal;
		personStyle.hover.background = personHover;
		personStyle.border = new RectOffset ( 0, 0, 0, 0 );
		
		GUIStyle personStyleActive = new GUIStyle ();
		personStyleActive.normal.background = personOnNormal;
		personStyleActive.hover.background = personOnHover;
		personStyleActive.border = new RectOffset ( 0, 0, 0, 0 );
		
		
		GUIStyle listStyle = new GUIStyle ();
		listStyle.normal.background = listNormal;
		listStyle.hover.background = listHover;
		listStyle.border = new RectOffset ( 0, 0, 0, 0 );
		
		GUIStyle listStyleActive = new GUIStyle ();
		listStyleActive.normal.background = listOnNormal;
		listStyleActive.hover.background = listOnHover;
		listStyleActive.border = new RectOffset ( 0, 0, 0, 0 );
		
		
		availableSorts = new List<Sort> ()
		{
			
			new Sort ( "Featured", 1, flagStyle, flagStyleActive ),
			new Sort ( "Recent", 2, clockStyle, clockStyleActive ),
			new Sort ( "Name", 3, textStyle, textStyleActive ),
			new Sort ( "Album", 4, recordStyle, recordStyleActive ),
			new Sort ( "Artist", 5, personStyle, personStyleActive ),
			new Sort ( "Genre", 6, listStyle, listStyleActive )
		};
	}
	

	void StartOMB ()
	{
		
		currentSort = availableSorts[0];
		
		allSongsList = new List<Song> ();
		allRecentlyAddedList = new List<Song> ();
		specificSort = new List<Song> ();
		featuredList = new List<Featured> ();
		
		Thread refreshThread = new Thread ( SortAvailableDownloads );
		refreshThread.Start();
		
		downloadFeaturedArtwork = false;
		StartCoroutine ( "DownloadFeatured" );
	}
	
	
	void SortAvailableDownloads()
	{

		System.IO.StreamReader streamReader = new System.IO.StreamReader ( startupManager.supportPath + "Downloads.xml" );
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
		
		downloadFeaturedArtwork = true;
	}
	
	
	IEnumerator DownloadFeatured ()
	{
		
		while ( downloadFeaturedArtwork == false ) {}
		
		foreach ( Featured featuredSong in featuredList )
		{
			
			if ( String.IsNullOrEmpty ( featuredSong.song.smallArtworkURL ) == false )
			{
			
				WWW featuredArtworkWWW = new WWW ( featuredSong.song.smallArtworkURL );
				yield return featuredArtworkWWW;
				
				Texture2D downloadedArtwork = new Texture2D ( 360, 360 );
				featuredArtworkWWW.LoadImageIntoTexture ( downloadedArtwork );
				
				featuredSong.artwork = downloadedArtwork;
			}
		}
		
		downloadFeaturedArtwork = false;
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
			GUILayout.BeginArea ( new Rect ( 20, onlineMusicBrowserPosition.width / 8 + 5, 216, 40 ));
			GUILayout.BeginHorizontal ();
			
			foreach ( Sort sort in availableSorts )
			{
				
				if ( currentSort == sort )
					currentStyle = sort.activeStyle;
				else
					currentStyle = sort.normalStyle;
				
				if ( GUILayout.Button ( "", currentStyle, GUILayout.Height ( 36 )))
				{
					
					currentSort = sort;
					sortBy = sort.method;
					scrollPosition = new Vector2 ( 0, 0 );
				}
			}
			
			GUILayout.EndHorizontal ();
			GUILayout.EndArea ();
			
			GUI.Label ( new Rect ( onlineMusicBrowserPosition.width - 236, onlineMusicBrowserPosition.width / 8 + 5, 216, 40 ), currentSort.name, sortLabelStyle );
			
			GUILayout.Space ( 20 );
			GUILayout.BeginHorizontal ();
			GUILayout.Space ( 100 );
	
			if ( sortBy != 1 )
				scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  onlineMusicBrowserPosition.height - 200 ));
				
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
								downloadButtonText = "Download '" + song.name + "'";
								
								currentDownloadPercentage = "";
								currentDownloadSize = "Fetching";
									
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
									
									if ( downloadArtwork == true )
									{
					
										if ( downloadingSong.largeArtworkURL != null )
										{
						
											UnityEngine.Debug.Log ( "DownloadArtwork ()" );
											StartCoroutine ( "DownloadArtwork", song );
										}
									}
									
									if ( startupManager.developmentMode == true )
										UnityEngine.Debug.Log ( url );
									
									downloadingSong = song;
									
									currentDownloadPercentage = " - Processing Download";
									
									try
									{
										
										using ( client = new WebClient ())
										{
						 
							        		client.DownloadFileCompleted += new AsyncCompletedEventHandler ( DownloadFileCompleted );
							
							        		client.DownloadProgressChanged += new DownloadProgressChangedEventHandler( DownloadProgressCallback );
											
							        		client.DownloadFileAsync ( url, startupManager.tempPath + song.name + "." + song.format );
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
							if ( String.IsNullOrEmpty ( song.largeArtworkURL ) == false )
								downloadArtwork = GUILayout.Toggle ( downloadArtwork, "Download Artwork" );

							if ( song.links != null )
							{
								
								GUILayout.Label ( "", infoLabelStyle );
							
								GUILayout.Label ( "Support " + song.artist + " by visiting the following link(s)", labelStyle );
								
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
					
					if ( GUILayout.Button ( featured.artwork ))
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
							downloadButtonText = "Download '" + featured.song.name + "'";
							
							currentDownloadPercentage = "";
							currentDownloadSize = "Fetching";
								
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
			if ( GUI.Button ( new Rect ( onlineMusicBrowserPosition.width / 2 - 160, onlineMusicBrowserPosition.height / 2, 320, 64 ), "Enable OnlineMusicBrowser" ))
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
				File.Delete ( startupManager.tempPath + downloadingSong.name + "." + downloadingSong.format );
			} else {
				
				if ( File.Exists ( startupManager.downloadedPath + downloadingSong.name + "." + downloadingSong.format ))
					File.Delete ( startupManager.downloadedPath + downloadingSong.name + "." + downloadingSong.format );
				
				File.Move ( startupManager.tempPath + downloadingSong.name + "." + downloadingSong.format, startupManager.downloadedPath + downloadingSong.name + "." + downloadingSong.format );
			}

			songInfoOwner = null;
			currentDownloadPercentage = "";
			showSongInformation = false;
			downloading = false;
		}
	}	
	
	
	void DownloadProgressCallback ( object sender, DownloadProgressChangedEventArgs arg )
	{
	
		currentDownloadPercentage = " '" + downloadingSong.name + "' - " + arg.ProgressPercentage.ToString () + "% Complete";
	}
	
	
	IEnumerator DownloadArtwork ( Song downloadedSong )
	{
		
		if ( String.IsNullOrEmpty ( downloadedSong.largeArtworkURL ) == false )
		{
			
			UnityEngine.Debug.Log ( "Downloading Artwork" );
			
			byte[] bytes;
			Texture2D artwork;
			
			WWW artworkWWW = new WWW ( downloadedSong.largeArtworkURL );
			yield return artworkWWW;
			
			
			artwork = artworkWWW.texture;
			bytes = artwork.EncodeToPNG ();
			
			using ( FileStream writeArtwork = File.Create ( startupManager.slideshowPath + Path.DirectorySeparatorChar + downloadedSong.album + "Artwork.png" ))
			{
					
				writeArtwork.Write ( bytes, 0, bytes.Length );
			}
			
			UnityEngine.Debug.Log ( "Artwork Downloaded" );
		}
	}
}
