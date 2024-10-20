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

public class iAtom : MonoBehaviour
{
    public static iAtom call;

    LobbyManager lm;

    SmartContract Atom;
    string atomAddress;
    string atomABI;
    SmartContract atom0Gas;
    string atom0GasAddress;
    string atom0GasABI;

    JsonRpcProvider provider;

    public rawChangerManager rcm;
    public rawBankManager rbm;

    public TextMeshProUGUI statusText;
    public TMP_Text atomDisplay;

    private void Awake()
    {
        if (call)
            return;

        call = this;
    }

    private void Start()
    {
        lm = FindObjectOfType<LobbyManager>();
        provider = new JsonRpcProvider("https://rpc-rs.cosmos.nodestake.top");
        atomContract();
    }

    public void atomContract()
    {
        Atom = new SmartContract(atomAddress, atomABI, eWallet.call.atomwallet, true);
        atom0Gas = new SmartContract(atom0GasAddress, atom0GasABI, eWallet.call.atomwallet, true);
        atomConnect();
    }

    public async void atomConnect()
    {
        statusText.text = "Connecting to Atom...";

        string accnt = eWallet.call.atomwallet.GetAddress();

        var gasBalance = await provider.GetBalance(accnt);
        atomDisplay.text = gasBalance.ToString();

        if (float.Parse(gasBalance.ToString()) > 0.000005f)
        {
            statusText.text = "Atom Connected Successfully!";
            atomSign("Connect to Reign Alter World Store");
        }
        else
        {
            statusText.text = "Claim Atom Fauchet!";
            eWallet.call.atomFObj.SetActive(true);
            eWallet.call.atomFObj.transform.GetChild(0).GetComponent<TMP_Text>().text = accnt;
            lm.exeLoader.SetActive(false);
        }
    }

    // You can also have users sign messages using Embedded wallets:
    public async void atomSign(string message)
    {
        // This will return a signature
        string signature = await eWallet.call.atomwallet.SignMessage(message);

        statusText.text = "Signature: " + signature;
        lm.exeLoader.SetActive(false);
    }

    public async Task<BigInteger> atomAllowance<BigInteger>(string owner, string spender)
    {
        string methodName = "allowance";

        object[] arguments = new object[] {
            owner,
            spender
        };

        try
        {
            var status = await Atom.Call<BigInteger>(methodName, arguments);
            Debug.Log("Allowance Call Result: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> atomApprove(string spender, BigInteger value)
    {
        string methodName = "approve";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await Atom.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
            return transactionHash;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async void atomTransfer(string spender, BigInteger value)
    {
        string methodName = "transfer";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await Atom.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }
}
