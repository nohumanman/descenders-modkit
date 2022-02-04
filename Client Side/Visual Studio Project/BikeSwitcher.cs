using UnityEngine;
using UnityEngine.UI;

namespace DescendersSplitTimer
{
    class BikeSwitcher : MonoBehaviour
    {
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
