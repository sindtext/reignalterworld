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
    }

    // Update is called once per frame
    public void hectStart(Vector3 hectPos)
    {
        building.getZeroSpot(hectPos, true);
    }

    public void Hectare()
    {
        for(int i = 1; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<SphereCollider>().enabled = false;
        }

        if (Vector3.Distance(building.transform.position, transform.position) >= 112)
        {
            bool builded = false;
            foreach (Vector3 far in building.buildingSpot)
            {
                if (!builded && Vector3.Distance(far, transform.position) <= 252 && far != building.buildingSpot[6])
                {
                    builded = true;

                    string x = (far.x / (24 * 18)).ToString();
                    string z = (far.z / (14 * 18)).ToString();
                    GameManager.gm.roomID = x + "." + z;

                    building.spwanBuilding(far);

                    if (!DataServer.call.isBHMode && !DataServer.call.isTHMode)
                    {
                        StartCoroutine(switchNetwait());
                    }
                }
            }
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            float distance = Vector3.Distance(building.transform.position, transform.GetChild(i).position);

            if(distance >= 96 && i > 0)
            {
                transform.GetChild(i).GetComponent<AreCtrl>().Are(true, i);
            }
            else if (distance >= 88)
            {
                transform.GetChild(i).GetComponent<AreCtrl>().Are(true);
            }
            else
            {
                transform.GetChild(i).GetComponent<AreCtrl>().Are(false);
            }
        }

        for (int i = 1; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<SphereCollider>().enabled = true;
        }
    }

    IEnumerator switchNetwait()
    {
        yield return new WaitWhile(() => GameManager.gm.netStarting);

        if (GameManager.gm.spawner[1] == null)
        {
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
