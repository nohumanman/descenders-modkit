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
    public SkinnedMeshRenderer cachedPrevMeshRenderer;
    public Animation ourClips;
    public Animation copiedClips;
    int currentBike = 0;
    int frm = 0;
    void Start()
    {
        UnityEngine.Object[] x = Resources.LoadAll("Prefabs", typeof(GameObject));
        foreach(UnityEngine.Object obj in x)
            Debug.Log(obj.name);
    }
    void Update()
    {
        if (bikeReplacements.Count == 0 && GetBikeObject() != null && GetBikeModelAnim() != null)
        {
            frm++;
            if (frm > 200)
            {
                Debug.Log("GETTING ALL BIKEREPLACEMENTS!!");
                bikeReplacements = new List<BikeReplacement>();
                BikeReplacement bikeReplacement = new BikeReplacement();
                bikeReplacement.skinnedMeshRenderer = newSkinnedMeshRenderer.sharedMesh;
                bikeReplacement.animationClips = animToAnimStates(ourClips);
                bikeReplacement.nm = "bike replacement";
                bikeReplacements.Add(bikeReplacement);

                BikeReplacement currentBike = new BikeReplacement();
                cachedPrevMeshRenderer.sharedMesh = Instantiate(GetBikeObject().GetComponent<SkinnedMeshRenderer>().sharedMesh);
                currentBike.skinnedMeshRenderer = cachedPrevMeshRenderer.sharedMesh;
                //currentBike.skinnedMeshRenderer = Instantiate(GetBikeObject().GetComponent<SkinnedMeshRenderer>().sharedMesh);
                Debug.Log("40");
                foreach(AnimationClip q in animToAnimStates(GetBikeModelAnim()))
                    copiedClips.AddClip(q, q.name);
                Debug.Log("43");
                currentBike.animationClips = animToAnimStates(copiedClips);
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
        Debug.Log("SwitchBikeModel('" + bikeReplacement.nm + "');");
        Debug.Log("Replacing with sharedMesh '" + bikeReplacement.skinnedMeshRenderer.name + "'");
        // replace bike mesh with mesh of bikeReplacement.
        GetBikeObject().GetComponent<SkinnedMeshRenderer>().sharedMesh = bikeReplacement.skinnedMeshRenderer;
        // get the current bike Animation component
        Animation currentBikeAnim = GetBikeModelAnim();
        currentBikeAnim.Stop();
        foreach (AnimationClip q in bikeReplacement.animationClips)
        {
            Debug.Log("Switching anim '" + q.name + "'");
            if (q.name == "base")
                currentBikeAnim.clip = q;
            currentBikeAnim.RemoveClip(q.name);
            currentBikeAnim.AddClip(q, q.name);
        }
        currentBikeAnim.Play();
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
        {
            AnimationClip clone = Instantiate(x.clip);
            clone.name = x.name;
            animStateList.Add(clone);
        }
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
