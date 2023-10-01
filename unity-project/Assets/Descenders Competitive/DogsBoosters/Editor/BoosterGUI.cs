using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Booster))]
public class BoosterGUI : Editor
{


    #region Serialized Propeties
    SerializedProperty CustomSettings;
    SerializedProperty Preset;
    SerializedProperty BoostIntensity;
    SerializedProperty BoostDuration;
    SerializedProperty triggered;
    SerializedProperty CurrentObject;
    SerializedProperty distanceFromBoost;
    SerializedProperty SmoothedBoost;
    SerializedProperty BoostTimer;
    string SettingsName;
    #endregion

    void OnEnable()
    {
        CustomSettings = serializedObject.FindProperty("CustomSettings");
        Preset = serializedObject.FindProperty("BoostPreset");
        BoostIntensity = serializedObject.FindProperty("BoostIntensity");
        BoostDuration = serializedObject.FindProperty("BoostDuration");
        CurrentObject = serializedObject.FindProperty("m_Object");
        distanceFromBoost = serializedObject.FindProperty("distanceFromBoost");
        triggered = serializedObject.FindProperty("triggered");
        SmoothedBoost = serializedObject.FindProperty("SmoothedBoost");
        BoostTimer = serializedObject.FindProperty("BoostTimer");
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();
        Booster Boost = (Booster)target;
        GUIStyle style = new GUIStyle();
        style.richText = true;
        GUILayout.Label("<size=12><b>Created by Gary Lowell (AKA <color=orange>Dog</color><color=fuchsia>tor</color><color=cyan>que</color>)</b> </size>", style);
        GUILayout.Label("<size=12><b>____________________________________</b> </size>", style);
        GUILayout.Space(10);

        if (GUILayout.Button(new GUIContent(SettingsName, "Would you like to use preset settings or custom settings?")))
        {
            CustomSettings.boolValue = !CustomSettings.boolValue;
        }


        if (!CustomSettings.boolValue)
        {
            SettingsName = "SWITCH TO CUSTOM SETTINGS";
        }
        else
        {
            SettingsName = "SWITCH TO PRESET SETTINGS";
        }

        if (!Boost.CustomSettings)
        {
            EditorGUILayout.PropertyField(Preset);
        }
        else
        {
            EditorGUILayout.PropertyField(BoostIntensity);
            EditorGUILayout.PropertyField(BoostDuration);
        }

        GUILayout.Label("<size=12><b>____________________________________</b> </size>", style);
        GUILayout.Space(10);

        Boost.DebugView = EditorGUILayout.Foldout(Boost.DebugView, "Debug Section");
        if (Boost.DebugView)
        {
            EditorGUILayout.PropertyField(triggered);
            EditorGUILayout.PropertyField(CurrentObject);
            EditorGUILayout.PropertyField(distanceFromBoost);
            EditorGUILayout.PropertyField(SmoothedBoost);
            EditorGUILayout.PropertyField(BoostTimer);
        }
        serializedObject.ApplyModifiedProperties();
        //base.OnInspectorGUI();
    }
}
