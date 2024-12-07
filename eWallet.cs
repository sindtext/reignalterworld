using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Eidolon.Provider;
using Eidolon.Wallets;

public class eWallet : MonoBehaviour
{
    public static eWallet call;

    LobbyManager lm;

    public rawChangerManager rcm;
    public rawBankManager rbm;

    public GameObject connectBtn;
    public GameObject createBtn;
    public GameObject disconnectBtn;
    public GameObject walletLoad;
    public Transform tokenContainer;
    public TMP_Text walletDisplay;
    public GameObject rawbankBtn;
    public GameObject u2uFObj;
    public GameObject metisFObj;
    public GameObject emcFObj;
    public GameObject atomFObj;
    public GameObject taikoFObj;
    public GameObject gachaObj;

    public string skaleID;
    public string skaleKey;
    public EmbeddedWallet wallet;
    public EmbeddedWallet u2uwallet;
    public EmbeddedWallet metiswallet;
    public EmbeddedWallet emcwallet;
    public EmbeddedWallet atomwallet;
    public EmbeddedWallet taikowallet;
    public EmbeddedWallet zytronwallet;

    public string account;

    public TextMeshProUGUI statusText;
    public string faucetWallet = "7c7d22995926c3410ddf43404a76d8e63ba1f0454ccd0b559cbb1fd53226ab9c";

    private void Awake()
    {
        if (call)
            return;

        call = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        lm = FindObjectOfType<LobbyManager>();
    }

    public IEnumerator checkWallet(bool newWallet = false)
    {
        rbm = FindObjectOfType<rawBankManager>(true);
        rcm = FindObjectOfType<rawChangerManager>(true);

        rbm.initRawBank();
        rcm.initRawChanger();

        string signer = "XxX";
        signer = Signer.Load(skaleID, skaleKey);
        yield return new WaitWhile(() => signer == "XxX");

        if (signer == null)
        {
            createBtn.SetActive(true);
            connectBtn.SetActive(false);
            disconnectBtn.SetActive(false);
            gachaObj.SetActive(false);
        }
        else
        {
            createBtn.SetActive(false);
            connectBtn.SetActive(true);
            disconnectBtn.SetActive(false);
            gachaObj.SetActive(true);
        }

        if(newWallet) createBtn.transform.parent.GetChild(3).gameObject.SetActive(true);
    }

    public void endWallet()
    {
        walletDisplay.gameObject.SetActive(false);
        rawbankBtn.SetActive(false);
        createBtn.SetActive(false);
        connectBtn.SetActive(true);
        disconnectBtn.SetActive(false);
        walletLoad.SetActive(true);
        walletLoad.GetComponent<TMP_Text>().text = "Please Connect your Wallet...";
        tokenContainer.GetChild(0).gameObject.SetActive(false);
        tokenContainer.GetChild(1).gameObject.SetActive(false);
        tokenContainer.GetChild(2).gameObject.SetActive(false);
    }

    public void SignUpSkale()
    {
        lm.exeLoader.SetActive(true);
        statusText.text = "Creating Wallet...";

        bool signUpSuccess = Signer.Create(skaleID, skaleKey);

        if (signUpSuccess)
        {
            statusText.text = "Account created successfully!";
        }
        else
        {
            statusText.text = "Username already exists. Login...";
        }
    }

    public void LoginSkaleBtn()
    {
        StartCoroutine(LoginSkale());
    }

    public IEnumerator LoginSkale()
    {
        lm.exeLoader.SetActive(true);
        statusText.text = "Signing In...";

        string signer = "XxX";
        signer = Signer.Load(skaleID, skaleKey);
        yield return new WaitWhile(() => signer == "XxX");

        if (signer == null)
        {
            statusText.text = "Wrong Account/Password!!! Please Contact GM";
        }
        else
        {
            if(wallet == null)
            {
                wallet = new EmbeddedWallet(signer, "37084624", new JsonRpcProvider("https://testnet.skalenodes.com/v1/lanky-ill-funny-testnet"));
                yield return new WaitWhile(() => wallet == null);
                u2uwallet = new EmbeddedWallet(signer, "2484", new JsonRpcProvider("https://rpc-nebulas-testnet.uniultra.xyz/"));
                yield return new WaitWhile(() => u2uwallet == null);
                emcwallet = new EmbeddedWallet(signer, "99876", new JsonRpcProvider("https://rpc2-testnet.emc.network"));
                yield return new WaitWhile(() => emcwallet == null);
                taikowallet = new EmbeddedWallet(signer, "167009", new JsonRpcProvider("https://rpc.hekla.taiko.xyz"));
                yield return new WaitWhile(() => taikowallet == null);
                zytronwallet = new EmbeddedWallet(signer, "50098", new JsonRpcProvider("https://rpc-testnet.zypher.network"));
                yield return new WaitWhile(() => zytronwallet == null);
            }

            account = wallet.GetAddress();
            statusText.text = "Logged in Successfully! Connected Address: " + account;

            walletDisplay.text = account;

            StartCoroutine(u2u.call.u2uContract());
            StartCoroutine(iEMC.call.emcContract());
            StartCoroutine(iTaiko.call.taikoContract());
        }
    }

    public void copyFauchet(string urls)
    {
        UniClipboard.SetText(eWallet.call.account);

        Application.OpenURL(urls);
    }
}
