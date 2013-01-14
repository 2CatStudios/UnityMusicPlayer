using System.IO;
using UnityEngine;
using System.Collections;
//Written by GibsonBethke
//Jesus, you are awesome!
//Thanks to Jan Heemstra for the idea
public class PaneManager : MonoBehaviour
{

	public MusicViewer musicViewer;
	public OnlineMusicViewer onlineMusicViewer;

	enum pane {musicViewer, onlineMusicViewer};
	pane currentPane = pane.musicViewer;

	bool moving = false;
	bool moveToOMV = false;
	bool moveToMV = false;
	
	float moveVelocity = 0.0F;

/*

	GUI.Window 0 is MusicViewer
	GUI.Window 1 is OnlineMusicViewer
	GUI.Window 2 is DownloadIndo
	GUI.Window 3 is UpdateAvailable

*/
	void Update()
	{

		if(Input.GetKey (KeyCode.RightArrow) && currentPane == pane.musicViewer && moving == false)
		{

			moving = true;
			moveToOMV = true;
			currentPane = pane.onlineMusicViewer;
			onlineMusicViewer.SendMessage ("Refresh");
		}

		if(Input.GetKey (KeyCode.LeftArrow) && currentPane == pane.onlineMusicViewer && moving == false)
		{
			
			moving = true;
			moveToMV = true;
			currentPane = pane.musicViewer;
		}

		if(moveToOMV == true)
		{

			float smoothDampIn = Mathf.SmoothDamp(onlineMusicViewer.onlineMusicViewerPosition.x, 0.0F, ref moveVelocity, 0.1F, 4000);
			float smoothDampOut = Mathf.SmoothDamp(musicViewer.musicViewerPosition.x, -1000, ref moveVelocity, 0.1F, 4000);

			onlineMusicViewer.onlineMusicViewerPosition.x = smoothDampIn;
			musicViewer.musicViewerPosition.x = smoothDampOut;

			if(onlineMusicViewer.onlineMusicViewerPosition.x < 5)
			{

				moveVelocity = 0;
				moveToOMV = false;

				onlineMusicViewer.onlineMusicViewerPosition.x = 0;
				musicViewer.musicViewerPosition.x = -1000;

				moving = false;
			}
		}

		if(moveToMV == true)
		{

			float smoothDampIn = Mathf.SmoothDamp(musicViewer.musicViewerPosition.x, 0.0F, ref moveVelocity, 0.1F, 4000);
			float smoothDampOut = Mathf.SmoothDamp(onlineMusicViewer.onlineMusicViewerPosition.x, 1000, ref moveVelocity, 0.1F, 4000);

			onlineMusicViewer.onlineMusicViewerPosition.x = smoothDampOut;
			musicViewer.musicViewerPosition.x = smoothDampIn;

			if(musicViewer.musicViewerPosition.x > -5)
			{

				moveVelocity = 0;
				moveToMV = false;

				onlineMusicViewer.onlineMusicViewerPosition.x = 1000;
				musicViewer.musicViewerPosition.x = 0;

				moving = false;
			}
		}
	}
}