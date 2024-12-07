using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Collections;
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
    SmartContract u2u0Gas;

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

    public IEnumerator u2uContract()
    {
        if(RAWS == null)
        {
            RAWS = new SmartContract(RAWSSupplyManager.Address, RAWSSupplyManager.ABI, eWallet.call.u2uwallet, "https://rpc-nebulas-testnet.uniultra.xyz/");
            yield return new WaitWhile(() => RAWS == null);
            RAWBank = new SmartContract(RawBankManager.Address, RawBankManager.ABI, eWallet.call.u2uwallet, "https://rpc-nebulas-testnet.uniultra.xyz/");
            yield return new WaitWhile(() => RAWBank == null);
            RAWChanger = new SmartContract(RawChangerManager.Address, RawChangerManager.ABI, eWallet.call.u2uwallet, "https://rpc-nebulas-testnet.uniultra.xyz/");
            yield return new WaitWhile(() => RAWChanger == null);
            EmbeddedWallet faucetwallet = new EmbeddedWallet(eWallet.call.faucetWallet, "2484", provider);
            yield return new WaitWhile(() => faucetwallet == null);
            u2u0Gas = new SmartContract(rawU2UFaucetManager.Address, rawU2UFaucetManager.ABI, faucetwallet);
            yield return new WaitWhile(() => u2u0Gas == null);
        }
        u2uConnect();
    }

    public async void u2uConnect()
    {
        statusText.text = "Connecting to U2U Network...";

        string accnt = eWallet.call.u2uwallet.GetAddress();

        var gasBalance = await provider.GetBalance(accnt);
        u2uDisplay.text = gasBalance.ToString();

        if (float.Parse(gasBalance.ToString()) > 0.000005f)
        {
            BigInteger rawBalance = await rawsBalance<BigInteger>(accnt);
            rawsDisplay.text = rawBalance.ToString();
            if (rawBalance > 0) rawsDisplay.text = rawsDisplay.text.Substring(0, rawsDisplay.text.Length - 18);

            statusText.text = "U2U Network Connected Successfully!";
            StartCoroutine(iSkale.call.sContract());
        }
        else
        {
            statusText.text = "Claim U2U Fauchet!";
            u2uFauchetCall(accnt);
        }
    }

    public async void u2uFauchetCall(string receiver)
    {
        string methodName = "u2uPay";

        object[] arguments = new object[] {
            receiver
        };

        try
        {
            var transactionHash = await u2u0Gas.SendTransaction(methodName, gas: "100000", parameters: arguments);

            statusText.text = "U2U distributed successfully.";

            StartCoroutine(iSkale.call.sContract());
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            statusText.text = "U2U distribution failed.";
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
            lm.stTutor.SetActive(true);
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
            string txStatus = await provider.GetTransactionStatus(status);
            return txStatus;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }
}
