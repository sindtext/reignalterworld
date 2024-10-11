using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Swiper : MonoBehaviour
{
    public GameObject scrollbar;
    float scroll_pos = 0;
    float[] pos;
    int posisi = 0;

    bool isMove;
    // Start is called before the first frame update
    void Start()
    {
        pos = new float[transform.childCount];
    }

    public void next()
    {
        if (posisi < pos.Length - 1)
        {
            posisi += 1;
            scroll_pos = pos[posisi];
            isMove = true;
        }
    }
    public void prev()
    {
        if (posisi > 0)
        {
            posisi -= 1;
            scroll_pos = pos[posisi];
            isMove = true;
        }
    }
    // Update is called once per frame
    private void Update()
    {
        if (!isMove)
            return;

        float distance = 1f / (pos.Length - 1f);
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }

        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
            {
                scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.15f);
                posisi = i;
                DataManager.dm.charIdx = posisi;
            }
            
            if(i == pos.Length - 1 && scrollbar.GetComponent<Scrollbar>().value == pos[i])
            {
                isMove = false;
            }
        }
    }
}
