using UnityEngine;
using System.Collections;

public abstract class BaseInteractable : MonoBehaviour
{
    public abstract void StartInteract(HookBehaviour interactor);

    public abstract void StopInteract();
}
