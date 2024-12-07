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

public class iTaiko : MonoBehaviour
{
    public static iTaiko call;

    SmartContract Taiko;
    SmartContract taiko0Gas;

    JsonRpcProvider provider;

    public rawChangerManager rcm;
    public rawBankManager rbm;

    public TextMeshProUGUI statusText;
    public TMP_Text taikoDisplay;

    private void Awake()
    {
        if (call)
            return;

        call = this;
    }

    private void Start()
    {
        provider = new JsonRpcProvider("https://rpc.hekla.taiko.xyz");
        //taikoContract();
    }

    public IEnumerator taikoContract()
    {
        if (taiko0Gas == null)
        {
            //Taiko = new SmartContract(emcAddress, emcABI, eWallet.call.emcwallet, "emc");
            EmbeddedWallet faucetwallet = new EmbeddedWallet(eWallet.call.faucetWallet, "167009", provider);
            yield return new WaitWhile(() => faucetwallet == null);
            taiko0Gas = new SmartContract(rawTaikoFaucetManager.Address, rawTaikoFaucetManager.ABI, faucetwallet);
            yield return new WaitWhile(() => taiko0Gas == null);
        }
        taikoConnect();
    }

    public async void taikoConnect()
    {
        statusText.text += "\n Connecting to Taiko Network...";

        string accnt = eWallet.call.taikowallet.GetAddress();

        var gasBalance = await provider.GetBalance(accnt);
        taikoDisplay.text = gasBalance.ToString();

        if (float.Parse(gasBalance.ToString()) > 0.000005f)
        {
            statusText.text += "\n Taiko Network Connected Successfully!";
            taikoSign("Connect to Reign Alter World Store");
        }
        else
        {
            statusText.text += "\n Claim Taiko Fauchet!";

            taikoFauchetCall(accnt);
        }
    }

    public async void taikoFauchetCall(string receiver)
    {
        string methodName = "taikoPay";

        object[] arguments = new object[] {
            receiver
        };

        try
        {
            var transactionHash = await taiko0Gas.SendTransaction(methodName, gas: "100000", parameters: arguments);

            statusText.text += "\n Taiko ETH distributed successfully.";
            taikoConnect();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            statusText.text += "\n Taiko ETH distribution failed.";
        }
    }

    // You can also have users sign messages using Embedded wallets:
    public async void taikoSign(string message)
    {
        // This will return a signature
        string signature = await eWallet.call.taikowallet.SignMessage(message);

        statusText.text = "Taiko Signed : " + message;
    }

    public async Task<BigInteger> taikoAllowance<BigInteger>(string owner, string spender)
    {
        string methodName = "allowance";

        object[] arguments = new object[] {
            owner,
            spender
        };

        try
        {
            var status = await Taiko.Call<BigInteger>(methodName, arguments);
            Debug.Log("Allowance Call Result: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> taikoApprove(string spender, BigInteger value)
    {
        string methodName = "approve";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await Taiko.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
            return transactionHash;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async void taikoTransfer(string spender, BigInteger value)
    {
        string methodName = "transfer";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await Taiko.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }
}
