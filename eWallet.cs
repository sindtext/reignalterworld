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
    public GameObject tokenContainer;
    public TMP_Text walletDisplay;
    public GameObject rawbankBtn;
    public GameObject u2uFObj;
    public GameObject metisFObj;
    public GameObject emcFObj;
    public GameObject atomFObj;

    public string skaleID;
    public string skaleKey;
    public EmbeddedWallet wallet;
    public EmbeddedWallet u2uwallet;
    public EmbeddedWallet metiswallet;
    public EmbeddedWallet emcwallet;
    public EmbeddedWallet atomwallet;

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

    public void checkWallet()
    {
        rbm = FindObjectOfType<rawBankManager>(true);
        rcm = FindObjectOfType<rawChangerManager>(true);

        rbm.initRawBank();
        rcm.initRawChanger();

        string signer = Signer.Load(skaleID, skaleKey);

        if (signer == null)
        {
            createBtn.SetActive(true);
            connectBtn.SetActive(false);
            disconnectBtn.SetActive(false);
        }
        else
        {
            createBtn.SetActive(false);
            connectBtn.SetActive(true);
            disconnectBtn.SetActive(false);
        }
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
        tokenContainer.SetActive(false);
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

        LoginSkale();
    }

    public async void LoginSkale()
    {
        lm.exeLoader.SetActive(true);
        statusText.text = "Signing In...";
        string signer = Signer.Load(skaleID, skaleKey);

        if (signer == null)
        {
            statusText.text = "Wrong Account/Password!!! Please Contact GM";
        }
        else
        {
            wallet = new EmbeddedWallet(signer, "37084624", new JsonRpcProvider("https://testnet.skalenodes.com/v1/lanky-ill-funny-testnet"));
            u2uwallet = new EmbeddedWallet(signer, "2484", new JsonRpcProvider("https://rpc-nebulas-testnet.uniultra.xyz/"));
            metiswallet = new EmbeddedWallet(signer, "59902", new JsonRpcProvider("https://sepolia.metisdevops.link"));
            emcwallet = new EmbeddedWallet(signer, "99876", new JsonRpcProvider("https://rpc1-testnet.emc.network"));

            account = wallet.GetAddress();
            statusText.text = "Logged in Successfully! Connected Address: " + account;

            walletDisplay.text = account;

            u2u.call.u2uContract();
        }
    }

    public void copyFauchet(string urls)
    {
        UniClipboard.SetText(eWallet.call.account);

        Application.OpenURL(urls);
    }
}
