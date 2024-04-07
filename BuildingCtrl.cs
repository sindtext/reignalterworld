using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingCtrl : MonoBehaviour
{
    public Vector3[] buildingSpot = new Vector3[6];

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void hideBuilding()
    {
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            for (int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                transform.GetChild(i).GetChild(j).gameObject.SetActive(false);
            }
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void spwanBuilding(Vector3 loc)
    {
        hideBuilding();

        transform.position = loc;
        float spawnLoc = (Vector3.Distance(Vector3.zero, transform.position) / (18 * 28)) % 9;

        if (spawnLoc == 2 && transform.position.x == 0 && transform.position.x > transform.position.z)
        {
            transform.GetChild(6).gameObject.SetActive(true);
            transform.GetChild(6).GetChild(0).gameObject.SetActive(true);
        }
        else if (spawnLoc == 2 && transform.position.x == 0 && transform.position.x < transform.position.z)
        {
            transform.GetChild(5).gameObject.SetActive(true);
            transform.GetChild(5).GetChild(0).gameObject.SetActive(true);
        }
        else if (spawnLoc == 2 && transform.position.z == 0 && transform.position.x > transform.position.z)
        {
            transform.GetChild(4).gameObject.SetActive(true);
            transform.GetChild(4).GetChild(0).gameObject.SetActive(true);
        }
        else if (spawnLoc == 2 && transform.position.z == 0 && transform.position.x < transform.position.z)
        {
            transform.GetChild(3).gameObject.SetActive(true);
            transform.GetChild(3).GetChild(0).gameObject.SetActive(true);
        }
        else if (spawnLoc == 0)
        {
            transform.GetChild(2).gameObject.SetActive(true);
            transform.GetChild(2).GetChild(0).gameObject.SetActive(true);
        }
        else if(spawnLoc <= 3)
        {
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        }

        float defLoc = loc.x;

        buildingSpot[0] = new Vector3(defLoc, 0, loc.z + 28 * 18);
        buildingSpot[1] = new Vector3(loc.x + 24 * 18, 0, loc.z + 14 * 18);
        buildingSpot[2] = new Vector3(loc.x + 24 * 18, 0, loc.z - 14 * 18);
        buildingSpot[3] = new Vector3(defLoc, 0, loc.z - 28 * 18);
        buildingSpot[4] = new Vector3(loc.x - 24 * 18, 0, loc.z + 14 * 18);
        buildingSpot[5] = new Vector3(loc.x - 24 * 18, 0, loc.z - 14 * 18);
    }
}
