using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class Builder : PongManager
{
    public RigidBodiesExcludeLayers rigidBodiesExcludeLayers;
    public GameObject ballPrefab;
    public GameObject fragmentPrefab;
    public List<GameObject> spikePrefabs = new List<GameObject>();
    public List<GameObject> debuffPrefabs = new List<GameObject>();
    public GameObject webDebuffSlowPrefab;
    public List<GameObject> edges = new List<GameObject>();
    public GameObject padPrefab;
    public GameObject blockPrefab;
    public PhysicMaterial bouncyMaterial;
    public PhysicMaterial notSoBouncyMaterial;
    //measures
    float canvasFieldDepth;
    float canvasWallThickness;
    float canvasPadWidth;
    public float canvasBallDiameter;
    float canvasSpikeDiameter;
    float canvasDebuffDiameter;
    public Mesh storedMeshForVirtualDebuffs;
    //SET INITIAL SIZES ON START. THE FIELD IS FIRST BUILT AS PART OF THE SCREEN CANVAS
    //THIS WAY WE CAN ENSURE CERTAIN PROPORTIONS
    //ONCE BUILD THE FIELD IS MOVED TO WORLD SPACE AND THE ACTUAL WORLD SPACE SIZES
    //ARE PERSISTED BY THE GAME MANAGER FOR LATER USE
    void Start()
    {
        canvasFieldDepth = Screen.width * 2;
        canvasWallThickness = 20;
        canvasPadWidth = Screen.height * 0.01f * pm.initialSizes.padSize; // *0.01f === /100
        canvasBallDiameter = Screen.height * 0.01f * pm.initialSizes.ballSize; // *0.01f === /100
        canvasSpikeDiameter = Screen.height * 0.01f * pm.initialSizes.spikeSize; // *0.01f === /100
        canvasDebuffDiameter = Screen.height * 0.01f * pm.initialSizes.debuffSize; // *0.01f === /100
    }
    public void Build()
    {
        field = new Field(
            fieldParent,
            MakeFloor(Side.Top),
            MakeFloor(Side.Bottom),
            MakeWall(Side.Left),
            MakeWall(Side.Right),
            MakeBackground(),
            MakePad(PadType.Rough, Side.Left),
            MakePad(PadType.Rough, Side.Right),
            MakeFullBall(BallMesh.Cube),
            MakeSpikeStore(),
            MakeDebuffStore(),
            MakeFragmentStore()
        );
        MakeSizes();
        field.ball.SetBallState(State.Idle);
        field.spikeStore.StoreSpikes();
        field.debuffStore.StoreDebuffs();
        storedMeshForVirtualDebuffs = mm.NewMesh(BallMesh.Icosahedron.ToString(), Vector3.one * canvasBallDiameter * 1.2f);
    }
    void MakeSizes()
    {
        Vector2 cs = new Vector2(menuCanvas.GetComponent<RectTransform>().rect.width, menuCanvas.GetComponent<RectTransform>().rect.height);
        sizes = new Sizes
        (   
            field.ball.col.bounds.size.z, //ball diameter
            field.leftPad.col.bounds.size.z, //pad width
            field.background.col.bounds.size.x, //field width
            field.background.col.bounds.size.y, //field height
            field.bottomFloor.col.bounds.size.x, //field depth
            field.background.col.bounds.size.z, //wall thickness
            menuCanvas.planeDistance, //canvas plane distance (think of it as the distance at which the camera is focused)
            cs //canvas size (screen size)
        );
        //REMOVED BALL DIAMETER FROM FIELD BOUNDS CALCULATION FOR SPEECH BUBBLES, THEY NEED EXACT FIELD BOUNDS TO DETECT IF THEY ARE ACTUALLY IN THE FIELD
        //IF THIS CHANGE MESSES WITH IN FIELD BALL DETECTION (FOR RESET WHEN BALL OUTSIDE OF FIELD), REVERT AND GIVE SPEECH BUBBLE THEIR OWN FIELDBOUNDS VARIALBE.
        // PongEntity.fieldBounds = new Vector2 ((sizes.fieldWidth+sizes.ballDiameter)*0.5f, (sizes.fieldHeight+sizes.ballDiameter)*0.5f);
        PongEntity.fieldBounds = new Vector2 (sizes.fieldWidth*0.5f, sizes.fieldHeight*0.5f);
    }
    Wall MakeWall(Side side)
    {
        Wall wall = GameObject.Instantiate(edges[(int)EdgeType.Wall], menuCanvas.transform).GetComponent<Wall>();
        wall.sd = side;
        Mesh mesh = wall.meshF.mesh;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x * canvasFieldDepth, vertices[i].y * Screen.height, vertices[i].z * canvasWallThickness);
        }
        mm.SetupMesh(vertices, mesh, wall.meshF);
        Vector3 v3;
        switch (wall.sd)
        {
            default:
            case Side.Left:
                wall.gameObject.name = "LeftWall";
                wall.transform.rotation = Quaternion.Euler(0, -90, 0);
                v3 = new Vector3(-((Screen.width * 0.5f) + (wall.meshF.mesh.bounds.size.z * 0.5f)), 0, 0);
                break;
            case Side.Right:
                wall.gameObject.name = "RightWall";
                wall.transform.rotation = Quaternion.Euler(0, 90, 0);
                v3 = new Vector3((Screen.width * 0.5f) + (wall.meshF.mesh.bounds.size.z * 0.5f), 0, 0);
                break;
        }
        wall.transform.localPosition = v3;
        (wall.col as BoxCollider).size = new Vector3(mesh.bounds.size.x, mesh.bounds.size.y, mesh.bounds.size.z);
        wall.transform.SetParent(fieldParent.transform);
#if UNITY_WEBGL
        wall.meshR.material = new Material(mm.webMaterials.edgeMaterial);
#else
        wall.meshR.material = new Material(mm.materials.edgeMaterial);
#endif

        return wall;
    }
    Floor MakeFloor(Side side)
    {
        Floor floor = GameObject.Instantiate(edges[(int)EdgeType.Floor], menuCanvas.transform).GetComponent<Floor>();
        floor.sd = side;
        Mesh mesh = floor.meshF.mesh;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x * canvasFieldDepth, vertices[i].y * Screen.width, vertices[i].z * canvasWallThickness);
        }
        mm.SetupMesh(vertices, mesh, floor.meshF);

        Vector3 v3;
        switch (floor.sd)
        {
            default:
            case Side.Top:
                floor.gameObject.name = "TopFloor";
                floor.transform.rotation = Quaternion.Euler(-90, -90, 0);
                v3 = new Vector3(0, (Screen.height * 0.5f) + (floor.meshF.mesh.bounds.size.z * 0.5f), 0);
                break;
            case Side.Bottom:
                floor.gameObject.name = "BottomFloor";
                floor.transform.rotation = Quaternion.Euler(90, 90, 0);
                v3 = new Vector3(0, -((Screen.height * 0.5f) + (floor.meshF.mesh.bounds.size.z * 0.5f)), 0);
                break;
        }
        floor.transform.localPosition = v3;
        (floor.col as BoxCollider).size = new Vector3(mesh.bounds.size.x, mesh.bounds.size.y, mesh.bounds.size.z);
        floor.transform.SetParent(fieldParent.transform);
        floor.meshR.material = new Material(mm.materials.edgeMaterial);
#if UNITY_WEBGL
        floor.meshR.material = new Material(mm.webMaterials.edgeMaterial);
#else
        floor.meshR.material = new Material(mm.materials.edgeMaterial);
#endif
        return floor;
    }
    Background MakeBackground()
    {
        Background background = GameObject.Instantiate(edges[(int)EdgeType.Background], menuCanvas.transform).GetComponent<Background>();
        background.gameObject.name = "Background";
        Mesh mesh = background.meshF.mesh;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x * Screen.width, vertices[i].y * Screen.height, vertices[i].z * canvasWallThickness);
        }
        mm.SetupMesh(vertices, mesh, background.meshF);

        (background.col as BoxCollider).size = new Vector3(mesh.bounds.size.x, mesh.bounds.size.y, mesh.bounds.size.z);
        background.transform.localPosition = new Vector3(0, 0, (canvasFieldDepth + canvasWallThickness) * 0.5f);
        background.transform.SetParent(fieldParent.transform);
#if UNITY_WEBGL
        background.meshR.material = new Material(mm.webMaterials.edgeMaterial);
        background.meshR.material.SetFloat("_TilesFlatness", 1);
#else
        background.meshR.material = new Material(mm.materials.edgeMaterial);
        background.meshR.material.SetFloat("_CellsDepth", -1);
#endif

        return background;
    }
    public Pad MakePad(PadType padType, Side side)
    {
        Pad pad = GameObject.Instantiate(padPrefab, menuCanvas.transform).GetComponent<Pad>();
        pad.sd = side;
        pad.padType = padType;
        pad.meshF.mesh = mm.NewMesh("Pad"+padType.ToString(), Vector3.one * canvasPadWidth);
        if ((side == Side.Left && field.leftPad != null) || (side == Side.Right && field.rightPad != null))
        {
            mm.ResizePadMesh(pad.meshF.mesh, new Vector3(0, (side == Side.Left ? field.leftPad.meshF.mesh.bounds.size.y : field.rightPad.meshF.mesh.bounds.size.y) - canvasPadWidth * 3, 0));
        }
        pad.gameObject.name = side == Side.Left ? "LeftPad" : "RightPad";
        switch (side)
        {
            case Side.Left:
                if (leftPlayer.controller == PlayerController.Player)
                {
                    if (Gamepad.all.Count > 0)
                    {
                        pad.cntrl = PadController.GamePad;
                    } else
                    {
                        pad.cntrl = PadController.KeyBoard;
                    }
                }
                else
                {
                    pad.cntrl = PadController.Environment;
                }
                break;
            case Side.Right:
                if (rightPlayer.controller == PlayerController.Player)
                {
                    if (Gamepad.all.Count > 1)
                    {
                        pad.cntrl = PadController.GamePad;
                    }
                    else if (Gamepad.all.Count > 0)
                    {
                        pad.cntrl = PadController.KeyBoard;
                    }
                    else
                    {
                        rightPlayer.controller = PlayerController.Environment;
                        pad.cntrl = PadController.Environment;
                    }
                }
                else
                {
                    pad.cntrl = PadController.Environment;
                }
                break;
        }
        pad.EnableControls();
        pad.col = pad.AddComponent<MeshCollider>();
        (pad.col as MeshCollider).sharedMesh = pad.meshF.mesh;
        (pad.col as MeshCollider).convex = true;
        pad.transform.SetParent(fieldParent.transform);
        pad.SetInitialPadPosition(fieldParent.GetComponentInChildren<Background>().col.bounds.size.x, pad.col.bounds.size.x);
        pad.projectile.transform.position = new Vector3(pad.projectile.transform.position.x, pad.projectile.transform.position.y, pad.col.bounds.size.z);
        pad.transform.rotation = Quaternion.Euler(0, side == Side.Left ? 90 : -90, 0);
        pad.meshR.material = mm.materials.padMaterial;
        pad.rbd.excludeLayers = rigidBodiesExcludeLayers.padExclude;
        return pad;
    }
    public BallEntity MakeFullBall(BallMesh bm)
    {
        BallEntity ball = GameObject.Instantiate(ballPrefab, menuCanvas.transform).GetComponent<BallEntity>();
        ball.ballType = bm;

        ball.gameObject.name = bm.ToString() + "Full";
        ball.meshF.mesh = mm.NewMesh(bm.ToString(), Vector3.one * canvasBallDiameter * pm.initialSizes.ballSizeMultiplier);
        if (bm == BallMesh.Cube)
        {
            ball.col = ball.AddComponent<BoxCollider>();
            (ball.col as BoxCollider).size = new Vector3(ball.meshF.mesh.bounds.size.x, ball.meshF.mesh.bounds.size.y, ball.meshF.mesh.bounds.size.z);
            if (pve.virtualField.ball.GetComponent<MeshRenderer>() != null)
            {
                Destroy(pve.virtualField.ball.GetComponent<MeshRenderer>());
            }
            if (pve.virtualField.ball.GetComponent<SphereCollider>() != null)
            {
                Destroy(pve.virtualField.ball.GetComponent<SphereCollider>());
            }
            if (pve.virtualField.ball.GetComponent<BoxCollider>() == null)
            {
                pve.virtualField.ball.AddComponent<BoxCollider>();
            }
        }
        else if (bm == BallMesh.Sphere)
        {
            ball.col = ball.AddComponent<SphereCollider>();
            (ball.col as SphereCollider).radius = ball.meshF.mesh.bounds.size.x * 0.5f;
            if (pve.virtualField.ball.GetComponent<MeshRenderer>() != null)
            {
                Destroy(pve.virtualField.ball.GetComponent<MeshRenderer>());
            }
            if (pve.virtualField.ball.GetComponent<BoxCollider>() != null)
            {
                Destroy(pve.virtualField.ball.GetComponent<BoxCollider>());
            }
            if (pve.virtualField.ball.GetComponent<SphereCollider>() == null)
            {
                pve.virtualField.ball.AddComponent<SphereCollider>();
            }
        }
        else
        {
            ball.col = ball.AddComponent<MeshCollider>();
            (ball.col as MeshCollider).sharedMesh = ball.meshF.mesh;
            (ball.col as MeshCollider).convex = true;
            if (pve.virtualField.ball.GetComponent<BoxCollider>() != null)
            {
                Destroy(pve.virtualField.ball.GetComponent<BoxCollider>());
            }
            if (pve.virtualField.ball.GetComponent<SphereCollider>() != null)
            {
                Destroy(pve.virtualField.ball.GetComponent<SphereCollider>());
            }
            if (pve.virtualField.ball.GetComponent<MeshCollider>() == null)
            {
                pve.virtualField.ball.AddComponent<MeshCollider>();
            }
        }
        ball.col.isTrigger = false;
        ball.col.sharedMaterial = bouncyMaterial;
        ball.transform.localPosition = Vector3.zero;
        ball.transform.SetParent(fieldParent.transform);
        ball.meshR.material = mm.materials.ballMaterials.MaterialForCurrentMesh(bm);
        ball.rbd.excludeLayers = rigidBodiesExcludeLayers.ballExclude;
        return ball;
    }
    public FragmentStore MakeFragmentStore()
    {
        List<Fragment> leftPadFragments = new List<Fragment>();
        List<Fragment> rightPadFragments = new List<Fragment>();
        List<Fragment> cubeFragments = new List<Fragment>();
        List<Fragment> icosahedronFreezeFragments = new List<Fragment>();
        List<Fragment> icosahedronBurnFragments = new List<Fragment>();
        List<Fragment> octacontagonFragments = new List<Fragment>();

        //make pad fragments
        for (int i = 0; i < 4; i++)
        {
            Fragment leftFrg = GameObject.Instantiate(fragmentPrefab).GetComponent<Fragment>();
            Fragment rightFrg = GameObject.Instantiate(fragmentPrefab).GetComponent<Fragment>();
            leftFrg.gameObject.name = "LeftPadFragment" + (i + 1);
            rightFrg.gameObject.name = "RightPadFragment" + (i + 1);

            leftPadFragments.Add(leftFrg);
            rightPadFragments.Add(rightFrg);

            leftFrg.meshF.mesh = mm.NewMesh("PadFragment" + (i + 1), Vector3.one * canvasPadWidth);
            rightFrg.meshF.mesh = mm.NewMesh("PadFragment" + (i + 1), Vector3.one * canvasPadWidth);

            leftFrg.col = leftFrg.AddComponent<MeshCollider>();
            rightFrg.col = rightFrg.AddComponent<MeshCollider>();
            (leftFrg.col as MeshCollider).sharedMesh = leftFrg.meshF.mesh;
            (rightFrg.col as MeshCollider).sharedMesh = rightFrg.meshF.mesh;

            (leftFrg.col as MeshCollider).convex = (rightFrg.col as MeshCollider).convex = true;
            leftFrg.col.isTrigger = rightFrg.col.isTrigger = false;
            leftFrg.col.sharedMaterial = rightFrg.col.sharedMaterial = notSoBouncyMaterial;
            leftFrg.meshR.material = rightFrg.meshR.material = mm.materials.padMaterial;
            leftFrg.gameObject.layer = rightFrg.gameObject.layer = LayerMask.NameToLayer("Fragment");

            leftFrg.gameObject.SetActive(false);
            rightFrg.gameObject.SetActive(false);

        }

        //make cube fragments
        for (int i = 0; i < 4; i++)
        {
            Fragment cubeFrg = GameObject.Instantiate(fragmentPrefab).GetComponent<Fragment>();
            cubeFrg.gameObject.name = "CubeFragment" + (i + 1);
            cubeFrg.meshF.mesh = mm.NewMesh(cubeFrg.gameObject.name, Vector3.one * canvasBallDiameter * pm.initialSizes.ballSizeMultiplier);
            cubeFrg.col = cubeFrg.AddComponent<MeshCollider>();
            (cubeFrg.col as MeshCollider).sharedMesh = cubeFrg.meshF.mesh;
            (cubeFrg.col as MeshCollider).convex = true;
            cubeFrg.col.isTrigger = false;
            cubeFrg.col.sharedMaterial = notSoBouncyMaterial;
            cubeFrg.meshR.material = mm.materials.ballFragmentsMaterials.cubeFragmentsMaterials[i];
            cubeFragments.Add(cubeFrg);
            cubeFrg.gameObject.layer = LayerMask.NameToLayer("Fragment");
            cubeFrg.gameObject.SetActive(false);
        }

        //make icosahedron fragments
        for (int i = 0; i < 20; i++)
        {
            Fragment icosahedronFrg = GameObject.Instantiate(fragmentPrefab).GetComponent<Fragment>();
            icosahedronFrg.gameObject.name = "IcosahedronFragment" + (i + 1);
            icosahedronFrg.meshF.mesh = mm.NewMesh(icosahedronFrg.gameObject.name, Vector3.one * canvasBallDiameter * pm.initialSizes.ballSizeMultiplier);
            icosahedronFrg.col = icosahedronFrg.AddComponent<MeshCollider>();
            (icosahedronFrg.col as MeshCollider).sharedMesh = icosahedronFrg.meshF.mesh;
            (icosahedronFrg.col as MeshCollider).convex = true;
            icosahedronFrg.col.isTrigger = false;
            icosahedronFrg.col.sharedMaterial = notSoBouncyMaterial;
            if ((i + 1) % 2 == 0)
            {
                icosahedronFrg.meshR.material = mm.materials.ballFragmentsMaterials.burnFragmentMaterial;
                icosahedronBurnFragments.Add(icosahedronFrg);
            }
            else
            {
                icosahedronFrg.meshR.material = mm.materials.ballFragmentsMaterials.freezeFragmentMaterial;
                icosahedronFreezeFragments.Add(icosahedronFrg);
            }
            icosahedronFrg.gameObject.layer = LayerMask.NameToLayer("Fragment");
            icosahedronFrg.gameObject.SetActive(false);
        }

        //make octacontagon fragments
        for (int i = 0; i < 4; i++)
        {
            Fragment octacontagonFrg = GameObject.Instantiate(fragmentPrefab).GetComponent<Fragment>();
            octacontagonFrg.gameObject.name = "OctacontagonFragment" + (i + 1);
            octacontagonFrg.meshF.mesh = mm.NewMesh(octacontagonFrg.gameObject.name, Vector3.one * canvasBallDiameter * pm.initialSizes.ballSizeMultiplier);
            octacontagonFrg.col = octacontagonFrg.AddComponent<MeshCollider>();
            (octacontagonFrg.col as MeshCollider).sharedMesh = octacontagonFrg.meshF.mesh;
            (octacontagonFrg.col as MeshCollider).convex = true;
            octacontagonFrg.col.isTrigger = false;
            octacontagonFrg.col.sharedMaterial = notSoBouncyMaterial;
            octacontagonFrg.meshR.material = mm.materials.ballFragmentsMaterials.ballFragmentMaterial;
            octacontagonFragments.Add(octacontagonFrg);
            octacontagonFrg.gameObject.layer = LayerMask.NameToLayer("Fragment");
            octacontagonFrg.gameObject.SetActive(false);
        }

        FragmentStore newFragmentStore = new FragmentStore(leftPadFragments, rightPadFragments, cubeFragments, icosahedronFreezeFragments, icosahedronBurnFragments, octacontagonFragments);
        return newFragmentStore;
    }
    public SpikeEntity MakeSpike(SpikeType t, SpikeMesh sm)
    {
        SpikeEntity spike = GameObject.Instantiate(spikePrefabs[(int)t], menuCanvas.transform).GetComponent<SpikeEntity>();
        spike.meshF.mesh = mm.NewMesh(sm.ToString(), Vector3.one * canvasSpikeDiameter);

        spike.transform.localPosition = Vector3.zero;
        spike.gameObject.name = t.ToString() + "-" + sm.ToString();
        spike.col = spike.AddComponent<MeshCollider>();
        (spike.col as MeshCollider).sharedMesh = spike.meshF.mesh;
        (spike.col as MeshCollider).convex = true;
        spike.col.isTrigger = false;
        spike.col.sharedMaterial = bouncyMaterial;
        spike.transform.SetParent(fieldParent.transform);
        spike.spikeMesh = sm;
        spike.meshR.material = mm.materials.spikeMaterials.MaterialForCurrentMesh(t);
        spike.rbd.excludeLayers = rigidBodiesExcludeLayers.spikeExclude;
        return spike;
    }
    public SpikeStore MakeSpikeStore()
    {
        SpikeStore newSpikeStore = new SpikeStore();
        newSpikeStore.spikePadPiecePair = new SpikePair(MakeSpike(SpikeType.SpikePadPiece, SpikeMesh.AddPiece), "spikePadPiecePair");
        newSpikeStore.spikePadBlockPair = new SpikePair(MakeSpike(SpikeType.SpikePadBlock, SpikeMesh.AddBlock), "spikePadBlockPair");
        newSpikeStore.spikeDissolvePair = new SpikePair(MakeSpike(SpikeType.SpikeDissolve, SpikeMesh.Dissolve), "spikeDissolvePair");
        newSpikeStore.spikeRandomDirectionPair = new SpikePair(MakeSpike(SpikeType.SpikeRandomDirection, SpikeMesh.RandomDirection), "spikeRandomDirectionPair");
        newSpikeStore.spikeWallAttractorPair = new SpikePair(MakeSpike(SpikeType.SpikeWallAttractor, SpikeMesh.RandomDirection), "spikeWallAttractorPair");
        newSpikeStore.spikeMagnetPair = new SpikePair(MakeSpike(SpikeType.SpikeMagnet, SpikeMesh.Magnet), "spikeMagnetPair");
        newSpikeStore.spikeHealthUpPair = new SpikePair(MakeSpike(SpikeType.HealthUp, SpikeMesh.AddPiece), "spikeHealthUpPair");
        newSpikeStore.activeSpikes = new SpikePair(null, "activeSpikes");
        newSpikeStore.lastActiveSpikes = newSpikeStore.spikePadPiecePair;
        newSpikeStore.lastActiveSpikes.name = "lastActiveSpikes";
        return newSpikeStore;
    }
    public DebuffStore MakeDebuffStore()
    {
        DebuffStore newDebuffStore = new DebuffStore();
        newDebuffStore.debuffFreeze = MakeDebuff(DebuffType.DebuffFreeze) as DebuffFreeze;
        newDebuffStore.debuffBurn = MakeDebuff(DebuffType.DebuffBurn) as DebuffBurn;
        newDebuffStore.debuffSlow = MakeDebuff(DebuffType.DebuffSlow) as DebuffSlow;
        return newDebuffStore;
    }
    public DebuffEntity MakeDebuff(DebuffType t)
    {
        DebuffEntity debuff;
        switch (t)
        {
            default:
                debuff = GameObject.Instantiate(debuffPrefabs[(int)t], menuCanvas.transform).GetComponent<DebuffEntity>();
                debuff.meshF.mesh = mm.NewMesh("Octacontagon", Vector3.one * canvasDebuffDiameter);// * canvasSpikeDiameter);
                debuff.col = debuff.AddComponent<MeshCollider>();
                (debuff.col as MeshCollider).sharedMesh = debuff.meshF.mesh;
                (debuff.col as MeshCollider).convex = true;
                debuff.transform.localPosition = Vector3.zero;
                debuff.gameObject.name = t.ToString();
                debuff.col.isTrigger = false;
                debuff.col.sharedMaterial = bouncyMaterial;
                debuff.transform.SetParent(fieldParent.transform);
                debuff.rbd.excludeLayers = rigidBodiesExcludeLayers.debuffExclude;
                return debuff;
            case DebuffType.DebuffSlow:
#if UNITY_WEBGL
                debuff = GameObject.Instantiate(webDebuffSlowPrefab, menuCanvas.transform).GetComponent<DebuffEntity>();
#else
                debuff = GameObject.Instantiate(debuffPrefabs[(int)t], menuCanvas.transform).GetComponent<DebuffEntity>();
#endif
                debuff.transform.localScale *= canvasDebuffDiameter;
                debuff.transform.localPosition = Vector3.zero;
                debuff.gameObject.name = t.ToString();
                (debuff.col as SphereCollider).radius = 5f; //DistortionSphere sphere in VFX has a 10 diameter
                debuff.col.isTrigger = true;
                debuff.col.sharedMaterial = null;
                debuff.transform.SetParent(fieldParent.transform);
                return debuff;
        }
    }
    public Block MakeBlock(Side side, Pad pad)
    {
        Block block = GameObject.Instantiate(blockPrefab, fieldParent.transform).GetComponent<Block>();
        block.meshF.mesh = mm.NewMesh("PadBlock", Vector3.one * sizes.padWidth);
        block.gameObject.name = side + "-PadBlock-" + pad.sd;
        block.col = block.AddComponent<MeshCollider>();
        (block.col as MeshCollider).sharedMesh = block.meshF.mesh;
        (block.col as MeshCollider).convex = true;
        block.col.isTrigger = false;
        block.sd = side;
        block.pad = pad;
        float startingPoint = side == Side.Top ? field.background.col.bounds.size.y * 0.5f - block.col.bounds.size.y * 0.5f : -(field.background.col.bounds.size.y * 0.5f - block.col.bounds.size.y * 0.5f);
        block.transform.localPosition = new Vector3(pad.transform.position.x, startingPoint, stagePosZ);
        block.meshR.material = mm.materials.blockMaterial;
        block.transform.localScale = currentStage == Stage.FreeMove ? new Vector3(block.transform.localScale.x, block.transform.localScale.y, block.transform.localScale.z * sizes.fieldDepth) : block.transform.localScale;
        return block;
    }

    public void MakeEnergyShield(Pad pad)
    {
        Fragment energyShield = GameObject.Instantiate(fragmentPrefab, pad.transform).GetComponent<Fragment>();
        energyShield.gameObject.name = pad.gameObject.name + "EnergyShield";
        energyShield.gameObject.layer = pad.gameObject.layer;

        energyShield.meshF.mesh = mm.NewMesh("EnergyShield", Vector3.one * canvasPadWidth);
        float xMultiplier = pad.meshF.mesh.bounds.size.y / (canvasPadWidth * 3) > 1 ? pad.meshF.mesh.bounds.size.y / (canvasPadWidth * 3) * 0.1f : 0;
        energyShield.transform.localScale = new Vector3(energyShield.transform.localScale.x, energyShield.transform.localScale.y+xMultiplier, energyShield.transform.localScale.z);

        energyShield.col = energyShield.AddComponent<MeshCollider>();
        (energyShield.col as MeshCollider).sharedMesh = energyShield.meshF.mesh;
        (energyShield.col as MeshCollider).convex = true;
        energyShield.col.isTrigger = false;
        energyShield.col.sharedMaterial = bouncyMaterial;
        energyShield.meshR.material = vfx.energyShieldMaterial;
        energyShield.transform.localPosition = new Vector3(0, 0, pad.col.bounds.size.z / pad.transform.localScale.z);
        energyShield.transform.localScale *= 1.5f;
        energyShield.meshR.material.SetFloat("_Alpha", 0);
        pad.energyShield = energyShield.gameObject;
    }
}
