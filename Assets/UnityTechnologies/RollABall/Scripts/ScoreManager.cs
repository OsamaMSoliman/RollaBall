using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : SingletonNetwork<ScoreManager>
{
    // Reference to the score UI text element
    [SerializeField] private Text scoreText;
    [SerializeField] private Text winnerTxt;

    // Dictionary to store scores for each player
    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();

    private void Start()
    {
        // Update the local score UI
        scoreText.text = "";
        winnerTxt.text = "";

        GameManager.Instance.OnGameOver += () =>
        {
            Debug.Log($"{IsServer} {IsClient} {IsHost} {IsLocalPlayer} {IsOwner} {IsOwnedByServer}");
            var sortedByScores = playerScores.OrderByDescending(kv => kv.Value).FirstOrDefault();
            bool isDefault = sortedByScores.Value == 0 && sortedByScores.Key == 0;
            UpdateWinnerTxtClientRpc(isDefault ? "?" : sortedByScores.Key.ToString());
        };
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
    // Update the score text element
    private void UpdateScoreUIClientRpc(string scoreString)
    {
        // NOTE: on the client side this dictionary is empty!
        // that's why we have to send scoreString as input to ClientRpc
        scoreText.text = scoreString;
    }

    [ClientRpc]
    // Update the score text element
    private void UpdateWinnerTxtClientRpc(string winner)
    {
        // NOTE: on the client side this dictionary is empty!
        // that's why we have to send scoreString as input to ClientRpc
        winnerTxt.text = $"Winner Player: {winner}";
    }

    // Server RPC method to increment the score for a specific player
    [ServerRpc(RequireOwnership = false)]
    public void IncrementScoreServerRpc(ulong playerId)
    {
        // Check if the player exists in the dictionary
        playerScores.TryGetValue(playerId, out int playerScore);
        // Increment the player's score or Add the player to the dictionary and set their initial score to 1
        playerScores[playerId] = playerScore + 1;
        // Update the score UI
        UpdateScoreUIClientRpc(getScoreUpdates());
    }
}

