using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerRoundManager : NetworkBehaviour
{
    private GameManager manager;
    private NetworkUiManager networkUi;

    [SerializeField] private ServerJengaTowerSpawner jengaTowerSpawner;
    [SerializeField] private float waitingPeriod;

    private (List<JengaBlock> blocks, Vector3 spawnPosition) blueTower = (new(), Vector3.zero);
    private readonly List<Player> bluePlayers = new();
    private (List<JengaBlock> blocks, Vector3 spawnPosition) redTower = (new(), Vector3.zero);
    private readonly List<Player> redPlayers = new();

    private float waitTimeRemaining;

    private void Awake()
    {
        manager = FindObjectOfType<GameManager>(true);
        networkUi = FindObjectOfType<NetworkUiManager>(true);

        waitTimeRemaining = waitingPeriod;
    }

    private void Update()
    {
        CleanupNull();

        if (IsRoundInProgress())
        {
            networkUi.globalText = "";
            // Set text for what team is going
            // Who on the team is going
            // And time remaining for that turn
        }
        else
        {
            foreach (var player in manager.PlayerLookup.Refs)
            {
                player.isSpectating = true;
            }

            if (manager.PlayerLookup.Refs.Count() < 2)
            {
                waitTimeRemaining = waitingPeriod;
                networkUi.globalText = "Waiting for more players...";
            }
            else
            {
                waitTimeRemaining -= Time.deltaTime;
                networkUi.globalText = $"Starting in {Mathf.RoundToInt(waitTimeRemaining)} seconds!";
                if (waitTimeRemaining <= 0)
                {
                    CleanupPreviousRound();
                    StartRound();
                }
            }
        }
    }

    private void CleanupNull()
    {
        blueTower.blocks.RemoveAll(b => b == null);
        bluePlayers.RemoveAll(p => p == null);
        redPlayers.RemoveAll(p => p == null);
        redTower.blocks.RemoveAll(b => b == null);
    }

    private void CleanupPreviousRound()
    {
        CleanupNull();

        blueTower.blocks.ForEach(b => b.NetworkDestroy());
        bluePlayers.Clear();
        redTower.blocks.ForEach(b => b.NetworkDestroy());
        redPlayers.Clear();

        waitTimeRemaining = waitingPeriod;
    }

    private void StartRound()
    {
        blueTower = jengaTowerSpawner.SpawnTower(jengaTowerSpawner.blueBaseTransform.position);
        redTower = jengaTowerSpawner.SpawnTower(jengaTowerSpawner.redBaseTransform.position);

        List<Player> players = manager.PlayerLookup.Refs.ToList();
        int playerNumber = 0;
        while (players.Count > 0)
        {
            int index = Random.Range(0, players.Count);
            Player player = players[index];
            players.RemoveAt(index);
            player.RoundInit();
            if (playerNumber % 2 == 0)
            {
                player.ServerStopSpectating(blueTower.spawnPosition);
                bluePlayers.Add(player);
            }
            else
            {
                player.ServerStopSpectating(redTower.spawnPosition);
                redPlayers.Add(player);
            }
            playerNumber++;
        }
    }

    private bool IsRoundInProgress()
    {
        bool someBlocksOnBothTeams = blueTower.blocks.Count != 0 &&
            redTower.blocks.Count != 0;

        bool somePlayersNotSpecatingOnBothTeams = bluePlayers.Count(p => !p.isSpectating) != 0 &&
            redPlayers.Count(p => !p.isSpectating) != 0;

        return someBlocksOnBothTeams && somePlayersNotSpecatingOnBothTeams;
    }
}
