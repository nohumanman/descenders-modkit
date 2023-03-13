using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;
using SplitTimer;
using System;

namespace SplitTimer
{
    class Flags : MonoBehaviour
    {
        List<GameObject> spawned = new List<GameObject>();
        public void SpawnFlags()
        {
            GameObject banner1 = AssetBundling.Instance.bundle.LoadAsset<GameObject>("nohumanman_banner");
            GameObject banner2 = AssetBundling.Instance.bundle.LoadAsset<GameObject>("desc_comp_banner");
            GameObject banner1Inst = Instantiate(banner1);
            spawned.Add(banner1Inst);
            banner1Inst.transform.position = new Vector3(250, 0, 234);
            banner1Inst.transform.eulerAngles = new Vector3(0, -38, 0);
            GameObject banner2Inst = Instantiate(banner2);
            banner2Inst.transform.position = new Vector3(251, 0, 259);
            banner2Inst.transform.eulerAngles = new Vector3(0, 48, 0);
            spawned.Add(banner2Inst);            
        }
        void Update()
        {
            try
            {
                foreach (GameObject obj in spawned)
                    if (obj == null)
                        spawned.Remove(obj);
            }
            catch (InvalidOperationException){ }
            if (spawned.Count == 0 && Utilities.instance.GetCurrentMap() == "0" && AssetBundling.Instance.bundle != null)
                SpawnFlags();
        }
    }
}
