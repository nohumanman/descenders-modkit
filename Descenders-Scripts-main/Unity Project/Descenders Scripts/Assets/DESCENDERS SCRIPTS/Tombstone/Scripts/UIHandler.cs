using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
namespace TombstoneSystem{
	public class UIHandler : ModBehaviour {
		public CanvasGroup canvasGroup;
		
		void Start(){
			canvasGroup.alpha = 0f;
		}

		IEnumerator HideUI(){
			while (true){
				canvasGroup.alpha -= 0.01f;
				yield return new WaitForSeconds(0.001f);
				if (canvasGroup.alpha == 0){
					break;
				}
			}
		}

		IEnumerator ShowUI(){
			while (true){
				canvasGroup.alpha += 0.01f;
				yield return new WaitForSeconds(0.001f);
				if (canvasGroup.alpha == 1){
					break;
				}
				Cursor.visible = true;
			}
		}

		void Update () {
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.E)){
				if (canvasGroup.alpha == 0f){
					StartCoroutine(ShowUI());
				}
				else if (canvasGroup.alpha == 1f){
					StartCoroutine(HideUI());
				}
				Cursor.visible = false;
			}
		}
	}
}