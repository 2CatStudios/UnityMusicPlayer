using System.IO;
using UnityEngine;
using System.Collections;
//Written by GibsonBethke
//Jesus, you are awesome!
//Thanks to Jan Heemstra for the idea
public class PaneManager : MonoBehaviour
{

	public StartupManager startupManager;
//	public MusicManager musicManager;
	public MusicViewer musicViewer;
	public OnlineMusicBrowser onlineMusicBrowser;
	
	public GUISkin guiSkin;
	GUIStyle leftArrowStyle;
	GUIStyle rightArrowStyle;
	
	public Texture2D leftArrowNormal;
	public Texture2D leftArrowHover;
	public Texture2D rightArrowNormal;
	public Texture2D rightArrowHover;

	internal bool popupBlocking = false;
	
	internal bool loading = false;
	bool startup = true;

	internal enum pane { musicViewer, onlineMusicBrowser };
	internal pane currentPane = pane.musicViewer;

	bool moving = false;
	internal bool moveToOMB = false;
	bool moveToMV = false;

	float moveVelocity = 0.0F;

/*

	GUI.Window 0 is MusicViewer
	GUI.Window 1 is OnlineMusicViewer
	GUI.Window 2 is DownloadInfo
	GUI.Window 3 is UpdateAvailable
	GUI.Window 4 is *NULL*
	GUI.Window 5 is Settings
	GUI.Window 6 is NewFolder

*/
	
	
	void Start ()
	{
		
		leftArrowStyle = new GUIStyle ();
		leftArrowStyle.normal.background = leftArrowNormal;
		leftArrowStyle.hover.background = leftArrowHover;
		
		rightArrowStyle = new GUIStyle ();
		rightArrowStyle.normal.background = rightArrowNormal;
		rightArrowStyle.hover.background = rightArrowHover;

	}
	
	
	void Update()
	{
		
		if ( musicViewer.slideshow == false )
		{
		
			if ( popupBlocking == false && Input.GetKey ( KeyCode.RightArrow ) && currentPane == pane.musicViewer && moving == false && loading == false )
			{

				moving = true;
				moveToOMB = true;
				onlineMusicBrowser.showOnlineMusicBrowser = true;
			}

			if ( popupBlocking == false && Input.GetKey ( KeyCode.LeftArrow ) && currentPane == pane.onlineMusicBrowser && moving == false )
			{
			
				moving = true;
				moveToMV = true;
				musicViewer.showMusicViewer = true;
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

				musicViewer.showMusicViewer = false;
				moving = false;
			}
		}
		
		//Move to MusicViewer from OnlineMusicBrowser
		if ( moveToMV == true )
		{
			
			moving = true;

			float smoothDampIn = Mathf.SmoothDamp ( musicViewer.musicViewerPosition.x, 0.0F, ref moveVelocity, 0.1F, 4000 );
			float smoothDampOut = Mathf.SmoothDamp ( onlineMusicBrowser.onlineMusicBrowserPosition.x, onlineMusicBrowser.onlineMusicBrowserPosition.width + onlineMusicBrowser.onlineMusicBrowserPosition.width / 4, ref moveVelocity, 0.1F, 4000 );

			musicViewer.musicViewerPosition.x = smoothDampIn;
			onlineMusicBrowser.onlineMusicBrowserPosition.x = smoothDampOut;

			if ( musicViewer.musicViewerPosition.x > -5 )
			{

				moveVelocity = 0;
				moveToMV = false;

				currentPane = pane.musicViewer;

				musicViewer.musicViewerPosition.x = 0;
				onlineMusicBrowser.onlineMusicBrowserPosition.x = onlineMusicBrowser.onlineMusicBrowserPosition.width + onlineMusicBrowser.onlineMusicBrowserPosition.width / 4;

				onlineMusicBrowser.showOnlineMusicBrowser = false;
				onlineMusicBrowser.scrollPosition = new Vector2 ( 0, 0 );
				onlineMusicBrowser.horizontalScrollPosition = new Vector2 ( 0, 0 );
				moving = false;
			}
		}
	}


	void OnGUI ()
	{

		if ( moveToOMB == true )
			GUI.FocusWindow ( 1 );

		if ( moveToMV == true )
			GUI.FocusWindow ( 0 );

		if ( startup == true )
		{

			GUI.FocusWindow ( 0 );
			startup = false;
		}
		
		GUI.skin = guiSkin;
		
		if ( musicViewer.slideshow == false )
		{
			
			if ( musicViewer.showArrows == true )
			{
				
				if ( moving == false )
				{
				
					if ( currentPane == pane.onlineMusicBrowser )
					{
					
						if ( GUI.Button ( new Rect ( 25, 25, 36, 36 ), "", leftArrowStyle ))
						{
						
							if ( currentPane == pane.onlineMusicBrowser )
							{
							
								moving = true;
								moveToMV = true;
								musicViewer.showMusicViewer = true;
							}
						}
					}
				
					if ( currentPane == pane.musicViewer && loading == false )
					{
					
						if ( GUI.Button ( new Rect ( musicViewer.musicViewerPosition.width - 65, 25, 36, 36 ), "", rightArrowStyle ))
						{
						
							if ( currentPane == pane.musicViewer )
							{
							
								moving = true;
								moveToOMB = true;
								onlineMusicBrowser.showOnlineMusicBrowser = true;
							}
						}
					}
				}
			}
		}
		
		if ( musicViewer.slideshow == false )
		{
			
			if ( GUI.Button ( new Rect ( musicViewer.musicViewerPosition.width - 75, musicViewer.musicViewerPosition.height - 40, 60, 30 ), "Quit" ))
				musicViewer.SendMessage ( "Quit" );
		}
	}
}