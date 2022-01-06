using UnityEngine;

namespace RocketLeagueMod
{
    class NetworkedGameObject : MonoBehaviour
    {
        public Utilities utilities;
        public UDPClient udpClient;
        public bool IsHost = true;
        public string UniqueIdentifier = "";

        void Start()
        {
            Debug.Log("Unique Identifier is: " + UniqueIdentifier);
            udpClient = Loader.gameObject.GetComponent<UDPClient>();
            utilities = Loader.gameObject.GetComponent<Utilities>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                IsHost = !IsHost;
            }
        }

        void FixedUpdate()
        {
            if (IsHost)
            {
                UpdateValues();
            }
            else
            {
                StartCoroutine(udpClient.SendData("GET|" + UniqueIdentifier));
            }
        }

        public void UpdateValuesFromString(string newVals)
        {
            //Debug.Log("Updating Values");
            
            string[] thingy = newVals.Split('|');
            float xPos = float.Parse(thingy[1]);
            float yPos = float.Parse(thingy[2]);
            float zPos = float.Parse(thingy[3]);
            float xRot = float.Parse(thingy[4]);
            float yRot = float.Parse(thingy[5]);
            float zRot = float.Parse(thingy[6]);
            float xVel = float.Parse(thingy[7]);
            float yVel = float.Parse(thingy[8]);
            float zVel = float.Parse(thingy[9]);
            this.gameObject.transform.position = new Vector3(xPos, yPos, zPos);
            this.gameObject.transform.rotation = Quaternion.Euler(new Vector3(xRot, yRot, zRot));
            this.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(xVel, yVel, zVel);
        }

        void UpdateValues()
        {
            string xPos = this.gameObject.transform.position.x.ToString();
            string yPos = this.gameObject.transform.position.y.ToString();
            string zPos = this.gameObject.transform.position.z.ToString();
            string xRot = this.gameObject.transform.rotation.x.ToString();
            string yRot = this.gameObject.transform.rotation.y.ToString();
            string zRot = this.gameObject.transform.rotation.z.ToString();
            string xVel = this.gameObject.GetComponent<Rigidbody>().velocity.x.ToString();
            string yVel = this.gameObject.GetComponent<Rigidbody>().velocity.y.ToString();
            string zVel = this.gameObject.GetComponent<Rigidbody>().velocity.z.ToString();
            StartCoroutine(udpClient.SendData("UPDATE|" + UniqueIdentifier + "|" + xPos + "|" + yPos + "|" + zPos + "|" + xRot + "|" + yRot + "|" + zRot + "|" + xVel + "|" + yVel + "|" + zVel));
        }
    }
}
