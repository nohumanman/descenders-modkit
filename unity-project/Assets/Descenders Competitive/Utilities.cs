using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
namespace DescendersCompetitive{
	public class Utilities : MonoBehaviour {
		[MenuItem("Tools/Descenders Competitive/Locate Missing Scripts/All")]
		public static void FindAllMissingScriptsAll(){
			FindAllMissingScriptsInScene();
			FindAllMissingScriptsInAssets();
		}
		[MenuItem("Tools/Descenders Competitive/Locate Missing Scripts/In Scene")]
		public static void FindAllMissingScriptsInScene(){
			foreach(GameObject gameObject in FindObjectsOfType<GameObject>()){
				foreach(Component component in gameObject.GetComponentsInChildren<Component>()){
					if (component == null){
						Debug.LogWarning("GameObject found with missing script '" + gameObject.name + "'", gameObject);
					}
				}
			}
		}
		[MenuItem("Tools/Descenders Competitive/Locate Missing Scripts/In Assets")]
		public static void FindAllMissingScriptsInAssets(){
			string[] prefabPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)).ToArray();
			foreach (string path in prefabPaths)
			{
				GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				foreach (Component component in prefab.GetComponentsInChildren<Component>())
				{
					if (component == null)
					{
						Debug.LogWarning("Object with missing script at asset path '" + path + "'");
						break;
					}
				}
			}
		}
	}
}