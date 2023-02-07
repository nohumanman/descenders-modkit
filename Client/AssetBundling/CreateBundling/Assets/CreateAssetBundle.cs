using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CreateAssetBundle : MonoBehaviour {
	#if UNITY_EDITOR
	[MenuItem("Assets/Build AssetBundles")]
	public static void Create(){
		string assetBundleDirectory = "Assets/AssetBundles";
		BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
	}
	#endif
}
