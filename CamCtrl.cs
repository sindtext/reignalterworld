using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamCtrl : MonoBehaviour
{
    public bool zoom;
    public bool moveCam;
    public RectTransform mainChar;
    BuildingCtrl building;
    public Transform radarContainer;
    public GameObject[] radarObj;
    Dictionary<int, RectTransform> mapObj = new Dictionary<int, RectTransform> { };
    Dictionary<int, Vector3> mapPos = new Dictionary<int, Vector3> { };

    public charController _cc;
    Camera mainCam;

    private void Start()
    {
        building = FindObjectOfType<BuildingCtrl>();
        mainCam = Camera.main;
        zoom = true;
    }
    // Start is called before the first frame update
    public void starting(charController cc)
    {
        _cc = cc;
        moveCam = true;
    }

    public void detectBuilding(Vector3 zeroSpot)
    {
        int zeroPos = mapObj.Count;
        Vector3 vec;
        int idx = 0;
        int oldIdx = 0;

        for (int i = 0; i < building.buildingSpot.Length; i++ )
        {
            bool marking = true;
            vec = building.buildingSpot[i];

            foreach (KeyValuePair<int, Vector3> entry in mapPos)
            {
                if (vec == entry.Value)
                {
                    marking = false;
                    oldIdx = entry.Key;
                }
            }

            if (marking)
            {
                mapPos.Add(zeroPos + idx, vec);
                addToRadar(zeroPos + idx, zeroSpot, vec);
                idx++;
            }
            else
            {
                mapPos.Remove(oldIdx);
                Destroy(mapObj[oldIdx].gameObject);
                mapObj.Remove(oldIdx);
                mapPos.Add(oldIdx, vec);
                addToRadar(oldIdx, zeroSpot, vec);
            }
        }
    }

    public void markingBuilding(Vector3 zeroSpot)
    {
        int zeroPos = mapObj.Count;
        Vector3 vec;
        int idx = 0;
        int oldIdx = 0;

        for (int i = 0; i < building.zeroSpot.Length; i++)
        {
            bool marking = true;
            vec = building.zeroSpot[i];

            foreach (KeyValuePair<int, Vector3> entry in mapPos)
            {
                if (vec == entry.Value)
                {
                    marking = false;
                    oldIdx = entry.Key;
                }
            }

            if (marking)
            {
                mapPos.Add(zeroPos + idx, vec);
                addToRadar(zeroPos + idx, zeroSpot, vec);
                idx++;
            }
            else
            {
                mapPos.Remove(oldIdx);
                Destroy(mapObj[oldIdx].gameObject);
                mapObj.Remove(oldIdx);
                mapPos.Add(oldIdx, vec);
                addToRadar(oldIdx, zeroSpot, vec);
            }
        }
    }

    void addToRadar(int mapIdx, Vector3 zeroSpot, Vector3 vec)
    {
        float spawnLoc = Mathf.Abs((Vector3.Distance(zeroSpot, vec) / (28 * 18)) % 8);
        float spawnZ = Mathf.Abs((vec.z / (14 * 18)) % 8);

        if (zeroSpot == vec)
        {
            mapObj.Add(mapIdx, Instantiate(radarObj[2], new Vector3(vec.x, vec.z, 0) * .24f, radarObj[2].transform.rotation).GetComponent<RectTransform>());
            mapObj[mapIdx].transform.SetParent(radarContainer, false);
        }
        else if ((Mathf.Round(spawnLoc) == 2 && spawnZ == 0) || spawnLoc == 2)
        {
            mapObj.Add(mapIdx, Instantiate(radarObj[3], new Vector3(vec.x, vec.z, 0) * .24f, radarObj[3].transform.rotation).GetComponent<RectTransform>());
            mapObj[mapIdx].transform.SetParent(radarContainer, false);
        }
        else if (spawnLoc <= 3)
        {
            mapObj.Add(mapIdx, Instantiate(radarObj[1], new Vector3(vec.x, vec.z, 0) * .24f, radarObj[1].transform.rotation).GetComponent<RectTransform>());
            mapObj[mapIdx].transform.SetParent(radarContainer, false);
        }
        else
        {
            mapObj.Add(mapIdx, Instantiate(radarObj[0], new Vector3(vec.x, vec.z, 0) * .24f, radarObj[0].transform.rotation).GetComponent<RectTransform>());
            mapObj[mapIdx].transform.SetParent(radarContainer, false);
        }
    }

    public void zooming()
    {
        moveCam = true;
        zoom = !zoom;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (_cc != null) CamMove();

        if (!moveCam)
            return;

        if (zoom && mainCam.orthographicSize > 4.8f)
        {
            mainCam.orthographicSize -= Time.deltaTime * 4;
        }
        else if (!zoom && mainCam.orthographicSize < 8f)
        {
            mainCam.orthographicSize += Time.deltaTime * 4;
        }
        else
        {
            moveCam = false;
        }
    }

    void CamMove()
    {
        mainChar.localEulerAngles = new Vector3(0,0, _cc.transform.eulerAngles.y * -1);
        transform.position = Vector3.Lerp(transform.position, new Vector3(_cc.transform.position.x, 0.008f, _cc.transform.position.z), Time.deltaTime * Mathf.Lerp(0, _cc.playerSpeed, _cc.playerSpeed));
        Vector3 pone = _cc.transform.position * -.24f;
        radarContainer.localPosition = new Vector3(pone.x, pone.z, 0);
        /*for (int i = 0; i < mapPos.Count; i++)
        {
            Vector3 vec = mapPos[i];
            Vector3 pone = _cc.transform.position;
            mapObj[i].localPosition = (new Vector3(vec.x, vec.z, 0) - new Vector3(pone.x, pone.z, 0)) / 4;
        }*/
    }
}
