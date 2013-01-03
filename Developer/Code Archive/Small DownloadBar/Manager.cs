using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Diagnostics;

//Written By Gibson Bethke
//Thanks to Jesus my savior for all your unconditional blessings!
//Thanks to Jan Heemstra for all your mental assistance
//Thanks to the UnityCommunity for help getting me off the ground

public class Manager : MonoBehaviour
{
	
	#region Variables

	public float currentVersion;
	public string appLocation;

	bool updateOnce = false;

	#region Timebar

	bool isPaused;	
	float pausePoint;
	int songTime;
	int minutes;
	int seconds;
	int rtMinutes;
	int rtSeconds;
	String extraZero;
	String rtExtraZero;

	//Timebar visual
	public GUIText timemark;
	float timebarTime;
	
	#endregion
	
	#region SongList
	
	//Songlist
	internal string [] clipList;
	int i;
	
	//Songlist visual
	Vector2 scrollPosition;
	
	#endregion
	
	#region Directory
	
	//Predefined path to UMP files for Mac and Windows
	static string mac = "/Users/" + Environment.UserName + "/Library/Application Support/2Cat Studios/UnityMusicPlayer";
	static string windows = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\2Cat Studios\\UnityMusicPlayer";

	internal bool onMac;
	
	string path;
	public string publicPath;
	static string mediaPath;

	bool filesExist;
	
	#endregion
	
	#region CurrentSong
	
	//Current song
	public string songLocation;
	int currentSongNumber = 0;
	
	//Current song visual
	string rawCurrentSong;
	public GUIText currentSong;
	
	#endregion
		
	#region Warning
	
	//Warning visual
	public GUIText filesExistText;
	public GUITexture filesExistImage;
	public Texture x;
	public Texture tick;
	
	#endregion
	
	#region Infobar
	
	//Infobar Object
	InfoBar infoBar;
	
	#endregion
	
	#region Controls
	
	//Toggles the DebugLog Messages (to improve performance in editor)
	public bool debugMode = false;
	
	//GUI
	public bool showGUI;
	public GUISkin GuiSkin;
	
	//List of seven previous songs
	int[] previousSongs = new int[7] {0, 0, 0, 0, 0, 0, 0};
	int psPlace = 6;
	
	bool loop = false;
	bool shuffle = false;
	bool continuous = false;
	
	//Between song delay
	public float betweenSongDelay = 1.0f;
	
	//Volume bar
	float volumeBarValue = 1.0F;
	
	#endregion
	
	#region Prefs
	
	//Preferences
	static string prefsLocation;
	
	#endregion

	#endregion
	
	#region Functions
	
	//Prepare player to run\\
	void Start ()
	{    
				
		//If debugMode is on at start, tell us (incase we're looking at performance, and forgot about debugMode
		if(debugMode == true)
			UnityEngine.Debug.Log ("Debug Mode is on");
		
		//Check if the first part of the OS name is "Unix" (Mac OS is a Unix-based system)
		if(Environment.OSVersion.ToString().Substring (0, 4) == "Unix")
		{
			
			//Set path to the predefined location for UMP music files on Macintosh-based OS
			path = mac;

			onMac = true;
			
			//If debugMode is on, tell us that the user is running a Macintosh-based Operating System
			if(debugMode == true)
				UnityEngine.Debug.Log ("Player Running on Macintosh OS");
		} else {
			
			//Set path to the predefined location for UMP music files on a Windows-based OS
			path = windows;

			onMac = false;

			//If debugMode is on, tell us that the user is running a Windows-based Operating System
			if(debugMode == true)
				UnityEngine.Debug.Log ("Player Running on Windows OS");
		}

		publicPath = path;
		
		//Find & assign infoBar and audioVisualizer
		infoBar = GameObject.FindGameObjectWithTag("InfoBar").GetComponent<InfoBar>();
				
		//Assign mediaPath to the correct directory
		mediaPath = path + Path.DirectorySeparatorChar + "Media";

		//Assign prefsLocation to the correct directory
		prefsLocation = path + Path.DirectorySeparatorChar + "Preferences.ump";

		//Get the current directory (so we know where to save the new version if an update is avaliable)
		appLocation = Environment.CurrentDirectory;
		
		//Check if the directories exist
		//NOTE: We only need to check mediaPath because if mediaPath exists, so does path
		if(!Directory.Exists (mediaPath))
		{
			
			//If debugMode is on, tell us that the directories do not exist
			if(debugMode == true)
				UnityEngine.Debug.Log ("Directories Do Not Exist");
			
			//Show the X image to alert the user that something is wrong
			filesExistImage.texture = x;
			
			//Tell the user that they need to create the directories for UMP to work
			filesExistText.text = "The requried directories do not exist!\nTo create them, click 'Create Directories'";	
			
			//Set the boolean filesExist to false and the array clipList to null
			filesExist = false;
			clipList = null;
		} else {
		
		//Set the boolean filesExist to true
		filesExist = true;

		//Get the all the files ending in .WAV in path & assign them to the array clipList
		clipList = Directory.GetFiles(mediaPath, "*.wav");
		
		//Get the number of songs currently playiable and send that to infoBar
		infoBar.songCount = clipList.Length;	
			
		//If debugMode is on, display how many songs are playable
		if(debugMode == true)
			UnityEngine.Debug.Log (clipList.Length + " songs are playable");
		}

		//If the Preferences file exists set the preferences accordingly
		if(File.Exists (prefsLocation))
		{

			//Bring all the text from our file into memory
			string[] prefs = File.ReadAllLines(prefsLocation);

			//Set loop, shuffle, and the volume to what it was last
			loop = Convert.ToBoolean(prefs[0]);
			shuffle = Convert.ToBoolean(prefs[1]);
			continuous = Convert.ToBoolean(prefs[2]);
			volumeBarValue = Convert.ToSingle(prefs[3]);
			previousSongs[0] = Convert.ToInt32(prefs[4]);
			if(previousSongs[0] > clipList.Length)
				previousSongs[0] = clipList.Length;

			previousSongs[1] = Convert.ToInt32(prefs[5]);
			if(previousSongs[1] > clipList.Length)
				previousSongs[1] = clipList.Length;

			previousSongs[2] = Convert.ToInt32(prefs[6]);
			if(previousSongs[2] > clipList.Length)
				previousSongs[2] = clipList.Length;

			previousSongs[3] = Convert.ToInt32(prefs[7]);
			if(previousSongs[3] > clipList.Length)
				previousSongs[3] = clipList.Length;

			previousSongs[4] = Convert.ToInt32(prefs[8]);
			if(previousSongs[4] > clipList.Length)
				previousSongs[4] = clipList.Length;

			previousSongs[5] = Convert.ToInt32(prefs[9]);
			if(previousSongs[5] > clipList.Length)
				previousSongs[5] = clipList.Length;

			previousSongs[6] = Convert.ToInt32(prefs[10]);
			if(previousSongs[6] > clipList.Length)
				previousSongs[6] = clipList.Length;

			TextWriter savePrefs = new StreamWriter(prefsLocation);
			savePrefs.WriteLine(loop + "\n" + shuffle + "\n" + continuous + "\n" + volumeBarValue + "\n" + previousSongs[0] + "\n" + previousSongs[1] + "\n" + previousSongs[2] + "\n" + previousSongs[3] + "\n" + previousSongs[4] + "\n" + previousSongs[5] + "\n" + previousSongs[6]);
			savePrefs.Close();		

		//If the Preferences file does not exist, create it
		} else if(filesExist == true){

			//Create the file
			using (FileStream createPrefs = File.Create(prefsLocation))
            {
				
				//Make a byte array and fill it with the values for loop, shuffle, volume, and empty values for the previous sogns.
				Byte[] preferences = new UTF8Encoding(true).GetBytes(loop + "\n" + shuffle + "\n" + continuous + "\n" + volumeBarValue + "\n 0 \n 0 \n 0 \n 0 \n 0 \n 0 \n 0");

				//Write to our new file
                createPrefs.Write(preferences, 0, preferences.Length);

				createPrefs.Close();
            }
		}
		InvokeRepeating("CheckMediaPath", 0, 2);
	}

	void CheckMediaPath ()
	{

		if(clipList != Directory.GetFiles(mediaPath, "*.wav"))
		{

			clipList = Directory.GetFiles(mediaPath, "*.wav");
			infoBar.songCount = clipList.Length;
		}
	}
	
	//Handles GUI relating to playing audio\\
	void OnGUI ()
	{
				
		//Set the GUI skin
		GUI.skin = GuiSkin;

		//Create and center the song list
        GUILayout.BeginHorizontal ();
		GUILayout.Space (Screen.width/2 - 300);
		GUILayout.BeginVertical ();
		GUILayout.Space (Screen.height/2 - 125);
		
		//Create a scrollable list for the songs
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(600), GUILayout.Height(400));
		
		//Check if the directories exist
		if (filesExist == false)
		{
			
			//If they don't, make a button that allows the user to create them
			if (GUILayout.Button ("Create Directories"))
			{
				
				//If debugMode is on, tell us that we're creating the directories
				if(debugMode == true)
					UnityEngine.Debug.Log ("Creating Directories...");
				
				//Create the directory "path"
	          	Directory.CreateDirectory(path);
				
				//Create the directory "mediaPath"
	          	Directory.CreateDirectory(mediaPath);
				
				//Set the boolean "filesExist" to true
	          	filesExist = true;
				
				//Display a nice image to reassure the user everything is okay
				filesExistImage.texture = tick;
				
				//Inform the user that the directories have been created and that they must restart the player for their music to show up
				filesExistText.text = "The directories have been created successfully!\nPlace .WAV audio files in created directory and restart player\n\nThe spacebar will pause/resume audio\n'Help & Information' is a draggable window";
				
				//If debugMode is on, tell us that the Directories have been created
				if(debugMode == true)
					UnityEngine.Debug.Log ("Directories Created");

				//Create the file
				using (FileStream createPrefs = File.Create(prefsLocation))
				{
					
					//Make a byte array and fill it with the values for loop, shuffle, volume, and empty values for the previous sogns.
					Byte[] preferences = new UTF8Encoding(true).GetBytes(loop + "\n" + shuffle + "\n" + continuous + "\n" + volumeBarValue + "\n 0 \n 0 \n 0 \n 0 \n 0 \n 0 \n 0");
					
					//Write to our new file
					createPrefs.Write(preferences, 0, preferences.Length);
					
					createPrefs.Close();
				}
				
				//Call the function "OpenFolder"
				OpenFolder();
			}
			
		//If they do, repeat this step for every song in "mediaPath"
		} else if(clipList != null && showGUI == true)
		{
		
				for (i = 0; i < clipList.Length; i ++ )
				{
			
					//Give the button we're about to create the name of a song in the directory
					string clipToPlay = clipList[i].Substring(mediaPath.Length + 1);
			
					//Create a button in the scrollable list we made earlier
					if(GUILayout.Button (clipToPlay.Substring(0, clipToPlay.Length -4)))
					{
						
						//Set the int currentSongNumber to the current value of i
						currentSongNumber = i;
					
						//Shift everything in the array previousSongs back one
						previousSongs[0] = previousSongs[1];
						previousSongs[1] = previousSongs[2];
						previousSongs[2] = previousSongs[3];
						previousSongs[3] = previousSongs[4];
						previousSongs[4] = previousSongs[5];
						previousSongs[5] = previousSongs[6];
						previousSongs[6] = currentSongNumber;
						psPlace = 6;
					
					    //Clear all songs from memory
						Resources.UnloadUnusedAssets();
								
						//Play it, Sam
						StartCoroutine("PlayAudio");
					}
				} 
			}
		
		//Check if there are any songs in the folder
		if(filesExist == true)
			//If there arn't, display a button with the text "Add Songs"
			if(GUILayout.Button ("Add Songs"))
				//If the button is pressed, call the function "OpenFolder"
				OpenFolder ();
		
		//Tell the GUI to shut down (this is used internally and only when shutting down the application
		GUI.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		
		#region Controls
		
		#region NextButton
		
		if(GUI.Button (new Rect(Screen.width/2 - 70, Screen.height/4 - 7, 55, 30), "Next"))
		{

			if(psPlace != 6)
			{

				psPlace += 1;
				currentSongNumber = previousSongs[psPlace];

				//Tell the player to start streaming the song
				StartCoroutine ("PlayAudio");
			} else {

				if(audio.isPlaying == true)
				{
							
					if(shuffle == true)
					{

						//Clear the last song from memory
						Resources.UnloadUnusedAssets();
						
						//Get a random number between 0 and the total songs
						currentSongNumber = UnityEngine.Random.Range (0, clipList.Length);
						
						previousSongs[0] = previousSongs[1];
						previousSongs[1] = previousSongs[2];
						previousSongs[2] = previousSongs[3];
						previousSongs[3] = previousSongs[4];
						previousSongs[4] = previousSongs[5];
						previousSongs[5] = previousSongs[6];
						previousSongs[6] = currentSongNumber;
				
						//Set the song to our random number
//						currentSongNumber = clipList[currentSongNumber];
				
						//Tell the player to start streaming the song
						StartCoroutine ("PlayAudio");
				
					} else {

						//Clear the last song from memory
						Resources.UnloadUnusedAssets();
						
						//Add one to i, selecting the next song
						currentSongNumber += 1;
						
						previousSongs[0] = previousSongs[1];
						previousSongs[1] = previousSongs[2];
						previousSongs[2] = previousSongs[3];
						previousSongs[3] = previousSongs[4];
						previousSongs[4] = previousSongs[5];
						previousSongs[5] = previousSongs[6];
						previousSongs[6] = currentSongNumber;
					
						//If there are no more songs in the playlist, start it over
						if(i > clipList.Length)
						i = 0;
					
						//Set the song to our next song
//						currentSongNumber = clipList[currentSongNumber];
				
						//Tell the player to start streaming the song
						StartCoroutine ("PlayAudio");
					}
				} else {
			
					//Clear the last song from memory
					Resources.UnloadUnusedAssets();
			
					//Set i to 0, starting the music from the top of the playlist
					i = 0;
					currentSongNumber = 0;
				
					//Tell the player to start streaming the song
					StartCoroutine ("PlayAudio");
				}
			}
		}
		
		#endregion
		
		#region BackButton
		
		if(GUI.Button (new Rect(Screen.width/2 - 125, Screen.height/4 - 7, 55, 30), "Back"))
		{

			//Clear the last song from memory
			Resources.UnloadUnusedAssets();
			
			if(psPlace < 0)

				currentSongNumber = UnityEngine.Random.Range (0, clipList.Length);
			else
			{

				//Select the previous song
				psPlace -= 1;

				//Set the song to our next song
				currentSongNumber = previousSongs[psPlace];
//				currentSongNumber = clipList[previousSongs[psPlace]];
				
				//Tell the player to start streaming the song
				StartCoroutine ("PlayAudio");
			}
		}
		
		#endregion
		
		#region VolumeBar
		
		//Make a label for the volume bar that says "Volume"
		GUI.Label(new Rect(Screen.width/2 - 110, Screen.height/4 - 42, 100, 25), "Volume");
		
		//Create a volume bar, and give the value of it to the float "volumeBarValue"
		volumeBarValue = GUI.HorizontalSlider(new Rect(Screen.width/2 - 115, Screen.height/4 - 20, 100, 30), volumeBarValue, 0.0F, 1.0F);
		
		#endregion
		
		#region LoopButton
		
		//Create a lable for the above toggle switch
		GUI.Label(new Rect(Screen.width/2 + 10, Screen.height/4 - 42, 120, 30), "Loop");
		
		//Create a toggle switch that will pass it's value to the boolean "loop"		
		if(loop = GUI.Toggle(new Rect(Screen.width/2 - 5, Screen.height/4 - 35, 100, 20), loop, ""))
			//If both Loop and Shuffle are toggled, turn Shuffle off
			if(loop == true && shuffle == true || loop == true && continuous == true)
			{

				shuffle = false;
				continuous = false;
			}

		#endregion
		
		#region ShuffleButton

		//Create a lable for the toggle switch
		GUI.Label(new Rect(Screen.width/2 + 10, Screen.height/4 - 19, 120, 30), "Shuffle");
		
		//Create a toggle switch that will pass it's value to the boolean "shuffle"		
		if(shuffle = GUI.Toggle(new Rect(Screen.width/2 - 5, Screen.height/4 - 14, 100, 20), shuffle, ""))
			//If both Shuffle and Loop are toggled, turn Loop off
			if(shuffle == true && loop == true || shuffle == true && continuous == true)
			{

				loop = false;
				continuous = false;
			}
		
		#endregion

		#region ContinuousPlay

		//Create a lable for the toggle switch
		GUI.Label(new Rect(Screen.width/2 + 10, Screen.height/4 + 1, 120, 30), "Continuous");
		
		//Create a toggle switch that will pass it's value to the boolean "continuous"		
		if(continuous = GUI.Toggle(new Rect(Screen.width/2 - 5, Screen.height/4 + 6, 100, 20), continuous, ""))
			//If both Shuffle and Loop are toggled, turn Loop off
			if(continuous == true && shuffle == true || continuous == true && loop == true)
			{

				shuffle = false;
				loop = false;
			}
				

		#endregion
		
		#region QuitButton

		//Display a button for quitting the application
		if(GUI.Button(new Rect(Screen.width - 75, Screen.height - 50, 60, 30), "Quit"))
		{
			
			//Clear the memory
			Resources.UnloadUnusedAssets();
			
			TextWriter savePrefs = new StreamWriter(prefsLocation);

			savePrefs.WriteLine(loop + "\n" + shuffle + "\n" + continuous + "\n" + volumeBarValue + "\n" + previousSongs[0] + "\n" + previousSongs[1] + "\n" + previousSongs[2] + "\n" + previousSongs[3] + "\n" + previousSongs[4] + "\n" + previousSongs[5] + "\n" + previousSongs[6]);

            savePrefs.Close();
			
			//Quit the application if the button has been clicked
			Application.Quit();
			
			//If debugMode is on, tell us that quit has been called
			if(debugMode == true)
				UnityEngine.Debug.Log ("Quit has been called");
		}
		
		#endregion

		#endregion
	}

	//Get and play audio
	IEnumerator PlayAudio ()
	{

		//Get the name of the song to play, remove the path to it, and it's extension, and send that to the GUIText "currentSong"
		rawCurrentSong = clipList[currentSongNumber].Substring(mediaPath.Length + 1);
		currentSong.text = rawCurrentSong.Substring(0, rawCurrentSong.Length -4);

		songLocation = clipList[currentSongNumber];

		if(debugMode == true)
			UnityEngine.Debug.Log ("Preparing to play " + rawCurrentSong.Substring(0, rawCurrentSong.Length -4));
		
		//Get the location of the song and wait for it to load (incase it's streaming from the internet)
		WWW www = new WWW("file:" + songLocation);
//		WWW www = new WWW(url);
		yield return www;
			
		//Pass the location we just got to the AudioSource
		audio.clip = www.audioClip;
		
// *	For web streaming
// *	audio.clip = www.GetAudioClip(false, true, AudioType.WAV);
		
		//Set the int "seconds" to the length of the current song
		seconds =  Mathf.RoundToInt (audio.clip.length);
		
		//If the length of the song is more than 60 seconds, format it to look more standard
		if(seconds > 60)
		{
			
			//The int "minutes" equals the int "seconds" divided by 60
			minutes = seconds/60;
			
			//The int "seconds" subtracts the int "minutes" multiplied by sixty
			seconds -= minutes*60;
		} else
			
			//If the song is under 60 seconds, reset the minutes
			minutes = 0;
		
		if(seconds < 10)
			extraZero = "0";
		else
			extraZero = "";
		
		//Update the timebar
		timemark.text = (int) rtMinutes + ":" + rtSeconds + "][" + minutes + ":" + seconds;

		//Check if everything has worked up to this point, and we're ready to play
		if(audio.clip.isReadyToPlay)
		{
			
			//Reset the timebar
			rtMinutes = 00;
			rtSeconds = 00;
						
			//Tell the AudioSource to play the audio
			audio.Play();

			updateOnce = false;
			
			//Set the boolean "isPaused" is false
			isPaused = false;
			
			//If debugMode is on, tell us the song should be playing
			if(debugMode == true)
				UnityEngine.Debug.Log ("Playing audio");
			
		}
		
		StopCoroutine("PlayAudio");
		
		//If there is an issue getting the location of the song, tell us in the DebugLog
		if(www.error != null)
			UnityEngine.Debug.Log(www.error);
	}
			
	//Things that must be done every frame\\
	void Update ()
	{
				
		//Check if the user has pressed the spacebar
		if (Input.GetKeyDown(KeyCode.Space))			
		{
			
			//If the player is playing a song & the user pressed spacebar
			if(isPaused == false)
			{
			
				//Pause the audio
				audio.Pause();
				
				//Remember where we last were in the song
				pausePoint = audio.time;
				
				//Set the boolean "isPaused" to true
				isPaused = true;
				
				//If debugMode is on, display the point at which the song was paused
				if(debugMode == true)
					UnityEngine.Debug.Log ("Pausing audio at " + pausePoint);
				
			//If the player is not playing a song, but the user did press the spacebar
			} else {
				
				//Tell the AudioSource to play the audio
				audio.Play();
				
				//Remind the AudioSource where we last were in the song
				audio.time = pausePoint;
				
				//Set the boolean "isPaused" to false
				isPaused = false;
				
				//If debugMode is on, display the point at which the song was resumed
				if(debugMode == true)
					UnityEngine.Debug.Log ("Resuming audio at " + pausePoint);
			}
		}
		
		//Set the volume of AudioSource to the current value of the float "volumeBarValue"
		audio.volume = volumeBarValue;
		
		//If the audio is playing during this frame
		if(audio.isPlaying == true)
		{

			//Set the int "rtSeconds" to the current place in the song, rounded to the nearest .10
			rtSeconds = Mathf.RoundToInt(audio.time);
			
			//If we're past 60 seconds, reformat
			if(rtSeconds >= 60)
			{
				
				//Set the int "rtMinutes" to the int "rtSeconds" divided by 60
				rtMinutes = rtSeconds/60;
				
				//Subtract the int "rtMinutes" multiplied by 60 from the int "rtSeconds" 
				rtSeconds -= rtMinutes*60;
			}
			
			//If the current second is less than 10, add a zero (for formatting)
			if(rtSeconds < 10)
				rtExtraZero = "0";
			else
				rtExtraZero = "";
			
			//For every frame, set the timemark text to the current place in the song
			timemark.text = (int) rtMinutes + ":" + rtExtraZero + rtSeconds + "][" + minutes + ":" + extraZero + seconds;

			if(updateOnce == false)
			{

				//Check if the song is over
				if(rtSeconds == seconds && rtMinutes == minutes)
				{

					updateOnce = true;
					if(continuous == true || loop == false && shuffle == false)
						Invoke("SongEnd", 1);
					else
						SongEnd ();
				}
			}
		}
	}
	
	//Handles what we do once the song ends\\
	void SongEnd ()
	{
		
		//If debugMode is on, tell us how many AudioClips are in memory
		if(debugMode == true)
			UnityEngine.Debug.Log("AudioClips in Memory: " + Resources.FindObjectsOfTypeAll(typeof(AudioClip)).Length);

		psPlace = 6;

		if(continuous == true)
		{

			//Clear the last song from memory
			Resources.UnloadUnusedAssets();

			UnityEngine.Debug.Log(currentSongNumber);
			UnityEngine.Debug.Log(clipList.Length);

			if(currentSongNumber == clipList.Length - 1)
				currentSongNumber = 0;
			else
				currentSongNumber++;

			previousSongs[0] = previousSongs[1];
			previousSongs[1] = previousSongs[2];
			previousSongs[2] = previousSongs[3];
			previousSongs[3] = previousSongs[4];
			previousSongs[4] = previousSongs[5];
			previousSongs[5] = previousSongs[6];
			previousSongs[6] = currentSongNumber;

			//Tell the player to start streaming the song
			StartCoroutine ("PlayAudio");
		} else
		{
		
			//Check if the user wants the song to loop
			if(loop == true)
			{
		
				//Reset the Timebar
				rtMinutes = 0;
				rtSeconds = 00;
				timemark.text = (int) rtMinutes + ":" + rtSeconds + "][" + minutes + ":" + seconds;
			
				//Tell the AudioSource to play the same song
				audio.Play ();

				updateOnce = false;
			} else
			{
					
				//Check if the user wants the player to shuffle through their songs
				if(shuffle == true)
				{

					//Clear the last song from memory
					Resources.UnloadUnusedAssets();
				
					//Get a random number between 0 and the total songs
					currentSongNumber = UnityEngine.Random.Range (0, clipList.Length);

					previousSongs[0] = previousSongs[1];
					previousSongs[1] = previousSongs[2];
					previousSongs[2] = previousSongs[3];
					previousSongs[3] = previousSongs[4];
					previousSongs[4] = previousSongs[5];
					previousSongs[5] = previousSongs[6];
					previousSongs[6] = currentSongNumber;
					psPlace = 6;
				
					//Tell the player to start streaming the song
					StartCoroutine ("PlayAudio");
				} else
				{
				
					//If the user doesn't want any songs to play
				
					//Clear all songs from memory
					Resources.UnloadUnusedAssets();
				
					//Reset the Timebar
					rtMinutes = 0;
					rtSeconds = 00;
					minutes = 0;
					seconds = 00;

					timemark.text = "0:00][0:00";
					currentSong.text = "UnityMusicPlayer";
				}
			}
		}
	}

	//The function for opening the directory that the music is stored in for the user\\
	void OpenFolder ()
	{
		
		//Open the folder located at "mediaPath"
		Process.Start(mediaPath);
		
		//If debugMode is on, tell us the path to mediaPath has been opened in Finder
		if(debugMode == true)
			UnityEngine.Debug.Log ("Opening mediaPath in Finder");
	}

	#endregion
}