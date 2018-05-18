using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TextChangeAnimation : MonoBehaviour {
    [TextArea]
    public string normalString, selectedString;
    public bool normal;

    private void Update()
    {
        string text;
        if(normal)
        {
            text = normalString;
        } else
        {
            text = selectedString;
        }

        GetComponent<Text>().text = text;
    }
}
