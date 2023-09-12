using UnityEngine;
using ModTool.Interface;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class TeleportElement : ModBehaviour, IPointerEnterHandler {
    public GameObject aim;
    public Camera orthoCam;
    public GameObject preemptiveSpawnPoint;
    void Start(){
        preemptiveSpawnPoint.SetActive(false);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        preemptiveSpawnPoint.gameObject.SetActive(true);
        SetPositionFromGameobjectOverTerrain(
            aim,
            GetComponentInParent<Canvas>().GetComponent<RectTransform>(),
            preemptiveSpawnPoint.GetComponent<RectTransform>()
        );
        preemptiveSpawnPoint.GetComponentInChildren<Text>().text = name;
    }
    public void OnPointerExit(){
        preemptiveSpawnPoint.gameObject.SetActive(false);
    }
    public void SetPositionFromGameobjectOverTerrain(GameObject player, RectTransform canvasRectTransform, RectTransform thisRectTransform){
        Vector2 ViewportPosition = orthoCam.WorldToViewportPoint(
            player.transform.position
        );
        Vector2 WorldObject_ScreenPosition=new Vector2(
            (
                (ViewportPosition.x * canvasRectTransform.sizeDelta.x)
                -
                (canvasRectTransform.sizeDelta.x * 0.5f)
            ),
            (
                (ViewportPosition.y * canvasRectTransform.sizeDelta.y)
                -
                (canvasRectTransform.sizeDelta.y * 0.5f)
            )
        );
        thisRectTransform.anchoredPosition =  WorldObject_ScreenPosition;
    }
}