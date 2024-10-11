using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ikCtrl : MonoBehaviour
{
    public charController _cc;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if (!_cc.actLock)
            return;

        float angleToPosition = Vector3.Angle(_cc.transform.forward, _cc.aj.targetAtk.transform.position);

        transform.LookAt(_cc.aj.targetAtk.transform);
    }
}
