using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using PongGame.PongLocker;
using PongGame.MeshLocker;
using PongGame.AudioLocker;
using PongGame.BallLocker;
namespace PongGame
{
    public class BallEntity : PongEntity
    {
        float timeScaleWhenStartedRandomDireciton = 1;
        public BallMesh ballType;
        public bool attracted;
        public State st = State.Dead;
        public ConstantForce cf;
        public Side dissolveSide;
        public bool dissolved = false;
        public float speedOverTimeModifier = 1f;
        public int maxSpeedOverTimeModifier = 5;
        public float ballMaxSpeed => pm.speeds.entitySpeeds.ballMaxLinearVelocity * speedModifier * speedOverTimeModifier;
        public float ballMinSpeed => pm.speeds.entitySpeeds.ballMinLinearVelocity * speedModifier * speedOverTimeModifier;
        public float ballMaxRotationSpeed => pm.speeds.entitySpeeds.ballMaxAngularVelocity * speedModifier;
        public float ballMinRotationSpeed => pm.speeds.entitySpeeds.ballMinAngularVelocity * speedModifier;
        public float sqrBallMaxSpeed => ballMaxSpeed * ballMaxSpeed;
        public float sqrBallMinSpeed => ballMinSpeed * ballMinSpeed;
        public float sqrBallMaxRotationSpeed => ballMaxRotationSpeed * ballMaxRotationSpeed;
        public float sqrBallMinRotationSpeed => ballMinRotationSpeed * ballMinRotationSpeed;
        bool withinSpeedLimits => rbd.velocity.sqrMagnitude >= sqrBallMinSpeed && rbd.velocity.sqrMagnitude <= sqrBallMaxSpeed;
        bool withinRotationSpeedLimits => rbd.angularVelocity.sqrMagnitude >= sqrBallMinRotationSpeed && rbd.angularVelocity.sqrMagnitude <= sqrBallMaxRotationSpeed;
        public bool cyclingRandomDirection = false;
        Vector2 randoDirectionTrigerZone => fieldBounds * 0.5f;
        bool inRandoDirectionTriggerZone => transform.position.x < randoDirectionTrigerZone.x && transform.position.x > -randoDirectionTrigerZone.x && transform.position.y < randoDirectionTrigerZone.y && transform.position.y > -randoDirectionTrigerZone.y;
        Vector3 clampedVelocity
        {
            get
            {
                if (rbd.velocity.sqrMagnitude > sqrBallMaxSpeed) { return rbd.velocity.normalized * ballMaxSpeed; }
                else if (rbd.velocity.sqrMagnitude < sqrBallMinSpeed) { return rbd.velocity.normalized * ballMinSpeed; }
                return rbd.velocity;
            }
        }
        Vector3 clampedAngularVelocity
        {
            get
            {
                if (rbd.angularVelocity.sqrMagnitude > ballMaxRotationSpeed) { return rbd.angularVelocity.normalized * ballMaxRotationSpeed; }
                else if (rbd.angularVelocity.sqrMagnitude < sqrBallMinRotationSpeed) { return rbd.angularVelocity.normalized * ballMinRotationSpeed; }
                return rbd.angularVelocity;
            }
        }
        public Vector3 initialBallVelocity
        {
            get
            {
                int[] directions = { -1, 1 };
                float x = UnityEngine.Random.Range(1f, 2f) * directions[UnityEngine.Random.Range(0, 2)] * ballMaxSpeed * 0.75f;
                float y = UnityEngine.Random.Range(1f, 2f) * directions[UnityEngine.Random.Range(0, 2)] * ballMaxSpeed * 0.75f;
                float z = 0;

                return new Vector3(x, y, z);
            }
        }
        public Vector3 initialBallAngularVelocity
        {
            get
            {
                int[] directions = { -1, 1 };
                float x = UnityEngine.Random.Range(1f, 2f) * directions[UnityEngine.Random.Range(0, 2)] * ballMaxRotationSpeed * 0.75f;
                float y = UnityEngine.Random.Range(1f, 2f) * directions[UnityEngine.Random.Range(0, 2)] * ballMaxRotationSpeed * 0.75f;
                float z = 0;

                return new Vector3(x, y, z);
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            rbd.maxLinearVelocity = ballMaxSpeed;
            rbd.maxDepenetrationVelocity = rbd.maxLinearVelocity;
            rbd.maxAngularVelocity = ballMaxRotationSpeed;
        }
        protected override void Update()
        {
            if (!pauseCalled)
            {
                base.Update();
                if (st == State.Live && dissolveSide != Side.None)
                {
                    if (!dissolved && ((dissolveSide == Side.Left && transform.position.x < 0) || (dissolveSide == Side.Right && transform.position.x > 0)))
                    {
                        Dissolve();
                    }
                    else if (dissolved && ((dissolveSide == Side.Right && transform.position.x < 0) || (dissolveSide == Side.Left && transform.position.x > 0)))
                    {
                        Materialize();
                    }
                }
            }
        }
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (st == State.Live)
            {
                if (!magnetized)
                {
                    if (!withinSpeedLimits)
                    {
                        rbd.velocity = clampedVelocity;
                    }
                    if (!withinRotationSpeedLimits)
                    {
                        rbd.angularVelocity = clampedAngularVelocity;
                    }

                }
                if (Mathf.Abs(transform.position.x) > Mathf.Abs(fieldBounds.x) || Mathf.Abs(transform.position.y) > Mathf.Abs(fieldBounds.y))
                {
                    ResetBall();
                }
            }
        }
        protected override void OnParticleCollision(GameObject other)
        {
            base.OnParticleCollision(other);
            switch (other.name)
            {
                case "IceProjectile":
                    Frozen();
                    break;
            }
        }
        void OnCollisionEnter(Collision collision)
        {
            if (lct != Time.time)
            {
                lct = Time.time;
                if (currentStage == Stage.FreeMove && collision.gameObject.GetComponent<Pad>() != null)
                {
                    am.PlayAudio(PongAudioType.BallShieldBounce, transform.position);
                }
                else if (collision.gameObject.GetComponent<Pad>() != null)
                {
                    am.PlayAudio(PongAudioType.PadBounce, transform.position);
                }
                else
                {
                    am.PlayAudio(PongAudioType.BallBounce, transform.position);
                }
                if (collision.gameObject.GetComponent<Pad>() != null && magnetized)
                {
                    magnetized = false;
                }
                bounced.Invoke();
            }
        }
        void OnCollisionExit(Collision collision)
        {
            if (lct != Time.time)
            {
                lct = Time.time;
                if (((currentStage == Stage.Universe && field.debuffStore.debuffSlow.gameObject.activeSelf) || currentStage == Stage.FireAndIce) && !field.fragmentStore.NoMoreFragmentsForBall(ballType))
                {
                    field.fragmentStore.DropBallFragments(ballType);
                    if ((field.ball.ballType == BallMesh.IcosahedronRough || field.ball.ballType == BallMesh.Octacontagon) && field.fragmentStore.NoMoreFragmentsForBall(ballType))
                    {
                        BallEntity newBall = builder.MakeFullBall(field.ball.ballType == BallMesh.IcosahedronRough ? BallMesh.IcosahedronRough : BallMesh.Octacontagon);
                        newBall.SetBallForStage();
                        field.ReplaceEntity(Entity.Ball, newBall);
                        if (PongManager.mainSettings.cutScenesOn && field.ball.ballType == BallMesh.IcosahedronRough)
                        {
                            csm.PlayScene(CutScene.PolyFeelsEvenLighter);
                        }
                    }
                    if (PongManager.mainSettings.cutScenesOn && field.ball.ballType == BallMesh.IcosahedronRough && field.fragmentStore.cubeFragments.Count == 3)
                    {
                        csm.PlayScene(CutScene.PolyFeelsLighter);
                    }
                }

            }
        }
        public void StartBall(Vector3? velocity = null, Vector3? angularVelocity = null)
        {
            rbd.isKinematic = false;
            Vector3 finalVel = velocity != null ? new Vector3(velocity.Value.x, velocity.Value.y, 0) : initialBallVelocity;
            Vector3 finalAngVel = angularVelocity != null ? angularVelocity.Value : Vector3.zero;
            MoveBall(finalVel, finalAngVel);
            StartCoroutine("CycleSpeedUpOverTime");
        }
        void ResetBall()
        {
            transform.position = new Vector3(0, 0, stagePosZ);
            StopBall();
            newGameManager.SpikeSpawn();
            StartBall();
        }
        public void StopBall()
        {
            if (st == State.Dead)
            {
                transform.position = new Vector3(0, 0, stagePosZ);
                transform.rotation = new Quaternion(0, 0, 0, 0);
            }
            StopAllBallCoroutines();
            newGameManager.StopCoroutine("CycleSpikeSpawn");
            rbd.isKinematic = true;
            if (PongManager.mainSettings.gameMode == GameMode.Time)
            {
                newGameManager.PauseFieldDoMoveZ();
            }
            PongManager.launchCalled = false;
        }
        public void MoveBall(Vector3 vel, Vector3 angVel)
        {
            rbd.AddForce(vel, ForceMode.Acceleration);
            rbd.AddTorque(angVel, ForceMode.VelocityChange);
        }
        public void SetBallState(State state)
        {
            st = state;
            cf.force = Vector3.zero;
            switch (state)
            {
                case State.Live:
                    StartBall();
                    newGameManager.SpikeSpawn();
                    break;
                case State.Dead:
                case State.Idle:
                    gameObject.SetActive(state == State.Idle);
                    StopBall();
                    break;
            }
        }
        public void SetBallForStage()
        {
            if (PongManager.mainSettings.gameMode != GameMode.NonStop)
            {
                StopAllBallCoroutines();
            }
            switch (currentStage)
            {
                case Stage.StartMenu:
                case Stage.DD:
                    meshR.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                    transform.rotation = Quaternion.Euler(new Vector3(45, 45, 0));
                    rbd.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                    break;
                default:
                case Stage.DDD:
                    rbd.constraints = RigidbodyConstraints.FreezePositionZ;
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    break;
                case Stage.Universe:
                    rbd.constraints = RigidbodyConstraints.FreezePositionZ;
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    break;
                case Stage.GravityWell:
                    rbd.constraints = RigidbodyConstraints.FreezePositionZ;
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    break;
                case Stage.FreeMove:
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    rbd.constraints = RigidbodyConstraints.None;
                    break;
                case Stage.FireAndIce:
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    rbd.constraints = RigidbodyConstraints.FreezePositionZ;
                    break;
                case Stage.Neon:
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    rbd.constraints = RigidbodyConstraints.FreezePositionZ;
                    break;
                case Stage.Final:
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    rbd.constraints = RigidbodyConstraints.FreezePositionZ;
                    break;
            }
        }
        IEnumerator CycleSpeedUpOverTime()
        {
            while (speedOverTimeModifier < maxSpeedOverTimeModifier)
            {
                yield return new WaitForSeconds(5);
                speedOverTimeModifier += 0.2f;
                rbd.maxLinearVelocity = ballMaxSpeed;
                rbd.maxDepenetrationVelocity = rbd.maxLinearVelocity;
            }
        }
        public void AttractWall(Side side)
        {
            if (!attracted)
            {
                switch (side)
                {
                    case Side.Top:
                        cf.force = new Vector3(0, pm.speeds.entitySpeeds.ballMaxLinearVelocity * pm.gameEffects.floorAttractorStrength, 0);
                        vfx.WallAttractorMaterial(field.topFloor);
                        break;
                    case Side.Bottom:
                        cf.force = new Vector3(0, -(pm.speeds.entitySpeeds.ballMaxLinearVelocity * pm.gameEffects.floorAttractorStrength), 0);
                        vfx.WallAttractorMaterial(field.bottomFloor);
                        break;
                    case Side.Left:
                        cf.force = new Vector3(-(pm.speeds.entitySpeeds.ballMaxLinearVelocity * pm.gameEffects.wallAttractorStrength), 0, 0);
                        vfx.WallAttractorMaterial(field.leftWall);
                        break;
                    case Side.Right:
                        cf.force = new Vector3(pm.speeds.entitySpeeds.ballMaxLinearVelocity * pm.gameEffects.wallAttractorStrength, 0, 0);
                        vfx.WallAttractorMaterial(field.rightWall);
                        break;

                }
                StartCoroutine("CycleWallAttractorTimer");
            }

        }
        IEnumerator CycleWallAttractorTimer()
        {
            attracted = true;
            yield return new WaitForSeconds(pm.gameEffects.wallAttractorDuration);
            cf.force = Vector3.zero;
            attracted = false;
        }
        protected override IEnumerator CycleFrozen()
        {
            frozen = true;
            rbd.isKinematic = true;
            SetBallState(State.Idle);
            float t = 0f;
            while (t < pm.gameEffects.frostDuration)
            {
                t += Time.deltaTime;
                if (t > pm.gameEffects.frostDuration) { t = pm.gameEffects.frostDuration; }
                meshR.material.SetFloat("_FrostAmmount", Mathf.SmoothStep(meshR.material.GetFloat("_FrostAmmount"), 1, t / pm.gameEffects.frostDuration));
                yield return null;
            }
            meshR.material.SetFloat("_FrostAmmount", 1);
            yield return new WaitForSeconds(1);
            t = 0f;
            rbd.isKinematic = false;
            frozen = false;
            while (t < pm.gameEffects.frostDuration * 0.25f)
            {
                t += Time.deltaTime;
                if (t > pm.gameEffects.frostDuration * 0.25f) { t = pm.gameEffects.frostDuration * 0.25f; } // *0.25f === /4
                meshR.material.SetFloat("_FrostAmmount", Mathf.SmoothStep(meshR.material.GetFloat("_FrostAmmount"), 0, t / (pm.gameEffects.frostDuration * 0.25f)));
                yield return null;
            }
            meshR.material.SetFloat("_FrostAmmount", 0);
        }
        public void TriggerDissolve(Side side)
        {
            dissolveSide = side;
        }
        public void Dissolve()
        {
            StopCoroutine("CycleDissolve");
            StopCoroutine("CycleMaterialize");
            StartCoroutine("CycleDissolve");
        }
        IEnumerator CycleDissolve()
        {
            float t = 0f;
            dissolved = true;
            am.PlayMusic(MusicType.DissolveMusic);
            meshR.material.SetFloat("_EmissionIntensity", 1);
            if (currentStage < Stage.Neon)
            {
                meshR.material.SetFloat("_DissolveEdgeDepth", 0.01f);
            }
            while (t < pm.gameEffects.dissolveSpeed)
            {
                t += Time.deltaTime;
                if (t > pm.gameEffects.dissolveSpeed) { t = pm.gameEffects.dissolveSpeed; }
                meshR.material.SetFloat("_DissolveProgress", Mathf.SmoothStep(0, pm.gameEffects.dissolveStrength, t / pm.gameEffects.dissolveSpeed));
                yield return null;
            }
        }
        public void Materialize()
        {
            StopCoroutine("CycleDissolve");
            StopCoroutine("CycleMaterialize");
            StartCoroutine("CycleMaterialize");
        }
        IEnumerator CycleMaterialize()
        {
            float t = 0f;
            dissolved = false;
            am.KillSecondaryMusic();
            while (t < pm.gameEffects.dissolveSpeed)
            {
                t += Time.deltaTime;
                if (t > pm.gameEffects.dissolveSpeed) { t = pm.gameEffects.dissolveSpeed; }
                meshR.material.SetFloat("_DissolveProgress", Mathf.SmoothStep(pm.gameEffects.dissolveStrength, 0, t / pm.gameEffects.dissolveSpeed));
                yield return null;
            }
            meshR.material.SetFloat("_EmissionIntensity", 0);
            if (currentStage < Stage.Neon)
            {
                meshR.material.SetFloat("_DissolveEdgeDepth", 0);            
            }
        }
        public void StopAllBallCoroutines()
        {
            StopAllCoroutines();
            if (am.musicAudioSource != null)
            {
                am.musicAudioSource.pitch = 1;
            }
            if (cyclingRandomDirection)
            {
                Time.timeScale = timeScaleWhenStartedRandomDireciton;            
            }
            am.KillSecondaryMusic();
            cf.force = Vector3.zero;
            frozen = false;
            dissolved = false;
            magnetized = false;
            attracted = false;
            speedOverTimeModifier = 1;
            speedModifier = 1;
            cyclingRandomDirection = false;
            rbd.maxLinearVelocity = ballMaxSpeed;
            rbd.maxDepenetrationVelocity = rbd.maxLinearVelocity;
            rbd.maxAngularVelocity = ballMaxRotationSpeed;
            meshR.material.SetFloat("_EmissionIntensity", 0);
    #if UNITY_WEBGL
            meshR.material.SetFloat("_DissolveEdgeDepth", 0);
    #else
            meshR.material.SetFloat("_DissolveEdgeDepth", currentStage < Stage.Neon ? 0 : 1);
            #endif
            meshR.material.SetFloat("_DissolveProgress", 0);
            meshR.material.SetFloat("_FrostAmmount", 0);
            meshR.material.SetFloat("_SuctionRange", 0);
            meshR.material.SetFloat("_SuctionThreshold", 0);
            dissolveSide = Side.None;            
        }
        public void RandomDirection()
        {
            StopCoroutine("CycleRandomDirection");
            StartCoroutine("CycleRandomDirection");
        }
        IEnumerator CycleRandomDirection()
        {
            cyclingRandomDirection = true;
            Vector3 newDir;
            for (int i = 0; i < 4; i++)
            {
                while (!inRandoDirectionTriggerZone || magnetized)
                {
                    yield return null;
                }
                float t = 0;
                timeScaleWhenStartedRandomDireciton = Time.timeScale;
                while (t < 0.5f)
                {
                    t += Time.unscaledDeltaTime;
                    if (t > 0.5f) { t = 0.5f; }
                    am.musicAudioSource.pitch = Mathf.Lerp(1, 0.6f, t/0.5f);
                    Time.timeScale = Mathf.Lerp(timeScaleWhenStartedRandomDireciton, 0.7f, t/0.5f);
                    yield return null;
                }
                meshR.material.SetFloat("_SuctionRange", col.bounds.size.x);
                meshR.material.SetFloat("_SuctionThreshold", col.bounds.size.x * 0.5f);
                newDir = field.ball.initialBallVelocity;
                while (Vector3.Angle(newDir, rbd.velocity) < 90)
                {
                    newDir = field.ball.initialBallVelocity;
                }
                meshR.material.SetVector("_SuctionTarget", RandomDirectionTarget(newDir));
                field.ball.rbd.AddForce(newDir, ForceMode.VelocityChange);
                t = 0;
                while (t < 0.5f)
                {
                    t += Time.unscaledDeltaTime;
                    if (t > 0.5f) { t = 0.5f; }
                    am.musicAudioSource.pitch = Mathf.Lerp(0.6f, 1, t/0.5f);
                    Time.timeScale = Mathf.Lerp(0.7f, timeScaleWhenStartedRandomDireciton, t/0.5f);
                    yield return null;
                }
                am.musicAudioSource.pitch = 1;
                Time.timeScale = timeScaleWhenStartedRandomDireciton;
                yield return new WaitForSeconds(0.7f);
                meshR.material.SetVector("_SuctionTarget", Vector3.zero);
                meshR.material.SetFloat("_SuctionRange", 0);
                meshR.material.SetFloat("_SuctionThreshold", 0);
                yield return new WaitForSeconds(2.3f);
            }
            cyclingRandomDirection = false;
            meshR.material.SetVector("_SuctionTarget", Vector3.zero);
            meshR.material.SetFloat("_SuctionRange", 0);
            meshR.material.SetFloat("_SuctionThreshold", 0);
        }
        Vector3 RandomDirectionTarget(Vector3 targetDirection)
        {
            Vector3 newTarget = Vector3.MoveTowards(transform.position, targetDirection, col.bounds.size.x*3);
            newTarget.z = transform.position.z;
            return newTarget;
        }
    }
}
