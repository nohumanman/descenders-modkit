using System.Collections;
using System.Collections.Generic;
using ModTool.Interface;
using UnityEngine;

namespace CustomTimer
{
    [System.Serializable]
    public class TimerTrigger : ModBehaviour
    {
        public TriggerType triggerType;

        public Timer timerVar;

        private void OnTriggerStay(Collider other)
        {
            if (other.transform.root.name == "Player_Human")
            {
                if (triggerType == TriggerType.Start)
                    timerVar.ResetTimer();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.root.name == "Player_Human")
            {
                if (triggerType == TriggerType.Start)
                    timerVar.StartTimer();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.name == "Player_Human")
            {
                if (triggerType == TriggerType.Checkpoint)
                    timerVar.Checkpoint(this);
                else if (triggerType == TriggerType.Stop)
                    timerVar.StopTimer("Stopped for unknown reason.");
                else if (triggerType == TriggerType.Finish)
                    timerVar.FinishTimer();
                else if (triggerType == TriggerType.Pause){
                    timerVar.PauseTimer();
                    timerVar.Checkpoint(this);
                }
                else if (triggerType == TriggerType.Resume){
                    timerVar.ResumeTimer();
                    timerVar.Checkpoint(this);
                }
            }
        }
    }

    public enum TriggerType
    {
        Start, Checkpoint, Stop, Finish, Pause, Resume
    }
}