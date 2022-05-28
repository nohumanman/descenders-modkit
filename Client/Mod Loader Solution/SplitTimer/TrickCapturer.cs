using System;
using UnityEngine;
using System.Collections.Generic;
using ModLoaderSolution;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;
using ModTool.Interface;
using ModTool.Shared;
using ModTool;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using TMPro;
using InControl;

namespace SplitTimer
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
            string trick = utilities.GetPlayerTrick();
            if (trick != oldTrick && trick != "")
            {
                NetClient.Instance.SendData("TRICK|" + trick);
                oldTrick = trick;
            }
        }
    }
}
