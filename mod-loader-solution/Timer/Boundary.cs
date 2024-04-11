using UnityEngine;
using System.Collections;


namespace ModLoaderSolution
{
    public class Boundary : MonoBehaviour
    {
        string boundaryHash;
        public Trail trail;
        public bool inBoundary = false;
        bool notifiedServerOfExit = true;
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
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            // Utilities.Log("Boundary | Boundary added to " + this.gameObject.name);
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
                    trail.lastBoundryExit = -1f;
                    other.gameObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
                    PlayerManagement.Instance.OnBoundryEnter(
                        trail.name,
                        boundaryHash
                    );
                }
                notifiedServerOfEnter = true;
                notifiedServerOfExit = false;
                inBoundary = true;
            }
        }
        public void ForceUpdate()
        {
            if (inBoundary)
            {
                PlayerManagement.Instance.OnBoundryEnter(
                     trail.name,
                     boundaryHash
                 );
            }
        }
        void FixedUpdate()
        {
            if (!inBoundary && !notifiedServerOfExit) {
                PlayerManagement.Instance.OnBoundryExit(
                    trail.name,
                    boundaryHash,
                    this.gameObject.name
                );
                // if we're in no boundaries, set lastBoundryExit to time
                if (!trail.InAnyBoundaries()){
                    trail.lastBoundryExit = Time.time;
                }
                else{
                    trail.lastBoundryExit = -1; // we're in a boundary
                }
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
