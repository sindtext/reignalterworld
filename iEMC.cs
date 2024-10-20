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
    string emc0GasAddress;
    string emc0GasABI;

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
        emc0Gas = new SmartContract(emc0GasAddress, emc0GasABI, eWallet.call.emcwallet, true);
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
            eWallet.call.emcFObj.SetActive(true);
            eWallet.call.emcFObj.transform.GetChild(0).GetComponent<TMP_Text>().text = accnt;
            lm.exeLoader.SetActive(false);
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
