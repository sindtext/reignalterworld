using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Player : NetworkBehaviour
{
    NetworkCharacterController _ncc;
    CharacterController _CC;
    charController _cc;
    WeaponManager wm;

    bool Act;

    [Networked(OnChanged = nameof(OnEquip))] public NetworkBool onSpawn { get; set; }

    [Networked(OnChanged = nameof(OnAction))] public NetworkBool onAct { get; set; }


    private void Awake()
    {
        _ncc = GetComponent<NetworkCharacterController>();
        _CC = GetComponent<CharacterController>();
        _cc = transform.GetChild(0).GetComponent<charController>();
        wm = _cc.gameObject.GetComponent<WeaponManager>();
    }

    private void Start()
    {
        GameManager.gm.LoaderUI.SetActive(false);
        _cc.isMine = _ncc.HasInputAuthority;
        _CC.enabled = false;
        _cc.playerSetup();
        StartCoroutine(checkRadius());
    }

    public static void OnAction(Changed<Player> act)
    {
        act.Behaviour.wm.weaponSwitch();
    }

    public static void OnEquip(Changed<Player> act)
    {
        act.Behaviour.wm.equip();
    }

    public void ReportGather(string RSSpos)
    {
        if(Object.HasInputAuthority)
        {
            RPC_Gathering(RSSpos);
        }
    }

    IEnumerator checkRadius()
    {
        yield return new WaitForSeconds(0.1f);

        Collider[] pCall = Physics.OverlapSphere(transform.position, 16);

        foreach (Collider other in pCall)
        {
            if (other.CompareTag("Player"))
            {
                if (other.transform.parent != transform && Vector3.Distance(GameManager.gm.spawnPos, transform.position) >= 112)
                {
                    other.gameObject.GetComponent<charController>().shadow = transform.GetChild(0).gameObject;
                    transform.GetChild(0).gameObject.SetActive(false);
                }
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data) && !_cc.inAct)
        {
            if(data.act == 0 && Act)
            {
                Act = true;
                _cc.initAnim(data.act);
                wm.netActiveWeapon = data.act;
                onAct = !onAct;
            }
            else
            {
                Act = false;
            }

            float normalizedSpeed = Vector3.Dot(data.direction.normalized, data.direction.normalized);
            if (data.direction == Vector3.zero)
            {
                _cc.playerSpeed = Mathf.Lerp(_cc.playerSpeed, normalizedSpeed * _cc.walkSpeed, 0.05f);
            }
            else
            {
                _cc.playerSpeed = Mathf.Lerp(_cc.playerSpeed, normalizedSpeed * _cc.runSpeed, 0.05f);
            }

            if (_cc.secondChar)
            {
                transform.position = GameManager.gm.character[0].transform.parent.position;
                transform.rotation = GameManager.gm.character[0].transform.parent.rotation;
            }
            else
            {
                data.direction.Normalize();
                _ncc.Move(4 * data.direction * Runner.DeltaTime);
            }
        }

        if (onSpawn)
            return;

        onSpawn = true;
        wm.netActiveWeapon = 1;
        wm.netWeaponType = DataManager.dm.weaponType;
        wm.netWeapon = DataManager.dm.weapon;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_Gathering(string rssID, RpcInfo info = default)
    {
        if(!info.IsInvokeLocal)
        {
            RSSControl[] RSSs = FindObjectsOfType<RSSControl>();

            foreach (RSSControl rss in RSSs)
            {
                string RSSpos = rss.transform.position.x.ToString() + rss.transform.position.z.ToString();
                if (RSSpos == rssID)
                {
                    rss.rssRecord();
                }
            }
        }
    }
}
