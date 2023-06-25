using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ScoreManager : NetworkBehaviour
{
    // Reference to the score UI text element
    [SerializeField] private Text scoreText;

    // Network variable to synchronize the score across all clients
    private NetworkVariable<int> syncedScore = new NetworkVariable<int>(0);

    private void Start()
    {
        // Update the local score UI
        UpdateScoreUI();

        syncedScore.OnValueChanged += OnSyncedScoreChanged;
    }

    private void UpdateScoreUI()
    {
        // Update the score text element
        scoreText.text = "Score: " + syncedScore.Value.ToString();
    }

    // Server RPC method to increment the score
    [ServerRpc (RequireOwnership = false)]
    public void IncrementScoreServerRpc()
    {
        // Increment the score
        syncedScore.Value++;
    }

    // Hook method called when the synced score changes
    private void OnSyncedScoreChanged(int previousScore, int newScore)
    {
        // Update the score UI
        UpdateScoreUI();
    }
}
