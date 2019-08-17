using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class Selecter : MonoBehaviour
{
    public string selecterName;
    public string selecterTag;
    public int min;
    public int max;
    public Text nameDisplay;
    public Text minDisplay;
    public Text maxDisplay;
    public Slider sliderInput;
    public InputField textInput;

    private bool _textFocusExit;
    private bool _textLastState;

    // Start is called before the first frame update
    private void Start()
    {
        sliderInput.onValueChanged.AddListener(RefreshSlider);
        _textLastState = false;
        _textFocusExit = true;
        nameDisplay.text = selecterName;
        minDisplay.text = min.ToString();
        maxDisplay.text = max.ToString();
        textInput.text = ((min + max) / 2).ToString();
    }

    // Update is called once per frame
    private void Update()
    {
       RefreshSelecter(); 
    }

    private void RefreshSelecter()
    {
        var textFocus = textInput.isFocused;
        _textFocusExit = !textFocus && _textLastState;
        _textLastState = textFocus;

        if (_textFocusExit)
        {
            _textFocusExit = false;
            var text = textInput.text;
            text = text != "" ? text : 0 < min ? min.ToString() : 0 > max ? max.ToString() : "0"; 
            var value = int.Parse(text);
            if (value > max)
            {
                value = max;
            }
            else if (value < min)
            {
                value = min;
            }
            textInput.text = value.ToString();
            
        }
        if(textFocus)
        {
            var text = textInput.text;
            text = text != "" ? text : 0 < min ? min.ToString() : 0 > max ? max.ToString() : "0"; 
            int.TryParse(text, out var value);
            sliderInput.value = ((float)value - min) / ((float)max - min);
        }
    }

    private void RefreshSlider(float sliderValue)
    {
        var value = (int)(sliderValue * ((float) max - min) + min);
        textInput.text = value.ToString();
    }

    
}
