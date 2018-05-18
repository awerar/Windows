using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour {

    [Header("Gameobject")]
    public LayoutElement content;
    public RectTransform title;
    private WindowManager windowManager;

	// Use this for initialization
	void Start () {
        content.preferredHeight = transform.parent.GetComponent<RectTransform>().rect.height - title.localPosition.y;
        windowManager = FindObjectOfType<WindowManager>();
	}
	
    public void Back()
    {
        windowManager.UnloadOnWindow(1);
    }
}
