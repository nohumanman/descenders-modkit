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
            GameObject encompassingCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            encompassingCube.name = "EncompassingCube";
            encompassingCube.transform.localScale = new Vector3(1, 1, 1) * 50000;
            encompassingCube.AddComponent<IceVolume>();
        }
        public void DisableIce()
        {
            encompassingCube.SetActive(false);
        }
        public void EnableIce()
        {
            encompassingCube.SetActive(true);
        }
    }
}
