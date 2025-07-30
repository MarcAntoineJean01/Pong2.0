using UnityEngine;
using UnityEngine.UI;
public class ProgressBar : MonoBehaviour
{
    public int minimum;
    public int maximum;
    public int current;
    public Image bg;
    public Image mask;
    public Image fill;
    public bool showMask;
    public bool isRadial;

    public Color fillColor;
    public Color bgColor;
    public Color maskColor;
    void Start()
    {
        fill.color = fillColor;
        mask.color = maskColor;
        bg.color = bgColor;

        Mask maskComponent = mask.GetComponent<Mask>();
        maskComponent.showMaskGraphic = showMask;            
    }
    public void SetCurrentFill()
    {
        if (current > maximum) {current = maximum;}
        float currentOffset = current - minimum;
        float fillAmount = currentOffset / maximum;
        Image imageToFill = isRadial ? fill : mask;
        imageToFill.fillAmount = fillAmount;
        // if (currentOffset > maximum)
        // {
        //     minimum += maximum;
        // }
        // if (current < minimum)
        // {
        //     minimum -= maximum;
        // }
    }
}