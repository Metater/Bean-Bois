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
        scrollMaterial.mainTextureOffset = scrollPosition;
        scrollPosition += scrollVelocity * Time.deltaTime;
    }
}
