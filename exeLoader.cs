using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class exeLoader : MonoBehaviour
{
    Image loaderObj;
    float moveDirection;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        loaderObj = gameObject.GetComponent<Image>();
        moveDirection = .16f;
        loaderObj.fillClockwise = true;
        loaderObj.fillAmount = 0;
        InvokeRepeating("load", 0, .16f);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    // Update is called once per frame
    void load()
    {
        loaderObj.fillAmount += moveDirection;
        if(loaderObj.fillAmount >= 1)
        {
            moveDirection *= -1;
            loaderObj.fillAmount += moveDirection;
            loaderObj.fillClockwise = !loaderObj.fillClockwise;
        }

        if (loaderObj.fillAmount <= 0)
        {
            moveDirection *= -1;
            loaderObj.fillAmount += moveDirection;
            loaderObj.fillClockwise = !loaderObj.fillClockwise;
        }
    }
}
