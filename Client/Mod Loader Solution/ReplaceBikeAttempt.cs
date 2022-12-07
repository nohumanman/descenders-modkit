using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReplaceBikeAttempt : MonoBehaviour
{
    public struct BikeReplacement
    {
        public string nm;
        public Mesh skinnedMeshRenderer;
        public AnimationClip[] animationClips;
    }
    public List<BikeReplacement> bikeReplacements = new List<BikeReplacement>();
    public SkinnedMeshRenderer newSkinnedMeshRenderer;
    public Animation ourClips;
    int currentBike = 0;
    int frm = 0;
    void Update()
    {
        if (bikeReplacements.Count == 0 && GetBikeObject() != null && GetBikeModelAnim() != null)
        {
            frm++;
            if (frm > 60)
            {
                Debug.Log("GETTING ALL BIKEREPLACEMENTS!!");
                bikeReplacements = new List<BikeReplacement>();
                BikeReplacement bikeReplacement = new BikeReplacement();
                bikeReplacement.skinnedMeshRenderer = newSkinnedMeshRenderer.sharedMesh;
                bikeReplacement.animationClips = animToAnimStates(ourClips);
                bikeReplacement.nm = "bike replacement";
                bikeReplacements.Add(bikeReplacement);
                BikeReplacement currentBike = new BikeReplacement();
                currentBike.skinnedMeshRenderer = GetBikeObject().GetComponent<SkinnedMeshRenderer>().sharedMesh;
                currentBike.animationClips = animToAnimStates(GetBikeModelAnim());
                currentBike.nm = "existing bike";
                bikeReplacements.Add(currentBike);
            }
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (currentBike >= bikeReplacements.Count)
                currentBike = 0;
            Debug.Log("Switching to bike '" + bikeReplacements[currentBike].nm + "'");
            SwitchBikeModel(bikeReplacements[currentBike]);
            currentBike++;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            
            foreach(BikeReplacement bikeReplacement in bikeReplacements)
            {
                Debug.Log("\nFrom bike '" + bikeReplacement.nm + "':");
                foreach (AnimationClip x in bikeReplacement.animationClips)
                {
                    Debug.Log(x.name);
                    Debug.Log(x.empty);
                }
            }
        }
    }
    Animation GetBikeModelAnim()
    {
        foreach (Animation a in FindObjectsOfType<Animation>())
            if (a.name == "BikeModel")
                return a;
        return null;
    }
    void SwitchBikeModel(BikeReplacement bikeReplacement)
    {
        GetBikeObject().GetComponent<SkinnedMeshRenderer>().sharedMesh = bikeReplacement.skinnedMeshRenderer;
        foreach (Animation a in FindObjectsOfType<Animation>())
        {
            if (a.name == "BikeModel")
            {
                Debug.Log("Switching animations up!");
                GameObject BikeModel = a.gameObject;
                Animation anim = BikeModel.GetComponent<Animation>();
                anim.Stop();
                foreach (AnimationClip q in bikeReplacement.animationClips)
                    anim.RemoveClip(q.name);
                foreach (AnimationClip q in bikeReplacement.animationClips)
                {
                    Debug.Log("Switching anim '" + q.name + "'");
                    if (q.name == "base")
                        anim.clip = q;
                    anim.RemoveClip(q.name);
                    anim.AddClip(q, q.name);
                }
                anim.Play();
            }
        }
        foreach (BikeAnimation x in FindObjectsOfType<BikeAnimation>())
        {
            Debug.Log("Copying and destroying componenet!");
            CopyComponent(x, x.gameObject);
            Destroy(x);
        }
    }
    AnimationClip[] animToAnimStates(Animation anim)
    {
        List<AnimationClip> animStateList = new List<AnimationClip>();
        foreach (AnimationState x in anim)
            animStateList.Add(x.clip);
        return animStateList.ToArray();
    }
    // stolen from https://answers.unity.com/questions/458207/copy-a-component-at-runtime.html
    Component CopyComponent(Component original, GameObject destination)
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        // Copied fields can be restricted with BindingFlags
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy;
    }
    GameObject GetBikeObject()
    {
        foreach (SkinnedMeshRenderer x in FindObjectsOfType<SkinnedMeshRenderer>())
            if (x.gameObject.name == "bike_downhill_LOD0")
                return x.gameObject;
        return null;
    }
}
