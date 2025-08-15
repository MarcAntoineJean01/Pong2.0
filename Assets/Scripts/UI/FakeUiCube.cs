
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using TMPro;
public class FakeUiCube : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color normalColor;
    protected float fadeDuration = 0.1f;
    public Material mat;
    public GameObject[] sides = new GameObject[6];
    protected float size => GetComponentInParent<GridLayoutGroup>().cellSize.x;
    public Sequence sqnc;
    public TMP_Text text;
    protected void SetCube()
    {
        sides[0].transform.localPosition = new Vector3(0, 0, -size * 0.5f);
        sides[0].transform.localRotation = Quaternion.Euler(0, 0, 0);

        sides[1].transform.localPosition = new Vector3(0, 0, size * 0.5f);
        sides[1].transform.localRotation = Quaternion.Euler(0, 180, 180);

        sides[2].transform.localPosition = new Vector3(0, -size * 0.5f, 0);
        sides[2].transform.localRotation = Quaternion.Euler(-90, 0, 0);

        sides[3].transform.localPosition = new Vector3(0, size * 0.5f, 0);
        sides[3].transform.localRotation = Quaternion.Euler(90, 0, 0);

        sides[4].transform.localPosition = new Vector3(-size * 0.5f, 0, 0);
        sides[4].transform.localRotation = Quaternion.Euler(0, 90, 0);

        sides[5].transform.localPosition = new Vector3(size * 0.5f, 0, 0);
        sides[5].transform.localRotation = Quaternion.Euler(0, 270, 0);
        if (PongBehaviour.um.useMeshForUiCubes)
        {
            gameObject.AddComponent<MeshRenderer>().material = mat;
            gameObject.AddComponent<MeshFilter>().mesh = MeshManager.uiFinalMesh;
            foreach (GameObject side in sides)
            {
                GameObject.Destroy(side.GetComponent<Image>());
            }
        }
    }
    void Start()
    {
        SetCube();
        // mat = new Material(mat);
        // if (!PongBehaviour.um.useMeshForUiCubes)
        // {
        //     foreach (GameObject side in sides)
        //     {
        //         side.GetComponent<Image>().material = mat;
        //     } 
        // }
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -(size * 0.5f));
        sqnc = DOTween.Sequence();
        sqnc.Append(transform.DORotate(new Vector3(2.5f, 2.5f, 2.5f), 0.2f).SetEase(Ease.Linear));
        sqnc.Append(transform.DORotate(new Vector3(-2.5f, 0, 0), 0.2f).SetEase(Ease.Linear));
        sqnc.Append(transform.DORotate(new Vector3(2.5f, -2.5f, -2.5f), 0.2f).SetEase(Ease.Linear));
        sqnc.Append(transform.DORotate(new Vector3(0, 0, 0), 0.2f).SetEase(Ease.Linear));
        sqnc.SetEase(Ease.Linear);
        sqnc.SetAutoKill(false);
        sqnc.OnComplete(() => sqnc.Restart());
        sqnc.Complete();
    }
    void OnEnable()
    {
        transform.DOKill();
        StopAllCoroutines();
        if (PongBehaviour.am.beatsOn)
        {
            mat.SetColor("_BaseColor", normalColor);
        }
        else
        {
            mat.SetColor("_BaseColor", Color.Lerp(Color.white, normalColor, PongBehaviour.vfx.styleLerpValue));
        }
        mat.SetFloat("_DissolveProgress", 0);
        transform.localScale = Vector3.one;
    }
}
