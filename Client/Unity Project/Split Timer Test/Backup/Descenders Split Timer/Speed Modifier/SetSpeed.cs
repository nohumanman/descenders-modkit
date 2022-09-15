using UnityEngine;
using ModTool.Interface;
public class SetSpeed : ModBehaviour {
    public Vector3 SpeedToSet = new Vector3(0, 0, 0);
    [Tooltip("doesn't do anything")]
    public bool isAdditive;
    [Tooltip("if the SpeedToSet should be in the direction of the forward of this checkpoint")]
    public bool isRelative;
    public void OnTriggerEnter(Collider other)
    {
        if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human"){
            Vector3 _SpeedToSet;
            if (isRelative)
                _SpeedToSet = SpeedToSet.magnitude * transform.forward;
            if (isAdditive)
                _SpeedToSet = SpeedToSet;
            else
                _SpeedToSet = SpeedToSet;
            other.transform.root.SendMessage("SetVelocity", _SpeedToSet);
        }
    }
}