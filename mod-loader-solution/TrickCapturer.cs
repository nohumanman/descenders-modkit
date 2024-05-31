using System;
using UnityEngine;
using System.Collections.Generic;
using ModLoaderSolution;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine.UI;
using ModTool.Interface;
using ModTool.Shared;
using ModTool;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using TMPro;
using InControl;

namespace ModLoaderSolution
{
    public class TrickCapturer : MonoBehaviour
    {
        public Utilities utilities;
        string oldTrick = "";
        public void Start()
        {
            utilities = gameObject.GetComponent<Utilities>();
        }
        public void Update()
        {
            Utilities.LogMethodCallStart();
            string trick = utilities.GetPlayerTrick();
            if (trick != oldTrick && trick != "")
            {
                NetClient.Instance.SendData("TRICK", trick);
                oldTrick = trick;
            }
            Utilities.LogMethodCallEnd();
        }
    }
}
