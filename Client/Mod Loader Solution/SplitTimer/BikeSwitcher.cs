using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;

namespace SplitTimer
{
	public class BikeSwitcher : MonoBehaviour
	{
        public string oldBike;
        public void ToBike(string bike)
        {
            Debug.Log("ToBike(" + bike + ")");
            StartCoroutine(_ToBike(bike));
        }
        IEnumerator _ToBike(string bike)
        {
            if (!IsDescBike(oldBike) && IsDescBike(bike))
                yield return DelicatePlayerRespawn();
            if (bike == "enduro")
                gameObject.GetComponent<Utilities>().SetBike(0);
            else if (bike == "downhill")
                gameObject.GetComponent<Utilities>().SetBike(1);
            else if (bike == "hardtail")
                gameObject.GetComponent<Utilities>().SetBike(2);
            else
            {
                if (AssetBundling.Instance.bundle != null)
                {
                    GameObject bikeReplacement = AssetBundling.Instance.bundle.LoadAsset<GameObject>(bike);
                    ReplaceBike(
                        bikeReplacement.GetComponentInChildren<SkinnedMeshRenderer>(),
                        bikeReplacement.GetComponent<Animation>()
                    );
                }
                else
                    throw new System.Exception("AssetBundle not loaded! Can't load into specialised demo!!");
            }
            PlayerInf.Instance.OnBikeSwitch(oldBike, bike);
            oldBike = bike;
        }
        public void ReplaceBike(SkinnedMeshRenderer newSkinnedMeshRenderer, Animation newAnimation)
        {
            GetBikeObject().GetComponent<SkinnedMeshRenderer>().sharedMesh = newSkinnedMeshRenderer.sharedMesh;
            Animation currentBikeAnim = GetBikeModelAnim();
            currentBikeAnim.Stop();
            foreach (AnimationClip q in animToAnimStates(newAnimation))
            {
                if (q.name == "base")
                    currentBikeAnim.clip = q;
                currentBikeAnim.RemoveClip(q.name);
                currentBikeAnim.AddClip(q, q.name);
            }
            currentBikeAnim.Play();
            foreach (BikeAnimation x in FindObjectsOfType<BikeAnimation>())
            {
                CopyComponent(x, x.gameObject);
                Destroy(x);
            }
        }
        IEnumerator DelicatePlayerRespawn()
        {
            Vector3 pos = Utilities.instance.GetPlayer().transform.position;
            Vector3 rot = Utilities.instance.GetPlayer().transform.eulerAngles;
            Destroy(Utilities.instance.GetPlayer());
            yield return new WaitForSeconds(0.1f);
            FindObjectOfType<PlayerManager>().SpawnPlayerObject(Utilities.instance.GetPlayerInfoImpact());
            yield return new WaitForSeconds(0.1f);
            Utilities.instance.GetPlayer().transform.position = pos;
            Utilities.instance.GetPlayer().transform.eulerAngles = rot;
            yield return new WaitForSeconds(0.2f);
        }
        bool IsDescBike(string bike)
        {
            // returns true if player was previously on a descenders-own bike
            return bike == "enduro" || bike == "hardtail" || bike == "downhill";
        }
        GameObject GetBikeObject()
        {
            foreach (SkinnedMeshRenderer x in FindObjectsOfType<SkinnedMeshRenderer>())
                if (x.gameObject.name == "bike_downhill_LOD0" && x.gameObject.transform.root.name == "Player_Human")
                    return x.gameObject;
            return null;
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
        Animation GetBikeModelAnim()
        {
            foreach (Animation a in FindObjectsOfType<Animation>())
            {
                Debug.Log("Anim Root: " + a.transform.root.name);
                if (a.name == "BikeModel" && a.transform.root.name == "Player_Human")
                    return a;
            }
            return null;
        }
    }
}