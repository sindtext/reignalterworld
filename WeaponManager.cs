using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class WeaponManager : NetworkBehaviour
{
    public GameObject lContainer;
    public GameObject rContainer;
    GameObject[] weapons;
    GameObject[] tools;
    GameObject actL;
    GameObject actR;

    public int netActiveWeapon;
    public int netWeaponType;
    public int netWeapon;
    public int netWeaponLvl;
    public int netAnimalTools;
    public int netFishTools;
    public int netMudTools;
    public int netPeatTools;
    public int netPlantTools;
    public int netRockTools;
    public int netThoriumTools;
    public int netTreeTools;

    private void Start()
    {

    }

    public void equip()
    {
        if(weapons == null) weapons = new GameObject[4];
        if(tools == null) tools = new GameObject[8];

        if (weapons[0] != null)
        {
            foreach(GameObject obj in weapons)
            {
                obj.SetActive(false);
            }
            foreach (GameObject obj in tools)
            {
                obj.SetActive(false);
            }
        }

        weapons[0] = lContainer.transform.Find("Attack/").GetChild(netWeaponType).gameObject;
        weapons[1] = rContainer.transform.Find("Attack/").GetChild(netWeaponType).gameObject;

        weapons[2] = weapons[0].transform.GetChild(netWeapon).gameObject;
        for (int i = 0; i < netWeaponLvl; i++)
        {
            weapons[2].transform.GetChild(i).gameObject.SetActive(true);
        }

        weapons[3] = weapons[1].transform.GetChild(netWeapon).gameObject;
        for (int i = 0; i < netWeaponLvl; i++)
        {
            weapons[3].transform.GetChild(i).gameObject.SetActive(true);
        }

        tools[0] = rContainer.transform.Find("Animal/").GetChild(netAnimalTools).gameObject;
        tools[1] = rContainer.transform.Find("Fish/").GetChild(netFishTools).gameObject;
        tools[2] = rContainer.transform.Find("Mud/").GetChild(netMudTools).gameObject;
        tools[3] = rContainer.transform.Find("Peat/").GetChild(netPeatTools).gameObject;
        tools[4] = rContainer.transform.Find("Plant/").GetChild(netPlantTools).gameObject;
        tools[5] = rContainer.transform.Find("Rock/").GetChild(netRockTools).gameObject;
        tools[6] = rContainer.transform.Find("Thorium/").GetChild(netThoriumTools).gameObject;
        tools[7] = rContainer.transform.Find("Tree/").GetChild(netTreeTools).gameObject;


        foreach (GameObject obj in weapons)
        {
            obj.SetActive(true);
        }
        foreach (GameObject obj in tools)
        {
            obj.SetActive(true);
        }

        weaponSwitch();
    }

    public void weaponSwitch()
    {
        foreach (Transform child in lContainer.transform)
        {
            child.gameObject.SetActive(false);
        }
        actL = lContainer.transform.GetChild(netActiveWeapon).gameObject;
        actL.SetActive(true);

        foreach (Transform child in rContainer.transform)
        {
            child.gameObject.SetActive(false);
        }
        actR = rContainer.transform.GetChild(netActiveWeapon).gameObject;
        actR.SetActive(true);

        if (gameObject.GetComponent<charController>().FOV != null)
        {
            gameObject.GetComponent<charController>().setFOV();
        }
    }
}
