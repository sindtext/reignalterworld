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

public class u2u : MonoBehaviour
{
    public static u2u call;

    LobbyManager lm;

    SmartContract RAWS;
    SmartContract RAWBank;
    SmartContract RAWChanger;
    SmartContract u2uFaucet;

    JsonRpcProvider provider;

    public rawChangerManager rcm;
    public rawBankManager rbm;

    public TextMeshProUGUI statusText;
    public TMP_Text u2uDisplay;
    public TMP_Text rawsDisplay;

    private void Awake()
    {
        if (call)
            return;

        call = this;
    }

    private void Start()
    {
        lm = FindObjectOfType<LobbyManager>();
        provider = new JsonRpcProvider("https://rpc-nebulas-testnet.uniultra.xyz/");
    }

    public void u2uContract()
    {
        RAWS = new SmartContract(RAWSSupplyManager.Address, RAWSSupplyManager.ABI, eWallet.call.u2uwallet, true);
        RAWBank = new SmartContract(RawBankManager.Address, RawBankManager.ABI, eWallet.call.u2uwallet, true);
        RAWChanger = new SmartContract(RawChangerManager.Address, RawChangerManager.ABI, eWallet.call.u2uwallet, true);
        u2uConnect();
    }

    public async void u2uConnect()
    {
        statusText.text = "Connecting to U2U...";

        string accnt = eWallet.call.u2uwallet.GetAddress();

        var gasBalance = await provider.GetBalance(accnt);
        u2uDisplay.text = gasBalance.ToString();

        if (float.Parse(gasBalance.ToString()) > 0.000005f)
        {
            BigInteger rawBalance = await rawsBalance<BigInteger>(accnt);
            rawsDisplay.text = rawBalance.ToString();
            if (rawBalance > 0) rawsDisplay.text = rawsDisplay.text.Substring(0, rawsDisplay.text.Length - 18);

            statusText.text = "U2U Connected Successfully!";
            iSkale.call.sContract();
        }
        else
        {
            statusText.text = "Claim U2U Fauchet!";
            eWallet.call.u2uFObj.SetActive(true);
            eWallet.call.u2uFObj.transform.GetChild(0).GetComponent<TMP_Text>().text = accnt;
            lm.exeLoader.SetActive(false);
        }
    }

    public async Task<BigInteger> rawsBalance<BigInteger>(string owner)
    {
        string methodName = "balanceOf";

        object[] arguments = new object[] {
            owner
        };

        try
        {
            var status = await RAWS.Call<BigInteger>(methodName, arguments);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> rawBankCreate(string uid, BigInteger uintpin, BigInteger uintlock)
    {
        string methodName = "CreateAccount";

        object[] arguments = new object[] {
            "raw" + uid,
            uintpin,
            uintlock
        };

        try
        {
            var status = await RAWBank.SendTransaction(methodName, gas: "100000", parameters: arguments);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<BigInteger> rawBankBalance<BigInteger>(string uid)
    {
        string methodName = "CheckBalance";

        object[] arguments = new object[] {
            "RAWS",
            "raw" + uid
        };

        try
        {
            var status = await RAWBank.Call<BigInteger>(methodName, arguments);
            Debug.Log(status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> rawBankSecure(string uid, BigInteger uintpin, BigInteger uintlock)
    {
        string methodName = "SecureAccount";

        object[] arguments = new object[] {
            "raw" + uid,
            uintpin,
            uintlock
        };

        try
        {
            var status = await RAWBank.SendTransaction(methodName, gas: "100000", parameters: arguments);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> rawChangerExchange(string ticket, string uid, BigInteger uintpin, BigInteger uintlock, BigInteger amount)
    {
        string methodName = "rawExchange";

        object[] arguments = new object[] {
            ticket,
            "raw" + uid,
            uintpin,
            uintlock,
            amount
        };

        try
        {
            var status = await RAWChanger.SendTransaction(methodName, gas: "100000", parameters: arguments);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }
}
