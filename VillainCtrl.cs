using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class VillainCtrl : MonoBehaviour
{
    public GameObject[] dropObj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void SpawnVillain(Vector3 mainPos, int index)
    {
        transform.position = mainPos;
        transform.GetChild(index).GetComponent<PortalCtrl>().spawnVillain(GameManager.gm.character[0].pOne.Runner);
    }

    public void VillainDrop(Vector3 loc, int[] idx)
    {
        for (int i = 0; i < idx.Length; i++)
        {
            Vector3 spawnLoc = loc + Random.insideUnitSphere * 2;
            spawnLoc = new Vector3(spawnLoc.x, 0.1f, spawnLoc.z);
            NetworkObject itemdrop = GameManager.gm.character[0].pOne.Runner.Spawn(dropObj[idx[i]], spawnLoc, Quaternion.identity, GameManager.gm.character[0].pOne.Runner.LocalPlayer);
            itemdrop.GetComponent<ItemDrop>().itemrunner = GameManager.gm.character[0].pOne.netObj.gameObject;
        }
    }
}
