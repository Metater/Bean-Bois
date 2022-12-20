using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerJengaTowerSpawner : NetworkBehaviour
{
    private const float HalfBaseHeight = 4f;
    private const float JengaBlockHeight = 4f;
    private const float HalfJengaBlockHeight = JengaBlockHeight / 2f;
    private const float JengaBlockWidth = 8f;
    private const float JengaBlockSpacing = 1f / 4f;
    private const int TowerLayerCount = 18;

    #region Fields
    public Transform blueBaseTransform;
    public Transform redBaseTransform;
    [SerializeField] private JengaBlock jengaBlockPrefab;
    #endregion Fields

    private void Update()
    {
        EasterEgg();
    }

    #region Easter Egg
    private readonly List<JengaBlock> easterEggBlocks = new();
    private void EasterEgg()
    {
        if (Input.GetKeyDown(KeyCode.KeypadDivide))
        {
            var blueBlocks = SpawnTower(blueBaseTransform.position).blocks;
            var redBlocks = SpawnTower(redBaseTransform.position).blocks;
            easterEggBlocks.AddRange(blueBlocks);
            easterEggBlocks.AddRange(redBlocks);
        }
        if (Input.GetKeyDown(KeyCode.KeypadMultiply))
        {
            foreach (var block in easterEggBlocks)
            {
                if (block != null)
                {
                    NetworkServer.Destroy(block.gameObject);
                }
            }
            easterEggBlocks.Clear();
        }
    }
    #endregion Easter Egg

    [Server]
    public (List<JengaBlock> blocks, Vector3 spawnPosition) SpawnTower(Vector3 basePosition)
    {
        List<JengaBlock> blocks = new();

        float xPosition = basePosition.x;
        float GetYPosition(int layer) => (basePosition.y + HalfBaseHeight + HalfJengaBlockHeight) + (JengaBlockHeight * (float)layer);
        float zPosition = basePosition.z;
        Vector3 GetPosition(int block, int layer)
        {
            if (layer % 2 == 0)
            {
                switch (block)
                {
                    case 0:
                        return new(xPosition, GetYPosition(layer), zPosition - (JengaBlockWidth + JengaBlockSpacing));
                    case 1:
                        return new(xPosition, GetYPosition(layer), zPosition);
                    case 2:
                        return new(xPosition, GetYPosition(layer), zPosition + (JengaBlockWidth + JengaBlockSpacing));
                }
            }
            else
            {
                switch (block)
                {
                    case 0:
                        return new(xPosition - (JengaBlockWidth + JengaBlockSpacing), GetYPosition(layer), zPosition);
                    case 1:
                        return new(xPosition, GetYPosition(layer), zPosition);
                    case 2:
                        return new(xPosition + (JengaBlockWidth + JengaBlockSpacing), GetYPosition(layer), zPosition);
                }
            }
            throw new Exception("Unreachable");
        }
        Quaternion GetRotation(int layer) => layer % 2 == 0 ? Quaternion.identity : Quaternion.Euler(0, 90, 0);

        for (int layer = 0; layer < TowerLayerCount; layer++)
        {
            for (int block = 0; block < 3; block++)
            {
                JengaBlock jengaBlock = Instantiate(jengaBlockPrefab, GetPosition(block, layer), GetRotation(layer));
                NetworkServer.Spawn(jengaBlock.gameObject);
                blocks.Add(jengaBlock);
            }
        }

        return (blocks, GetPosition(1, TowerLayerCount + 1));
    }
}
