using System.Collections.Generic;
using UnityEngine;

public enum BallMesh
{
    Cube,
    IcosahedronRough,
    Icosahedron,
    Octacontagon,
    Icosikaihenagon,
    Sphere
}
public enum SpikeMesh
{
    AddPiece,
    AddBlock,
    Dissolve,
    WallAttractor,
    RandomDirection,
    Magnet
}
public struct VertexPair
{
    public Vector3 Vertex1 { get; set; }
    public Vector3 Vertex2 { get; set; }

    public VertexPair(Vector3 vertex1, Vector3 vertex2)
    {
        Vertex1 = vertex1;
        Vertex2 = vertex2;
    }
}

public struct Sizes
{
    public float ballDiameter { get; set; }
    public float padWidth { get; set; }
    public float fieldWidth { get; set; }
    public float fieldHeight { get; set; }
    public float fieldDepth { get; set; }
    public float wallThickness { get; set; }
    public float planeDistance { get; set; }
    public Vector2 canvasSize { get; set; }

    public Sizes(float ballDiameter, float padWidth, float fieldWidth, float fieldHeight, float fieldDepth, float wallThickness, float planeDistance, Vector2 canvasSize)
    {
        this.ballDiameter = ballDiameter;
        this.padWidth = padWidth;
        this.fieldWidth = fieldWidth;
        this.fieldHeight = fieldHeight;
        this.fieldDepth = fieldDepth;
        this.wallThickness = wallThickness;
        this.planeDistance = planeDistance;
        this.canvasSize = canvasSize;

    }

}
[System.Serializable]
public class Materials
{
    public Material edgeMaterial;
    public BallMaterials ballMaterials;
    public BallFragmentsMaterials ballFragmentsMaterials;
    public SpikeMaterials spikeMaterials;
    public Material padMaterial;
    public Material blockMaterial;
    [ColorUsage(true, true)]
    public Color ballDissolveGlowColor;
    [ColorUsage(true, true)]
    public Color darknessPolyGlowColor;
}
[System.Serializable]
public class BallFragmentsMaterials
{
    public Material burnFragmentMaterial;
    public Material freezeFragmentMaterial;
    public Material ballFragmentMaterial;
    public List<Material> cubeFragmentsMaterials = new List<Material>(4);
}
[System.Serializable]
public class BallMaterials
{
    public Material cubeMaterial;
    public Material icosahedronRoughMaterial;
    public Material icosahedronMaterial;
    public Material octacontagonMaterial;
    public Material icosikaihenagonMaterial;
    public Material sphereMaterial;
    public Material fallBackMaterial;
    public Material MaterialForCurrentMesh(BallMesh ballType)
    {
        switch (ballType)
        {
            default:
                return fallBackMaterial;
            case BallMesh.Cube:
                return cubeMaterial;
            case BallMesh.IcosahedronRough:
                return icosahedronRoughMaterial;
            case BallMesh.Icosahedron:
                return icosahedronMaterial;
            case BallMesh.Octacontagon:
                return octacontagonMaterial;
            case BallMesh.Icosikaihenagon:
                return icosikaihenagonMaterial;
            case BallMesh.Sphere:
                return sphereMaterial;

        }
    }
}
[System.Serializable]
public class SpikeMaterials
{
    public Material addPadPieceMaterial;
    public Material addPadBlockMaterial;
    public Material dissolveMaterial;
    public Material randomDirectionMaterial;
    public Material attractorMaterial;
    public Material repulsorMaterial;
    public Material wallAttractorMaterial;
    public Material healthUpMaterial;
    public Material MaterialForCurrentMesh(SpikeType spikeType)
    {
        switch (spikeType)
        {
            default:
                return addPadPieceMaterial;
            case SpikeType.SpikePadPiece:
                return addPadPieceMaterial;
            case SpikeType.SpikePadBlock:
                return addPadBlockMaterial;
            case SpikeType.SpikeDissolve:
                return dissolveMaterial;
            case SpikeType.SpikeWallAttractor:
                return wallAttractorMaterial;
            case SpikeType.SpikeRandomDirection:
                return randomDirectionMaterial;
            case SpikeType.SpikeMagnet:
                return repulsorMaterial;
            case SpikeType.HealthUp:
                return healthUpMaterial;
        }
    }
}
