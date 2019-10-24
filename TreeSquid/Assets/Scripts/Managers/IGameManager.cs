using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ManagerStatus { Shutdown, Initializing, Started }
public interface IGameManager
{
    ManagerStatus status { get; }
    void Startup();
}
