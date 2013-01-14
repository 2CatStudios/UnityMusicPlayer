using System;
using System.IO;
using UnityEngine;
using System.Text;
using System.Collections;
using System.Diagnostics;
//Written by GibsonBethke
//Thank you for your ∞ mercy, Jesus!
public class StartupManager : MonoBehaviour
{

	public float runningVersion;
	internal bool onMac;
	
	//Predefined path to UMP files for Mac and Windows
	static string mac = "/Users/" + Environment.UserName + "/Library/Application Support/2Cat Studios/UnityMusicPlayer";
	static string windows = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\2Cat Studios\\UnityMusicPlayer";

	string path;
	string mediaPath;
	internal string publicMediaPath;
	
	//Preferences
	internal string prefsLocation;

	void Start ()
	{    

//-------

		if(Environment.OSVersion.ToString().Substring (0, 4) == "Unix")
		{

			path = mac;
			onMac = true;
		} else
		{

			path = windows;
			onMac = false;
		}
		mediaPath = path + Path.DirectorySeparatorChar + "Media";
		publicMediaPath = mediaPath;

//-------

		prefsLocation = path + Path.DirectorySeparatorChar + "Preferences.ump";

//-------

		if(!Directory.Exists (mediaPath))
			Directory.CreateDirectory(mediaPath);

//-------

		if(!File.Exists (prefsLocation))
		{

			using (FileStream createPrefs = File.Create(prefsLocation))
			{
				
				Byte[] preferences = new UTF8Encoding(true).GetBytes("false\nfalse\nfalse\n1.0F\n0\n0\n0\n0\n0\n0\n0");	
				createPrefs.Write(preferences, 0, preferences.Length);
			}
		}
	}
}
