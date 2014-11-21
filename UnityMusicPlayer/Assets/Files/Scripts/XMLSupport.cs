using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
//Thanks to Mike Talbot
public static class XMLSupport 
{
	
	public static T DeserializeXml<T> (this string xml) where T : class
	{
		
		if( xml != null )
		{
			
			var s = new XmlSerializer ( typeof ( T ) );
			using ( var m = new MemoryStream ( Encoding.UTF8.GetBytes ( xml )))
			{
				
				return ( T ) s.Deserialize ( m );
			}
		}
	
		UnityEngine.Debug.LogError ( "A wild error has apperaed!" );
		return null;
	}
}