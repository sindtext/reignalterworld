using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskPatrol : Node
{
    private Transform _transform;
    private Animator _animator;
    private Vector3[] _waypoints;

    private int _currentWaypointIndex = 0;
    private int _lastWaypointIndex = 0;

    private float _waitTime = 4f; // in seconds
    private float _waitCounter = 0f;
    private bool _waiting = false;

    Villain villain;

    public TaskPatrol(Transform transform, Vector3[] waypoints)
    {
        _transform = transform;
        _animator = transform.GetChild(0).GetComponent<Animator>();
        _waypoints = waypoints;
        _currentWaypointIndex = Random.Range(0, _waypoints.Length);
        _animator.SetFloat("Speed", 4);

        villain = transform.GetComponent<Villain>();
    }

    public override NodeState Evaluate()
    {
        if(villain.runAway)
        {
            _currentWaypointIndex = Random.Range(0, _waypoints.Length);
            villain.runAway = false;
        }

        if (_waiting)
        {
            _waitCounter += Time.deltaTime;
            if (_waitCounter >= _waitTime)
            {
                _lastWaypointIndex = _currentWaypointIndex;
                _currentWaypointIndex = Random.Range(0, _waypoints.Length);
                _waiting = false;
                _animator.SetFloat("Speed", 4);
            }
        }
        else
        {
            if (villain.repost)
            {
                Vector3 direction = (_waypoints[_currentWaypointIndex] - _waypoints[_lastWaypointIndex]).normalized * 8;
                _waypoints[_currentWaypointIndex] = villain.transform.position - direction;

                villain.repost = false;
            }

            Vector3 wp = _waypoints[_currentWaypointIndex];
            if (Vector3.Distance(_transform.position, wp) < 0.01f)
            {
                _transform.position = wp;
                _waitCounter = 0f;
                _waiting = true;

                _animator.SetFloat("Speed", 0);
                _waitTime = Random.Range(2, 8);
            }
            else
            {
                _transform.position = Vector3.MoveTowards(_transform.position, wp, villain.walkSpeed * Time.deltaTime);
                _transform.LookAt(wp);
            }
        }

        state = NodeState.RUNNING;
        return state;
    }

}