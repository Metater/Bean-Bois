using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Items will be deleted when client that last touched them disconnects, not good
*/

public class GameManager : MonoBehaviour
{
    public Player LocalPlayer { get; private set; }

    /*
    [Header("Prefabs")]
    [SerializeField] private ProjectileTrail projectileTrailPrefab;

    public void SpawnProjectileTrail(Vector3 a, Vector3 b, bool hitObject, Vector3 hitNormal)
    {
        var projectileTrail = Instantiate(projectileTrailPrefab, Vector3.zero, Quaternion.identity);
        projectileTrail.Init(a, b, hitObject, hitNormal);
    }
    */
}
