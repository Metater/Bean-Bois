using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpark : MonoBehaviour
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
