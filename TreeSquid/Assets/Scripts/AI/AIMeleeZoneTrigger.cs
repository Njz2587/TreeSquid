using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMeleeZoneTrigger : MonoBehaviour
{
    public AIGuardStateMachine currentOverlappingGuard;

    
    private void Update()
    {
        if (currentOverlappingGuard)
        {
            if (currentOverlappingGuard.IsAwake)
            {
                if (!currentOverlappingGuard.inMeleeRange)
                {
                    currentOverlappingGuard.inMeleeRange = true;
                }
            }
            else
            {
                currentOverlappingGuard = null;
            }

        }
    }
    
    void OnTriggerEnter(Collider col)
    {
        // Get the AIStateMachine
        AIStateMachine _stateMachine = AIManager.Instance.GetAIStateMachine(col.GetInstanceID());
        //AIStateMachine _stateMachine = col.gameObject.GetComponent<AIStateMachine>();
        // Make sure the state machine is valid
        if (_stateMachine)
        {
            // Try to cast it to an AI Guard State Machine
            AIGuardStateMachine _guardStateMachine;
            if (_stateMachine.GetType() == typeof(AIGuardStateMachine))
            {
                _guardStateMachine = (AIGuardStateMachine)_stateMachine;
                currentOverlappingGuard = _guardStateMachine;
                if (_guardStateMachine.IsAwake && _guardStateMachine.targetType == AITargetType.Visual_Player)
                {
                    _guardStateMachine.inMeleeRange = true;
                    //_guardStateMachine.SetStateOverride(AIStateType.Attack);
                    //PlayerVars.instance.ResetToCheckPoint(10);
                }
            }
        }

    }

    void OnTriggerExit(Collider col)
    {
        // Get the AIStateMachine
        //AIStateMachine _stateMachine = col.gameObject.GetComponent<AIStateMachine>();
        AIStateMachine _stateMachine = AIManager.Instance.GetAIStateMachine(col.GetInstanceID());
        // Make sure the state machine is valid
        if (_stateMachine)
        {
            // Try to cast it to an AI Guard State Machine
            AIGuardStateMachine _guardStateMachine;
            currentOverlappingGuard = null;
            if (_stateMachine.GetType() == typeof(AIGuardStateMachine))
            {
                _guardStateMachine = (AIGuardStateMachine)_stateMachine;
                if (_guardStateMachine.IsAwake)
                {
                    _guardStateMachine.inMeleeRange = false;
                }
            }
        }

    }
}
