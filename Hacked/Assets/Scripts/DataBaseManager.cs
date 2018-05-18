using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBaseManager : MonoBehaviour {

    public DataBase[] dataBases;

    private void Start()
    {
        List<DataBase> dbTemp = new List<DataBase>(dataBases);

        foreach (Window w in FindObjectsOfType<Window>())
        {
            dbTemp.Add(w.db);
        }

        dataBases = dbTemp.ToArray();
    }

    public DataBase GetDataBase(string name)
    {
        foreach (DataBase d in dataBases)
        {
            if (d.name.ToLower() == name.ToLower())
            {
                return d;
            }
        }

        return null;
    }

    public bool HasDatabaseByName(string name)
    {
        foreach (DataBase d in dataBases)
        {
            if (d.name.ToLower() == name.ToLower())
            {
                return true;
            }
        }
        return false;
    }
}

[System.Serializable]
public class DataBase : Machine
{
    [Header("General Info")]
    public string DatabaseName;

    [Header("Login details")]
    public string username;
    public string password;

    [Header("Files")]
    public File[] files;

    public DataBase() : base(MachineType.Database, "")
    {
        name = DatabaseName;
    }

    public bool ContainsFile(string name)
    {
        foreach (File f in files)
        {
            if (f.name == name)
            {
                return true;
            }
        }

        return false;
    }

    public File? GetFile(string name)
    {
        foreach (File f in files)
        {
            if (f.name == name)
            {
                return f;
            }
        }

        return null;
    }
}

[System.Serializable]
public struct File
{
    public TextAsset file;
    public string name;
    public FileAcces accesability;
}
