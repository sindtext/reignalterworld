using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PortalCtrl : MonoBehaviour
{
    int density = 4;
    public float spawnTimer;
    public GameObject[] villainObj;

    public NetworkRunner _runner;
    // Start is called before the first frame update
    private void FixedUpdate()
    {
        if(spawnTimer > 0)
        spawnTimer -= Time.deltaTime;
    }

    // Update is called once per frame
    public void spawnVillain(NetworkRunner runner)
    {
        if (spawnTimer > 0)
            return;

        _runner = runner;
        int vCount = 0;
        Villain[] villainCheck = FindObjectsOfType<Villain>();
        foreach (Villain vlnC in villainCheck)
        {
            if (Vector3.Distance(vlnC.transform.position, transform.position) < 64 && vlnC.gameObject.activeInHierarchy)
            {
                vCount++;
            }
        }

        if (vCount > density)
            return;

        foreach (GameObject obj in villainObj)
        {
            Villain villain = obj.GetComponent<Villain>();
            if ((villain.vType[villain.vIndex] != "Coward" && Random.Range(0, 100) <= 16) || (villain.vType[villain.vIndex] == "Coward" && Random.Range(0, 100) <= GameManager.gm.animalRatio && !DataServer.call.isBHMode))
            {
                villain = runner.Spawn(obj, transform.position, Quaternion.identity, runner.LocalPlayer).GetComponent<Villain>();
                villain.brith(transform);
            }
        }
    }
}
