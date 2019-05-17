using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;            // The speed that the player will move at.
    public Camera TPCamera;

    Vector3 moveVector;                 // The vector to store the direction of the player's movement.
    Animator anim;                      // Reference to the animator component.
    Rigidbody playerRigidbody;          // Reference to the player's rigidbody.

    public float margin = 0.7f;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        moveVector = PlayerInput.GetMovementInput(TPCamera);

        Animating();
        Vector3 movement = moveVector * speed * Time.deltaTime;
        Move(movement);

        // player jump
        //if (PlayerInput.GetJumpInput() && IsGrounded())
        //{
        //    playerRigidbody.AddForce(Vector3.up * 3000.0f);
        //}

    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, margin);
    }

    void Move(Vector3 movement)
    {
        transform.position += movement;
    }

    void Animating()
    {
        bool walking = moveVector.x != 0 || moveVector.z != 0;
        anim.SetBool("IsWalking", walking);
    }
}
