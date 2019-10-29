using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AILineOfSightCone : MonoBehaviour
{
    // Inspector Assigned Variables
    [SerializeField] AIGuardStateMachine _guardStateMachine;
    // Public Variables
    public Collider _playerCollider;
    // Private Variables
    private bool _playerWithinCone = false;
    private int _playerLayerMask;
    // Public Properties
    public bool PlayerWithinCone { get { return _playerWithinCone; } set { _playerWithinCone = value; } }

    // Start is called before the first frame update
    void Start()
    {
        // Check if we have a valid state machine
        if (!_guardStateMachine)
        {
            // Find state machine based on root transform
            _guardStateMachine = transform.root.GetComponentInChildren<AIGuardStateMachine>();
        }
        // Set the state machine's head reference to be this
        _guardStateMachine.SightCone = this;
        // Cache player layer mask
        _playerLayerMask = LayerMask.NameToLayer("Squid");

    }

    void OnTriggerEnter(Collider other)
    {
        // Make sure the state machine is valid and that the AI isn't dead
        if (_guardStateMachine && _guardStateMachine.IsAwake)
        {
            // Check if this is the player
            if (!_playerWithinCone && other.gameObject.CompareTag("Player"))
            {
                if (!_playerCollider || (!_playerCollider != other))
                {
                    _playerCollider = other;
                }
                // Player is within cone!
                _playerWithinCone = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Make sure the state machine is valid and that the AI isn't dead
        if (_guardStateMachine && _guardStateMachine.IsAwake)
        {
            // Check if this is the player
            if (!_playerWithinCone && other.gameObject.CompareTag("Player"))
            {
                // Player is within cone!
                _playerWithinCone = false;
            }
        }
    }
}
