using UnityEngine;
using System.Collections;


namespace ModLoaderSolution
{
    public class ThreeDTimer : MonoBehaviour
    {
        public TextMesh textMesh;
        public void Start()
        {
            textMesh = GetComponent<TextMesh>();
        }
        void Update()
        {
            if (SplitTimerText.Instance != null)
                textMesh.text = SplitTimerText.Instance.FormatTime(Time.time - SplitTimerText.Instance.timeStart);
        }
    }
}
