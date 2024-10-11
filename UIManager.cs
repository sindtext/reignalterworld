using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public UIAnim[] uiList;
    public float animSpeed;

    // Start is called before the first frame update
    void Start()
    {
        uiList = transform.GetComponentsInChildren<UIAnim>(true);
        Invoke("startingUI", .32f);
    }

    void startingUI()
    {
        foreach (UIAnim uia in uiList)
        {
            if (!uia.isStatic)
            {
                uia.gameObject.SetActive(false);
            }
        }

        setDynamic("LoaderUI");
    }

    public void openUI(string name)
    {
        int index = 0;
        for (int i = 0; i < uiList.Length; i++)
        {
            if (uiList[i].name == name)
            {
                index = i;
            }
        }

        StartCoroutine(opening(index));
    }

    public void closeUI(string name)
    {
        int index = 0;
        for (int i = 0; i < uiList.Length; i++)
        {
            if (uiList[i].name == name)
            {
                index = i;
            }
        }

        StartCoroutine(closing(index));
    }

    public void callChild(string name)
    {
        int index = 0;
        for (int i = 0; i < uiList.Length; i++)
        {
            if (uiList[i].name == name)
            {
                index = i;
            }
        }

        uiList[index].CallScreen();
    }

    public bool getMove(string name)
    {
        int index = 0;
        for (int i = 0; i < uiList.Length; i++)
        {
            if (uiList[i].name == name)
            {
                index = i;
            }
        }

        return uiList[index].inMove;
    }

    public void setDynamic(string name)
    {
        int index = 0;
        for (int i = 0; i < uiList.Length; i++)
        {
            if (uiList[i].name == name)
            {
                index = i;
            }
        }

        uiList[index].isStatic = false;
    }

    IEnumerator bgFadeIn(int index)
    {
        Image bg = uiList[index].gameObject.GetComponent<Image>();

        float speed = animSpeed;
        float trans = uiList[index].maxA;
        float fade = 0.1f;
        bg.color = new Color32((byte)(bg.color.r * 255), (byte)(bg.color.g * 255), (byte)(bg.color.b * 255), (byte)(fade * 255));

        while (bg.color.a < trans + 0.01f)
        {
            fade += Time.fixedDeltaTime * speed;
            bg.color = new Color32((byte)(bg.color.r * 255), (byte)(bg.color.g * 255), (byte)(bg.color.b * 255), (byte)(fade * 255));

            yield return null;
        }

        bg.color = new Color32((byte)(bg.color.r * 255), (byte)(bg.color.g * 255), (byte)(bg.color.b * 255), (byte)(trans * 255));
    }

    IEnumerator bgFadeOut(int index)
    {
        Image bg = uiList[index].gameObject.GetComponent<Image>();

        float speed = animSpeed;
        float trans = 0.1f;
        float fade = uiList[index].maxA;
        bg.color = new Color32((byte)(bg.color.r * 255), (byte)(bg.color.g * 255), (byte)(bg.color.b * 255), (byte)(fade * 255));

        while (bg.color.a > trans - 0.01f)
        {
            fade -= Time.fixedDeltaTime * speed;
            bg.color = new Color32((byte)(bg.color.r * 255), (byte)(bg.color.g * 255), (byte)(bg.color.b * 255), (byte)(fade * 255));

            yield return null;
        }

        bg.color = new Color32((byte)(bg.color.r * 255), (byte)(bg.color.g * 255), (byte)(bg.color.b * 255), (byte)(trans * 255));
    }

    // Update is called once per frame
    IEnumerator opening(int index)
    {
        int idx = 0;
        for (int i = 0; i < uiList.Length; i++)
        {
            if (uiList[i].gameObject.activeInHierarchy && !uiList[i].isStatic)
            {
                idx = i;
            }
        }

        yield return closing(idx);
        yield return bgFadeOut(idx);

        RectTransform rect = uiList[index].transform.GetChild(0).gameObject.GetComponent<RectTransform>();
        uiList[index].gameObject.SetActive(true);
        StartCoroutine(bgFadeIn(index));

        if (uiList[index].isScale)
        {
            rect.localScale = new Vector3(0, 0, 1);

            float speed = animSpeed;
            float x = 0;
            float y = 0;

            while (rect.localScale.x < 1)
            {
                x += Time.deltaTime * speed;
                y += Time.deltaTime * speed;
                rect.localScale = new Vector3(x, y, 1);

                yield return null;
            }

            rect.localScale = new Vector3(1, 1, 1);
        }
    }

    // Update is called once per frame
    IEnumerator closing(int index)
    {
        RectTransform rect = uiList[index].transform.GetChild(0).gameObject.GetComponent<RectTransform>();

        if (uiList[index].isScale)
        {
            rect.localScale = new Vector3(1, 1, 1);

            float speed = animSpeed;
            float x = 1;
            float y = 1;

            while (rect.localScale.x > 0.1f)
            {
                x -= Time.deltaTime * speed;
                y -= Time.deltaTime * speed;
                rect.localScale = new Vector3(x, y, 1);

                yield return null;
            }

            rect.localScale = new Vector3(0, 0, 1);
            uiList[index].gameObject.SetActive(false);
        }
    }
}
