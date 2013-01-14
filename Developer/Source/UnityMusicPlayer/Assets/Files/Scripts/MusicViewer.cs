using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
//Written by GibsonBethke
//Thanks for getting me through this, Jesus!
public class MusicViewer : MonoBehaviour
{

	#region Variables

	StartupManager startupManager;
	
	bool updateOnce = false;

//-------

	bool isPaused;	
	float pausePoint;
	int songTime;
	int minutes;
	int seconds;
	int rtMinutes;
	int rtSeconds;
	string extraZero;
	string rtExtraZero;
	
	//Timebar visual
	public GUIText timemark;
	float timebarTime;

	
	//Songlist
	internal string [] clipList;
	int i;
	
	//Songlist visual
	Vector2 scrollPosition;

//-------
	
	//Current song
	public string songLocation;
	int currentSongNumber = -1;
	
	//Current song visual
	string rawCurrentSong;
	public GUIText currentSong;

//-------
	
	public bool debugMode = false;
	
	//GUI
	bool hideGUI = false;
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

//-------

	string mediaPath;
	string prefsLocation;
	GameObject manager;

	internal Rect musicViewerPosition = new Rect (0, 0, 800, 600);

	bool showVisualizer = false;
	bool halfSpeed = false;
	bool doubleSpeed = false;

	#endregion
	
	void Start ()
	{

		manager = GameObject.FindGameObjectWithTag ("Manager");
		startupManager = manager.GetComponent<StartupManager>();
		mediaPath = startupManager.publicMediaPath;
		prefsLocation = startupManager.prefsLocation;

		clipList = Directory.GetFiles(mediaPath, "*.wav");

		string[] prefs = File.ReadAllLines(prefsLocation);
		
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

		InvokeRepeating ("Refresh", 0, 2);
	}

	void Refresh ()
	{

		if(clipList != Directory.GetFiles(mediaPath, "*.wav"))
			clipList = Directory.GetFiles(mediaPath, "*.wav");
	}

	void OnGUI ()
	{

		GUI.skin = GuiSkin;

		musicViewerPosition = GUI.Window (0, musicViewerPosition, MusicViewerPane, "");

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
	}

	void MusicViewerPane (int wid)
	{

		GUILayout.BeginHorizontal ();
		GUILayout.Space (100);
		GUILayout.BeginVertical ();
		GUILayout.Space (175);
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(600), GUILayout.Height(390));

		if(clipList != null && hideGUI == false)
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

		if(GUILayout.Button ("Add Songs"))
			OpenFolder ();
		}
		
		GUI.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

#region NextButton

		bool pressOkay = true;
		if(GUI.Button (new Rect(Screen.width/2 - 70, Screen.height/4 - 7, 55, 30), "Next") || Input.GetKeyDown (KeyCode.DownArrow) && pressOkay == true)
		{
			
			if(psPlace != 6)
			{
			
				psPlace += 1;
				currentSongNumber = previousSongs[psPlace];

				//Tell the player to start streaming the song
				StartCoroutine ("PlayAudio");
			} else {
			
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
					if(currentSongNumber >= clipList.Length)
						currentSongNumber = 0;
				
					//Tell the player to start streaming the song
					StartCoroutine ("PlayAudio");
				}
			}

			pressOkay = false;
		}
		
#endregion
		
#region BackButton

		bool pressOkay2 = true;
		if(GUI.Button (new Rect(Screen.width/2 - 125, Screen.height/4 - 7, 55, 30), "Back") || Input.GetKeyUp (KeyCode.UpArrow) && pressOkay2 == true)
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
				//currentSongNumber = clipList[previousSongs[psPlace]];
			
				//Tell the player to start streaming the song
				StartCoroutine ("PlayAudio");
			}

			pressOkay2 = false;
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

		hideGUI = GUI.Toggle (new Rect (240, 580, 80, 20), hideGUI, "Hide Songs");
		showVisualizer = GUI.Toggle (new Rect (320, 580, 100, 20), showVisualizer, "AudioVisualizer");
		
		if(doubleSpeed = GUI.Toggle (new Rect (420, 580, 95, 20), doubleSpeed, "Double Speed"))
		{
			
			manager.audio.pitch = 2.0F;
			
			if(doubleSpeed == true && halfSpeed == true)
				halfSpeed = false;
		}
		
		if(halfSpeed = GUI.Toggle (new Rect (515, 580, 90, 20), halfSpeed, "Half Speed"))
		{
			
			manager.audio.pitch = 0.5F;
			
			if(halfSpeed == true && doubleSpeed == true)
				doubleSpeed = false;
		}
		
		if(halfSpeed == false && doubleSpeed == false)
			manager.audio.pitch = 1.0F;
		
		if (showVisualizer == true)
		{
			
			GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizationL> ().showAV = true;
			GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizationR> ().showAV = true;
		} else {
			
			GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizationL> ().showAV = false;	
			GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizationR> ().showAV = false;
		}
	}
	
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
		yield return www;
		
		manager.audio.clip = www.audioClip;
		
		seconds =  Mathf.RoundToInt (manager.audio.clip.length);
		
		//If the length of the song is more than 60 seconds, format it to look a bit more standard
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
		if(manager.audio.clip.isReadyToPlay)
		{
			
			//Reset the timebar
			rtMinutes = 00;
			rtSeconds = 00;
			
			//Tell the AudioSource to play the audio
			manager.audio.Play();
			
			updateOnce = false;
			
			//Set the boolean "isPaused" is false
			isPaused = false;
			
			//If debugMode is on, tell us the song should be playing
			if(debugMode == true)
				UnityEngine.Debug.Log ("Playing audio");
			
		}

		//If there is an issue getting the location of the song, tell us in the DebugLog
		if(www.error != null)
			UnityEngine.Debug.Log(www.error);
	}
	
	//Things that must be done every frame
	void Update ()
	{
		
		//Check if the user has pressed the spacebar
		if (Input.GetKeyUp(KeyCode.Space))			
		{
			
			//If the player is playing a song & the user pressed spacebar
			if(isPaused == false)
			{
				
				//Pause the audio
				manager.audio.Pause();
				
				//Remember where we last were in the song
				pausePoint = manager.audio.time;
				
				//Set the boolean "isPaused" to true
				isPaused = true;
				
				//If debugMode is on, display the point at which the song was paused
				if(debugMode == true)
					UnityEngine.Debug.Log ("Pausing audio at " + pausePoint);
				
				//If the player is not playing a song, but the user did press the spacebar
			} else {
				
				//Tell the AudioSource to play the audio
				manager.audio.Play();

				//Remind the AudioSource where we last were in the song
				manager.audio.time = pausePoint;
				
				//Set the boolean "isPaused" to false
				isPaused = false;
				
				//If debugMode is on, display the point at which the song was resumed
				if(debugMode == true)
					UnityEngine.Debug.Log ("Resuming audio at " + pausePoint);
			}
		}
		
		//Set the volume of AudioSource to the current value of the float "volumeBarValue"
		manager.audio.volume = volumeBarValue;
		
		//If the audio is playing during this frame
		if(manager.audio.isPlaying == true)
		{
			
			//Set the int "rtSeconds" to the current place in the song, rounded to the nearest .10
			rtSeconds = Mathf.RoundToInt(manager.audio.time);
			
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
				manager.audio.Play ();
				
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
}