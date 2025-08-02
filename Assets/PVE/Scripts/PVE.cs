using UnityEngine;
using UnityEngine.SceneManagement;
public class PVE : PongManager
{
    public bool roundOn = false;
    public bool pveActive = false;
    public Scene simulationScene;
    public PhysicsScene physicsScene;
    public PvePredictionLines renderSimulationLines;
    public LineRenderer ballLineRenderer;
    public LineRenderer leftSpikeLineRenderer;
    public LineRenderer rightSpikeLineRenderer;
    public bool simulating = false;
    public bool predict = false;
    public VirtualField virtualField;
    bool skipSimulation = false;
    int skippedFrames = 0;
    void OnEnable()
    {
        newGameManager.roundStarted.AddListener(() => roundOn = true);
        newGameManager.roundStopped.AddListener(() => { roundOn = false; predict = false; });
        virtualField = new VirtualField();
        newGameManager.spawnedSpikes.AddListener(() => OnSpikeSpawned());
    }
    void OnSpikeSpawned()
    {
        virtualField.CopyMeshCollider(field.spikeStore.lastActiveSpikes.spikeLeft.col as MeshCollider, virtualField.leftSpike.GetComponent<MeshCollider>());
        virtualField.CopyMeshCollider(field.spikeStore.lastActiveSpikes.spikeRight.col as MeshCollider, virtualField.rightSpike.GetComponent<MeshCollider>());
        skippedFrames = 0;
        skipSimulation = false;
    }
    void FixedUpdate()
    {
        if (roundOn && pveActive)
        {
            if (!predict)
            {
                predict = true;
            }
            if (predict && !simulating && !skipSimulation)
            {
                SimulateTrajectory();
            }
            if (skipSimulation)
            {
                skippedFrames += 1;
                if (skippedFrames >= (currentStage == Stage.FreeMove ? 200 : 100))
                {
                    skippedFrames = 0;
                    skipSimulation = false;
                }
            }
        }
    }
    public void StartPVE()
    {
        pveActive = true;
        CreatePhysicsScene();
    }
    void UpdateVirtualFieldScale()
    {
        virtualField.ball.transform.localScale = field.ball.transform.localScale;
        virtualField.leftPad.transform.localScale = field.leftPad.transform.localScale;
        virtualField.rightPad.transform.localScale = field.rightPad.transform.localScale;
        virtualField.leftSpike.transform.localScale = field.spikeStore.lastActiveSpikes.spikeLeft.transform.localScale;
        virtualField.rightSpike.transform.localScale = field.spikeStore.lastActiveSpikes.spikeRight.transform.localScale;
        virtualField.debuffBurn.transform.localScale = field.debuffStore.debuffBurn.transform.localScale;
        virtualField.debuffFreeze.transform.localScale = field.debuffStore.debuffFreeze.transform.localScale;
        virtualField.edges[0].transform.localScale = field.topFloor.transform.localScale;
        virtualField.edges[1].transform.localScale = field.bottomFloor.transform.localScale;
        virtualField.edges[2].transform.localScale = field.leftWall.transform.localScale;
        virtualField.edges[3].transform.localScale = field.rightWall.transform.localScale;
        virtualField.edges[4].transform.localScale = field.background.transform.localScale;
        virtualField.blocks[0].transform.localScale = field.leftPad.topBlock ? field.leftPad.topBlock.transform.localScale : field.leftPad.transform.localScale;
        virtualField.blocks[1].transform.localScale = field.leftPad.bottomBlock ? field.leftPad.bottomBlock.transform.localScale : field.leftPad.transform.localScale;
        virtualField.blocks[2].transform.localScale = field.rightPad.topBlock ? field.rightPad.topBlock.transform.localScale : field.rightPad.transform.localScale;
        virtualField.blocks[3].transform.localScale = field.rightPad.bottomBlock ? field.rightPad.bottomBlock.transform.localScale : field.rightPad.transform.localScale;
    }
    void UpdateVirtualEdgesPostions()
    {
        virtualField.edges[0].transform.position = field.topFloor.transform.position;
        virtualField.edges[0].transform.rotation = field.topFloor.transform.rotation;
        virtualField.edges[1].transform.position = field.bottomFloor.transform.position;
        virtualField.edges[1].transform.rotation = field.bottomFloor.transform.rotation;
        virtualField.edges[2].transform.position = field.leftWall.transform.position;
        virtualField.edges[2].transform.rotation = field.leftWall.transform.rotation;
        virtualField.edges[3].transform.position = field.rightWall.transform.position;
        virtualField.edges[3].transform.rotation = field.rightWall.transform.rotation;
        virtualField.edges[4].transform.position = field.background.transform.position;
        virtualField.edges[4].transform.rotation = field.background.transform.rotation;

        virtualField.leftPad.transform.rotation = field.leftPad.transform.rotation;
        virtualField.rightPad.transform.rotation = field.rightPad.transform.rotation;
    }
    void UpdateVirtualEntitiesPositions()
    {
        virtualField.ball.transform.position = field.ball.transform.position;
        virtualField.leftPad.transform.position = field.leftPad.transform.position;
        virtualField.rightPad.transform.position = field.rightPad.transform.position;

        if (field.spikeStore.lastActiveSpikes.spikeLeft.gameObject.activeSelf)
        {
            virtualField.leftSpike.transform.position = field.spikeStore.lastActiveSpikes.spikeLeft.transform.position;
        }
        if (field.spikeStore.lastActiveSpikes.spikeRight.gameObject.activeSelf)
        {
            virtualField.rightSpike.transform.position = field.spikeStore.lastActiveSpikes.spikeRight.transform.position;
        }

        if (field.debuffStore.debuffBurn.gameObject.activeSelf)
        {
            virtualField.debuffBurn.transform.position = field.debuffStore.debuffBurn.transform.position;
        }
        if (field.debuffStore.debuffFreeze.gameObject.activeSelf)
        {
            virtualField.debuffFreeze.transform.position = field.debuffStore.debuffFreeze.transform.position;
        }
        if (field.leftPad.topBlock != null)
        {
            virtualField.blocks[0].transform.position = field.leftPad.topBlock.transform.position;
        }
        if (field.leftPad.bottomBlock != null)
        {
            virtualField.blocks[1].transform.position = field.leftPad.bottomBlock.transform.position;
        }
        if (field.rightPad.topBlock != null)
        {
            virtualField.blocks[2].transform.position = field.rightPad.topBlock.transform.position;
        }
        if (field.rightPad.bottomBlock != null)
        {
            virtualField.blocks[3].transform.position = field.rightPad.bottomBlock.transform.position;
        }
    }
    void AssignVirtualEdgesColliderValues()
    {
        virtualField.CopyBoxCollider(field.topFloor.col as BoxCollider, virtualField.edges[0].GetComponent<BoxCollider>());
        virtualField.CopyBoxCollider(field.bottomFloor.col as BoxCollider, virtualField.edges[1].GetComponent<BoxCollider>());
        virtualField.CopyBoxCollider(field.leftWall.col as BoxCollider, virtualField.edges[2].GetComponent<BoxCollider>());
        virtualField.CopyBoxCollider(field.rightWall.col as BoxCollider, virtualField.edges[3].GetComponent<BoxCollider>());
        virtualField.CopyBoxCollider(field.background.col as BoxCollider, virtualField.edges[4].GetComponent<BoxCollider>());

    }
    void AssignVirtualEntitiesComponentValues()
    {
        virtualField.CopyRigidBody(field.ball.rbd, virtualField.ball.GetComponent<Rigidbody>());
        virtualField.CopyRigidBody(field.leftPad.rbd, virtualField.leftPad.GetComponent<Rigidbody>());
        virtualField.CopyRigidBody(field.rightPad.rbd, virtualField.rightPad.GetComponent<Rigidbody>());
        virtualField.CopyMeshCollider(field.leftPad.col as MeshCollider, virtualField.leftPad.GetComponent<MeshCollider>());
        virtualField.CopyMeshCollider(field.rightPad.col as MeshCollider, virtualField.rightPad.GetComponent<MeshCollider>());
        if ((field.ball.ballType != BallMesh.Cube && field.ball.ballType != BallMesh.Sphere) || (field.ball.ballType == BallMesh.Cube && !field.fragmentStore.ballFragmentsEmpty))
        {
            virtualField.CopyMeshCollider(field.ball.col as MeshCollider, virtualField.ball.GetComponent<MeshCollider>());
        }
        else if (field.ball.ballType == BallMesh.Cube)
        {
            virtualField.CopyBoxCollider(field.ball.col as BoxCollider, virtualField.ball.GetComponent<BoxCollider>());
        }
        else if (field.ball.ballType == BallMesh.Sphere)
        {
            virtualField.CopySphereCollider(field.ball.col as SphereCollider, virtualField.ball.GetComponent<SphereCollider>());
        }

        virtualField.CopyRigidBody(field.spikeStore.lastActiveSpikes.spikeLeft.rbd, virtualField.leftSpike.GetComponent<Rigidbody>());
        virtualField.CopyRigidBody(field.spikeStore.lastActiveSpikes.spikeRight.rbd, virtualField.rightSpike.GetComponent<Rigidbody>());

        if (field.debuffStore.debuffBurn.gameObject.activeSelf && !field.debuffStore.debuffBurn.orbiting)
        {
            virtualField.CopyMeshCollider(field.debuffStore.debuffBurn.col as MeshCollider, virtualField.debuffBurn.GetComponent<MeshCollider>(), builder.storedMeshForVirtualDebuffs);
            virtualField.CopyRigidBody(field.debuffStore.debuffBurn.rbd, virtualField.debuffBurn.GetComponent<Rigidbody>());
        }
        if (field.debuffStore.debuffFreeze.gameObject.activeSelf && !field.debuffStore.debuffFreeze.orbiting)
        {
            virtualField.CopyMeshCollider(field.debuffStore.debuffFreeze.col as MeshCollider, virtualField.debuffFreeze.GetComponent<MeshCollider>(), builder.storedMeshForVirtualDebuffs);
            virtualField.CopyRigidBody(field.debuffStore.debuffFreeze.rbd, virtualField.debuffFreeze.GetComponent<Rigidbody>());
        }

        if (field.leftPad.topBlock != null)
        {
            virtualField.CopyMeshCollider(field.leftPad.topBlock.col as MeshCollider, virtualField.blocks[0].GetComponent<MeshCollider>());
        }
        if (field.leftPad.bottomBlock != null)
        {
            virtualField.CopyMeshCollider(field.leftPad.bottomBlock.col as MeshCollider, virtualField.blocks[1].GetComponent<MeshCollider>());
        }
        if (field.rightPad.topBlock != null)
        {
            virtualField.CopyMeshCollider(field.rightPad.topBlock.col as MeshCollider, virtualField.blocks[2].GetComponent<MeshCollider>());
        }
        if (field.rightPad.bottomBlock != null)
        {
            virtualField.CopyMeshCollider(field.rightPad.bottomBlock.col as MeshCollider, virtualField.blocks[3].GetComponent<MeshCollider>());
        }
        if (currentStage == Stage.FreeMove)
        {
            virtualField.ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
    }
    void StartMovingEntities()
    {
        virtualField.ball.GetComponent<VirtualEntity>().live = true;
        virtualField.leftSpike.GetComponent<VirtualEntity>().live = true;
        virtualField.rightSpike.GetComponent<VirtualEntity>().live = true;
        virtualField.debuffBurn.GetComponent<VirtualEntity>().live = true;
        virtualField.debuffFreeze.GetComponent<VirtualEntity>().live = true;

        virtualField.ball.GetComponent<Rigidbody>().maxLinearVelocity = field.ball.rbd.maxLinearVelocity;
        virtualField.ball.GetComponent<Rigidbody>().maxAngularVelocity = field.ball.rbd.maxAngularVelocity;
        virtualField.ball.GetComponent<Rigidbody>().maxDepenetrationVelocity = field.ball.rbd.maxDepenetrationVelocity;

        virtualField.ball.GetComponent<Rigidbody>().isKinematic = false;
        virtualField.ball.GetComponent<Rigidbody>().angularVelocity = field.ball.rbd.angularVelocity;
        virtualField.ball.GetComponent<Rigidbody>().velocity = field.ball.rbd.velocity;
        virtualField.ball.GetComponent<Rigidbody>().AddForce(field.ball.rbd.velocity, ForceMode.VelocityChange);
        virtualField.ball.transform.rotation = field.ball.transform.rotation;
        virtualField.ball.GetComponent<Rigidbody>().angularVelocity = field.ball.rbd.angularVelocity;
        virtualField.ball.GetComponent<ConstantForce>().force = field.ball.cf.force;

        if (field.spikeStore.lastActiveSpikes.spikeLeft.gameObject.activeSelf)
        {
            virtualField.leftSpike.GetComponent<Rigidbody>().maxLinearVelocity = field.spikeStore.lastActiveSpikes.spikeLeft.rbd.maxLinearVelocity;
            virtualField.leftSpike.GetComponent<Rigidbody>().maxAngularVelocity = field.spikeStore.lastActiveSpikes.spikeLeft.rbd.maxAngularVelocity;
            virtualField.leftSpike.GetComponent<Rigidbody>().maxDepenetrationVelocity = field.spikeStore.lastActiveSpikes.spikeLeft.rbd.maxDepenetrationVelocity;

            virtualField.leftSpike.GetComponent<Rigidbody>().isKinematic = false;
            virtualField.leftSpike.GetComponent<Rigidbody>().velocity = field.spikeStore.lastActiveSpikes.spikeLeft.rbd.velocity;
            virtualField.leftSpike.GetComponent<Rigidbody>().AddForce(field.spikeStore.lastActiveSpikes.spikeLeft.rbd.velocity, ForceMode.VelocityChange);
            virtualField.leftSpike.transform.rotation = field.spikeStore.lastActiveSpikes.spikeLeft.transform.rotation;
            virtualField.leftSpike.GetComponent<Rigidbody>().angularVelocity = field.spikeStore.lastActiveSpikes.spikeLeft.rbd.angularVelocity;
        }
        if (field.spikeStore.lastActiveSpikes.spikeRight.gameObject.activeSelf)
        {
            virtualField.rightSpike.GetComponent<Rigidbody>().maxLinearVelocity = field.spikeStore.lastActiveSpikes.spikeRight.rbd.maxLinearVelocity;
            virtualField.rightSpike.GetComponent<Rigidbody>().maxAngularVelocity = field.spikeStore.lastActiveSpikes.spikeRight.rbd.maxAngularVelocity;
            virtualField.rightSpike.GetComponent<Rigidbody>().maxDepenetrationVelocity = field.spikeStore.lastActiveSpikes.spikeRight.rbd.maxDepenetrationVelocity;

            virtualField.rightSpike.GetComponent<Rigidbody>().isKinematic = false;
            virtualField.rightSpike.GetComponent<Rigidbody>().angularVelocity = field.spikeStore.lastActiveSpikes.spikeRight.rbd.angularVelocity;
            virtualField.rightSpike.GetComponent<Rigidbody>().velocity = field.spikeStore.lastActiveSpikes.spikeRight.rbd.velocity;
            virtualField.rightSpike.GetComponent<Rigidbody>().AddForce(field.spikeStore.lastActiveSpikes.spikeRight.rbd.velocity, ForceMode.VelocityChange);
            virtualField.rightSpike.transform.rotation = field.spikeStore.lastActiveSpikes.spikeRight.transform.rotation;
            virtualField.rightSpike.GetComponent<Rigidbody>().angularVelocity = field.spikeStore.lastActiveSpikes.spikeRight.rbd.angularVelocity;
        }

        if (field.debuffStore.debuffBurn.gameObject.activeSelf)
        {
            virtualField.debuffBurn.GetComponent<Rigidbody>().isKinematic = false;
            virtualField.debuffBurn.GetComponent<Rigidbody>().velocity = field.debuffStore.debuffBurn.rbd.velocity;
            virtualField.debuffBurn.GetComponent<Rigidbody>().AddForce(field.debuffStore.debuffBurn.rbd.velocity, ForceMode.VelocityChange);
            virtualField.debuffBurn.transform.rotation = field.debuffStore.debuffBurn.transform.rotation;
            virtualField.debuffBurn.GetComponent<Rigidbody>().angularVelocity = field.debuffStore.debuffBurn.rbd.angularVelocity;
        }
        if (field.debuffStore.debuffFreeze.gameObject.activeSelf)
        {
            virtualField.debuffFreeze.GetComponent<Rigidbody>().isKinematic = false;
            virtualField.debuffFreeze.GetComponent<Rigidbody>().velocity = field.debuffStore.debuffFreeze.rbd.velocity;
            virtualField.debuffFreeze.GetComponent<Rigidbody>().AddForce(field.debuffStore.debuffFreeze.rbd.velocity, ForceMode.VelocityChange);
            virtualField.debuffFreeze.transform.rotation = field.debuffStore.debuffFreeze.transform.rotation;
            virtualField.debuffFreeze.GetComponent<Rigidbody>().angularVelocity = field.debuffStore.debuffFreeze.rbd.angularVelocity;
        }
    }
    void VirtualFieldLive()
    {
        virtualField.ball.gameObject.SetActive(true);
        virtualField.leftPad.gameObject.SetActive(true);
        virtualField.rightPad.gameObject.SetActive(true);

        if (field.spikeStore.lastActiveSpikes.spikeLeft.gameObject.activeSelf)
        {
            virtualField.leftSpike.gameObject.SetActive(true);
        }
        if (field.spikeStore.lastActiveSpikes.spikeRight.gameObject.activeSelf)
        {
            virtualField.rightSpike.gameObject.SetActive(true);
        }

        if (field.debuffStore.debuffBurn.gameObject.activeSelf)
        {
            virtualField.debuffBurn.gameObject.SetActive(true);
        }
        if (field.debuffStore.debuffFreeze.gameObject.activeSelf)
        {
            virtualField.debuffFreeze.gameObject.SetActive(true);
        }
        if (field.leftPad.topBlock != null)
        {
            virtualField.blocks[0].SetActive(true);
        }
        if (field.leftPad.bottomBlock != null)
        {
            virtualField.blocks[1].SetActive(true);
        }
        if (field.rightPad.topBlock != null)
        {
            virtualField.blocks[2].SetActive(true);
        }
        if (field.rightPad.bottomBlock != null)
        {
            virtualField.blocks[3].SetActive(true);
        }
        foreach (GameObject edge in virtualField.edges)
        {
            edge.gameObject.SetActive(true);
        }
        if (currentStage == Stage.FreeMove)
        {
            virtualField.foreGround.gameObject.SetActive(true);
        }
    }
    void VirtualFieldDead()
    {
        virtualField.ball.GetComponent<VirtualEntity>().live = false;
        virtualField.leftSpike.GetComponent<VirtualEntity>().live = false;
        virtualField.rightSpike.GetComponent<VirtualEntity>().live = false;
        virtualField.debuffBurn.GetComponent<VirtualEntity>().live = false;
        virtualField.debuffFreeze.GetComponent<VirtualEntity>().live = false;

        virtualField.ball.GetComponent<Rigidbody>().isKinematic = false;
        virtualField.ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        virtualField.leftSpike.GetComponent<Rigidbody>().velocity = Vector3.zero;
        virtualField.rightSpike.GetComponent<Rigidbody>().velocity = Vector3.zero;
        virtualField.debuffBurn.GetComponent<Rigidbody>().velocity = Vector3.zero;
        virtualField.debuffFreeze.GetComponent<Rigidbody>().velocity = Vector3.zero;
        virtualField.ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        virtualField.leftSpike.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        virtualField.rightSpike.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        virtualField.debuffBurn.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        virtualField.debuffFreeze.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        virtualField.ball.GetComponent<Rigidbody>().isKinematic = true;
        virtualField.ball.gameObject.SetActive(false);
        virtualField.leftPad.gameObject.SetActive(false);
        virtualField.rightPad.gameObject.SetActive(false);
        virtualField.leftSpike.gameObject.SetActive(false);
        virtualField.rightSpike.gameObject.SetActive(false);
        virtualField.debuffBurn.gameObject.SetActive(false);
        virtualField.debuffFreeze.gameObject.SetActive(false);
        foreach (GameObject block in virtualField.blocks)
        {
            block.gameObject.SetActive(false);
        }
        foreach (GameObject edge in virtualField.edges)
        {
            edge.gameObject.SetActive(false);
        }
        virtualField.foreGround.gameObject.SetActive(false);
    }
    void AssignLayers()
    {
        virtualField.ball.gameObject.layer = virtualField.leftSpike.gameObject.layer = virtualField.rightSpike.gameObject.layer = field.ball.gameObject.layer;
        virtualField.leftPad.gameObject.layer = field.leftPad.gameObject.layer;
        virtualField.debuffBurn.gameObject.layer = field.debuffStore.debuffBurn.gameObject.layer;
        virtualField.debuffFreeze.gameObject.layer = field.debuffStore.debuffFreeze.gameObject.layer;
        virtualField.rightPad.gameObject.gameObject.layer = field.rightPad.gameObject.layer;
        if (field.leftPad.topBlock != null)
        {
            virtualField.blocks[0].gameObject.layer = field.leftPad.topBlock.gameObject.layer;
        }
        if (field.leftPad.bottomBlock != null)
        {
            virtualField.blocks[1].gameObject.layer = field.leftPad.bottomBlock.gameObject.layer;
        }
        if (field.rightPad.topBlock != null)
        {
            virtualField.blocks[2].gameObject.layer = field.rightPad.topBlock.gameObject.layer;
        }
        if (field.rightPad.bottomBlock != null)
        {
            virtualField.blocks[3].gameObject.layer = field.rightPad.bottomBlock.gameObject.layer;
        }
    }
    public void SetVirutalFieldForStage()
    {
        VirtualFieldDead();
        UpdateVirtualEdgesPostions();
        UpdateVirtualEntitiesPositions();
        AssignVirtualEdgesColliderValues();
        AssignVirtualEntitiesComponentValues();
        UpdateVirtualFieldScale();
        AssignLayers();
        skippedFrames = 0;
        skipSimulation = false;
    }
    void CreatePhysicsScene()
    {
        if (SceneManager.GetSceneByName("Simulation").IsValid() == false)
        {
            simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        }
        physicsScene = simulationScene.GetPhysicsScene();
        SceneManager.MoveGameObjectToScene(virtualField.parent, simulationScene);
    }
    void SimulateTrajectory()
    {
        simulating = true;
        UpdateVirtualEntitiesPositions();
        VirtualFieldLive();
        int positionCount = currentStage == Stage.FreeMove ? 200 : 100;
        if (renderSimulationLines.ball)
        {
            ballLineRenderer.positionCount = positionCount;
        }
        if (renderSimulationLines.leftSpike)
        {
            leftSpikeLineRenderer.positionCount = positionCount;
        }
        if (renderSimulationLines.rightSpike)
        {
            rightSpikeLineRenderer.positionCount = positionCount;
        }

        StartMovingEntities();

        // RUN SIMULATION
        for (var i = 0; i < positionCount; i++)
        {

            physicsScene.Simulate(Time.fixedDeltaTime);
            if (renderSimulationLines.ball) { ballLineRenderer.SetPosition(i, virtualField.ball.transform.position); }
            if (renderSimulationLines.leftSpike) { leftSpikeLineRenderer.SetPosition(i, virtualField.leftSpike.transform.position); }
            if (renderSimulationLines.rightSpike) { rightSpikeLineRenderer.SetPosition(i, virtualField.rightSpike.transform.position); }
            if (!virtualField.ball.gameObject.activeSelf)
            {
                if (renderSimulationLines.ball) { ballLineRenderer.positionCount = i; }
                if (renderSimulationLines.leftSpike) { leftSpikeLineRenderer.positionCount = i; }
                if (renderSimulationLines.rightSpike) { rightSpikeLineRenderer.positionCount = i; }
                VirtualFieldDead();
                simulating = false;
                return;
            }
        }
        VirtualFieldDead();
        skipSimulation = true;
        simulating = false;
    }
}
