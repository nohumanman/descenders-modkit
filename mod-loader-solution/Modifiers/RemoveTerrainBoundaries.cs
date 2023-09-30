using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIdentification;
using ModLoaderSolution;

namespace ModLoaderSolution
{
	public class RemoveTerrainBoundaries : MonoBehaviour
	{
		public void Start()
        {
			Terrain activeTerrain = Terrain.activeTerrain;
			Terrain component = Instantiate(activeTerrain.gameObject).GetComponent<Terrain>();
			TerrainData terrainData = Instantiate(activeTerrain.terrainData);
			component.terrainData = terrainData;
			component.gameObject.GetComponent<TerrainCollider>().terrainData = terrainData;
			activeTerrain.terrainData.size = (new Vector3(300000f, activeTerrain.terrainData.size.y, 300000f));
			activeTerrain.drawHeightmap = false;
			activeTerrain.drawTreesAndFoliage = false;
			Vector3 size = activeTerrain.terrainData.size;
			((Component)activeTerrain).transform.position = new Vector3((0f - size.x) / 2f, 0f, (0f - size.z) / 2f);
			((Collider)((Component)activeTerrain).gameObject.GetComponent<TerrainCollider>()).enabled = false;
		}
	}
}
