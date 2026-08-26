using UnityEngine;

public class WalkAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float moveThreshold = 0.01f;
    [SerializeField] private float stopDelay = 0.1f;   // grace before counting as "stopped" (kills flicker)
    [SerializeField] private AudioSource walkSource;   // looping footsteps (assign on the player; leave empty on NPCs)

    private Vector3 lastPos;
    private float stillTime;

    void Start()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        Vector3 delta = transform.position - lastPos;
        lastPos = transform.position;

        // Smooth the moving flag: only "stopped" after being still for stopDelay,
        // so physics/frame timing gaps don't flicker the animation or audio
        bool movingNow = delta.magnitude / Time.deltaTime > moveThreshold;
        if (movingNow) stillTime = 0f;
        else           stillTime += Time.deltaTime;
        bool moving = stillTime < stopDelay;

        if (animator != null) animator.SetBool("IsMoving", moving);

        // Face the direction of travel
        if (sprite != null && Mathf.Abs(delta.x) > 0.0001f)
            sprite.flipX = delta.x < 0f;

        // Loop footsteps while moving
        if (walkSource != null)
        {
            if (moving && !walkSource.isPlaying) walkSource.Play();
            else if (!moving && walkSource.isPlaying) walkSource.Pause();
        }
    }
}