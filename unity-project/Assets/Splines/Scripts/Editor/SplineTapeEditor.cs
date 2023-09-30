using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(SplineTape))]
public class SplineTapeEditor : Editor
{
	public override void OnInspectorGUI()
	{
		SplineTape splineTape = (SplineTape)target;

		DrawDefaultInspector();

		GUILayout.Space(10);
		GUILayout.Label("Spline tools", EditorStyles.boldLabel);
		if (GUILayout.Button("Respawn prefabs"))
		{
			splineTape.clickToRegen = true;
			EditorUtility.SetDirty(target);
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
		if (GUILayout.Button("Add spline node"))
		{
			GameObject newNode = new GameObject("Node", typeof(SplineExtraInfo));
			splineTape.AddNode(newNode);
			EditorUtility.SetDirty(target);
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			if (newNode != null)
				Selection.activeGameObject = newNode;
		}
		if (GUILayout.Button("Remove spline node"))
		{
			splineTape.RemoveNode();
			EditorUtility.SetDirty(target);
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
	}
}
