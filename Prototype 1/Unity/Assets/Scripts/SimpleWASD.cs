using UnityEngine;

public class SimpleWASD : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        Vector3 movement = Vector3.zero;

        // W and S for forward/backward
        if (Input.GetKey(KeyCode.W)) movement += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) movement -= Vector3.forward;

        // A and D for left/right
        if (Input.GetKey(KeyCode.A)) movement -= Vector3.right;
        if (Input.GetKey(KeyCode.D)) movement += Vector3.right;

        // Q and E for rotate
        if (Input.GetKey(KeyCode.Q)) transform.Rotate(0, -90f * Time.deltaTime, 0);
        if (Input.GetKey(KeyCode.E)) transform.Rotate(0, 90f * Time.deltaTime, 0);

        // Move
        if (movement != Vector3.zero)
        {
            transform.Translate(movement.normalized * moveSpeed * Time.deltaTime, Space.Self);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
    }
}
