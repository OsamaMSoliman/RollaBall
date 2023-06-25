using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : SingletonNetwork<GameManager>
{
    public event Action OnGameStarted;
    public event Action OnGameOver;
    private int playerCount = 0;
    private bool gameStarted = false;

    private void Start()
    {
        // Subscribe to the event when a client successfully connects to the server
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    // public override void OnDestroy()
    // {
    //     // Unsubscribe from the event when the GameManager is destroyed
    //     NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    //     base.OnDestroy();
    // }

    private void OnClientConnected(ulong clientId)
    {
        // Increment the player count when a client successfully connects
        playerCount++;

        // Check if at least two players have joined
        if (playerCount >= 2 && !gameStarted)
        {
            // Start the game
            StartGame();
        }
    }

    private void StartGame()
    {
        gameStarted = true;
        OnGameStarted?.Invoke();
    }

    internal void GameOver()
    {
        gameStarted = false;
        OnGameOver?.Invoke();
    }
}
