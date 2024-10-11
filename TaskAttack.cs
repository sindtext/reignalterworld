using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskAttack : Node
{
    private Animator _animator;

    private Transform _lastTarget;
    private charController _cc;

    Villain villain;

    private float _attackTime = 1f;
    private float _attackCounter = 0f;

    public TaskAttack(Transform transform)
    {
        _animator = transform.GetChild(0).GetComponent<Animator>();
        villain = transform.GetComponent<Villain>();
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");
        if (target !=  null && target != _lastTarget)
        {
            _cc = target.GetComponent<charController>();
            _lastTarget = target;
        }

        _attackCounter += Time.deltaTime;
        if (_attackCounter >= _attackTime)
        {
            bool enemyIsDead = _cc.isDie;
            if (enemyIsDead)
            {
                ClearData("target");
                _animator.SetBool("Attack", false);
                _animator.SetFloat("Speed", 4);
            }
            else
            {
                villain.vAttack(_cc);
                _attackCounter = 0f;
            }
        }

        state = NodeState.RUNNING;
        return state;
    }

}