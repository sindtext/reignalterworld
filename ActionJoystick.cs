using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionJoystick : MonoBehaviour
{
    GameObject targetCtrl;
    GameObject targetAtk;
    public int actindex;

    public charController _cc;

    void Update()
    {

    }

    public void regAtk(GameObject obj)
    {
        targetCtrl = obj;
        Debug.Log(targetCtrl.transform.parent.name);
    }

    public void atkPush()
    {
        targetAtk = targetCtrl;
        if (targetAtk.CompareTag("Nature"))
        {
            actindex = int.Parse(targetCtrl.transform.parent.name);
        }
        else
        {
            actindex = 1;
        }
    }

    public void atkUp()
    {
        actindex = 0;
    }

    public void actAtk(int index)
    {
        if(index == 7)
        {
            targetAtk.GetComponent<RSSControl>()._cc = _cc;
            targetAtk.GetComponent<RSSControl>().gather(5);
        }
        _cc.transform.parent.LookAt(targetAtk.transform.parent);
        _cc.transform.parent.eulerAngles = new Vector3(0, _cc.transform.parent.eulerAngles.y, _cc.transform.parent.eulerAngles.z);

        _cc.actionAnim(index);

        _cc.inAct = index == 1;
    }
}