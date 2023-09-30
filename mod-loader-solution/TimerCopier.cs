using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using UnityEngine.UI;
public class TimerCopier : ModBehaviour
{
    public Text textFrom;
    public Text textTo;
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        StartCoroutine(DelayedStart());
    }
    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1f);
        textFrom = FindObjectOfType<ModLoaderSolution.SplitTimerText>().text;
        textTo = this.gameObject.GetComponent<Text>();
    }
    void Update()
    {
        if (textTo != null && textFrom != null)
            textTo.text = textFrom.text;
    }
}
