using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class charController : MonoBehaviour
{
    ActionJoystick aj;
    public Player pOne;

    public float walkSpeed = 3f;
    public float runSpeed = 8f;
    public float crouchedSpeed = 2f;
    public float mouseSensitivity = 2f;
    public float jumpHeight = 5;

    public CamCtrl cameraPivot;
    public BorderCtrl bordPivot;

    public float playerSpeed;
    private bool isGrounded = false;
    private Animator animator;
    private CapsuleCollider capsuleCollider;
    private float capsuleHalfHeight;

    public bool isMine;
    public bool secondChar;
    bool actLock;
    public bool inAct;

    public GameObject shadow;

    public void playerSetup()
    {
        animator = GetComponent<Animator>();
        capsuleCollider = transform.GetComponent<CapsuleCollider>();
        capsuleHalfHeight = capsuleCollider.height / 2;

        if (!isMine)
            return;

        cameraPivot = FindObjectOfType<CamCtrl>();
        bordPivot = FindObjectOfType<BorderCtrl>();
        aj = FindObjectOfType<ActionJoystick>();
        pOne = transform.parent.GetComponent<Player>();

        secondChar = GameManager.gm.character[0] != null;

        if (secondChar)
        {
            transform.position = GameManager.gm.character[0].transform.position;
            GameManager.gm.character[1] = this;
        }
        else
        {
            aj._cc = this;
            aj.regAtk(gameObject);
            transform.parent.GetComponent<CharacterController>().enabled = true;
            GameManager.gm.character[0] = this;
            cameraPivot.transform.position = new Vector3(transform.position.x, 0.008f, transform.position.z);
        }
    }

    void FixedUpdate()
    {
        CheckGround();
        //--set the crouched state to a default value of false, unless I am pressing the crouch button 
        
        //--sets Speed, "inAir" and "isCrouched" parameters in the Animator--
        animator.SetFloat("Speed", playerSpeed);
        animator.SetBool("inAir", false);

        if (!isGrounded)
        {
            animator.SetBool("inAir", true);
        }

        if (!isMine || secondChar)
            return;

        if(playerSpeed > walkSpeed)
        {
            if (!animator.GetBool("Gather"))
            {
                animator.SetBool("Gather", true);
            }

            if (animator.GetFloat("Pos") > 0)
            {
                animator.SetFloat("Pos", 0);
            }
        }

        cameraPivot.CamMove(transform.position, playerSpeed);
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

    public void initAnim(int index)
    {
        aj.actAtk(index);
    }

    public void actionAnim(int index)
    {
        animator.SetFloat("Pos", index);
        animator.SetBool("Gather", false);
        animator.SetTrigger(index.ToString());
        if(index <= 1)
        {
            StartCoroutine(endAtk(1));
        }
        else
        {
            StartCoroutine(endAtk(5));
        }
    }

    IEnumerator endAtk(float wait)
    {
        yield return new WaitForSeconds(wait);

        inAct = false;

        animator.SetBool("Gather", true);
        actLock = false;
        aj.regAtk(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isMine || secondChar)
            return;

        if (other.CompareTag("Nature") && !actLock)
        {
            if (other.transform.parent.gameObject.name == "7")
            {
                other.GetComponent<RSSControl>().canceled = false;
            }
            actLock = true;
            aj.regAtk(other.transform.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isMine || secondChar)
            return;

        if (other.CompareTag("Nature") && actLock)
        {
            if(other.transform.parent.gameObject.name == "7")
            {
                other.GetComponent<RSSControl>().canceled = true;
            }
            actLock = false;
            aj.regAtk(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (shadow != null)
        {
            shadow.SetActive(true);
        }
    }
}
