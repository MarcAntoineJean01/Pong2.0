using UnityEngine;

public class FreeMoveFieldManager : PongManager
{
    BoxCollider foregroundCollider => foreground.GetComponent<BoxCollider>();
    [SerializeField]
    private LineRenderer zLine;
    [SerializeField]
    private LineRenderer xLine;
    [SerializeField]
    private Camera textureCamera;
    private MeshRenderer foregroundMesh => foreground.GetComponent<MeshRenderer>();
    private MeshFilter foregroundFilter => foreground.GetComponent<MeshFilter>();
    [SerializeField]
    private ForegroundCollisionDetector foreground;
    private RenderTexture foregroundTexture;
    private static int cullingMask;
    void OnEnable()
    {
        foregroundTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
        textureCamera.targetTexture = foregroundTexture;
        foregroundMesh.material.SetTexture("_BaseTexture", foregroundTexture);
        zLine.positionCount = xLine.positionCount = 4;
        xLine.loop = false;
        zLine.loop = true;
        zLine.startWidth = 3;
        zLine.endWidth = 3;
        foregroundCollider.size = (field.background.col as BoxCollider).size;
        foreground.transform.localScale = field.background.transform.localScale;
        foreground.transform.position = new Vector3(field.background.transform.position.x, field.background.transform.position.y, mainSettings.gameMode == GameMode.Time ? stagePosZ - 0.6566f : CameraManager.activeVCam.transform.position.z + menuCanvas.planeDistance + 0.73056f); // 0.6566 | 0.73056 sweet spot to match main camera
        foregroundCollider.center = new Vector3(0, 0, sizes.ballDiameter * 1.5f / field.ball.transform.localScale.z);
        xLine.SetPosition(0, new Vector3(field.ball.transform.position.x, foregroundCollider.transform.position.y - (foregroundCollider.size.y * 0.5f * foreground.transform.localScale.x), stagePosZ));
        xLine.SetPosition(1, new Vector3(field.ball.transform.position.x, field.background.transform.position.y - (foregroundCollider.size.y * 0.5f * foreground.transform.localScale.x), field.background.transform.position.z - (foregroundCollider.size.z * 0.5f * foreground.transform.localScale.x)));
        xLine.SetPosition(2, new Vector3(field.ball.transform.position.x, field.background.transform.position.y + (foregroundCollider.size.y * 0.5f * foreground.transform.localScale.x), field.background.transform.position.z - (foregroundCollider.size.z * 0.5f * foreground.transform.localScale.x)));
        xLine.SetPosition(3, new Vector3(field.ball.transform.position.x, foregroundCollider.transform.position.y + (foregroundCollider.size.y * 0.5f * foreground.transform.localScale.x), stagePosZ));

        zLine.SetPosition(0, new Vector3(field.rightWall.transform.position.x - (foregroundCollider.size.z * 0.5f * foreground.transform.localScale.x), field.rightWall.transform.position.y - (foregroundCollider.size.y * 0.5f * foreground.transform.localScale.x), field.ball.transform.position.z));
        zLine.SetPosition(1, new Vector3(field.rightWall.transform.position.x - (foregroundCollider.size.z * 0.5f * foreground.transform.localScale.x), field.rightWall.transform.position.y + (foregroundCollider.size.y * 0.5f * foreground.transform.localScale.x), field.ball.transform.position.z));
        zLine.SetPosition(2, new Vector3(field.leftWall.transform.position.x + (foregroundCollider.size.z * 0.5f * foreground.transform.localScale.x), field.leftWall.transform.position.y + (foregroundCollider.size.y * 0.5f * foreground.transform.localScale.x), field.ball.transform.position.z));
        zLine.SetPosition(3, new Vector3(field.leftWall.transform.position.x + (foregroundCollider.size.z * 0.5f * foreground.transform.localScale.x), field.leftWall.transform.position.y - (foregroundCollider.size.y * 0.5f * foreground.transform.localScale.x), field.ball.transform.position.z));
        if (pve.pveActive)
        {
            pve.virtualField.foreGround.transform.position = foreground.transform.position;
            pve.virtualField.foreGround.transform.rotation = foreground.transform.rotation;
            pve.virtualField.foreGround.transform.localScale = foreground.transform.localScale;
            pve.virtualField.CopyBoxCollider(fmfm.foregroundCollider, pve.virtualField.foreGround.GetComponent<BoxCollider>());
        }
        textureCamera.transform.position = CameraManager.activeVCam.transform.position;
        textureCamera.transform.rotation = CameraManager.activeVCam.transform.rotation;
        foregroundFilter.mesh = field.background.meshF.mesh;
        foreground.screenBounce.AddListener(() => PlayScreenBounce());
        cullingMask = cm.mainCam.cullingMask;
        cm.mainCam.cullingMask = 1 << LayerMask.NameToLayer("Foreground");
    }
    void PlayScreenBounce()
    {
        am.PlayAudio(AudioType.BalScreenBounce, field.ball.transform.position);
    }
    void FixedUpdate()
    {
        for (int i = 0; i < xLine.positionCount; i++)
        {
            xLine.SetPosition(i, new Vector3(field.ball.transform.position.x, xLine.GetPosition(i).y, xLine.GetPosition(i).z));
            zLine.SetPosition(i, new Vector3(zLine.GetPosition(i).x, zLine.GetPosition(i).y, field.ball.transform.position.z));
        }
    }
    void OnDestroy()
    {
        if (cm.mainCam != null)
        {
            cm.mainCam.cullingMask = cullingMask;
        }
    }
}
