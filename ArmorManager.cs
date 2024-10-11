using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ArmorManager : NetworkBehaviour
{
    public GameObject underContainer;
    public GameObject armorContainer;
    public GameObject skinContainer;

    GameObject[] armor;
    GameObject[] skin;

    public int netActiveArmor;
    public int netArmorType;
    public int[] netArmor;
    public int[] netArmorLvl;
    public int netSkinType;
    public int[] netSkin;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void equip()
    {
        if (armor == null) armor = new GameObject[6];
        if (skin == null) skin = new GameObject[6];

        if (armor[0] != null)
        {
            foreach (GameObject obj in armor)
            {
                foreach (GameObject chld in obj.transform)
                {
                    chld.SetActive(false);
                }
                obj.SetActive(false);
            }
            foreach (GameObject obj in skin)
            {
                foreach (GameObject chld in obj.transform)
                {
                    chld.SetActive(false);
                }
                obj.SetActive(false);
            }
        }

        if (netActiveArmor == 0)
        {
            for (int i = 0; i < armor.Length; i++)
            {
                if (netArmor[i] == 4)
                {
                    skin[i] = skinContainer.transform.GetChild(i).GetChild(netSkin[i]).GetChild(0).gameObject;

                    skin[i].SetActive(true);
                }
                else
                {
                    armor[i] = armorContainer.transform.GetChild(i).GetChild(netArmor[i]).GetChild(netArmorLvl[i]).gameObject;

                    armor[i].SetActive(true);
                }
            }
        }
        else
        {
            for (int i = 0; i < skin.Length; i++)
            {
                skin[i] = skinContainer.transform.GetChild(i).GetChild(netSkin[i]).GetChild(0).gameObject;

                skin[i].SetActive(true);
            }
        }

        underContainer.SetActive(true);
        armorSwitch();
    }

    public void armorSwitch()
    {
        foreach (Transform child in armorContainer.transform)
        {
            child.gameObject.SetActive(netActiveArmor == 0);
        }

        foreach (Transform child in skinContainer.transform)
        {
            child.gameObject.SetActive(true);
        }
    }
}
