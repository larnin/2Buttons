using UnityEngine;
using System.Collections;

public class RotationControler : MonoBehaviour
{
    string leftButton = "A";
    string rightButton = "B";

    [SerializeField] float m_initialCursorAngle = 0.0f;
    [SerializeField] float m_rotationAcceleration = 20.0f;
    [SerializeField] float m_rotationMaxSpeed = 5.0f;
    [SerializeField] GameObject m_cursorObject = null;
    [SerializeField] float m_cursorDistance = 10.0f;

    float m_speed = 0;
    float m_angle = 0;
    bool m_haveThrowHookLastFrame = false;

    void Start()
    {
        m_angle = m_initialCursorAngle;
        if (m_cursorObject != null)
        {
            m_cursorObject.transform.parent = transform;
            m_cursorObject.transform.position = Vector3.zero;
        }
    }
    
    void Update()
    {
        bool moveLeft = Input.GetButton(leftButton);
        bool moveRight = Input.GetButton(rightButton);

        float acceleration = m_rotationAcceleration * Time.deltaTime;

        if(moveRight)
            m_speed -= acceleration;
        if (moveLeft)
            m_speed += acceleration;

        if(!moveRight && !moveLeft && m_speed != 0)
        {
            float newSpeed = m_speed - Mathf.Sign(m_speed) * acceleration;
            if (Mathf.Sign(m_speed) != Mathf.Sign(newSpeed))
                newSpeed = 0;
            m_speed = newSpeed;
        }

        if (moveLeft && moveRight)
            m_speed = 0;

        if (m_speed < -m_rotationMaxSpeed)
            m_speed = -m_rotationMaxSpeed;
        if (m_speed > m_rotationMaxSpeed)
            m_speed = m_rotationMaxSpeed;

        m_angle += m_speed * Time.deltaTime;

        if (m_angle < 0)
            m_angle += 360.0f;
        if (m_angle > 360.0f)
            m_angle -= 360.0f;

        bool throwHook = moveLeft && moveRight;

        if(throwHook != m_haveThrowHookLastFrame)
        {
            m_haveThrowHookLastFrame = throwHook;

            Event<ThrowHookEvent>.Broadcast(new ThrowHookEvent(throwHook, m_angle));
        }

        UpdateCursor();
    }

    void UpdateCursor()
    {
        if (m_cursorObject == null)
            return;

        float radAngle = Mathf.Deg2Rad * m_angle;

        Vector2 pos = new Vector2(Mathf.Cos(radAngle), Mathf.Sin(radAngle)) * m_cursorDistance;

        m_cursorObject.transform.localPosition = pos;
    }
}
