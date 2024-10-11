using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckEnemyInFOVRange : Node
{
    private static int _enemyLayerMask = LayerMask.NameToLayer("Damagable");

    private Transform _transform;
    private Animator _animator;

    Villain villain;

    public CheckEnemyInFOVRange(Transform transform)
    {
        _transform = transform;
        _animator = transform.GetChild(0).GetComponent<Animator>();

        villain = transform.GetComponent<Villain>();
    }

    public override NodeState Evaluate()
    {
        object t = GetData("target");
        if (t == null)
        {
            villain.FOV.FindVisibleTargets();
            if (villain.FOV.visibleTargets.Count > 0)
            {
                parent.parent.SetData("target", villain.FOV.visibleTargets[0].transform);
                if (villain.vType[villain.vIndex] == "Coward")
                {
                    _animator.SetFloat("Speed", 0);
                }
                else
                {
                    _animator.SetFloat("Speed", 8);
                }

                state = NodeState.SUCCESS;
                return state;
            }

            state = NodeState.FAILURE;
            return state;
        }

        state = NodeState.SUCCESS;
        return state;
    }
}