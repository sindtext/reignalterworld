using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Eidolon.Wallets;
using Eidolon.Provider;
using Eidolon.SmartContracts;
using Eidolon.Util;
using System.Collections;
using Org.BouncyCastle.Asn1.Cmp;

public class iCore : MonoBehaviour
{
    public static iCore call;

    LobbyManager lm;

    public EmbeddedWallet coreWallet;
    SmartContract Core;
    SmartContract core0Gas;
    SmartContract coreGacha;

    JsonRpcProvider provider;

    public TextMeshProUGUI statusText;
    public TMP_Text coreDisplay;
    int gachaValue;
    public TMP_InputField gachaText;
    public GameObject gachaResult;
    public GameObject gachaBtn;

    private void Awake()
    {
        if (call)
            return;

        call = this;
    }

    private void Start()
    {
        lm = FindObjectOfType<LobbyManager>();
        provider = new JsonRpcProvider("https://rpc.ankr.com/klaytn_testnet");
    }

    public IEnumerator coreContract()
    {
        string signer = "XxX";
        signer = Signer.Load(eWallet.call.ID, eWallet.call.Key);
        yield return new WaitWhile(() => signer == "XxX");

        coreWallet = new EmbeddedWallet(signer, "1001", provider);
        yield return new WaitWhile(() => coreWallet == null);

        if (core0Gas == null)
        {
            EmbeddedWallet faucetwallet = new EmbeddedWallet(eWallet.call.faucetWallet, "1001", provider);
            yield return new WaitWhile(() => faucetwallet == null);
            core0Gas = new SmartContract(rawCoreFaucetManager.Address, rawCoreFaucetManager.ABI, faucetwallet);
            yield return new WaitWhile(() => core0Gas == null);
        }

        coreConnect();
    }

    public async void coreConnect()
    {
        statusText.text = "Connecting to KAIA Network...";

        string accnt = coreWallet.GetAddress();

        var gasBalance = await provider.GetBalance(accnt);
        coreDisplay.transform.parent.gameObject.SetActive(true);
        coreDisplay.text = ((Mathf.Floor((float)gasBalance * 1000000000)) / 1000000000).ToString("F9") + " KAIA";

        if (float.Parse(gasBalance.ToString()) > 0.56f)
        {
            statusText.text = "KAIA Network Connected Successfully!";
            coreSign("Connect to Reign Alter World Store");
        }
        else
        {
            statusText.text = "Claim KAIA Fauchet!";

            coreFauchetCall(accnt);
        }
    }

    public async void coreFauchetCall(string receiver)
    {
        string methodName = "CorePay";

        object[] arguments = new object[] {
            receiver
        };

        try
        {
            var transactionHash = await core0Gas.SendTransaction(methodName, gas: "100000", parameters: arguments);

            statusText.text = "KAIA distributed successfully.";
            coreConnect();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            statusText.text = "KAIA distribution failed.";
        }
    }

    // You can also have users sign messages using Embedded wallets:
    public async void coreSign(string message)
    {
        // This will return a signature
        string signature = await coreWallet.SignMessage(message);

        statusText.text = "KAIA Signed : " + message;
    }

    public async Task<BigInteger> coreAllowance<BigInteger>(string owner, string spender)
    {
        string methodName = "allowance";

        object[] arguments = new object[] {
            owner,
            spender
        };

        try
        {
            var status = await Core.Call<BigInteger>(methodName, arguments);
            Debug.Log("Allowance Call Result: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> coreApprove(string spender, BigInteger value)
    {
        string methodName = "approve";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await Core.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
            return transactionHash;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async void coreTransfer(string spender, BigInteger value)
    {
        string methodName = "transfer";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await Core.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public void checkGacha()
    {
        gachaBtn.transform.parent.GetChild(1).gameObject.SetActive(false);
        StartCoroutine(checkingGacha());
    }

    IEnumerator checkingGacha()
    {
        yield return new WaitWhile(() => coreWallet == null);
        coreGacha = new SmartContract(CoreGachaManager.Address, CoreGachaManager.ABI, coreWallet);
        checkedGacha();
    }

    public async void checkedGacha()
    {
        bool gachaReady = await exeCheckGacha();
        int lastsign = 0;
        BigInteger tempValue = await exeLastSign<BigInteger>();
        lastsign = (int)tempValue;

        if (!gachaReady && lastsign != 0)
        {
            gachaBtn.transform.parent.GetChild(1).gameObject.SetActive(false);
            gachaBtn.SetActive(true);
            gachaBtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Redeemed";
            gachaBtn.GetComponent<Button>().interactable = false;
            gachaText.text = "REDEEMED";
            gachaResult.SetActive(false);
        }
        else
        {
            gachaBtn.transform.parent.GetChild(1).gameObject.SetActive(true);
            gachaBtn.SetActive(false);
            gachaText.text = "::::::::::::::::";
            gachaResult.SetActive(false);
        }
    }

    public async void runGacha()
    {
        lm.exeLoader.SetActive(true);
        var gasBalance = await provider.GetBalance(eWallet.call.account);
        Debug.Log(gasBalance.ToString());

        if (float.Parse(gasBalance.ToString()) > 0.56f)
        {
            bool gachaReady = await exeCheckGacha();
            if (gachaReady)
            {
                prepareGacha();
            }
            else
            {
                int lastsign = 0;
                BigInteger tempValue = await exeLastSign<BigInteger>();
                lastsign = (int)tempValue;

                if (lastsign == 0)
                {
                    await exeSignGacha();
                    prepareGacha();
                }
                else
                {
                    gachaBtn.transform.parent.GetChild(1).gameObject.SetActive(false);
                    gachaBtn.SetActive(true);
                    gachaBtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Redeemed";
                    gachaBtn.GetComponent<Button>().interactable = false;
                    gachaText.text = "REDEEMED";
                    gachaResult.SetActive(false);
                    lm.exeLoader.SetActive(false);
                }
            }
        }
    }

    async void prepareGacha()
    {
        await exeGacha();
        BigInteger tempValue = await exeLastGacha<BigInteger>();
        gachaValue = (int)tempValue;
        gachaText.text = gachaValue.ToString();
        gachaResult.SetActive(false);
        gachaBtn.SetActive(true);
        gachaBtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Redeem Ticket";
        gachaBtn.GetComponent<Button>().interactable = true;
        lm.exeLoader.SetActive(false);
    }

    public void redeemGacha()
    {
        gachaBtn.transform.parent.GetChild(1).gameObject.SetActive(false);
        gachaBtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Redeemed";
        gachaBtn.GetComponent<Button>().interactable = false;
        lm.exeLoader.SetActive(true);

        float pow = gachaValue >= 10000 ? 4 : gachaValue < 100 ? 2 : 3;
        float luck = Mathf.Pow(10, gachaValue % pow);

        gachaResult.transform.GetChild(0).GetComponent<TMP_Text>().text = luck.ToString("N");
        gachaResult.transform.GetChild(1).gameObject.SetActive(pow == 4);
        gachaResult.transform.GetChild(2).gameObject.SetActive(pow == 3);
        gachaResult.transform.GetChild(3).gameObject.SetActive(pow == 2);

        string currency = pow == 4 ? "rawbrz" : pow == 3 ? "rawslv" : "rawgld";
        StartCoroutine(iSkale.call.savingProcessor(luck.ToString(), currency));
    }

    public async Task<BigInteger> exeLastSign<BigInteger>()
    {
        string methodName = "LastSign";

        object[] arguments = new object[] {
            eWallet.call.account
        };

        try
        {
            var status = await coreGacha.Call<BigInteger>(methodName, arguments);
            Debug.Log("Result: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<bool> exeCheckGacha()
    {
        string methodName = "CheckGacha";

        object[] arguments = new object[] {
            eWallet.call.account
        };

        try
        {
            var status = await coreGacha.Call<bool>(methodName, arguments);
            Debug.Log("Transaction Hash: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> exeSignGacha()
    {
        string methodName = "SignGacha";

        object[] arguments = new object[] {

        };

        try
        {
            var transactionHash = await coreGacha.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
            return transactionHash;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> exeGacha()
    {
        string methodName = "Gacha";

        object[] arguments = new object[] {

        };

        try
        {
            var transactionHash = await coreGacha.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
            return transactionHash;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<BigInteger> exeLastGacha<BigInteger>()
    {
        string methodName = "LastGacha";

        object[] arguments = new object[] {
            eWallet.call.account
        };

        try
        {
            var status = await coreGacha.Call<BigInteger>(methodName, arguments);
            Debug.Log("Result: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }
}
