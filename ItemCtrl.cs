using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemCtrl : MonoBehaviour
{
    public string iName;
    public string iCat;
    public string iTypeName;
    public int iType;
    public int iOrder;
    public int iLvl;
    public float iAmount;
    public bool isConsumable;
    public bool isSpawnable;
    public bool isEquipable;
    public TMP_Text amount;
    public TMP_Text level;
    public Image icon;
    public Image sign;
    public Image bg;

    Button myBtn;
    BackpackManager bm;

    // Start is called before the first frame update
    void Start()
    {
        myBtn = gameObject.GetComponent<Button>();
        myBtn.onClick.AddListener(btnAct);
    }

    // Update is called once per frame
    public void setupItem(string nam, string type, string cat, int list, int lvl, int idx, float amnt, bool consume, bool spawn, bool equip, BackpackManager bpman)
    {
        bm = bpman;
        int index = 0;
        string typecolor = type == "Resource" ? type : lvl < 4 ? "common" : lvl > 6 ? "epic" : "rare";
        for (int i = 0; i < GameManager.gm.listClr.Length; i++)
        {
            if(GameManager.gm.listClr[i].listName == typecolor)
            {
                index = i;
            }
        }

        bg.color = new Color32(GameManager.gm.listClr[index].myClr[lvl - 1].r, GameManager.gm.listClr[index].myClr[lvl - 1].g, GameManager.gm.listClr[index].myClr[lvl - 1].b, 32);
        sign.color = GameManager.gm.listClr[index].myClr[lvl - 1];

        iAmount = amnt;
        iTypeName = type;
        iName = nam;
        iCat = cat;
        iType = idx;
        iOrder = list;
        iLvl = lvl;
        amount.text = "x" + amnt.ToString();
        level.text = "Lv." + lvl.ToString();
        isConsumable = consume;
        isSpawnable = spawn;
        isEquipable = equip;

        if (type == "Equipment")
        {
            Invoke(cat, 0);
        }
        else
        {
            Invoke(type, 0);
        }
    }

    void Weapon()
    {
        icon.sprite = GameManager.gm.weaponIcon[iType];
    }

    void Armor()
    {
        icon.sprite = GameManager.gm.armorIcon[iType];
    }

    void Item()
    {
        icon.sprite = GameManager.gm.itemIcon[iType];
    }

    void Resource()
    {
        icon.sprite = GameManager.gm.rssIcon[iType];
    }

    void btnAct()
    {
        bm.itemCtrl = this;
        bm.hiLight.GetComponent<RectTransform>().localScale = Vector3.one;
        bm.hiLight.GetComponent<RectTransform>().sizeDelta = gameObject.GetComponent<RectTransform>().sizeDelta;
        bm.hiLight.transform.SetParent(transform);
        bm.hiLight.transform.localPosition = Vector3.zero;
        bm.hiLight.SetActive(true);
        bm.actSetup();
    }
}
