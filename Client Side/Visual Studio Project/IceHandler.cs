using UnityEngine;
using UnityEngine.UI;

namespace DescendersSplitTimer
{
    class IceHandler : MonoBehaviour
    {
        public GameObject encompassingCube;

        public void Start()
        {
            Debug.Log("DescendersSplitTimer - IceHandler instantiated!");
            encompassingCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            encompassingCube.GetComponent<BoxCollider>().isTrigger = true;
            encompassingCube.name = "EncompassingCube";
            encompassingCube.transform.localScale = new Vector3(1, 1, 1);
            encompassingCube.AddComponent<IceVolume>();
        }
        public void DisableIce()
        {
            encompassingCube.transform.localScale = new Vector3(1, 1, 1);
        }
        public void EnableIce()
        {
            encompassingCube.transform.localScale = new Vector3(1, 1, 1) * 50000;
        }
    }
}
