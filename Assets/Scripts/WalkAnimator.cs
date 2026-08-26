using UnityEngine;

public class WalkAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float moveThreshold = 0.01f;

    private Vector3 lastPos;

    void Start()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        Vector3 delta = transform.position - lastPos;
        lastPos = transform.position;

        bool moving = delta.magnitude / Time.deltaTime > moveThreshold;
        if (animator != null) animator.SetBool("IsMoving", moving);

        // Face the direction of travel
        if (sprite != null && Mathf.Abs(delta.x) > 0.0001f)
            sprite.flipX = delta.x < 0f;
    }
}