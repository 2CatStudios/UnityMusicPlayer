using UnityEngine;
using System.Collections;
//Written by Michael Bethke
//Thank you Jesus, for your forgiveness
public class LoadingImage : MonoBehaviour
{

	public Texture2D[] loadingImages = new Texture2D [ 6 ];
	internal bool showLoadingImages = false;
	int imageIndex = 0;
	

	void OnGUI ()
	{

		GUI.depth = 0;

		if ( showLoadingImages == true )
			GUI.DrawTexture ( new Rect ( Screen.width/2 - 128, Screen.height/2 + 64, 256, 128 ), loadingImages [ imageIndex ]);
	}


	void LoadingImages ()
	{

		if ( showLoadingImages == true )
		{
			
			if ( imageIndex >= loadingImages.Length -1 )
				imageIndex = 0;
			else
				imageIndex++;
		} else {
			imageIndex = 0;
			CancelInvoke ( "LoadingImages" );
		}
	}
}
