using UnityEngine;

// A pre-placed trap. Lure a dinosaur into it and it springs.
public class Trap : MonoBehaviour
{
    [SerializeField] private bool oneUse = true;
    [SerializeField] private GameObject sprungVisual;   // optional: triggered-trap sprite to show
    [SerializeField] private Animator animator;          // plays the trap closing animation
    [SerializeField] private string snapTrigger = "Spring";

    public void Spring()
    {
        if (!oneUse) return;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;           // can't catch again

        if (sprungVisual != null) sprungVisual.SetActive(true);
        if (animator != null) animator.SetTrigger(snapTrigger);

        SoundManager.Instance?.TrapSnap(transform.position);
    }
}
