using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISensor : MonoBehaviour
{
    // Private Variables
    private AIStateMachine _parentStateMachine = null;
    public AIStateMachine parentStateMachine { set { _parentStateMachine = value; } }

    void OnTriggerEnter(Collider col)
    {
        if (_parentStateMachine != null)
            if (col.gameObject.CompareTag("AI Sound Emitter") == true)
            {
                Debug.Log("AI Sound Emitter detected!");
            }
            _parentStateMachine.OnTriggerEvent(AITriggerEventType.Enter, col);
    }

    void OnTriggerStay(Collider col)
    {
        if (_parentStateMachine != null)
            _parentStateMachine.OnTriggerEvent(AITriggerEventType.Stay, col);
    }

    void OnTriggerExit(Collider col)
    {
        if (_parentStateMachine != null)
            _parentStateMachine.OnTriggerEvent(AITriggerEventType.Exit, col);
    }

}
