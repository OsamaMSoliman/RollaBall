using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : NetworkBehaviour
{
    // Reference to the score UI text element
    [SerializeField] private Text scoreText;

    // Dictionary to store scores for each player
    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();

    private void Start()
    {
        // Update the local score UI
        if (IsServer) UpdateScoreUIClientRpc("");
    }

    private string getScoreUpdates()
    {
        // Generate the score text
        string scoreString = "Scores:\n";

        // Sort the player scores in descending order
        foreach (var playerScore in playerScores.OrderByDescending(kv => kv.Value))
        {
            scoreString += $"Player {playerScore.Key}: {playerScore.Value}\n";
        }
        return scoreString;
    }

    [ClientRpc]
    private void UpdateScoreUIClientRpc(string scoreString)
    {
        // Update the score text element
        scoreText.text = scoreString;
    }

    // Server RPC method to increment the score for a specific player
    [ServerRpc(RequireOwnership = false)]
    public void IncrementScoreServerRpc(ulong playerId)
    {
        // Check if the player exists in the dictionary
        if (playerScores.ContainsKey(playerId))
        {
            // Increment the player's score
            playerScores[playerId]++;
        }
        else
        {
            // Add the player to the dictionary and set their initial score to 1
            playerScores.Add(playerId, 1);
        }

        // Update the score UI
        UpdateScoreUIClientRpc(getScoreUpdates());
    }
}

