using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SlideAnimation : MonoBehaviour {

    [Range(0, 1)]
    public float normalisedPosition;
    public bool OnXAxis;
    public float min;
    public float max;

    private RectTransform rect;

    // Update is called once per frame
    void Update () {
        rect = GetComponent<RectTransform>();
        if (OnXAxis)
        {
            rect.pivot = new Vector2(1 - (normalisedPosition / 2), rect.pivot.y);
        } else
        {
            rect.pivot = new Vector2(rect.pivot.x, (normalisedPosition*(max-min))+min);
        }
	}
}
