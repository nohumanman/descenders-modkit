using System.Collections;
using System.Collections.Generic;
using ModTool.Interface;
using UnityEngine;

public class Booster : ModBehaviour
{
    //[Tooltip("10 is considered weak boost, 20 = moderate")]
    public enum BoostSettingEnum 
    { 
        Weak = 0,
        Moderate = 1,
        Strong = 2,
        Extreme = 3
    };
    public BoostSettingEnum BoostPreset;

    public int BoostIntensity = 10;
    public float BoostDuration = 1;
    public bool CustomSettings;
    public bool DebugView;
    public GameObject m_Object;
    public float distanceFromBoost;
    public bool triggered = false;
    public float SmoothedBoost;
    public float BoostTimer;

    void Start()
    {
        if (!CustomSettings)
        {
            if (BoostPreset == BoostSettingEnum.Weak)
            {
                BoostIntensity = 10;
                BoostDuration = 1;
            }

            if (BoostPreset == BoostSettingEnum.Moderate)
            {
                BoostIntensity = 20;
                BoostDuration = 1;
            }

            if (BoostPreset == BoostSettingEnum.Strong)
            {
                BoostIntensity = 30;
                BoostDuration = 1;
            }

            if (BoostPreset == BoostSettingEnum.Extreme)
            {
                BoostIntensity = 40;
                BoostDuration = 1;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.name == "Player_Human")
        {
            if (m_Object == null && triggered == false)
            {
                triggered = true;
                m_Object = other.gameObject;
            }
        }
    }

void FixedUpdate()
    {

        if (m_Object)
        {
            distanceFromBoost = Vector3.Distance(m_Object.transform.position, this.transform.position);

        }
        if (triggered)
        {
            BoostTimer += Time.deltaTime;
            BoostTimer = Mathf.Clamp(BoostTimer, 0, BoostDuration);
            SmoothedBoost += Time.deltaTime* 3;
            SmoothedBoost = Mathf.Clamp(SmoothedBoost, 0, 1);

            if (m_Object)
            foreach (Rigidbody rBody in m_Object.transform.root.GetComponentsInChildren<Rigidbody>())
            {
                rBody.AddForce(this.transform.right * ((rBody.mass * BoostIntensity) * -1)* SmoothedBoost, ForceMode.Force);
            }
        }


        if (triggered && distanceFromBoost > 1 && BoostTimer >= BoostDuration)
        {
            StartCoroutine(BoostExit());
        }
    }

    IEnumerator BoostExit()
    {
        yield return new WaitForSeconds(0);
        m_Object = null;
        triggered = false;
        SmoothedBoost = 0;
        BoostTimer = 0;
    }

}
