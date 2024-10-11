using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskLife : Node
{
    private Animator _animator;

    Villain villain;

    public TaskLife(Transform transform)
    {
        _animator = transform.GetChild(0).GetComponent<Animator>();
        villain = transform.GetComponent<Villain>();
    }

    public override NodeState Evaluate()
    {
        if(villain.isDie)
        {
            _animator.SetBool("Attack", false);
            state = NodeState.SUCCESS;
            return state;
        }

        if (villain.attacked)
        {
            parent.SetData("target", villain.attacker.transform);
            villain.attacked = false;

            if (villain.vType[villain.vIndex] == "Coward")
            {
                _animator.SetFloat("Speed", 0);
            }
            else
            {
                _animator.SetFloat("Speed", 8);
            }
        }

        state = NodeState.FAILURE;
        return state;
    }
}
