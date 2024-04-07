using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class WeaponManager : NetworkBehaviour
{
    charController _cc;

    public GameObject weaponContainer;
    GameObject weaponType;
    GameObject weapon;
    GameObject animalTools;
    GameObject fishTools;
    GameObject mudTools;
    GameObject peatTools;
    GameObject plantTools;
    GameObject rockTools;
    GameObject thoriumTools;
    GameObject treeTools;
    GameObject act;

    [Networked]
    public int netActiveWeapon { get; set; }
    [Networked]
	public int netWeaponType { get; set; }
    [Networked]
    public int netWeapon { get; set; }
    [Networked]
    public int netAnimalTools { get; set; }
    [Networked]
    public int netFishTools { get; set; }
    [Networked]
    public int netMudTools { get; set; }
    [Networked]
    public int netPeatTools { get; set; }
    [Networked]
    public int netPlantTools { get; set; }
    [Networked]
    public int netRockTools { get; set; }
    [Networked]
    public int netThoriumTools { get; set; }
    [Networked]
    public int netTreeTools { get; set; }

    private void Start()
    {
        _cc = gameObject.GetComponent<charController>();
    }

    public void equip()
    {
        weaponType = weaponContainer.transform.Find("Attack/").GetChild(netWeaponType).gameObject;
        weaponType.SetActive(true);
        weapon = weaponType.transform.GetChild(netWeapon).gameObject;
        weapon.SetActive(true);
        peatTools = weaponContainer.transform.Find("Peat/").GetChild(netPeatTools).gameObject;
        peatTools.SetActive(true);
        plantTools = weaponContainer.transform.Find("Plant/").GetChild(netPlantTools).gameObject;
        plantTools.SetActive(true);
        rockTools = weaponContainer.transform.Find("Rock/").GetChild(netRockTools).gameObject;
        rockTools.SetActive(true);
        treeTools = weaponContainer.transform.Find("Tree/").GetChild(netTreeTools).gameObject;
        treeTools.SetActive(true);
        weaponSwitch();
    }

    public void weaponSwitch()
    {
        foreach (Transform child in weaponContainer.transform)
        {
            child.gameObject.SetActive(false);
        }
        act = weaponContainer.transform.GetChild(netActiveWeapon).gameObject;
        act.SetActive(true);
    }
}
