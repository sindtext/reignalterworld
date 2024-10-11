using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager sm;

    AudioSource audio;
    public AudioSource audioFx;
    public AudioSource playerFx;
    public AudioClip clickFx;
    public AudioClip alertFx;
    public AudioClip bagFx;
    public AudioClip emptyFx;
    public AudioClip reloadFx;
    public AudioClip[] weaponFx;
    public AudioClip[] impactFx;
    public AudioClip[] rssFx;
    public AudioClip[] villainFx;
    public AudioClip[] villdieFx;

    Button[] anyBtn;

    private void Awake()
    {
        sm = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        audio = gameObject.GetComponent<AudioSource>();
        audioFx = transform.GetChild(0).gameObject.GetComponent<AudioSource>();
        playerFx = transform.GetChild(1).gameObject.GetComponent<AudioSource>();

        anyBtn = FindObjectsOfType<Button>(true);
        foreach(Button btn in anyBtn)
        {
            btn.onClick.AddListener(clickPlay);
        }
    }

    void randompitch()
    {
        audioFx.pitch = Random.Range(0.8f, 2.4f);
        playerFx.pitch = Random.Range(0.8f, 2.4f);
    }

    void clickPlay()
    {
        audio.clip = clickFx;
        audio.Play();
    }

    public void alertPlay()
    {
        audio.clip = alertFx;
        audio.Play();
    }

    public void bagPlay()
    {
        playerFx.pitch = 1;
        playerFx.clip = bagFx;
        playerFx.Play();
    }

    public void emptyPlay()
    {
        playerFx.clip = emptyFx;
        playerFx.Play();
    }

    public void reloadPlay()
    {
        playerFx.pitch = 1;
        playerFx.clip = reloadFx;
        playerFx.Play();
    }

    public void weaponPlay(int idx)
    {
        randompitch();
        playerFx.clip = weaponFx[idx];
        playerFx.Play();
    }

    public void impactPlay(int idx)
    {
        randompitch();
        audioFx.clip = impactFx[idx];
        audioFx.Play();
    }

    public void rssPlay(int idx)
    {
        randompitch();
        audioFx.clip = rssFx[idx];
        audioFx.Play();
    }

    public void villainPlay(int idx)
    {
        randompitch();
        audioFx.clip = villainFx[idx];
        audioFx.Play();
    }

    public void villDiePlay(int idx)
    {
        randompitch();
        audioFx.clip = villdieFx[idx];
        audioFx.Play();
    }
}
