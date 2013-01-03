using UnityEngine;
using System.Collections;
//Written by Gibson Bethke
//This is a modified version of code by Jason Whitehouse
public class AudioVisualizationR : MonoBehaviour
{
		
	public bool showAV = false;

	public	float scale; //Scale along x (length)
	public	float yScale = 1; //Scale of all frequency-based motion on the y
	public	float scaleOffset; //Scaling of sine waves
	public	float speedOffset = 8; //Speed of sine waves
	public	float minSin; //Minimum amplitude multiplier of sine waves.
	public	int channel; //Which channel to pull audio from (0 or 1)
	int numSamples = 128; //Number of samples to take from audio
	public	AudioSource audioSource; //AudioSource to pull data from
	public	float sineFade; //Number of points along lineRenderer it takes for sine wave to fade out

	private float[] fCur; //Current frequency value (per vertex)
	private float[] fMax; //Target frequency value (per vertex)

	private float[] spectrum; //Arrat to store spectrum data from audio
	private float rmsValue;
	private float[] volume; //Array to store outpu data in for calculating volume
	private float mMax; // Target value for sine amplitude multiplier
	private float mCur = 0; //Current value for sine amplitude multiplier

	public LineRenderer[] renderers;
	
	void Start ()
	{
	
		//Define arrays
		volume = new float[numSamples];
		spectrum = new float[numSamples];
		fCur = new float [numSamples];
		fMax = new float [numSamples];
		
		foreach(LineRenderer l in renderers)
		{
			
        	l.SetVertexCount (numSamples);
        	l.useWorldSpace = false;
        }
	}
	
	void Update ()
		{
			
			if (showAV == true)
			{

				foreach (LineRenderer l in renderers)	
					l.enabled = true;

				#region Get volume
				//Thank you to Aldo Naletto on Unity Answers**/
		
				//Fill array with samples
				audioSource.GetOutputData (volume, channel);
		
				int p;
				float sum = 0;
		
				for (p = 0; p < numSamples; p++)
					// sum squared samples
					sum += volume [p] * volume [p];

				// rms = square root of average
				rmsValue = Mathf.Sqrt (sum / numSamples);
				rmsValue = Mathf.Max (rmsValue, .3F);
	
				#endregion


				//Get Spectrum data
				audioSource.GetSpectrumData (spectrum, channel, FFTWindow.Rectangular);
	
				#region Iterate through each sample and linerenderer point
				for (int i = 0; i < numSamples; i++)
				{
				
					//Move along x plane based on scale
					float x = scale * i;
		   
					//Move last segment out into the distance to make beam look longer
					if (i == numSamples - 1)	
						x += 400;
		   
					//Get spectrum data for current index. Multiply in scale and volume.
					float y = spectrum [i] * rmsValue * 2 * yScale;
		   
					//Transform y value towards target
					fMax [i] = Mathf.Max (fMax [i], y); //Same as above we want the highwest point, so wither the current target or the new data
		   

					#region Transform towards target
					if (fCur [i] > fMax [i])	
						fCur [i] = Mathf.Clamp (fCur [i] - Time.deltaTime * 60 * rmsValue * 5, fMax [i], fCur [i]);
					else
						fCur [i] = Mathf.Clamp (fCur [i] + Time.deltaTime * 100 * rmsValue * 5, fCur [i], fMax [i]);
			   
					fMax [i] -= Time.deltaTime * 3000; //Lower our max over time
					y = fCur [i]; //Set the y value
			
					#endregion

					//Apply calculated position to current point in Linerenderer
					foreach (LineRenderer l in renderers)
							l.SetPosition (i, new Vector3 (x, y, 0));
			}
		
			#endregion
					
			foreach (LineRenderer l in renderers)	
					l.gameObject.renderer.material.mainTextureOffset -= new Vector2 (Time.deltaTime * 20 * mCur, 0);
		} else {
				foreach (LineRenderer l in renderers)	
						l.enabled = false;
		}
	}
}