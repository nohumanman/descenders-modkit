using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModTool.Interface;

namespace CustomTimer
{
    public class Timer : ModBehaviour
    {
        [Tooltip("This is the bool that makes the checks for teleport and boundaries.")]
        public bool HasSecurity = true;

        [Tooltip("The text that will contain the total time")]
        public Text timerText;
        [Tooltip("The text that will contain the time at a checkpoint")]
        public Text checkpointText;
        [Tooltip("The canvas that contains timerText and checkpointText")]
        public Canvas timerCanvas;
        [System.NonSerialized]
        public List<TimerTrigger> enteredCheckpoints = new List<TimerTrigger>();
        [System.NonSerialized]
        public List<TimerTrigger> totalCheckpoints = new List<TimerTrigger>();
        [System.NonSerialized]
        public List<Boundary> trailBoundariesList = new List<Boundary>();
        [Tooltip("Parent gameobject of all boundaries")]
        public Transform trailBoundaries;
        [Tooltip("Parent gameobject of all checkpoints")]
        public Transform trailCheckpoints;
        private bool started = false;
        private float timeCount;
        private bool shouldIncrement = true;
        private bool literallyFloat = false;

        private Vector3 previousPlayerPosition; // to prevent respawn cheating

        public void Start()
        {
            foreach (Transform child in trailBoundaries)trailBoundariesList.Add(child.GetComponent<Boundary>());
            foreach (Transform child in trailCheckpoints) totalCheckpoints.Add(child.GetComponent<TimerTrigger>());
        }

        public void StartTimer()
        {
            shouldIncrement = true;
            timerCanvas.gameObject.SetActive(true);
            StopCoroutine("DisableUI");
            timeCount = 0.0f;
            timerText.color = Color.white;
            timerText.text = FormatTime(timeCount);
            EnableUI();
            started = true;
        }
        public void ResetTimer()
        {
            if (!started)
            {
                StopCoroutine("DisableUI");
                EnableUI();
            }
            checkpointText.gameObject.SetActive(false);
            enteredCheckpoints.Clear();
            timeCount = 0.0f;
            started = true;
        }

        public void PauseTimer(){
            shouldIncrement = false;
        }
        
        public void ResumeTimer(){
            shouldIncrement = true;
        }

        public void Checkpoint(TimerTrigger trigger)
        {
            if (!started)
                return;
            if (enteredCheckpoints.Contains(trigger))
                return;
            enteredCheckpoints.Add(trigger);
            checkpointText.text = FormatTime(timeCount);
            checkpointText.gameObject.SetActive(true);
            StopCoroutine("HideCheckpointTimer");
            StartCoroutine("HideCheckpointTimer");
        }

        public void StopTimer(string message)
        {
            if (!started)
                return;
            foreach (Boundary boundary in trailBoundariesList)
            {
                boundary.isInside =false; 
            }
            timeCount = 0.0f;
            timerText.color = Color.red;
            timerText.text = message;
            StartCoroutine("DisableUI");
            checkpointText.gameObject.SetActive(false);
            started = false;
        }

        public void FinishTimer()
        {
            if (!started)
                return;
            if (enteredCheckpoints.Count+2 == totalCheckpoints.Count)
            {
                checkpointText.gameObject.SetActive(false);
                StartCoroutine("DisableUI");
                started = false;
                StopTimer("SUBMITTING TIME - CONGRATS!");
                this.GetComponent<Submit>().SubmitTime(FormatTime(timeCount));
            }
            else
                StopTimer("Code 102: Didn't enter all checkpoints");
        }

        private void EnableUI()
        {
            Debug.Log("Enabling UI");
            timerCanvas.gameObject.SetActive(true);
            timerText.color = Color.white;
        }

        IEnumerator DisableUI()
        {
            yield return new WaitForSeconds(5f);
            timerCanvas.gameObject.SetActive(false);
        }

        IEnumerator HideCheckpointTimer()
        {
            yield return new WaitForSeconds(5f);
            checkpointText.gameObject.SetActive(false);
        }

        public void Update(){
            // Handle User Input
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha2)){
                StopTimer("Code 101: Timer Stopped.");
            }
            else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
            {
                // Toggle the checkpoints and the canvas.
                trailCheckpoints.gameObject.SetActive(!trailCheckpoints.gameObject.activeSelf);
                timerCanvas.gameObject.SetActive(!timerCanvas.gameObject.activeSelf);
            }
            else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Slash)){
                literallyFloat = !literallyFloat;
            }
            else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.P)){
                foreach (Transform child in trailBoundaries.transform) child.GetComponent<MeshRenderer>().enabled = !child.GetComponent<MeshRenderer>().enabled;
                foreach (Transform child in trailCheckpoints.transform) child.GetComponent<MeshRenderer>().enabled = !child.GetComponent<MeshRenderer>().enabled;
            }
        }

        public void FixedUpdate()
        {
            if (literallyFloat){
                foreach (Rigidbody rb in GameObject.Find("Player_Human").GetComponentsInChildren<Rigidbody>()){
                    rb.AddForce(new Vector3(0, 350, 0));
                }
            }
            if (started)
            {
                if (shouldIncrement){
                    timeCount += Time.deltaTime;
                }
                timerText.text = FormatTime(timeCount);
                bool anyInside = false;
                foreach (Boundary boundary in trailBoundariesList)
                {
                    if (boundary.isInside) { anyInside = true; break; }
                }
                if (HasSecurity){
                    if (!anyInside)
                    {
                        ResetTimer();
                        StopTimer("Code 102: Out of bounds");
                    }
                    if (Vector3.Distance((GameObject.Find("Player_Human").transform.position), (previousPlayerPosition)) > 5)
                    {
                        StopTimer("Code 102: Respawned");
                    }
                }
                previousPlayerPosition = GameObject.Find("Player_Human").transform.position;
            }
        }

        private string FormatTime(float time)
        {
            int intTime = (int)time;
            int minutes = intTime / 60;
            int seconds = intTime % 60;
            float fraction = time * 1000;
            fraction = (fraction % 1000);
            string timeText = System.String.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction);
            return timeText;
        }
    }
}