using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum Command
{
    Help, Windows, Modules, Load, Parameter_error, Name_error, Unload, Load_database, Back, Files, Open
}

public enum CommandType
{
    Basic, Advanced, None
}

public enum MachineType
{
    Shell, Database
}

public class ShellManager : MonoBehaviour {

    [Header("Game Objects")]
    public InputField input;
    public Text user;
    public RectTransform command;
    public GameObject content;
    public GameObject viewPort;
    private WindowManager windowManager;
    private DataBaseManager databaseMan;

    [Header("Prefabs")]
    public GameObject printText;

    private bool HasStoppedInputting;
    private List<string> inputHistory = new List<string>();
    private int currentHistoryIndex = -1;

    private Machine currentMachine;
    public Machine CurrentMachine {
        get {
            return currentMachine;
        }

        set {
            currentMachine = value;
            user.text = currentMachine.name + ">";
        }

    }

    private string InputUser {
        get {
            return input.transform.Find("User").GetComponent<Text>().text;
        }
    }
    private string InputText {
        get {
            return input.text;
        }

        set {
            input.text = value;
        }
    }

    private void Start()
    {
        windowManager = FindObjectOfType<WindowManager>();
        databaseMan = FindObjectOfType<DataBaseManager>();

        CurrentMachine = new Shell();

        input.ActivateInputField();

        content.GetComponent<LayoutElement>().minHeight = RectTransformOf(viewPort).rect.height;
        RectTransformOf(content).anchoredPosition = Vector2.zero;
    }

    public void StopInputting()
    {
        HasStoppedInputting = true;
    }

    public void ExecuteCurrentCommand()
    {
        inputHistory.Add(InputText);

        GameObject command = Instantiate(input.gameObject, content.transform);
        Destroy(command.GetComponent<InputField>());

        Text commandText = command.transform.Find("Command").GetComponent<Text>();
        Text commandUser = command.transform.Find("User").GetComponent<Text>();

        commandText.text = InputText;
        commandUser.text = InputUser;

        ActionData data = new ActionData(InputText, this);
        while (data.r != Response.Finished)
        {
            Action action = Action.Create(data, this, windowManager, databaseMan);
            data = new ActionData(input.text, this, action.Execute());
        }

        if (Action.Create(data, this, windowManager, databaseMan).GetType() != typeof(Actions.Unload))
        {
            (Action.CommandToAction[Command.Unload] as Actions.Unload).timesTriedToUnloadOnShell = 0;
        }

        ScrollConsole();

        InputText = string.Empty;

        input.ActivateInputField();
    }

    public void ActivateInputField()
    {
        input.ActivateInputField();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && HasStoppedInputting)
        {
            ExecuteCurrentCommand();
            currentHistoryIndex = inputHistory.Count;
        }

        HasStoppedInputting = false;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentHistoryIndex < inputHistory.Count)
            {
                currentHistoryIndex++;
            }

            if (currentHistoryIndex >= 0 && currentHistoryIndex < inputHistory.Count)
            {
                InputText = inputHistory[currentHistoryIndex];
                input.ActivateInputField();
                input.caretPosition = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentHistoryIndex > 0)
            {
                currentHistoryIndex--;
            }

            if (currentHistoryIndex >= 0 && currentHistoryIndex < inputHistory.Count)
            {
                InputText = inputHistory[currentHistoryIndex];
                input.ActivateInputField();
            }
        }

        float parentWidth = command.transform.parent.GetComponent<RectTransform>().rect.width;
        float userWidth = user.GetComponent<RectTransform>().rect.width;
        command.GetComponent<LayoutElement>().minWidth = parentWidth - userWidth;
    }

    private static RectTransform RectTransformOf (GameObject obj)
    {
        return obj.GetComponent<RectTransform>();
    }

    public void Print(string str, bool middle = false)
    {
        Text text = Instantiate(printText, content.transform).GetComponent<Text>();
        text.text = str;

        if (middle)
        {
            text.alignment = TextAnchor.MiddleCenter;
        }
    }

    public void ScrollConsole()
    {
        //Make the command line the bottom most thing
        input.transform.SetSiblingIndex(input.transform.parent.childCount - 1);

        //Scroll the shell
        RectTransformOf(content).anchoredPosition = Vector2.zero;
    }

    public static string FormatString(string str)
    {
        str = str.ToLower();
        string formattedString = string.Empty;

        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (i == 0)
            {
                formattedString += c.ToString().ToUpper();
            }
            else
            {
                formattedString += c;
            }
        }

        return formattedString;
    }
}

public class CommandHelper {

    private static Dictionary<Command, CommandDocumentation> commandToDoc = new Dictionary<Command, CommandDocumentation>
        {
            { Command.Load, new CommandDocumentation(new string[2] { "Module", "Window" }, Command.Load, new string[1] { "Use this command to open a module on a specified window." }, CommandType.Basic, new List<MachineType>() {MachineType.Shell })},
            { Command.Help, new CommandDocumentation(new string[0], Command.Help, new string[2] { "Use this command to bring up a short help menu of the basic commands. For a list of advanced commands, visit the help module", "Use this command to get a list of the avalible commands for a database." }, CommandType.Basic, new List<MachineType>() {MachineType.Shell, MachineType.Database })},
            { Command.Modules, new CommandDocumentation(new string[0], Command.Modules, new string[1] { "Use this command to get a list of the currently avalible modules." }, CommandType.Basic, new List<MachineType>() {MachineType.Shell })},
            { Command.Windows, new CommandDocumentation(new string[0], Command.Windows, new string[1] { "Use this command to get a list of avalible windows" }, CommandType.Basic, new List<MachineType>() {MachineType.Shell })},
            { Command.Unload, new CommandDocumentation(new string[1] { "Window" }, Command.Unload, new string[1] { "Use this command to unload the current module on a specified window. Whatever you do though, don't try to unload the shell!" }, CommandType.Advanced, new List<MachineType>() {MachineType.Shell })},
            { Command.Load_database, new CommandDocumentation(new string[1] { "name" }, Command.Load_database, new string[1] { "Use this command to load a specified database in the shell." }, CommandType.Advanced, new List<MachineType>() { MachineType.Shell }) },
            { Command.Back, new CommandDocumentation(new string[0], Command.Back, new string[1] { "Use this command to get back to the standard shell." }, CommandType.Basic, new List<MachineType>() { MachineType.Database})},
            { Command.Files, new CommandDocumentation(new string[0], Command.Files, new string[1] { "Use this command to get a list of the files stored on the database you're on." }, CommandType.Basic, new List<MachineType>() { MachineType.Database})},
            { Command.Open, new CommandDocumentation(new string[1] { "File" }, Command.Open, new string[1] { "This command opens a specified file on the current database. Note that you only get to read the file, not edit it." }, CommandType.Basic, new List<MachineType>() { MachineType.Database})}
        };

    public static string GetDocumentationOf(Command c, MachineType m)
    {
        string parameters = "";
        foreach (string parameter in GetParametersOf(c))
        {
            parameters += " + <" + parameter + ">";
        }

        return GetNameOf(c) + parameters + ": " + GetDescriptionOf(c, m);
    }

    public static bool CommandIsOfType(Command c, MachineType m)
    {
        List<MachineType> types = commandToDoc[c].machines;

        foreach (MachineType type in types)
        {
            if (type == m)
            {
                return true;
            }
        }

        return false;
    }

    public static IEnumerable<string> GetAllDocumentationOfType(CommandType type, MachineType machine = MachineType.Shell)
    {
        foreach (KeyValuePair<Command, CommandDocumentation> kvp in commandToDoc)
        {
            if ((kvp.Value.type == type || type == CommandType.None) && kvp.Value.machines.Contains(machine))
            {
                yield return GetDocumentationOf(kvp.Key, machine);
            }
        }
    }

    public static string GetNameOf(Command c)
    {
        return commandToDoc[c].name;
    }

    public static List<string> GetParametersOf(Command c)
    {
        return commandToDoc[c].parameters;
    }

    public static string GetDescriptionOf(Command c, MachineType m)
    {
        return commandToDoc[c].descriptions[commandToDoc[c].machines.IndexOf(m)];
    }
}

public class CommandDocumentation
{
    public List<string> parameters;
    public string name;
    public string[] descriptions;
    public CommandType type;
    public List<MachineType> machines;

    public CommandDocumentation(string[] parameters, Command name, string[] descriptions, CommandType type, List<MachineType> machines)
    {
        this.descriptions = descriptions;
        this.name = name.ToString();
        this.parameters = new List<string>(parameters);
        this.type = type;
        this.machines = machines;
    }
}

public abstract class Machine
{
    [HideInInspector]
    public MachineType type;

    [HideInInspector]
    public string name;

    public Machine(MachineType type, string name)
    {
        this.type = type;
        this.name = name;
    }
}

public class Shell : Machine
{
    public Shell() : base(MachineType.Shell, "Shell")
    {

    }
}