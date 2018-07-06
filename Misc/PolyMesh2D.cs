using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PolyMesh2D : MonoBehaviour
{
    PolygonCollider2D _collider;
    MeshFilter _meshFilter;
    [SerializeField]
    Mesh _mesh;

    void OnEnable()
    {
        _collider = GetComponent<PolygonCollider2D>();
        _meshFilter = GetComponent<MeshFilter>();

        if (_mesh == null)
        {
            _mesh = new Mesh();
            _meshFilter.sharedMesh = _mesh;
            RegenerateMesh();
        }
    }

    void Update()
    {
        if (Application.isPlaying)
            return;

        RegenerateMesh();
    }

    void RegenerateMesh()
    {
        List<Vector3> points = _collider.points.Select(p => (Vector3)p).ToList();
        int[] tris = Triangulator.Triangulate(points);

        _mesh.SetVertices(points);
        _mesh.triangles = tris;
    }
}