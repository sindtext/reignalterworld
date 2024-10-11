using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Player : NetworkBehaviour
{
    [Header("GET")]
    public NetworkCharacterController _ncc;
    public CharacterController _CC;
    public ProjectilesCtrl pc;
    public NetworkObject netObj;

    charController _cc;
    WeaponManager wm;
    ArmorManager am;

    [Header("EXTERNAL")]
    public LayerMask hitMask;

    private void Awake()
    {
        _ncc = GetComponent<NetworkCharacterController>();
        _CC = GetComponent<CharacterController>();
        _cc = transform.GetChild(0).GetComponent<charController>();
        wm = _cc.gameObject.GetComponent<WeaponManager>();
        am = _cc.gameObject.GetComponent<ArmorManager>();
        pc = transform.GetChild(1).GetComponent<ProjectilesCtrl>();
        netObj = gameObject.GetComponent<NetworkObject>();
    }

    private void Start()
    {
        _cc.isMine = _ncc.HasInputAuthority;
        _cc.isMaster = _ncc.HasStateAuthority;
        _CC.enabled = false;

        _cc.playerSetup();
        StartCoroutine(checkRadius());
    }

    public void ReportGather(string RSSpos)
    {
        if(Object.HasInputAuthority)
        {
            RPC_Gathering(RSSpos);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data) && !_cc.isDie)
        {
            float normalizedSpeed = Vector3.Dot(data.direction.normalized, data.direction.normalized);
            if (data.direction == Vector3.zero)
            {
                _cc.playerSpeed = Mathf.Lerp(_cc.playerSpeed, normalizedSpeed * _cc.walkSpeed, 0.16f);
            }
            else
            {
                if (_cc.inMove) _cc.inMove = false;
                if (_cc.inGather) _cc.inGather = false;
                _cc.playerSpeed = Mathf.Lerp(_cc.playerSpeed, normalizedSpeed * _cc.runSpeed, 0.08f);
            }

            if (_cc.secondChar)
            {
                int idx = GameManager.gm.character[0] == _cc ? 1 : 0;
                transform.position = GameManager.gm.character[idx].transform.parent.position;
                transform.rotation = GameManager.gm.character[idx].transform.parent.rotation;
            }
            else
            {
                data.direction.Normalize();
                _ncc.Move(data.direction * Runner.DeltaTime);
            }
        }
    }

    IEnumerator checkRadius()
    {
        yield return new WaitWhile(() => !Runner.IsRunning);
        yield return new WaitWhile(() => !Runner.isActiveAndEnabled);
        yield return new WaitWhile(() => !netObj.IsValid);
        yield return new WaitWhile(() => !netObj.isActiveAndEnabled);
        yield return new WaitWhile(() => GameManager.gm.netStarting);

        if (_cc.isMine)
        {
            yield return new WaitWhile(() => !netObj.HasStateAuthority);
            yield return new WaitWhile(() => !netObj.HasInputAuthority);
            yield return new WaitForSeconds(.16f);

            RPC_buildCharacter(
                DataManager.dm.pHunter,
                DataManager.dm.pGatherer,
                DataManager.dm.pEndurance,
                DataManager.dm.pStength,
                DataManager.dm.pSpeed,
                DataManager.dm.pAgility,
                DataManager.dm.pAccuracy,
                DataManager.dm.pIntelligence,
                PlayerPrefs.GetFloat("currentHealth", _cc.pHealth)
            );

            _cc.switchWeapon();
            _cc.switchArmor();

            RPC_charVisibility(!_cc.secondChar);
        }
        else
        {
            //yield return new WaitForSeconds(.32f);

            GameManager.gm.character[0].pOne.RPC_buildCharacter(
                DataManager.dm.pHunter,
                DataManager.dm.pGatherer,
                DataManager.dm.pEndurance,
                DataManager.dm.pStength,
                DataManager.dm.pSpeed,
                DataManager.dm.pAgility,
                DataManager.dm.pAccuracy,
                DataManager.dm.pIntelligence,
                PlayerPrefs.GetFloat("currentHealth", GameManager.gm.character[0].pHealth)
            );

            GameManager.gm.character[0].switchWeapon();
            GameManager.gm.character[0].switchArmor();

            GameManager.gm.character[0].pOne.RPC_charVisibility(!GameManager.gm.character[0].secondChar);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_charVisibility(bool visi, RpcInfo info = default)
    {
        transform.GetChild(0).gameObject.SetActive(visi);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_buildCharacter(float hunt, float geth, float end, float str, float spd, float agi, float accu, float intel, float chealth, RpcInfo info = default)
    {
        StartCoroutine(EXE_buildCharacter(hunt, geth, end, str, spd, agi, accu, intel, chealth));
    }

    IEnumerator EXE_buildCharacter(float hunt, float geth, float end, float str, float spd, float agi, float accu, float intel, float chealth)
    {
        yield return new WaitWhile(() => _cc == null);
        yield return new WaitWhile(() => !_cc.gameObject.activeInHierarchy);
        _cc.buildCharacter(hunt, geth, end, str, spd, agi, accu, intel, chealth);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_Death(RpcInfo info = default)
    {
        _cc.animator.SetBool("Die", true);
        _cc.isDie = true;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_Revive(RpcInfo info = default)
    {
        _cc.animator.SetBool("Die", false);
        _cc.isDie = false;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_weaponSwitch(int type, int weapon, int lvl, float radius, float impulse, float areadmg, float dmg, float speed, float ammo, float life, float fade, float multiDmg, RpcInfo info = default)
    {
        StartCoroutine(EXE_weaponSwitch(type, weapon, lvl, radius, impulse, areadmg, dmg, speed, ammo, life, fade, multiDmg));
    }

    IEnumerator EXE_weaponSwitch(int type, int weapon, int lvl, float radius, float impulse, float areadmg, float dmg, float speed, float ammo, float life, float fade, float multiDmg)
    {
        yield return new WaitWhile(() => _cc == null);
        yield return new WaitWhile(() => !_cc.gameObject.activeInHierarchy);
        wm.netWeaponType = type;
        wm.netWeapon = weapon;
        wm.netWeaponLvl = lvl;
        pc.shotSetup(netObj, GameManager.gm.weaponTypeList[wm.netWeaponType].weapon[wm.netWeapon].bulletType, radius, impulse, areadmg, dmg, speed, ammo, life, fade, multiDmg, wm.netWeaponType);
        wm.netActiveWeapon = 1;
        _cc.animator.SetFloat("Pos", wm.netWeaponType);
        _cc.animator.SetInteger("Weapon", wm.netWeaponType);
        if (_cc.inGather) _cc.inGather = false;
        wm.equip();
        //onSwitch = !onSwitch;
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_armorSwitch(int atype, int[] armor, int[] aLvl, int stype, int[] skin, int actived, RpcInfo info = default)
    {
        StartCoroutine(EXE_armorSwitch(atype, armor, aLvl, stype, skin, actived));
    }

    IEnumerator EXE_armorSwitch(int atype, int[] armor, int[] aLvl, int stype, int[] skin, int actived)
    {
        yield return new WaitWhile(() => _cc == null);
        yield return new WaitWhile(() => !_cc.gameObject.activeInHierarchy);
        am.netArmorType = atype;
        am.netArmor = armor;
        am.netArmorLvl = aLvl;

        am.netSkinType = stype;
        am.netSkin = skin;

        am.netActiveArmor = actived;
        am.equip();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_Gathering(string rssID, RpcInfo info = default)
    {
        if(!info.IsInvokeLocal)
        {
            RSSControl[] RSSs = FindObjectsOfType<RSSControl>();

            foreach (RSSControl rss in RSSs)
            {
                string RSSpos = ((int)rss.transform.position.x).ToString() + ((int)rss.transform.position.z).ToString();
                if (RSSpos == rssID)
                {
                    rss.rssRecord();
                }
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_Action(int act, bool trueDmg, bool armed, RpcInfo info = default)
    {
        wm.netActiveWeapon = act;
        wm.weaponSwitch();

        _cc.actionAnim(wm.netActiveWeapon, armed);
        //onAct = !onAct;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_setDamage(NetworkObject sender, float damage, bool trueDmg, RpcInfo info = default)
    {
        _cc.getDamage(sender, damage, trueDmg);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_Damage(float hlt, RpcInfo info = default)
    {
        _cc.setHealth(hlt);
    }
}
