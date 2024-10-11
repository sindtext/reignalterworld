using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BtnCtrl : MonoBehaviour
{
    public bool isBg;
    public bool isImage;
    public bool isSingle;
    public BtnCtrl[] relatedBtn;

    Button myBtn;

    // Start is called before the first frame update
    void Start()
    {
        myBtn = gameObject.GetComponent<Button>();
        relatedBtn = transform.parent.GetComponentsInChildren<BtnCtrl>(true);

        myBtn.onClick.AddListener(btnAct);
    }

    // Update is called once per frame
    void btnAct()
    {
        if(isSingle)
        {
            TMP_Text myTxt = transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
            myTxt.color = new Color32(255, 255, 255, 240);
        }
        else
        {
            foreach (BtnCtrl btn in relatedBtn)
            {
                if (!btn.isSingle)
                {
                    if (isImage)
                    {
                        Image myImg = btn.transform.GetChild(0).gameObject.GetComponent<Image>();
                        myImg.color = new Color32(255, 255, 255, 160);
                    }
                    else if (isBg)
                    {
                        Image myImg = btn.transform.GetChild(0).gameObject.GetComponent<Image>();
                        myImg.color = new Color32(0, 0, 0, 160);
                    }
                    else
                    {
                        TMP_Text txt = btn.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
                        txt.color = new Color32(255, 255, 255, 240);
                        //txt.fontSize = 32;
                    }
                }
            }

            if (isImage)
            {
                Image myImg = transform.GetChild(0).gameObject.GetComponent<Image>();
                myImg.color = new Color32(0, 160, 160, 160);
            }
            else if (isBg)
            {
                Image myImg = transform.GetChild(0).gameObject.GetComponent<Image>();
                myImg.color = new Color32(0, 160, 160, 160);
            }
            else
            {
                TMP_Text myTxt = transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
                myTxt.color = new Color32(0, 160, 160, 240);
                //myTxt.fontSize = 48;
            }
        }
    }
}
