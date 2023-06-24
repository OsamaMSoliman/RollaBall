using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private Rigidbody rb;
    public float moveSpeed = 5f;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!IsLocalPlayer) return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        if (movement != Vector3.zero)
        {
            MovePlayerServerRpc(movement);
        }
    }

    [ServerRpc]
    void MovePlayerServerRpc(Vector3 movement)
    {
        // Apply the movement to the host's Rigidbody
        rb.AddForce(movement * moveSpeed);

        // Synchronize the updated position and velocity to all clients
        MovePlayerClientRpc(rb.position, rb.velocity);
    }

    [ClientRpc]
    void MovePlayerClientRpc(Vector3 position, Vector3 velocity)
    {
        // Update the position and velocity of the client's Rigidbody
        rb.position = position;
        rb.velocity = velocity;
    }
}