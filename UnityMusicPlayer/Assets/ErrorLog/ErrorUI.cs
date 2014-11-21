using UnityEngine;
using System.Collections;
//Written by Michael Bethke
[RequireComponent (typeof (ErrorLog))]
public class ErrorUI : MonoBehaviour
{

	ErrorLog errorLog;
	GUIStyle textStyle;
	
	public Color textColour = Color.black;
	
	internal Vector2 debugScrollPosition = new Vector2 ( 0, 0 );
	
	
	void Start ()
	{
		
		errorLog = gameObject.GetComponent<ErrorLog>();
		
		textStyle = new GUIStyle ();
		textStyle.normal.textColor = textColour;
		textStyle.fontSize = 16;
	}
	

	void OnGUI ()
	{
			
		GUILayout.BeginArea ( new Rect ( 0, 0, Screen.width, Screen.height ));
		debugScrollPosition = GUILayout.BeginScrollView ( debugScrollPosition, false, false, GUILayout.Width ( Screen.width ), GUILayout.Height ( Screen.height ));
		GUILayout.FlexibleSpace ();
			
		for ( int index = 0; index < errorLog.log.Count; index += 1 )
		{
				
				GUILayout.Label ( errorLog.log[index], textStyle );
		}
				
		GUILayout.EndScrollView ();
		GUILayout.EndArea ();
	}
}
