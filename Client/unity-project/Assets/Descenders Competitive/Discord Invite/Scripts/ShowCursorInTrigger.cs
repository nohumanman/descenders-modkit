using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
public class ShowCursorInTrigger : ModBehaviour {
    void OnTriggerEnter(Collider other){
        if (other.gameObject.transform.root.name == "Player_Human")
            Cursor.visible = true;
    }
    void OnTriggerExit(Collider other){
        if (other.gameObject.transform.root.name == "Player_Human")
            Cursor.visible = false;
    }
}