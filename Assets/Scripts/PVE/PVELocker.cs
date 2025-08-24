using System.Collections.Generic;
using UnityEngine;
namespace PongGame.PVELocker
{
    public enum VirtualEntityType
    {
        Ball,
        Spike,
        Debuff
    }
    [System.Serializable]
    public class PvePredictionLines
    {
        public bool leftSpike = false;
        public bool rightSpike = false;
        public bool ball = false;
    }
    public class VirtualField
    {
        public GameObject parent;
        public GameObject ball;
        public GameObject leftPad;
        public GameObject rightPad;
        public GameObject leftSpike;
        public GameObject rightSpike;
        public GameObject debuffBurn;
        public GameObject debuffFreeze;
        public GameObject foreGround;
        public List<GameObject> edges;
        public List<GameObject> blocks;
        public List<GameObject> movingObjects;

        public VirtualField()
        {
            this.parent = new GameObject("Parent");
            this.parent.transform.position = Vector3.zero;
            this.parent.transform.rotation = Quaternion.identity;
            this.ball = new GameObject("Ball");
            this.leftPad = new GameObject("LeftPad");
            this.rightPad = new GameObject("RightPad");
            this.leftSpike = new GameObject("leftSpike");
            this.rightSpike = new GameObject("RightSpike");
            this.debuffBurn = new GameObject("DebuffBurn");
            this.debuffFreeze = new GameObject("DebuffFreeze");
            this.foreGround = new GameObject("ForeGround");
            blocks = new List<GameObject>(4) { new GameObject("TopLeftBlock"), new GameObject("BottomLeftBlock"), new GameObject("TopRightBlock"), new GameObject("BottomRightBlock") };
            edges = new List<GameObject>(5) { new GameObject("TopFloor"), new GameObject("BottomFloor"), new GameObject("LeftWall"), new GameObject("RightWall"), new GameObject("Background") };
            movingObjects = new List<GameObject>(7) { ball, leftPad, rightPad, leftSpike, rightSpike, debuffBurn, debuffFreeze };
            foreach (GameObject obj in movingObjects)
            {
                obj.transform.SetParent(parent.transform);
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = Quaternion.identity;
                obj.AddComponent<MeshCollider>();
                obj.AddComponent<Rigidbody>().isKinematic = false;
            }
            leftPad.GetComponent<Rigidbody>().isKinematic = true;
            rightPad.GetComponent<Rigidbody>().isKinematic = true;
            foreach (GameObject block in blocks)
            {
                block.transform.SetParent(parent.transform);
                block.transform.position = Vector3.zero;
                block.transform.rotation = Quaternion.identity;
                block.AddComponent<MeshCollider>();
            }
            foreach (GameObject edge in edges)
            {
                edge.transform.SetParent(parent.transform);
                edge.transform.position = Vector3.zero;
                edge.transform.rotation = Quaternion.identity;
                edge.AddComponent<BoxCollider>();
            }
            foreGround.transform.SetParent(parent.transform);
            foreGround.transform.position = Vector3.zero;
            foreGround.transform.rotation = Quaternion.identity;
            foreGround.AddComponent<BoxCollider>();
            this.ball.AddComponent<ConstantForce>();
            this.ball.AddComponent<VirtualEntity>().entityType = VirtualEntityType.Ball;
            this.leftSpike.AddComponent<VirtualEntity>().entityType = VirtualEntityType.Spike;
            this.rightSpike.AddComponent<VirtualEntity>().entityType = VirtualEntityType.Spike;
            this.debuffBurn.AddComponent<VirtualEntity>().entityType = VirtualEntityType.Debuff;
            this.debuffFreeze.AddComponent<VirtualEntity>().entityType = VirtualEntityType.Debuff;
        }
        public void CopyRigidBody(Rigidbody original, Rigidbody copy)
        {
            copy.mass = original.mass;
            copy.drag = original.drag;
            copy.angularDrag = original.angularDrag;
            copy.automaticCenterOfMass = original.automaticCenterOfMass;
            copy.automaticInertiaTensor = original.automaticInertiaTensor;
            copy.useGravity = original.useGravity;
            copy.isKinematic = original.isKinematic;
            copy.collisionDetectionMode = original.collisionDetectionMode;
            copy.constraints = original.constraints;
            copy.includeLayers = original.includeLayers;
            copy.excludeLayers = original.excludeLayers;
            copy.maxLinearVelocity = original.maxLinearVelocity;
            copy.maxAngularVelocity = original.maxAngularVelocity;
            copy.maxDepenetrationVelocity = original.maxDepenetrationVelocity;
        }
        public void CopyBoxCollider(BoxCollider original, BoxCollider copy)
        {
            copy.isTrigger = original.isTrigger;
            copy.material = original.material;
            copy.size = original.size;
            copy.layerOverridePriority = original.layerOverridePriority;
            copy.includeLayers = original.includeLayers;
            copy.excludeLayers = original.excludeLayers;
            copy.center = original.center;
        }
        public void CopyMeshCollider(MeshCollider original, MeshCollider copy, Mesh overrideMesh = null)
        {
            copy.sharedMesh = overrideMesh != null ? overrideMesh : original.sharedMesh;
            copy.convex = original.convex;
            copy.isTrigger = original.isTrigger;
            copy.material = original.material;
            copy.layerOverridePriority = original.layerOverridePriority;
            copy.includeLayers = original.includeLayers;
            copy.excludeLayers = original.excludeLayers;
        }
        public void CopySphereCollider(SphereCollider original, SphereCollider copy)
        {
            copy.isTrigger = original.isTrigger;
            copy.material = original.material;
            copy.radius = original.radius;
            copy.layerOverridePriority = original.layerOverridePriority;
            copy.includeLayers = original.includeLayers;
            copy.excludeLayers = original.excludeLayers;
            copy.center = original.center;
        }
    }
}
