using UnityEngine;

public class VirtualEntity : MonoBehaviour
{
    public bool live = false;
    public VirtualEntityType entityType;
    bool withinBallSpeedLimits => GetComponent<Rigidbody>().velocity.sqrMagnitude >= PongBehaviour.field.ball.sqrBallMinSpeed && GetComponent<Rigidbody>().velocity.sqrMagnitude <= PongBehaviour.field.ball.sqrBallMaxSpeed;
    bool withinBallRotationSpeedLimits => GetComponent<Rigidbody>().angularVelocity.sqrMagnitude >= PongBehaviour.field.ball.sqrBallMinRotationSpeed && GetComponent<Rigidbody>().angularVelocity.sqrMagnitude <= PongBehaviour.field.ball.sqrBallMaxRotationSpeed;
    bool withinSpikeSpeedLimits => GetComponent<Rigidbody>().velocity.sqrMagnitude == PongBehaviour.field.spikeStore.lastActiveSpikes.spikeLeft.sqrSpikeSpeed;
    public bool withinDebuffSpeedLimits =>GetComponent<Rigidbody>() != null ? GetComponent<Rigidbody>().velocity.sqrMagnitude >= PongBehaviour.field.debuffStore.debuffBurn.sqrDebuffMinSpeed && GetComponent<Rigidbody>().velocity.sqrMagnitude <= PongBehaviour.field.debuffStore.debuffBurn.sqrDebuffMaxSpeed : true;
    Vector3 clampedBallVelocity
    {
        get
        {
            if (GetComponent<Rigidbody>().velocity.sqrMagnitude > PongBehaviour.field.ball.sqrBallMaxSpeed) { return GetComponent<Rigidbody>().velocity.normalized * PongBehaviour.field.ball.ballMaxSpeed; }
            else if (GetComponent<Rigidbody>().velocity.sqrMagnitude < PongBehaviour.field.ball.sqrBallMinSpeed) { return GetComponent<Rigidbody>().velocity.normalized * PongBehaviour.field.ball.ballMinSpeed; }
            return GetComponent<Rigidbody>().velocity;
        }
    }
    Vector3 clampedBallAngularVelocity
    {
        get
        {
            if (GetComponent<Rigidbody>().angularVelocity.sqrMagnitude > PongBehaviour.field.ball.ballMaxRotationSpeed) { return GetComponent<Rigidbody>().angularVelocity.normalized * PongBehaviour.field.ball.ballMaxRotationSpeed; }
            else if (GetComponent<Rigidbody>().angularVelocity.sqrMagnitude < PongBehaviour.field.ball.sqrBallMinRotationSpeed) { return GetComponent<Rigidbody>().angularVelocity.normalized * PongBehaviour.field.ball.ballMinRotationSpeed; }
            return GetComponent<Rigidbody>().angularVelocity;
        }
    }
    Vector3 clampedSpikeVelocity
    {
        get
        {
            if (GetComponent<Rigidbody>().velocity.sqrMagnitude != PongBehaviour.field.spikeStore.lastActiveSpikes.spikeLeft.sqrSpikeSpeed) { return GetComponent<Rigidbody>().velocity.normalized * PongBehaviour.pm.speeds.entitySpeeds.spikeLinearVelocity * PongBehaviour.field.spikeStore.lastActiveSpikes.spikeLeft.speedModifier; }
            return GetComponent<Rigidbody>().velocity;
        }
    }
    Vector3 clampedDebuffVelocity
    {
        get
        {
            if(GetComponent<Rigidbody>().velocity.sqrMagnitude > PongBehaviour.field.debuffStore.debuffBurn.sqrDebuffMaxSpeed) {return GetComponent<Rigidbody>().velocity.normalized * PongBehaviour.field.debuffStore.debuffBurn.debuffMaxSpeed;}
            else if(GetComponent<Rigidbody>().velocity.sqrMagnitude < PongBehaviour.field.debuffStore.debuffBurn.sqrDebuffMinSpeed) {return GetComponent<Rigidbody>().velocity.normalized * PongBehaviour.field.debuffStore.debuffBurn.debuffMinSpeed;}
            return GetComponent<Rigidbody>().velocity;            
        }
    }
    void FixedUpdate()
    {
        if (live)
        {
            if (entityType == VirtualEntityType.Ball)
            {

                if (!withinBallSpeedLimits)
                {
                    GetComponent<Rigidbody>().velocity = clampedBallVelocity;
                }
                if (!withinBallRotationSpeedLimits)
                {
                    GetComponent<Rigidbody>().angularVelocity = clampedBallAngularVelocity;
                }
            }
            else if (entityType == VirtualEntityType.Spike)
            {
                if (!withinSpikeSpeedLimits)
                {
                    GetComponent<Rigidbody>().velocity = clampedSpikeVelocity;
                }
            }
            else if (entityType == VirtualEntityType.Debuff)
            {
                if (!withinDebuffSpeedLimits)
                {
                    GetComponent<Rigidbody>().velocity = clampedDebuffVelocity;
                }
            }            
        }

    }
    void OnCollisionEnter(Collision collision)
    {
        if ((entityType == VirtualEntityType.Ball || entityType == VirtualEntityType.Spike) && (collision.gameObject.name == "LeftWall" || collision.gameObject.name == "RightWall" || collision.gameObject.name == "LeftPad" || collision.gameObject.name == "RightPad"))
        {
            if (entityType == VirtualEntityType.Ball)
            {
                if (collision.gameObject.name == "LeftWall")
                {
                    PongBehaviour.field.leftPad.targetY = transform.position.y;
                    PongBehaviour.field.leftPad.targetZ = transform.position.z;
                }
                else if (collision.gameObject.name == "RightWall")
                {
                    PongBehaviour.field.rightPad.targetY = transform.position.y;
                    PongBehaviour.field.rightPad.targetZ = transform.position.z;
                }
            }
            gameObject.SetActive(false);
        }
    }
}
