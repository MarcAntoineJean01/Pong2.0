using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PongLocker;
using MeshLocker;
public class MeshManager : PongManager
{
    public static Mesh uiCubeMesh;
    public static Mesh uiSphereMesh;
    public static Mesh uiFinalMesh;
    bool fieldMeshesExpanded = false;
    public Materials materials;
    public Materials webMaterials;
    public static bool transitioning = false;
    void OnEnable()
    {
        uiCubeMesh = NewMesh("UICube", Vector3.one*um.uiCubeSize);
        uiSphereMesh = NewMesh("UISphere", Vector3.one*um.uiCubeSize);

        uiFinalMesh = new Mesh();
        uiFinalMesh.MarkDynamic();
        uiFinalMesh.vertices = uiCubeMesh.vertices;
        uiFinalMesh.triangles = uiCubeMesh.triangles;
        uiFinalMesh.uv = uiCubeMesh.uv;
        uiFinalMesh.normals = uiCubeMesh.normals;
        uiFinalMesh.colors = uiCubeMesh.colors;
        uiFinalMesh.tangents = uiCubeMesh.tangents;
        PostResize(uiFinalMesh.vertices, uiFinalMesh);
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
    public void ResizePadFragmentsMeshes(Side side, Vector3 resize)
    {
        switch (side)
        {
            case Side.Left:
                field.fragmentStore.leftPadFragments.ForEach(frg =>
                {
                    Vector3[] vertices = frg.meshF.mesh.vertices;
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(vertices[i].x + (vertices[i].x > 0 ? resize.x * 0.5f : -(resize.x * 0.5f)), vertices[i].y + (vertices[i].y > 0 ? resize.y * 0.5f : -(resize.y * 0.5f)), vertices[i].z + (vertices[i].z > 0 ? resize.z * 0.5f : -(resize.z * 0.5f)));
                    }
                    PostResize(vertices, frg.meshF.mesh);
                });
                break;
            case Side.Right:
                field.fragmentStore.rightPadFragments.ForEach(frg =>
                {
                    Vector3[] vertices = frg.meshF.mesh.vertices;
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(vertices[i].x + (vertices[i].x > 0 ? resize.x * 0.5f : -(resize.x * 0.5f)), vertices[i].y + (vertices[i].y > 0 ? resize.y * 0.5f : -(resize.y * 0.5f)), vertices[i].z + (vertices[i].z > 0 ? resize.z * 0.5f : -(resize.z * 0.5f)));
                    }
                    PostResize(vertices, frg.meshF.mesh);
                });
                break;
        }
    }
    public void ResizeEnergyShieldMesh(Mesh mesh, Vector3 resize)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x, vertices[i].y + (vertices[i].y > 0 ? resize.y * 0.5f : -(resize.y * 0.5f)), vertices[i].z);
        }
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
    public List<VertexPair> CreateVertexPairs(Vector3[] oldVertices, Vector3[] newVertices, bool startingMesh)
    {
        List<VertexPair> vertexPair = new List<VertexPair>();
        int count = startingMesh ? oldVertices.Length : newVertices.Length;
        for (int i = 0; i < count; i++)
        {
            var vertex = startingMesh ? oldVertices[i] : newVertices[i];

            var nearestToCurrentVertex = startingMesh ? newVertices.OrderBy(v => Vector3.Distance(v, vertex)).FirstOrDefault() : oldVertices.OrderBy(v => Vector3.Distance(v, vertex)).FirstOrDefault();

            vertexPair.Add(new VertexPair(vertex, nearestToCurrentVertex));
        }
        return vertexPair;
    }
    void Deform(Mesh oldMesh, Mesh newMesh, float ammount, int[] oldTriangles, int[] newTriangles, List<Vector3> finalVertices, Mesh interpolatedMesh, List<VertexPair> pairsOfVertices1, List<VertexPair> pairsOfVertices2)
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
    void DeformSameVertexCount(Mesh startingMesh, float ammount, int[] startingTriangles, List<Vector3> finalVertices, Mesh interpolatedMesh, List<VertexPair> pairsOfVertices)
    {
        finalVertices = pairsOfVertices.Select(p => Vector3.Lerp(p.Vertex1, p.Vertex2, ammount)).ToList();

        interpolatedMesh.Clear();

        interpolatedMesh.SetVertices(finalVertices);
        interpolatedMesh.triangles = startingTriangles;

        interpolatedMesh.bounds = startingMesh.bounds;
        interpolatedMesh.uv = startingMesh.uv;
        interpolatedMesh.uv2 = startingMesh.uv2;
        interpolatedMesh.uv3 = startingMesh.uv3;

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
                Deform(oldMesh, newMesh, t / pm.speeds.transitionSpeeds.meshSwappingSpeed, oldTriangles, newTriangles, finalVertices, interpolatedMesh, CreateVertexPairs(oldVertices, newVertices, true), CreateVertexPairs(oldVertices, newVertices, false));
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
    public void UpdateUICubeMesh(float t)
    {
        DeformSameVertexCount(uiCubeMesh, t, uiCubeMesh.triangles, uiFinalMesh.vertices.ToList(), uiFinalMesh, CreateVertexPairs(uiCubeMesh.vertices, uiSphereMesh.vertices, true));
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
