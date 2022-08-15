using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryLinker : MonoBehaviour {
    public GameObject boundaryLinks;
    public GameObject boundaries;
    public GameObject boundary;
    public Vector3 scaleOfBoundaryJoin = new Vector3(5, 50, 5);
    List<GameObject> boundaryPoints = new List<GameObject>();
    List<GameObject> resentlySpawnedBoundaryPoints = new List<GameObject>();
    public void SpawnBoundaries(){
        boundaryPoints.Clear();
        resentlySpawnedBoundaryPoints.Clear();
        foreach(Transform child in boundaryLinks.transform)
            boundaryPoints.Add(child.gameObject);
        int i = 0;
        
		foreach(GameObject boundaryPoint in boundaryPoints){
            if (i+1 < boundaryPoints.Count)
                SpawnBoundary(boundaryPoints[i], boundaryPoints[i+1]);
            i++;
        }
    }
    public void ReverseLinks(){
        List<Transform> linksList = new List<Transform>();
        foreach(Transform child in boundaryLinks.transform){
            linksList.Add(child);
        }
        linksList.Reverse();
        int i = 0;
        foreach(Transform child in linksList){
            child.SetSiblingIndex(i);
            i++;
        }
    }
    public void UndoBoundarySpawn(){
        foreach(GameObject x in resentlySpawnedBoundaryPoints)
            DestroyImmediate(x);
    }
    public void SpawnBoundary(GameObject from, GameObject to){
        // Must be run on a unit cube.
        Debug.Log("Creating boundary between " + from.name + " to " + to.name);
        Vector3 center = from.transform.position - to.transform.position;
        float distance = center.magnitude;
        Vector3 posToSpawn = from.transform.position - (center / 2);
        GameObject boundaryInstance = Instantiate(boundary);
        boundaryInstance.transform.position = posToSpawn;
        Vector3 localScale = boundaryInstance.transform.localScale;
        boundaryInstance.transform.localScale = new Vector3(
            localScale.x,
            localScale.y,
            distance
        );
        boundaryInstance.transform.LookAt(to.transform);
        GameObject boundaryInstanceAtJoin = Instantiate(boundary);
        boundaryInstanceAtJoin.transform.position = from.transform.position;
        boundaryInstanceAtJoin.transform.localScale = scaleOfBoundaryJoin;
        boundaryInstanceAtJoin.transform.LookAt(boundaryInstance.transform);
        boundaryInstanceAtJoin.transform.eulerAngles = new Vector3(
            0,
            boundaryInstanceAtJoin.transform.eulerAngles.y,
            0
        );

        boundaryInstance.transform.SetParent(boundaries.transform);
        boundaryInstanceAtJoin.transform.SetParent(boundaries.transform);
        resentlySpawnedBoundaryPoints.Add(boundaryInstanceAtJoin);
        resentlySpawnedBoundaryPoints.Add(boundaryInstance);
    }
}
