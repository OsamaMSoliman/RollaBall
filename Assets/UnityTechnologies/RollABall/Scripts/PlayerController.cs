using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private Rigidbody rb;
    public float moveSpeed = 5f;

    public static event Action<Transform> PlayerSpawnedLocally;
    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        if (IsLocalPlayer) PlayerSpawnedLocally?.Invoke(transform);
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
    private void MovePlayerServerRpc(Vector3 movement)
    {
        // Apply the movement to the host's Rigidbody
        rb.AddForce(movement * moveSpeed);

        // Synchronize the updated position and velocity to all clients
        MovePlayerClientRpc(rb.position, rb.velocity);
    }

    [ClientRpc]
    private void MovePlayerClientRpc(Vector3 position, Vector3 velocity)
    {
        // Update the position and velocity of the client's Rigidbody
        rb.position = position;
        rb.velocity = velocity;
    }

    // Reference to the ScoreManager script
    private ScoreManager scoreManager;

    private void Start()
    {
        // Find and cache the ScoreManager script
        scoreManager = FindObjectOfType<ScoreManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsLocalPlayer) return;

        if (other.CompareTag("Pick Up"))
        {
            // Get the network object associated with the pickup
            NetworkObject networkObject = other.GetComponent<NetworkObject>();

            // Check if the network object exists
            if (networkObject != null)
            {
                // Call the server method to handle pickup collection and score incrementing
                CollectPickupServerRpc(networkObject.NetworkObjectId);
            }
        }
    }

    // Server RPC method to handle pickup collection and score incrementing
    [ServerRpc(RequireOwnership = true)]
    private void CollectPickupServerRpc(ulong pickupNetworkObjectId, ServerRpcParams serverRpcParams = default)
    {
        // Get the network object associated with the pickup using the ID
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(pickupNetworkObjectId, out NetworkObject pickupNetworkObject))
        {
            // Only allow the server to despawn the pickup object
            if (NetworkManager.Singleton.IsServer)
            {
                // Destroy the pickup object on all clients
                pickupNetworkObject.Despawn(true);
            }

            // Increment the score for the specific player on the server
            scoreManager.IncrementScoreServerRpc(serverRpcParams.Receive.SenderClientId);
        }
    }
}