using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCtrl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void GroundSet(int rare, int rate, bool spawn, int natureOrder)
    {
        transform.GetChild(0).GetComponent<NatureCtrl>().NatureSet(rare, rate, spawn, natureOrder);
    }
}
