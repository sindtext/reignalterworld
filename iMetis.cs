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

public class iMetis : MonoBehaviour
{
    public static iMetis call;

    LobbyManager lm;

    SmartContract Metis;
    string metisAddress;
    string metisABI;
    SmartContract metis0Gas;
    string metis0GasAddress;
    string metis0GasABI;

    JsonRpcProvider provider;

    public rawChangerManager rcm;
    public rawBankManager rbm;

    public TextMeshProUGUI statusText;
    public TMP_Text metisDisplay;

    private void Awake()
    {
        if (call)
            return;

        call = this;
    }

    private void Start()
    {
        lm = FindObjectOfType<LobbyManager>();
        provider = new JsonRpcProvider("https://sepolia.metisdevops.link");
        metisContract();
    }

    public void metisContract()
    {
        Metis = new SmartContract(metisAddress, metisABI, eWallet.call.metiswallet, "metis");
        metis0Gas = new SmartContract(metis0GasAddress, metis0GasABI, eWallet.call.metiswallet, "metis");
        metisConnect();
    }

    public async void metisConnect()
    {
        statusText.text = "Connecting to METIS...";

        string accnt = eWallet.call.metiswallet.GetAddress();

        var gasBalance = await provider.GetBalance(accnt);
        metisDisplay.text = gasBalance.ToString();

        if (float.Parse(gasBalance.ToString()) > 0.000005f)
        {
            statusText.text = "METIS Connected Successfully!";
            metisSign("Connect to Reign Alter World Store");
        }
        else
        {
            statusText.text = "Claim METIS Fauchet!";
            eWallet.call.metisFObj.SetActive(true);
            eWallet.call.metisFObj.transform.GetChild(0).GetComponent<TMP_Text>().text = accnt;
            lm.exeLoader.SetActive(false);
        }
    }

    // You can also have users sign messages using Embedded wallets:
    public async void metisSign(string message)
    {
        // This will return a signature
        string signature = await eWallet.call.metiswallet.SignMessage(message);

        statusText.text = "Signature: " + signature;
        lm.exeLoader.SetActive(false);
    }

    public async Task<BigInteger> metisAllowance<BigInteger>(string owner, string spender)
    {
        string methodName = "allowance";

        object[] arguments = new object[] {
            owner,
            spender
        };

        try
        {
            var status = await Metis.Call<BigInteger>(methodName, arguments);
            Debug.Log("Allowance Call Result: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> metisApprove(string spender, BigInteger value)
    {
        string methodName = "approve";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await Metis.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
            return transactionHash;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async void metisTransfer(string spender, BigInteger value)
    {
        string methodName = "transfer";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await Metis.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }
}
