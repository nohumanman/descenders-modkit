using UnityEngine;
using ModTool.Interface;
public class SetSpeed : ModBehaviour {
    public Vector3 SpeedToSet = new Vector3(0, 0, 0);
    [Tooltip("if the SpeedToSet should be in the direction of the forward of this checkpoint")]
    public bool isRelative;
    [Tooltip("Set to true if you want this to only set speed when you respawn.")]
    public bool onlyApplyBoostOnSpawn = false;
    bool usingXbox;
    float timeOfLastRespawn = 0;
    void Update(){
        bool hasPressedB = (
            (Input.GetKeyDown("joystick button 2") && !usingXbox)
            || (Input.GetKeyDown("joystick button 1") && usingXbox)
            || Input.GetKeyDown(KeyCode.R)
        );
        if (hasPressedB)
            timeOfLastRespawn = Time.time;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human"){
            foreach (string name in Input.GetJoystickNames())
                if (name == "Controller (Xbox One For Windows)")
                    usingXbox = true;

            if (!onlyApplyBoostOnSpawn || (onlyApplyBoostOnSpawn && (Time.time - timeOfLastRespawn) < 1)){
                Vector3 _SpeedToSet;
                if (isRelative)
                    _SpeedToSet = SpeedToSet.magnitude * other.transform.root.forward;
                else
                    _SpeedToSet = SpeedToSet;
                other.transform.root.SendMessage("SetVelocity", _SpeedToSet);
            }
        }
    }
}