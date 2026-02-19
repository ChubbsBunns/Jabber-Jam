using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public Rigidbody2D rb;
    float moveHorizontal, moveVertival;
    float moveSpeed = 10f;
    Vector2 movement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!IsOwner)
        {
            return; 
        }
        Move();
    }
    public void Move()
    {
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertival = Input.GetAxis("Vertical");
        movement = new Vector2(moveHorizontal, moveVertival).normalized;
        rb.linearVelocity = movement*moveSpeed;
    }

    public override void OnNetworkSpawn()
    {
        // Only attach camera to YOUR player
        if (IsOwner)
        {
            CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.target = transform;
            }
            else
            {
                Debug.LogError("Camera doesn't have CameraFollow script!");
            }
        }
    }

}
