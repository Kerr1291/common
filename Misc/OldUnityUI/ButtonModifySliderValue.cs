using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonModifySliderValue : MonoBehaviour {

    public Slider slider;

    public float delta = 1.0f;

    public void OnClick()
    {
        if( slider == null )
            return;

        slider.value += delta;
    }
}
