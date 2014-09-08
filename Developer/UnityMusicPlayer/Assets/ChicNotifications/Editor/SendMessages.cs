using UnityEditor;
using UnityEngine;
using System.Collections;
//Written by Michael Bethke

[CustomEditor ( typeof ( NotificationManager ))]
public class SendMessages : Editor
{	

	string customMessage = "Custom Message Text";
	bool darkFont = false;
	bool notificationSound = true;
	float delayTime = 5.0f;
	bool moveX = false;
	
    public override void OnInspectorGUI ()
    {
		
        DrawDefaultInspector();
        
        NotificationManager notificationManager = ( NotificationManager ) target;

		GUILayout.Space ( 10 );
		GUILayout.Label ( "Sample Notifications:\n");
		darkFont = GUILayout.Toggle ( darkFont, "Dark Font" );
		
		notificationSound = GUILayout.Toggle ( notificationSound, "Play Notification Sound" );
		
		GUILayout.BeginHorizontal ();
		delayTime = EditorGUILayout.FloatField ( delayTime, GUILayout.Width ( 20 ));
		GUILayout.Label ( "Message Duration (seconds)" );
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		
		moveX = GUILayout.Toggle ( moveX, "Move Text Along X-Axis" );
		
		GUILayout.BeginHorizontal ();
		
        if ( GUILayout.Button ( "Short Message", GUILayout.Width ( 200 )))
        {
			
			notificationManager.Message ( "Hello, World!", darkFont, notificationSound, delayTime, moveX );
        }
		GUILayout.FlexibleSpace ();
        if ( GUILayout.Button ( "Medium Message", GUILayout.Width ( 200 )))
        {
			
            notificationManager.Message ( "The quick brown fox jumps over the lazy dog.", darkFont, notificationSound, delayTime, moveX );
        }
		
		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		
        if ( GUILayout.Button ( "Long Message", GUILayout.Width ( 200 )))
        {
			
            notificationManager.Message ( "It is during our darkest moments that we must focus to see the light.", darkFont, notificationSound, delayTime, moveX );
        }
		GUILayout.FlexibleSpace ();
        if ( GUILayout.Button ( "Custom Message", GUILayout.Width ( 200 )))
        {
			
            notificationManager.Message ( customMessage, darkFont, notificationSound, delayTime, moveX );
        }
		
		GUILayout.EndHorizontal ();
		
		customMessage = EditorGUILayout.TextField ( customMessage );
    }
}