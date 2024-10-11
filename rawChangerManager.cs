using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Eidolon.Provider;
using Eidolon.Wallets;

public class rawChangerManager : MonoBehaviour
{
    public GameObject pinChanger;

    LobbyManager lm;

    public float brzBlnc;
    public float slvBlnc;
    public float gldBlnc;
    public TMP_Text blncDisplay;
    string blncName;
    float maxBalance;

    public float brzAsset;
    public float slvAsset;
    public float gldAsset;
    public TMP_Text brzDisplay;
    public TMP_Text slvDisplay;
    public TMP_Text gldDisplay;
    string exchangeName;
    float maxExchange;

    int gold;
    int raw;

    private UnityWebRequest webRequest;
    private bool isRequestInProgress = false;

    public TextMeshProUGUI statusText;

    private void Awake()
    {

    }

    private void Start()
    {
        lm = FindObjectOfType<LobbyManager>();
    }

    public void initRawChanger()
    {
        DataServer.call.rcm = this;
        u2u.call.rcm = this;
        iSkale.call.rcm = this;
    }

    public void openExhange()
    {
        iSkale.call.brzBalance(eWallet.call.account);
        iSkale.call.slvBalance(eWallet.call.account);
        iSkale.call.gldBalance(eWallet.call.account);

        exchangeType("Bronze");

        StartCoroutine(DataServer.call.readDocummentData("Account/Progress"));
        lm.exeLoader.SetActive(false);
    }

    public void savingType(string crncy)
    {
        blncName = crncy;
        blncDisplay.text = crncy == "Bronze" ? brzBlnc.ToString() : crncy == "Silver" ? slvBlnc.ToString() : gldBlnc.ToString();
        maxBalance = crncy == "Bronze" ? brzBlnc : crncy == "Silver" ? slvBlnc : gldBlnc;
    }

    public void saving(TMP_InputField saveAmnt)
    {
        if (float.Parse(saveAmnt.text) > maxBalance || float.Parse(saveAmnt.text) < 100)
            return;

        lm.exeLoader.SetActive(true);
        float crntBlnc = blncName == "Bronze" ? brzAsset : blncName == "Silver" ? slvAsset : gldAsset;

        DataServer.call.updateShortData("Account/Progress", new string[1] { blncName }, new object[1] { maxBalance - float.Parse(saveAmnt.text) });
        DataServer.call.updateShortData("Account/RawBank", new string[1] { blncName }, new object[1] { crntBlnc + float.Parse(saveAmnt.text) });

        StartCoroutine(savingProcessor(saveAmnt));
    }

    IEnumerator savingProcessor(TMP_InputField saveAmnt)
    {
        statusText.text = "Saving On Process...";

        int lck = 0;
        string crncy = blncName == "Bronze" ? "rawbrz" : blncName == "Silver" ? "rawslv" : "rawgld";

        yield return DataServer.call.eLockCall((result) => {
            lck = result;

            int _key = (123456 + lck) % 999999;
            var stringpin = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - _key.ToString("X").Length) + _key.ToString("X");
            BigInteger uintpin = BigInteger.Parse(stringpin, System.Globalization.NumberStyles.HexNumber);

            int _lock = DateTime.Now.Hour % 6;
            string timestamp = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();
            var stringlock = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - _lock.ToString("X").Length) + _lock.ToString("X");
            BigInteger uintlock = BigInteger.Parse(stringlock, System.Globalization.NumberStyles.HexNumber);

            int _amnt = int.Parse(saveAmnt.text);
            var stringamnt = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - _amnt.ToString("X").Length) + _amnt.ToString("X");
            BigInteger uintamnt = BigInteger.Parse(stringamnt, System.Globalization.NumberStyles.HexNumber);

            iSkale.call.currencySave(crncy, uintpin, uintlock, uintamnt, timestamp);
        });
    }

    public void exchangeType(string crncy)
    {
        exchangeName = crncy;
        maxExchange = crncy == "Bronze" ? brzAsset : crncy == "Silver" ? slvAsset : gldAsset;
    }

    public void exchange(TMP_InputField excAmnt)
    {
        if (float.Parse(excAmnt.text) > maxExchange || float.Parse(excAmnt.text) < 100)
            return;

        string CurencyName = exchangeName == "Bronze" ? "Silver" : "Gold";
        float exchangeAmount = float.Parse(excAmnt.text) / 100;
        float crntBlnc = exchangeName == "Bronze" ? slvAsset : gldAsset;

        if (exchangeName == "Gold")
        {
            gold = int.Parse(excAmnt.text);
            raw = (int)exchangeAmount;
            pinChanger.SetActive(true);
        }
        else
        {
            lm.exeLoader.SetActive(true);
            DataServer.call.updateShortData("Account/RawBank", new string[1] { exchangeName }, new object[1] { maxExchange - float.Parse(excAmnt.text) });
            DataServer.call.updateShortData("Account/RawBank", new string[1] { CurencyName }, new object[1] { crntBlnc + exchangeAmount });

            exchangeProcessor(excAmnt);
        }
    }
    async void exchangeProcessor(TMP_InputField excAmnt)
    {
        statusText.text = "Exchange On Process...";

        string crna = exchangeName == "Bronze" ? "rawbrz" : exchangeName == "Silver" ? "rawslv" : "rawgld";
        string crnb = exchangeName == "Bronze" ? "rawslv" : "rawgld";

        int _amnt = int.Parse(excAmnt.text);
        var stringamnt = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - _amnt.ToString("X").Length) + _amnt.ToString("X");
        BigInteger uintamnt = BigInteger.Parse(stringamnt, System.Globalization.NumberStyles.HexNumber);

        BigInteger allowAmnt = crna == "rawbrz" ? await iSkale.call.brzAllowance<BigInteger>(eWallet.call.account, eRAWChangerManager.Address) : await iSkale.call.slvAllowance<BigInteger>(eWallet.call.account, eRAWChangerManager.Address);

        if (allowAmnt < uintamnt * 1000000000000000000)
        {
            statusText.text = "Approving Transaction...";
            BigInteger _value = (BigInteger)maxExchange;
            BigInteger _multi = 1000000000000000000;
            var stringvalue = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - (_value * _multi).ToString("X").Length) + (_value * _multi).ToString("X");
            BigInteger uintvalue = BigInteger.Parse(stringvalue, System.Globalization.NumberStyles.HexNumber);

            if (crna == "rawbrz") await iSkale.call.brzApprove(eRAWChangerManager.Address, uintvalue);
            if (crna == "rawslv") await iSkale.call.slvApprove(eRAWChangerManager.Address, uintvalue);
        }

        statusText.text = "Exchange On Process...";
        iSkale.call.currencyExchange(crna, crnb, uintamnt);
    }

    public void exchangeToRaw(TMP_InputField code)
    {
        lm.exeLoader.SetActive(true);
        StartCoroutine(rawProcessor(code));
    }

    IEnumerator rawProcessor(TMP_InputField code)
    {
        statusText.text = "Exchange On Process...";
        string ticket = "";
        int locker = 0;

        yield return DataServer.call.ticketCall("alpha", (result) => {
            ticket = result;
        });

        yield return DataServer.call.lockCall((result) => {
            locker = result;

            int _key = (int.Parse(code.text) + locker) % 999999;
            var stringpin = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - _key.ToString("X").Length) + _key.ToString("X");
            BigInteger uintpin = BigInteger.Parse(stringpin, System.Globalization.NumberStyles.HexNumber);

            int _lock = DateTime.Now.Hour % 6;
            string timestamp = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();
            var stringlock = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - _lock.ToString("X").Length) + _lock.ToString("X");
            BigInteger uintlock = BigInteger.Parse(stringlock, System.Globalization.NumberStyles.HexNumber);

            var stringraw = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - raw.ToString("X").Length) + raw.ToString("X");
            BigInteger uintraw = BigInteger.Parse(stringraw, System.Globalization.NumberStyles.HexNumber);

            exchangeExecute(ticket, uintpin, uintlock, uintraw, timestamp);
        });
    }

    async public void exchangeExecute(string ticket, BigInteger uintpin, BigInteger uintlock, BigInteger uintraw, string timestamp)
    {
        var stringgold = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - gold.ToString("X").Length) + gold.ToString("X");
        BigInteger uintgold = BigInteger.Parse(stringgold, System.Globalization.NumberStyles.HexNumber);
        BigInteger allowAmnt = await iSkale.call.gldAllowance<BigInteger>(eWallet.call.account, eRAWChangerManager.Address);
        if (allowAmnt < uintgold * 1000000000000000000)
        {
            statusText.text = "Approving Transaction...";
            BigInteger _value = (BigInteger)maxExchange;
            BigInteger _multi = 1000000000000000000;
            var stringvalue = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - (_value * _multi).ToString("X").Length) + (_value * _multi).ToString("X");
            BigInteger uintvalue = BigInteger.Parse(stringvalue, System.Globalization.NumberStyles.HexNumber);

            await iSkale.call.gldApprove(eRAWChangerManager.Address, uintvalue);
        }

        statusText.text = "Exchange On Process...";
        iSkale.call.crossExchange("rawgld", uintgold);

        PlayerPrefs.SetInt("tempGold", gold);
        PlayerPrefs.SetFloat("tempExc", maxExchange - gold);
        PlayerPrefs.SetString("timeStamp", timestamp);

        await u2u.call.rawChangerExchange(ticket, DataServer.call.refID, uintpin, uintlock, uintraw);

        iSkale.call.useRaws(uintgold);
        DataServer.call.updateShortData("Account/RawBank", new string[2] { exchangeName, "timeStamp" }, new object[2] { maxExchange - gold, timestamp });
        DataServer.call.saveWallet("");
        pinChanger.SetActive(false);
        openExhange();
    }
}
