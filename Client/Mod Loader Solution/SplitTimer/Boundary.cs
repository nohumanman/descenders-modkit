using System;
using UnityEngine;
using System.Collections;


namespace SplitTimer
{
    public class Boundary : MonoBehaviour
    {
        string boundaryHash;
        public Trail trail;
        bool inBoundary = false;
        bool notifiedServer = false;
        public string GetHash(int minCharAmount, int maxCharAmount)
        {
            string glyphs = "abcdefghijklmnopqrstuvwxyz1234567890";
            int charAmount = UnityEngine.Random.Range(minCharAmount, maxCharAmount); //set those to the minimum and maximum length of your string
            string myString = "";
            for (int i = 0; i < charAmount; i++)
            {
                myString += glyphs[UnityEngine.Random.Range(0, glyphs.Length)];
            }
            return myString;
        }
        public void Start()
        {
            boundaryHash = GetHash(20, 50);
            Debug.Log("Boundary | Boundary added to " + this.gameObject.name);
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.transform.name == "bicycleDude_Rig_V02_Slave_R_Elbow" && other.transform.root.name == "Player_Human")
            {
                other.gameObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
                PlayerInfo.Instance.OnBoundryEnter(
                    trail.name,
                    boundaryHash
                );
                notifiedServer = false;
                inBoundary = true;
            }
        }
        IEnumerable OnTriggerStay(Collider other)
        {
            yield return new WaitForFixedUpdate();
            if (other.transform.name == "bicycleDude_Rig_V02_Slave_R_Elbow" && other.transform.root.name == "Player_Human")
            {
                inBoundary = true;
                notifiedServer = false;
            }
        }
        void FixedUpdate()
        {
            if (!inBoundary && !notifiedServer) {
                PlayerInfo.Instance.OnBoundryExit(
                    trail.name,
                    boundaryHash
                );
                notifiedServer = true;
            }
            inBoundary = false;
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.U))
                this.gameObject.GetComponent<MeshRenderer>().enabled = !this.gameObject.GetComponent<MeshRenderer>().enabled;
        }
        void OnTriggerExit(Collider other)
        {
            if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
            {
                PlayerInfo.Instance.OnBoundryExit(
                    trail.name,
                    boundaryHash
                );
            }
        }
    }
}
