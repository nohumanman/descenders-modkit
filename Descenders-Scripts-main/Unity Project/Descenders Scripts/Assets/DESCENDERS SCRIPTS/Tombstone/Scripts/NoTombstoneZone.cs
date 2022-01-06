using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

namespace TombstoneSystem{
	public class NoTombstoneZone : ModBehaviour {
		void OnTriggerEnter(Collider col) 
		{
			if (col.gameObject.GetComponent<TombstoneSystem.Tombstone>() != null){
				Destroy(col.gameObject);
			}
		}
	}
}