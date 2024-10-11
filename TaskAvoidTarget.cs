using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskAvoidTarget : Node
{
    private Transform _transform;
    private Animator _animator;
    private Vector3[] _waypoints;

    private int _lastWaypointIndex = 0;
    private int _currentWaypointIndex = 0;

    Villain villain;

    public TaskAvoidTarget(Transform transform, Vector3[] waypoints)
    {
        _transform = transform;
        _animator = transform.GetChild(0).GetComponent<Animator>();
        _waypoints = waypoints;

        villain = transform.GetComponent<Villain>();
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        if(_lastWaypointIndex == _currentWaypointIndex)
        {
            _currentWaypointIndex = Random.Range(0, _waypoints.Length);
        }

        if (target != null)
        {
            if ((villain.vType[villain.vIndex] == "Coward") || (villain.vilainCurrentHealth <= villain.vilainCurrentHealth * 10 / 100))
            {
                if (villain.repost)
                {
                    Vector3 direction = (_waypoints[_currentWaypointIndex] - _waypoints[_lastWaypointIndex]).normalized * 8;
                    _waypoints[_currentWaypointIndex] = villain.transform.position - direction;

                    villain.repost = false;
                }

                _animator.SetFloat("Speed", 8);
                Vector3 wp = _waypoints[_currentWaypointIndex];
                if (Vector3.Distance(_transform.position, wp) < 0.01f)
                {
                    _lastWaypointIndex = _currentWaypointIndex;
                }
                else
                {
                    _transform.position = Vector3.MoveTowards(_transform.position, wp, villain.runSpeed * Time.deltaTime);
                    _transform.LookAt(wp);
                }

                state = NodeState.RUNNING;
                return state;
            }

            if (Vector3.Distance(_transform.position, target.position) > villain.fovRange * 4)
            {
                villain.runAway = true;
                _lastWaypointIndex = _currentWaypointIndex;
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