using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using UnityEngine.UI;
using System.Text.RegularExpressions; // Regex
public class TimerCopier : ModBehaviour
{
    public Text textFrom;
    public Text textTo;
    void Start()
    {
        DontDestroyOnLoad(this.gameObject.transform.root);
        StartCoroutine(DelayedStart());
    }
    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1f);
        textFrom = FindObjectOfType<ModLoaderSolution.SplitTimerText>().text;
        textTo = this.gameObject.GetComponent<Text>();
    }
    public string RemoveHTMLTags(string input)
    {
        // remove all <tags></tags>
        return Regex.Replace(input, "<.*?>", string.Empty);
    }
    public void LateUpdate()
    {
        // textFrom.text can have '<color=red>text</color>', but needs to be 'text'
        if (textTo != null && textFrom != null)
            textTo.text = RemoveHTMLTags(textFrom.text); // remove the colour fields so the shadow is black
    }
}
