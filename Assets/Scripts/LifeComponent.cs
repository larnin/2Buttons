using UnityEngine;
using System.Collections;

public class LifeComponent : MonoBehaviour
{
    [SerializeField] bool m_isPlayer = false;
    [SerializeField] GameObject m_deathPrefab = null;
    [SerializeField] float m_destroyAfterDeathDelay = -1;

    bool m_killed = false;

    public void Hit()
    {
        if (m_killed)
            return;

        m_killed = true;

        if (m_isPlayer)
            Event<PlayerDeathEvent>.Broadcast(new PlayerDeathEvent());

        if(m_deathPrefab != null)
        {
            var obj = Instantiate(m_deathPrefab);
            obj.transform.position = transform.position;
        }

        if (m_destroyAfterDeathDelay >= 0)
            Destroy(gameObject, m_destroyAfterDeathDelay);
    }
}
