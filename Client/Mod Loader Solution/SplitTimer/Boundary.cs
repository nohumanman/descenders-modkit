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
        bool notifiedServerOfExit = false;
        bool notifiedServerOfEnter = false;
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
        public void OnTriggerStay(Collider other)
        {
            StartCoroutine(OnTriggerStayEnum(other));
        }
        IEnumerator OnTriggerStayEnum(Collider other)
        {
            yield return new WaitForFixedUpdate();
            if (other.transform.name == "bicycleDude_Rig_V02_Slave_R_Elbow" && other.transform.root.name == "Player_Human")
            {
                if (!inBoundary && !notifiedServerOfEnter)
                {
                    other.gameObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
                    PlayerInfo.Instance.OnBoundryEnter(
                        trail.name,
                        boundaryHash
                    );
                }
                notifiedServerOfEnter = true;
                notifiedServerOfExit = false;
                inBoundary = true;
            }
        }
        void FixedUpdate()
        {
            if (!inBoundary && !notifiedServerOfExit) {
                PlayerInfo.Instance.OnBoundryExit(
                    trail.name,
                    boundaryHash
                );
                notifiedServerOfExit = true;
                notifiedServerOfEnter = false;
            }
            inBoundary = false;
        }
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.U))
                this.gameObject.GetComponent<MeshRenderer>().enabled = !this.gameObject.GetComponent<MeshRenderer>().enabled;
        }
    }
}
