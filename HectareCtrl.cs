using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HectareCtrl : MonoBehaviour
{
    BuildingCtrl building;

    // Start is called before the first frame update
    void Start()
    {
        building = FindObjectOfType<BuildingCtrl>();

        if (transform.position.x == 0 && transform.position.z == 0)
        {
            building.spwanBuilding(Vector3.zero);
        }
        else
        {
            Hectare();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Hectare()
    {
        gameObject.SetActive(false);

        if (Vector3.Distance(building.transform.position, transform.position) >= 112)
        {
            bool builded = false;
            foreach (Vector3 far in building.buildingSpot)
            {
                if (!builded && Vector3.Distance(far, transform.position) <= 252)
                {
                    builded = true;
                    building.spwanBuilding(far);
                    GameManager.gm.spawnPos = far;
                    if (GameManager.gm.spawner[1] == null)
                    {
                        string x = (far.x / (24 * 18)).ToString();
                        string z = (far.z / (14 * 18)).ToString();
                        GameManager.gm.roomID = x + "." + z;
                        GameManager.gm.starNet(false);
                    }
                    else
                    {
                        BasicSpawner spawnTemp = GameManager.gm.spawner[0];
                        GameManager.gm.spawner[0] = GameManager.gm.spawner[1];
                        GameManager.gm.spawner[1] = spawnTemp;

                        charController charTemp = GameManager.gm.character[0];
                        GameManager.gm.character[0] = GameManager.gm.character[1];
                        GameManager.gm.character[1] = charTemp;
                    }
                }
            }
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            if (Vector3.Distance(building.transform.position, transform.GetChild(i).position) >= 80)
            {
                transform.GetChild(i).GetComponent<AreCtrl>().Are(true);
            }
            else
            {
                transform.GetChild(i).GetComponent<AreCtrl>().Are(false);
            }
        }

        gameObject.SetActive(true);
    }
}
