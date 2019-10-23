using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardHeadControl : MonoBehaviour
{
    // Inspector Assigned Variables
    [SerializeField] AIGuardStateMachine _guardStateMachine;
    // Private Variables
    private int _playerLayerMask;
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
        _guardStateMachine.HeadControl = this;
        // Cache player layer mask
        _playerLayerMask = LayerMask.NameToLayer("Squid");

    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if there is a valid state machine
        if (_guardStateMachine)
        {
            // Check if this object we're colliding with is the player
            if (collision.collider.gameObject.layer == _playerLayerMask)
            {
                // Check if the guard is awake
                if (_guardStateMachine.IsAwake)
                {
                    // Check knock out force
                    _guardStateMachine.CheckKnockOutForce(collision.impulse / Time.fixedDeltaTime);
                }
            }
        }
    }
}
