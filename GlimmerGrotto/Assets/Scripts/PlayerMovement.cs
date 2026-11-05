using UnityEngine;
using UnityEngine.InputSystem;   // InputValue lives in the Input System namespace

// PlayerMovement: Fatih's controls. Runs left and right, turns to face the way he
// runs, tells the Animator when he runs, and jumps, but only from the ground.
// Needs a Player Input component on the same GameObject (Behavior: Send Messages),
// which calls OnMove and OnJump when the Move and Jump actions happen.
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float runSpeed = 10f;   // units per second at full input
    [SerializeField] float jumpSpeed = 25f;  // upward speed at take-off

    // The Animator finds parameters by number. We work out the number for
    // "isRunning" once, so the name is typed in exactly one place.
    static readonly int IsRunningHash = Animator.StringToHash("isRunning");

    Vector2 moveInput;               // the latest value of the Move action
    Rigidbody2D body;                // moves Fatih; gravity acts on it
    Animator animator;               // plays Idling or Running
    CapsuleCollider2D bodyCollider;  // Fatih's collider: what is he touching?
    int groundMask;                  // the Ground layer as a layer mask (one bit per layer)

    void Start()
    {
        body = GetComponent<Rigidbody2D>();         // look it up once and keep it
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        groundMask = LayerMask.GetMask("Ground");   // look the layer up once
    }

    void Update()
    {
        Run();    // every frame
        FlipSprite();
    }

    // Player Input sends this message when the Move value changes,
    // and once more with (0, 0) when the player lets go.
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        // Debug.Log(moveInput);   // Part 1: remove the // to print the input to the Console
    }

    // Player Input sends this message when the Jump button is pressed.
    void OnJump(InputValue value)
    {
        // Not touching the Ground layer? Then leave the method: no jump.
        if (!bodyCollider.IsTouchingLayers(groundMask)) return;

        if (value.isPressed)
        {
            // Set the vertical speed to jumpSpeed; keep the horizontal speed.
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpSpeed);
        }
    }

    void Run()
    {
        // The input decides the horizontal speed; the vertical speed stays
        // whatever physics (gravity, a jump, a landing) has made it.
        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed,
                                             body.linearVelocity.y);
        body.linearVelocity = playerVelocity;

        animator.SetBool(IsRunningHash, IsMovingHorizontally());
    }

    void FlipSprite()
    {
        // Turn only while moving, so Fatih keeps facing the way he last ran.
        if (IsMovingHorizontally())
        {
            // Scale x = 1 faces right, -1 faces left. Mathf.Sign gives 1 or -1.
            float direction = Mathf.Sign(body.linearVelocity.x);
            transform.localScale = new Vector3(direction, 1f, 1f);
        }
    }

    // One test, used by Run and FlipSprite. Mathf.Epsilon is the smallest
    // positive float, so this simply means "the x velocity is not 0".
    bool IsMovingHorizontally()
    {
        return Mathf.Abs(body.linearVelocity.x) > Mathf.Epsilon;
    }
}
