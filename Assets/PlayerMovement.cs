using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // If this is the Player capsule, ensure it is named "Player"
        gameObject.name = "Player";
    }

    void Update()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
            moveZ = 1f;
        if (Input.GetKey(KeyCode.DownArrow))
            moveZ = -1f;
        if (Input.GetKey(KeyCode.LeftArrow))
            moveX = -1f;
        if (Input.GetKey(KeyCode.RightArrow))
            moveX = 1f;

        Vector3 movement = new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);
    }
}
