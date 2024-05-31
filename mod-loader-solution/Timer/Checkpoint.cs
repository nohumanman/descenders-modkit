using System;
using UnityEngine;
using ModLoaderSolution;
using System.IO;

namespace ModLoaderSolution
{
    public class Checkpoint : MonoBehaviour
    {
        string hash;
        public string GetHash(int minCharAmount, int maxCharAmount)
        {
            string glyphs = "abcdefghijklmnopqrstuvwxyz1234567890";
            int charAmount = UnityEngine.Random.Range(minCharAmount, maxCharAmount); //set those to the minimum and maximum length of your string
            string myString = "";
            for (int i = 0; i < charAmount; i++)
                myString += glyphs[UnityEngine.Random.Range(0, glyphs.Length)];
            return myString;
        }
        public CheckpointType checkpointType;
        public Trail trail;
        public bool doesWork = true;
        public void Start()
        {
            hash = GetHash(20, 50);
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            if (trail.splitsAreCheckpoints)
                this.tag = "Checkpoint";
            else
                this.tag = "Untagged";
            Utilities.Log("Checkpoint | Checkpoint script added to " + this.gameObject.name);
        }
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.U))
                gameObject.GetComponent<MeshRenderer>().enabled = !gameObject.GetComponent<MeshRenderer>().enabled;
        }
        public void LogCheckpointToFile()
        {
            // log to LocalLow > RageSuid > Descenders > checkpoint-logs.txt
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low\\RageSquid\\Descenders\\checkpoint-logs.txt";
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine("SplitTimer.Checkpoint | " + DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss.fff") + " - checkpoint '" + this.name + "' entered, elapsed time: " + (SplitTimerText.Instance.finalTime - SplitTimerText.Instance.timeStart).ToString());
            writer.Close();
        }
        void OnTriggerEnter(Collider other)
        {
            // check if our other.transform.name is Bike so we're actually looking at the bike not arm or something
            if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
            {
                LogCheckpointToFile();
                if (Utilities.instance.isInReplayMode())
                    return;
                // if doesn't work or stats not default
                if (!StatsModification.instance.IfStatsAreDefault())
                {
                    SplitTimerText.Instance.StopTimer();
                    return;
                }
                PlayerManagement.Instance.OnCheckpointEnter(trail.gameObject.name, checkpointType.ToString(), trail.checkpointList.Count, (SplitTimerText.Instance.finalTime - SplitTimerText.Instance.timeStart).ToString(), hash);
                SplitTimerText.Instance.hidden = false;
                if (this.checkpointType == CheckpointType.Start)
                {
                    Utilities.instance.RestartReplay();
                    NetClient.Instance.SendData("START_SPEED", PlayerManagement.Instance.speed);
                    SplitTimerText.Instance.RestartTimer(this.trail);
                    //NetClient.Instance.gameObject.GetComponent<Utilities>().SetVel(5);
                }
                else if (this.checkpointType == CheckpointType.Finish)
                {
                    Utilities.instance.SaveReplayToFile("tmp");
                    SplitTimerText.Instance.StopTimer();
                }
            }
        }
    }
    public enum CheckpointType
    {
        Start, Finish, Intermediate
    }
}
