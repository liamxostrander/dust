using UnityEngine;
using UnityEngine.UI;
public class StatusBar : MonoBehaviour
{
    public Slider slider;

    public void setMaxValue(float value)
    {
        slider.maxValue = value;
        slider.value = value;
    }
    public void setValue(float value)
    {
        slider.value = value;
    }
}
