using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Reflection;
using System.IO;

namespace ModLoaderSolution
{
    public class CustomDebug : MonoBehaviour {
         
        public void LogAll()
        {
            string path = (
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                + "Low\\RageSquid\\Descenders\\modkit-debug.txt"
            );
            File.WriteAllText(path, "");
            string data = "";
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                data += "\n" + obj.name;
                foreach (Component comp in obj.GetComponents<Component>())
                {
                    if (comp != null)
                    {
                        data += "\n   -> " + obj.name + "." + comp.ToString();
                        try
                        {
                            PropertyInfo[] properties = comp.GetType().GetProperties();
                            foreach (PropertyInfo pI in properties)
                                try
                                {
                                    data += "\n        -> " + obj.name + "." + comp.ToString() + "." + pI.Name + " = " + pI.GetValue(comp, null);
                                }
                                catch
                                {
                                    data += "\n        -> Unfetchable '" + pI.Name + "'";
                                }
                        }
                        catch
                        {
                            data += "\n unfetchable ";
                        }
                    }
                    File.AppendAllText(path, data);
                    data = "";
                }
            }
        }
        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha0) && Input.GetKeyDown(KeyCode.O))
                LogAll();
        }
    }
}
