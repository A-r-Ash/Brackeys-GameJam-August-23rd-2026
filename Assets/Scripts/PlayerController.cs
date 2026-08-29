using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Keyboard (WASD / arrows)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        moveInput = moveInput.normalized; // stops diagonal moving faster
    }

    void FixedUpdate()
    {
        // Physics movement belongs in FixedUpdate
        rb.linearVelocity = moveInput * moveSpeed;
    }
}