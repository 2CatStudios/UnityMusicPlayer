using UnityEngine;
using System.Collections;
//Written by Michael Bethke
//This is a modified version of code by Jason Whitehouse
//Thank you to Aldo Naletto on Unity Answers
public class AudioVisualizer : MonoBehaviour
{

	StartupManager musicManager;
	MusicViewer musicViewer;
	
	public AudioSource audioSource;

	public float scale;
	public float yScale = 1;
	public int numSamples = 128;

	private float[] leftFCur; //Current frequency value (per vertex)
	private float[] leftFMax; //Target frequency value (per vertex)
	
	private float[] rightFCur;
	private float[] rightFMax;
	
	private float[] leftSpectrum;
	private float[] rightSpectrum;
	
	private float rmsValue;
	
	private float[] leftAudioOutput;
	private float[] rightAudioOutput;

	public LineRenderer topLeftLine;
	public LineRenderer bottomLeftLine;
	public LineRenderer topRightLine;
	public LineRenderer bottomRightLine;

	void Start ()
	{

		musicViewer = GameObject.FindGameObjectWithTag ( "MusicViewer" ).GetComponent<MusicViewer> ();

		leftAudioOutput = new float[numSamples];
		rightAudioOutput = new float[numSamples];
		leftSpectrum = new float[numSamples];
		rightSpectrum = new float[numSamples];
		leftFCur = new float [numSamples];
		rightFCur = new float [numSamples];
		leftFMax = new float [numSamples];
		rightFMax = new float [numSamples];

		topLeftLine.SetVertexCount ( numSamples );
		bottomLeftLine.SetVertexCount ( numSamples );
		topRightLine.SetVertexCount ( numSamples );
		bottomRightLine.SetVertexCount ( numSamples );
	}

	void Update ()
	{

		if ( musicViewer.showVisualizer == true )
		{

			topLeftLine.enabled = true;
			bottomLeftLine.enabled = true;
			topRightLine.enabled = true;
			bottomRightLine.enabled = true;

			audioSource.GetOutputData ( leftAudioOutput, 0 );
			audioSource.GetOutputData ( rightAudioOutput, 1 );

			int p;
			float sum = 0;

			for (p = 0; p < numSamples; p++)
			{
				
				sum += leftAudioOutput [p] * leftAudioOutput [p];
				sum += rightAudioOutput [p] * rightAudioOutput [p];
			}

			rmsValue = Mathf.Sqrt ( sum / numSamples );
			rmsValue = Mathf.Max ( rmsValue, .3F );

			audioSource.GetSpectrumData ( leftSpectrum, 0, FFTWindow.Triangle );
			audioSource.GetSpectrumData ( rightSpectrum, 1, FFTWindow.Triangle );

			for ( int i = 0; i < numSamples; i++ )
			{

				float x = scale * i;

				float leftY = leftSpectrum [i] * rmsValue * yScale;
				float rightY = rightSpectrum [i] * rmsValue * yScale;

				leftFMax [i] = Mathf.Max ( leftFMax [i], leftY );
				rightFMax [i] = Mathf.Max ( rightFMax [i], rightY );

				if ( leftFCur [i] > leftFMax [i])	
					leftFCur [i] = Mathf.Clamp ( leftFCur [i] - Time.deltaTime * 60 * rmsValue * 5, leftFMax [i], leftFCur [i]);
				else
					leftFCur [i] = Mathf.Clamp ( leftFCur [i] + Time.deltaTime * 100 * rmsValue * 5, leftFCur [i], leftFMax [i]);
				
				
				if ( rightFCur [i] > rightFMax [i])	
					rightFCur [i] = Mathf.Clamp ( rightFCur [i] - Time.deltaTime * 60 * rmsValue * 5, rightFMax [i], rightFCur [i]);
				else
					rightFCur [i] = Mathf.Clamp ( rightFCur [i] + Time.deltaTime * 100 * rmsValue * 5, rightFCur [i], rightFMax [i]);

				leftFMax [i] -= Time.deltaTime * 6000;
				rightFMax [i] -= Time.deltaTime * 6000;
				leftY = leftFCur [i];
				rightY = rightFCur [i];

				topLeftLine.SetPosition ( i, new Vector3 ( x, leftY, 0 ));
				bottomLeftLine.SetPosition ( i, new Vector3 ( x, leftY, 0 ));
				topRightLine.SetPosition ( i, new Vector3 ( x, rightY, 0 ));
				bottomRightLine.SetPosition ( i, new Vector3 ( x, rightY, 0 ));
			}

		} else {
			topLeftLine.enabled = false;
			bottomLeftLine.enabled = false;
			topRightLine.enabled = false;
			bottomRightLine.enabled = false;
		}
	}
}