using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForgeCtrl : MonoBehaviour
{
    public ForgeManager fm;
    public string blprtCat;
    public string blprtName;
    public int blprtIdx;
    public int blprtLvl;
    public int blprtNeed;

    Button myBtn;

    // Start is called before the first frame update
    void Start()
    {
        myBtn = gameObject.GetComponent<Button>();
        myBtn.onClick.AddListener(btnAct);
    }

    // Update is called once per frame
    void btnAct()
    {
        if (blprtName == "")
            return;

        fm.hiLight.GetComponent<RectTransform>().localScale = Vector3.one;
        fm.hiLight.GetComponent<RectTransform>().sizeDelta = gameObject.GetComponent<RectTransform>().sizeDelta;
        fm.hiLight.transform.SetParent(transform);
        fm.hiLight.transform.localPosition = Vector3.zero;
        fm.hiLight.SetActive(true);

        fm.setForge(blprtCat, blprtName, blprtIdx, blprtLvl, blprtNeed);
    }
}
