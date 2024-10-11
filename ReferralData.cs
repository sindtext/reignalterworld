using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReferralData : MonoBehaviour
{
    TMP_Text RefNam;
    TMP_Text RefID;

    // Update is called once per frame
    public void setup(string nam, string uid)
    {
        RefNam = transform.GetChild(0).GetComponent<TMP_Text>();
        RefID = transform.GetChild(1).GetComponent<TMP_Text>();

        RefNam.text = nam;
        RefID.text = uid;
    }
}
