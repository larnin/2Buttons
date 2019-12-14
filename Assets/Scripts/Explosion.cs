using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
    [SerializeField] float m_size = 1;
    [SerializeField] float m_power = 1;
    [SerializeField] float m_duration = 1;
    [SerializeField] float m_explosionDuration = 0.5f;

    float m_timer = 0;
    
    void Update()
    {
        m_timer += Time.deltaTime;

        if (m_timer < m_explosionDuration)
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, m_size);

            Vector2 pos = transform.position;

            foreach (var c in colliders)
            {
                if (c.gameObject == gameObject)
                    continue;

                var rigidbody = c.GetComponent<Rigidbody2D>();
                if (rigidbody == null)
                    return;

                Vector2 targetPos = c.transform.position;

                var dir = targetPos - pos;
                float dist = dir.magnitude;
                dir /= dist;
                var orthoDir = new Vector2(dir.y, -dir.x);

                float power = dist / m_size * m_power;

                var velocity = rigidbody.velocity;

                var dirVelocity = Utility.Project(velocity, dir);
                var orthroVelocity = Utility.Project(velocity, orthoDir);

                float distDirVelocity = dirVelocity.magnitude;
                if (distDirVelocity < power)
                    dirVelocity = dir * power;

                velocity = dirVelocity + orthroVelocity;

                rigidbody.velocity = velocity;

                var explosive = c.GetComponent<ExplosiveInteractable>();
                if (explosive != null)
                    explosive.OnHit();
            }
        }

        if (m_timer >= m_duration)
            Destroy(gameObject);
    }
}
