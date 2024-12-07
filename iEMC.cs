using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Eidolon.Wallets;
using Eidolon.Provider;
using Eidolon.SmartContracts;
using Eidolon.Util;
using System.Collections;

public class iEMC : MonoBehaviour
{
    public static iEMC call;

    SmartContract EMC;
    SmartContract emc0Gas;

    JsonRpcProvider provider;

    public rawChangerManager rcm;
    public rawBankManager rbm;

    public TextMeshProUGUI statusText;
    public TMP_Text emcDisplay;

    private void Awake()
    {
        if (call)
            return;

        call = this;
    }

    private void Start()
    {
        provider = new JsonRpcProvider("https://rpc2-testnet.emc.network");
        //emcContract();
    }

    public IEnumerator emcContract()
    {
        if(emc0Gas == null)
        {
            //EMC = new SmartContract(emcAddress, emcABI, eWallet.call.emcwallet, "emc");
            EmbeddedWallet faucetwallet = new EmbeddedWallet(eWallet.call.faucetWallet, "99876", provider);
            yield return new WaitWhile(() => faucetwallet == null);
            emc0Gas = new SmartContract(rawEMCFauchetManager.Address, rawEMCFauchetManager.ABI, faucetwallet);
            yield return new WaitWhile(() => emc0Gas == null);
        }

        emcConnect();
    }

    public async void emcConnect()
    {
        statusText.text += "\n Connecting to EMC Network...";

        string accnt = eWallet.call.emcwallet.GetAddress();

        var gasBalance = await provider.GetBalance(accnt);
        emcDisplay.text = gasBalance.ToString();

        if (float.Parse(gasBalance.ToString()) > 0.000005f)
        {
            statusText.text += "\n EMC Network Connected Successfully!";
            emcSign("Connect to Reign Alter World Store");
        }
        else
        {
            statusText.text += "\n Claim EMC Fauchet!";

            emcFauchetCall(accnt);
        }
    }

    public async void emcFauchetCall(string receiver)
    {
        string methodName = "emcPay";

        object[] arguments = new object[] {
            receiver
        };

        try
        {
            var transactionHash = await emc0Gas.SendTransaction(methodName, gas: "100000", parameters: arguments);

            statusText.text += "\n EMC distributed successfully.";
            emcConnect();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            statusText.text += "\n EMC distribution failed.";
        }
    }

    // You can also have users sign messages using Embedded wallets:
    public async void emcSign(string message)
    {
        // This will return a signature
        string signature = await eWallet.call.emcwallet.SignMessage(message);

        statusText.text = "EMC Signed : " + message;
    }

    public void checkGacha()
    {
        StartCoroutine(checkingGacha());
    }

    IEnumerator checkingGacha()
    {
        yield return new WaitWhile(() => eWallet.call.emcwallet == null);

    }

    public async Task<BigInteger> emcAllowance<BigInteger>(string owner, string spender)
    {
        string methodName = "allowance";

        object[] arguments = new object[] {
            owner,
            spender
        };

        try
        {
            var status = await EMC.Call<BigInteger>(methodName, arguments);
            Debug.Log("Allowance Call Result: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> emcApprove(string spender, BigInteger value)
    {
        string methodName = "approve";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await EMC.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
            return transactionHash;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async void emcTransfer(string spender, BigInteger value)
    {
        string methodName = "transfer";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await EMC.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }
}
