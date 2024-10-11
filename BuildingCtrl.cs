using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingCtrl : MonoBehaviour
{
    public Vector3[] buildingSpot = new Vector3[7];
    public Vector3[] zeroSpot = new Vector3[7];
    public Vector3 radarAnchor;
    CamCtrl cameraPivot;

    // Start is called before the first frame update
    void Awake()
    {
        cameraPivot = FindObjectOfType<CamCtrl>();
    }

    private void Start()
    {
        radarAnchor = new Vector3(PlayerPrefs.GetFloat("anchorX", 0), 0, PlayerPrefs.GetFloat("anchorZ", 0));
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

    public void spwanBuilding(Vector3 loc, bool reLoc = false)
    {
        hideBuilding();

        transform.position = loc;

        GameManager.gm.xCoor.text = loc.x.ToString();
        GameManager.gm.yCoor.text = loc.z.ToString();

        float spawnLoc;
        float spawnZ;

        spawnLoc = Mathf.Abs((Vector3.Distance(radarAnchor, loc) / (28 * 18)) % 8);
        spawnZ = Mathf.Abs((loc.z / (14 * 18)) % 8);
        if (Mathf.Round(spawnLoc) > 4 && !reLoc)
        {
            getZeroSpot(loc);
        }
        else
        {
            if (!DataServer.call.isBHMode && !DataServer.call.isTHMode)
            {
                if (spawnLoc <= 3)
                {
                    GameManager.gm.spawnPos = new Vector3(loc.x, 0, loc.z);
                    PlayerPrefs.SetString("roomID", GameManager.gm.roomID);
                }

                if (radarAnchor == loc)
                {
                    transform.GetChild(2).gameObject.SetActive(true);
                    transform.GetChild(2).GetChild(0).gameObject.SetActive(true);
                }
                else if ((Mathf.Round(spawnLoc) == 2 && spawnZ == 0) || spawnLoc == 2)
                {
                    transform.GetChild(3).gameObject.SetActive(true);
                    transform.GetChild(3).GetChild(0).gameObject.SetActive(true);
                }
                else if (spawnLoc <= 3)
                {
                    transform.GetChild(1).gameObject.SetActive(true);
                    transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    transform.GetChild(0).gameObject.SetActive(true);
                    transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                }
            }

            buildingSpot[0] = new Vector3(loc.x, 0, loc.z + 28 * 18);
            buildingSpot[1] = new Vector3(loc.x + 24 * 18, 0, loc.z + 14 * 18);
            buildingSpot[2] = new Vector3(loc.x + 24 * 18, 0, loc.z - 14 * 18);
            buildingSpot[3] = new Vector3(loc.x, 0, loc.z - 28 * 18);
            buildingSpot[4] = new Vector3(loc.x - 24 * 18, 0, loc.z + 14 * 18);
            buildingSpot[5] = new Vector3(loc.x - 24 * 18, 0, loc.z - 14 * 18);
            buildingSpot[6] = loc;

            cameraPivot.detectBuilding(radarAnchor);
        }
    }

    public void getZeroSpot(Vector3 loc, bool starting = false)
    {
        zeroSpot[0] = new Vector3(radarAnchor.x * 8, 0, radarAnchor.z + 28 * 18 * 8);
        zeroSpot[1] = new Vector3(radarAnchor.x + 24 * 18 * 8, 0, radarAnchor.z + 14 * 18 * 8);
        zeroSpot[2] = new Vector3(radarAnchor.x + 24 * 18 * 8, 0, radarAnchor.z - 14 * 18 * 8);
        zeroSpot[3] = new Vector3(radarAnchor.x * 8, 0, radarAnchor.z - 28 * 18 * 8);
        zeroSpot[4] = new Vector3(radarAnchor.x - 24 * 18 * 8, 0, radarAnchor.z + 14 * 18 * 8);
        zeroSpot[5] = new Vector3(radarAnchor.x - 24 * 18 * 8, 0, radarAnchor.z - 14 * 18 * 8);
        zeroSpot[6] = radarAnchor;

        for(int i = 0; i < zeroSpot.Length - 1; i++)
        {
            if(Vector3.Distance(transform.position, zeroSpot[i]) < Vector3.Distance(transform.position, radarAnchor))
            {
                radarAnchor = zeroSpot[i];
            }
        }

        PlayerPrefs.SetFloat("anchorX", radarAnchor.x);
        PlayerPrefs.SetFloat("anchorZ", radarAnchor.z);

        cameraPivot.markingBuilding(radarAnchor);

        if (starting)
        {
            spwanBuilding(loc);
        }
        else
        {
            spwanBuilding(loc, true);
        }
    }
}
