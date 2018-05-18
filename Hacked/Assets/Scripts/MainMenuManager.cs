using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour {

    private WindowManager windowManager;

    private void Start()
    {
        windowManager = FindObjectOfType<WindowManager>();
    }

    public void Quit()
    {
        windowManager.LoadModuleOnWindow(ModuleType.Quit, 1);
        FindObjectOfType<HelpMenuUnlockManager>().UnlockPage(HelpSection.MainMenu, "Sub Modules");
    }

    public void Play()
    {
        windowManager.LoadModuleOnWindow(ModuleType.Shell, 2);
    }

    public void Options()
    {
        windowManager.LoadModuleOnWindow(ModuleType.Options, 1);
        FindObjectOfType<HelpMenuUnlockManager>().UnlockPage(HelpSection.MainMenu, "Sub Modules");
    }
}
