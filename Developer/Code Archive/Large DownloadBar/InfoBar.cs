using UnityEngine;
using System.Collections;
//Written by Gibson Bethke
public class InfoBar : MonoBehaviour
{
	
	public int songCount;
	string currentVersion;
	public Texture2D logo;
	Rect windowSaS = new Rect(250, 580, 300, 300);
	public bool hideSongs = false;
	bool showVisualizer = false;

	void Start ()
	{

		currentVersion = GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager>().currentVersion.ToString ();
	}

    void OnGUI()
	{
		
        windowSaS = GUI.Window(0, windowSaS, Window, "Help & Information");
		windowSaS.x = Mathf.Clamp(windowSaS.x, Screen.width/2 - 150, Screen.width/2 - 150);
	 	windowSaS.y = Mathf.Clamp(windowSaS.y, Screen.height-windowSaS.height + 10, Screen.height-20);
    }
	
    void Window (int windowID)
	{

		GUI.Label (new Rect (80, 17, 128, 128), logo);
		GUI.Label (new Rect (100, 55, 128, 50), "Version: " + currentVersion);
		GUI.Label (new Rect (10, 75, 250, 50), "2Cat Studios " + "\u00a9" + " 080212\nGibson Bethke - Code & Development\nJan Heemstra - 2DTextures & Support");
		GUI.Label (new Rect (10, 125, 250, 80), "Only Waveform Audio (.wav) will play\nbecause of mp3 licensing fees");
		GUI.Label (new Rect (10, 160, 260, 80), songCount + " songs");
		
		hideSongs = GUI.Toggle (new Rect (10, 190, 100, 20), hideSongs, "Hide Songs");
		if (hideSongs == true)
			GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager> ().showGUI = false;
		else
			GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager> ().showGUI = true;

		
		showVisualizer = GUI.Toggle (new Rect (110, 190, 150, 20), showVisualizer, "Show AudioVisualizer");
		if (showVisualizer == true)
		{

			GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizationL> ().showAV = true;
			GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizationR> ().showAV = true;
		} else {
		
			GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizationL> ().showAV = false;	
			GameObject.FindGameObjectWithTag ("AudioVisualizer").GetComponent<AudioVisualizationR> ().showAV = false;
		}
		
		if (GUI.Button (new Rect (10, 210, 85, 22), "Open Folder"))
			GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager> ().SendMessage ("OpenFolder");

		if (GUI.Button (new Rect (110, 210, 70, 22), "Download Songs"))
		{

			GameObject.FindGameObjectWithTag ("Manager").GetComponent<Manager> ().showGUI = false;
			hideSongs = true;
			GameObject.FindGameObjectWithTag ("WebManager").GetComponent<WebManager>().showDownloadList = true;
			windowSaS.y = 580;
			GUI.FocusWindow (1);
		}
		GUI.Label(new Rect(10, 235, 260, 120), "To contact us about ideas/comments/concerns\nplease email us at gibsonbethke@gmail.com");

		GUI.DragWindow ();
	}
}