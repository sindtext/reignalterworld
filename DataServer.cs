using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using Google;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.Events;

public class DataServer : MonoBehaviour
{
    public static DataServer call;

    IDictionary<string, object> cloudData = new Dictionary<string, object> { };
    IDictionary<string, object> userData = new Dictionary<string, object> { };
    LobbyManager lm;
    UIManager um;
    DeviceCheck dc;
    public rawBankManager rbm;
    public rawChangerManager rcm;

    public string webClientId = "<your client id here>";
    private GoogleSignInConfiguration configuration;
    private FirebaseAuth auth;
    FirebaseFirestore database;

    private AuthResult userSign;
    string tempMail;
    string tempPass;
    bool isUserFault;
    bool isUserSign;

    public DocumentSnapshot userCloud;
    bool isUserCloud;
    bool isUserData;

    public string uID;
    public string walletID;
    public string refID;
    public bool isBHMode;
    public bool isTHMode;
    public bool newDevice;
    string devicename;
    public bool newWallet;

    void Awake()
    {
        if (call)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        call = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

	void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if (scene.buildIndex == 0)
		{
            Invoke("initDataServer", .64f);
        }
	}

    void initDataServer()
    {
        lm = FindObjectOfType<LobbyManager>();
        um = FindObjectOfType<UIManager>();
        dc = FindObjectOfType<DeviceCheck>();

        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true
        };

        StartCoroutine(CheckFirebaseDependencies());
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    IEnumerator CheckFirebaseDependencies()
    {
        Task<DependencyStatus> initTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => initTask.IsCompleted);

        if (initTask.Result != DependencyStatus.Available)
        {
            lm.AddToInformation("Dependency Not Available");

            yield break;
        }

        lm.AddToInformation("Init Dependency Success");

        auth = FirebaseAuth.DefaultInstance;
        database = FirebaseFirestore.DefaultInstance;

        if (userSign != null)
        {
            StartCoroutine(readProfiles(userSign.User.UserId));

            yield break;
        }

        if (PlayerPrefs.HasKey("gSign"))
        {
            StartCoroutine(googleSignIn());

            yield break;
        }

        if (PlayerPrefs.HasKey("eSign"))
        {
            StartCoroutine(emailSignIn(PlayerPrefs.GetString("eSign"), PlayerPrefs.GetString("pSign")));

            yield break;
        }

        um.openUI("Load");
    }

    public void OnGoogleSignIn()
    {
        StartCoroutine(googleSignIn());
    }

    IEnumerator googleSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        Task<GoogleSignInUser> signTask = GoogleSignIn.DefaultInstance.SignInSilently();
        yield return new WaitUntil(() => signTask.IsCompleted);

        if (signTask.IsFaulted || signTask.IsCanceled)
        {
            lm.AddToInformation("Auto Google SignIn Is Faulted");

            signTask = GoogleSignIn.DefaultInstance.SignIn();
            yield return new WaitUntil(() => signTask.IsCompleted);

            if (signTask.IsFaulted || signTask.IsCanceled)
            {
                lm.AddToInformation("Manual Google SignIn Is Faulted");

                yield break;
            }
        }

        lm.AddToInformation("Welcome: " + signTask.Result.DisplayName + "!");
        PlayerPrefs.SetString("gSign", "signed");
        Credential gcredential = GoogleAuthProvider.GetCredential(signTask.Result.IdToken, null);

        Task<AuthResult> authTask = auth.SignInAndRetrieveDataWithCredentialAsync(gcredential);
        yield return new WaitUntil(() => authTask.IsCompleted);

        if (authTask.IsFaulted || authTask.IsCanceled)
        {
            lm.AddToInformation("Firebase SignIn Is Faulted");

            yield break;
        }

        userSign = authTask.Result;
        uID = userSign.User.UserId;
        lm.AddToInformation("Firebase Google SignIn successfully: " + userSign.User.DisplayName + " | " + userSign.User.UserId);

        StartCoroutine(readProfiles(userSign.User.UserId));
    }

    public void OnEmailSignIn(string email, string password)
    {
        PlayerPrefs.SetString("eSign", email);
        PlayerPrefs.SetString("pSign", password);
        StartCoroutine(emailSignIn(email, password));
    }

    IEnumerator emailSignIn(string email, string password)
    {
        Task<AuthResult> authTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => authTask.IsCompleted);

        if (authTask.IsCanceled || authTask.IsFaulted)
        {
            lm.AddToInformation("Firebase SignIn Is Faulted");
            
            authTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => authTask.IsCompleted);

            if (authTask.IsFaulted || authTask.IsCanceled)
            {
                lm.AddToInformation("Firebase Register Is Faulted");

                yield break;
            }
        }

        userSign = authTask.Result;
        uID = userSign.User.UserId;
        lm.AddToInformation("Firebase eMail SignIn successfully: " + userSign.User.DisplayName + " | " + userSign.User.UserId);

        StartCoroutine(readProfiles(userSign.User.UserId));
    }

    public void checkReferral(string referral)
    {
        StartCoroutine(referralCheck(referral));
    }

    IEnumerator referralCheck(string referral)
    {
        if (referral.Substring(0, 3) == "usr")
        {
            Task<DocumentSnapshot> readConnect = database.Collection("Referral").Document("Connect").Collection(referral).Document("gameId").GetSnapshotAsync();
            yield return new WaitUntil(() => readConnect.IsCompleted);

            if (readConnect.IsFaulted || readConnect.IsCanceled)
            {
                yield break;
            }

            cloudData = new Dictionary<string, object> { };
            cloudData = readConnect.Result.ToDictionary();

            if (cloudData == null)
            {
                saveReferral(referral);
            }
            else
            {

                lm.inviteText.transform.GetChild(1).GetComponent<UIAnim>().Alert();
            }
        }
        else
        {
            Task<QuerySnapshot> readReferral = database.Collection("Referral").Document("Data").Collection(referral).GetSnapshotAsync();
            yield return new WaitUntil(() => readReferral.IsCompleted);

            QuerySnapshot queryReferral = readReferral.Result;

            if (queryReferral.Count != 0)
            {
                saveReferral(referral);
            }
            else
            {
                lm.inviteText.transform.GetChild(1).GetComponent<UIAnim>().Alert();
            }
        }
    }

    void saveReferral(string referral)
    {
        lm.exeLoader.SetActive(true);
        StartCoroutine(referralSave(referral));
    }

    IEnumerator referralSave(string referral)
    {
        if (referral.Substring(0, 3) == "usr")
        {
            cloudData = new Dictionary<string, object> { };
            cloudData.Add("gameId", referral);
            cloudData.Add("walletId", "");

            Task writeReferral = database.Collection("Referral").Document("Connect").Collection(referral).Document("gameId").SetAsync(cloudData);
            yield return new WaitUntil(() => writeReferral.IsCompleted);

            updateShortData("Account/Profiles", new string[3] { "charID", "playerName", "teleId" }, new object[3] { DataManager.dm.charIdx, lm.displayNameText.text, referral.Substring(3, referral.Length - 3) });
        }
        else
        {
            string referralID = userSign.User.UserId.Substring(0, 4) + userSign.User.UserId.Substring(userSign.User.UserId.Length - 4, 4);

            cloudData = new Dictionary<string, object> { };
            cloudData.Add("UpLineID", referral);
            cloudData.Add("DownLineName", lm.displayNameText.text);
            cloudData.Add("DownLineID", referralID);

            Task writeReferral = database.Collection("Referral").Document("Data").Collection(referral).Document(referralID).SetAsync(cloudData);
            yield return new WaitUntil(() => writeReferral.IsCompleted);

            cloudData = new Dictionary<string, object> { };
            cloudData.Add("rawShare", 0);
            cloudData.Add("bronzeShare", 0);
            cloudData.Add("fishShare", 0);
            cloudData.Add("harvestShare", 0);
            cloudData.Add("logShare", 0);
            cloudData.Add("mudShare", 0);
            cloudData.Add("ravinShare", 0);
            cloudData.Add("soilShare", 0);
            cloudData.Add("stoneShare", 0);

            Task writeShare = database.Collection("Referral").Document("Share").Collection(referral).Document(referralID).SetAsync(cloudData);
            yield return new WaitUntil(() => writeShare.IsCompleted);

            updateShortData("Account/Profiles", new string[4] { "charID", "playerName", "referId", "uplineId" }, new object[4] { DataManager.dm.charIdx, lm.displayNameText.text, referralID, referral });
        }
        lm.exeLoader.SetActive(false);
    }

    public void readReferral()
    {
        StartCoroutine(referralRead());
    }

    IEnumerator referralRead()
    {
        Task<QuerySnapshot> readReferral = database.Collection("Referral").Document("Data").Collection(refID).GetSnapshotAsync();
        yield return new WaitUntil(() => readReferral.IsCompleted);

        int refCount = 0;
        QuerySnapshot queryRef = readReferral.Result;
        if (queryRef != null)
        {
            foreach (DocumentSnapshot refer in queryRef.Documents)
            {
                cloudData = new Dictionary<string, object> { };
                cloudData = refer.ToDictionary();
                if (cloudData != null)
                {
                    if (cloudData["DownLineID"].ToString() != refID)
                    {
                        refCount++;
                        lm.setupReferral(cloudData["DownLineName"].ToString(), cloudData["DownLineID"].ToString());
                    }
                }
            }
        }

        lm.referralCount.text = "Your Referral [" + refCount.ToString() + "]";

        Task<QuerySnapshot> readShare = database.Collection("Referral").Document("Share").Collection(refID).GetSnapshotAsync();
        yield return new WaitUntil(() => readShare.IsCompleted);

        float rawCount = lm.rawWalletTemp;
        QuerySnapshot queryShare = readShare.Result;
        if (queryShare != null)
        {
            foreach (DocumentSnapshot refer in queryShare.Documents)
            {
                cloudData = new Dictionary<string, object> { };
                cloudData = refer.ToDictionary();
                if (cloudData != null)
                {
                    rawCount += float.Parse(cloudData["rawShare"].ToString());
                }
            }
        }

        lm.referralShare.text = rawCount.ToString();
    }

    public void saveWallet(string wallet)
    {
        PlayerPrefs.DeleteKey("tempGold");
        PlayerPrefs.DeleteKey("tempExc");
        PlayerPrefs.DeleteKey("timeStamp");
        rbm.checkBalance();

        if (refID.Substring(0, 3) == "usr") StartCoroutine(walletSave(wallet));
    }

    IEnumerator walletSave(string wallet)
    {
        lm.exeLoader.SetActive(true);
        cloudData = new Dictionary<string, object> { };
        cloudData.Add("walletId", wallet);

        Task writeWallet = database.Collection("Referral").Document("Connect").Collection("usr" + lm.teleText.text).Document("gameId").UpdateAsync(cloudData);
        yield return new WaitUntil(() => writeWallet.IsCompleted);
        lm.exeLoader.SetActive(false);
    }

    public void checkDevice()
    {
        StartCoroutine(deviceCheck());
    }

    IEnumerator deviceCheck()
    {
        devicename = "unknown";
        if (Application.platform == RuntimePlatform.WindowsEditor) devicename = "admin";
        if (Application.platform == RuntimePlatform.WindowsPlayer) devicename = "windows";
        if (Application.platform == RuntimePlatform.OSXPlayer) devicename = "osx";
        if (Application.platform == RuntimePlatform.WebGLPlayer) devicename = "webgl";
        if (Application.platform == RuntimePlatform.Android) devicename = "android";
        if (Application.platform == RuntimePlatform.IPhonePlayer) devicename = "ios";

        string sn = dc.getsn();
        string mac = dc.getmac();

        string idType = !string.IsNullOrEmpty(sn) ? "sn" : !string.IsNullOrEmpty(mac) ? "mac" : "unknown";
        string idValue = !string.IsNullOrEmpty(sn) ? sn : !string.IsNullOrEmpty(mac) ? mac : devicename + refID;
        Task<DocumentSnapshot> readDevice = database.Collection("Device").Document(devicename).Collection(idType).Document(idValue).GetSnapshotAsync();
        yield return new WaitUntil(() => readDevice.IsCompleted);

        if (readDevice.IsFaulted || readDevice.IsCanceled)
        {
            yield break;
        }

        cloudData = new Dictionary<string, object> { };
        cloudData = readDevice.Result.ToDictionary();
        newDevice = cloudData == null;
    }
    
    public void saveRawBank(string wallet, string keys, string timestamp)
    {
        lm.exeLoader.SetActive(true);
        newWallet = false;
        StartCoroutine(rawbankSave(wallet, keys, timestamp));
    }

    IEnumerator rawbankSave(string wallet, string keys, string timestamp)
    {
        string sn = dc.getsn();
        string mac = dc.getmac();

        cloudData = new Dictionary<string, object> { };
        cloudData.Add("uId", userSign.User.UserId);
        cloudData.Add("uKey", keys);
        cloudData.Add("timeStamp", timestamp);
        cloudData.Add("walletId", wallet);
        cloudData.Add("balance", string.IsNullOrEmpty(lm.upLine) ? 0 : 100);
        cloudData.Add("sn", devicename + "_" + sn);
        cloudData.Add("mac", devicename + "_" + mac);
        cloudData.Add("Agate", 0);
        cloudData.Add("Bronze", 0);
        cloudData.Add("Silver", 0);
        cloudData.Add("Gold", 0);

         Task writeRawBank = database.Collection("UserData").Document(userSign.User.UserId).Collection("Account").Document("RawBank").SetAsync(cloudData);
        yield return new WaitUntil(() => writeRawBank.IsCompleted);

        cloudData = new Dictionary<string, object> { };
        cloudData.Add("uId", userSign.User.UserId);
        cloudData.Add("timeStamp", DateTime.UtcNow.ToString());
        cloudData.Add("walletId", wallet);
        cloudData.Add("sn", devicename + "_" + sn);
        cloudData.Add("mac", devicename + "_" + mac);

        string idType = !string.IsNullOrEmpty(sn) ? "sn" : !string.IsNullOrEmpty(mac) ? "mac" : "unknown";
        string idValue = !string.IsNullOrEmpty(sn) ? sn : !string.IsNullOrEmpty(mac) ? mac : devicename + refID;
        if (!newDevice) idValue += "_" + refID;
        Task writeDevice = database.Collection("Device").Document(devicename).Collection(idType).Document(idValue).SetAsync(cloudData);
        yield return new WaitUntil(() => writeDevice.IsCompleted);

        if (!string.IsNullOrEmpty(lm.upLine))
        {
            cloudData = new Dictionary<string, object> { };
            cloudData.Add("UpLineID", lm.upLine);
            cloudData.Add("DownLineName", lm.displayNameText.text);
            cloudData.Add("DownLineID", refID);

            Task writeReferral = database.Collection("Referral").Document("Data").Collection(refID).Document("000000").SetAsync(cloudData);
            yield return new WaitUntil(() => writeReferral.IsCompleted);

            cloudData = new Dictionary<string, object> { };
            cloudData.Add("rawShare", 0);
            cloudData.Add("bronzeShare", 0);
            cloudData.Add("fishShare", 0);
            cloudData.Add("harvestShare", 0);
            cloudData.Add("logShare", 0);
            cloudData.Add("mudShare", 0);
            cloudData.Add("ravinShare", 0);
            cloudData.Add("soilShare", 0);
            cloudData.Add("stoneShare", 0);

            Task writeMyShare = database.Collection("Referral").Document("Share").Collection(refID).Document("000000").SetAsync(cloudData);
            yield return new WaitUntil(() => writeMyShare.IsCompleted);

            cloudData = new Dictionary<string, object> { };
            cloudData.Add("rawShare", 50);

            Task updateShare = database.Collection("Referral").Document("Share").Collection(lm.upLine).Document(refID).UpdateAsync(cloudData);
            yield return new WaitUntil(() => updateShare.IsCompleted);
        }

        readingProfile();
        yield return new WaitWhile(() => !eWallet.call.connectBtn.activeInHierarchy);
        lm.exeLoader.SetActive(false);
        eWallet.call.LoginSkale();
    }

    public IEnumerator lockCall(Action<int> callback)
    {
        int idx = 0;

        Task<DocumentSnapshot> readLock = database.Collection("UserData").Document(userSign.User.UserId).Collection("Account").Document("RawBank").GetSnapshotAsync();
        yield return new WaitUntil(() => readLock.IsCompleted);

        if (readLock.IsFaulted || readLock.IsCanceled)
        {
            yield break;
        }

        IDictionary<string, object> lockData = new Dictionary<string, object> { };
        lockData = readLock.Result.ToDictionary();

        if (lockData != null)
        {
            string[] stridx = lockData["timeStamp"].ToString().Split(':');
            idx = int.Parse(stridx[0]) % 6;
        }

        Task<DocumentSnapshot> readChain = database.Collection("GameData").Document("rawbank").Collection("chain").Document("keylock").GetSnapshotAsync();
        yield return new WaitUntil(() => readChain.IsCompleted);

        if (readChain.IsFaulted || readChain.IsCanceled)
        {
            yield break;
        }

        IDictionary<string, object> chainData = new Dictionary<string, object> { };
        chainData = readChain.Result.ToDictionary();

        if (chainData != null)
        {
            string a = chainData["alpha"].ToString().Substring(idx, 1);
            string b = chainData["beta"].ToString().Substring(idx, 1);
            string c = chainData["delta"].ToString().Substring(idx, 1);
            string d = chainData["epsilon"].ToString().Substring(idx, 1);
            string e = chainData["iota"].ToString().Substring(idx, 1);
            string f = chainData["kappa"].ToString().Substring(idx, 1);

            string locker = a + b + c + d + e + f;

            Debug.Log("lockCall : " + int.Parse(locker));
            yield return int.Parse(locker);
            callback(int.Parse(locker));
        }
    }

    public IEnumerator ticketCall(string ticketname, Action<string> callback)
    {
        Task<DocumentSnapshot> readTicket = database.Collection("GameData").Document("rawchanger").Collection("ticket").Document("exchange").GetSnapshotAsync();
        yield return new WaitUntil(() => readTicket.IsCompleted);

        if (readTicket.IsFaulted || readTicket.IsCanceled)
        {
            yield break;
        }

        IDictionary<string, object> ticketData = new Dictionary<string, object> { };
        ticketData = readTicket.Result.ToDictionary();

        if (ticketData != null)
        {
            Debug.Log("ticketCall : " + ticketData[ticketname].ToString());
            yield return ticketData[ticketname].ToString();
            callback(ticketData[ticketname].ToString());
        }
    }

    public IEnumerator eLockCall(Action<int> callback)
    {
        Task<DocumentSnapshot> readKey = database.Collection("UserData").Document(userSign.User.UserId).Collection("Account").Document("RawChanger").GetSnapshotAsync();
        yield return new WaitUntil(() => readKey.IsCompleted);

        if (readKey.IsFaulted || readKey.IsCanceled)
        {
            yield break;
        }

        IDictionary<string, object> keyData = new Dictionary<string, object> { };
        keyData = readKey.Result.ToDictionary();

        if (keyData == null)
        {
            int pin = 123456;
            string timestamp = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();
            IDictionary<string, object> newKeyData = new Dictionary<string, object> { };
            newKeyData.Add("keys", pin.ToString("X"));
            newKeyData.Add("timeStamp", timestamp);

            Task writeMyKey = database.Collection("UserData").Document(userSign.User.UserId).Collection("Account").Document("RawChanger").SetAsync(newKeyData);
            yield return new WaitUntil(() => writeMyKey.IsCompleted);

            callback(999999);
            yield break;
        }

        int idx = 0;

        Task<DocumentSnapshot> readLock = database.Collection("UserData").Document(userSign.User.UserId).Collection("Account").Document("RawChanger").GetSnapshotAsync();
        yield return new WaitUntil(() => readLock.IsCompleted);

        if (readLock.IsFaulted || readLock.IsCanceled)
        {
            yield break;
        }

        IDictionary<string, object> lockData = new Dictionary<string, object> { };
        lockData = readLock.Result.ToDictionary();

        if (lockData != null)
        {
            string[] stridx = lockData["timeStamp"].ToString().Split(':');
            idx = int.Parse(stridx[0]) % 6;
        }

        Task<DocumentSnapshot> readChain = database.Collection("GameData").Document("rawchanger").Collection("chain").Document("keylock").GetSnapshotAsync();
        yield return new WaitUntil(() => readChain.IsCompleted);

        if (readChain.IsFaulted || readChain.IsCanceled)
        {
            yield break;
        }

        IDictionary<string, object> chainData = new Dictionary<string, object> { };
        chainData = readChain.Result.ToDictionary();

        if (chainData != null)
        {
            string a = chainData["alpha"].ToString().Substring(idx, 1);
            string b = chainData["beta"].ToString().Substring(idx, 1);
            string c = chainData["delta"].ToString().Substring(idx, 1);
            string d = chainData["epsilon"].ToString().Substring(idx, 1);
            string e = chainData["iota"].ToString().Substring(idx, 1);
            string f = chainData["kappa"].ToString().Substring(idx, 1);

            string locker = a + b + c + d + e + f;

            callback(int.Parse(locker));
        }
    }

    public void readingProfile()
    {
        StartCoroutine(readProfiles(userSign.User.UserId));
    }

    IEnumerator readProfiles(string userid)
    {
        Task<DocumentSnapshot> readTask = database.Collection("UserData").Document(userid).Collection("Account").Document("Profiles").GetSnapshotAsync();
        yield return new WaitUntil(() => readTask.IsCompleted);

        if (readTask.IsFaulted || readTask.IsCanceled)
        {
            yield break;
        }

        userCloud = readTask.Result;
        userData = new Dictionary<string, object> { };
        userData = userCloud.ToDictionary();

        if (userData == null)
        {
            userData = new Dictionary<string, object> { };
            StartCoroutine(savingProfile(0, "", userSign.User.UserId, userSign.User.Email, userSign.User.DisplayName, "", "", ""));
            saveLongData("Backpack/Equipment/Weapon/Sword", new string[6] { "names", "type", "cat", "idx", "lvl", "amount" }, new object[6] { "Sword", "Equipment", "Weapon", 0, 1, 1 });
            saveShortData("Character/Progress", new string[10] { "Exp", "Lvl", "pHunter", "pGatherer", "pEndurance", "pStength", "pSpeed", "pAgility", "pAccuracy", "pIntelligence" }, new object[10] { 0, 1, 50, 50, 50, 50, 50, 50, 50, 50 });
            saveShortData("Character/Armor", new string[12] { "helmet", "vest", "gloves", "pad", "belt", "shoes", "helmetLvl", "vestLvl", "glovesLvl", "padLvl", "beltLvl", "shoesLvl" }, new object[12] { 4, 4, 4, 4, 4, 4, 0, 0, 0, 0, 0, 0 });
            saveShortData("Character/Cloth", new string[6] { "helmet", "vest", "gloves", "pad", "belt", "shoes" }, new object[6] { 0, 0, 0, 0, 0, 0 });
            saveShortData("Character/Skin", new string[12] { "helmetX", "vestX", "glovesX", "padX", "beltX", "shoesX", "helmetY", "vestY", "glovesY", "padY", "beltY", "shoesY" }, new object[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            saveShortData("Account/Progress", new string[10] { "Merit", "Medal", "Job", "Wage", "Power", "Rank", "Advisor", "Bronze", "Silver", "Gold" }, new object[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        }
        else if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            lm.AddToInformation("successfull readData");

            lm.updateProfiles(
                int.Parse(userData["charID"].ToString()),
                userData["playerName"].ToString(),
                userData["userId"].ToString(),
                userData["userMail"].ToString(),
                userData["userName"].ToString(),
                userData["teleId"].ToString(),
                userData["referId"].ToString(),
                userData["uplineId"].ToString()
            );
            
            yield return new WaitForSeconds(.32f);

            checkDevice();
            lm.GoLobby(string.IsNullOrEmpty(userData["playerName"].ToString()));

            yield return new WaitForSeconds(.32f);

            refID = string.IsNullOrEmpty(userData["referId"].ToString()) ? userData["teleId"].ToString() : userData["referId"].ToString();

            if (string.IsNullOrEmpty(refID)) yield break;

            eWallet.call.skaleID = "raw" + refID;
            eWallet.call.skaleKey = refID;

            Task<QuerySnapshot> readReferral = database.Collection("Referral").Document("Data").Collection(refID).GetSnapshotAsync();
            yield return new WaitUntil(() => readReferral.IsCompleted);

            QuerySnapshot queryReferral = readReferral.Result;

            Task<DocumentSnapshot> rawbankTask = database.Collection("UserData").Document(userid).Collection("Account").Document("RawBank").GetSnapshotAsync();
            yield return new WaitUntil(() => rawbankTask.IsCompleted);

            cloudData = new Dictionary<string, object> { };
            cloudData = rawbankTask.Result.ToDictionary();

            if(cloudData != null)
            {
                newWallet = false;
                if (queryReferral.Count > 0 && queryReferral != null)
                {
                    walletID = cloudData["walletId"].ToString();

                    lm.rawWalletTemp = float.Parse(cloudData["balance"].ToString());
                    eWallet.call.checkWallet();
                }
                else
                {
                    saveRawBank(cloudData["walletId"].ToString(), cloudData["uKey"].ToString(), cloudData["timeStamp"].ToString());
                }
            }
            else
            {
                newWallet = true;
                eWallet.call.checkWallet();
            }

            Task<DocumentSnapshot> eqTask = database.Collection("UserData").Document(userid).Collection("Character").Document("Armor").GetSnapshotAsync();
            yield return new WaitUntil(() => eqTask.IsCompleted);

            cloudData = new Dictionary<string, object> { };
            cloudData = eqTask.Result.ToDictionary();

            if (cloudData == null)
            {
                saveShortData("Character/Armor", new string[12] { "helmet", "vest", "gloves", "pad", "belt", "shoes", "helmetLvl", "vestLvl", "glovesLvl", "padLvl", "beltLvl", "shoesLvl" }, new object[12] { 4, 4, 4, 4, 4, 4, 0, 0, 0, 0, 0, 0 });
                saveShortData("Character/Cloth", new string[6] { "helmet", "vest", "gloves", "pad", "belt", "shoes" }, new object[6] { 0, 0, 0, 0, 0, 0 });
                saveShortData("Character/Skin", new string[12] { "helmetX", "vestX", "glovesX", "padX", "beltX", "shoesX", "helmetY", "vestY", "glovesY", "padY", "beltY", "shoesY" }, new object[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            }
        }
        else
        {
            foreach(Image img in GameManager.gm.charPro)
            {
                img.sprite = GameManager.gm.charProList[int.Parse(userData["charID"].ToString())];
            }

            foreach (TMP_Text nam in GameManager.gm.playName)
            {
                nam.text = userData["playerName"].ToString();
            }
        }
    }

    IEnumerator savingProfile(int charID, string playername, string userid, string usermail, string username, string teleid, string referid, string uplineid)
    {
        cloudData = new Dictionary<string, object> { };
        cloudData.Add("charID", charID);
        cloudData.Add("playerName", playername);
        cloudData.Add("userId", userid);
        cloudData.Add("userMail", usermail);
        cloudData.Add("userName", username);
        cloudData.Add("teleId", teleid);
        cloudData.Add("referId", referid);
        cloudData.Add("uplineId", uplineid);


        Task writeTask = database.Collection("UserData").Document(userid).Collection("Account").Document("Profiles").SetAsync(cloudData);
        yield return new WaitUntil(() => writeTask.IsCompleted);
        
        lm.AddToInformation("Create Profiles Successfull");
        StartCoroutine(readProfiles(userid));
    }

    public IEnumerator readDocummentData(string dataPath, bool newdata = true)
    {
        string[] path = dataPath.Split('/');

        Task<DocumentSnapshot> readTask = database.Collection("UserData").Document(userSign.User.UserId).Collection(path[0]).Document(path[1]).GetSnapshotAsync();
        yield return new WaitUntil(() => readTask.IsCompleted);

        if (readTask.IsFaulted || readTask.IsCanceled)
        {
            yield break;
        }

        cloudData = new Dictionary<string, object> { };
        cloudData = readTask.Result.ToDictionary();

        if (cloudData != null)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                rcm.brzBlnc = float.Parse(cloudData["Bronze"].ToString());
                rcm.slvBlnc = float.Parse(cloudData["Silver"].ToString());
                rcm.gldBlnc = float.Parse(cloudData["Gold"].ToString());

                rcm.savingType("Bronze");
            }
            else
            {
                if (newdata && path[1] == "Progress")
                {
                    foreach (KeyValuePair<string, object> data in cloudData)
                    {
                        DataManager.dm.progressContainer.Add(data.Key, float.Parse(data.Value.ToString()));
                    }
                }

                if (path[0] == "Account")
                {
                    GameManager.gm.userProgress(newdata, cloudData["Merit"].ToString(), cloudData["Medal"].ToString(), cloudData["Job"].ToString(), cloudData["Wage"].ToString(), cloudData["Power"].ToString(), cloudData["Rank"].ToString(), cloudData["Advisor"].ToString(), cloudData["Bronze"].ToString(), cloudData["Silver"].ToString(), cloudData["Gold"].ToString());
                }
                else
                {
                    if (path[1] == "Armor")
                    {
                        DataManager.dm.armor = new int[] { int.Parse(cloudData["helmet"].ToString()), int.Parse(cloudData["vest"].ToString()), int.Parse(cloudData["gloves"].ToString()), int.Parse(cloudData["pad"].ToString()), int.Parse(cloudData["belt"].ToString()), int.Parse(cloudData["shoes"].ToString()) };

                        DataManager.dm.armorLvl = new int[] { int.Parse(cloudData["helmetLvl"].ToString()), int.Parse(cloudData["vestLvl"].ToString()), int.Parse(cloudData["glovesLvl"].ToString()), int.Parse(cloudData["padLvl"].ToString()), int.Parse(cloudData["beltLvl"].ToString()), int.Parse(cloudData["shoesLvl"].ToString()) };
                    }
                    else if (path[1] == "Skin")
                    {
                        DataManager.dm.skinx = new int[] { int.Parse(cloudData["helmetX"].ToString()), int.Parse(cloudData["vestX"].ToString()), int.Parse(cloudData["glovesX"].ToString()), int.Parse(cloudData["padX"].ToString()), int.Parse(cloudData["beltX"].ToString()), int.Parse(cloudData["shoesX"].ToString()) };

                        DataManager.dm.skiny = new int[] { int.Parse(cloudData["helmetY"].ToString()), int.Parse(cloudData["vestY"].ToString()), int.Parse(cloudData["glovesY"].ToString()), int.Parse(cloudData["padY"].ToString()), int.Parse(cloudData["beltY"].ToString()), int.Parse(cloudData["shoesY"].ToString()) };
                    }
                    else if (path[1] == "Cloth")
                    {
                        DataManager.dm.cloth = new int[] { int.Parse(cloudData["helmet"].ToString()), int.Parse(cloudData["vest"].ToString()), int.Parse(cloudData["gloves"].ToString()), int.Parse(cloudData["pad"].ToString()), int.Parse(cloudData["belt"].ToString()), int.Parse(cloudData["shoes"].ToString()) };
                    }
                    else
                    {
                        GameManager.gm.playerProgress(float.Parse(cloudData["Lvl"].ToString()), float.Parse(cloudData["Exp"].ToString()));
                    }
                }
            }
        }
    }

    public IEnumerator readQueryData(int slot, string dataPath)
    {
        string[] path = dataPath.Split('/');

        Task<QuerySnapshot> readTask = database.Collection("UserData").Document(userSign.User.UserId).Collection(path[0]).Document(path[1]).Collection(path[2]).GetSnapshotAsync();
        yield return new WaitUntil(() => readTask.IsCompleted);

        if (readTask.IsFaulted || readTask.IsCanceled)
        {
            yield break;
        }

        QuerySnapshot queryBackpack = null;
        QuerySnapshot queryWarehouse = null;

        if (path[0] == "Backpack") queryBackpack = readTask.Result;
        if (path[0] == "Warehouse") queryWarehouse = readTask.Result;

        int idx = 0;

        if (queryBackpack != null)
        {
            for (int i = 0; i < DataManager.dm.backpack[slot].bag.Length; i++)
            {
                if (string.IsNullOrEmpty(DataManager.dm.backpack[slot].bag[i].names))
                {
                    idx = i;

                    break;
                }
            }

            foreach (DocumentSnapshot backpack in queryBackpack.Documents)
            {
                cloudData = new Dictionary<string, object> { };
                cloudData = backpack.ToDictionary();
                if (cloudData != null)
                {
                    //idx = int.Parse(cloudData["idx"].ToString());

                    DataManager.dm.backpack[slot].bag[idx].names = backpack.Id.ToString();
                    DataManager.dm.backpack[slot].bag[idx].type = cloudData["type"].ToString();
                    DataManager.dm.backpack[slot].bag[idx].cat = cloudData["cat"].ToString();
                    DataManager.dm.backpack[slot].bag[idx].idx = int.Parse(cloudData["idx"].ToString());
                    DataManager.dm.backpack[slot].bag[idx].lvl = int.Parse(cloudData["lvl"].ToString());
                    DataManager.dm.backpack[slot].bag[idx].amount = float.Parse(cloudData["amount"].ToString());

                    idx++;
                }
            }
        }

        if (queryWarehouse != null)
        {
            for (int i = 0; i < DataManager.dm.warehouse[slot].rack.Length; i++)
            {
                if (string.IsNullOrEmpty(DataManager.dm.warehouse[slot].rack[i].names))
                {
                    idx = i;

                    break;
                }
            }

            foreach (DocumentSnapshot warehouse in queryWarehouse.Documents)
            {
                cloudData = new Dictionary<string, object> { };
                cloudData = warehouse.ToDictionary();
                if (cloudData != null)
                {
                    //idx = int.Parse(cloudData["idx"].ToString());

                    DataManager.dm.warehouse[slot].rack[idx].names = warehouse.Id.ToString();
                    DataManager.dm.warehouse[slot].rack[idx].type = cloudData["type"].ToString();
                    DataManager.dm.warehouse[slot].rack[idx].cat = cloudData["cat"].ToString();
                    DataManager.dm.warehouse[slot].rack[idx].idx = int.Parse(cloudData["idx"].ToString());
                    DataManager.dm.warehouse[slot].rack[idx].lvl = int.Parse(cloudData["lvl"].ToString());
                    DataManager.dm.warehouse[slot].rack[idx].amount = float.Parse(cloudData["amount"].ToString());

                    idx++;
                }
            }
        }
    }

    public void saveShortData(string dataKey, string[] pKey, object[] pData)
    {
        StartCoroutine(savingShortData(dataKey, pKey, pData));
    }

    IEnumerator savingShortData(string dataKey, string[] pKey, object[] pData)
    {
        string[] path = dataKey.Split('/');
        cloudData = new Dictionary<string, object> { };
        for (int i = 0; i < pKey.Length; i++)
        {
            cloudData.Add(pKey[i], pData[i]);
        }

        Task writeTask = database.Collection("UserData").Document(userSign.User.UserId).Collection(path[0]).Document(path[1]).SetAsync(cloudData);
        yield return new WaitUntil(() => writeTask.IsCompleted);
    }

    public void saveLongData(string dataKey, string[] pKey, object[] pData, RefineryManager rm = null)
    {
        StartCoroutine(savingLongData(dataKey, pKey, pData, rm));
    }

    IEnumerator savingLongData(string dataKey, string[] pKey, object[] pData, RefineryManager rm = null)
    {
        string[] path = dataKey.Split('/');
        cloudData = new Dictionary<string, object> { };
        for (int i = 0; i < pKey.Length; i++)
        {
            cloudData.Add(pKey[i], pData[i]);
        }

        Task writeTask = database.Collection("UserData").Document(userSign.User.UserId).Collection(path[0]).Document(path[1]).Collection(path[2]).Document(path[3]).SetAsync(cloudData);
        yield return new WaitUntil(() => writeTask.IsCompleted);

        if (rm != null) rm.openrefinery(rm.lastPlace);
    }

    public void updateShortData(string dataKey, string[] pKey, object[] pData)
    {
        StartCoroutine(updatingShortData(dataKey, pKey, pData));
    }

    IEnumerator updatingShortData(string dataKey, string[] pKey, object[] pData)
    {
        string[] path = dataKey.Split('/');
        cloudData = new Dictionary<string, object> { };
        for (int i = 0; i < pKey.Length; i++)
        {
            cloudData.Add(pKey[i], pData[i]);
        }

        Task writeTask = database.Collection("UserData").Document(userSign.User.UserId).Collection(path[0]).Document(path[1]).UpdateAsync(cloudData);
        yield return new WaitUntil(() => writeTask.IsCompleted);

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            readingProfile();
        }
    }

    public void updateLongData(string dataKey, string[] pKey, object[] pData, RefineryManager rm = null)
    {
        StartCoroutine(updatingLongData(dataKey, pKey, pData, rm));
    }

    IEnumerator updatingLongData(string dataKey, string[] pKey, object[] pData, RefineryManager rm = null)
    {
        string[] path = dataKey.Split('/');
        cloudData = new Dictionary<string, object> { };
        for (int i = 0; i < pKey.Length; i++)
        {
            cloudData.Add(pKey[i], pData[i]);
        }

        Task writeTask = database.Collection("UserData").Document(userSign.User.UserId).Collection(path[0]).Document(path[1]).Collection(path[2]).Document(path[3]).UpdateAsync(cloudData);
        yield return new WaitUntil(() => writeTask.IsCompleted);

        if (rm != null) rm.openrefinery(rm.lastPlace);
    }

    public void readLongData(string dataKey, RefineryManager rm = null)
    {
        StartCoroutine(longDataRead(dataKey, rm));
    }

    IEnumerator longDataRead(string dataPath, RefineryManager rm = null)
    {
        int ordr = 0;
        int lvl = 0;
        float amnt = 0;
        long fnsh = 0;

        string[] path = dataPath.Split('/');

        Task<DocumentSnapshot> readTask = database.Collection("UserData").Document(userSign.User.UserId).Collection(path[0]).Document(path[1]).Collection(path[2]).Document(path[3]).GetSnapshotAsync();
        yield return new WaitUntil(() => readTask.IsCompleted);

        cloudData = new Dictionary<string, object> { };
        cloudData = readTask.Result.ToDictionary();

        if (cloudData != null)
        {
            if(rm != null)
            {
                ordr = int.Parse(cloudData["order"].ToString());
                lvl = int.Parse(cloudData["level"].ToString());
                amnt = float.Parse(cloudData["amount"].ToString());
                fnsh = long.Parse(cloudData["finish"].ToString());

                if (amnt > 0)
                {
                    rm.working(ordr, lvl, fnsh, amnt);
                    yield break;
                }
            }
        }

        if (rm == null)
            yield break;

        readTask = database.Collection("UserData").Document(userSign.User.UserId).Collection(path[0]).Document(path[1]).Collection(path[2]).Document(path[4]).GetSnapshotAsync();
        yield return new WaitUntil(() => readTask.IsCompleted);
        
        cloudData = new Dictionary<string, object> { };
        cloudData = readTask.Result.ToDictionary();

        if (cloudData != null)
        {
            ordr = int.Parse(cloudData["order"].ToString());
            lvl = int.Parse(cloudData["level"].ToString());
            amnt = float.Parse(cloudData["amount"].ToString());
            fnsh = long.Parse(cloudData["finish"].ToString());

            if (amnt > 0)
            {
                rm.working(ordr, lvl, fnsh, amnt);
            }
        }
    }
}
