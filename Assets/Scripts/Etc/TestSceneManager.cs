using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TestSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject shooter;
    [SerializeField] private GameObject target;
    [SerializeField] private Transform ephemeralTarget;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        var collider = target.GetComponent<Collider>();
        Stopwatch sw = Stopwatch.StartNew();
        Vector3 targetCurrentPos = target.transform.position;
        target.transform.position = ephemeralTarget.position;
        collider.enabled = false;
        collider.enabled = true;
        if (Physics.Raycast(shooter.transform.position, shooter.transform.right, out var _))
        {
                
        }
        target.transform.position = targetCurrentPos;
        print(sw.Elapsed);
    }
}
