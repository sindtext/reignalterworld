using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class rawBankManager : MonoBehaviour
{
    LobbyManager lm;

    public float rawAsset;
    public TMP_Text rawDisplay;

    public TextMeshProUGUI statusText;

    private void Awake()
    {

    }

    private void Start()
    {
        lm = FindObjectOfType<LobbyManager>();
    }

    public void initRawBank()
    {
        DataServer.call.rbm = this;
        u2u.call.rbm = this;
        iSkale.call.rbm = this;
    }

    public void createAccount(TMP_InputField code)
    {
        if (code.text.Length != 6)
        {
            code.transform.GetChild(1).GetComponent<UIAnim>().Alert();
        }
        else
        {
            lm.exeLoader.SetActive(true);
            statusText.text = "Checking Account...";

            //checkAccount(code);
            newAccount(code);
        }
    }

    // Update is called once per frame
    async public void checkAccount(TMP_InputField code)
    {
        try
        {
            BigInteger checkbalance = await u2u.call.rawBankBalance<BigInteger>(DataServer.call.refID);
            Debug.Log(checkbalance);

            int _key = int.Parse(code.text);
            var stringpin = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - _key.ToString("X").Length) + _key.ToString("X");
            BigInteger uintpin = BigInteger.Parse(stringpin, System.Globalization.NumberStyles.HexNumber);

            int _lock = DateTime.Now.Hour % 6;
            string timestamp = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();
            var stringlock = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - _lock.ToString("X").Length) + _lock.ToString("X");
            BigInteger uintlock = BigInteger.Parse(stringlock, System.Globalization.NumberStyles.HexNumber);

            await u2u.call.rawBankSecure(DataServer.call.refID, uintpin, uintlock);

            int pin = int.Parse(code.text);
            DataServer.call.saveRawBank(eWallet.call.account, pin.ToString("X"), timestamp);
            code.transform.parent.gameObject.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            newAccount(code);
        }
    }

    // Update is called once per frame
    async public void newAccount(TMP_InputField code)
    {
        statusText.text = "Creating RawBank Account...";

        int _key = int.Parse(code.text);
        var stringpin = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - _key.ToString("X").Length) + _key.ToString("X");
        BigInteger uintpin = BigInteger.Parse(stringpin, System.Globalization.NumberStyles.HexNumber);

        int _lock = DateTime.Now.Hour % 6;
        string timestamp = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();
        var stringlock = "0000000000000000000000000000000000000000000000000000000000000000".Substring(0, 64 - _lock.ToString("X").Length) + _lock.ToString("X");
        BigInteger uintlock = BigInteger.Parse(stringlock, System.Globalization.NumberStyles.HexNumber);

        await u2u.call.rawBankCreate(DataServer.call.refID, uintpin, uintlock);

        int pin = int.Parse(code.text);
        DataServer.call.saveRawBank(eWallet.call.account, pin.ToString("X"), timestamp);
        code.transform.parent.gameObject.SetActive(false);
    }

    // Update is called once per frame
    async public void checkBalance()
    {
        BigInteger checkbalance = await u2u.call.rawBankBalance<BigInteger>(DataServer.call.refID);

        rawAsset = (float)checkbalance;
        rawDisplay.text = checkbalance.ToString();
    }
}
