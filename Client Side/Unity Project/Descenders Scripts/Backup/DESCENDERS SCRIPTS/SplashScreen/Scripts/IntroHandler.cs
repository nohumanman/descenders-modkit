using System.Collections;
using UnityEngine;
using ModTool.Interface;

public class IntroHandler : ModBehaviour {
	public GameObject VideoPlayer;
	public CanvasGroup canvasGroup;
	bool isLoaded = false;

	IEnumerator FadeToBlack(){
		while (canvasGroup.alpha != 1){
			canvasGroup.alpha += 0.1f;
			yield return new WaitForSeconds(0.0001f);
		}
	}

	IEnumerator FadeToNone(){
		while (canvasGroup.alpha != 0){
			canvasGroup.alpha -= 0.01f;
			yield return new WaitForSeconds(0.01f);
		}
	}

	IEnumerator ExitSplashScreen(){
		yield return FadeToBlack();
		VideoPlayer.SetActive(false);
		yield return new WaitForSeconds(1f);
		yield return FadeToNone();
	}

	void Update(){
		if (Input.anyKeyDown && !isLoaded){
			StartCoroutine(ExitSplashScreen());
			isLoaded = true;
		}
	}
}
