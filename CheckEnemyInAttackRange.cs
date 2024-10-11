using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckEnemyInAttackRange : Node
{
    private Transform _transform;
    private Animator _animator;

    Villain villain;

    public CheckEnemyInAttackRange(Transform transform)
    {
        _transform = transform;
        _animator = transform.GetChild(0).GetComponent<Animator>();

        villain = transform.GetComponent<Villain>();
    }

    public override NodeState Evaluate()
    {
        object t = GetData("target");
        if (t != null)
        {
            Transform target = (Transform)t;

            if (target == null)
            {
                _animator.SetFloat("Speed", 4);
                ClearData("target");
            }
            else if (Vector3.Distance(_transform.position, target.position) <= villain.attackRange)
            {
                _transform.LookAt(target.position);
                _animator.SetBool("Attack", true);

                state = NodeState.SUCCESS;
                return state;
            }
            else if (Vector3.Distance(_transform.position, target.position) > villain.fovRange)
            {
                _animator.SetFloat("Speed", 4);
                ClearData("target");
            }

            state = NodeState.FAILURE;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }

}