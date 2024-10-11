using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BackpackManager : MonoBehaviour
{
    public bool isWareHouse;
    public Transform[] container;
    public Transform[] racks;
    public Transform slot;
    public GameObject hiLight;
    public GameObject itemAct;
    public GameObject itemActs;
    
    public GameObject itemObj;
    public ItemCtrl itemCtrl;
    public int weaponLoc;

    public string lastSlot = "Item";
    int itemLoc = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        openbag(lastSlot);
    }

    // Update is called once per frame
    public void openbag(string name)
    {
        itemAct.SetActive(false);
        foreach (Transform bag in container)
        {
            bag.gameObject.SetActive(false);
            if (bag.name == name)
            {
                bag.gameObject.SetActive(true);
                bag.parent.parent.GetComponent<ScrollRect>().content = bag.GetComponent<RectTransform>();
                slot = bag;
            }
        }

        if (isWareHouse)
        {
            itemActs.SetActive(false);
            foreach (Transform rack in racks)
            {
                rack.gameObject.SetActive(false);
                if (rack.name == name)
                {
                    rack.gameObject.SetActive(true);
                    rack.parent.parent.GetComponent<ScrollRect>().content = rack.GetComponent<RectTransform>();
                    slot = rack;
                }
            }
        }

        lastSlot = name;
        Invoke(name, 0);
    }

    void Item()
    {
        itemLoc = 0;
        hiLight.transform.SetParent(transform);
        hiLight.SetActive(false);
        foreach (Transform child in container[0])
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < DataManager.dm.backpack[0].bag.Length; i++)
        {
            if (!string.IsNullOrEmpty(DataManager.dm.backpack[0].bag[i].names) && DataManager.dm.backpack[0].bag[i].amount > 0)
            {
                GameObject item = Instantiate(itemObj, container[0]);
                bool consume = DataManager.dm.backpack[0].bag[i].cat == "Medic";
                item.GetComponent<ItemCtrl>().setupItem(DataManager.dm.backpack[0].bag[i].names, DataManager.dm.backpack[0].bag[i].type, DataManager.dm.backpack[0].bag[i].cat, 0, DataManager.dm.backpack[0].bag[i].lvl, DataManager.dm.backpack[0].bag[i].idx, DataManager.dm.backpack[0].bag[i].amount, consume, false, false, this);
            }
        }

        if (!isWareHouse)
            return;

        foreach (Transform child in racks[0])
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < DataManager.dm.warehouse[0].rack.Length; i++)
        {
            if (!string.IsNullOrEmpty(DataManager.dm.warehouse[0].rack[i].names) && DataManager.dm.warehouse[0].rack[i].amount > 0)
            {
                GameObject item = Instantiate(itemObj, racks[0]);
                item.GetComponent<ItemCtrl>().setupItem(DataManager.dm.warehouse[0].rack[i].names, DataManager.dm.warehouse[0].rack[i].type, DataManager.dm.warehouse[0].rack[i].cat, 0, DataManager.dm.warehouse[0].rack[i].lvl, DataManager.dm.warehouse[0].rack[i].idx, DataManager.dm.warehouse[0].rack[i].amount, false, false, false, this);
            }
        }
    }

    void Equipment()
    {
        itemLoc = 1;
        hiLight.transform.SetParent(transform);
        hiLight.SetActive(false);
        foreach (Transform child in container[1])
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < DataManager.dm.backpack[1].bag.Length; i++)
        {
            if (!string.IsNullOrEmpty(DataManager.dm.backpack[1].bag[i].names) && DataManager.dm.backpack[1].bag[i].amount > 0)
            {
                GameObject item = Instantiate(itemObj, container[1]);
                item.GetComponent<ItemCtrl>().setupItem(DataManager.dm.backpack[1].bag[i].names, DataManager.dm.backpack[1].bag[i].type, DataManager.dm.backpack[1].bag[i].cat, 0, DataManager.dm.backpack[1].bag[i].lvl, DataManager.dm.backpack[1].bag[i].idx, DataManager.dm.backpack[1].bag[i].amount, false, false, true, this) ;
            }
        }

        if (!isWareHouse)
            return;

        foreach (Transform child in racks[1])
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < DataManager.dm.warehouse[1].rack.Length; i++)
        {
            if (!string.IsNullOrEmpty(DataManager.dm.warehouse[1].rack[i].names) && DataManager.dm.warehouse[1].rack[i].amount > 0)
            {
                GameObject item = Instantiate(itemObj, racks[1]);
                item.GetComponent<ItemCtrl>().setupItem(DataManager.dm.warehouse[1].rack[i].names, DataManager.dm.warehouse[1].rack[i].type, DataManager.dm.warehouse[1].rack[i].cat, 0, DataManager.dm.warehouse[1].rack[i].lvl, DataManager.dm.warehouse[1].rack[i].idx, DataManager.dm.warehouse[1].rack[i].amount, false, false, false, this);
            }
        }
    }

    void Resource()
    {
        itemLoc = 2;
        hiLight.transform.SetParent(transform);
        hiLight.SetActive(false);
        foreach (Transform child in container[2])
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < DataManager.dm.backpack[2].bag.Length; i++)
        {
            if(!string.IsNullOrEmpty(DataManager.dm.backpack[2].bag[i].names) && DataManager.dm.backpack[2].bag[i].amount > 0)
            {
                GameObject item = Instantiate(itemObj, container[2]);
                bool consume = DataManager.dm.backpack[2].bag[i].idx == 11;
                item.GetComponent<ItemCtrl>().setupItem(DataManager.dm.backpack[2].bag[i].names, DataManager.dm.backpack[2].bag[i].type, DataManager.dm.backpack[2].bag[i].cat, 0, DataManager.dm.backpack[2].bag[i].lvl, DataManager.dm.backpack[2].bag[i].idx, DataManager.dm.backpack[2].bag[i].amount, consume, false, false, this);
            }
        }

        if (!isWareHouse)
            return;

        foreach (Transform child in racks[2])
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < DataManager.dm.warehouse[2].rack.Length; i++)
        {
            if (!string.IsNullOrEmpty(DataManager.dm.warehouse[2].rack[i].names) && DataManager.dm.warehouse[2].rack[i].amount > 0)
            {
                GameObject item = Instantiate(itemObj, racks[2]);
                item.GetComponent<ItemCtrl>().setupItem(DataManager.dm.warehouse[2].rack[i].names, DataManager.dm.warehouse[2].rack[i].type, DataManager.dm.warehouse[2].rack[i].cat, 0, DataManager.dm.warehouse[2].rack[i].lvl, DataManager.dm.warehouse[2].rack[i].idx, DataManager.dm.warehouse[2].rack[i].amount, false, false, false, this);
            }
        }
    }

    public void actSetup()
    {
        if(itemCtrl.transform.parent.parent.parent.parent.name == "Left")
        {
            itemAct.SetActive(true);
            if (isWareHouse) itemActs.SetActive(false);
        }
        else
        {
            itemAct.SetActive(false);
            if (isWareHouse) itemActs.SetActive(true);
        }

        TMP_Text txt = itemAct.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TMP_Text>();

        if (isWareHouse)
        {
            TMP_Text txts = itemActs.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TMP_Text>();

            txt.text = "save";
            txts.text = "add";
        }
        else
        {
            txt.text = itemCtrl.isEquipable ? "equip" : itemCtrl.isSpawnable ? "spawn" : itemCtrl.isConsumable ? "consume" : "use";
        }
    }

    public void itemAction()
    {
        if (isWareHouse)
        {
            saveToWareHouse();
            if (itemCtrl.isEquipable && DataManager.dm.weapon == itemCtrl.iOrder)
            {
                Invoke("bare" + itemCtrl.iCat, 0);
            }
        }
        else if (itemCtrl.isEquipable)
        {
            Invoke(itemCtrl.iCat, 0);
        }
        else if (itemCtrl.isSpawnable)
        {
            Debug.Log("spawn");
        }
        else if (itemCtrl.isConsumable)
        {
            Invoke(itemCtrl.iCat, 0);
        }
        else
        {
            Debug.Log("use");
        }
    }

    public void addToBackpack()
    {
        DataManager.dm.saveItem(itemLoc, itemCtrl.iName, itemCtrl.iType, itemCtrl.iAmount * -1, itemCtrl.iTypeName, itemCtrl.iCat, itemCtrl.iLvl, this);
        DataManager.dm.addItem(itemLoc, itemCtrl.iName, itemCtrl.iType, itemCtrl.iAmount, itemCtrl.iTypeName, itemCtrl.iCat, itemCtrl.iLvl, this);
    }

    void saveToWareHouse()
    {
        DataManager.dm.addItem(itemLoc, itemCtrl.iName, itemCtrl.iType, itemCtrl.iAmount * -1, itemCtrl.iTypeName, itemCtrl.iCat, itemCtrl.iLvl, this);
        DataManager.dm.saveItem(itemLoc, itemCtrl.iName, itemCtrl.iType, itemCtrl.iAmount, itemCtrl.iTypeName, itemCtrl.iCat, itemCtrl.iLvl, this);
    }

    void Material()
    {
        if (itemCtrl.iType != 11)
            return;

        GameManager.gm.character[0].treat(4 * itemCtrl.iLvl);
        DataManager.dm.addItem(itemLoc, itemCtrl.iName, itemCtrl.iType, -1, itemCtrl.iTypeName, itemCtrl.iCat, itemCtrl.iLvl, this);
    }

    void Medic()
    {
        GameManager.gm.character[0].treat(4 * itemCtrl.iLvl);
        DataManager.dm.addItem(itemLoc, itemCtrl.iName, itemCtrl.iType, - 1, itemCtrl.iTypeName, itemCtrl.iCat, itemCtrl.iLvl, this);
    }

    void Weapon()
    {
        PlayerPrefs.SetInt("currentweaponType", itemCtrl.iType);
        GameManager.gm.weaponSlot[weaponLoc].sprite = GameManager.gm.weaponIcon[itemCtrl.iType * (itemCtrl.iOrder + 1)];
        DataManager.dm.weaponType = itemCtrl.iType;
        DataManager.dm.weapon = itemCtrl.iOrder;
        DataManager.dm.weaponLvl = itemCtrl.iLvl;
        GameManager.gm.character[0].switchWeapon();
    }

    void Armor()
    {
        DataManager.dm.addItem(itemLoc, itemCtrl.iName, itemCtrl.iType, -1, itemCtrl.iTypeName, itemCtrl.iCat, itemCtrl.iLvl, this);
        int idxList = Mathf.FloorToInt(itemCtrl.iType / 6);
        DataManager.dm.armorType = idxList % 4;
        DataManager.dm.armor[idxList] = itemCtrl.iOrder;
        DataManager.dm.armorLvl[idxList] = itemCtrl.iLvl;
        GameManager.gm.character[0].switchArmor();
    }

    void bareWeapon()
    {
        PlayerPrefs.SetInt("currentweaponType", 12);
        DataManager.dm.weaponType = 12;
        DataManager.dm.weapon = 0;
        DataManager.dm.weaponLvl = 1;
        GameManager.gm.character[0].switchWeapon();
    }
}
