using UnityEngine;
using System.Collections;

public class MoveHookInteraction : HookInteractionBase
{
    [SerializeField] float m_attractSpeed = 0;

    bool m_enabled = false;
    GameObject m_hook = null;
    
    Vector2 m_oldPos;

    Rigidbody2D m_rigidbody = null;
    HookBehaviour m_hookBehaviour = null;

    public override bool Attach(GameObject hook, GameObject target)
    {
        m_enabled = true;
        m_hook = hook;

        Vector2 pos = transform.position;
        Vector2 hookPos = hook.transform.position;
        m_oldPos = pos;

        return true;
    }

    public override void Detach()
    {
        m_enabled = false;
    }
    
    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_hookBehaviour = GetComponent<HookBehaviour>();
    }
    
    void FixedUpdate()
    {
        if (!m_enabled)
            return;

        Vector2 pos = transform.position;
        Vector2 hookPos = m_hook.transform.position;
        Vector2 dir = hookPos - pos;
        float distance = dir.magnitude;

        Vector2 normalDir = new Vector2(dir.y, -dir.x);

        var velocity = m_rigidbody.velocity;

        var correctedVelocity = Utility.Project(velocity, normalDir);

        var attractVelocity = Utility.Project(velocity, dir);
        if (Vector2.Dot(correctedVelocity, normalDir) < 0)
            attractVelocity = Vector2.zero;
        float attractMagnitude = attractVelocity.magnitude;
        if (attractMagnitude < m_attractSpeed)
            attractMagnitude = m_attractSpeed;
        attractVelocity = attractMagnitude * dir / distance;
        correctedVelocity += attractVelocity;

        Vector2 nextPos = m_oldPos + correctedVelocity * Time.deltaTime;
        float moveDist = (nextPos - m_oldPos).magnitude;

        RaycastHit2D[] hits = new RaycastHit2D[16];
        int result = m_rigidbody.Cast((nextPos - m_oldPos) / moveDist, hits, moveDist);
        if(result != 0)
        {
            for(int i = 0; i < result; i++)
            {
                if (hits[i].distance < moveDist)
                    moveDist = hits[i].distance;
            }
            nextPos = m_oldPos + correctedVelocity.normalized * moveDist;
        }

        m_rigidbody.position = nextPos;
        m_rigidbody.velocity = correctedVelocity;

        m_oldPos = nextPos;
    }
}
