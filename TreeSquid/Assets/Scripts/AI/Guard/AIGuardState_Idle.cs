using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGuardState_Idle : AIGuardState
{
    // Inspector Assigned Variables
    [SerializeField] Vector2 _idleTimeRange = new Vector2(10.0f, 60.0f);
    [SerializeField] AudioClip idleSound;
    // Private Variables
    float _idleTime = 0.0f;
    float _timer = 0.0f;
    /// <summary>
    /// Returns the type of the state
    /// </summary>
    /// <returns></returns>
    public override AIStateType GetStateType()
    {
        return AIStateType.Idle;
    }
    public override void OnEnterState()
    {
        base.OnEnterState();
        Debug.Log("Entered idle");
        // Make sure the state machine is valid
        if (_guardStateMachine == null)
            return;
        // Set the idle time
        _idleTime = Random.Range(_idleTimeRange.x, _idleTimeRange.y);
        _timer = 0.0f;
        // Configure state machine
        _guardStateMachine.NavAgentControl(true, false);
        _guardStateMachine.speed = 0;
        _guardStateMachine.seeking = 0;
        _guardStateMachine.attackType = 0;
        _guardStateMachine.ClearTarget();
        // Play the idle sound
        //AudioManager.instance.PlayOneShotSound("Enemies", idleSound, _guardStateMachine.headTransform.position, 1.0f, 1.0f);
    }

    /// <summary>
    /// Performs all checks on each update frame
    /// </summary>
    /// <returns></returns>
    public override AIStateType OnUpdate()
    {
        // Make sure there is a valid state machine
        if (_guardStateMachine == null)
            return AIStateType.Idle;

        // Check if the player is visible
        if (_guardStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            // Set the target of the Sword Husk
            _guardStateMachine.SetTarget(_guardStateMachine.VisualThreat);
            // Put Sword Husk into Pursuit State
            return AIStateType.Pursuit;
        }
        // Check if the threat is an audio emitter
        if (_guardStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            // Set the target of the Guard
            _guardStateMachine.SetTarget(_guardStateMachine.AudioThreat);
            // Put SGuard into Alert State
            return AIStateType.Alerted;
        }
        // Update the idle timer
        _timer += Time.deltaTime;
        // Wander if idle time has been exceeded
        if (_timer > _idleTime)
        {
            // Put Guard into Wander State
            return AIStateType.Wander;
        }
        // By default, keep Guard in Idle State
        return AIStateType.Idle;
    }

}
