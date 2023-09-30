using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ModTool.Interface;
using System.Linq;
namespace DescendersCompetitive{
	public class Utilities : ModBehaviour {
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
					}
				}
			}
		}
		[MenuItem("Tools/Descenders Competitive/Verify/Partial Verify")]
        public static void PartialVerify(){
            foreach(Camera cam in FindObjectsOfType<Camera>()){
                Debug.LogWarning("Camera found! Remember cameras do NOT play well with Descenders!", cam);
            }
			if (FindObjectsOfType<Terrain>().Length > 1)
				Debug.LogWarning("Multiple terrains detected! This could cause issues without TerrainBoundryRemover");
			Debug.Log("Partial Verify Complete");
        }
        [MenuItem("Tools/Descenders Competitive/Verify/Full Verify")]
        public static void VerifyScriptConfig(){
			Debug.Log("Starting full Verify");
			PartialVerify();
            int errors = 0;
            int warnings = 0;

            // check if all startlines assigned to SpawnPoint are spawnpoints.
            if (FindObjectOfType<SpawnPoint>() != null){
                foreach (GameObject spawnPoint in FindObjectOfType<SpawnPoint>().SpawnPoints){
                    if (spawnPoint.tag != "Startline")
                        Debug.LogError("GameObject '" + spawnPoint.name + "' doesn't have a startline tag on it!");
                }
            }

            if (FindObjectOfType<APILoaderScript.ModLoader>() == null){
                Debug.LogError("APILoaderScript.ModLoader is not present!");
                errors += 1;
            }
            if (FindObjectOfType<JsonMapInfo>() == null){
                Debug.LogError("JsonMapInfo is not present!");
                errors += 1;
            }
            if (FindObjectOfType<TimerText>() == null){
                Debug.LogError("TimerText is not present!");
                errors += 1;
            }
            foreach(TeleportPad tp in FindObjectsOfType<TeleportPad>()){
                if (tp.TeleportPoint == null){
                    //Debug.LogWarning("TeleportPoint on TeleportPad is null!", tp.TeleportPoint);
                    warnings += 1;
                }
            }
            foreach(TimerInfo timerInf in FindObjectsOfType<TimerInfo>()){
                if (timerInf.startCheckpoint == null){
                    Debug.LogError("No startCheckpoint on TimerInfo!", timerInf.transform);
                    errors += 1;
                }
                if (timerInf.endCheckpoint == null){
                    Debug.LogError("No endCheckpoint on TimerInfo!", timerInf.transform);
                    errors += 1;
                }
                if (timerInf.leaderboardText == null){
                    Debug.LogWarning("No leaderboardText on TimerInfo!", timerInf.transform);
                    warnings += 1;
                }
                if (timerInf.autoLeaderboardText == null){
                    Debug.LogWarning("No autoLeaderboardText on TimerInfo!", timerInf.transform);
                    warnings += 1;
                }
                if (timerInf.boundaries == null){
                    Debug.LogError("No boundary gameobject on TimerInfo!", timerInf.transform);
                    errors += 1;
                }
                else{
                    foreach(Transform boundary in timerInf.boundaries.transform){
                        if (boundary.gameObject.GetComponent<MeshRenderer>() == null){
                            Debug.LogError("Boundary has no MeshRenderer!", boundary);
                            errors += 1;
                        }
                        else {
                            if (boundary.gameObject.GetComponent<MeshRenderer>().enabled){
                                Debug.LogWarning("MeshRenderer for boundary is enabled - consider Disabling all before export!", boundary);
                                warnings += 1;
                            }
                            if (boundary.gameObject.GetComponent<MeshRenderer>().sharedMaterial == null){
                                Debug.LogWarning("MeshRenderer has null material!", boundary);
                                warnings += 1;
                            }
                        }
                    }
                    foreach(Transform checkpoint in timerInf.endCheckpoint.transform.parent.transform){
                        if (checkpoint.gameObject.GetComponent<MeshRenderer>() == null){
                            Debug.LogError("Checkpoint has no MeshRenderer!", checkpoint);
                            errors += 1;
                        }
                        else {
                            if (checkpoint.gameObject.GetComponent<MeshRenderer>().enabled){
                                Debug.LogWarning("MeshRenderer for checkpoint is enabled - consider Disabling all before export!", checkpoint);
                                warnings += 1;
                            }
                            if (checkpoint.gameObject.GetComponent<MeshRenderer>().sharedMaterial == null){
                                Debug.LogWarning("MeshRenderer for checkpoint has null material!", checkpoint);
                                warnings += 1;
                            }
                        }
                    }
                }
            }
            if (errors == 0)
                Debug.Log("Scripts verified! (with " + warnings + " warnings)");
            else
                Debug.LogError("Scripts not verified!");
        }
        [MenuItem("Tools/Descenders Competitive/Boundaries/AutoAssignBoundaries")]
        public static void AttemptBoundaryAutoAssign(){
            foreach(TimerInfo timerInf in FindObjectsOfType<TimerInfo>())
                if (timerInf.boundaries == null)
                    foreach(Transform obj in timerInf.transform)
                        if (obj.name == "Boundaries")
                            timerInf.boundaries = obj.gameObject;
        }
        [MenuItem("Tools/Descenders Competitive/Boundaries/SelectBoundaries")]
        public static void SelectAllBoundaries(){
            List<GameObject> boundaries = new List<GameObject>();
            foreach(TimerInfo timerInf in FindObjectsOfType<TimerInfo>())
                foreach(Transform obj in timerInf.boundaries.transform)
                    boundaries.Add(obj.gameObject);
            GameObject[] x = new GameObject[boundaries.Count];
            int i = 0;
            foreach(GameObject boundary in boundaries){
                x[i] = boundary;
                i++;
            }
            Selection.objects = x;
        }
	}
}