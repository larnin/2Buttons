using UnityEngine;
using System.Collections;

public class GrabMovableHookInteraction : HookInteractionBase
{
    [SerializeField] float m_attractSpeed = 0;

    public override bool Attach(GameObject hook, GameObject target)
    {
        throw new System.NotImplementedException();
    }

    public override void Detach()
    {
        throw new System.NotImplementedException();
    }
    
    void Start()
    {

    }
    
    void Update()
    {

    }
}
