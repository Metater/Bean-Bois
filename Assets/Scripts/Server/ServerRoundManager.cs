using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Telepathy;
using UnityEngine;

public class ServerRoundManager : NetworkBehaviour
{
    private const float WaitingPeriod = 60; // seconds

    private ServerManager server;
    private GameManager manager;
    [SerializeField] private ServerJengaTowerSpawner jengaTowerSpawner;

    private (List<JengaBlock> blocks, Vector3 spawnPosition) blueTower = (new(), Vector3.zero);
    private readonly List<Player> bluePlayers = new();
    private (List<JengaBlock> blocks, Vector3 spawnPosition) redTower = (new(), Vector3.zero);
    private readonly List<Player> redPlayers = new();

    private float waitTimeRemaining = WaitingPeriod;

    // Players can disappear at anytime, blocks can disappear at anytime, they can be destroyed

    private void Awake()
    {
        server = FindObjectOfType<ServerManager>(true);
        manager = FindObjectOfType<GameManager>(true);
    }

    public override void OnStartServer()
    {
        //jengaTowerSpawner.SpawnTower(jengaTowerSpawner.blueBaseTransform.position).blocks;
    }

    private void Update()
    {
        blueTower.blocks.RemoveAll(b => b is null);
        bluePlayers.RemoveAll(p => p is null);
        redPlayers.RemoveAll(p => p is null);
        redTower.blocks.RemoveAll(b => b is null);

        // On start: player.PlayerResetState

        if (IsRoundInProgress())
        {
            waitTimeRemaining = WaitingPeriod;
            server.SetText("");
        }
        else
        {
            foreach (var player in manager.PlayerLookup.Refs)
            {
                player.Spectate();
            }

            if (manager.PlayerLookup.Refs.Count() < 2)
            {
                waitTimeRemaining = WaitingPeriod;
                server.SetText("Waiting for more players...");
            }
            else
            {
                waitTimeRemaining -= Time.deltaTime;
                server.SetText($"Starting in {Mathf.RoundToInt(waitTimeRemaining)} seconds!");
                if (waitTimeRemaining <= 0)
                {
                    CleanupPreviousRound();
                    StartRound();
                }
            }
        }
    }

    [Server]
    private void CleanupPreviousRound()
    {
        blueTower.blocks.ForEach(b => b.NetworkDestroy());
        bluePlayers.Clear();
        redTower.blocks.ForEach(b => b.NetworkDestroy());
        redPlayers.Clear();
    }

    [Server]
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
            player.ResetState();
            if (playerNumber % 2 == 0)
            {
                player.Spawn(blueTower.spawnPosition);
                bluePlayers.Add(player);
            }
            else
            {
                player.Spawn(redTower.spawnPosition);
                redPlayers.Add(player);
            }
            playerNumber++;
        }
    }

    [Server]
    private bool IsRoundInProgress()
    {
        bool someBlocksOnBothTeams = blueTower.blocks.Count != 0 && redTower.blocks.Count != 0;
        int bluePlayersNotSpecating = bluePlayers.Count(p => !p.IsSpectating);
        int redPlayersNotSpecating = redPlayers.Count(p => !p.IsSpectating);
        bool somePlayersNotSpecatingOnBothTeams = bluePlayersNotSpecating != 0 && redPlayersNotSpecating != 0;

        return someBlocksOnBothTeams && somePlayersNotSpecatingOnBothTeams;
    }
}
