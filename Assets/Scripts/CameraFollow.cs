using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] GameObject m_target = null;
    [SerializeField] float m_speed = 1;
    [SerializeField] float m_speedPow = 1;
    [SerializeField] Rect m_restrictedRect = new Rect(-100, -100, 200, 200);

    Vector2 m_targetPosition;
    Camera m_camera;
    
    void Start()
    {
        if (m_target != null)
            m_targetPosition = m_target.transform.position;
        m_camera = GetComponentInChildren<Camera>();
    }
    
    void Update()
    {
        if (m_target != null)
            m_targetPosition = m_target.transform.position;

        Vector2 pos = transform.position;

        Vector2 dir = m_targetPosition - pos;
        float dist = dir.magnitude;

        if (dist < 0.01f)
            return;

        float move = Mathf.Pow(dist * m_speed, m_speedPow) * Time.deltaTime;
        if (move > dist)
            move = dist;

        Vector2 newPos = pos + dir * move / dist;

        if(m_camera != null)
        {
            Vector2 camSize = new Vector2(m_camera.orthographicSize * m_camera.aspect, m_camera.orthographicSize);

            Rect camRect = new Rect(m_restrictedRect.x + camSize.x, m_restrictedRect.y + camSize.y, m_restrictedRect.width - camSize.x * 2, m_restrictedRect.height - camSize.y * 2);

            if (camRect.width < 0)
                newPos.x = camRect.x + camRect.width / 2;
            else if (newPos.x < camRect.x)
                newPos.x = camRect.x;
            else if (newPos.x > camRect.x + camRect.width)
                newPos.x = camRect.x + camRect.width;

            if (camRect.height < 0)
                newPos.y = camRect.y + camRect.height / 2;
            else if (newPos.y < camRect.y)
                newPos.y = camRect.y;
            else if (newPos.y > camRect.y + camRect.height)
                newPos.y = camRect.y + camRect.height;
        }

        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(m_restrictedRect.x, m_restrictedRect.y), new Vector3(m_restrictedRect.x + m_restrictedRect.width, m_restrictedRect.y));
        Gizmos.DrawLine(new Vector3(m_restrictedRect.x + m_restrictedRect.width, m_restrictedRect.y), new Vector3(m_restrictedRect.x + m_restrictedRect.width, m_restrictedRect.y + m_restrictedRect.height));
        Gizmos.DrawLine(new Vector3(m_restrictedRect.x + m_restrictedRect.width, m_restrictedRect.y + m_restrictedRect.height), new Vector3(m_restrictedRect.x, m_restrictedRect.y + m_restrictedRect.height));
        Gizmos.DrawLine(new Vector3(m_restrictedRect.x, m_restrictedRect.y + m_restrictedRect.height), new Vector3(m_restrictedRect.x, m_restrictedRect.y));
    }
}
