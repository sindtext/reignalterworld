using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Numerics;
using Eidolon.Wallets;
using Eidolon.Provider;
using Eidolon.SmartContracts;
using Eidolon.Util;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class iZytron : MonoBehaviour
{
    string signature;
    string endPoint = "https://rpc-zytron-testnet-linea.zypher.game/";
    SmartContract zytronGacha;
    JsonRpcProvider provider;
    int gachaValue;

    private void Start()
    {
        provider = new JsonRpcProvider("https://rpc-testnet.zypher.network/");
    }

    public async void runZytronGacha()
    {
        var currentGasBalance = await provider.GetBalance(eWallet.call.account);
        Debug.Log(currentGasBalance.ToString());

        zytronGacha = new SmartContract(ZytronGachaManager.Address, ZytronGachaManager.ABI, eWallet.call.zytronwallet, "https://rpc-testnet.zypher.network/");

        BigInteger tempSign = await exeZytronLastSign<BigInteger>();
        int gachaReady = (int)tempSign;

        if (float.Parse(currentGasBalance.ToString()) > 0.000005f)
        {
            if(gachaReady == 0)
            {
                await exeZytronSignGacha();
                runZytronGacha();
            }
            else
            {
                await exeZytronGacha();
                BigInteger tempValue = await exeZytronLastGacha<BigInteger>();
                gachaValue = (int)tempValue;
                Debug.Log(gachaValue);
            }
        }
    }

    public async Task<BigInteger> exeZytronLastSign<BigInteger>()
    {
        string methodName = "lastSign";

        object[] arguments = new object[] {

        };

        try
        {
            var status = await zytronGacha.Call<BigInteger>(methodName, arguments);
            Debug.Log("Transaction Hash: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<bool> exeZytronCheckGacha()
    {
        string methodName = "checkGacha";

        object[] arguments = new object[] {

        };

        try
        {
            var status = await zytronGacha.Call<bool>(methodName, arguments);
            Debug.Log("Transaction Hash: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> exeZytronSignGacha()
    {
        string methodName = "signGacha";

        object[] arguments = new object[] {

        };

        try
        {
            var transactionHash = await zytronGacha.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
            return transactionHash;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<string> exeZytronGacha()
    {
        string methodName = "gacha";

        object[] arguments = new object[] {
            
        };

        try
        {
            var transactionHash = await zytronGacha.SendTransaction(methodName, gas: "100000", parameters: arguments);
            Debug.Log("Transaction Hash: " + transactionHash);
            return transactionHash;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<BigInteger> exeZytronLastGacha<BigInteger>()
    {
        string methodName = "lastGacha";

        object[] arguments = new object[] {

        };

        try
        {
            var status = await zytronGacha.Call<BigInteger>(methodName, arguments);
            Debug.Log("Transaction Hash: " + status);
            return status;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }
}
