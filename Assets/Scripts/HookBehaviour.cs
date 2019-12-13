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

    Transform m_ropeVisualTransform = null;
    Vector2 m_ropeVisualSize = new Vector2(1, 1);
    Transform m_hookVisualTransform = null;
    Vector2 m_hookVisualSize = new Vector2(1, 1);

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

        m_ropeVisualTransform = transform.Find("Rope");
        m_hookVisualTransform = transform.Find("Hook");

        if(m_ropeVisualTransform != null)
        {
            var render = m_ropeVisualTransform.GetComponentInChildren<SpriteRenderer>();
            if(render != null)
            {
                var rect = render.sprite.rect;
                m_ropeVisualSize = new Vector2(rect.width, rect.height);
                m_ropeVisualSize /= render.sprite.pixelsPerUnit;
            }
        }
        if(m_hookVisualTransform != null)
        {
            var render = m_hookVisualTransform.GetComponentInChildren<SpriteRenderer>();
            if (render != null)
            {
                var rect = render.sprite.rect;
                m_hookVisualSize = new Vector2(rect.width, rect.height);
                m_hookVisualSize /= render.sprite.pixelsPerUnit;
            }
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

        UpdateHookRender();
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

    void UpdateHookRender()
    {
        bool show = m_hookState != HookState.Idle;

        m_hookVisualTransform.gameObject.SetActive(show);
        m_ropeVisualTransform.gameObject.SetActive(show);

        if (!show)
            return;

        Vector2 pos = transform.position;
        Vector2 hookPos = m_hookObject.transform.position;

        m_hookVisualTransform.position = new Vector3(hookPos.x, hookPos.y, m_hookVisualTransform.position.z);

        Vector2 dir = hookPos - pos;

        float angle = Utility.Angle(dir);
        m_hookVisualTransform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle - 90);

        float dist = dir.magnitude;
        if (dist > 0.01f)
        {
            float ropeDist = dist - m_hookVisualSize.y / 2.0f;
            Vector2 ropeTarget = pos + dir / dist * ropeDist;
            Vector2 ropePos = (ropeTarget + pos) / 2;

            m_ropeVisualTransform.position = new Vector3(ropePos.x, ropePos.y, m_ropeVisualTransform.position.z);
            m_ropeVisualTransform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle - 90);
            m_ropeVisualTransform.localScale = new Vector3(1, ropeDist / m_ropeVisualSize.y, 1);
        }
        else m_ropeVisualTransform.gameObject.SetActive(false);
    }
}
