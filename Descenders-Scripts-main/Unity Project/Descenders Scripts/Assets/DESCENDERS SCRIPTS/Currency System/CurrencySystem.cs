using UnityEngine;
using ModTool.Interface;

namespace CustomCurrencySystem{
	public class CurrencySystem : ModBehaviour {
		public float balance = 0f;
		public bool AddToBal(float intake){
			balance += intake;
			return true;
		}
		public bool TakeFromBal(float outtake){
			if (balance < outtake){
				Debug.LogWarning("Cannot afford this!");
				return false;
			}
			balance -= outtake;
			return true;
		}
	}
}