using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

namespace CustomUi {
	public class UI : ModBehaviour {
		public GameObject map;
		public GameObject menu;
		public GameObject help;
		public GameObject BikeSwitcher;
		public Section currentSection;
		public Animator blackTint;
		private static UI _instance;
    	public static UI Instance { get { return _instance; } }
		private void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(this.gameObject);
			}
			else {
				_instance = this;
			}
		}
		public enum Section{
			Map, Quests, Landmarks, Help, BikeSwitcher
		}
		
		public bool isShowing;
		void Start(){
			if (map.activeInHierarchy || menu.activeInHierarchy || help.activeInHierarchy){
				isShowing = true;
			}
			else{
				isShowing = false;
			}
		}
		bool slideMenuOpen = false;
		
		public void SlideMenu(Animator animator){
			if (animator.GetCurrentAnimatorStateInfo(0).IsName("OpenMenu")){
				animator.Play("CloseMenu");
				blackTint.Play("CloseMenu");
				slideMenuOpen = false;
			}
			else{
				if (!slideMenuOpen){
					slideMenuOpen = true;
					animator.Play("OpenMenu");
					blackTint.Play("OpenMenu");
				}
				else{
					slideMenuOpen = false;
					animator.Play("CloseMenu");
					blackTint.Play("CloseMenu");
				}
			}
		}

		void Update () {
			if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M)){
				if (!isShowing){
					if (currentSection == Section.Map){
						GoMap();
					}
					else if (currentSection == Section.Help){
						GoHelp();
					}
					else{
						GoMap();
					}
				}
				else{
					DisableUI();
				}
			}
			if (Input.GetKeyDown(KeyCode.Escape)){
				DisableUI(!Cursor.visible);				
			}
			if (Input.GetKeyDown(KeyCode.Tab)){
				GoSwitcher();
			}
			else if (Input.GetKeyUp(KeyCode.Tab)){
				DisableUI();
			}
		}

		public void DisableUI(bool disableCursor = true){
			cameraViewingTerrain.gameObject.SetActive(false);
			map.SetActive(false);
			menu.SetActive(false);
			help.SetActive(false);
			BikeSwitcher.SetActive(false);
			Cursor.visible = !disableCursor;
			isShowing = false;
			blackTint.Play("CloseMenu");
		}

		public void EnableUI(){
			isShowing = true;
			Cursor.visible = true;
		}

		public Camera cameraViewingTerrain;
		public void GoMap(){
			DisableUI();
			currentSection = Section.Map;
			map.SetActive(true);
			menu.SetActive(true);
			cameraViewingTerrain.gameObject.SetActive(false);
			cameraViewingTerrain.gameObject.SetActive(true);
			EnableUI();
		}
		public void GoHelp(){
			DisableUI();
			currentSection = Section.Help;
			help.SetActive(true);
			menu.SetActive(true);
			EnableUI();
		}

		public void GoSwitcher(){
			DisableUI();
			currentSection = Section.BikeSwitcher;
			BikeSwitcher.SetActive(true);
			EnableUI();
		}
	}
}