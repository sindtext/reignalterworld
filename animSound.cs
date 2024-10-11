using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animSound : MonoBehaviour
{
    public AudioSource audioFx;

    public AudioClip animalFx;
    public AudioClip fishFx;
    public AudioClip peatFx;
    public AudioClip plantFx;
    public AudioClip stoneFx;
    public AudioClip treeFx;

    // Start is called before the first frame update
    void Start()
    {
        audioFx = gameObject.GetComponent<AudioSource>();
    }

    void randompitch()
    {
        audioFx.pitch = Random.Range(0.8f, 2.4f);
    }

    // Update is called once per frame
    public void animalPlay()
    {
        audioFx.pitch = 1;
        audioFx.clip = animalFx;
        audioFx.Play();
    }

    // Update is called once per frame
    public void fishPlay()
    {
        randompitch();
        audioFx.clip = fishFx;
        audioFx.Play();
    }

    // Update is called once per frame
    public void peatPlay()
    {
        randompitch();
        audioFx.clip = peatFx;
        audioFx.Play();
    }

    // Update is called once per frame
    public void plantPlay()
    {
        audioFx.pitch = 1;
        audioFx.clip = plantFx;
        audioFx.Play();
    }

    // Update is called once per frame
    public void stonePlay()
    {
        randompitch();
        audioFx.clip = stoneFx;
        audioFx.Play();
    }

    // Update is called once per frame
    public void treePlay()
    {
        randompitch();
        audioFx.clip = treeFx;
        audioFx.Play();
    }
}
