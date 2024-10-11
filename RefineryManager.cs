using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RefineryManager : MonoBehaviour
{
    public int refineryLvl;

    string RefineName;
    string MachineName;
    string MattName;

    public Transform[] storage;
    public Image[] rssInfo;
    public Transform[] ingredient;
    public Transform fuel;
    public Transform[] result;
    public Transform slot;
    public TMP_Text timer;

    public GameObject hiLight;
    public GameObject itemActL;
    public GameObject itemActR;
    public GameObject workBtn;
    public GameObject mattAmntPop;
    public GameObject zonkPop;

    public string lastPlace = "tanner";
    public GameObject itemObj;
    public MatterCtrl matterCtrl;

    int refineMatt;
    int mattLvl;
    float mattAmnt;
    float maxAmnt;

    bool atWork;
    float workTime;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        if(DataManager.dm.backpack.Length > 0)
        openrefinery(lastPlace);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    public void openrefinery(string name)
    {
        hiLight.transform.SetParent(transform);
        hiLight.SetActive(false);
        foreach (Transform child in storage[0])
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < DataManager.dm.backpack[2].bag.Length; i++)
        {
            if (!string.IsNullOrEmpty(DataManager.dm.backpack[2].bag[i].names) && DataManager.dm.backpack[2].bag[i].amount > 0 && DataManager.dm.backpack[2].bag[i].lvl == refineryLvl)
            {
                GameObject item = Instantiate(itemObj, storage[0]);
                item.GetComponent<MatterCtrl>().setupItem(DataManager.dm.backpack[2].bag[i].names, DataManager.dm.backpack[2].bag[i].type, DataManager.dm.backpack[2].bag[i].cat, 0, DataManager.dm.backpack[2].bag[i].lvl, DataManager.dm.backpack[2].bag[i].idx, DataManager.dm.backpack[2].bag[i].amount, this);
            }
        }

        lastPlace = name;
        atWork = false;
        Invoke(name, 0);
    }

    void tanner()
    {
        clearResultInfo();
        RefineName = "Tent";
        MachineName = "Tanner";

        DataServer.call.readLongData("Refinery/Tent/Tanner/2/6", this);
        rssInfo[0].sprite = GameManager.gm.rssIcon[2];
        rssInfo[1].sprite = GameManager.gm.rssIcon[6];
    }

    void sieve()
    {
        clearResultInfo();
        RefineName = "Tent";
        MachineName = "Sieve";

        DataServer.call.readLongData("Refinery/Tent/Sieve/5/7", this);
        rssInfo[0].sprite = GameManager.gm.rssIcon[5];
        rssInfo[1].sprite = GameManager.gm.rssIcon[7];
    }

    void trunk()
    {
        clearResultInfo();
        RefineName = "Tent";
        MachineName = "Trunk";

        DataServer.call.readLongData("Refinery/Tent/Trunk/3/9", this);
        rssInfo[0].sprite = GameManager.gm.rssIcon[3];
        rssInfo[1].sprite = GameManager.gm.rssIcon[9];
    }

    void clearResultInfo(bool btnOnly = false)
    {
        CancelInvoke();
        timer.text = "...";

        zonkPop.SetActive(false);
        mattAmntPop.SetActive(false);
        itemActL.SetActive(false);
        itemActR.SetActive(false);
        workBtn.SetActive(false);

        if (btnOnly)
            return;

        ingredient[0].GetChild(1).GetComponent<Image>().color = Color.clear;
        ingredient[0].GetChild(5).GetComponent<TMP_Text>().text = "";
        ingredient[0].GetChild(6).GetComponent<TMP_Text>().text = "";

        result[0].GetChild(1).GetComponent<Image>().color = Color.clear;
        result[0].GetChild(5).GetComponent<TMP_Text>().text = "";
        result[0].GetChild(6).GetComponent<TMP_Text>().text = "";

        result[1].GetChild(1).GetComponent<Image>().color = Color.clear;
        result[1].GetChild(5).GetComponent<TMP_Text>().text = "";
        result[1].GetChild(6).GetComponent<TMP_Text>().text = "";
    }

    public void actSetup(int idx, MatterCtrl mtt)
    {
        if (atWork)
            return;

        clearResultInfo();

        if (idx == 0)
        {
            itemActL.SetActive(false);
        }
        else
        {
            matterCtrl = mtt;
            refineMatt = matterCtrl.iType;
            maxAmnt = Mathf.Min(matterCtrl.iAmount, 400);
            mattAmnt = maxAmnt;
            mattLvl = matterCtrl.iLvl;
            itemActL.SetActive(true);
        }
    }

    public void calculateAmnt()
    {
        itemActL.SetActive(false);
        itemActR.SetActive(false);

        if (maxAmnt >= 40)
        {
            zonkPop.SetActive(false);
            prepareMatt();
        }
        else
        {
            zonkPop.SetActive(true);
            zonkPop.transform.GetChild(0).GetComponent<TMP_Text>().text = "Insufficient Resource";
        }
    }

    public void changeAmount()
    {
        if (itemActR.activeInHierarchy)
            mattAmntPop.SetActive(true);
    }

    public void addAmount(TMP_InputField amnt)
    {
        if (string.IsNullOrEmpty(amnt.text))
            return;

        mattAmnt = float.Parse(amnt.text);
        prepareMatt();
    }

    void prepareMatt()
    {
        mattAmnt = Mathf.Min(mattAmnt, maxAmnt);
        workTime = mattAmnt * mattLvl;

        addToMachine();

        zonkPop.SetActive(false);
        mattAmntPop.SetActive(false);
        itemActL.SetActive(false);
        itemActR.SetActive(true);
    }

    void addToMachine()
    {
        ingredient[0].GetChild(1).GetComponent<Image>().sprite = GameManager.gm.rssIcon[int.Parse(DataManager.dm.resourceDictionary["rssIcon" + refineMatt.ToString()].ToString())];
        ingredient[0].GetChild(1).GetComponent<Image>().color = Color.white;
        ingredient[0].GetChild(1).GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
        ingredient[0].GetChild(5).GetComponent<TMP_Text>().text = mattAmnt.ToString() + "x";
        ingredient[0].GetChild(6).GetComponent<TMP_Text>().text = "Lv." + mattLvl.ToString();

        result[0].GetChild(1).GetComponent<Image>().sprite = GameManager.gm.rssIcon[int.Parse(DataManager.dm.resourceDictionary["rssIcon" + refineMatt.ToString() + "0"].ToString())];
        result[0].GetChild(1).GetComponent<Image>().color = Color.white;
        result[0].GetChild(5).GetComponent<TMP_Text>().text = Mathf.Floor((mattAmnt / 2 * float.Parse(DataManager.dm.resourceDictionary["rssRefine" + refineMatt.ToString()].ToString()) / 100)).ToString() + "x";
        result[0].GetChild(6).GetComponent<TMP_Text>().text = "Lv." + (mattLvl + 1).ToString();

        result[1].GetChild(1).GetComponent<Image>().sprite = GameManager.gm.rssIcon[int.Parse(DataManager.dm.resourceDictionary["rssIcon" + refineMatt.ToString() + "1"].ToString())];
        result[1].GetChild(1).GetComponent<Image>().color = Color.white;
        result[1].GetChild(5).GetComponent<TMP_Text>().text = Mathf.Floor((mattAmnt / 2 * (100 - float.Parse(DataManager.dm.resourceDictionary["rssRefine" + refineMatt.ToString()].ToString())) / 100)).ToString() + "x";
        result[1].GetChild(6).GetComponent<TMP_Text>().text = "Lv." + (mattLvl + 1).ToString();

        workBtn.SetActive(true);
        workBtn.transform.GetChild(0).gameObject.SetActive(false);

        TimeSpan ts = TimeSpan.FromSeconds(workTime);
        timer.text = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
    }

    void startTick()
    {
        TimeSpan ts = TimeSpan.FromSeconds(workTime);
        timer.text = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);

        workTime--;

        if(workTime < 0)
        {
            CancelInvoke();

            workBtn.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    public void working(int rfnm, int lvl, long fnsh, float amnt)
    {
        atWork = true;
        refineMatt = rfnm;
        mattLvl = lvl;
        mattAmnt = amnt;

        DateTime utcDate = DateTime.UtcNow;
        long strt = (long)utcDate.Year * 12 * 30 * 24 * 60 * 60 + (long)utcDate.Month * 30 * 24 * 60 * 60 + (long)utcDate.Day * 24 * 60 * 60 + (long)utcDate.Hour * 60 * 60 + (long)utcDate.Minute * 60 + (long)utcDate.Second;
        Debug.Log(fnsh);
        Debug.Log(strt);
        Debug.Log(fnsh - strt);

        workTime = Mathf.Max(fnsh - strt, 0);
        Debug.Log(workTime);

        addToMachine();
        workBtn.SetActive(true);
        workBtn.transform.GetChild(0).gameObject.SetActive(false);
        InvokeRepeating("startTick", 0, 1);
    }

    public void Refine()
    {
        clearResultInfo(true);

        DataManager.dm.addItem(2, matterCtrl.iName, matterCtrl.iType, mattAmnt * -1, matterCtrl.iTypeName, matterCtrl.iCat, matterCtrl.iLvl);

        Debug.Log(workTime);
        DateTime utcDate = DateTime.UtcNow.AddSeconds(workTime);
        long fnsh = (long)utcDate.Year * 12 * 30 * 24 * 60 * 60 + (long)utcDate.Month * 30 * 24 * 60 * 60 + (long)utcDate.Day * 24 * 60 * 60 + (long)utcDate.Hour * 60 * 60 + (long)utcDate.Minute * 60 + (long)utcDate.Second;

        DataServer.call.saveLongData("Refinery/" + RefineName + "/" + MachineName + "/" + matterCtrl.iName, new string[4] { "order", "level", "amount", "finish" }, new object[4] { matterCtrl.iType, matterCtrl.iLvl, mattAmnt, fnsh }, this);
    }

    public void addToBackpack()
    {
        clearResultInfo(true);

        string mName = DataManager.dm.resourceDictionary["rssIcon" + refineMatt.ToString() + "0"].ToString();
        string mCat = DataManager.dm.resourceDictionary["rss" + refineMatt.ToString() + "0" + "Cat"].ToString();
        DataManager.dm.addItem(2, mName, int.Parse(mName), Mathf.Floor((mattAmnt / 2 * float.Parse(DataManager.dm.resourceDictionary["rssRefine" + refineMatt.ToString()].ToString()) / 100)), "Resource", mCat, mattLvl + 1);

        mName = DataManager.dm.resourceDictionary["rssIcon" + refineMatt.ToString() + "1"].ToString();
        mCat = DataManager.dm.resourceDictionary["rss" + refineMatt.ToString() + "1" + "Cat"].ToString();
        DataManager.dm.addItem(2, mName, int.Parse(mName), Mathf.Floor((mattAmnt / 2 * (100 - float.Parse(DataManager.dm.resourceDictionary["rssRefine" + refineMatt.ToString()].ToString())) / 100)), "Resource", mCat, mattLvl + 1);

        DataServer.call.updateLongData("Refinery/" + RefineName + "/" + MachineName + "/" + refineMatt.ToString(), new string[4] { "order", "level", "amount", "finish" }, new object[4] { 0, 0, 0, 0 }, this);
    }
}
