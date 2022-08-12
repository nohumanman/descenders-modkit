using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

public class DiscordInvite : ModBehaviour {
    public string url = "https://discord.com/invite/aqwnkgSxPQ";
    public GameObject[] enableWhenOver;
    public GameObject[] disableWhenOver;
    bool mouseOver = false;
    public void SetActiveAll(GameObject[] objs, bool isActive){
        foreach(GameObject obj in objs)
            obj.SetActive(isActive);
    }
    void Start(){
        OnMouseExit();
    }
    void Update(){
        if (mouseOver && Input.GetKeyDown(KeyCode.Mouse0))
            Application.OpenURL(url);
    }
    void OnMouseOver(){
        mouseOver = true;
        SetActiveAll(enableWhenOver, true);
        SetActiveAll(disableWhenOver, false);
    }
    void OnMouseExit(){
        mouseOver = false;
        SetActiveAll(enableWhenOver, false);
        SetActiveAll(disableWhenOver, true);
    }
}
