using UnityEngine;

// Put on a one-shot effect (e.g. a death animation) so it removes itself after it plays.
public class DestroyAfter : MonoBehaviour
{
    [SerializeField] private float seconds = 1f;

    void Start()
    {
        Destroy(gameObject, seconds);
    }
}
