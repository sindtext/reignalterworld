using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskGoToTarget : Node
{
    private Transform _transform;
    private Animator _animator;

    Villain villain;

    public TaskGoToTarget(Transform transform)
    {
        _transform = transform;
        _animator = transform.GetChild(0).GetComponent<Animator>();

        villain = transform.GetComponent<Villain>();
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");
        if(target != null)
        {
            if (Vector3.Distance(_transform.position, target.position) > 0.01f)
            {
                _animator.SetBool("Attack", false);
                _transform.position = Vector3.MoveTowards(
                    _transform.position, target.position, villain.runSpeed * Time.deltaTime);
                _transform.LookAt(target.position);
            }

            if (Vector3.Distance(_transform.position, target.position) > villain.fovRange)
            {
                villain.healthBar.gameObject.SetActive(false);
                _animator.SetFloat("Speed", 4);
                ClearData("target");
            }

            state = NodeState.RUNNING;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}