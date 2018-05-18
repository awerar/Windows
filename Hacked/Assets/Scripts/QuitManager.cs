using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitManager : MonoBehaviour {

    private WindowManager windowManager;

	// Use this for initialization
	void Start () {
        windowManager = FindObjectOfType<WindowManager>();
	}

    public void Quit()
    {
        Application.Quit();
    }

    public void Back()
    {
        windowManager.UnloadOnWindow(1);
    }
}
