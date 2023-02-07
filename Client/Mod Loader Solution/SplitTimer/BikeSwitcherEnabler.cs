using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BikeSwitcherEnabler : MonoBehaviour
{
    public GameObject obj;
    public KeyCode key = KeyCode.CapsLock;
    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            obj.SetActive(true);
            Cursor.visible = true;
        }
        if (Input.GetKeyUp(key))
        {
            obj.SetActive(false);
            Cursor.visible = false;
        }
    }
}
