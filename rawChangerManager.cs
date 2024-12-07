using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
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

    public TMP_Text txtMain;
    public TMP_Dropdown ddMain;
    public TMP_Dropdown ddPair;
    public Slider sdPair;
    public TMP_InputField dpPair;
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

    public IEnumerator openExhange()
    {
        brzDisplay.text = "-";
        slvDisplay.text = "-";
        gldDisplay.text = "-";

        iSkale.call.brzBalance(eWallet.call.account);
        yield return new WaitWhile(() => brzDisplay.text == "-");
        iSkale.call.slvBalance(eWallet.call.account);
        yield return new WaitWhile(() => slvDisplay.text == "-");
        iSkale.call.gldBalance(eWallet.call.account);
        yield return new WaitWhile(() => gldDisplay.text == "-");

        exchangeType();

        StartCoroutine(DataServer.call.readDocummentData("Account/Progress"));
        lm.exeLoader.SetActive(false);
    }

    public void savingType(string crncy = null)
    {
        blncName = string.IsNullOrEmpty(blncName) ? "Bronze" : string.IsNullOrEmpty(crncy) ? blncName : crncy;
        blncDisplay.text = blncName == "Bronze" ? brzBlnc.ToString() : blncName == "Silver" ? slvBlnc.ToString() : gldBlnc.ToString();
        maxBalance = blncName == "Bronze" ? brzBlnc : blncName == "Silver" ? slvBlnc : gldBlnc;
    }

    public void saving(TMP_InputField saveAmnt)
    {
        if (float.Parse(saveAmnt.text) > maxBalance)
            return;

        lm.exeLoader.SetActive(true);

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

            iSkale.call.currencySave(crncy, uintpin, uintlock, uintamnt, timestamp, saveAmnt.text);
            saveAmnt.text = "0";
        });
    }

    public void savingSuccess(string saveAmnt)
    {
        float crntBlnc = blncName == "Bronze" ? brzAsset : blncName == "Silver" ? slvAsset : gldAsset;
        DateTime utcDate = DateTime.UtcNow;
        long stamp = (long)utcDate.Year * 12 * 30 * 24 * 60 * 60 + (long)utcDate.Month * 30 * 24 * 60 * 60 + (long)utcDate.Day * 24 * 60 * 60 + (long)utcDate.Hour * 60 * 60 + (long)utcDate.Minute * 60 + (long)utcDate.Second;

        DataServer.call.updateLeaderboard("Currency/inGame", new string[2] { "score", "timestamp" }, new object[2] { brzAsset + slvAsset * 100 + gldAsset * 10000, stamp });
        DataServer.call.updateShortData("Account/Progress", new string[1] { blncName }, new object[1] { maxBalance - float.Parse(saveAmnt) });
        DataServer.call.updateShortData("Account/RawBank", new string[1] { blncName }, new object[1] { crntBlnc + float.Parse(saveAmnt) });

        StartCoroutine(openExhange());
    }

    public void exchangeType(TMP_Dropdown crncy = null)
    {
        if(crncy == null)
        {
            exchangeName = "Bronze";
            maxExchange = brzAsset;
            ddMain.SetValueWithoutNotify(0);
            ddPair.SetValueWithoutNotify(0);
        }
        else
        {
            exchangeName = crncy.options[crncy.value].text;
            maxExchange = exchangeName == "Bronze" ? brzAsset : exchangeName == "Silver" ? slvAsset : gldAsset;
            ddPair.SetValueWithoutNotify(crncy.value);
        }

        sdPair.maxValue = maxExchange;
    }

    public void valueUpdate(Slider excAmnt)
    {
        dpPair.text = (Mathf.Ceil(excAmnt.value / 100)).ToString();
        sdPair.value = Mathf.Ceil(excAmnt.value / 100) * 100;
        if(Mathf.Ceil(excAmnt.value / 100) > maxExchange / 100)
        {
            dpPair.text = (Mathf.Floor(excAmnt.value / 100)).ToString();
            sdPair.value = Mathf.Floor(excAmnt.value / 100) * 100;
        }
        txtMain.text = sdPair.value.ToString();
    }

    public void exchange(Slider excAmnt)
    {
        if (excAmnt.value > maxExchange)
            return;

        string CurencyName = exchangeName == "Bronze" ? "Silver" : "Gold";
        float exchangeAmount = excAmnt.value / 100;
        float crntBlnc = exchangeName == "Bronze" ? slvAsset : gldAsset;

        if (exchangeName == "Gold")
        {
            gold = (int)excAmnt.value;
            raw = (int)exchangeAmount;
            pinChanger.SetActive(true);
        }
        else
        {
            lm.exeLoader.SetActive(true);
            DataServer.call.updateShortData("Account/RawBank", new string[1] { exchangeName }, new object[1] { maxExchange - excAmnt.value });
            DataServer.call.updateShortData("Account/RawBank", new string[1] { CurencyName }, new object[1] { crntBlnc + exchangeAmount });

            exchangeProcessor(excAmnt);
        }
    }
    async void exchangeProcessor(Slider excAmnt)
    {
        statusText.text = "Exchange On Process...";

        string crna = exchangeName == "Bronze" ? "rawbrz" : exchangeName == "Silver" ? "rawslv" : "rawgld";
        string crnb = exchangeName == "Bronze" ? "rawslv" : "rawgld";

        int _amnt = (int)excAmnt.value;
        var stringamnt = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - _amnt.ToString("X").Length) + _amnt.ToString("X");
        BigInteger uintamnt = BigInteger.Parse(stringamnt, System.Globalization.NumberStyles.HexNumber);

        BigInteger allowAmnt = crna == "rawbrz" ? await iSkale.call.brzAllowance<BigInteger>(eWallet.call.account, eRawChangerManager.Address) : await iSkale.call.slvAllowance<BigInteger>(eWallet.call.account, eRawChangerManager.Address);

        if (allowAmnt < uintamnt * 1000000000000000000)
        {
            statusText.text = "Approving Transaction...";
            BigInteger _value = (BigInteger)maxExchange;
            BigInteger _multi = 1000000000000000000;
            var stringvalue = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - (_value * _multi).ToString("X").Length) + (_value * _multi).ToString("X");
            BigInteger uintvalue = BigInteger.Parse(stringvalue, System.Globalization.NumberStyles.HexNumber);

            if (crna == "rawbrz") await iSkale.call.brzApprove(eRawChangerManager.Address, uintvalue);
            if (crna == "rawslv") await iSkale.call.slvApprove(eRawChangerManager.Address, uintvalue);
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
        BigInteger allowAmnt = await iSkale.call.gldAllowance<BigInteger>(eWallet.call.account, eRawChangerManager.Address);
        if (allowAmnt < uintgold * 1000000000000000000)
        {
            statusText.text = "Approving Transaction...";
            BigInteger _value = (BigInteger)maxExchange;
            BigInteger _multi = 1000000000000000000;
            var stringvalue = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - (_value * _multi).ToString("X").Length) + (_value * _multi).ToString("X");
            BigInteger uintvalue = BigInteger.Parse(stringvalue, System.Globalization.NumberStyles.HexNumber);

            await iSkale.call.gldApprove(eRawChangerManager.Address, uintvalue);
        }

        pinChanger.SetActive(false);
        statusText.text = "Exchange On Process...";
        iSkale.call.crossExchange("rawgld", uintgold);

        PlayerPrefs.SetInt("tempGold", gold);
        PlayerPrefs.SetFloat("tempExc", maxExchange - gold);
        PlayerPrefs.SetString("timeStamp", timestamp);

        var txStatus = await u2u.call.rawChangerExchange(ticket, DataServer.call.refID, uintpin, uintlock, uintraw);

        if (txStatus == "Success")
        {
            statusText.text = "Exchange successfully!";
            iSkale.call.useRaws(uintgold);
            DataServer.call.updateShortData("Account/RawBank", new string[2] { exchangeName, "timeStamp" }, new object[2] { maxExchange - gold, timestamp });
            DataServer.call.saveWallet("");
            StartCoroutine(openExhange());
        }
        else
        {
            statusText.text = "Exchange failed, Try Again!";
            statusText.transform.parent.GetChild(2).gameObject.SetActive(true);
        }
    }
}
