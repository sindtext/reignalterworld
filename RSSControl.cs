using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class RSSControl : MonoBehaviour
{
    public charController _cc;

    public MeshRenderer rssResidu;
    public GameObject rssObj;
    string RSSpos;
    public bool canceled;
    public bool isGathered;

    private void OnEnable()
    {
        StartCoroutine(identify());
    }

    IEnumerator identify()
    {
        yield return new WaitForSeconds(0.1f);

        RSSpos = transform.position.x.ToString() + transform.position.z.ToString();
        if (PlayerPrefs.HasKey("RSS" + RSSpos))
        {
            int repos = PlayerPrefs.GetInt("RSS" + RSSpos, 0);
            if (int.Parse(DateTime.Now.DayOfYear.ToString() + DateTime.Now.Hour.ToString()) - repos < 4)
            {
                rssResidu.material.mainTextureOffset = new Vector2(0.125f * 7, 0);
                _cc = GameManager.gm.character[0];
                gather(0);
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
        gameObject.GetComponent<SphereCollider>().enabled = true;
        rssObj.SetActive(true);
        rssResidu.gameObject.SetActive(false);
        rssResidu.material.mainTextureOffset = new Vector2(0, 0);
    }

    public void gather(float time)
    {
        StartCoroutine(waitgathered(time));
    }

    IEnumerator waitgathered(float time)
    {
        yield return new WaitForSeconds(time);

        if(!canceled)
        {
            rssRecord();
            _cc.pOne.ReportGather(RSSpos);
        }
    }

    public void rssRecord()
    {
        RSSpos = transform.position.x.ToString() + transform.position.z.ToString();
        PlayerPrefs.SetInt("RSS" + RSSpos, int.Parse(DateTime.Now.DayOfYear.ToString() + DateTime.Now.Hour.ToString()));
        gathered();
    }

    void gathered()
    {
        gameObject.GetComponent<SphereCollider>().enabled = false;
        rssObj.SetActive(false);
        rssResidu.gameObject.SetActive(true);
        InvokeRepeating("rssAnim", 0, 0.1f);
    }

    // Update is called once per frame
    void rssAnim()
    {
        if (rssResidu.material.mainTextureOffset.x < 0.125f * 6)
        {
            rssResidu.material.mainTextureOffset = new Vector2(rssResidu.material.mainTextureOffset.x + 0.125f, 0);
        }
        else
        {
            CancelInvoke();
        }
    }
}
