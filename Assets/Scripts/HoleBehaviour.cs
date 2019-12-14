using UnityEngine;
using System.Collections;

public class HoleBehaviour : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var life = collision.GetComponent<LifeComponent>();
        if (life != null)
            life.Hit();
    }
}
