using System.IO;
using UnityEngine;
using System.Collections;
//Written by Michael Bethke
//Jesus, you are awesome!
//Thanks to Jan Heemstra for the idea.
public class PaneManager : MonoBehaviour
{

	public StartupManager startupManager;
	public MusicViewer musicViewer;
	public OnlineMusicBrowser onlineMusicBrowser;
	
#region FPS_Counter
	
	public bool countFPS = false;
	float updateInterval = 1.0f;
	private double lastInterval;
	private int frames = 0;
	private float fps;
	
#endregion
	
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

	GUI.Window 0 : MusicViewer
	GUI.Window 1 : OnlineMusicViewer
	GUI.Window 2 : DownloadInfo
	GUI.Window 3 : UpdateAvailable
	GUI.Window 4 : *NULL*
	GUI.Window 5 : Settings
	GUI.Window 6 : *NULL*

*/
	
	
	void Start ()
	{
		
		lastInterval = Time.realtimeSinceStartup;
		
		leftArrowStyle = new GUIStyle ();
		leftArrowStyle.normal.background = leftArrowNormal;
		leftArrowStyle.hover.background = leftArrowHover;
		
		rightArrowStyle = new GUIStyle ();
		rightArrowStyle.normal.background = rightArrowNormal;
		rightArrowStyle.hover.background = rightArrowHover;

	}
	
	
	void Update()
	{
		
		if ( startupManager.developmentMode == true )
		{
		
			if ( countFPS == true )
			{

				frames += 1;
				float timeNow = Time.realtimeSinceStartup;
				if ( timeNow > lastInterval + updateInterval )
				{
					fps = System.Convert.ToSingle ( frames / ( timeNow - lastInterval ));
				    frames = 0;
				    lastInterval = timeNow;
				}
				
				UnityEngine.Debug.Log ( Mathf.Ceil ( fps ));
			}
		}
		
		//Move to OnlineMusicBrowser from MusicViewer
		if ( moveToOMB == true )
		{

			float smoothDampIn = Mathf.SmoothDamp ( onlineMusicBrowser.onlineMusicBrowserPosition.x, 0.0F, ref moveVelocity, 0.1f );
			float smoothDampOut = Mathf.SmoothDamp ( musicViewer.musicViewerPosition.x, -musicViewer.musicViewerPosition.width + -musicViewer.musicViewerPosition.width / 4, ref moveVelocity, 0.1F, 4000 );

			onlineMusicBrowser.onlineMusicBrowserPosition.x = smoothDampIn;
			musicViewer.musicViewerPosition.x = smoothDampOut;

			if ( onlineMusicBrowser.onlineMusicBrowserPosition.x < 1 )
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

			float smoothDampIn = Mathf.SmoothDamp ( musicViewer.musicViewerPosition.x, 0.0F, ref moveVelocity, 0.1f );
			float smoothDampOut = Mathf.SmoothDamp ( onlineMusicBrowser.onlineMusicBrowserPosition.x, onlineMusicBrowser.onlineMusicBrowserPosition.width + onlineMusicBrowser.onlineMusicBrowserPosition.width / 4, ref moveVelocity, 0.1F, 4000 );

			musicViewer.musicViewerPosition.x = smoothDampIn;
			onlineMusicBrowser.onlineMusicBrowserPosition.x = smoothDampOut;

			if ( musicViewer.musicViewerPosition.x > -1 )
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
		
		if ( startupManager.preferences.enableArrows == true )
		{
		
			if ( musicViewer.slideshow == false && musicViewer.hideGUI == false )
			{
				
				if ( moving == false )
				{
				
					if ( currentPane == pane.onlineMusicBrowser )
					{
						
						if ( GUI.Button ( new Rect ( 25, 60, 36, 36 ), "", leftArrowStyle ))
						{
							
							MoveToMV ();
						}
					}
					
					if ( currentPane == pane.musicViewer && loading == false )
					{
						
						if ( GUI.Button ( new Rect ( musicViewer.musicViewerPosition.width - 65, 60, 36, 36 ), "", rightArrowStyle ))
						{
							
							MoveToOMB ();
						}
					}
				}
			}
		}
		
		if ( musicViewer.slideshow == false && musicViewer.hideGUI == false )
		{
			
			if ( GUI.Button ( new Rect ( musicViewer.musicViewerPosition.width - 75, musicViewer.musicViewerPosition.height - 40, 60, 30 ), "Quit" ))
				musicViewer.Quit ();
		}
	}
	
	
	internal void MoveToOMB ()
	{
		
		if ( musicViewer.slideshow == false && musicViewer.hideGUI == false )
		{
			
			if ( moving == false && popupBlocking == false && loading == false )
			{
			
				if ( currentPane == pane.musicViewer )
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
	
	
	internal void MoveToMV ()
	{
		
		if ( musicViewer.slideshow == false && musicViewer.hideGUI == false )
		{
			
			if ( moving == false && popupBlocking == false )
			{
			
				if ( currentPane == pane.onlineMusicBrowser )
				{
				
					if ( currentPane == pane.onlineMusicBrowser )
					{
					
						moving = true;
						moveToMV = true;
						musicViewer.showMusicViewer = true;
					}
				}
			}
		}
	}
}