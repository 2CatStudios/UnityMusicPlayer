using System;
using System.Net;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
//Written by GibsonBethke
//Thank you for everything, Jesus, you are so awesome!

public class Song
{
	
	public String name;

	public Album album;
	public Artist artist;
	public Genre genre;

	public String format;
	public String downloadLink;
	public String supportLink;
	
	public String releaseDate;
}

public class Album : IEquatable<Album>
{
	
	public String name;
	public List<Song> songs;
	
	public Album (String Name, List<Song> Songs)
	{
		
		this.name = Name;
		this.songs = Songs;
	}
	
	public bool Equals(Album other)
	{
		if (other == null) return false;
		return (this.name.Equals(other.name));
	}
}

public class Artist : IEquatable<Artist>
{
	
	public String name;
	public List<Song> songs;

	public Artist (String Name, List<Song> Songs)
	{

		this.name = Name;
		this.songs = Songs;
	}

	public bool Equals(Artist other)
	{
		if (other == null) return false;
		return (this.name.Equals(other.name));
	}
}

public class Genre : IEquatable<Genre>
{

	public String name;
	public List<Song> songs;

	public Genre (String Name, List<Song> Songs)
	{
		
		this.name = Name;
		this.songs = Songs;
	}
	
	public bool Equals(Genre other)
	{
		if (other == null) return false;
		return (this.name.Equals(other.name));
	}	
}

public class OnlineMusicBrowser : MonoBehaviour
{

	#region Variables

	public GUISkin guiSkin;
	public Texture2D guiHover;
	internal bool showUnderlay = false;
	
	internal bool showOnlineMusicBrowser = false;

	StartupManager startupManager;
	PaneManager paneManager;
	LoadingImage loadingImage;
	DownloadManager downloadManager;

	Vector2 scrollPosition;
	internal Rect onlineMusicBrowserPosition = new Rect(0, 0, 800, 600);
	internal string onlineMusicBrowserTitle;
	
	internal bool showDownloadList = false;
	internal bool songInfoWindowOpen = false;

	string[] allSongs;
	List<Song> allRecentList = new List<Song>();
	List<Song> allSongsList = new List<Song>();
	List<Album> allAlbumsList = new List<Album>();
	List<Artist> allArtistsList = new List<Artist>();
	List<Genre> allGenresList = new List<Genre>();
	
	internal int sortBy = 4;
	List<Song> specificSort = new List<Song>();
	internal string currentPlace = "Recent";

	#endregion

	void Start ()
	{

		startupManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent<StartupManager>();

		downloadManager = GameObject.FindGameObjectWithTag ("DownloadManager").GetComponent<DownloadManager>();
		loadingImage = GameObject.FindGameObjectWithTag ( "LoadingImage" ).GetComponent<LoadingImage>();
		paneManager = GameObject.FindGameObjectWithTag ("Manager").GetComponent<PaneManager>();

		onlineMusicBrowserPosition.width = Screen.width;
		onlineMusicBrowserPosition.height = Screen.height;
		onlineMusicBrowserPosition.x = onlineMusicBrowserPosition.width + onlineMusicBrowserPosition.width / 4;
	}

	void StartOMB ()
	{
		
		allSongs = startupManager.allSongs;
		Thread refreshThread = new Thread (SortAvailableDownloads);
		refreshThread.Start();
	}
	
	void SortAvailableDownloads()
	{

		int i = 0;

		while (i < allSongs.Length)
		{
			
			i += 9;
			Song song = new Song();
			song.name = allSongs [i - 8];
			
			Album tempAlbum = new Album(allSongs [i - 7], new List<Song>());
			if(allAlbumsList.Contains (tempAlbum))
			{
				
				Album addToAlbum = allAlbumsList.Find(Album => Album.name == tempAlbum.name);
				addToAlbum.songs.Add (song);
				song.album = tempAlbum;
			} else {
				
				tempAlbum.songs.Add (song);
				allAlbumsList.Add (tempAlbum);
				song.album = tempAlbum;
			}
			
			Artist tempArtist = new Artist(allSongs [i - 6], new List<Song>());
			if(allArtistsList.Contains (tempArtist))
			{
				
				Artist addToArtist = allArtistsList.Find(Artist => Artist.name == tempArtist.name);
				addToArtist.songs.Add (song);
				song.artist = tempArtist;
			} else {
				
				tempArtist.songs.Add (song);
				allArtistsList.Add (tempArtist);
				song.artist = tempArtist;
			}
			
			Genre tempGenre = new Genre(allSongs [i - 5], new List<Song>());
			if(allGenresList.Contains (tempGenre))
			{
				
				Genre addToGenre = allGenresList.Find(Genre => Genre.name == tempGenre.name);
				addToGenre.songs.Add (song);
				song.genre = tempGenre;
			} else {
				
				tempGenre.songs.Add (song);
				allGenresList.Add (tempGenre);
				song.genre = tempGenre;
			}
			
			
			song.format = allSongs [i - 4];
			song.downloadLink = allSongs [i - 3];
			song.supportLink = allSongs [i - 2];
			song.releaseDate = allSongs [i - 1];
			
			allSongsList.Add ( song );
			allRecentList.Add ( song );
		}

		allSongsList.Sort (( a, b ) => a.name.CompareTo ( b.name ));
		allAlbumsList.Sort (( a, b ) => a.name.CompareTo ( b.name ));
		allArtistsList.Sort (( a, b ) => a.name.CompareTo ( b.name ));
		allGenresList.Sort (( a, b ) => a.name.CompareTo ( b.name ));
		allRecentList.Reverse ();

		paneManager.loading = false;
	}
	
	void OnGUI ()
	{
		
		if ( showOnlineMusicBrowser == true )
		{
			
			GUI.skin = guiSkin;

			if ( songInfoWindowOpen == true )
				GUI.skin.button.hover.background = null;
			else
				GUI.skin.button.hover.background = guiHover;
			
			if ( paneManager.loading == false )
				onlineMusicBrowserPosition = GUI.Window ( 1, onlineMusicBrowserPosition, OnlineMusicBrowserPane, onlineMusicBrowserTitle );
		}
	}

	void OnlineMusicBrowserPane (int wid)
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

		scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  onlineMusicBrowserPosition.height - ( onlineMusicBrowserPosition.height / 4 + 56 )));
		GUILayout.Box ( "Current Sort: " + currentPlace );
		
		switch (sortBy)
		{

			case 0:
			if ( allSongsList.Count != 0 )
			{

				foreach ( Song song in allSongsList )
				{
				
					if ( GUILayout.Button ( song.name ))
					{
					
						if ( songInfoWindowOpen == false )
						{

							loadingImage.showLoadingImages = true;
							loadingImage.InvokeRepeating ( "LoadingImages", 0.25F, 0.25F );
							paneManager.popupBlocking = true;

							downloadManager.song = song;

							if ( song.downloadLink.StartsWith ( "|" ) == true )
							{

								downloadManager.url = null;
								downloadManager.downloadButtonText = song.downloadLink.Substring ( 1 );
							} else if ( song.downloadLink.StartsWith ( "h" ) == true )
							{

								downloadManager.url = new Uri ( song.downloadLink );
								downloadManager.downloadButtonText = "Download";
							}
								
							downloadManager.SendMessage ( "GetInfo" );
							songInfoWindowOpen = true;
						}
					}
				}
			}
			break;

			case 1:
			foreach (Album album in allAlbumsList)
			{

				if (GUILayout.Button (album.name))
				{

					specificSort = album.songs;
					currentPlace = "Albums > " + album.name;
					sortBy = 5;
				}
			}
			break;

			case 2:
			foreach (Artist artist in allArtistsList)
			{
				
				if (GUILayout.Button (artist.name))
				{

					specificSort = artist.songs;
					currentPlace = "Artists > " + artist.name;
					sortBy = 5;
				}
			}
			break;

			case 3:
			foreach (Genre genre in allGenresList)
			{
				
				if (GUILayout.Button (genre.name))
				{

					specificSort = genre.songs;
					currentPlace = "Genres > " + genre.name;
					sortBy = 5;
				}
			}
			break;

		case 4:
			if(allRecentList.Count != 0)
			{
				
				foreach (Song song in allRecentList)
				{
					
					if (GUILayout.Button (song.name))
					{
						
						if(songInfoWindowOpen == false)
						{
							
							loadingImage.showLoadingImages = true;
							loadingImage.InvokeRepeating ("LoadingImages", 0.25F, 0.25F);
							paneManager.popupBlocking = true;
							
							downloadManager.song = song;
							
							if ( song.downloadLink.StartsWith ( "|" ) == true )
							{
								
								downloadManager.url = null;
								downloadManager.downloadButtonText = song.downloadLink.Substring ( 1 );
							} else if ( song.downloadLink.StartsWith ( "h" ) == true )
							{
								
								downloadManager.url = new Uri (song.downloadLink);
								downloadManager.downloadButtonText = "Download";
							}
							
							downloadManager.SendMessage ("GetInfo");
							songInfoWindowOpen = true;
						}
					}
				}
			}
			break;

			case 5:
			foreach(Song song in specificSort)
			{
				
				if(GUILayout.Button (song.name))
				{

					if(songInfoWindowOpen == false)
					{

						loadingImage.showLoadingImages = true;
						loadingImage.InvokeRepeating ("LoadingImages", 0.25F, 0.25F);
						paneManager.popupBlocking = true;

						downloadManager.song = song;
						if ( song.downloadLink.StartsWith ( "|" ) == true )
						{
							
							downloadManager.url = null;
							downloadManager.downloadButtonText = song.downloadLink.Substring ( 1 );
						} else if ( song.downloadLink.StartsWith ( "h" ) == true )
						{
							
							downloadManager.url = new Uri (song.downloadLink);
							downloadManager.downloadButtonText = "Download";
						}

						downloadManager.SendMessage ("GetInfo");
						songInfoWindowOpen = true;
					}
				}
			}
			break;

		default:
			break;

		}
		GUILayout.EndScrollView ();
		GUILayout.EndHorizontal ();

		if ( showUnderlay == true )
			GUI.DrawTexture ( new Rect ( 0, 0, onlineMusicBrowserPosition.width, onlineMusicBrowserPosition.height ), startupManager.underlay );
	}
}