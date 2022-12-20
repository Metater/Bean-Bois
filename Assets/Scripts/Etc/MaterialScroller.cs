using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScroller : MonoBehaviour
{
    [SerializeField] private Material scrollMaterial;
    [SerializeField] private Vector2 scrollVelocity;

    private Vector2 scrollPosition = Vector2.zero;

    private void Update()
    {
        scrollPosition += scrollVelocity * Time.deltaTime;
        scrollMaterial.mainTextureOffset = scrollPosition;
    }

    private void OnApplicationQuit()
    {
        scrollMaterial.mainTextureOffset = Vector2.zero;
    }
}
