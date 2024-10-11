using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class regionCtrl : MonoBehaviour
{
    UIManager um;
    earthCtrl ec;

    Vector3 touchFirst;
    Vector3 touchStart;
    Vector3 touchEnd;

    // Start is called before the first frame update
    void Start()
    {
        um = FindObjectOfType<UIManager>();
        ec = FindObjectOfType<earthCtrl>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseDown()
    {
        touchFirst = Input.mousePosition;
    }

    private void OnMouseUp()
    {
        touchEnd = Input.mousePosition;

        if (touchFirst == touchEnd && ec.earthScale <= 2.4f && !um.getMove("RegionRight"))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                Debug.Log(hit.transform.gameObject.name);
                um.callChild("RegionRight");
                ec.earthScale = 3.2f;
                ec.scaleOutEarth = true;
            }
        }
    }
}
