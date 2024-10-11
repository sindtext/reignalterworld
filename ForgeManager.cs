using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForgeManager : MonoBehaviour
{
    public TMP_Text bluename;
    public Transform blueprint;
    public Transform bluereq;
    public TMP_Text forgeBtn;
    public Transform workBench;
    public Transform lNav;
    public Image forgeProgress;

    public GameObject hiLight;

    int slot;
    string forgeName;
    int forgeIdx;
    float forgeAmnt;
    string forgeType;
    string forgeCat;
    int forgeLvl;
    int forgeNeed;

    // Start is called before the first frame update
    private void OnEnable()
    {
        openNav("weapon");
        openWB("Range");
    }

    public void openNav(string names)
    {
        foreach (Transform nav in lNav.transform)
        {
            nav.gameObject.SetActive(nav.name == names);
        }
    }

    public void openWB(string names)
    {
        foreach(Transform wb in workBench.transform)
        {
            wb.gameObject.SetActive(wb.name == names);
            if (wb.name == names)
            {
                workBench.parent.GetComponent<ScrollRect>().content = wb.GetComponent<RectTransform>();
            }
        }
    }

    public void setForge(string blprtCat, string blprtName, int blprtIdx, int blprtLvl, int blprtNeed)
    {
        if (forgeBtn.text == "TAKE")
            return;

        forgeBtn.text = "FORGE";
        forgeBtn.transform.parent.GetComponent<Button>().interactable = true;
        bluename.text = blprtName;

        string[] forgeCode = blprtName.Split('_');
        slot = blprtCat == "Ammo" ? 0 : 1;
        forgeName = forgeCode[0] + forgeCode[1];
        forgeIdx = blprtIdx;
        forgeAmnt = blprtCat == "Ammo" ? 200 : 1;
        forgeType = blprtCat == "Ammo" ? "Item" : "Equipment";
        forgeCat = blprtCat;
        forgeLvl = blprtLvl;
        forgeNeed = blprtNeed;

        blueprint.GetChild(2).GetComponent<Image>().color = Color.white;

        if (blprtCat == "Ammo") blueprint.GetChild(2).GetComponent<Image>().sprite = GameManager.gm.itemIcon[blprtIdx];
        if (blprtCat == "Armor") blueprint.GetChild(2).GetComponent<Image>().sprite = GameManager.gm.armorIcon[blprtIdx];
        if (blprtCat == "Weapon") blueprint.GetChild(2).GetComponent<Image>().sprite = GameManager.gm.weaponIcon[blprtIdx];

        for (int i = 0; i < bluereq.childCount; i++)
        {
            if(i < blprtNeed)
            {
                string idx = blprtLvl.ToString() + "rss" + forgeCode[1] + i.ToString();
                string matt = DataManager.dm.resourceDictionary[idx].ToString();
                string amnt = DataManager.dm.resourceDictionary[blprtLvl.ToString() + forgeCode[1] + i.ToString()].ToString();
                bluereq.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.white;
                bluereq.GetChild(i).GetChild(0).GetComponent<Image>().sprite = GameManager.gm.rssIcon[int.Parse(DataManager.dm.resourceDictionary["rssIcon" + matt].ToString())];
                bluereq.GetChild(i).GetChild(1).GetComponent<TMP_Text>().text = GameManager.gm.rssName[int.Parse(DataManager.dm.resourceDictionary["rssIcon" + matt].ToString())];
                bluereq.GetChild(i).GetChild(2).GetComponent<TMP_Text>().text = amnt;

                checkMatt(matt, amnt);
            }
            else
            {
                bluereq.GetChild(i).GetChild(0).GetComponent<Image>().color = new Color32(0,0,0,0);
                bluereq.GetChild(i).GetChild(1).GetComponent<TMP_Text>().text = "";
                bluereq.GetChild(i).GetChild(2).GetComponent<TMP_Text>().text = "";
            }
        }
    }

    void checkMatt(string need, string amount)
    {
        bool mattenough = false;

        for (int i = 0; i < DataManager.dm.backpack[2].bag.Length; i++)
        {
            if (DataManager.dm.backpack[2].bag[i].idx == int.Parse(DataManager.dm.resourceDictionary["rssIcon" + need].ToString()))
            {
                mattenough = DataManager.dm.backpack[2].bag[i].amount >= float.Parse(amount);
            }
        }

        if (forgeBtn.transform.parent.GetComponent<Button>().interactable) forgeBtn.transform.parent.GetComponent<Button>().interactable = mattenough;
    }

    public void forge()
    {
        if (forgeBtn.text == "TAKE")
        {
            forgeBtn.text = "FORGE";
            setForge(forgeCat, bluename.text, forgeIdx, forgeLvl, forgeNeed);
            DataManager.dm.addItem(slot, forgeName, forgeIdx, forgeAmnt, forgeType, forgeCat, forgeLvl);
            forgeProgress.fillAmount = 0;
        }
        else
        {
            forgeBtn.transform.parent.GetComponent<Button>().interactable = false;
            InvokeRepeating("saveForge", 0, .2f);
        }
    }

    void saveForge()
    {
        forgeProgress.fillAmount += .1f;

        if(forgeProgress.fillAmount >= 1)
        {
            forgeBtn.text = "TAKE";
            forgeBtn.transform.parent.GetComponent<Button>().interactable = true;
            CancelInvoke("saveForge");
        }
    }
}
