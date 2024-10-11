using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreCtrl : MonoBehaviour
{
    HectareCtrl hectare;
    public LayerMask targetMask;

    int xPos;
    int zPos;
    int xPosChild;
    int zPosChild;
    int natureOrder;
    int rarity;
    int ratio;

    bool anyFish;
    bool anyMal;
    bool anyPeat;

    // Start is called before the first frame update
    void Start()
    {
        hectare = FindObjectOfType<HectareCtrl>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(other.GetComponent<charController>().isMine && !other.GetComponent<charController>().secondChar)
            {
                reAre();
                for(int i = 1; i < transform.parent.childCount; i++)
                {
                    transform.parent.GetChild(i).gameObject.GetComponent<SphereCollider>().enabled = false;
                }
                Invoke("reCollide", 2);
            }
        }
    }

    void reCollide()
    {
        for (int i = 1; i < transform.parent.childCount; i++)
        {
            transform.parent.GetChild(i).gameObject.GetComponent<SphereCollider>().enabled = true;
        }
    }

    public void Are(bool spawn, int idx = 99)
    {
        bool isSpawn = spawn;

        for (int i = 7; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        
        for (int i = 0; i < transform.childCount - 3; i++)
        {
            xPos = (int)(transform.position.x / 24);
            zPos = (int)(transform.position.z / 28);
            natureOrder = Mathf.Abs(xPos * zPos) % 8;

            xPosChild = (int)(transform.GetChild(i).position.x / 4);
            zPosChild = (int)(transform.GetChild(i).position.z / 7);
            ratio = Mathf.Abs(xPosChild) * Mathf.Abs(zPosChild);

            xPosChild = (int)(transform.GetChild(i).GetChild(0).GetChild(natureOrder).position.x);
            zPosChild = (int)(transform.GetChild(i).GetChild(0).GetChild(natureOrder).position.z);
            rarity = Mathf.Abs(xPosChild) * Mathf.Abs(zPosChild);

            if (rarity % 100 < GameManager.gm.fishRatio)
            {
                anyFish = true;
            }

            if (rarity % 100 < GameManager.gm.peatRatio)
            {
                anyPeat = true;
            }

            if (rarity % 100 < GameManager.gm.animalRatio)
            {
                anyMal = true;
            }

            if (natureOrder == 3) isSpawn = anyFish;

            transform.GetChild(i).GetComponent<GroundCtrl>().GroundSet(rarity, ratio, isSpawn && spawn, natureOrder);
        }

        if (idx != 99 && GameManager.gm.character[0] != null)
        {
            VillainCtrl villainCtrl = FindObjectOfType<VillainCtrl>();
            villainCtrl.SpawnVillain(transform.parent.position, idx);
        }

        if (isSpawn && spawn)
        {
            if (natureOrder == 1 && anyMal)
            {
                transform.GetChild(7).gameObject.SetActive(true);
            }
            else if (natureOrder == 3 && anyFish)
            {
                transform.GetChild(8).gameObject.SetActive(true);

                Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, 13.6f, targetMask);

                for (int i = 0; i < targetsInViewRadius.Length; i++)
                {
                    Transform target = targetsInViewRadius[i].transform;
                    Vector3 dirToTarget = (target.position - transform.position).normalized;
                    if (!targetsInViewRadius[i].CompareTag("Player"))
                    {
                        float dstToTarget = Vector3.Distance(transform.position, target.position);
                        if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget))
                        {
                            Debug.Log("Villain Pool");
                            target.GetComponent<Villain>().reSpawn();
                        }
                    }
                }
            }
            else if (natureOrder == 7 && anyPeat)
            {
                transform.GetChild(9).gameObject.SetActive(true);
            }
        }

        anyMal = false;
        anyFish = false;
        anyPeat = false;
    }

    void reAre()
    {
        hectare.transform.position = transform.position;

        if(GameManager.gm.character[1] != null)
        {
            if(Vector3.Distance(GameManager.gm.spawnPos, transform.position) <= 160)
            {
                GameManager.gm.switchNet();
            }
        }

        hectare.Hectare();
    }
}
