using System;
using System.IO;
using System.Net;
using UnityEngine;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Net.Security;
using System.IO.Compression;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
//Written by GibsonBethke
//Thank you Jesus, you are awesome!
//Thanks to Dom3D on UnityAnswers for your help!
public class AutomaticUpdate : MonoBehaviour
{

	string applicationLocation;
	Uri downloadLink;
	public GUIText guitext;
	public GUIText progresstText;


	void Start () 
	{

		string[] arguments = Environment.GetCommandLineArgs();
		ServicePointManager.ServerCertificateValidationCallback += delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

		downloadLink = new Uri (arguments[2].ToString ().Substring (1, arguments[2].Length-1));
		applicationLocation += arguments[1].ToString().Substring (1, arguments[1].Length -1) + "/UnityMusicPlayer";
		Directory.Delete(applicationLocation + ".app", true);

		guitext.text = "Downloading Update";
		
		using (WebClient client = new WebClient())
		try
		{

			client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);

			client.DownloadProgressChanged += (start, end) => { progresstText.text = end.ProgressPercentage.ToString() + "%" ; };

			client.DownloadFileAsync (downloadLink, applicationLocation + ".zip", 0);
		} catch (Exception error) {

			guitext.text = error.ToString ();
		}
	}

	void DownloadFileCompleted(object sender, AsyncCompletedEventArgs end)
	{

		guitext.text = "Download Finished! Uncompressing!";


	}
}
