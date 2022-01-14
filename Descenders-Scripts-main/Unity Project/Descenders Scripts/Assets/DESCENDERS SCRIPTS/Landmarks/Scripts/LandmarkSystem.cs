using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using UnityEngine.UI;
namespace Landmarks{
	public class LandmarkSystem : ModBehaviour {
		public GameObject descriptions;
		public Object DescriptionTemplate;
		public Object LandmarkTemplate;
		public GameObject LandmarksContentView;
		public Texture2D lockedTexture;
		// Use this for initialization
		void Start () {
			FormDescription();
		}

		public void FormDescription(){
			Debug.Log("Forming edscip");
			Landmark[] landmarks = GameObject.FindObjectsOfType<Landmark>();
			foreach (Transform description in descriptions.transform){
				Destroy(description.gameObject);
			}
			foreach (Transform landmarkUi in LandmarksContentView.transform){
				Destroy(landmarkUi.gameObject);
			}
			foreach(Landmark landmark in landmarks){
				GameObject instantiatedObj = (GameObject)Instantiate(DescriptionTemplate);
				instantiatedObj.transform.SetParent(descriptions.transform);
				LandmarkDescription landmarkDescription = instantiatedObj.GetComponent<LandmarkDescription>();
				if (landmark.landmarkLocked){
					landmarkDescription.description.text = "";
					landmarkDescription.title.text = landmark.landmarkTitle;
					landmarkDescription.PurchaseTitle.text = "Purchase for\n" + landmark.priceOfLandmark.ToString() + " Units";
					landmarkDescription.PurchaseButton.onClick.AddListener(delegate(){landmark.UnlockCheckmark();});
					landmarkDescription.image.gameObject.SetActive(false);
					landmarkDescription.gameObject.GetComponent<RawImage>().texture = lockedTexture;
					landmarkDescription.gameObject.SetActive(false);	
				}
				else{
					landmarkDescription.description.text = landmark.landmarkDesc;
					landmarkDescription.title.text = landmark.landmarkTitle;
					landmarkDescription.PurchaseTitle.text = "Purchase for\n" + landmark.priceOfLandmark.ToString() + " Units";
					landmarkDescription.PurchaseButton.gameObject.SetActive(false);
					landmarkDescription.image.texture = landmark.picture;
					landmarkDescription.gameObject.SetActive(false);	
				}
				GameObject landmarkObj = (GameObject)Instantiate(LandmarkTemplate);
				landmarkObj.transform.SetParent(LandmarksContentView.transform);
				landmarkObj.GetComponent<Button>().onClick.AddListener(delegate(){this.ShowDescription(instantiatedObj);});
				landmarkObj.GetComponentInChildren<Text>().text = landmark.landmarkTitle;
			}
		}
		
		// Update is called once per frame
		void Update () {
			
		}
		public void ShowDescription(GameObject desc){
			foreach (Transform x in descriptions.transform){
				Debug.Log(x.name);
				x.gameObject.SetActive(false);
			}
			desc.SetActive(true);
		}
	}
}