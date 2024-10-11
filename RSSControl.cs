using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class RSSControl : MonoBehaviour
{
    public charController _cc;

    public bool dynamicRss;
    public bool idleRss;
    public bool destroyable;
    public MeshRenderer rssResidu;
    public GameObject rssObj;
    string RSSpos;
    public bool canceled;
    public bool isGathered;

    private void OnEnable()
    {
        if (!dynamicRss)
            return;

        _cc = GameManager.gm.character[0];
    }

    public void identify()
    {
        RSSpos = ((int)transform.position.x).ToString() + ((int)transform.position.z).ToString();
        if (PlayerPrefs.HasKey("RSS" + RSSpos))
        {
            int repos = PlayerPrefs.GetInt("RSS" + RSSpos, 0);
            if (int.Parse(DateTime.Now.DayOfYear.ToString() + DateTime.Now.Hour.ToString()) - repos < 4)
            {
                if (destroyable)
                {
                    rssResidu.gameObject.SetActive(false);
                }
                rssResidu.material.mainTextureOffset = new Vector2(0.125f * 7, 0);
                _cc = GameManager.gm.character[0];
                rssRecord();
                _cc.pOne.ReportGather(RSSpos);
            }
            else
            {
                respawn();
                PlayerPrefs.DeleteKey("RSS" + RSSpos);
            }
        }
        else
        {
            respawn();
        }
    }

    void respawn()
    {
        isGathered = false;
        gameObject.GetComponent<SphereCollider>().enabled = true;
        rssObj.SetActive(true);
        rssResidu.gameObject.SetActive(true);
        rssResidu.transform.parent.gameObject.SetActive(false);
        rssResidu.material.mainTextureOffset = new Vector2(0, 0);

        if(idleRss)
        {
            animIdle();
        }
    }

    void animIdle()
    {
        rssResidu.transform.parent.gameObject.SetActive(true);
        InvokeRepeating("rssAnim", 0, 0.08f);
    }

    public void gather(float time)
    {
        isGathered = true;
        _cc.inGather = true;
        if(gameObject.activeInHierarchy)StartCoroutine(waitgathered(time));
    }

    IEnumerator waitgathered(float time)
    {
        float gathherCounter = time;
        while (gathherCounter > 0 && _cc.inGather)
        {
            gathherCounter -= Time.deltaTime;
            yield return null;
        }

        if (_cc.inGather && gameObject.GetComponent<SphereCollider>().enabled)
        {
            if(dynamicRss)
            {
                gathered();
                transform.root.gameObject.GetComponent<Villain>().Invoke("reSpawn", 4);
            }
            else
            {
                rssRecord();
                SoundManager.sm.rssPlay(int.Parse(transform.parent.name));
                _cc.pOne.ReportGather(RSSpos);
            }

            pickRSS();
        }
        else
        {
            isGathered = false;
        }
    }

    void pickRSS()
    {
        if(!DataServer.call.isBHMode && !DataServer.call.isTHMode) DataManager.dm.addProgress("Exp", GameManager.gm.rssExp);
        DataManager.dm.addItem(2, transform.parent.name, int.Parse(transform.parent.name), GameManager.gm.rssResult, "Resource", "Raw", 1);
    }

    public void rssRecord()
    {
        RSSpos = ((int)transform.position.x).ToString() + ((int)transform.position.z).ToString();
        PlayerPrefs.SetInt("RSS" + RSSpos, int.Parse(DateTime.Now.DayOfYear.ToString() + DateTime.Now.Hour.ToString()));
        gathered();
    }

    void gathered()
    {
        gameObject.GetComponent<SphereCollider>().enabled = false;
        rssObj.SetActive(false);
        if (!idleRss)
        {
            rssResidu.transform.parent.gameObject.SetActive(true);
            InvokeRepeating("rssAnim", 0, 0.08f);
        }
    }

    // Update is called once per frame
    void rssAnim()
    {
        if (rssResidu.material.mainTextureOffset.x < 0.125 * 6)
        {
            rssResidu.material.mainTextureOffset = new Vector2(rssResidu.material.mainTextureOffset.x + 0.125f, 0);
        }
        else
        {
            CancelInvoke("rssAnim");

            if (idleRss)
            {
                rssResidu.gameObject.SetActive(false);
                if (!isGathered)
                {
                    if (gameObject.activeInHierarchy) StartCoroutine(rssRespawn());
                }
            }
            else
            {
                if (gameObject.activeInHierarchy) StartCoroutine(rssDestroy());
            }
        }
    }

    IEnumerator rssRespawn()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(4,8));

        if (!isGathered)
        {
            respawn();
        }
    }

    IEnumerator rssDestroy()
    {
        yield return new WaitForSeconds(4);
        if (destroyable)
        {
            rssResidu.gameObject.SetActive(false);
        }
    }
}
