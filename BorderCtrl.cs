using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderCtrl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.GetChild(GameManager.gm.Climate).gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BordMove(Vector3 target, float speed)
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(target.x, 0.008f, target.z), Time.deltaTime * speed);
    }
}
