using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class worldBtn : MonoBehaviour
{
    void Update()
    {
        
    }

    public void techBtn()
    {

    }

    public void utilBtn()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        GameManager.gm.openBuilding();
    }
}
