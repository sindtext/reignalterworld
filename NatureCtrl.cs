using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureCtrl : MonoBehaviour
{
    Transform natureObj;
    int rarity;
    int ratio;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void NatureSet(int rare, int rate, bool spawn, int natureOrder)
    {
        rarity = rare;
        ratio = rate;

        for (int i = 0; i < transform.childCount; i++)
        {
            for (int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                transform.GetChild(i).GetChild(j).gameObject.SetActive(false);
            }
            transform.GetChild(i).gameObject.SetActive(false);
        }

        if(spawn)
        {
            natureObj = transform.GetChild(natureOrder);
            natureObj.gameObject.SetActive(true);
            Invoke(natureObj.gameObject.name, 0);
        }
    }

    void Fish()
    {
        if (rarity % 100 < GameManager.gm.fishRatio)
        {
            objSpawn();
        }
    }

    void Peat()
    {
        if (rarity % 100 < GameManager.gm.peatRatio)
        {
            objSpawn();
        }
    }

    void Animal()
    {
        if (rarity % 100 < GameManager.gm.animalRatio)
        {
            objSpawn();
        }
    }

    void Rock()
    {
        if (rarity % 100 < GameManager.gm.rockRatio)
        {
            objSpawn();
        }
    }

    void Tree()
    {
        if (rarity % 100 < GameManager.gm.treeRatio)
        {
            objSpawn();
        }
    }

    void Mud()
    {
        if (rarity % 100 < GameManager.gm.mudRatio)
        {
            objSpawn();
        }
    }

    void Plant()
    {
        if (rarity % 100 < GameManager.gm.plantRatio)
        {
            objSpawn();
        }
    }

    void Thorium()
    {
        if (rarity % 100 < GameManager.gm.thoriumRatio)
        {
            //objSpawn();
        }
    }

    void objSpawn()
    {
        if (natureObj.GetChild(ratio % 7).name == "2" || natureObj.GetChild(ratio % 7).name == "8")
            return;

        natureObj.GetChild(ratio % 7).GetComponentInChildren<RSSControl>(true).identify();
        natureObj.GetChild(ratio % 7).gameObject.SetActive(true);
    }
}
