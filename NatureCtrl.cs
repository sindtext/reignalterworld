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
            natureObj.GetChild(ratio % 7).gameObject.SetActive(true);
        }
    }

    void Peat()
    {
        if (rarity % 100 < GameManager.gm.peatRatio)
        {
            natureObj.GetChild(ratio % 7).gameObject.SetActive(true);
        }
    }

    void Animal()
    {
        if (rarity % 100 < GameManager.gm.animalRatio)
        {
            natureObj.GetChild(ratio % 7).gameObject.SetActive(true);
        }
    }

    void Rock()
    {
        if (rarity % 100 < GameManager.gm.rockRatio)
        {
            natureObj.GetChild(ratio % 7).gameObject.SetActive(true);
        }
    }

    void Tree()
    {
        if (rarity % 100 < GameManager.gm.treeRatio)
        {
            natureObj.GetChild(ratio % 7).gameObject.SetActive(true);
        }
    }

    void Mud()
    {
        if (rarity % 100 < GameManager.gm.mudRatio)
        {
            natureObj.GetChild(ratio % 7).gameObject.SetActive(true);
        }
    }

    void Plant()
    {
        if (rarity % 100 < GameManager.gm.plantRatio)
        {
            natureObj.GetChild(ratio % 7).gameObject.SetActive(true);
        }
    }

    void Thorium()
    {
        if (rarity % 100 < GameManager.gm.thoriumRatio)
        {
            natureObj.GetChild(ratio % 7).gameObject.SetActive(true);
        }
    }
}
