using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AIManager : MonoBehaviour, IGameManager
{
    // Inherited Members
    public ManagerStatus status { get; private set; }
    // Singleton
    public static AIManager Instance;
    public GameObject GuardObject;
    // Private variables
    private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>();

    void Awake()
    {
        if (!AIManager.Instance)
        {
            Debug.Log("Setting Singleton");
            Instance = this;
            GuardObject.SetActive(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Startup()
    {
        Debug.Log("AI manager starting...");
        status = ManagerStatus.Started;

    }

    /// <summary>
    /// Retrieves a registered AI state machine based on a key value
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public AIStateMachine GetAIStateMachine(int key)
    {
        AIStateMachine machine = null;
        if (_stateMachines.TryGetValue(key, out machine))
        {
            return machine;
        }

        return null;
    }

    // --------------------------------------------------------------------
    // Name	:	RegisterAIStateMachine
    // Desc	:	Stores the passed state machine in the dictionary with
    //			the supplied key
    // --------------------------------------------------------------------
    public void RegisterAIStateMachine(int key, AIStateMachine stateMachine)
    {
        if (!_stateMachines.ContainsKey(key))
        {
            _stateMachines[key] = stateMachine;
        }
    }
}
