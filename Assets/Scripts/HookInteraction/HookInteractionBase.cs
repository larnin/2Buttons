using UnityEngine;
using System.Collections;

public abstract class HookInteractionBase : MonoBehaviour
{
    public abstract bool Attach(GameObject hook, GameObject target);

    public abstract void Detach();
}
