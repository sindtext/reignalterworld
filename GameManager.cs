using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager gm;

    public GameObject LoaderUI;
    public GameObject spawnerObj;
    public UIManager um;
    public HectareCtrl hectare;
    public bool netStarting;

    public TMP_Text[] playName;
    public TMP_Text[] playBrz;
    public TMP_Text[] playSlv;
    public TMP_Text[] playGld;
    public TMP_Text[] playRaws;
    public TMP_Text[] playAgate;
    public TMP_Text[] playMerit;
    public TMP_Text[] playMedal;
    public TMP_Text[] playJob;
    public string[] playJobList;
    public TMP_Text[] playWage;
    public TMP_Text[] playRank;
    public string[] playRankList;
    public TMP_Text[] playAdv;
    public string[] playAdvList;
    public TMP_Text[] playPwr;
    public Image[] charPro;
    public Sprite[] charProList;
    public Image charExp;
    public float expTgt;
    public TMP_Text[] charLvl;
    public Image charHp;

    public BasicSpawner[] spawner;
    public Vector3 spawnPos;
    public Vector3 tempSpawnPos;
    public charController[] character;
    public LayerMask targetMask;
    public List<Vector3> targetPos = new List<Vector3>();
    public List<int> targetIdx= new List<int>();
    public GameObject[] villainObj;
    public ActionJoystick aj;
    CamCtrl cp;

    public string roomID;
    public int Climate;
    public int LastCoor;
    public TMP_Text xCoor;
    public TMP_Text yCoor;
    public string buildID;

    [Header("NATURE")]
    public int treeRatio;
    public int plantRatio;
    public int animalRatio;
    public int fishRatio;

    [Header("MINE")]
    public int rockRatio;
    public int peatRatio;
    public int mudRatio;
    public int thoriumRatio;

    [System.Serializable]
    public struct Armor
    {
        public string names;
        public string bulletType;
        public string skillType;
        public float ammoIdx;
        public float ammoSlot;
        public float areaRadius;
        public float areaImpulse;
        public float areaDamage;
        public float Damage;
        public float bltSpeed;
        public float timeToLife;
        public float timeToFade;
    }

    [System.Serializable]
    public struct armorType
    {
        public string names;
        public Armor[] weapon;
    }

    [Header("ARMOR")]
    public weaponType[] armorTypeList;
    public Weapon usedArmor;
    public Sprite[] armorIcon;

    [System.Serializable]
    public struct Weapon
    {
        public string names;
        public string bulletType;
        public string skillType;
        public float ammoIdx;
        public float ammoSlot;
        public float areaRadius;
        public float areaImpulse;
        public float areaDamage;
        public float Damage;
        public float bltSpeed;
        public float timeToLife;
        public float timeToFade;
    }

    [System.Serializable]
    public struct weaponType
    {
        public string names;
        public Weapon[] weapon;
    }

    [Header("WEAPON")]
    public weaponType[] weaponTypeList;
    public Weapon usedWeapon;
    public Image[] weaponSlot;
    public Sprite[] weaponIcon;

    [Header("RSS")]
    public float rssExp;
    public float rssResult;
    public string[] rssName;
    public Sprite[] rssIcon;

    [Header("HUNT")]
    public float huntExp;

    [System.Serializable]
    public struct clrList
    {
        public string listName;
        public Color32[] myClr;
    }

    [Header("ITEM")]
    public Sprite[] itemIcon;
    public clrList[] listClr;

    private void Awake()
    {
        gm = this;
        aj = FindObjectOfType<ActionJoystick>();
        cp = FindObjectOfType<CamCtrl>();
        um = FindObjectOfType<UIManager>();
        hectare = FindObjectOfType<HectareCtrl>();

        roomID = PlayerPrefs.GetString("roomID", "0.0");
    }

    // Start is called before the first frame update
    void Start()
    {
        playerSpawn(true);
    }

    public void starNet(bool first)
    {
        netStarting = true;
        if(spawner[0] == null)
        {
            spawner[0] = Instantiate(spawnerObj, Vector3.zero, Quaternion.identity).GetComponent<BasicSpawner>();
            spawner[0].Setup(first);
        }
        else if (!DataServer.call.isBHMode && !DataServer.call.isTHMode)
        {
            tempSpawnPos = spawnPos;
            spawner[1] = Instantiate(spawnerObj, Vector3.zero, Quaternion.identity).GetComponent<BasicSpawner>();
            spawner[1].Setup(first);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void switchNet()
    {
        if (DataServer.call.isBHMode || DataServer.call.isTHMode)
            return;

        StartCoroutine(switchLoad());
    }

    IEnumerator switchLoad()
    {
        targetIdx.Clear();
        targetPos.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(character[0].transform.position, 32, targetMask);

        foreach (Collider col in targetsInViewRadius)
        {
            if (col.CompareTag("Villain"))
            {
                if (!col.GetComponent<Villain>().duplicate)
                {
                    col.GetComponent<Villain>().vlnDuplicate();
                    targetPos.Add(col.transform.position);
                    targetIdx.Add(col.GetComponent<Villain>().vIndex);
                    col.GetComponent<Villain>().reSpawn();
                }
            }
        }

        yield return new WaitWhile(() => spawner[0] == null);
        yield return new WaitWhile(() => spawner[1] == null);
        yield return new WaitWhile(() => character[0] == null);
        yield return new WaitWhile(() => character[1] == null);

        aj._cc = character[1];
        cp.starting(character[1]);
        character[1].secondChar = false;
        character[1].pOne._CC.enabled = true;

        character[0].pOne.RPC_charVisibility(false);
        character[1].pOne.RPC_charVisibility(true);
        character[1].switchArmor();
        character[1].switchWeapon();
        character[1].pOne.RPC_buildCharacter(
            DataManager.dm.pHunter,
            DataManager.dm.pGatherer,
            DataManager.dm.pEndurance,
            DataManager.dm.pStength,
            DataManager.dm.pSpeed,
            DataManager.dm.pAgility,
            DataManager.dm.pAccuracy,
            DataManager.dm.pIntelligence,
            PlayerPrefs.GetFloat("currentHealth", character[1].pHealth)
        );

        character[0] = character[1];
        character[1] = null;

        spawner[0].exitWorld();
        spawner[0] = spawner[1];
        spawner[1] = null;

        PortalCtrl portal = FindObjectOfType<PortalCtrl>();

        for (int i = 0; i < targetPos.Count; i++)
        {
            Villain villain = character[0].pOne.Runner.Spawn(villainObj[targetIdx[i]], targetPos[i], Quaternion.identity, character[0].pOne.Runner.LocalPlayer).GetComponent<Villain>();
            villain.brith(portal.transform);

            Debug.Log("brith");
        }
    }

    public void userProgress(bool newdata, string merit, string medal, string job, string wage, string power, string rank, string advisor, string bronze, string silver, string gold)
    {
        foreach (TMP_Text txtMrt in playMerit) { txtMrt.text = merit; }
        foreach (TMP_Text txtMdl in playMedal) { txtMdl.text = medal; }
        foreach (TMP_Text txtJob in playJob) { txtJob.text = job; }
        foreach (TMP_Text txtWg in playWage) { txtWg.text = wage; }
        foreach (TMP_Text txtPwr in playPwr) { txtPwr.text = power; }
        foreach (TMP_Text txtRnk in playRank) { txtRnk.text = playRankList[int.Parse(rank)]; }
        if (newdata) foreach (TMP_Text txtAdv in playAdv) { txtAdv.text += " | " + playAdvList[int.Parse(advisor)]; }
        foreach (TMP_Text txtBrz in playBrz) { txtBrz.text = bronze; }
        foreach (TMP_Text txtSlv in playSlv) { txtSlv.text = silver; }
        foreach (TMP_Text txtGld in playGld) { txtGld.text = gold; }
    }

    public void playerProgress(float pLvl, float pExp)
    {
        float pMulti = Mathf.Ceil(pLvl / 10);
        expTgt = Mathf.Round(Mathf.Pow(pLvl / 10, pMulti) * 10000 / Mathf.Pow(10, pMulti)) * Mathf.Pow(10, pMulti);

        charExp.fillAmount = pExp / expTgt;

        foreach (TMP_Text txtLvl in charLvl)
        {
            txtLvl.text = ((int)pLvl).ToString("D2");
        }
    }

    public void openBuilding()
    {
        um.openUI(buildID == "Garage" ? "Warehouse" : buildID);
    }

    public void touchBuilding(string buildName)
    {
        buildID = buildName;
    }

    public void backLobby()
    {
        killNet();

        SceneManager.LoadScene(0);
    }

    void killNet()
    {
        if (spawner[0] != null) spawner[0].exitWorld();
        if (spawner[1] != null) spawner[1].exitWorld();
    }

    public void playerSpawn(bool firsttime = false)
    {
        if (!firsttime && (DataServer.call.isBHMode || DataServer.call.isTHMode))
        {
            backLobby();
        }
        else
        {
            if (!firsttime) character[0].revive();
            killNet();

            roomID = DataServer.call.isBHMode || DataServer.call.isTHMode ? "0.0" : PlayerPrefs.GetString("roomID", "0.0");
            string[] pos = roomID.Split(".");
            float x = int.Parse(pos[0]) * (24 * 18);
            float z = int.Parse(pos[1]) * (14 * 18);
            spawnPos = new Vector3(x, 0, z);

            hectare.transform.position = spawnPos;
            hectare.hectStart(spawnPos);
            hectare.Hectare();

            starNet(true);
        }
    }

    public void playerRevive()
    {
        character[0].revive(true);
    }
}
