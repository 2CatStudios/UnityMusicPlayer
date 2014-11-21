using UnityEngine;
using UnityEditor;

public class ExportAssetBundles
{
	
	[MenuItem("Assets/Build AssetBundle From Selection")]
	static void ExportResourceNoTrack ()
	{
			
	    string path = EditorUtility.SaveFilePanel ("Save Resource", "", "New Resource", "unity3d");
	    if (path.Length != 0)
		{
			
			BuildPipeline.BuildAssetBundle(Selection.activeObject, Selection.objects, path, BuildAssetBundleOptions.CollectDependencies, BuildTarget.StandaloneOSXIntel, BuildOptions.UncompressedAssetBundle );
	    }
	}
}