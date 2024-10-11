using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatterCtrl : MonoBehaviour
{
    public string iName;
    public string iCat;
    public string iTypeName;
    public int iType;
    public int iOrder;
    public int iLvl;
    public float iAmount;
    public TMP_Text amount;
    public TMP_Text level;

    public Image icon;
    public Image sign;
    public Image bg;

    Button myBtn;
    RefineryManager rm;
    // Start is called before the first frame update
    void Start()
    {
        myBtn = gameObject.GetComponent<Button>();
        myBtn.onClick.AddListener(btnAct);
    }

    // Update is called once per frame
    public void setupItem(string nam, string type, string cat, int list, int lvl, int idx, float amnt, RefineryManager rfman)
    {
        rm = rfman;
        int index = 0;
        string typecolor = type == "Resource" ? type : lvl < 4 ? "common" : lvl > 6 ? "epic" : "rare";
        for (int i = 0; i < GameManager.gm.listClr.Length; i++)
        {
            if (GameManager.gm.listClr[i].listName == typecolor)
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
        rm.matterCtrl = this;
        rm.hiLight.GetComponent<RectTransform>().localScale = Vector3.one;
        rm.hiLight.GetComponent<RectTransform>().sizeDelta = gameObject.GetComponent<RectTransform>().sizeDelta;
        rm.hiLight.transform.SetParent(transform);
        rm.hiLight.transform.localPosition = Vector3.zero;
        rm.hiLight.SetActive(true);

        Invoke(rm.lastPlace, 0);
    }

    void tanner()
    {
        int idx = iType == 2 || iType == 6 ? iType : 0;
        rm.actSetup(idx, this);
    }

    void sieve()
    {
        int idx = iType == 5 || iType == 7 ? iType : 0;
        rm.actSetup(idx, this);
    }

    void trunk()
    {
        int idx = iType == 3 || iType == 9 ? iType : 0;
        rm.actSetup(idx, this);
    }
}
