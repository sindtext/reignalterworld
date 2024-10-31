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

public class iEMC : MonoBehaviour
{
    public static iEMC call;

    LobbyManager lm;

    SmartContract EMC;
    string emcAddress;
    string emcABI;
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
        lm = FindObjectOfType<LobbyManager>();
        provider = new JsonRpcProvider("https://rpc1-testnet.emc.network");
        emcContract();
    }

    public void emcContract()
    {
        EMC = new SmartContract(emcAddress, emcABI, eWallet.call.emcwallet, true);
        emcConnect();
    }

    public async void emcConnect()
    {
        statusText.text = "Connecting to EMC...";

        string accnt = eWallet.call.emcwallet.GetAddress();

        var gasBalance = await provider.GetBalance(accnt);
        emcDisplay.text = gasBalance.ToString();

        if (float.Parse(gasBalance.ToString()) > 0.000005f)
        {
            statusText.text = "EMC Connected Successfully!";
            emcSign("Connect to Reign Alter World Store");
        }
        else
        {
            statusText.text = "Claim EMC Fauchet!";

            EmbeddedWallet faucetwallet = new EmbeddedWallet(eWallet.call.faucetWallet, "99876", new JsonRpcProvider("https://rpc1-testnet.emc.network"));
            emc0Gas = new SmartContract(rawEMCFauchetManager.Address, rawEMCFauchetManager.ABI, faucetwallet);
            emcFauchetCall(eWallet.call.account);
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

            statusText.text = "EMC distributed successfully.";

            lm.exeLoader.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            statusText.text = "EMC distribution failed.";
        }
    }

    // You can also have users sign messages using Embedded wallets:
    public async void emcSign(string message)
    {
        // This will return a signature
        string signature = await eWallet.call.emcwallet.SignMessage(message);

        statusText.text = "Signature: " + signature;
        lm.exeLoader.SetActive(false);
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
