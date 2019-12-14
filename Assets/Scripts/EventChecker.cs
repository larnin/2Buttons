using UnityEngine;
using System.Collections;
using DG.Tweening;

public class EventChecker : MonoBehaviour
{
    [SerializeField] float m_deathFadeDelay = 2;
    [SerializeField] string m_sceneAfterDeath = "";

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<PlayerDeathEvent>.Subscriber(OnDeath));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnDeath(PlayerDeathEvent e)
    {
        DOVirtual.DelayedCall(m_deathFadeDelay, () =>
        {
            if (this == null)
                return;

            SceneSystem.changeScene(m_sceneAfterDeath);
        });
    }
}
