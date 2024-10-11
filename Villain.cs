using System.Collections;
using System.Collections.Generic;
using BehaviorTree;
using UnityEngine.UI;
using Fusion;

public class Villain : Tree, ICanTakeDamage
{
    [UnityEngine.Header("GET")]
    public FieldOfView FOV;
    public NetworkObject netObj;
    
    UnityEngine.Transform parent;
    Node root;

    [UnityEngine.Header("EXTERNAL")]
    public UnityEngine.ParticleSystem shootVfx;
    public UnityEngine.ParticleSystem deathVfx;
    public UnityEngine.Transform healthBar;
    public Image hpBar;

    [UnityEngine.Header("SETUP")]
    public string[] vType = new[] { "Active", "Agresive", "Pasive", "Coward" };
    public int vIndex = 0;
    public float vilainDmg;
    public float vilainHealth;
    public float walkSpeed = 1.6f;
    public float runSpeed = 4.8f;
    public float fovRange = 8f;
    public float attackRange = 2f;

    [UnityEngine.Header("RUNTIME")]
    public UnityEngine.GameObject attacker;
    public float vilainCurrentHealth;
    public bool isDie;
    public bool runAway;
    public bool attacked;
    public bool repost;
    public bool duplicate;

    UnityEngine.Animator _anim;
    PortalCtrl[] waypoints;
    UnityEngine.Vector3[] wayPos;

    private void OnEnable()
    {
        isDie = false;
        FOV = gameObject.GetComponent<FieldOfView>();
        FOV.viewRadius = fovRange;
        netObj = gameObject.GetComponent<NetworkObject>();
        vilainCurrentHealth = vilainHealth;
        if(!DataServer.call.isBHMode && !DataServer.call.isTHMode) InvokeRepeating("checkLife", 8, 1);
    }

    public void brith(UnityEngine.Transform _parent)
    {
        parent = _parent;
        _root = SetupTree();
    }

    void checkLife()
    {
        if (netObj.StateAuthority.IsNone)
        {
            GameManager.gm.spawner[0].takeOver(netObj);
            PortalCtrl portal = FindObjectOfType<PortalCtrl>();
            brith(portal.transform);
        }
    }

    protected override void firstSpwan()
    {
        _anim = transform.GetChild(0).gameObject.GetComponent<UnityEngine.Animator>();
        InvokeRepeating("checkSpawn", 8, 4);
    }
    
    protected override Node SetupTree()
    {
        waypoints = FindObjectsOfType<PortalCtrl>();
        wayPos = new UnityEngine.Vector3[waypoints.Length];
        for(int i = 0; i < wayPos.Length; i++)
        {
            wayPos[i] = waypoints[i].transform.position;
        }

        if(vType[vIndex] == "Coward")
        {
            root = new Selector(new List<Node>
            {
                new TaskLife(transform),
                new Sequence(new List<Node>
                {
                    new CheckEnemyInFOVRange(transform),
                    new TaskAvoidTarget(transform, wayPos),
                }),
                new TaskPatrol(transform, wayPos),
            });
        }
        else if (vType[vIndex] == "Pasive")
        {
            root = new Selector(new List<Node>
            {
                new TaskLife(transform),
                new Sequence(new List<Node>
                {
                    new CheckEnemyInAttackRange(transform),
                    new TaskAttack(transform),
                }),
                new Sequence(new List<Node>
                {
                    new TaskGoToTarget(transform),
                }),
                new TaskPatrol(transform, wayPos),
            });
        }
        else if (vType[vIndex] == "Agresive")
        {
            root = new Selector(new List<Node>
            {
                new TaskLife(transform),
                new Sequence(new List<Node>
                {
                    new CheckEnemyInAttackRange(transform),
                    new TaskAttack(transform),
                }),
                new Sequence(new List<Node>
                {
                    new CheckEnemyInFOVRange(transform),
                    new TaskGoToTarget(transform),
                }),
                new TaskPatrol(transform, wayPos),
            });
        }
        else
        {
            root = new Selector(new List<Node>
            {
                new TaskLife(transform),
                new TaskAvoidTarget(transform, wayPos),
                new Sequence(new List<Node>
                {
                    new CheckEnemyInAttackRange(transform),
                    new TaskAttack(transform),
                }),
                new Sequence(new List<Node>
                {
                    new CheckEnemyInFOVRange(transform),
                    new TaskGoToTarget(transform),
                }),
                new TaskPatrol(transform, wayPos),
            });
        }

        return root;
    }

    private void LateUpdate()
    {
        healthBar.LookAt(UnityEngine.Camera.main.transform);
    }

    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        if(other.gameObject.layer == UnityEngine.LayerMask.NameToLayer("Water") || other.gameObject.layer == UnityEngine.LayerMask.NameToLayer("Obstacle"))
        {
            repost = true;
        }
    }

    public void checkSpawn()
    {
        if (_root == null)
            return;

        bool seen = false;
        UnityEngine.Collider[] vCall = UnityEngine.Physics.OverlapSphere(transform.position, 32, FOV.targetMask);

        foreach (UnityEngine.Collider other in vCall)
        {
            if (other.CompareTag("Player"))
            {
                seen = true;
                break;
            }
        }

        if (!seen)
            RPC_reSpawn();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    protected override void RPC_reSpawn(RpcInfo info = default)
    {
        if (info.IsInvokeLocal)
        {
            StartCoroutine(waitForRespawn());
        }
        bool seen = false;
        UnityEngine.Collider[] vCall = UnityEngine.Physics.OverlapSphere(transform.position, 32, FOV.targetMask);

        foreach (UnityEngine.Collider other in vCall)
        {
            if (other.CompareTag("Player"))
            {
                seen = true;
                break;
            }
        }

        if (!seen)
            return;

        GameManager.gm.spawner[0].takeOver(netObj);
        PortalCtrl portal = FindObjectOfType<PortalCtrl>();
        brith(portal.transform);
    }

    IEnumerator waitForRespawn()
    {
        yield return new UnityEngine.WaitForSeconds(1);

        if(netObj.HasStateAuthority) reSpawn();
    }

    public void reSpawn()
    {
        if (parent != null) parent.GetComponent<PortalCtrl>().spawnTimer += 4;

        GameManager.gm.spawner[0].takeOver(netObj);
        StartCoroutine(waitDestroy());
    }

    IEnumerator waitDestroy()
    {
        yield return new UnityEngine.WaitWhile(() => !netObj.HasStateAuthority);
        netObj.Runner.Despawn(netObj);
    }

    public void vAttack(charController _cc)
    {
        _cc.takeDamage(netObj, vilainDmg, true);
        SoundManager.sm.villainPlay(vIndex);
        if (shootVfx != null) shootVfx.Play();
    }

    public void takeDamage(NetworkObject sender, float damage, bool trueDmg)
    {
        if(isDie) return;

        RPC_vlnDamage(sender, damage);
    }

    [Rpc(sources : RpcSources.All, RpcTargets.All)]
    protected override void RPC_vlnDamage(NetworkObject sender, float damage, RpcInfo info = default)
    {
        attacked = true;
        attacker = sender.transform.GetChild(0).gameObject;

        vilainCurrentHealth -= vType[vIndex] == "Coward" ? 1 : damage;
        healthBar.gameObject.SetActive(true);
        hpBar.fillAmount = vilainCurrentHealth / vilainHealth;

        isDie = vilainCurrentHealth <= 0;
        if (isDie)
        {
            SoundManager.sm.villDiePlay(vIndex);
            if (deathVfx != null) deathVfx.Play();
            _anim.SetBool("Die", true);

            if (vType[vIndex] == "Coward")
            {
                transform.GetChild(3).gameObject.SetActive(true);
            }
            else if (GameManager.gm.character[0].gameObject == attacker)
            {
                VillainCtrl villainCtrl = FindObjectOfType<VillainCtrl>();
                int objLength = villainCtrl.dropObj.Length;
                int[] dropIdx = new int[3] { getRandomItem(objLength), getRandomItem(objLength), getRandomItem(objLength) };
                villainCtrl.VillainDrop(transform.position, dropIdx);

                Invoke("reSpawn", 4);
            }

            if (GameManager.gm.character[0].gameObject == attacker)
                DataManager.dm.addProgress("Exp", GameManager.gm.huntExp);
        }
    }

    public void vlnDuplicate()
    {
        RPC_setDuplicate();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    protected override void RPC_setDuplicate(RpcInfo info = default)
    {
        duplicate = true;
    }
    
    int getRandomItem(int objLength)
    {
        int item = DataServer.call.isBHMode || DataServer.call.isTHMode ? 0 : UnityEngine.Random.Range(0, objLength);
        return item;
    }

    public bool areDeath()
    {
        bool death = isDie;
        return death;
    }
}
