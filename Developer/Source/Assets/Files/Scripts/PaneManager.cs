using System.IO;
using UnityEngine;
using System.Collections;
//Written by GibsonBethke
//Jesus, you are awesome!
//Thanks to Jan Heemstra for the idea
public class PaneManager : MonoBehaviour
{

	public StartupManager startupManager;
	public MusicMaker musicMaker;
	public MusicViewer musicViewer;
	public OnlineMusicBrowser onlineMusicBrowser;

	internal bool popupBlocking = false;
	
	internal bool loading = true;
	bool startup = true;

	enum pane {musicMaker, musicViewer, onlineMusicBrowser};
	pane currentPane = pane.musicViewer;

	internal bool moving = false;
	internal bool moveToOMB = false;
	bool moveToMM = false;

	internal bool MMEnabled;
	bool moveToMVfOMB = false;
	bool moveToMVfMM = false;

	float moveVelocity = 0.0F;

/*

	GUI.Window 0 is MusicViewer
	GUI.Window 1 is OnlineMusicViewer
	GUI.Window 2 is DownloadInfo
	GUI.Window 3 is UpdateAvailable
	GUI.Window 4 is MusicMaker
	GUI.Window 5 is Streaming
	GUI.Window 6 is Settings

*/
	void Update()
	{
		if ( musicViewer.slideshow == false )
		{

			if ( popupBlocking == false && Input.GetKey (KeyCode.LeftArrow) && currentPane == pane.musicViewer && moving == false && startupManager.musicMakerEnabled == true )
			{
			
				moving = true;
				moveToMM = true;
			}

			if ( popupBlocking == false && Input.GetKey (KeyCode.RightArrow) && currentPane == pane.musicMaker && moving == false )
			{
			
				moving = true;
				moveToMVfMM = true;
			}

			if ( popupBlocking == false && Input.GetKey (KeyCode.LeftArrow) && currentPane == pane.onlineMusicBrowser && moving == false )
			{
			
				moving = true;
				moveToMVfOMB = true;
			}

			if ( popupBlocking == false && Input.GetKey (KeyCode.RightArrow) && currentPane == pane.musicViewer && moving == false && loading == false )
			{

				moving = true;
				moveToOMB = true;
			}
		}

		//Move to OnlineMusicBrowser from MusicViewer
		if ( moveToOMB == true )
		{

			float smoothDampIn = Mathf.SmoothDamp ( onlineMusicBrowser.onlineMusicBrowserPosition.x, 0.0F, ref moveVelocity, 0.1F, 4000 );
			float smoothDampOut = Mathf.SmoothDamp ( musicViewer.musicViewerPosition.x, -musicViewer.musicViewerPosition.width + -musicViewer.musicViewerPosition.width / 4, ref moveVelocity, 0.1F, 4000 );

			onlineMusicBrowser.onlineMusicBrowserPosition.x = smoothDampIn;
			musicViewer.musicViewerPosition.x = smoothDampOut;

			if ( onlineMusicBrowser.onlineMusicBrowserPosition.x < 5 )
			{
				
				moveVelocity = 0;
				moveToOMB = false;

				currentPane = pane.onlineMusicBrowser;

				onlineMusicBrowser.onlineMusicBrowserPosition.x = 0;
				musicViewer.musicViewerPosition.x = -onlineMusicBrowser.onlineMusicBrowserPosition.width + -onlineMusicBrowser.onlineMusicBrowserPosition.width / 4;

				moving = false;
			}
		}

		//Move to MusicViewer from OnlineMusicBrowser
		if ( moveToMVfOMB == true )
		{

			float smoothDampIn = Mathf.SmoothDamp ( musicViewer.musicViewerPosition.x, 0.0F, ref moveVelocity, 0.1F, 4000 );
			float smoothDampOut = Mathf.SmoothDamp ( onlineMusicBrowser.onlineMusicBrowserPosition.x, onlineMusicBrowser.onlineMusicBrowserPosition.width + onlineMusicBrowser.onlineMusicBrowserPosition.width / 4, ref moveVelocity, 0.1F, 4000 );

			musicViewer.musicViewerPosition.x = smoothDampIn;
			onlineMusicBrowser.onlineMusicBrowserPosition.x = smoothDampOut;

			if ( musicViewer.musicViewerPosition.x > -5 )
			{

				moveVelocity = 0;
				moveToMVfOMB = false;

				currentPane = pane.musicViewer;

				musicViewer.musicViewerPosition.x = 0;
				onlineMusicBrowser.onlineMusicBrowserPosition.x = onlineMusicBrowser.onlineMusicBrowserPosition.width + onlineMusicBrowser.onlineMusicBrowserPosition.width / 4;

				onlineMusicBrowser.sortBy = 0;
				onlineMusicBrowser.currentPlace = "Name";
				moving = false;
			}
		}

		//Move to MusicViewer from MusicMaker
		if ( moveToMVfMM == true )
		{
			
			float smoothDampIn = Mathf.SmoothDamp ( musicViewer.musicViewerPosition.x, 0.0F, ref moveVelocity, 0.1F, 4000 );
			float smoothDampOut = Mathf.SmoothDamp ( musicMaker.musicMakerPosition.x, -musicMaker.musicMakerPosition.width + -musicMaker.musicMakerPosition.width / 4, ref moveVelocity, 0.1F, 4000 );
			
			musicViewer.musicViewerPosition.x = smoothDampIn;
			musicMaker.musicMakerPosition.x = smoothDampOut;
			
			if ( musicViewer.musicViewerPosition.x < 5 )
			{
				
				moveVelocity = 0;
				moveToMVfMM = false;
				
				currentPane = pane.musicViewer;
				
				musicViewer.musicViewerPosition.x = 0;
				musicMaker.musicMakerPosition.x = -musicMaker.musicMakerPosition.width + -musicMaker.musicMakerPosition.width / 4;
				
				moving = false;
			}
		}

		//Move to MusicMaker from MusicViewer
		if ( moveToMM == true )
		{

			float smoothDampIn = Mathf.SmoothDamp ( musicMaker.musicMakerPosition.x, 0.0F, ref moveVelocity, 0.1F, 4000 );
			float smoothDampOut = Mathf.SmoothDamp ( musicViewer.musicViewerPosition.x, musicViewer.musicViewerPosition.width + musicViewer.musicViewerPosition.width / 4, ref moveVelocity, 0.1F, 4000 );

			musicViewer.musicViewerPosition.x = smoothDampOut;
			musicMaker.musicMakerPosition.x = smoothDampIn;
			
			if ( musicMaker.musicMakerPosition.x > -5 )
			{
				
				moveVelocity = 0;
				moveToMM = false;

				currentPane = pane.musicMaker;
				
				musicViewer.musicViewerPosition.x = musicViewer.musicViewerPosition.width + musicViewer.musicViewerPosition.width / 4;
				musicMaker.musicMakerPosition.x = 0;

				moving = false;
			}
		}
	}


	void OnGUI ()
	{

		if ( moveToOMB == true )
			GUI.FocusWindow ( 1 );

		if ( moveToMVfOMB == true || moveToMVfMM == true )
			GUI.FocusWindow ( 0 );

		if ( moveToMM == true )
			GUI.FocusWindow ( 4 );

		if ( startup == true )
		{

			GUI.FocusWindow ( 0 );
			startup = false;
		}
	}
}