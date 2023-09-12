using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
public class BikeSwitcherEnabler : ModBehaviour {
    public GameObject obj;
    void Update () {
        if (Input.GetKeyDown(KeyCode.CapsLock)){
            obj.SetActive(true);
            Cursor.visible = true;
        }   
        if (Input.GetKeyUp(KeyCode.CapsLock)){
            obj.SetActive(false);
            Cursor.visible = false;
        }
    }
}
