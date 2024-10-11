using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ProjectilesCtrl : NetworkBehaviour
{
    public GameObject bulletPref;
    public GameObject trowPref;
    public GameObject spreadPref;

    Transform bulletObj;
    Transform trowObj;
    Transform spreadObj;

    NetworkObject Sender;
    string names;
    int projectileIdx;
    float areaRadius;
    float areaImpulse;
    float areaDamage;
    float Damage;
    float bltSpeed;
    float Grafity;
    public float timeToLife;
    public float timeToFade;
    float multiDmg;
    public bool trueDmg;

    int bulletSlot;
    public float magazine;
    float magazSlot;
    float multiplyDmg;
    float multiplyAreaDmg;
    bool onReload;

    // Start is called before the first frame update
    void Start()
    {
        bulletObj = transform.GetChild(0).GetChild(0).transform;
        trowObj = transform.GetChild(0).GetChild(1).transform;
        spreadObj = transform.GetChild(0).GetChild(2).transform;
    }

    // Update is called once per frame
    public void shotSetup(NetworkObject sender, string type, float radius, float impulse, float areadmg, float dmg, float speed, float ammo, float life, float fade, float multidmg, int idx)
    {
        Sender = sender;
        names = type;
        projectileIdx = idx;
        areaRadius = radius;
        areaImpulse = impulse;
        areaDamage = areadmg;
        Damage = dmg;
        bltSpeed = speed;
        Grafity = 9.81f;
        timeToLife = life;
        timeToFade = fade;
        multiDmg = multidmg;

        magazine = 1;
        magazSlot = GameManager.gm.weaponTypeList[projectileIdx].weapon[0].ammoSlot;

        if (Sender.transform.GetChild(0).GetComponent<charController>().isMine) GameManager.gm.aj.useAmmo(1);

        if (magazSlot == 999)
            return;

        if (Sender.transform.GetChild(0).GetComponent<charController>().isMine)
        {
            for (int i = 0; i < DataManager.dm.backpack[0].bag.Length; i++)
            {
                if (DataManager.dm.backpack[0].bag[i].idx == (int)ammo)
                {
                    bulletSlot = i;

                    break;
                }
            }

            magazine = Mathf.Min(PlayerPrefs.GetFloat("currentAmmo" + bulletSlot.ToString(), magazSlot), DataManager.dm.backpack[0].bag[bulletSlot].amount);
            GameManager.gm.aj.useAmmo(magazine / magazSlot);
        }
    }

    public void useWeapon()
    {
        if(magazine > 0)
        {
            multiplyDmg = Damage * multiDmg / 100;
            multiplyAreaDmg = areaDamage * multiDmg / 100;
            Invoke(names, timeToFade);
            if (names == "streak")
            {
                Invoke(names, timeToFade + .16f);
                Invoke(names, timeToFade + .32f);
                Invoke(names, timeToFade + .48f);
            }

            if (magazSlot == 999 || !Sender.transform.GetChild(0).GetComponent<charController>().isMine)
                return;

            magazine--;
            PlayerPrefs.SetFloat("currentAmmo" + bulletSlot.ToString(), magazine);
            GameManager.gm.aj.useAmmo(magazine / magazSlot);
        }
        else
        {
            SoundManager.sm.emptyPlay();
            if (Sender.transform.GetChild(0).GetComponent<charController>().isMine)
            {
                if (!onReload)
                {
                    SoundManager.sm.reloadPlay();
                    StartCoroutine(reload());
                }
            }
        }
    }

    IEnumerator reload()
    {
        onReload = true;
        float magazCurrent = DataManager.dm.backpack[0].bag[bulletSlot].amount;
        float magazRest = magazCurrent - Mathf.Min(magazCurrent, magazSlot);
        float magazReload = 0;

        Debug.Log(Mathf.Min(magazCurrent, magazSlot) * -1);
        DataManager.dm.addItem(0, DataManager.dm.backpack[0].bag[bulletSlot].names, DataManager.dm.backpack[0].bag[bulletSlot].idx, Mathf.Min(magazCurrent, magazSlot) * -1, "Item", "Ammo", 1);

        while (magazRest > 0 && magazReload < Mathf.Min(magazSlot, magazCurrent))
        {
            magazReload++;
            GameManager.gm.aj.useAmmo(magazReload / magazSlot);
            yield return new WaitForSeconds(timeToFade);
        }

        magazine = Mathf.Min(magazSlot, magazCurrent);
        onReload = false;
    }

    void melee()
    {
        SpreadCtrl spreadBlt = Runner.Spawn(spreadPref, spreadObj.position, spreadObj.rotation, Runner.LocalPlayer).GetComponent<SpreadCtrl>();
        spreadBlt.bulletInit(Sender, areaRadius, areaImpulse, areaDamage + multiplyAreaDmg, Damage + multiplyDmg, bltSpeed, Grafity, timeToLife, timeToFade, projectileIdx, trueDmg);
    }

    void trow()
    {
        TrowCtrl trowBlt = Runner.Spawn(trowPref, trowObj.position, trowObj.rotation, Runner.LocalPlayer).GetComponent<TrowCtrl>();
        trowBlt.bulletInit(Sender, areaRadius, areaImpulse, areaDamage + multiplyAreaDmg, Damage + multiplyDmg, bltSpeed, Grafity, timeToLife, timeToFade, projectileIdx, trueDmg);
    }

    void shot()
    {
        BulletCtrl shotBlt = Runner.Spawn(bulletPref, bulletObj.position, bulletObj.rotation, Runner.LocalPlayer).GetComponent<BulletCtrl>();
        shotBlt.bulletInit(Sender, areaRadius, areaImpulse, areaDamage, Damage, bltSpeed, .16f, timeToLife, timeToFade, projectileIdx, trueDmg);
    }

    void spread()
    {
        for (int i = 0; i < 8; i++)
        {
            spreadObj.localRotation = Quaternion.Euler(0, (i - 3) * 5, 0);

            SpreadCtrl spreadBlt = Runner.Spawn(spreadPref, spreadObj.position, spreadObj.rotation, Runner.LocalPlayer).GetComponent<SpreadCtrl>();
            spreadBlt.bulletInit(Sender, areaRadius, areaImpulse, areaDamage, Damage, bltSpeed, 2, timeToLife, timeToFade, projectileIdx, trueDmg);
        }
    }

    void chase()
    {
        BulletCtrl shotBlt = Runner.Spawn(bulletPref, bulletObj.position, bulletObj.rotation, Runner.LocalPlayer).GetComponent<BulletCtrl>();
        shotBlt.bulletInit(Sender, areaRadius, areaImpulse, areaDamage, Damage, bltSpeed, Grafity, timeToLife, timeToFade, projectileIdx, trueDmg);
    }

    void streak()
    {
        BulletCtrl shotBlt = Runner.Spawn(bulletPref, bulletObj.position, bulletObj.rotation, Runner.LocalPlayer).GetComponent<BulletCtrl>();
        shotBlt.bulletInit(Sender, areaRadius, areaImpulse, areaDamage, Damage, bltSpeed, Grafity, timeToLife, timeToFade, projectileIdx, trueDmg);
    }

    public void impactSetup()
    {

    }
}
