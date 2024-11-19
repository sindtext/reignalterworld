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

public class iTaiko : MonoBehaviour
{
    public static iTaiko call;

    LobbyManager lm;

    SmartContract Taiko;
    string taikoAddress;
    string taikoABI;
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
        lm = FindObjectOfType<LobbyManager>();
        provider = new JsonRpcProvider("https://rpc.hekla.taiko.xyz");
        taikoContract();
    }

    public void taikoContract()
    {
        Taiko = new SmartContract(taikoAddress, taikoABI, eWallet.call.taikowallet, true);
        taikoConnect();
    }

    public async void taikoConnect()
    {
        statusText.text = "Connecting to Taiko Network...";

        string accnt = eWallet.call.taikowallet.GetAddress();

        var gasBalance = await provider.GetBalance(accnt);
        taikoDisplay.text = gasBalance.ToString();

        if (float.Parse(gasBalance.ToString()) > 0.000005f)
        {
            statusText.text = "Taiko Network Connected Successfully!";
            taikoSign("Connect to Reign Alter World Store");
        }
        else
        {
            statusText.text = "Claim Taiko Fauchet!";

            EmbeddedWallet faucetwallet = new EmbeddedWallet(eWallet.call.faucetWallet, "167009", new JsonRpcProvider("https://rpc.hekla.taiko.xyz"));
            taiko0Gas = new SmartContract(rawTaikoFaucetManager.Address, rawTaikoFaucetManager.ABI, faucetwallet);
            taikoFauchetCall(eWallet.call.account);
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

            statusText.text = "Taiko ETH distributed successfully.";

            lm.exeLoader.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            statusText.text = "Taiko ETH distribution failed.";
        }
    }

    // You can also have users sign messages using Embedded wallets:
    public async void taikoSign(string message)
    {
        // This will return a signature
        string signature = await eWallet.call.taikowallet.SignMessage(message);

        statusText.text = "Signature: " + signature;
        lm.exeLoader.SetActive(false);
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
