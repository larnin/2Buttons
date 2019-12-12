using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ExplosiveInteractable : BaseInteractable
{
    [SerializeField] float m_interactionExplosionDelay = 2;
    [SerializeField] float m_hitExplosionDelay = 1;
    [SerializeField] float m_minHitSpeedToExplode = 1;
    [SerializeField] float m_rapidFlashDelay = 0.5f;
    [SerializeField] GameObject m_explosionPrefab = null;

    float m_timer = 0;
    bool m_hit = false;

    Animator m_animator = null;

    public override void StartInteract(HookBehaviour interactor)
    {
        if (m_hit)
            return;

        m_timer = m_interactionExplosionDelay;
        m_hit = true;
    }

    public override void StopInteract()
    {
        //nothing
    }

    void Start()
    {
        m_animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        if (!m_hit)
            return;

        m_timer -= Time.deltaTime;

        if(m_animator != null)
        {
            if (m_timer < m_rapidFlashDelay)
                m_animator.SetTrigger("SlowPulse");
            else m_animator.SetTrigger("FastPulse");
        }

        if (m_timer <= 0)
        {
            OnExplode();
            m_hit = false;
        }
    }

    public void OnHit()
    {
        if (m_hit)
            return;

        m_timer = m_hitExplosionDelay;
        m_hit = true;
    }

    void OnExplode()
    {
        if(m_explosionPrefab != null)
        {
            var obj = Instantiate(m_explosionPrefab);
            obj.transform.position = transform.position;
        }

        DOVirtual.DelayedCall(0.1f, () => 
        {
            if (gameObject != null)
                Destroy(gameObject);
        });
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float speed = collision.relativeVelocity.magnitude;

        if (speed > m_minHitSpeedToExplode)
            OnHit();
    }
}
