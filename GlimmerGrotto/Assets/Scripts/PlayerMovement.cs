using UnityEngine;
using UnityEngine.InputSystem;   // InputValue lives in the Input System namespace

// PlayerMovement: Fatih's controls. Runs left and right, turns to face the way he
// runs, jumps, but only from the ground, and climbs ladders. Tells the Animator
// when he runs and when he climbs.
// Needs a Player Input component on the same GameObject (Behavior: Send Messages),
// which calls OnMove and OnJump when the Move and Jump actions happen.
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float runSpeed = 10f;   // units per second at full input
    [SerializeField] float jumpSpeed = 25f;  // upward speed at take-off
    [SerializeField] float climbSpeed = 5f;  // units per second on a ladder

    // The Animator finds parameters by number. We work out the numbers for
    // "isRunning" and "isClimbing" once, so each name is typed in one place.
    static readonly int IsRunningHash = Animator.StringToHash("isRunning");
    static readonly int IsClimbingHash = Animator.StringToHash("isClimbing");

    Vector2 moveInput;               // the latest value of the Move action
    Rigidbody2D body;                // moves Fatih; gravity acts on it
    Animator animator;               // plays Idling, Running or Climbing
    CapsuleCollider2D bodyCollider;  // Fatih's collider: what is he touching?
    int groundMask;                  // the Ground layer as a layer mask (one bit per layer)
    int climbingMask;                // the Climbing layer as a layer mask
    float gravityScaleAtStart;       // Fatih's own Gravity Scale, from the Inspector

    void Start()
    {
        body = GetComponent<Rigidbody2D>();         // look it up once and keep it
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        groundMask = LayerMask.GetMask("Ground");   // look the layer up once
        climbingMask = LayerMask.GetMask("Climbing");
        gravityScaleAtStart = body.gravityScale;    // before any ladder changes it
    }

    void Update()
    {
        Run();    // every frame
        FlipSprite();
        ClimbLadder();
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

    void ClimbLadder()
    {
        // Not touching anything on the Climbing layer? Then this is not a ladder:
        // give Fatih his normal gravity back and leave the method.
        if (!bodyCollider.IsTouchingLayers(climbingMask))
        {
            body.gravityScale = gravityScaleAtStart;
            animator.SetBool(IsClimbingHash, false);   // off the ladder: not climbing
            return;
        }

        // On a ladder, the y of the input decides the vertical speed. The
        // horizontal speed stays whatever Run has just set.
        Vector2 climbVelocity = new Vector2(body.linearVelocity.x,
                                            moveInput.y * climbSpeed);
        body.linearVelocity = climbVelocity;
        body.gravityScale = 0f;   // no gravity on a ladder, so Fatih stays put

        // The climbing animation plays only while Fatih moves up or down.
        animator.SetBool(IsClimbingHash, IsMovingVertically());
    }

    // One test, used by Run and FlipSprite. Mathf.Epsilon is the smallest
    // positive float, so this simply means "the x velocity is not 0".
    bool IsMovingHorizontally()
    {
        return Mathf.Abs(body.linearVelocity.x) > Mathf.Epsilon;
    }

    // The same test for the y velocity, used by ClimbLadder.
    bool IsMovingVertically()
    {
        return Mathf.Abs(body.linearVelocity.y) > Mathf.Epsilon;
    }
}
