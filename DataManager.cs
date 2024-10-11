using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    public static DataManager dm;

    public int charIdx;

    [Header("Talent")]
    public float pHunter = 50;
    public float pGatherer = 50;

    [Header("Role")]
    public float pGuardian;
    public float pFighter;
    public float pEngineer;
    public float pThinker;

    [Header("Attribute")]
    public float pEndurance = 50;
    public float pStength = 50;
    public float pSpeed = 50;
    public float pAgility = 50;
    public float pAccuracy = 50;
    public float pIntelligence = 50;

    [Header("Armor")]
    public int armorType;
    public int[] armor;
    public int[] armorLvl;

    [Header("Weapon")]
    public int weaponType;
    public int weapon;
    public int weaponLvl;

    [Header("Cloth")]
    public int clothType;
    public int[] cloth;

    [Header("Skin")]
    public int skinType;
    public int[] skinx;
    public int[] skiny;

    [Header("Progress")]
    public IDictionary<string, object> progressContainer = new Dictionary<string, object> { };

    [Header("Dictionary")]
    public IDictionary<string, object> resourceDictionary = new Dictionary<string, object> { };


    [System.Serializable]
    public struct Bag
    {
        public string names;
        public string type;
        public string cat;
        public int idx;
        public int lvl;
        public float amount;
    }


    [System.Serializable]
    public struct Backpack
    {
        public string names;
        public Bag[] bag;
    }

    [Header("Backpack")]
    public Backpack[] backpack;


    [System.Serializable]
    public struct Rack
    {
        public string names;
        public string type;
        public string cat;
        public int idx;
        public int lvl;
        public float amount;
    }


    [System.Serializable]
    public struct Warehouse
    {
        public string names;
        public Rack[] rack;
    }

    [Header("Warehouse")]
    public Warehouse[] warehouse;


    private void Awake()
    {
        dm = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            return;

        addDictionary();

        IDictionary<string, object> tempData = new Dictionary<string, object> { };
        tempData = DataServer.call.userCloud.ToDictionary();
        int charOrder = int.Parse(tempData["charID"].ToString());

        weaponType = PlayerPrefs.GetInt("currentweaponType", 0);
        clothType = charOrder % 1;
        skinType = charOrder % 1;

        backpack = new Backpack[3];
        for (int i = 0; i < backpack.Length; i++)
        {
            backpack[i].bag = new Bag[40];
        }

        warehouse = new Warehouse[3];
        for (int i = 0; i < warehouse.Length; i++)
        {
            warehouse[i].rack = new Rack[40];
        }

        DataServer.call.readingProfile();
        StartCoroutine(DataServer.call.readDocummentData("Account/Progress"));
        StartCoroutine(DataServer.call.readDocummentData("Character/Progress"));
        StartCoroutine(DataServer.call.readDocummentData("Character/Armor"));
        StartCoroutine(DataServer.call.readDocummentData("Character/Cloth"));
        StartCoroutine(DataServer.call.readDocummentData("Character/Skin"));
        StartCoroutine(DataServer.call.readQueryData(2, "Backpack/Resource/Raw"));
        StartCoroutine(DataServer.call.readQueryData(2, "Backpack/Resource/Material"));
        StartCoroutine(DataServer.call.readQueryData(1, "Backpack/Equipment/Weapon"));
        StartCoroutine(DataServer.call.readQueryData(1, "Backpack/Equipment/Armor"));
        StartCoroutine(DataServer.call.readQueryData(0, "Backpack/Item/Ammo"));
        StartCoroutine(DataServer.call.readQueryData(0, "Backpack/Item/Medic"));
        StartCoroutine(DataServer.call.readQueryData(2, "Warehouse/Resource/Raw"));
        StartCoroutine(DataServer.call.readQueryData(2, "Warehouse/Resource/Material"));
        StartCoroutine(DataServer.call.readQueryData(1, "Warehouse/Equipment/Weapon"));
        StartCoroutine(DataServer.call.readQueryData(1, "Warehouse/Equipment/Armor"));
        StartCoroutine(DataServer.call.readQueryData(0, "Warehouse/Item/Ammo"));
        StartCoroutine(DataServer.call.readQueryData(0, "Warehouse/Item/Medic"));
    }

    public void addProgress(string key, float amount)
    {
        float amountValue = 0;
        if (progressContainer.TryGetValue(key, out object value))
        {
            amountValue = (float)value;

            string[] accPro = new string[10] { "Merit", "Medal", "Job", "Wage", "Power", "Rank", "Advisor", "Bronze", "Silver", "Gold" };
            bool isAcc = false;
            
            foreach(string acc in accPro)
            {
                if(key == acc)
                {
                    isAcc = true;
                }
            }

            if (isAcc)
            {
                DataServer.call.updateShortData("Account/Progress", new string[1] { key }, new object[1] { amountValue + amount });
            }
            else if(progressContainer.TryGetValue("Lvl", out object valueLvl))
            {
                if(key == "Exp" && amountValue + amount >= GameManager.gm.expTgt)
                {
                    progressContainer["Lvl"] = (float)valueLvl + 1;
                    progressContainer[key] = (amountValue + amount) - GameManager.gm.expTgt;

                    DataServer.call.updateShortData("Character/Progress", new string[2] { key, "Lvl" }, new object[2] { (amountValue + amount) - GameManager.gm.expTgt, (float)valueLvl + 1 });
                    GameManager.gm.playerProgress((float)valueLvl + 1, (amountValue + amount) - GameManager.gm.expTgt);
                    return;
                }
                else
                {
                    DataServer.call.updateShortData("Character/Progress", new string[1] { key }, new object[1] { amountValue + amount });

                    if(key == "Exp") GameManager.gm.playerProgress((float)valueLvl, amountValue + amount);
                }
            }

            progressContainer[key] = amountValue + amount;
        }
    }

    public void addItem(int itemLoc, string itemName, int itemIdx, float itemAmount, string itemType, string itemCat, int itemLvl, BackpackManager bm = null)
    {
        SoundManager.sm.bagPlay();

        int idx = 0;
        for(int i = 0; i < backpack[itemLoc].bag.Length; i++)
        {
            if(backpack[itemLoc].bag[i].names == itemName && backpack[itemLoc].bag[i].amount > 0)
            {
                float rssValue = backpack[itemLoc].bag[i].amount;
                backpack[itemLoc].bag[i].amount = rssValue + itemAmount;
                DataServer.call.updateLongData("Backpack/" + itemType + "/" + itemCat + "/" + itemName, new string[1] { "amount" }, new object[1] { rssValue + itemAmount });

                if (bm != null) bm.openbag(bm.lastSlot);

                return;
            }

            if (string.IsNullOrEmpty(backpack[itemLoc].bag[i].names))
            {
                idx = i;

                break;
            }
        }

        backpack[itemLoc].bag[idx].names = itemName;
        backpack[itemLoc].bag[idx].type = itemType;
        backpack[itemLoc].bag[idx].cat = itemCat;
        backpack[itemLoc].bag[idx].idx = itemIdx;
        backpack[itemLoc].bag[idx].lvl = itemLvl;
        backpack[itemLoc].bag[idx].amount = itemAmount;

        DataServer.call.saveLongData("Backpack/" + itemType + "/" + itemCat + "/" + itemName, new string[6] { "names", "type", "cat", "idx", "lvl", "amount" }, new object[6] { itemName, itemType, itemCat, itemIdx, itemLvl, itemAmount });

        if(bm != null) bm.openbag(bm.lastSlot);
    }

    public void saveItem(int itemLoc, string itemName, int itemIdx, float itemAmount, string itemType, string itemCat, int itemLvl, BackpackManager bm)
    {
        int idx = 0;
        for (int i = 0; i < warehouse[itemLoc].rack.Length; i++)
        {
            if (warehouse[itemLoc].rack[i].names == itemName)
            {
                float rssValue = warehouse[itemLoc].rack[i].amount;
                warehouse[itemLoc].rack[i].amount = rssValue + itemAmount;
                DataServer.call.updateLongData("Warehouse/" + itemType + "/" + itemCat + "/" + itemName, new string[1] { "amount" }, new object[1] { rssValue + itemAmount });

                if (bm != null) bm.openbag(bm.lastSlot);

                return;
            }

            if (string.IsNullOrEmpty(warehouse[itemLoc].rack[i].names))
            {
                idx = i;

                break;
            }
        }

        warehouse[itemLoc].rack[idx].names = itemName;
        warehouse[itemLoc].rack[idx].type = itemType;
        warehouse[itemLoc].rack[idx].cat = itemCat;
        warehouse[itemLoc].rack[idx].idx = itemIdx;
        warehouse[itemLoc].rack[idx].lvl = itemLvl;
        warehouse[itemLoc].rack[idx].amount = itemAmount;

        DataServer.call.saveLongData("Warehouse/" + itemType + "/" + itemCat + "/" + itemName, new string[6] { "names", "type", "cat", "idx", "lvl", "amount" }, new object[6] { itemName, itemType, itemCat, itemIdx, itemLvl, itemAmount });

        if (bm != null) bm.openbag(bm.lastSlot);
    }

    void addDictionary()
    {
        resourceDictionary["rss2Cat"] = "Raw";
        resourceDictionary["rss3Cat"] = "Raw";
        resourceDictionary["rss4Cat"] = "Raw";
        resourceDictionary["rss5Cat"] = "Raw";
        resourceDictionary["rss6Cat"] = "Raw";
        resourceDictionary["rss7Cat"] = "Raw";
        resourceDictionary["rss8Cat"] = "Raw";
        resourceDictionary["rss9Cat"] = "Raw";
        resourceDictionary["rss20Cat"] = "Material";
        resourceDictionary["rss21Cat"] = "Material";
        resourceDictionary["rss30Cat"] = "Material";
        resourceDictionary["rss31Cat"] = "Material";
        resourceDictionary["rss40Cat"] = "Material";
        resourceDictionary["rss41Cat"] = "Material";
        resourceDictionary["rss50Cat"] = "Material";
        resourceDictionary["rss51Cat"] = "Material";
        resourceDictionary["rss60Cat"] = "Material";
        resourceDictionary["rss61Cat"] = "Material";
        resourceDictionary["rss70Cat"] = "Material";
        resourceDictionary["rss71Cat"] = "Material";
        resourceDictionary["rss80Cat"] = "Material";
        resourceDictionary["rss81Cat"] = "Material";
        resourceDictionary["rss90Cat"] = "Material";
        resourceDictionary["rss91Cat"] = "Material";

        resourceDictionary["rssIcon0"] = 0;
        resourceDictionary["rssIcon1"] = 1;
        resourceDictionary["rssIcon2"] = 2; // ravin
        resourceDictionary["rssIcon3"] = 3; // fish
        resourceDictionary["rssIcon4"] = 4; // mudflow
        resourceDictionary["rssIcon5"] = 5; // soil
        resourceDictionary["rssIcon6"] = 6; // harvest
        resourceDictionary["rssIcon7"] = 7; // stone
        resourceDictionary["rssIcon8"] = 8; // thor
        resourceDictionary["rssIcon9"] = 9; // log
        resourceDictionary["rssIcon20"] = 10; // hide
        resourceDictionary["rssIcon21"] = 11; // meat
        resourceDictionary["rssIcon30"] = 11; // meat
        resourceDictionary["rssIcon31"] = 11; // meat
        resourceDictionary["rssIcon40"] = 14; // -
        resourceDictionary["rssIcon41"] = 15; // -
        resourceDictionary["rssIcon50"] = 16; // clay
        resourceDictionary["rssIcon51"] = 17; // slime
        resourceDictionary["rssIcon60"] = 18; // rope
        resourceDictionary["rssIcon61"] = 19; // herb
        resourceDictionary["rssIcon70"] = 20; // gravel
        resourceDictionary["rssIcon71"] = 21; // sand
        resourceDictionary["rssIcon80"] = 22; // -
        resourceDictionary["rssIcon81"] = 23; // -
        resourceDictionary["rssIcon90"] = 24; // bark
        resourceDictionary["rssIcon91"] = 25; // wood

        resourceDictionary["rssRefine2"] = 50;
        resourceDictionary["rssRefine3"] = 50;
        resourceDictionary["rssRefine4"] = 0;
        resourceDictionary["rssRefine5"] = 30;
        resourceDictionary["rssRefine6"] = 20;
        resourceDictionary["rssRefine7"] = 40;
        resourceDictionary["rssRefine8"] = 0;
        resourceDictionary["rssRefine9"] = 50;

        resourceDictionary["1rssBow0"] = 60;
        resourceDictionary["1rssBow1"] = 91;
        resourceDictionary["1rssBow2"] = 0;
        resourceDictionary["1Bow0"] = 25;
        resourceDictionary["1Bow1"] = 25;
        resourceDictionary["1Bow2"] = 0;

        resourceDictionary["1rssSpear0"] = 7;
        resourceDictionary["1rssSpear1"] = 9;
        resourceDictionary["1rssSpear2"] = 60;
        resourceDictionary["1Spear0"] = 10;
        resourceDictionary["1Spear1"] = 25;
        resourceDictionary["1Spear2"] = 15;

        resourceDictionary["1rssFist0"] = 7;
        resourceDictionary["1rssFist1"] = 20;
        resourceDictionary["1rssFist2"] = 70;
        resourceDictionary["1Fist0"] = 15;
        resourceDictionary["1Fist1"] = 20;
        resourceDictionary["1Fist2"] = 15;

        resourceDictionary["1rssDagger0"] = 7;
        resourceDictionary["1rssDagger1"] = 20;
        resourceDictionary["1rssDagger2"] = 60;
        resourceDictionary["1Dagger0"] = 15;
        resourceDictionary["1Dagger1"] = 25;
        resourceDictionary["1Dagger2"] = 10;

        resourceDictionary["1rssShield0"] = 90;
        resourceDictionary["1rssShield1"] = 91;
        resourceDictionary["1rssShield2"] = 0;
        resourceDictionary["1Shield0"] = 30;
        resourceDictionary["1Shield1"] = 20;
        resourceDictionary["1Shield2"] = 0;

        resourceDictionary["1rssArrow0"] = 9;
        resourceDictionary["1rssArrow1"] = 70;
        resourceDictionary["1rssArrow2"] = 0;
        resourceDictionary["1Arrow0"] = 25;
        resourceDictionary["1Arrow1"] = 25;
        resourceDictionary["1Arrow2"] = 0;

        resourceDictionary["1rssHelmet0"] = 20;
        resourceDictionary["1rssHelmet1"] = 60;
        resourceDictionary["1rssHelmet2"] = 71;
        resourceDictionary["1Helmet0"] = 10;
        resourceDictionary["1Helmet1"] = 5;
        resourceDictionary["1Helmet2"] = 10;

        resourceDictionary["1rssVest0"] = 20;
        resourceDictionary["1rssVest1"] = 60;
        resourceDictionary["1rssVest2"] = 71;
        resourceDictionary["1Vest0"] = 15;
        resourceDictionary["1Vest1"] = 5;
        resourceDictionary["1Vest2"] = 5;

        resourceDictionary["1rssGlove0"] = 20;
        resourceDictionary["1rssGlove1"] = 60;
        resourceDictionary["1rssGlove2"] = 71;
        resourceDictionary["1Glove0"] = 5;
        resourceDictionary["1Glove1"] = 10;
        resourceDictionary["1Glove2"] = 10;

        resourceDictionary["1rssPad0"] = 20;
        resourceDictionary["1rssPad1"] = 60;
        resourceDictionary["1rssPad2"] = 71;
        resourceDictionary["1Pad0"] = 5;
        resourceDictionary["1Pad1"] = 15;
        resourceDictionary["1Pad2"] = 5;

        resourceDictionary["1rssBelt0"] = 20;
        resourceDictionary["1rssBelt1"] = 60;
        resourceDictionary["1rssBelt2"] = 71;
        resourceDictionary["1Belt0"] = 15;
        resourceDictionary["1Belt1"] = 5;
        resourceDictionary["1Belt2"] = 5;

        resourceDictionary["1rssShoes0"] = 20;
        resourceDictionary["1rssShoes1"] = 60;
        resourceDictionary["1rssShoes2"] = 71;
        resourceDictionary["1Shoes0"] = 5;
        resourceDictionary["1Shoes1"] = 10;
        resourceDictionary["1Shoes2"] = 10;
    }
}
