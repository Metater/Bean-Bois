using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchiveProjectileTrail : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float unitsPerSecond;
    [SerializeField] private float trailLength;

    [SerializeField] private ArchiveProjectileSpark projectileSparkPrefab;

    private float magnitude = 0;
    private float distanceToTravel;
    private Vector3 a;
    private Vector3 b;

    private bool hitObject;
    private Vector3 hitNormal;

    public void Init(Vector3 a, Vector3 b, bool hitObject, Vector3 hitNormal)
    {
        this.a = a;
        this.b = b;
        transform.rotation = Quaternion.LookRotation((b - a).normalized);
        transform.position = a;
        distanceToTravel = Vector3.Distance(a, b);

        this.hitObject = hitObject;
        this.hitNormal = hitNormal;
    }

    private void Update()
    {
        if (magnitude >= Mathf.Max(distanceToTravel - trailLength, 0))
        {
            Destroy(gameObject);
            if (hitObject)
            {
                Instantiate(projectileSparkPrefab, b, Quaternion.LookRotation(hitNormal));
            }
        }
        magnitude += unitsPerSecond * Time.deltaTime;
        Vector3 position = Vector3.Lerp(a, b, magnitude / distanceToTravel);
        transform.position = position;
    }
}
