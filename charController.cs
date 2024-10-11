using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class charController : MonoBehaviour, ICanTakeDamage
{
    [Header("GET")]
    public Animator animator;
    public ActionJoystick aj;
    public Player pOne;

    public FieldOfView FOV;
    CamCtrl cameraPivot;
    BorderCtrl bordPivot;
    CapsuleCollider capsuleCollider;
    float capsuleHalfHeight;

    [Header("EXTERNAL")]
    public Transform healthBar;
    public Image hpBar;

    [Header("Ability")]
    float pConstitution;
    float pPower;
    float pMovement;
    float pDexterity;
    float pSense;
    float pWisdom;

    [Header("BonusStats")]
    float pResilience;
    float pActSpeed;
    float pCastTime;
    float pCriticalDamage;
    float pCriticalChance;

    [Header("Stats")]
    public float pHealth;
    float pCarrying;
    float pRevenge;
    float pRoughness;
    float pMove = 6.4f;
    float pDeftness;
    float pCamouflage;
    float pEvasion;
    float pVision;
    float pReflect;
    float pVigilance;
    float pLuck;

    [Header("RUNTIME")]
    public float walkSpeed = 3f;
    public float crouchedSpeed = 2f;
    public float runSpeed;
    public float currentHealth;
    public float lockRange;
    public float unlockRange;
    public float luck;
    public float atkFoV;
    public float lockFoV;
    public float playerSpeed;

    public bool isMine;
    public bool isMaster;
    public bool secondChar;
    public bool actLock;
    public bool inHunt;
    public bool inGather;
    public bool inMove;
    public bool isDie;

    bool isGrounded = false;
    float idleCount = 3;
    Vector3 direction;
    float distance;
    ItemDrop dropObj;

    public void playerSetup()
    {
        animator = GetComponent<Animator>();
        capsuleCollider = transform.GetComponent<CapsuleCollider>();
        capsuleHalfHeight = capsuleCollider.height / 2;
        pOne = transform.parent.GetComponent<Player>();
        FOV = gameObject.GetComponent<FieldOfView>();

        if (isMine)
        {
            cameraPivot = FindObjectOfType<CamCtrl>();
            bordPivot = FindObjectOfType<BorderCtrl>();
            aj = FindObjectOfType<ActionJoystick>();

            secondChar = GameManager.gm.character[0] != null;

            if (secondChar)
            {
                transform.position = GameManager.gm.character[0].transform.position;
                GameManager.gm.character[1] = this;
            }
            else
            {
                aj._cc = this;
                aj.regAtk(pOne.transform.GetChild(2).gameObject);
                pOne._CC.enabled = true;
                GameManager.gm.character[0] = this;
                bordPivot.transform.position = new Vector3(transform.position.x, 0.008f, transform.position.z);
                cameraPivot.transform.position = new Vector3(transform.position.x, 0.008f, transform.position.z);
                cameraPivot.starting(this);
                //FOV.isDraw = true;
                //FOV.viewMeshFilter.gameObject.SetActive(true);
            }

            GameManager.gm.LoaderUI.SetActive(false);
            GameManager.gm.um.closeUI("Death");
            GameManager.gm.netStarting = false;
        }
    }

    public void buildCharacter(float hunt, float gath, float end, float str, float spd, float agi, float accu, float intel, float chealth)
    {
        pConstitution = end * 2 / 100;
        pPower = str * 2 / 100;
        pMovement = spd * 2 / 100;
        pDexterity = agi * 2 / 100;
        pSense = accu * 2 / 100;
        pWisdom = intel * 2 / 100;

        pResilience = (end + str) / 100;
        pActSpeed = (str + spd) / 100;
        pCastTime = (spd + agi) / 100;
        pCriticalDamage = (agi + accu) / 100;
        pCriticalChance = (accu + intel) / 100;

        float hunter = hunt / (hunt + gath);
        pHealth = 10000 * pConstitution * hunter;
        pRevenge = 100 * pPower * hunter;
        pMove = 12.8f * pMovement * hunter;
        pEvasion = 100 * pDexterity * hunter;
        pReflect = 100 * pSense * hunter;
        pVigilance = 24 * pWisdom * hunter;

        float gather = gath / (hunt + gath);
        pCarrying = 100 * pConstitution * gather;
        pRoughness = 100 * pPower * gather;
        pDeftness = 100 * pMovement * gather;
        pCamouflage = 8 * pDexterity * gather;
        pVision = 100 * pSense * gather;
        pLuck = 100 * pWisdom * gather;

        runSpeed = pMove;
        luck = pLuck;

        if (DataServer.call.isBHMode)
        {
            pHealth *= 4;
            currentHealth = pHealth;
            unlockRange = pCamouflage * 2;
            lockRange = pVigilance * 2;
            atkFoV = 120;
            lockFoV = 360;

            autoBounty();
        }
        else
        {
            currentHealth = chealth == 0 ? pHealth : Mathf.Min(pHealth, chealth);
            unlockRange = pCamouflage;
            lockRange = pVigilance;
            atkFoV = 48 * pReflect / 100 * Mathf.Max(GameManager.gm.usedWeapon.timeToLife, 0.8f);
            lockFoV = 96 * pVision / 100 * Mathf.Max(GameManager.gm.usedWeapon.timeToLife, 0.8f);

            if (DataServer.call.isTHMode) autoTreasure();
        }

        pOne._ncc.maxSpeed = runSpeed;
        FOV.viewAngle = lockFoV;
        FOV.viewRadius = lockRange;

        setHealth(currentHealth);
    }

    private void LateUpdate()
    {
        healthBar.LookAt(Camera.main.transform);
    }

    void FixedUpdate()
    {
        CheckGround();
        //--set the crouched state to a default value of false, unless I am pressing the crouch button 
        
        //--sets Speed, "inAir" and "isCrouched" parameters in the Animator--
        animator.SetBool("inAir", false);

        if (!isGrounded)
        {
            animator.SetBool("inAir", true);
        }

        if (!isMine || secondChar)
            return;

        animator.SetFloat("Speed", playerSpeed);
        if (playerSpeed > 6.4f)
        {
            animator.speed = playerSpeed / 6.4f;
        }
        else
        {
            animator.speed = 1;
        }

        if (playerSpeed < walkSpeed)
        {
            idleCount += Time.deltaTime;
            if (idleCount >= 3 && !inGather)
            {
                idleCount = 3;
                animator.SetBool("Idle", true);
            }
        }
        else
        {
            if (animator.GetBool("Idle")) animator.SetBool("Idle", false);
        }

        if(inMove)
        {
            direction = aj.targetAtk.transform.position - transform.position;
            distance = Vector3.Distance(aj.targetAtk.transform.position, transform.position);

            direction.Normalize();
            pOne._ncc.Move(direction * Time.fixedDeltaTime);
        }

        bordPivot.BordMove(transform.position, playerSpeed);
        //--check if character is on the ground
    }

    void CheckGround()
    {
        //--send a ray from the center of the collider to the ground. The player is "grounded" if the ray distance(length) is equal to half of the capsule height--
        Physics.Raycast(capsuleCollider.bounds.center, Vector3.down, out var hit);
        if (hit.distance < (capsuleHalfHeight + 0.1f))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    public void switchWeapon()
    {
        GameManager.gm.usedWeapon = GameManager.gm.weaponTypeList[DataManager.dm.weaponType].weapon[DataManager.dm.weapon];
        GameManager.gm.weaponSlot[0].sprite = GameManager.gm.weaponIcon[PlayerPrefs.GetInt("currentweaponType", 0)];
        pOne.RPC_weaponSwitch(
            DataManager.dm.weaponType,
            DataManager.dm.weapon,
            DataManager.dm.weaponLvl,
            GameManager.gm.usedWeapon.areaRadius,
            GameManager.gm.usedWeapon.areaImpulse,
            GameManager.gm.usedWeapon.areaDamage,
            GameManager.gm.usedWeapon.Damage,
            GameManager.gm.usedWeapon.bltSpeed,
            GameManager.gm.usedWeapon.ammoIdx,
            GameManager.gm.usedWeapon.timeToLife,
            GameManager.gm.usedWeapon.timeToFade,
            pDeftness
        );
    }

    public void switchArmor()
    {
        pOne.RPC_armorSwitch(
            DataManager.dm.armorType,
            DataManager.dm.armor,
            DataManager.dm.armorLvl,
            DataManager.dm.clothType,
            DataManager.dm.cloth,
            0
        );
    }

    void autoBounty()
    {
        CancelInvoke("getFOV");
        CancelInvoke("spawnBot");

        InvokeRepeating("getFOV", 8, pMove / 2);
        InvokeRepeating("spawnBot", 8, 64f / pMove);
    }

    void autoTreasure()
    {
        CancelInvoke("getFOV");
        InvokeRepeating("getFOV", 8, pMove / 2);
    }

    public void setFOV()
    {
        atkFoV = 48 * pReflect / 100 * Mathf.Max(GameManager.gm.usedWeapon.timeToLife, 0.8f);
        lockFoV = 96 * pVision / 100 * Mathf.Max(GameManager.gm.usedWeapon.timeToLife, 0.8f);
        FOV.viewAngle = lockFoV;
        FOV.viewRadius = lockRange;
    }

    public void getFOV()
    {
        FOV.FindVisibleTargets();
        if (FOV.visibleTargets.Count > 0)
        {
            CancelInvoke("scaleFoV");
            InvokeRepeating("reduceFoV", 0, .04f);
            bool onTarget = false;
            if(inHunt)
            {
                foreach(Transform tgt in FOV.visibleTargets)
                {
                    if(tgt.gameObject == aj.targetCtrl)
                    {
                        onTarget = true;
                    }
                }
            }
            
            if(!onTarget) aj.regAtk(FOV.visibleTargets[0].gameObject);
            aj.sign.gameObject.SetActive(false);
        }

        aj.atkPush();
    }

    void reduceFoV()
    {
        if(FOV.viewAngle <= atkFoV)
        {
            CancelInvoke("reduceFoV");
            return;
        }

        FOV.viewAngle -= 1.6f;
    }

    void scaleFoV()
    {
        if (FOV.viewAngle >= lockFoV)
        {
            CancelInvoke("scaleFoV");
            return;
        }

        FOV.viewAngle += 1.6f;
    }

    public void Action(int act)
    {
        if (act != 0 && !actLock)
        {
            StartCoroutine(autoMove(act));
        }
    }

    IEnumerator autoMove(int act)
    {
        direction = aj.targetAtk.transform.position - transform.position;
        distance = Vector3.Distance(aj.targetAtk.transform.position, transform.position);

        if ((DataServer.call.isBHMode || DataServer.call.isTHMode) && aj.targetAtk == pOne.transform.GetChild(2).gameObject)
        {
            inMove = true;

            int step = DataServer.call.isBHMode ? 80 : 0;

            while (step < 80 && inMove)
            {
                step++;
                playerSpeed = runSpeed;

                yield return null;
            }

            inMove = false;
            playerSpeed = walkSpeed;

            FOV.FindVisibleTargets();
            if (FOV.visibleTargets.Count > 0)
            {
                CancelInvoke("scaleFoV");
                InvokeRepeating("reduceFoV", 0, .04f);
                bool onTarget = false;
                if (inHunt)
                {
                    foreach (Transform tgt in FOV.visibleTargets)
                    {
                        if (tgt.gameObject == aj.targetCtrl)
                        {
                            onTarget = true;
                        }
                    }
                }

                if (!onTarget) aj.regAtk(FOV.visibleTargets[0].gameObject);
                aj.sign.gameObject.SetActive(false);
            }
        }
        else if (DataServer.call.isBHMode || DataServer.call.isTHMode || (aj.targetAtk.CompareTag("Nature") && distance < 4.8f && act != 1))
        {
            inMove = true;

            float range = act == 1 ? Mathf.Min(GameManager.gm.usedWeapon.areaRadius * GameManager.gm.usedWeapon.bltSpeed * GameManager.gm.usedWeapon.timeToLife * 2, lockRange) : 2.4f;
            while (distance > range && inMove)
            {
                playerSpeed = runSpeed;

                yield return null;
            }

            inMove = false;
            playerSpeed = walkSpeed;

            attack(act);
        }
        else
        {
            getFOV();

            yield return new WaitForSeconds(.16f);

            attack(1);
        }
    }

    void attack(int act)
    {
        aj.actAtk(act);

        bool doubleDmg = Random.Range(0, 100) < pReflect;
        bool trueDmg = Random.Range(0, 100) < pVision;
        bool armed = act == 1 ? pOne.pc.magazine > 0 : true;

        pOne.RPC_Action(act, trueDmg, armed);

        if (act == 1)
        {
            pOne.pc.trueDmg = trueDmg;
            pOne.pc.useWeapon();
            if (doubleDmg)
            {
                pOne.pc.trueDmg = trueDmg;
                pOne.pc.useWeapon();
            }
        }
    }

    public void actionAnim(int index, bool armed)
    {
        if (!gameObject.activeInHierarchy)
            return;

        idleCount = 0;
        animator.SetBool("Idle", false);
        animator.SetBool("Gathered", false);

        if(armed) animator.SetTrigger(index.ToString());
        actLock = true;
        if (index <= 1)
        {
            inHunt = true;
            animator.SetFloat("Skill", 0);
            StartCoroutine(endAtk(pOne.pc.timeToFade * 2));
        }
        else
        {
            StartCoroutine(endAtk(5));
        }
    }

    IEnumerator endAtk(float wait)
    {
        if (!isMine || secondChar)
        {
            actLock = false;
            animator.SetBool("Gathered", true);
            inHunt = false;

            yield break;
        }

        float atkCounter = wait;
        while (atkCounter > 0 && (inGather || inHunt))
        {
            atkCounter -= Time.deltaTime;
            yield return null;
        }

        actLock = false;
        animator.SetBool("Gathered", true);
        if (aj.targetAtk.CompareTag("Nature"))
        {
            inHunt = false;
            CancelInvoke("reduceFoV");
            InvokeRepeating("scaleFoV", 0, .08f);
            aj.btnIcon.sprite = aj.iconList[1];
            switchWeapon();
            aj.regAtk(pOne.transform.GetChild(2).gameObject);
            aj.sign.gameObject.SetActive(false);
        }
        else if (aj.targetAtk == pOne.transform.GetChild(2).gameObject || !aj.targetAtk.activeInHierarchy)
        {
            inHunt = false;
            CancelInvoke("reduceFoV");
            InvokeRepeating("scaleFoV", 0, .08f);
            aj.regAtk(pOne.transform.GetChild(2).gameObject);
            aj.sign.gameObject.SetActive(false);
        }
        else
        {
            distance = Vector3.Distance(aj.targetAtk.transform.position, transform.position);

            float unlockRage = aj.targetAtk.CompareTag("Player") ? aj.targetAtk.GetComponent<charController>().unlockRange : 0;

            if (distance >= lockRange - unlockRage || aj.targetAtk.GetComponent<ICanTakeDamage>().areDeath())
            {
                inHunt = false;
                CancelInvoke("reduceFoV");
                InvokeRepeating("scaleFoV", 0, .08f);
                aj.regAtk(pOne.transform.GetChild(2).gameObject);
                aj.sign.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isMine || secondChar)
            return;

        if (other.CompareTag("Nature") && !inHunt)
        {
            aj.regAtk(other.gameObject);
            aj.sign.gameObject.SetActive(true);
        }

        if(other.CompareTag("Item"))
        {
            dropObj = other.gameObject.GetComponent<ItemDrop>();

            if (GameManager.gm.character[0].pOne.gameObject != dropObj.itemrunner)
                return;

            if (dropObj.itemType == "Currency")
            {
                DataManager.dm.addProgress("Bronze", dropObj.itemAmnt);
            }
            else
            {
                int slot = dropObj.itemType == "Resource" ? 2 : dropObj.itemType == "Equipment" ? 1 : 0;
                DataManager.dm.addItem(slot, dropObj.itemName[dropObj.itemIdx], dropObj.itemIdx, dropObj.itemAmnt, dropObj.itemType, dropObj.itemSub, dropObj.itemRank);
            }

            dropObj.picked();
        }

        if(other.CompareTag("Interact"))
        {
            GameManager.gm.touchBuilding(other.gameObject.name);
            other.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isMine || secondChar)
            return;

        if (other.CompareTag("Nature") && !inHunt)
        {
            if(aj.tempTarget != other.gameObject)
            {
                if (aj.tempTarget == pOne.transform.GetChild(2).gameObject)
                {
                    aj.sign.gameObject.SetActive(false);
                }
                else
                {
                    aj.sign.gameObject.SetActive(true);
                }

                aj.regAtk(aj.tempTarget);
            }
        }

        if (other.CompareTag("Interact"))
        {
            GameManager.gm.touchBuilding("");
            other.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        }
    }

    void spawnBot()
    {
        for(int i = 1; i < 6; i++)
        {
            VillainCtrl villainCtrl = FindObjectOfType<VillainCtrl>();
            villainCtrl.SpawnVillain(transform.position, i);
        }
    }

    public void takeDamage(Fusion.NetworkObject sender, float damage, bool trueDmg)
    {
        if (isDie) return;

        pOne.RPC_setDamage(sender, damage, trueDmg);
    }

    public void getDamage(Fusion.NetworkObject sender, float damage, bool trueDmg)
    {
        if (isMine)
        {
            bool revenge = Random.Range(0, 100) < pRevenge;
            float roughness = damage * (1 - Random.Range(0, pRoughness) / 100);

            bool evasion = Random.Range(0, 100) < pEvasion;

            if (evasion)
                return;

            float dmg = trueDmg ? damage : damage - roughness;
            pOne.RPC_Damage(currentHealth - dmg);

            if(sender.gameObject.CompareTag("Villain"))
            {
                aj.regAtk(sender.gameObject);
            }
            else
            {
                aj.regAtk(sender.transform.GetChild(0).gameObject);
            }

            aj.sign.gameObject.SetActive(true);

            if (revenge)
            {
                attack(1);
            }

            isDie = currentHealth <= 0;

            if (isDie)
            {
                CancelInvoke("getFOV");
                CancelInvoke("spawnBot");

                pOne.RPC_Death();
                if (!DataServer.call.isBHMode && !DataServer.call.isTHMode) PlayerPrefs.SetFloat("currentHealth", pHealth);
                GameManager.gm.um.openUI("Death");
            }
        }
        else
        {
            healthBar.gameObject.SetActive(true);
        }
    }

    public void setHealth(float hlt)
    {
        currentHealth = hlt;
        if (isMine)
        {
            GameManager.gm.charHp.fillAmount = 1 - currentHealth / pHealth;
            if (!DataServer.call.isBHMode && !DataServer.call.isTHMode) PlayerPrefs.SetFloat("currentHealth", currentHealth);
        }
        else
        {
            hpBar.fillAmount = currentHealth / pHealth;
        }
    }

    public void treat(int times)
    {
        StartCoroutine(heal(times));
    }

    IEnumerator heal(int times)
    {
        int tms = 0;
        while (tms < times)
        {
            tms++;
            setHealth(currentHealth + pHealth * 2 / 100);
            yield return new WaitForSeconds(0.32f);
        }
    }

    public void revive(bool rvv = false)
    {
        if(rvv) setHealth(pHealth * 20 / 100);
        pOne.RPC_Revive();
        GameManager.gm.um.closeUI("Death");
    }

    public bool areDeath()
    {
        bool death = isDie;
        return death;
    }
}
