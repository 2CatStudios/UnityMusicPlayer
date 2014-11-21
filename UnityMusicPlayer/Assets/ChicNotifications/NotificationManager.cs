using UnityEngine;
using System.Collections;
//Written by Michael Bethke

[RequireComponent ( typeof ( AudioSource ))]						// If there is not an AudioSource on this gameObject, throw an error. An AudioSource is required to play the notification sound.
public class NotificationManager : MonoBehaviour
{

	public Font font;						// The font to use for all notifications. Can only be modified outside of runtime.
	public AudioClip soundEffect;						// The AudioClip to play when a notification has been received. Can be modified during runtime.
	
	Rect lineRect;						// The position to draw the line. Height value is modified by code [will override]. Can be modified during runtime.
	Rect textRect;						// The position to draw the text. X value is modified by code [will not override]. Can be modified during runtime.
	
	Color textAlpha;						// The colour of the font. Alpha value is modified by code [will override]. Can be modified during runtime.
	
	GUIStyle guistyle;						// The GUIStyle used to draw the Notification Text. Can only be modified outside of runtime.
	Texture2D line;						// The Texture of the line. Generated at runtime [will override]. Can be modified during runtime.
	
	float aColour = 0.0f;						// The Alpha value of 'textAlpha'. Will override. Cannot be modified.
	float ySize = 0;						// The Height value of 'lineRect'. Will override. Cannot be modified.
	float xDistance = 0;						// Part of the X value of 'textRect'. Can be modified during runtime.
	float xDistanceGoal = 100;						// The distance for Notification Text to move when displayed. Can be during runtime.
	bool moveX = true;						// Should the text move when displayed. Cannot be modified.
	
	bool visible = false;						// Is the Notification Texture visible. Cannot be modified.
	bool colourVisible = false;						// Is the Notification Text visible. Cannot be modified.
	
	bool displayNotification = false;						// Is the Notification active. Cannot be modified.
	float startTime;						// The time that the last notification was started. Used to determine how long a notification should display. Cannot be modified.
	
	float delayTime;						// The amount of time that must pass before the notification fades out. Cannot be modified.
	string notificationString;						// The message to display. Cannot be modified.
	

	void Start ()
	{
		
		if ( gameObject.tag != ( "NotificationManager" ))						// Verify that the Notification Manager can be found by other Objects.
		{
			
			UnityEngine.Debug.LogError ( "Notification Manager does not have the correct tag assigned! Please make sure that the tag 'NotificationManager' has been created, and applied to this GameObject!", gameObject );						// Throw an error if the correct tag has not been applied, and show the user the GameObject to apply the tag to.
		}
		
		textAlpha = new Color ( 1, 1, 1, 0 );						// Set the notification text's alpha value to 0 so it is hidden.
		
		guistyle = new GUIStyle ();						// Create a new GUIStyle.
		guistyle.font = font;						// Set it's font.
		guistyle.fontSize = 24;						// Set the font size.
		guistyle.padding = new RectOffset ( 0, 6, 0, 0 );						// Pad the right-side of the guistyle so that the notification text doesn't overlap the texture.
		guistyle.normal.textColor = new Color ( 1.0f, 1.0f, 1.0f, 1.0f );						// Set the GUIStyle's font color to solid white.
		guistyle.alignment = TextAnchor.UpperRight;						// Align the notification text to the top-right.
		guistyle.wordWrap = true;						// Allow the notification text to take more vertical room, if it runs out of horizontal room.
		
		Texture2D image2D = new Texture2D ( 1, 1, TextureFormat.ARGB32, false );						// Create a temporary empty texture.
		image2D.SetPixel ( 0, 0, new Color ( 1.0f, 1.0f, 1.0f, 0.8f ));						// Give the temporary texture a semi-transparent white colour.
		image2D.Apply ();						// Apply the changes (Important step!) to our temporary texture.
		
		line = image2D;						// Set the notification texture to the texture that we just created.
	}
	
	
	public void Message ( string message, bool darkFont = false, bool playSoundEffect = true, float displayTime = 5.0f, bool moveText = false )						// Function to receive messages. Has one mandatory parameter ( message ), and four optional ones.
	{
		
		notificationString = null;
		
		if ( darkFont == false )						// Sets the font colour based on the optional parameter 'darkFont'. If no value is given, the default value of False will be used.
		{
			
			guistyle.normal.textColor = new Color ( 1.0f, 1.0f, 1.0f, 1.0f );						// Set font colour to white.
		} else {
			
			guistyle.normal.textColor = new Color ( 0.3f, 0.3f, 0.3f, 1.0f );						// Set font colour to grey.
		}
		
		delayTime = displayTime;						// Set the delay time to the optional parameter 'displayTime'. If no value is given, the default value of Five Seconds will be used. This determines the length of time that a notification will be displayed.
		moveX = moveText;						// Set the value of moveText to the optional parameter 'moveText'. If no value is given, the default value of False will be used. This toggles the horizontal movement of the notification's text.
		
		if ( message != null && message.Length > 0 )						// Make sure that there is a message to display.
		{
		
			visible = false;						// Reset visible to False so that the notification starts invisible.
			colourVisible = false;						// Reset colourVisible to False so that the notification's text starts invisible.
			aColour = 0;						// Reset aColour to 0 so that the actual value of the notification's text colour is opaque.
			xDistance = 0;						// Reset xDistance to 0 so that the notification's text starts in the correct position.
			ySize = 0;						// Reset ySize to 0 so that the notification texture can scale in.
			
			if ( playSoundEffect == true )						// Play a sound effect if the optional parameter 'playSoundEffect' is true. If no value is given, the default value of True will be used.
			{
				
				gameObject.GetComponent<AudioSource>().PlayOneShot ( soundEffect, 0.7f );						// Locate the AudioSource on this gameObject, and send it the clip (and a default volume), and tell it to play.
			}
			notificationString = message;						// Set the notificationString to the mandatory parameter 'message'.
			displayNotification = true;						// Set fade to true, starting the fade-in effect.
		}
	}
	
	
	void Update ()
	{
		
		if ( displayNotification == true )						// Only execute the following code if there is a message to display.
		{
			
			if ( visible == false )						// Execute the following code if the notification texture is not completely visible.
			{
				
				ySize = Mathf.SmoothDamp ( ySize, Screen.height/3, ref ySize, 0.05f );						// Set the height of the notification texture to a gradually-increasing value (to scale the texture out).
				
				if ( ySize == Screen.height/3 )						// If the height of the notification texture is one-third of the screen's height, stop it's growth.
				{
					
					startTime = Time.realtimeSinceStartup;						// Set the float 'startTime' to the current time, so that we'll be able to count up, and only display the notification for a set amount of time.
					ySize = Screen.height/3;						// Set the height of the notification texture to exactly one-third of the screen's height.
					visible = true;						// Set the boolean 'visible' to true, so that we will know to start displaying the notification's text.
				}
			}

			if ( visible == true )						// Execute the following code if the notification texture is completely visible.
			{
				
				if ( moveX == true )						// If the notification's text should move, execute the following code.
				{
			
					if ( xDistance < xDistanceGoal - 0.5f )						// If the distance that the notification's text has traveled is less than it's goal, execute the following code.
					{
				
						xDistance = Mathf.SmoothDamp ( xDistance, xDistanceGoal, ref xDistance, 0.05f, 5 );						// Set the float 'xDistance' to a gradually-increasing value.
					}
				}
				
				if ( Time.realtimeSinceStartup >= startTime + delayTime )						// If the message has been displayed for the specified amount of time (the optional parameter 'displayTime' from the Message function), execute the following code.
				{
					
					if ( colourVisible == true )						// If the colour of the notification's text is completely visible, execute the following code.
					{
					
						aColour = Mathf.SmoothDamp ( aColour, 0.0f, ref aColour, 0.05f );						// Set the float 'aColour' to a gradually-decreasing value. aColour is used to determine the alpha-value of the notification's text colour.
					
						if ( aColour <= 0.05f )						// If aColour is close to invisible, execute the following code.
						{
						
							aColour = 0;						// Set the float aColour to exactly 0.
							colourVisible = false;						// Set the boolean 'colourVisible' to false, so that we'll stop trying to modify it (for optimization purposes).
						}
					} else {
						
						ySize = Mathf.SmoothDamp ( ySize, 0, ref ySize, 0.05f );						// Set the height of the notification texture to a gradually-decreasing value (to scale the texture in).
				
						if ( ySize < 0.15f )						// If the height of the notification texture is close to 0, execute the following code.
						{

							ySize = 0;						// Set the notification texture to exactly 0.
							visible = false;						// Mark the notification texture as invisible.
							displayNotification = false;						// Set the boolean 'displayNotification' to false so that none of the code in the Update function will run without a notification waiting to be displayed.
						}
					}
				} else {
				
					if ( colourVisible == false )						// If the colour of the notification's text is not completely visible, execute the following code.
					{
					
						aColour = Mathf.SmoothDamp ( aColour, 1.0f, ref aColour, 0.05f );						// Set the float 'aColour' to a gradually-increasing value. aColour is used to determine the alpha-value of the notification's text colour.
					
						if ( aColour >= 0.95f )						// If aColour is close to visible, execute the following code.
						{
					
							aColour = 1;						// Set the float aColour to exactly 1.
							colourVisible = true;						// Set the boolean 'colourVisible' to true, so that we'll stop trying to modify it (for optimization purposes).
						}
					}
				}
			}
			
			lineRect = new Rect ( Screen.width - 100, 0, 2, ySize );						// Set the Rect 'lineRect' to a new Rect containing the updated height value.
			textRect = new Rect ( Screen.width - ( 500 - xDistance ), lineRect.height - 48, 400, 96 );						// Set the Rect 'textRect' to a new Rect containing the updated horizontal-position.
			
			textAlpha = new Color ( 1, 1, 1, aColour );						// Set the Colour 'textAlpha' to a new Colour containing the updated alpha-value.
		}
	}
	
	
	void OnGUI ()
	{
		
		if ( displayNotification == true )						// If there is a notification to display, execute the following code.
		{
		
			GUI.DrawTexture ( lineRect, line, ScaleMode.StretchToFill );						// Draw the notification texture on screen.
			
			GUI.color = textAlpha;						// Set the gui content (the following label) colour to the Colour 'textAlpha' so that it can fade-in/out.
			GUI.Label ( textRect, notificationString, guistyle );						// Draw the notification's text on screen. If the Colour 'textAlpha' is invisible, no text will be visible.
			GUI.color = Color.white;						// Set the rest of the gui content back to it's normal colours.
		}
	}
}
