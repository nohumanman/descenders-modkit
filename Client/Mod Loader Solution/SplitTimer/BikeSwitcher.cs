using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;

namespace SplitTimer
{
	public class BikeSwitcher : MonoBehaviour
	{
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                ToSpecialisedDemo();
            if (Input.GetKeyDown(KeyCode.Alpha2))
                StartCoroutine(ToEnduro());
            if (Input.GetKeyDown(KeyCode.Alpha3))
                StartCoroutine(ToDownhill());
            if (Input.GetKeyDown(KeyCode.Alpha4))
                StartCoroutine(ToHardtail());
            if (Input.GetKeyDown(KeyCode.Alpha5))
                ToBMX();
            if (Input.GetKeyDown(KeyCode.Alpha6))
                ToCoffee();
            if (Input.GetKeyDown(KeyCode.Alpha7))
                ToCanyonSpectral();
            if (Input.GetKeyDown(KeyCode.Alpha8))
                ToFish();
            if (Input.GetKeyDown(KeyCode.Alpha9))
                ToIntenseM16();
        }
        public string oldBike;
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
        public void ToIntenseM16()
        {
            if (AssetBundling.Instance.bundle != null)
            {
                GameObject bikeReplacement = AssetBundling.Instance.bundle.LoadAsset<GameObject>("Intense_M16");
                ReplaceBike(
                    bikeReplacement.GetComponentInChildren<SkinnedMeshRenderer>(),
                    bikeReplacement.GetComponent<Animation>()
                );
                PlayerInf.Instance.OnBikeSwitch(oldBike, "Intense_M16");
                oldBike = "Intense_M16";
            }
            else
                Debug.LogError("AssetBundle not loaded! Can't load into specialised demo!!");
        }
        public void ToCanyonSpectral()
        {
            if (AssetBundling.Instance.bundle != null)
            {
                GameObject bikeReplacement = AssetBundling.Instance.bundle.LoadAsset<GameObject>("Canyon_Spectral");
                ReplaceBike(
                    bikeReplacement.GetComponentInChildren<SkinnedMeshRenderer>(),
                    bikeReplacement.GetComponent<Animation>()
                );
                PlayerInf.Instance.OnBikeSwitch(oldBike, "Canyon_Spectral");
                oldBike = "Canyon_Spectral";
            }
            else
                Debug.LogError("AssetBundle not loaded! Can't load into specialised demo!!");
        }
        public void ToFish()
        {
            if (AssetBundling.Instance.bundle != null)
            {
                GameObject bikeReplacement = AssetBundling.Instance.bundle.LoadAsset<GameObject>("Fish");
                ReplaceBike(
                    bikeReplacement.GetComponentInChildren<SkinnedMeshRenderer>(),
                    bikeReplacement.GetComponent<Animation>()
                );
                PlayerInf.Instance.OnBikeSwitch(oldBike, "Fish");
                oldBike = "Fish";
            }
            else
                Debug.LogError("AssetBundle not loaded! Can't load into specialised demo!!");
        }
        public void ToCoffee()
        {
            if (AssetBundling.Instance.bundle != null)
            {
                GameObject bikeReplacement = AssetBundling.Instance.bundle.LoadAsset<GameObject>("Coffee_Bike");
                ReplaceBike(
                    bikeReplacement.GetComponentInChildren<SkinnedMeshRenderer>(),
                    bikeReplacement.GetComponent<Animation>()
                );
                PlayerInf.Instance.OnBikeSwitch(oldBike, "Coffee_Bike");
                oldBike = "Coffee_Bike";
            }
            else
                Debug.LogError("AssetBundle not loaded! Can't load into specialised demo!!");
        }
        public void ToBMX()
        {
            if (AssetBundling.Instance.bundle != null)
            {
                GameObject bikeReplacement = AssetBundling.Instance.bundle.LoadAsset<GameObject>("BMX");
                ReplaceBike(
                    bikeReplacement.GetComponentInChildren<SkinnedMeshRenderer>(),
                    bikeReplacement.GetComponent<Animation>()
                );
                PlayerInf.Instance.OnBikeSwitch(oldBike, "BMX");
                oldBike = "BMX";
            }
            else
                Debug.LogError("AssetBundle not loaded! Can't load into specialised demo!!");
        }
        public void ToSpecialisedDemo()
        {
            if (AssetBundling.Instance.bundle != null)
            {
                GameObject bikeReplacement = AssetBundling.Instance.bundle.LoadAsset<GameObject>("Specialized_Demo");
                ReplaceBike(
                    bikeReplacement.GetComponentInChildren<SkinnedMeshRenderer>(),
                    bikeReplacement.GetComponent<Animation>()
                );
                PlayerInf.Instance.OnBikeSwitch(oldBike, "specialised_demo");
                oldBike = "specialised_demo";
            }
            else
                Debug.LogError("AssetBundle not loaded! Can't load into specialised demo!!");
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
        bool WasOnDescBike()
        {
            // returns true if player was previously on a descenders-own bike
            return oldBike == "enduro" || oldBike == "hardtail" || oldBike == "downhill";
        }
		public IEnumerator ToEnduro()
        {
            if (!WasOnDescBike())
                yield return DelicatePlayerRespawn();
            gameObject.GetComponent<Utilities>().SetBike(0);
            PlayerInf.Instance.OnBikeSwitch(oldBike, "enduro");
            oldBike = "enduro";
        }
		public IEnumerator ToDownhill()
        {
            if (!WasOnDescBike())
                yield return DelicatePlayerRespawn();
            gameObject.GetComponent<Utilities>().SetBike(1);
            PlayerInf.Instance.OnBikeSwitch(oldBike, "downhill");
            oldBike = "downhill";
        }
		public IEnumerator ToHardtail()
        {
            if (!WasOnDescBike())
                yield return DelicatePlayerRespawn();
            gameObject.GetComponent<Utilities>().SetBike(2);
            PlayerInf.Instance.OnBikeSwitch(oldBike, "hardtail");
            oldBike = "hardtail";
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