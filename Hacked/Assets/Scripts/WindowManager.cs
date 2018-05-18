using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Status
{
    Avalible, Used
}

public enum ModuleType
{
    Shell, Help, Main_menu, Options, Quit, None
}

public class WindowManager : MonoBehaviour {

    public List<Window> windows;
    public Dictionary<int, Window> IndexToWindow;
    public Dictionary<Window, int> WindowToIndex;
    public Dictionary<Window, Status> WindowToStatus;
    public Dictionary<Window, ModuleType> WindowToModule;
    public Dictionary<ModuleType, Window> ModuleToWindow;
    public Dictionary<ModuleType, Status> ModuleToStatus;

    //Assign in inspector
    [SerializeField]
    public Module[] modules;
    public Dictionary<ModuleType, GameObject> ModuleToPrefab;

    private void Start()
    {
        WindowToStatus = new Dictionary<Window, Status>();
        WindowToModule = new Dictionary<Window, ModuleType>();
        ModuleToWindow = new Dictionary<ModuleType, Window>();
        ModuleToPrefab = new Dictionary<ModuleType, GameObject>();
        IndexToWindow = new Dictionary<int, Window>();
        WindowToIndex = new Dictionary<Window, int>();
        ModuleToStatus = new Dictionary<ModuleType, Status>();

        foreach (Window window in windows)
        {
            WindowToStatus.Add(window, Status.Avalible);
            IndexToWindow.Add(window.index, window);
            WindowToIndex.Add(window, window.index);
        }

        foreach (Module m in modules)
        {
            ModuleToPrefab.Add(m.type, m.prefab);
            ModuleToStatus.Add(m.type, Status.Avalible);
        }

        LoadModuleOnWindow(ModuleType.Main_menu, 3);
    }

    public void LoadModuleOnWindow(ModuleType m, int w)
    {
        if (WindowToStatus[IndexToWindow[w]] != Status.Used)
        {
            Instantiate(ModuleToPrefab[m], IndexToWindow[w].transform);
            WindowToStatus[IndexToWindow[w]] = Status.Used;
            ModuleToStatus[m] = Status.Used;
            WindowToModule[IndexToWindow[w]] = m;
            ModuleToWindow[m] = IndexToWindow[w];
        }
    }

    public void UnloadOnWindow(int w)
    {
        WindowToStatus[IndexToWindow[w]] = Status.Avalible;
        ModuleToStatus[WindowToModule[IndexToWindow[w]]] = Status.Avalible;

        Destroy(IndexToWindow[w].transform.GetChild(1).gameObject);
        WindowToModule[IndexToWindow[w]] = ModuleType.None;
        ModuleToWindow[ModuleType.None] = IndexToWindow[w];
    }

    public IEnumerable<int> GetAvalibleWindows()
    {
        foreach(KeyValuePair<Window, Status> key in WindowToStatus)
        {
            if (key.Value == Status.Avalible)
            {
                yield return WindowToIndex[key.Key];
            }
        }
    }

    public IEnumerable<int> GetWindows()
    {
        foreach(Window w in windows)
        {
            yield return w.index;
        }
    }

    public bool WindowAvalible(int testW)
    {
        foreach (int w in GetAvalibleWindows())
        {
            if (w == testW)
            {
                return true;
            }
        }
        return false;
    }

    public bool WindowExists (int w)
    {
        return windows.Contains(IndexToWindow[w]);
    }

    public IEnumerable<ModuleType> GetModules()
    {
        foreach (Module m in modules)
        {
            if (m.canBeOpenedByPlayer)
            {
                yield return m.type;
            }
        }
    }

    public bool ModuleIsOpenableByPlayer(ModuleType m)
    {
        if (new List<ModuleType>(GetModules()).Contains(m))
        {
            return true;
        } else
        {
            return false;
        }
    }
}

[System.Serializable]
public struct Module
{
    public GameObject prefab;
    public ModuleType type;
    public bool canBeOpenedByPlayer;
}
