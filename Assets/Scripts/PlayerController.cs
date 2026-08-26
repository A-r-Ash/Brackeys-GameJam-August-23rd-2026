using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private VirtualJoystick joystick;   // optional on-screen stick (mobile)

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

        // On-screen joystick overrides when in use
        if (joystick != null && joystick.Direction != Vector2.zero)
            moveInput = joystick.Direction;

        moveInput = moveInput.normalized; // stops diagonal moving faster
    }

    void FixedUpdate()
    {
        // Physics movement belongs in FixedUpdate
        rb.linearVelocity = moveInput * moveSpeed;
    }
}