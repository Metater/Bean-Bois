using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchiveProjectileSpark : MonoBehaviour
{
    [SerializeField] private ParticleSystem spark;

    private void Update()
    {
        if (!spark.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
