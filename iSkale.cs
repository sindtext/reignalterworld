using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Eidolon.Wallets;
using Eidolon.Provider;
using Eidolon.SmartContracts;
using Eidolon.Util;

public class iSkale : MonoBehaviour
{
    public static iSkale call;

    LobbyManager lm;

    SmartContract RAWBrz;
    SmartContract RAWSlv;
    SmartContract RAWGld;
    SmartContract eRAWChanger;
    SmartContract sFuelFaucet;
    SmartContract skaleGacha;

    JsonRpcProvider provider;

    public rawChangerManager rcm;
    public rawBankManager rbm;

    public TextMeshProUGUI statusText;
    public TMP_Text sFuelDisplay;
    int gachaValue;
    public TMP_InputField gachaText;

    private void Awake()
    {
        if (call)
            return;

        call = this;
    }

    private void Start()
    {
        lm = FindObjectOfType<LobbyManager>();
        provider = new JsonRpcProvider();
    }

    public IEnumerator sContract()
    {
        RAWBrz = new SmartContract(RAWBrzManager.Address, RAWBrzManager.ABI, eWallet.call.wallet);
        yield return new WaitWhile(() => RAWBrz == null);
        RAWSlv = new SmartContract(RAWSlvManager.Address, RAWSlvManager.ABI, eWallet.call.wallet);
        yield return new WaitWhile(() => RAWSlv == null);
        RAWGld = new SmartContract(RAWGldManager.Address, RAWGldManager.ABI, eWallet.call.wallet);
        yield return new WaitWhile(() => RAWGld == null);
        eRAWChanger = new SmartContract(eRAWChangerManager.Address, eRAWChangerManager.ABI, eWallet.call.wallet);
        yield return new WaitWhile(() => eRAWChanger == null);
        EmbeddedWallet faucetwallet = new EmbeddedWallet(eWallet.call.faucetWallet, "37084624", new JsonRpcProvider("https://testnet.skalenodes.com/v1/lanky-ill-funny-testnet"));
        yield return new WaitWhile(() => faucetwallet == null);
        sFuelFaucet = new SmartContract(sRAWFaucetManager.Address, sRAWFaucetManager.ABI, faucetwallet);
        yield return new WaitWhile(() => sFuelFaucet == null);


        eWallet.call.createBtn.SetActive(false);
        eWallet.call.connectBtn.SetActive(false);
        eWallet.call.disconnectBtn.SetActive(true);

        if (DataServer.call.newWallet)
        {
            eWallet.call.walletDisplay.gameObject.SetActive(false);
            eWallet.call.rawbankBtn.gameObject.SetActive(true);
            eWallet.call.walletLoad.SetActive(true);
            eWallet.call.walletLoad.GetComponent<TMP_Text>().text = "Please Sign Rawbank...";

            lm.exeLoader.SetActive(false);
        }
        else
        {
            rbm.checkBalance();

            eWallet.call.walletDisplay.gameObject.SetActive(true);
            eWallet.call.rawbankBtn.gameObject.SetActive(false);
            eWallet.call.walletLoad.SetActive(false);

            sConnect();
        }
    }

    public async void sConnect()
    {
        statusText.text = "Connecting to SKALE Nebula Gaming Hub...";
        var currentGasBalance = await provider.GetBalance(eWallet.call.account);
        sFuelDisplay.text = ": " + currentGasBalance.ToString();

        if (float.Parse(currentGasBalance.ToString()) > 0.000005f)
        {
            BigInteger tempRaws = await readRaws<BigInteger>();
            if (tempRaws != 0)
            {
                var stringgold = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - PlayerPrefs.GetInt("tempGold").ToString("X").Length) + PlayerPrefs.GetInt("tempGold").ToString("X");
                BigInteger uintgold = BigInteger.Parse(stringgold, System.Globalization.NumberStyles.HexNumber);
                useRaws(uintgold);
                if (PlayerPrefs.HasKey("timeStamp")) DataServer.call.updateShortData("Account/RawBank", new string[2] { "Gold", "timeStamp" }, new object[2] { PlayerPrefs.GetFloat("tempExc"), PlayerPrefs.GetString("timeStamp") });
            }

            statusText.text = "SKALE Nebula Gaming Hub Connected Successfully!";
            DataServer.call.saveWallet(eWallet.call.account);
            StartCoroutine(rcm.openExhange());

            eWallet.call.walletLoad.SetActive(false);
            eWallet.call.tokenContainer.GetChild(0).gameObject.SetActive(true);
            eWallet.call.tokenContainer.parent.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().color = new Color(0, 0.625f, 0.625f, 0.625f);
            eWallet.call.tokenContainer.parent.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>().color = new Color(0, 0, 0, 0.625f);
            eWallet.call.tokenContainer.parent.GetChild(0).GetChild(2).GetChild(0).GetComponent<Image>().color = new Color(0, 0, 0, 0.625f);
        }
        else
        {
            statusText.text = "add sFuel to this wallet : " + eWallet.call.account;
            sFuelFauchetCall(eWallet.call.account);
        }
    }

    public async void sFuelFauchetCall(string receiver)
    {
        string methodName = "sRawFaucet";

        object[] arguments = new object[] {
            receiver
        };

        try
        {
            var transactionHash = await sFuelFaucet.SendTransaction(methodName, gas: "100000", parameters: arguments);

            statusText.text = "sFuel distributed successfully.";
            sConnect();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            statusText.text = "sFuel distribution failed.";
        }
    }

    public async void currencySave(string crncy, BigInteger key, BigInteger lck, BigInteger amnt, string timeStamp, string saveAmount)
    {
        string methodName = "Save";

        object[] arguments = new object[] {
            crncy,
            key,
            lck,
            amnt
        };

        try
        {
            var transactionHash = await eRAWChanger.SendTransaction(methodName, gas: "100000", parameters: arguments);

            string txStatus = await provider.GetTransactionStatus(transactionHash);
            if(txStatus == "Success")
            {
                DataServer.call.updateShortData("Account/RawChanger", new string[1] { "timeStamp" }, new object[1] { timeStamp });
                rcm.savingSuccess(saveAmount);
                statusText.text = "Saved successfully!";
            }
            else
            {
                statusText.text = "Saved failed, Try Again!";
                statusText.transform.parent.GetChild(2).gameObject.SetActive(true);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            statusText.text = "Saving failed...";
        }
    }

    public async void currencyExchange(string crna, string crnb, BigInteger amnt)
    {
        string methodName = "exchange";

        object[] arguments = new object[] {
            crna,
            crnb,
            amnt
        };

        try
        {
            var transactionHash = await eRAWChanger.SendTransaction(methodName, gas: "100000", parameters: arguments);
            string txStatus = await provider.GetTransactionStatus(transactionHash);
            if (txStatus == "Success")
            {
                statusText.text = "Exchanged successfully.";
                StartCoroutine(rcm.openExhange());
            }
            else
            {
                statusText.text = "Exchanged failed, Try Again!";
                statusText.transform.parent.GetChild(2).gameObject.SetActive(true);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            statusText.text = "Exchange failed.";
        }
    }

    public async void crossExchange(string crncy, BigInteger amnt)
    {
        string methodName = "crossChange";

        object[] arguments = new object[] {
            crncy,
            amnt
        };

        try
        {
            var transactionHash = await eRAWChanger.SendTransaction(methodName, gas: "100000", parameters: arguments);
            statusText.text = "Exchanged successfully.";
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            statusText.text = "Exchange failed.";
        }
    }

    public async Task<BigInteger> readRaws<BigInteger>()
    {
        string methodName = "getRAWS";

        object[] arguments = new object[] {
            
        };

        try
        {
            var status = await eRAWChanger.Call<BigInteger>(methodName, arguments);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async void useRaws(BigInteger amnt)
    {
        string methodName = "useRAWS";

        object[] arguments = new object[] {
            amnt
        };

        try
        {
            var transactionHash = await eRAWChanger.SendTransaction(methodName, gas: "100000", parameters: arguments);
            statusText.text = "Exchanged successfully.";
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            statusText.text = "Exchange failed.";
        }
    }

    public async void brzBalance(string owner)
    {
        string methodName = "balanceOf";

        object[] arguments = new object[] {
            owner
        };

        try
        {
            var status = await RAWBrz.Call<BigInteger>(methodName, arguments);
            if (status.ToString() == "0" || status.ToString().Length <= 18 || string.IsNullOrEmpty(status.ToString()))
            {
                rcm.brzAsset = 0;
                rcm.brzDisplay.text = rcm.gldAsset.ToString();
            }
            else
            {
                string stirngbalance = status.ToString().Substring(0, status.ToString().Length - 18);
                rcm.brzAsset = int.Parse(stirngbalance);
                rcm.brzDisplay.text = rcm.brzAsset.ToString();
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            rcm.brzAsset = 0;
            rcm.brzDisplay.text = rcm.brzAsset.ToString();
        }
    }

    public async Task<BigInteger> brzAllowance<BigInteger>(string owner, string spender)
    {
        string methodName = "allowance";

        object[] arguments = new object[] {
            owner,
            spender
        };

        try
        {
            var status = await RAWBrz.Call<BigInteger>(methodName, arguments);
            Debug.Log("Allowance Call Result: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> brzApprove(string spender, BigInteger value)
    {
        string methodName = "approve";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await RAWBrz.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
            return transactionHash;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async void brzTransfer(string spender, BigInteger value)
    {
        string methodName = "transfer";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await RAWBrz.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public async void slvBalance(string owner)
    {
        string methodName = "balanceOf";

        object[] arguments = new object[] {
            owner
        };

        try
        {
            var status = await RAWSlv.Call<BigInteger>(methodName, arguments);
            if (status.ToString() == "0" || status.ToString().Length <= 18 || string.IsNullOrEmpty(status.ToString()))
            {
                rcm.slvAsset = 0;
                rcm.slvDisplay.text = rcm.gldAsset.ToString();
            }
            else
            {
                string stirngbalance = status.ToString().Substring(0, status.ToString().Length - 18);
                rcm.slvAsset = int.Parse(stirngbalance);
                rcm.slvDisplay.text = rcm.slvAsset.ToString();
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            rcm.slvAsset = 0;
            rcm.slvDisplay.text = rcm.slvAsset.ToString();
        }
    }

    public async Task<BigInteger> slvAllowance<BigInteger>(string owner, string spender)
    {
        string methodName = "allowance";

        object[] arguments = new object[] {
            owner,
            spender
        };

        try
        {
            var status = await RAWSlv.Call<BigInteger>(methodName, arguments);
            Debug.Log("Allowance Call Result: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> slvApprove(string spender, BigInteger value)
    {
        string methodName = "approve";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await RAWSlv.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
            return transactionHash;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async void slvTransfer(string spender, BigInteger value)
    {
        string methodName = "transfer";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await RAWSlv.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public async void gldBalance(string owner)
    {
        string methodName = "balanceOf";

        object[] arguments = new object[] {
            owner
        };

        try
        {
            var status = await RAWGld.Call<BigInteger>(methodName, arguments);
            if (status.ToString() == "0" || status.ToString().Length <= 18 || string.IsNullOrEmpty(status.ToString()))
            {
                rcm.gldAsset = 0;
                rcm.gldDisplay.text = rcm.gldAsset.ToString();
            }
            else
            {
                string stirngbalance = status.ToString().Substring(0, status.ToString().Length - 18);
                rcm.gldAsset = int.Parse(stirngbalance);
                rcm.gldDisplay.text = rcm.gldAsset.ToString();
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            rcm.gldAsset = 0;
            rcm.gldDisplay.text = rcm.gldAsset.ToString();
        }
    }

    public async Task<BigInteger> gldAllowance<BigInteger>(string owner, string spender)
    {
        string methodName = "allowance";

        object[] arguments = new object[] {
            owner,
            spender
        };

        try
        {
            var status = await RAWGld.Call<BigInteger>(methodName, arguments);
            Debug.Log("Allowance Call Result: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> gldApprove(string spender, BigInteger value)
    {
        string methodName = "approve";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await RAWGld.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
            return transactionHash;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async void gldTransfer(string spender, BigInteger value)
    {
        string methodName = "transfer";

        object[] arguments = new object[] {
            spender,
            value
        };

        try
        {
            var transactionHash = await RAWGld.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    // You can also have users sign messages using Embedded wallets:
    public async void Sign(string message)
    {
        // This will return a signature
        string signature = await eWallet.call.wallet.SignMessage(message);

        Debug.Log("Signature: " + signature);
    }

    public async void runGacha()
    {
        var currentGasBalance = await provider.GetBalance(eWallet.call.account);
        Debug.Log(currentGasBalance.ToString());

        skaleGacha = new SmartContract(SkaleGachaManager.Address, SkaleGachaManager.ABI, eWallet.call.wallet);

        if (float.Parse(currentGasBalance.ToString()) > 0.000005f)
        {
            bool gachaReady = await exeCheckGacha();
            if(gachaReady)
            {
                await exeGacha();
                BigInteger tempValue = await exeLastGacha<BigInteger>();
                gachaValue = (int)tempValue;
                gachaText.text = gachaValue.ToString();
            }
            else
            {
                Debug.Log("GACHA HABIS");
            }
        }
    }

    public async Task<BigInteger> exeLastSign<BigInteger>()
    {
        string methodName = "lastSign";

        object[] arguments = new object[] {

        };

        try
        {
            var status = await skaleGacha.Call<BigInteger>(methodName, arguments);
            Debug.Log("Transaction Hash: " + status);
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
        string methodName = "checkGacha";

        object[] arguments = new object[] {

        };

        try
        {
            var status = await skaleGacha.Call<bool>(methodName, arguments);
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
        string methodName = "signGacha";

        object[] arguments = new object[] {

        };

        try
        {
            var transactionHash = await skaleGacha.SendTransaction(methodName, gas: "100000", parameters: arguments);
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
        string methodName = "gacha";

        object[] arguments = new object[] {

        };

        try
        {
            var transactionHash = await skaleGacha.SendTransaction(methodName, gas: "100000", parameters: arguments);
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
        string methodName = "lastGacha";

        object[] arguments = new object[] {

        };

        try
        {
            var status = await skaleGacha.Call<BigInteger>(methodName, arguments);
            Debug.Log("Transaction Hash: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            await exeSignGacha();
            runGacha();
            throw;
        }
    }
}
