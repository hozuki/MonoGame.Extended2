using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public sealed class TessellationSink : Sink
{

    internal TessellationSink(Mesh mesh)
    {
        _mesh = mesh;
        _mutableTriangles = new List<Triangle>();
        _frozenTriangles = Array.Empty<Triangle>();
    }

    public void AddTriangle(Triangle triangle)
    {
        EnsureNotClosed();

        _mutableTriangles.Add(triangle);
    }

    public void AddTriangles(Triangle[] triangles)
    {
        EnsureNotClosed();

        _mutableTriangles.AddRange(triangles);
    }

    internal void SetFrozenTriangles(Triangle[] triangles)
    {
        _frozenTriangles = triangles;
    }

    internal Triangle[] Triangles => _frozenTriangles;

    protected override void OnClosed()
    {
        _frozenTriangles = _mutableTriangles.ToArray();
        _mutableTriangles.Clear();
        _mesh.Close(this);
    }

    private readonly List<Triangle> _mutableTriangles;
    private Triangle[] _frozenTriangles;

    private readonly Mesh _mesh;

}
