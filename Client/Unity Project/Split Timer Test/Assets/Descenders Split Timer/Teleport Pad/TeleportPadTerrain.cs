using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
public class TeleportPadTerrain : TeleportPad {
    [Header("Terrain optimisation")]
    public GameObject objectToEnable;
    public GameObject[] objectsToDisable;
    public override void TeleportPlayer(GameObject to){
        base.TeleportPlayer(to);
        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(false);
        }
        objectToEnable.SetActive(true);
    }
}