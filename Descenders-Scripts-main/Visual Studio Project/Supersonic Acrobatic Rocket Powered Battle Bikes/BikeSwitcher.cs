using UnityEngine;
using UnityEngine.UI;

namespace RocketLeagueMod
{
    class BikeSwitcher : MonoBehaviour
    {
        GameObject hugeSlippenis;
        public void Start()
        {
            GameObject slippyObj = GameObject.Find("WithSlippynes");
            Debug.Log("Found WithSlippynes");
            hugeSlippenis = GameObject.Find("HugeSlippenis");
            Debug.Log("Found HugeSlippenis");
            hugeSlippenis.AddComponent<IceVolume>();
            Debug.Log("Added component to hugeSlippenis");
            hugeSlippenis.GetComponent<IceVolume>().enabled = false;
            Debug.Log("Added component to hugeSlippenis");
            slippyObj.AddComponent<IceVolume>();
            Debug.Log("Added component to slippyObj");
        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                hugeSlippenis.SetActive(!hugeSlippenis.activeInHierarchy);
            }
        }
        public void ToEnduro()
        {
            GetComponent<Utilities>().SetBike(0);
        }
        public void ToDowhill()
        {
            GetComponent<Utilities>().SetBike(1);
        }
        public void ToHardtail()
        {
            GetComponent<Utilities>().SetBike(2);
        }
    }
}
