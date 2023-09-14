using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;

namespace ModLoaderSolution
{
    public class SlipModifier : MonoBehaviour
    {
        // gY\u007f\u0083VF\u0084 = baseSlip
        // [JmOrvR = skiddingSlip
        public float slipAim = 2f;
        public float skidSlipAim = 0.2f;
        private float origSlip;
        private float origSkidSlip;
        private void OnTriggerEnter(Collider col)
        {
            Ice(col, state: true);
        }
        private void OnTriggerExit(Collider col)
        {
            Ice(col, state: false);
        }
        private void Update()
        {
            if (origSlip == 0f && Singleton<PlayerManager>.SP.GetPlayerImpact() != null && Singleton<PlayerManager>.SP.GetPlayerImpact().bike != null)
            {
                origSlip = (float)Singleton<PlayerManager>.SP.GetPlayerImpact()
                    .bike.GetType().GetField("gY\u007f\u0083VF\u0084")
                    .GetValue(Singleton<PlayerManager>.SP.GetPlayerImpact().bike);
                origSkidSlip = (float)Singleton<PlayerManager>.SP.GetPlayerImpact()
                    .bike.GetType().GetField("[JmOrvR")
                    .GetValue(Singleton<PlayerManager>.SP.GetPlayerImpact().bike);
            }
        }
        private void Ice(Collider col, bool state)
        {
            PlayerInfoImpact playerFromCollider = Singleton<PlayerManager>.SP.GetPlayerFromCollider(col, alsoCheckRagdoll: true);
            if (playerFromCollider != null)
            {
                if (state)
                {
                    playerFromCollider.bike.GetType()
                        .GetField("gY\u007f\u0083VF\u0084")
                        .SetValue(playerFromCollider.bike, slipAim);
                    playerFromCollider.bike.GetType()
                        .GetField("[JmOrvR")
                        .SetValue(playerFromCollider.bike, skidSlipAim);
                }
                else
                {
                    playerFromCollider.bike
                        .GetType().GetField("gY\u007f\u0083VF\u0084")
                        .SetValue(playerFromCollider.bike, origSlip);
                    playerFromCollider.bike
                        .GetType().GetField("[JmOrvR")
                        .SetValue(playerFromCollider.bike, origSkidSlip);
                }
            }
        }
    }
}