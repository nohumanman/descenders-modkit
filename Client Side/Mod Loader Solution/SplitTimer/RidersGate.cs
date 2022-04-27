using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplitTimer{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(AudioSource))]
	public class RidersGate : MonoBehaviour {
		public AudioClip beforeOpen;
		public AudioClip onOpen;
		public void TriggerGate(float randomTime){
			StartCoroutine(TriggerGateCoro(randomTime));
		}
		IEnumerator TriggerGateCoro(float randomTime)
        {
			GetComponent<AudioSource>().PlayOneShot(beforeOpen);
			yield return new WaitForSeconds(beforeOpen.length);
			yield return new WaitForSeconds(randomTime);
			GetComponent<AudioSource>().PlayOneShot(onOpen);
			GetComponent<Animator>().Play("Open");
			yield return new WaitForSeconds(5f);
			GetComponent<Animator>().Play("Close");
		}
	}
}