using UnityEngine;
using System.Collections;

public class GrabMovableHookInteraction : HookInteractionBase
{
    [SerializeField] float m_attractSpeed = 0;

    bool m_enabled = false;

    GameObject m_hook = null;
    GameObject m_target = null;
    Rigidbody2D m_targetRigidbody;
    BaseInteractable m_targetInteractable;
    
    Vector2 m_oldPos;

    HookBehaviour m_hookBehaviour = null;

    public override bool Attach(GameObject hook, GameObject target)
    {
        if (hook == null || target == null)
            return false;

        m_enabled = true;

        m_hook = hook;
        m_target = target;
        m_targetRigidbody = m_target.GetComponent<Rigidbody2D>();
        m_targetInteractable = m_target.GetComponent<BaseInteractable>();

        Vector2 pos = transform.position;
        Vector2 targetPos = m_target.transform.position;
        m_oldPos = targetPos;

        if (m_targetInteractable != null)
            m_targetInteractable.StartInteract(m_hookBehaviour);

        return true;
    }

    public override void Detach()
    {
        m_enabled = false;

        m_target = null;

        if (m_targetInteractable != null)
            m_targetInteractable.StopInteract();
    }
    
    void Start()
    {
        m_hookBehaviour = GetComponent<HookBehaviour>();
    }
    
    void Update()
    {
        if (!m_enabled)
            return;

        if (m_target == null)
        {
            m_hookBehaviour.BackCurrentHook();
            return;
        }

        if (m_targetRigidbody == null)
            return;

        Vector2 pos = transform.position;
        Vector2 targetPos = m_target.transform.position;
        Vector2 dir = pos - targetPos;
        float distance = dir.magnitude;

        Vector2 normalDir = new Vector2(dir.y, -dir.x);

        var velocity = m_targetRigidbody.velocity;

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
        int result = m_targetRigidbody.Cast((nextPos - m_oldPos) / moveDist, hits, moveDist);
        if (result != 0)
        {
            for (int i = 0; i < result; i++)
            {
                if (hits[i].distance < moveDist)
                    moveDist = hits[i].distance;
            }
            nextPos = m_oldPos + correctedVelocity.normalized * moveDist;
        }

        m_targetRigidbody.position = nextPos;
        m_targetRigidbody.velocity = correctedVelocity;

        m_oldPos = nextPos;
    }
}
