using UnityEngine;
using System.Collections;
//Written by Jan H. Heemstra and M. Gibson Bethke
public class MusicMaker : MonoBehaviour
{

//	public GUISkin guiskin;

//	public int rows = 8;
//	public int columns = 6;

	internal Rect musicMakerPosition = new Rect (-1000, 0, 800, 600);

	void Start ()
	{

		musicMakerPosition.width = Screen.width;
		musicMakerPosition.height = Screen.height;
		musicMakerPosition.x = -musicMakerPosition.width + -musicMakerPosition.width / 4;
	}

	void OnGUI ()
	{

//		GUI.skin = guiskin;
		musicMakerPosition = GUI.Window (4, musicMakerPosition, MusicMakerPane, "MusicMaker");
	}

	void MusicMakerPane (int wid)
	{

/*		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();

		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
*/	}
}