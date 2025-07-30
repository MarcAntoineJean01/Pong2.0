using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshManager : PongManager
{
    bool fieldMeshesExpanded = false;
    public Materials materials;
    public Materials webMaterials;
    public static bool transitioning = false;
    public BallMesh NextBallMesh(BallMesh oldMesh)
    {
        switch (oldMesh)
        {
            case BallMesh.Cube:
                return BallMesh.IcosahedronRough;
            case BallMesh.Icosahedron:
                return BallMesh.Octacontagon;
            case BallMesh.Octacontagon:
                return BallMesh.Icosikaihenagon;
            default:
                return BallMesh.Cube;
        }
    }
    public int FragmentsForMesh(BallMesh oldMesh)
    {
        switch (oldMesh)
        {
            default:
                return 0;
            case BallMesh.Cube:
                return 4;
            case BallMesh.Icosahedron:
                return 20;
            case BallMesh.Octacontagon:
                return 4;
        }
    }
    public Mesh NewMesh(string type, Vector3 multiplier)
    {
        Mesh mesh = Resources.Load("PongMeshes/" + type.Replace("Fragmented", ""), typeof(Mesh)) as Mesh;
        Mesh meshInstance = new Mesh();
        meshInstance.vertices = mesh.vertices;
        meshInstance.triangles = mesh.triangles;
        meshInstance.uv = mesh.uv;
        meshInstance.normals = mesh.normals;
        meshInstance.colors = mesh.colors;
        meshInstance.tangents = mesh.tangents;
        Vector3[] vertices = meshInstance.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x * multiplier.x, vertices[i].y * multiplier.y, vertices[i].z * multiplier.z);
        }
        PostResize(vertices, meshInstance);
        Resources.UnloadAsset(mesh);
        return meshInstance;
    }
    void PostResize(Vector3[] vertices, Mesh mesh)
    {
        mesh.SetVertices(vertices);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
    void ResizeMesh(Mesh mesh, Vector3 multiplier)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x * multiplier.x, vertices[i].y * multiplier.y, vertices[i].z * multiplier.z);
        }
        PostResize(vertices, mesh);
    }
    public void ResizePadMesh(Mesh mesh, Vector3 resize)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x + (vertices[i].x > 0 ? resize.x * 0.5f : -(resize.x * 0.5f)), vertices[i].y + (vertices[i].y > 0 ? resize.y * 0.5f : -(resize.y * 0.5f)), vertices[i].z + (vertices[i].z > 0 ? resize.z * 0.5f : -(resize.z * 0.5f)));
        }
        PostResize(vertices, mesh);
    }
    public void ResizeEnergyShieldMesh(Mesh mesh, Vector3 resize)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x, vertices[i].y + (vertices[i].y > 0 ? resize.y * 0.5f : -(resize.y * 0.5f)), vertices[i].z);
        }
        // for (int i = 0; i < vertices.Length; i++)
        // {
        //     vertices[i] = new Vector3(vertices[i].x*resize.x, vertices[i].y*resize.y, vertices[i].z*resize.z);
        // }
        PostResize(vertices, mesh);
    }
    public void ResizeMeshTop(Mesh mesh, float resize)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y > 0)
            {
                vertices[i] = new Vector3(vertices[i].x, vertices[i].y + resize, vertices[i].z);
            }
        }
        PostResize(vertices, mesh);
    }
    public void ResizeMeshBottom(Mesh mesh, float resize)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y < 0)
            {
                vertices[i] = new Vector3(vertices[i].x, vertices[i].y - resize, vertices[i].z);
            }
        }
        PostResize(vertices, mesh);
    }
    public void ResizePadDepth(Mesh mesh, float resize)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].z < 0)
            {
                vertices[i] = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z - resize);

            }
            if (vertices[i].z > 0)
            {
                vertices[i] = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z + resize);

            }
        }
        PostResize(vertices, mesh);
    }
    public List<VertexPair> CreatePairs1(Vector3[] oldVertices, Vector3[] newVertices)
    {
        List<VertexPair> pairsOfVertices1 = new List<VertexPair>();

        for (int i = 0; i < oldVertices.Length; i++)
        {
            var oldVertex = oldVertices[i];

            var nearestToOldVertex = newVertices.OrderBy(v => Vector3.Distance(v, oldVertex)).FirstOrDefault();

            pairsOfVertices1.Add(new VertexPair(oldVertex, nearestToOldVertex));
        }
        return pairsOfVertices1;
    }
    public List<VertexPair> CreatePairs2(Vector3[] oldVertices, Vector3[] newVertices)
    {
        List<VertexPair> pairsOfVertices2 = new List<VertexPair>();

        for (int i = 0; i < newVertices.Length; i++)
        {
            var newVertex = newVertices[i];

            var nearestToNewVertex = oldVertices.OrderBy(v => Vector3.Distance(v, newVertex)).FirstOrDefault();

            pairsOfVertices2.Add(new VertexPair(newVertex, nearestToNewVertex));
        }
        return pairsOfVertices2;
    }
    void Deform(MeshRenderer renderer, Mesh oldMesh, Mesh newMesh, float ammount, int[] oldTriangles, int[] newTriangles, List<Vector3> finalVertices, Mesh interpolatedMesh, List<VertexPair> pairsOfVertices1, List<VertexPair> pairsOfVertices2)
    {
        if (ammount < 0.5f)
        {
            finalVertices = pairsOfVertices1.Select(p => Vector3.Lerp(p.Vertex1, p.Vertex2, ammount)).ToList();
        }
        else
        {
            finalVertices = pairsOfVertices2.Select(p => Vector3.Lerp(p.Vertex2, p.Vertex1, ammount)).ToList();
        }

        interpolatedMesh.Clear();

        interpolatedMesh.SetVertices(finalVertices);
        interpolatedMesh.triangles = ammount < 0.5f ? oldTriangles : newTriangles;

        interpolatedMesh.bounds = ammount < 0.5f ? oldMesh.bounds : newMesh.bounds;
        interpolatedMesh.uv = ammount < 0.5f ? oldMesh.uv : newMesh.uv;
        interpolatedMesh.uv2 = ammount < 0.5f ? oldMesh.uv2 : newMesh.uv2;
        interpolatedMesh.uv3 = ammount < 0.5f ? oldMesh.uv3 : newMesh.uv3;

        interpolatedMesh.RecalculateNormals();
    }
    public void TransitionBallMeshes(BallMesh newMeshType, float multiplier)
    {
        StopAllCoroutines();
        Mesh newMesh = NewMesh(newMeshType.ToString(), Vector3.one * builder.canvasBallDiameter * multiplier);
        StartCoroutine(CycleSwitchMeshes(field.ball, field.ball.meshF.mesh, newMesh));
    }
    IEnumerator CycleSwitchMeshes(PongEntity entity, Mesh oldMesh, Mesh newMesh)
    {
        transitioning = false;
        Vector3[] oldVertices = oldMesh.vertices;
        Vector3[] newVertices = newMesh.vertices;

        int[] oldTriangles = oldMesh.triangles;
        int[] newTriangles = newMesh.triangles;

        Mesh interpolatedMesh = new Mesh();
        interpolatedMesh.MarkDynamic();
        entity.meshF.mesh = interpolatedMesh;

        List<Vector3> finalVertices = new List<Vector3>(oldVertices);
        float t = 0f;
        {
            while (t < pm.speeds.transitionSpeeds.meshSwappingSpeed)
            {
                t += Time.unscaledDeltaTime;
                if (t > pm.speeds.transitionSpeeds.meshSwappingSpeed) { t = pm.speeds.transitionSpeeds.meshSwappingSpeed; }
                Deform(entity.meshR, oldMesh, newMesh, t / pm.speeds.transitionSpeeds.meshSwappingSpeed, oldTriangles, newTriangles, finalVertices, interpolatedMesh, CreatePairs1(oldVertices, newVertices), CreatePairs2(oldVertices, newVertices));
                yield return null;
            }
        }
        GameObject.Destroy(entity.col);
        if (newTriangles.Length > 750) // max allowed triangles is 250 newTriangles.length is "ammount of triangles * 3"
        {
            entity.col = entity.gameObject.AddComponent<SphereCollider>();
            (entity.col as SphereCollider).radius = entity.meshF.mesh.bounds.size.x * 0.5f;
        }
        else
        {
            entity.col = entity.gameObject.AddComponent<MeshCollider>();
            (entity.col as MeshCollider).sharedMesh = entity.meshF.mesh;
            (entity.col as MeshCollider).convex = true;
        }
        entity.col.isTrigger = false;
        entity.col.sharedMaterial = builder.bouncyMaterial;
        transitioning = false;
    }
    public void TransitionFieldMeshes(bool expand = true)
    {
        if ((expand && !fieldMeshesExpanded) || (!expand && fieldMeshesExpanded))
        {
            Mesh mesh = field.topFloor.meshF.mesh;
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = expand ? new Vector3(vertices[i].x * 10, vertices[i].y * 10, vertices[i].z) : new Vector3(vertices[i].x * 0.1f, vertices[i].y * 0.1f, vertices[i].z);
            }
            SetupMesh(vertices, mesh, field.topFloor.meshF);


            mesh = field.bottomFloor.meshF.mesh;
            vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = expand ? new Vector3(vertices[i].x * 10, vertices[i].y * 10, vertices[i].z) : new Vector3(vertices[i].x * 0.1f, vertices[i].y * 0.1f, vertices[i].z);
            }
            SetupMesh(vertices, mesh, field.bottomFloor.meshF);


            mesh = field.leftWall.meshF.mesh;
            vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = expand ? new Vector3(vertices[i].x * 10, vertices[i].y, vertices[i].z) : new Vector3(vertices[i].x * 0.1f, vertices[i].y, vertices[i].z);
            }
            SetupMesh(vertices, mesh, field.leftWall.meshF);


            mesh = field.rightWall.meshF.mesh;
            vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = expand ? new Vector3(vertices[i].x * 10, vertices[i].y, vertices[i].z) : new Vector3(vertices[i].x * 0.1f, vertices[i].y, vertices[i].z);
            }
            SetupMesh(vertices, mesh, field.rightWall.meshF);
        }
        fieldMeshesExpanded = expand;
    }
    
    public void SetupMesh(Vector3[] vertices, Mesh mesh, MeshFilter meshFilter)
    {
        mesh.SetVertices(vertices);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        meshFilter.mesh = mesh;
    }
}
