using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModTool.Interface;

namespace CustomCurrencySystem{
	public class BalUi : ModBehaviour {

		public CurrencySystem currencySystem;
		public Text textOfBal;

		void Update () {
			textOfBal.text = "Bal\n" + currencySystem.balance.ToString() + " Units";
		}
	}
}