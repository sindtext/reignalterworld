using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public class DeviceCheck : MonoBehaviour
{
    InputDevice xr;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public string getsn()
    {
        string sn = xr.serialNumber;
        if (string.IsNullOrEmpty(sn)) sn = SystemInfo.deviceUniqueIdentifier;

        return sn;
    }

    // Update is called once per frame
    public string getmac()
    {
        string macAdress = "";
        NetworkInterface[] nics  = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface adapter in nics)
        {
            PhysicalAddress address = adapter.GetPhysicalAddress();
            if (address.ToString() != "")
            {
                macAdress = address.ToString();
                return macAdress;
            }
        }

        return "";
    }
}
