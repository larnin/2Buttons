using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class HookBehaviour : MonoBehaviour
{
    [Serializable]
    class HookInteractionInfo
    {
        public LayerMask mask;
        public HookInteractionBase hookInteraction = null;
    }

    enum HookState
    {
        Idle,
        Throwing,
        Attached,
        ComingBack,
    }

    [SerializeField] float m_maxDistance = 10.0f;
    [SerializeField] float m_hookSpeed = 10.0f;
    [SerializeField] List<HookInteractionInfo> m_hookInteractions = new List<HookInteractionInfo>();
    [SerializeField] LayerMask m_hookInteractionMask;

    SubscriberList m_subscriberList = new SubscriberList();

    HookState m_hookState = HookState.Idle;
    float m_angle;
    float m_totalDistance = 0.0f;

    bool m_needNextThrow = false;
    float m_nextThrowAngle = 0.0f;
    int m_hookInteractionIndex = 0;

    GameObject m_hookObject = null;
    GameObject m_hookGrabPoint = null;
    GameObject m_grabObject = null;

    private void Awake()
    {
        m_subscriberList.Add(new Event<ThrowHookEvent>.Subscriber(OnHookThrow));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();

        if (m_hookObject != null)
            Destroy(m_hookObject);
    }
    
    void Start()
    {
        if(m_hookObject == null)
        {
            m_hookObject = new GameObject("Hook Head");
        }
    }
    
    void Update()
    {
        switch(m_hookState)
        {
            case HookState.Idle:
                if (m_needNextThrow)
                    StartNextHook();
                break;
            case HookState.Throwing:
                UpdateThrowing();
                break;
            case HookState.Attached:
                UpdateAttached();
                break;
            case HookState.ComingBack:
                UpdateComingBack();
                break;
        }

        DebugDrawHook();
    }

    void OnHookThrow(ThrowHookEvent e)
    {
        if (e.pressed)
        {
            m_needNextThrow = true;
            m_nextThrowAngle = e.angle;
            StartNextHook();
        }
        else BackCurrentHook();
    }

    void StartNextHook()
    {
        if (!m_needNextThrow)
            return;

        if (m_hookState != HookState.Idle)
            return;

        m_hookState = HookState.Throwing;

        float radAngle = Mathf.Deg2Rad * m_nextThrowAngle;
        m_angle = m_nextThrowAngle;
        m_totalDistance = 0.0f;
        m_hookObject.transform.position = transform.position;
        m_needNextThrow = false;
    }

    public void BackCurrentHook()
    {
        if (m_hookState == HookState.Idle || m_hookState == HookState.ComingBack)
            return;

        if(m_hookInteractionIndex >= 0)
            m_hookInteractions[m_hookInteractionIndex].hookInteraction.Detach();

        m_hookState = HookState.ComingBack;
        if(m_hookGrabPoint != null)
            Destroy(m_hookGrabPoint);
    }

    void UpdateThrowing()
    {
        float dist = m_hookSpeed * Time.deltaTime;
        m_totalDistance += dist;

        Vector3 pos = m_hookObject.transform.position;
        Vector3 oldPos = pos;
        float radAngle = Mathf.Deg2Rad * m_angle;
        pos += new Vector3(Mathf.Cos(radAngle), Mathf.Sin(radAngle), 0) * dist;

        float rayDistance = (pos - oldPos).magnitude;

        var hit = Physics2D.Raycast(oldPos, (pos - oldPos) / rayDistance, rayDistance, m_hookInteractionMask.value);
        if (hit.transform != null)
        {
            m_grabObject = hit.transform.gameObject;
            m_hookState = HookState.Attached;
            pos = hit.point;

            if (m_hookGrabPoint == null)
                m_hookGrabPoint = new GameObject("Hook grab point");
            m_hookGrabPoint.transform.parent = hit.transform;
            m_hookGrabPoint.transform.position = hit.point;

            AttachHookToTarget();
        }
        else if (m_totalDistance >= m_maxDistance)
            BackCurrentHook();

        m_hookObject.transform.position = pos;
    }

    void UpdateAttached()
    {
        if (m_hookGrabPoint != null)
        {
            m_hookObject.transform.position = m_hookGrabPoint.transform.position;
            m_hookObject.transform.rotation = m_hookGrabPoint.transform.rotation;
        }


        if (m_grabObject == null)
        {
            BackCurrentHook();
            return;
        }
    }

    void UpdateComingBack()
    {
        Vector3 hookPos = m_hookObject.transform.position;
        Vector3 pos = transform.position;
        Vector3 dir = pos - hookPos;
        float dist = dir.magnitude;
        float speed = m_hookSpeed * Time.deltaTime;

        if (speed >= dist)
            m_hookState = HookState.Idle;

        dir *= speed / dist;

        hookPos += dir;
        m_hookObject.transform.position = hookPos;
    }

    void DebugDrawHook()
    {
        if(m_hookState != HookState.Idle)
        {
            Debug.DrawLine(transform.position, m_hookObject.transform.position, Color.red);
        }
    }

    void AttachHookToTarget()
    {
        if (m_grabObject == null)
            return;

        m_hookInteractionIndex = -1;

        for (int i = 0; i < m_hookInteractions.Count; i++)
        {
            if((m_hookInteractions[i].mask.value & (1 << m_grabObject.layer)) != 0)
            {
                m_hookInteractionIndex = i;

                break;
            }
        }

        if (m_hookInteractionIndex < 0)
            return;

        m_hookInteractions[m_hookInteractionIndex].hookInteraction.Attach(m_hookObject, m_grabObject);
    }
}
