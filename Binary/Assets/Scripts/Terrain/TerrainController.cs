using System.Collections.Generic;
using UnityEngine;

public class TerrainController : MonoBehaviour
{
    [SerializeField] private List<BoxCollider2D> _TerrainColliders;

    public List<BoxCollider2D> TerrainColliders => _TerrainColliders;
}
