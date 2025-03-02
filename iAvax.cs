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

public class iAvax : MonoBehaviour
{
    public static iAvax call;

    LobbyManager lm;

    public EmbeddedWallet avaxWallet;
    SmartContract Avax;
    SmartContract avax0Gas;
    SmartContract avaxGacha;

    JsonRpcProvider provider;

    public TextMeshProUGUI statusText;
    public TMP_Text avaxDisplay;
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
        provider = new JsonRpcProvider("https://api.avax-test.network/ext/bc/C/rpc");
    }

    public IEnumerator avaxContract()
    {
        string signer = "XxX";
        signer = Signer.Load(eWallet.call.ID, eWallet.call.Key);
        yield return new WaitWhile(() => signer == "XxX");

        avaxWallet = new EmbeddedWallet(signer, "43113", provider);
        yield return new WaitWhile(() => avaxWallet == null);

        if (avax0Gas == null)
        {
            EmbeddedWallet faucetwallet = new EmbeddedWallet(eWallet.call.faucetWallet, "43113", provider);
            yield return new WaitWhile(() => faucetwallet == null);
            avax0Gas = new SmartContract(rawAvaxFaucetManager.Address, rawAvaxFaucetManager.ABI, faucetwallet);
            yield return new WaitWhile(() => avax0Gas == null);
        }

        avaxConnect();
    }

    public async void avaxConnect()
    {
        statusText.text = "Connecting to AVAX Network...";

        string accnt = avaxWallet.GetAddress();

        var gasBalance = await provider.GetBalance(accnt);
        avaxDisplay.transform.parent.gameObject.SetActive(true);
        avaxDisplay.text = ((Mathf.Floor((float)gasBalance * 1000000000)) / 1000000000).ToString("F9") + " AVAX";

        if (float.Parse(gasBalance.ToString()) > 0.56f)
        {
            statusText.text = "AVAX Network Connected Successfully!";
            avaxSign("Connect to Reign Alter World Store");
        }
        else
        {
            statusText.text = "Claim AVAX Fauchet!";

            avaxFauchetCall(accnt);
        }
    }

    public async void avaxFauchetCall(string receiver)
    {
        string methodName = "AvaxPay";

        object[] arguments = new object[] {
            receiver
        };

        try
        {
            var transactionHash = await avax0Gas.SendTransaction(methodName, gas: "100000", parameters: arguments);

            statusText.text = "AVAX distributed successfully.";
            avaxConnect();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            statusText.text = "AVAX distribution failed.";
        }
    }

    // You can also have users sign messages using Embedded wallets:
    public async void avaxSign(string message)
    {
        // This will return a signature
        string signature = await avaxWallet.SignMessage(message);

        statusText.text = "AVAX Signed : " + message;
    }

    public async Task<BigInteger> avaxAllowance<BigInteger>(string owner, string spender)
    {
        string methodName = "allowance";

        object[] arguments = new object[] {
            owner,
            spender
        };

        try
        {
            var status = await Avax.Call<BigInteger>(methodName, arguments);
            Debug.Log("Allowance Call Result: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> avaxApprove(string spender, BigInteger value)
    {
        string methodName = "approve";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await Avax.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
            return transactionHash;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async void avaxTransfer(string spender, BigInteger value)
    {
        string methodName = "transfer";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await Avax.SendTransaction(methodName, gas: "100000", parameters: arguments);
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
        yield return new WaitWhile(() => avaxWallet == null);
        avaxGacha = new SmartContract(AvaxGachaManager.Address, AvaxGachaManager.ABI, avaxWallet);
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
            var status = await avaxGacha.Call<BigInteger>(methodName, arguments);
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
            var status = await avaxGacha.Call<bool>(methodName, arguments);
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
            var transactionHash = await avaxGacha.SendTransaction(methodName, gas: "100000", parameters: arguments);
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
            var transactionHash = await avaxGacha.SendTransaction(methodName, gas: "100000", parameters: arguments);
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
            var status = await avaxGacha.Call<BigInteger>(methodName, arguments);
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
