using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using PongGame.PongLocker;
using PongGame.BallLocker;
namespace PongGame
{
    public class Pad : PongEntity
    {
        public PadType padType;
        [Serializable]
        public class PadConfirmEvent : UnityEvent { }
        [SerializeField]
        protected PadConfirmEvent m_PadConfirm = new();
        public PadConfirmEvent padConfirm
        {
            get { return m_PadConfirm; }
            set { m_PadConfirm = value; }
        }
        [Serializable]
        public class PadPauseEvent : UnityEvent { }
        [SerializeField]
        protected PadPauseEvent m_PadPause = new();
        public PadPauseEvent padPause
        {
            get { return m_PadPause; }
            set { m_PadPause = value; }
        }
        PadPowers powers = new(false, false);
        public Projectile projectile;
        float moveDistance = 0.1f;
        bool magnetCoolingDown = false;
        public bool magnetOn = false;
        public int attractorCharges = 0;
        public int repulsorCharges = 0;
        bool projectileCoolingDown = false;
        public Side sd;
        public PadController cntrl;
        float targetThreshold => PongManager.sizes.padWidth;
        public float targetY;
        public float targetZ;
        bool freeMove = false;
        public Block topBlock;
        public Block bottomBlock;
        public GameObject energyShield;
        public PongPlayerControls playerControls;
        float horizontalAxis;
        float verticalAxis;
        float topOffset => topBlock != null ? topBlock.col.bounds.size.y : 0;
        float bottomOffset => bottomBlock != null ? bottomBlock.col.bounds.size.y : 0;
        bool playerInputs => cntrl == PadController.KeyBoard || cntrl == PadController.GamePad;
        bool padUpTrigger
        {
            get
            {
                if (playerInputs)
                {
                    if (currentStage == Stage.Neon)
                    {
                        return horizontalAxis < 0;
                    }
                    return verticalAxis > 0;
                }
                return transform.localPosition.y < targetY && Vector2.Distance(new Vector2(0, transform.localPosition.y), new Vector2(0, targetY)) > targetThreshold;
            }
        }
        bool padDownTrigger
        {
            get
            {
                if (playerInputs)
                {
                    if (currentStage == Stage.Neon)
                    {
                        return horizontalAxis > 0;
                    }
                    return verticalAxis < 0;
                }
                return transform.localPosition.y > targetY && Vector2.Distance(new Vector2(0, transform.localPosition.y), new Vector2(0, targetY)) > targetThreshold;
            }
        }
        bool padForwardTrigger
        {
            get
            {
                if (playerInputs)
                {
                    return sd == Side.Left ? horizontalAxis > 0 : horizontalAxis < 0;
                }
                return transform.localPosition.z < targetZ && Vector2.Distance(new Vector2(0, transform.localPosition.z), new Vector2(0, targetZ)) > targetThreshold;
            }
        }
        bool padBackwardTrigger
        {
            get
            {
                if (playerInputs)
                {
                    return sd == Side.Left ? horizontalAxis < 0 : horizontalAxis > 0;
                }
                return transform.localPosition.z > targetZ && Vector2.Distance(new Vector2(0, transform.localPosition.z), new Vector2(0, targetZ)) > targetThreshold;
            }
        }
        public bool playerControlsEnabled = false;
        public void EnableControls()
        {
            if (cntrl == PadController.GamePad)
            {
                playerControls = new PongPlayerControls();
                playerControls.devices = new InputDevice[] { sd == Side.Left ? Gamepad.all[0] : Gamepad.all[1] };
                playerControls.PadControls.Move.performed += ctx => verticalAxis = ctx.ReadValue<Vector2>().y;
                playerControls.PadControls.Move.performed += ctx => horizontalAxis = ctx.ReadValue<Vector2>().x;
                playerControls.PadControls.Confirm.performed += ctx => padConfirm.Invoke();
                playerControls.PadControls.Pause.performed += ctx => padPause.Invoke();
                playerControls.PadControls.Enable();
                playerControlsEnabled = true;
            }
            else if (cntrl == PadController.KeyBoard)
            {
                playerControls = new PongPlayerControls();
                playerControls.devices = new InputDevice[] { Keyboard.current };
                playerControls.PadControls.Move.performed += ctx => verticalAxis = ctx.ReadValue<Vector2>().y;
                playerControls.PadControls.Move.performed += ctx => horizontalAxis = ctx.ReadValue<Vector2>().x;
                playerControls.PadControls.Confirm.performed += ctx => padConfirm.Invoke();
                playerControls.PadControls.Pause.performed += ctx => padPause.Invoke();
                playerControls.PadControls.Enable();
                playerControlsEnabled = true;
            }
            else if (!pve.pveActive)
            {
                pve.StartPVE();
            }
        }
        public void DisableControls()
        {
            if (playerControls != null)
            {
                playerControls.Disable();
            }
            playerControlsEnabled = false;
            cntrl = PadController.Environment;
        }
        void OnCollisionEnter(Collision collision)
        {
            if (lct != Time.time && field.ball.st == State.Live)
            {
                if (currentStage == Stage.Neon && collision.gameObject.GetComponent<BallEntity>() != null)
                {
                    lct = Time.time;
                    cm.SplitScreenCameraNoise(sd == Side.Left ? CameraManager.leftPadVCamEnd : CameraManager.rightPadVCamEnd);
                }
                if (currentStage == Stage.FreeMove)
                {
                    StartCoroutine(CycleEnergyShieldIndent(collision.contacts[0].point));
                }
            }
        }
        void OnCollisionExit(Collision collision)
        {
            if (lct != Time.time)
            {
                lct = Time.time;

                if (collision.gameObject.GetComponent<DebuffBurn>() != null && CanRemovePiece())
                {
                    RemovePadPiece();
                    collision.gameObject.GetComponent<DebuffBurn>().TriggerExplosion();
                }
                if (collision.gameObject.GetComponent<DebuffFreeze>() != null)
                {
                    Frozen();
                    collision.gameObject.GetComponent<DebuffFreeze>().TriggerExplosion();
                }
            }
        }
        protected override void OnParticleCollision(GameObject other)
        {
            base.OnParticleCollision(other);
            switch (other.name)
            {
                case "FireSphere":
                    if (CanRemovePiece())
                    {
                        RemovePadPiece();
                    }
                    break;
                case "IceSphere":
                    Frozen();
                    break;
            }
        }
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!PongManager.stillTransitioning || (PongManager.mainSettings.gameMode == GameMode.NonStop && field.ball.st == State.Live))
            {
                UpdatePad();
            }
        }
        public void SetInitialPadPosition(float fieldWidth, float padWidth)
        {
            // *0.5f === /2
            float positionX = sd == Side.Left ? -(fieldWidth * 0.5f - padWidth) : (fieldWidth * 0.5f - padWidth);
            transform.localPosition = new Vector3(positionX, 0, 0);
        }
        void UpdatePad()
        {
            if (!frozen)
            {
                if (powers.magnet && !magnetOn && !magnetCoolingDown && field.ball.st == State.Live && playerControlsEnabled)
                {
                    if (playerControls.PadControls.RightAction.IsPressed())
                    {
                        Attractor();
                        return;
                    }
                    if (playerControls.PadControls.LeftAction.IsPressed())
                    {
                        Repulsor();
                        return;
                    }
                }
                Vector3 move = transform.position;
                bool shouldMove = false;
                Vector2 playerAxes = playerInputs ? new Vector2(horizontalAxis, verticalAxis) : new Vector2(1, 1);
                if (currentStage == Stage.FreeMove && sd == Side.Right && cntrl == PadController.Environment && padForwardTrigger) { playerAxes = new Vector2(-1, 1); }
                if (currentStage == Stage.Neon && sd == Side.Right && cntrl == PadController.Environment && padUpTrigger) { playerAxes = new Vector2(-1, 1); }
                if (padUpTrigger && (transform.localPosition.y < 0 || InBounds(true)))
                {
                    if (currentStage == Stage.Neon)
                    {
                        move.y += moveDistance * Time.fixedDeltaTime * pm.speeds.entitySpeeds.PadLinearVelocity * -playerAxes.x * speedModifier;
                    }
                    else
                    {
                        move.y += moveDistance * Time.fixedDeltaTime * pm.speeds.entitySpeeds.PadLinearVelocity * Mathf.Abs(playerAxes.y) * speedModifier;
                    }
                    shouldMove = true;
                }
                if (padDownTrigger && (transform.localPosition.y >= 0 || InBounds()))
                {
                    if (currentStage == Stage.Neon)
                    {
                        move.y += moveDistance * Time.fixedDeltaTime * pm.speeds.entitySpeeds.PadLinearVelocity * -playerAxes.x * speedModifier;
                    }
                    else
                    {
                        move.y -= moveDistance * Time.fixedDeltaTime * pm.speeds.entitySpeeds.PadLinearVelocity * Mathf.Abs(playerAxes.y) * speedModifier;
                    }
                    shouldMove = true;
                }
                if (sd == Side.Left)
                {
                    if (freeMove && padForwardTrigger && transform.position.z < field.background.transform.position.z - col.bounds.size.z * 0.5f - field.background.col.bounds.size.z - Vector3.Distance(transform.position, (sd == Side.Left ? field.fragmentStore.leftPadFragments : field.fragmentStore.rightPadFragments)[0].transform.position))
                    {
                        move.z += moveDistance * Time.fixedDeltaTime * pm.speeds.entitySpeeds.PadLinearVelocity * playerAxes.x * speedModifier;
                        shouldMove = true;
                    }
                    if (freeMove && padBackwardTrigger && (transform.position.z >= (cm.mainCam.transform.position.z + PongManager.sizes.planeDistance + Vector3.Distance(transform.position, (sd == Side.Left ? field.fragmentStore.leftPadFragments : field.fragmentStore.rightPadFragments)[0].transform.position))))
                    {
                        move.z -= moveDistance * Time.fixedDeltaTime * pm.speeds.entitySpeeds.PadLinearVelocity * Mathf.Abs(playerAxes.x) * speedModifier;
                        shouldMove = true;
                    }
                }
                else
                {
                    if (freeMove && padForwardTrigger && transform.position.z < field.background.transform.position.z - col.bounds.size.z * 0.5f - field.background.col.bounds.size.z - Vector3.Distance(transform.position, (sd == Side.Left ? field.fragmentStore.leftPadFragments : field.fragmentStore.rightPadFragments)[0].transform.position))
                    {
                        move.z -= moveDistance * Time.fixedDeltaTime * pm.speeds.entitySpeeds.PadLinearVelocity * playerAxes.x * speedModifier;
                        shouldMove = true;
                    }
                    if (freeMove && padBackwardTrigger && (transform.position.z >= (cm.mainCam.transform.position.z + PongManager.sizes.planeDistance + Vector3.Distance(transform.position, (sd == Side.Left ? field.fragmentStore.leftPadFragments : field.fragmentStore.rightPadFragments)[0].transform.position))))
                    {
                        move.z += moveDistance * Time.fixedDeltaTime * pm.speeds.entitySpeeds.PadLinearVelocity * -playerAxes.x * speedModifier;
                        shouldMove = true;
                    }
                }

                if (shouldMove)
                {
                    rbd.MovePosition(move);
                }
                else
                {
                    if (transform.localPosition.y >= 0 && !InBounds(true))
                    {
                        float threshold = field.topFloor.transform.position.y - PongManager.sizes.wallThickness * 0.5f - col.bounds.size.y * 0.5f - topOffset - (freeMove ? Vector3.Distance(transform.position, (sd == Side.Left ? field.fragmentStore.leftPadFragments : field.fragmentStore.rightPadFragments)[3].transform.position) : 0);
                        transform.position = new Vector3(transform.position.x, threshold, transform.position.z);
                    }
                    else if (transform.localPosition.y < 0 && !InBounds())
                    {
                        float threshold = -(field.topFloor.transform.position.y - PongManager.sizes.wallThickness * 0.5f - col.bounds.size.y * 0.5f) + bottomOffset + (freeMove ? Vector3.Distance(transform.position, (sd == Side.Left ? field.fragmentStore.leftPadFragments : field.fragmentStore.rightPadFragments)[3].transform.position) : 0);
                        transform.position = new Vector3(transform.position.x, threshold, transform.position.z);
                    }
                }
            }

        }
        bool InBounds(bool goingUp = false)
        {
            float threshold = field.topFloor.transform.position.y
            - PongManager.sizes.wallThickness * 0.5f
            - col.bounds.size.y * 0.5f
            - (freeMove ? Vector3.Distance(transform.position, (sd == Side.Left ? field.fragmentStore.leftPadFragments : field.fragmentStore.rightPadFragments)[3].transform.position) : 0);
            if (goingUp)
            {
                return Mathf.Abs(transform.position.y) + moveDistance < threshold - topOffset;
            }
            return Mathf.Abs(transform.position.y) + moveDistance < threshold - bottomOffset;
        }
        public void SetPadForStage()
        {
            StopAllCoroutines();
            targetY = 0;
            targetZ = stagePosZ;
            switch (currentStage)
            {
                case Stage.StartMenu:
                case Stage.DD:
                    meshR.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                    rbd.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX;
                    break;
                default:
                case Stage.DDD:
                    powers.magnet = true;
                    freeMove = false;
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    rbd.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX;
                    break;
                case Stage.Universe:
                    freeMove = false;
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    break;
                case Stage.GravityWell:
                    powers.magnet = true;
                    freeMove = false;
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    break;
                case Stage.FreeMove:
                    powers.magnet = true;
                    freeMove = true;
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    rbd.constraints = RigidbodyConstraints.FreezePositionX;
                    break;
                case Stage.FireAndIce:
                    powers.magnet = true;
                    powers.projectiles = true;
                    freeMove = false;
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    rbd.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX;
                    break;
                case Stage.Neon:
                    powers.magnet = true;
                    powers.projectiles = true;
                    freeMove = false;
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    break;
                case Stage.Final:
                    powers.magnet = true;
                    powers.projectiles = true;
                    freeMove = false;
                    meshR.shadowCastingMode = ShadowCastingMode.On;
                    break;
            }
        }
        public void AddPadPiece()
        {
            mm.ResizePadMesh(meshF.mesh, new Vector3(0, meshF.mesh.bounds.size.x, 0));
            mm.ResizePadFragmentsMeshes(sd, new Vector3(0, meshF.mesh.bounds.size.x, 0));
            (col as MeshCollider).convex = true;
            switch (sd)
            {
                case Side.Left:
                    pve.virtualField.leftPad.GetComponent<MeshCollider>().convex = true;
                    break;
                case Side.Right:
                    pve.virtualField.rightPad.GetComponent<MeshCollider>().convex = true;
                    break;
            }
        }
        public bool CanAddPiece()
        {
            // *0.3333f === /3
            return col.bounds.size.y + PongManager.sizes.padWidth <= PongManager.sizes.fieldHeight * 0.3333f;
        }
        public bool CanRemovePiece()
        {
            return col.bounds.size.y - PongManager.sizes.padWidth >= PongManager.sizes.padWidth * 3f;
        }
        public void RemovePadPiece()
        {
            mm.ResizePadMesh(meshF.mesh, new Vector3(0, -meshF.mesh.bounds.size.x, 0));
            mm.ResizePadFragmentsMeshes(sd, new Vector3(0, -meshF.mesh.bounds.size.x, 0));
            (col as MeshCollider).convex = true;
            if (sd == Side.Left)
            {
                pve.virtualField.leftPad.GetComponent<MeshCollider>().convex = true;
            }
            if (sd == Side.Right)
            {
                pve.virtualField.rightPad.GetComponent<MeshCollider>().convex = true;
            }
        }
        public bool CanAddBlock(Side side)
        {
            // *0.3333f === /3
            return side == Side.Top ? topBlock == null || topBlock.col.bounds.size.y + PongManager.sizes.padWidth <= PongManager.sizes.fieldHeight * 0.3333f : bottomBlock == null || bottomBlock.col.bounds.size.y + PongManager.sizes.padWidth <= PongManager.sizes.fieldHeight * 0.3333f;
        }
        public void AddPadBlock(Side side)
        {
            if (side == Side.Top)
            {
                if (topBlock == null)
                {
                    topBlock = builder.MakeBlock(Side.Top, this);
                }
                else
                {
                    mm.ResizeMeshBottom(topBlock.meshF.mesh, PongManager.sizes.padWidth);
                    (topBlock.col as MeshCollider).convex = true;
                    if (sd == Side.Left)
                    {
                        pve.virtualField.blocks[0].GetComponent<MeshCollider>().convex = true;
                    }
                    if (sd == Side.Right)
                    {
                        pve.virtualField.blocks[2].GetComponent<MeshCollider>().convex = true;
                    }
                }
            }
            else
            {
                if (bottomBlock == null)
                {
                    bottomBlock = builder.MakeBlock(Side.Bottom, this);
                }
                else
                {
                    mm.ResizeMeshTop(bottomBlock.meshF.mesh, PongManager.sizes.padWidth);
                    (bottomBlock.col as MeshCollider).convex = true;
                    if (sd == Side.Left)
                    {
                        pve.virtualField.blocks[1].GetComponent<MeshCollider>().convex = true;
                    }
                    if (sd == Side.Right)
                    {
                        pve.virtualField.blocks[3].GetComponent<MeshCollider>().convex = true;
                    }
                }
            }
        }
        public void RemovePadBlock(Side side)
        {
            switch (side)
            {
                case Side.Top:
                    GameObject.Destroy(topBlock.gameObject);
                    topBlock = null;
                    break;
                case Side.Bottom:
                    GameObject.Destroy(bottomBlock.gameObject);
                    bottomBlock = null;
                    break;
            }
        }
        public bool CanAddCharge(bool polarity)
        {
            return polarity ? attractorCharges < PongManager.options.padMaxMagnetCharges : repulsorCharges < PongManager.options.padMaxMagnetCharges;
        }
        public bool CanAddHealth()
        {
            return sd == Side.Left ? PongManager.leftPlayer.healthBar.current < PongManager.leftPlayer.healthBar.maximum : PongManager.rightPlayer.healthBar.current < PongManager.rightPlayer.healthBar.maximum;
        }
        public void AddCharge(bool polarity)
        {
            if (polarity)
            {
                attractorCharges += 1;
                if (sd == Side.Left)
                {
                    um.leftAttractorCount.text = attractorCharges.ToString();
                }
                else
                {
                    um.rightAttractorCount.text = attractorCharges.ToString();
                }
            }
            else
            {
                repulsorCharges += 1;
                if (sd == Side.Left)
                {
                    um.leftRepulsorCount.text = attractorCharges.ToString();
                }
                else
                {
                    um.rightRepulsorCount.text = attractorCharges.ToString();
                }
            }
        }
        public void Attractor()
        {
            if (attractorCharges > 0)
            {
                attractorCharges -= 1;
                StopCoroutine("CycleAttractor");
                StopCoroutine("CycleRepulsor");
                StartCoroutine("CycleAttractor");
            }
            displayHud.Invoke(sd);
        }
        IEnumerator CycleAttractor()
        {
            magnetOn = true;
            StartCoroutine("CycleMagnetCooldown");
            float radius = PongManager.sizes.fieldWidth * 0.01f * pm.gameEffects.magnetSize; // *0.01f === /100
            Vector3 origin = new(sd == Side.Right ? transform.position.x - PongManager.sizes.padWidth * 0.5f : transform.position.x + PongManager.sizes.padWidth * 0.5f, transform.position.y, transform.position.z);
            vfx.MakePadMagnetEffect(transform, origin, radius, true);
            yield return new WaitForSeconds(pm.gameEffects.magnetDuration);
            magnetOn = false;
        }
        public void Repulsor()
        {
            if (repulsorCharges > 0)
            {
                repulsorCharges -= 1;
                StopCoroutine("CycleAttractor");
                StopCoroutine("CycleRepulsor");
                StartCoroutine("CycleRepulsor");
            }
            displayHud.Invoke(sd);
        }
        IEnumerator CycleRepulsor()
        {
            magnetOn = true;
            StartCoroutine("CycleMagnetCooldown");
            float radius = PongManager.sizes.fieldWidth * 0.01f * pm.gameEffects.magnetSize; // *0.01f === /100
            Vector3 origin = new(sd == Side.Right ? transform.position.x - PongManager.sizes.padWidth * 0.5f : transform.position.x + PongManager.sizes.padWidth * 0.5f, transform.position.y, transform.position.z);
            vfx.MakePadMagnetEffect(transform, origin, radius);
            yield return new WaitForSeconds(pm.gameEffects.magnetDuration);
            magnetOn = false;
        }
        IEnumerator CycleMagnetCooldown()
        {
            magnetCoolingDown = true;
            yield return new WaitForSeconds(pm.gameEffects.magnetCooldown);
            magnetCoolingDown = false;
        }
        void IceProjectile()
        {
            if (!projectileCoolingDown)
            {
                StartCoroutine("CycleProjectileCooldown");
                projectile.Fire(0);
            }
        }
        void FireProjectile()
        {
            if (!projectileCoolingDown)
            {
                StartCoroutine("CycleProjectileCooldown");
                projectile.Fire(1);
            }
        }
        IEnumerator CycleProjectileCooldown()
        {
            projectileCoolingDown = true;
            yield return new WaitForSeconds(pm.gameEffects.projectileCooldown);
            projectileCoolingDown = false;
        }
        public void StopAllPadCoroutines()
        {
            StopAllCoroutines();
            frozen = false;
            magnetOn = false;
            magnetCoolingDown = false;
            projectileCoolingDown = false;
            projectile.KillProjectiles();
            meshR.material.SetFloat("_FrostAmmount", 0);
            meshR.material.SetFloat("_SuctionRange", 0);
            meshR.material.SetFloat("_SuctionThreshold", 0);
            if (energyShield != null)
            {
                energyShield.GetComponent<Fragment>().meshR.material.SetVector("_CollisionPosition", Vector3.zero);
                energyShield.GetComponent<Fragment>().meshR.material.SetFloat("_IndentStrength", 0);
            }
            speedModifier = 1;

        }

        IEnumerator CycleEnergyShieldIndent(Vector3 collisionPosition)
        {
            float t = 0;
            energyShield.GetComponent<Fragment>().meshR.material.SetVector("_CollisionPosition", energyShield.transform.InverseTransformPoint(collisionPosition));
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                if (t > 0.5f) { t = 0.5f; }
                energyShield.GetComponent<Fragment>().meshR.material.SetFloat("_IndentStrength", Mathf.Lerp(0, -10f, t / 0.5f));
                yield return null;
            }
            t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                if (t > 0.5f) { t = 0.5f; }
                energyShield.GetComponent<Fragment>().meshR.material.SetFloat("_IndentStrength", Mathf.Lerp(-0.5f, 0, t / 0.5f));
                yield return null;
            }
        }
        public static void ScalePadBlocks(float t, float initialBlockScale, Vector3 initialLeftPadPos, Vector3 initialRightPadPos)
        {
            var normalizedProgress = t / pm.speeds.transitionSpeeds.entitiesTransitionSpeed;
            var easing = newStageManager.moveEntitiesCurve.Evaluate(normalizedProgress);
            foreach (Block block in field.Blocks())
            {
                if (nextStage == Stage.FreeMove)
                {
                    block.transform.localScale = new Vector3(
                        block.transform.localScale.x,
                        block.transform.localScale.y,
                        Mathf.Lerp(initialBlockScale, initialBlockScale * PongManager.sizes.fieldDepth, easing));
                }
                else if (block.transform.localScale.z > initialBlockScale)
                {
                    block.transform.localScale = new Vector3(
                        block.transform.localScale.x,
                        block.transform.localScale.y,
                        Mathf.Lerp(initialBlockScale * PongManager.sizes.fieldDepth, initialBlockScale, easing));
                }
            }
        }
    }
}

