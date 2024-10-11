using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAnim : MonoBehaviour
{
    int repeat;
    public float animSpeed;

    public float maxR;
    public float maxG;
    public float maxB;
    public float maxA;

    public bool isStatic;
    public bool isScale;
    public bool isFade;
    public bool isHorizontal;

    Image bg;

    public bool inMove;

    // Start is called before the first frame update
    void Start()
    {
        maxR = gameObject.GetComponent<Image>().color.r;
        maxG = gameObject.GetComponent<Image>().color.g;
        maxB = gameObject.GetComponent<Image>().color.b;
        maxA = gameObject.GetComponent<Image>().color.a;
    }

    public void Alert()
    {
        repeat = 0;

        SoundManager.sm.alertPlay();
        InvokeRepeating("doAlert", 0, .32f);
    }

    void doAlert()
    {
        Image img = gameObject.GetComponent<Image>();

        if (repeat < 8)
        {
            if (img.color == Color.white)
            {
                img.color = Color.red;
            }
            else
            {
                img.color = Color.white;
            }

            repeat++;
        }
        else
        {
            CancelInvoke("doAlert");
            repeat = 0;
        }
    }

    public void CallScreen()
    {
        if (inMove)
            return;

        inMove = true;
        StartCoroutine(moving());
        if(!isFade) StartCoroutine(bgFade());
    }

    // Update is called once per frame
    IEnumerator moving()
    {
        RectTransform rect = transform.GetChild(0).gameObject.GetComponent<RectTransform>();

        if (isScale)
        {
            rect.localScale = new Vector3(0, 0, 1);
            float speed = rect.localScale.x == 1 ? -animSpeed : animSpeed;
            float x = rect.localScale.x == 1 ? 1 : 0;
            float y = rect.localScale.x == 1 ? 1 : 0;

            while ((speed == 1 && rect.localScale.x < 1) || (speed == -1 && rect.localScale.x > 0))
            {
                x += Time.deltaTime * speed;
                y += Time.deltaTime * speed;
                rect.localScale = new Vector3(x, y, 1);

                yield return null;
            }

            rect.localScale = rect.localScale.x > .5f ? new Vector3(1, 1, 1) : new Vector3(0, 0, 1);
            inMove = false;
        }

        if (isHorizontal)
        {
            float speed = animSpeed;
            Vector2 detination = new Vector2(rect.anchoredPosition.x * -1, rect.anchoredPosition.y);

            while ((rect.anchoredPosition.x < detination.x && detination.normalized.x > 0) || (rect.anchoredPosition.x > detination.x && detination.normalized.x < 0))
            {
                rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, detination + detination.normalized, Time.deltaTime * speed);

                yield return null;
            }

            rect.anchoredPosition = detination;
            inMove = false;
        }

        if (isFade)
        {
            yield return bgFade();

            gameObject.SetActive(false);
            inMove = false;
        }
    }

    IEnumerator bgFade()
    {
        bg = gameObject.GetComponent<Image>();

        float speed = animSpeed;
        float trans = bg.color.a == maxA ? 0.1f : maxA;
        float fade = bg.color.a == maxA ? maxA : 0.1f;

        bg.color = new Color32((byte)(bg.color.r * 255), (byte)(bg.color.g * 255), (byte)(bg.color.b * 255), (byte)(fade * 255));

        while (bg.color.a < trans)
        {
            fade += Time.deltaTime * speed;
            bg.color = new Color32((byte)(bg.color.r * 255), (byte)(bg.color.g * 255), (byte)(bg.color.b * 255), (byte)(fade * 255));

            yield return null;
        }

        bg.color = new Color32((byte)(bg.color.r * 255), (byte)(bg.color.g * 255), (byte)(bg.color.b * 255), (byte)(trans * 255));
    }
}
