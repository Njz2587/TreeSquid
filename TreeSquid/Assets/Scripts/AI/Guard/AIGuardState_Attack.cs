using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGuardState_Attack : AIGuardState
{
    // Inspector Assigned Variables
    [SerializeField] float _attackTime = 6.8f;
    //[SerializeField] Vector2 _attackTimeRange = new Vector2(0.5f, 10.0f);

    // Private Variables
    float _timer = 0.0f;

    /// <summary>
    /// Returns the type of the state
    /// </summary>
    /// <returns></returns>
    public override AIStateType GetStateType()
    {
        return AIStateType.Attack;
    }

    /// <summary>
    /// Initializes state machine for Attack state
    /// </summary>
    public override void OnEnterState()
    {
        base.OnEnterState();
        // Make sure the state machine is valid
        if (_guardStateMachine == null)
            return;
        // Set the timer
        _timer = 0.0f;
        Debug.Log("Entered attack state");
        // Configure State Machine
        _guardStateMachine.NavAgentControl(false, false);
        _guardStateMachine.seeking = 0;
        _guardStateMachine.speed = 0.0f;
        _guardStateMachine.attackType = 1;

        if (_guardStateMachine.inMeleeRange)
        {
            PlayerVars.instance.ResetToCheckPoint(10);
            Instantiate(_guardStateMachine.caughtSquidEffect, PlayerVars.instance.player.transform.position, Quaternion.Euler(-90, 0, 0));
        }
    }

    /// <summary>
    /// Essentially the engine of the state, performs all checks on each update frame
    /// </summary>
    /// <returns></returns>
    public override AIStateType OnUpdate()
    {
        // Make sure there is a valid state machine
        if (_guardStateMachine == null)
            return AIStateType.Idle;

        // Update the idle timer
        _timer += Time.deltaTime;
        // Wander if idle time has been exceeded
        if (_timer > _attackTime)
        {
            // Clear the player target
            _guardStateMachine.ClearTarget();
            // Set waypoint destination
            _guardStateMachine.navAgent.SetDestination(_guardStateMachine.GetWaypointPosition(false));
            // Resume NavMeshAgent
            _guardStateMachine.navAgent.isStopped = false;
            // Put Guard into Patrol State
            return AIStateType.Patrol;
        }
        // By default, keep Guard in Attack State
        return AIStateType.Attack;
    }
}
