using UnityEngine;
using ModTool.Interface;
public class EnableObjOnTab : ModBehaviour {
    public GameObject obj;
    public void Start(){
        obj.SetActive(false);
    }
    public void Update(){
        if (Input.GetKeyDown(KeyCode.Tab)){
            obj.SetActive(true);
            Cursor.visible = true;
        }
        if (Input.GetKeyUp(KeyCode.Tab)){
            obj.SetActive(false);
            Cursor.visible = false;
        }
    }
}