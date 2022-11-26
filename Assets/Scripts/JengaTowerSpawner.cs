using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JengaTowerSpawner : NetworkBehaviour
{
    private const float HalfBaseHeight = 4f;
    private const float JengaBlockHeight = 4f;
    private const float HalfJengaBlockHeight = JengaBlockHeight / 2f;
    private const float JengaBlockWidth = 8f;
    private const float JengaBlockSpacing = 1f / 4f;

    [SerializeField] private JengaBlock jengaBlockPrefab;
    [SerializeField] private Transform baseTransform;
    [SerializeField] private int layerCount;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SpawnTower(baseTransform.position);
        }
    }

    [Server]
    public List<JengaBlock> SpawnTower(Vector3 basePosition)
    {
        List<JengaBlock> tower = new();

        float xPosition = basePosition.x;
        float GetYPosition(int layer) => (basePosition.y + HalfBaseHeight + HalfJengaBlockHeight) + (JengaBlockHeight * (float)layer);
        float zPosition = basePosition.z;
        Vector3 GetPosition(int block, int layer)
        {
            if (layer % 2 == 0) // z-axis spacing
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
            else // x-axis spacing
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

        for (int layer = 0; layer < layerCount; layer++)
        {
            for (int block = 0; block < 3; block++)
            {
                JengaBlock jengaBlock = Instantiate(jengaBlockPrefab, GetPosition(block, layer), GetRotation(layer));
                NetworkServer.Spawn(jengaBlock.gameObject);
                tower.Add(jengaBlock);
            }
        }

        return tower;
    }
}
