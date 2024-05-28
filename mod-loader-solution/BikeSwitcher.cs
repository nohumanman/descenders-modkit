using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;

namespace ModLoaderSolution
{
	public class BikeSwitcher : MonoBehaviour
	{
        public string oldBike;
        public void Start(){
            oldBike = GetBike();
        }
        public void ToBike(string bike, string id)
        {
            Utilities.Log("id " + id + " switching to bike '" + bike + "'");
            if (Utilities.instance.isInReplayMode())
                return;
            if (id == (new PlayerIdentification.SteamIntegration()).getSteamId())
            {
                // if it's us, let the server know
                PlayerManagement.Instance.OnBikeSwitch(bike);
            }                
            StartCoroutine(_ToBike(bike, id));
        }
        public static string GetBike(){
            int pref = FindObjectOfType<PrefsManager>().GetInt("PREFERREDBIKE");
            if (pref == 1)
                return "downhill";
            else if (pref == 2)
                return "hardtail";
            return "enduro";
        }
        IEnumerator _ToBike(string bike, string id)
        {
            Utilities.LogMethodCallStart();
            GameObject PlayerObject = Utilities.GetPlayerFromId(id);
            if (PlayerObject != null)
            {
                GameObject BikeObject = GetBikeObject(PlayerObject);
                // if it wasn't a descenders bike and is now a descenders bike
                if (!IsDescBike(oldBike) && IsDescBike(bike))
                    yield return DelicatePlayerRespawn(id, PlayerObject, Utilities.GetPlayerInfoImpactFromId(id));
                int bikeType = Utilities.instance.GetBikeInt(bike);
                // if it's us, set our preferred bike
                if (id == (new PlayerIdentification.SteamIntegration()).getSteamId())
                    FindObjectOfType<PrefsManager>().SetInt("PREFERREDBIKE", bikeType);
                Utilities.instance.SetBike(Utilities.GetPlayerInfoImpactFromId(id), bikeType);
                // TEMPORARILY REMOVED NON-DESCENDERS BIKES 28.05.2024
                /*
                else
                {
                    if (AssetBundling.Instance.bundle != null)
                    {
                        GameObject bikeReplacement = AssetBundling.Instance.bundle.LoadAsset<GameObject>(bike);
                        if (bike == "BMX")
                        {
                            // rider animations on character_clothed_ragdoll
                            AnimatorOverrideController aoc = new AnimatorOverrideController(
                                GetPlayerAnim(PlayerObject).runtimeAnimatorController
                            );
                            var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                            foreach (AnimationClip descAnim in aoc.animationClips)
                            {
                                foreach (AnimationClip ourAnim in bikeReplacement.GetComponentInChildren<Animator>().runtimeAnimatorController.animationClips)
                                    if (ourAnim.name == descAnim.name)
                                    {
                                        Utilities.Log("Replacing anim " + descAnim.name + " with our custom one");
                                        anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(descAnim, ourAnim));
                                    }
                            }
                            aoc.ApplyOverrides(anims);
                            GetPlayerAnim(PlayerObject).runtimeAnimatorController = aoc;

                            // replace gestures
                            Gesture[] gestures = new Gesture[0] { };
                            string gesturesField = "EL\u0080\u007f\u0084\u0080o";
                            gestures = (Gesture[])typeof(Cyclist).GetField(gesturesField).GetValue(Utilities.GetPlayer().GetComponent<Cyclist>());
                            foreach (Gesture gesture in gestures)
                            {
                                // change gesture animations here!
                                Utilities.Log(gesture.trickName);
                            }
                            // replace gestures
                            typeof(Cyclist).GetField(gesturesField).SetValue(
                                Utilities.GetPlayer().GetComponent<Cyclist>(),
                                gestures
                            );
                        }
                        ReplaceBike(
                            bikeReplacement.GetComponentInChildren<SkinnedMeshRenderer>(),
                            bikeReplacement.GetComponent<Animation>(),
                            BikeObject,
                            PlayerObject
                        );
                        Utilities.instance.SetFreeCam();
                        Utilities.instance.SetBikeCamera();
                    }
                    else
                        throw new System.Exception("AssetBundle not loaded! Can't load into specialised demo!!");
                }
                */
            }
            Utilities.LogMethodCallEnd();
        }
        public Animator GetPlayerAnim(GameObject PlayerObject)
        {
            foreach (Animator a in FindObjectsOfType<Animator>())
                if (a.name == "character_clothed_ragdoll" && a.transform.root == PlayerObject.transform)
                    return a;
            return null;
        }
        public void ReplaceBike(SkinnedMeshRenderer newSkinnedMeshRenderer, Animation newAnimation, GameObject BikeObject, GameObject PlayerObject)
        {
            BikeObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = newSkinnedMeshRenderer.sharedMesh;
            
            Animation currentBikeAnim = GetBikeModelAnim(PlayerObject);
            if (currentBikeAnim == null)
            {
                Utilities.Log("Aaiosjdopiasjdpoajsdopjaspodjaposdjpoasjd==123=1-23=1-293=-1203=-102=3-012=3");
            }
            Utilities.Log("bikeObject:" + BikeObject);
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
        IEnumerator DelicatePlayerRespawn(string id, GameObject Player, PlayerInfoImpact playerInfoImpact)
        {
            // Player = Utilities.GetPlayer()
            Vector3 pos = Player.transform.position;
            Vector3 rot = Player.transform.eulerAngles;
            Destroy(Player);
            yield return new WaitForSeconds(0.1f);
            FindObjectOfType<PlayerManager>().SpawnPlayerObject(playerInfoImpact);
            yield return new WaitForSeconds(0.1f);
            Utilities.GetPlayerFromId(id).transform.position = pos;
            Utilities.GetPlayerFromId(id).transform.eulerAngles = rot;
            yield return new WaitForSeconds(0.2f);
        }
        bool IsDescBike(string bike)
        {
            // returns true if player was previously on a descenders-own bike
            return bike == "enduro" || bike == "hardtail" || bike == "downhill";
        }
        GameObject GetBikeObject(GameObject PlayerObject)
        {
            foreach (SkinnedMeshRenderer x in FindObjectsOfType<SkinnedMeshRenderer>())
                //if (x.gameObject.name == "bike_downhill_LOD0" && x.gameObject.transform.root.name == "Player_Human")
                if (x.gameObject.name == "bike_downhill_LOD0" && x.gameObject.transform.root == PlayerObject.transform)
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
                field.SetValue(copy, field.GetValue(original));
            return copy;
        }
        Animation GetBikeModelAnim(GameObject PlayerObject)
        {
            foreach (Animation a in FindObjectsOfType<Animation>())
                if (a.name == "BikeModel" && a.transform.root == PlayerObject.transform)
                    return a;
            return null;
        }
    }
}