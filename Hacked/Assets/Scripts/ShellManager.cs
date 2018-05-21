using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum Command
{
    Help, Windows, Modules, Load, Parameter_error, Name_error, Unload, Load_database, Back, Files, Open, Error
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
    public GameObject printList;
    public GameObject row;
    public GameObject column;
    public GameObject listName;

    private bool HasStoppedInputting;
    private List<string> inputHistory = new List<string>();
    private int currentHistoryIndex = -1;

    [Header("Texts")]
    [TextArea]
    public string nameErrorText;
    [TextArea]
    public string paramErrorText;

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

        CommandData c = ParseCommand(commandText.text);
        ExecuteCommand(c);

        ScrollConsole();

        InputText = string.Empty;

        input.ActivateInputField();
    }

    /// <summary>
    /// Takes a string as an input and tries to turn divide into a command and parametors.
    /// </summary>
    /// <param name="rawData">The string to parse</param>
    /// <returns>A CommandData. If there is an error during the parse the CommandData will have Command.Error as its command</returns>
    public CommandData ParseCommand(string rawData)
    {
        List<string> choppedData = new List<string>(rawData.Split(' '));
        CommandData command = new CommandData();

        try
        {
            command.c = (Command)Enum.Parse(typeof(Command), FormatString(choppedData[0]));
            choppedData.RemoveAt(0);
        }
        catch (ArgumentException)
        {
            Print(nameErrorText);
            return CommandData.Error;
        }

        if (!CommandHelper.CommandIsOfType(command.c, CurrentMachine.type))
        {
            Print(nameErrorText);
            return CommandData.Error;
        }

        if (choppedData.Count != CommandHelper.GetParametersOf(command.c).Count)
        {
            Print(paramErrorText);
            return CommandData.Error;
        }
        else
        {
            command.parameters = choppedData;
        }

        return command;
    }

    public void ExecuteCommand(CommandData commandData)
    {
        if (commandData.c == Command.Error) return;

        List<string> parameters = commandData.parameters;
        Command c = commandData.c;
        

        //WARNING! DON'T UNCOLLAPSE! ALL CODE FOR EACH OF THE COMMANDS INSIDE!
        switch (c)
        {
            case Command.Help:
                string str = string.Empty;
                IEnumerable<string> sNames;
                IEnumerable<string> sDocumentation;

                sNames = CommandHelper.GetAllNamesOfType(CommandType.Basic);
                sDocumentation = CommandHelper.GetAllDocumentationOfType(CommandType.Basic);
                Print("=== Basic Commands ===\n", true);

                List<ListElement> names = new List<ListElement>();
                List<ListElement> documentation = new List<ListElement>();

                foreach(string n in sNames)
                {
                    names.Add(new ListElement(n));
                }

                foreach (string d in sDocumentation)
                {
                    documentation.Add(new ListElement(d));
                }

                List<ListColumn> columns = new List<ListColumn>();
                columns.Add(new ListColumn("Name", names));
                columns.Add(new ListColumn("Description", documentation));

                PrintList(columns);
                
                if (CurrentMachine.type == MachineType.Shell)
                {
                    Print("\n === For a list of Advanced Commands visit the help module === ", true);
                }
                break;

            case Command.Modules:
                columns = new List<ListColumn>();
                List<ListElement> modules = new List<ListElement>();
                List<ListElement> status = new List<ListElement>();

                foreach (ModuleType m in windowManager.GetModules())
                {
                    modules.Add(new ListElement(m.ToString()));
                    status.Add(new ListElement(windowManager.ModuleToStatus[m].ToString()));
                    if (windowManager.ModuleToStatus[m] == Status.Used)
                    {
                        status[status.Count - 1].color = new Color(1, 0, 0, 0.5f);
                        modules[modules.Count - 1].color = new Color(1, 0, 0, 0.5f);
                    }
                }

                columns.Add(new ListColumn("Name", modules));
                columns.Add(new ListColumn("Status", status));

                PrintList(columns);
                break;

            case Command.Windows:
                string output = string.Empty;
                foreach (int window in windowManager.GetWindows())
                {
                    output += "\t\n" + window + " - " + windowManager.WindowToStatus[windowManager.IndexToWindow[window]].ToString();
                }
                Print(output.Substring(2));
                break;

            case Command.Load:
                string mString = FormatString(parameters[0]);

                ModuleType mod = ModuleType.None;
                try
                {
                    mod = (ModuleType)Enum.Parse(typeof(ModuleType), mString);
                }
                catch (ArgumentException)
                {
                    Print("The module requested is not avalible. Type 'Modules' for a list of avalible modules.");
                    return;
                }

                if (!windowManager.ModuleIsOpenableByPlayer(mod) || windowManager.ModuleToStatus[mod] == Status.Used)
                {
                    Print("The module requested is not avalible. Type 'Modules' for a list of avalible modules.");
                    return;
                }

                int w = int.Parse(parameters[1]);

                if (!windowManager.WindowAvalible(w))
                {
                    Print("The requested window is currently in use or doesn't exist. Type 'Windows' for a list of avalible windows");
                    return;
                }

                windowManager.LoadModuleOnWindow(mod, w);

                Print("The requested module " + parameters[0] + " has succesfully been loaded on window " + parameters[1] + ".");
                break;

            case Command.Unload:
                if (windowManager.WindowAvalible(int.Parse(parameters[0])))
                {
                    Print("Window either doesn't exist or already has module loaded on it. For more info, tpye 'help' or visit the 'commands' page in the shell help menu.");
                    return;
                }

                if (windowManager.WindowToModule[windowManager.IndexToWindow[int.Parse(parameters[0])]] == ModuleType.Shell)
                {
                    Print("You can't unload the shell from shell!");
                    return;
                }

                windowManager.UnloadOnWindow(int.Parse(parameters[0]));
                break;

            case Command.Load_database:
                string name = parameters[0];

                if (!databaseMan.HasDatabaseByName(name))
                {
                    Print("The database you are trying to access doesn't exist. Did you spell the name right?");
                    return;
                }

                CurrentMachine = databaseMan.GetDataBase(name);

                Print("You are now in database " + parameters[0] + ". To get back, type the command 'Back'.");
                break;

            case Command.Back:
                CurrentMachine = new Shell();
                Print("You're now in the shell.");
                break;

            case Command.Files:
                if ((CurrentMachine as DataBase).files.Length == 0)
                {
                    Print("Could not list the files on this database since it doesn't have any.");
                    return;
                }

                string tempS = string.Empty;

                foreach (File file in (CurrentMachine as DataBase).files)
                {
                    tempS += file.name + "\n";
                }

                Print(tempS);
                break;

            case Command.Open:
                File? fn = (CurrentMachine as DataBase).GetFile(parameters[0]);

                if (fn == null)
                {
                    Print("Could not open the requested file since there's no file named " + parameters[0] + ". Did you misspel the name?");
                    return;
                }

                File f = fn.Value;

                Print("==== File begin ====", true);
                Print(f.file.text);
                Print("==== File end ====\n", true);
                break;
        }
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

    public void PrintList(List<ListColumn> columns)
    {
        GameObject list = Instantiate(printList, content.transform);

        Transform columnNames = list.transform.GetChild(1);
        Transform data = list.transform.GetChild(0);

        foreach (ListColumn column in columns)
        {
            Instantiate(listName, columnNames).GetComponent<Text>().text = column.header;

            GameObject c = Instantiate(this.column, data);
            foreach (ListElement element in column.values)
            {
                Text e = Instantiate(row, c.transform).GetComponent<RectTransform>().GetChild(0).GetComponent<Text>();
                e.text = element.text;
                e.color = element.color;
            }
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

    public static IEnumerable<string> GetAllDocumentationOfType(CommandType type, MachineType machine = MachineType.Shell)
    {
        foreach (KeyValuePair<Command, CommandDocumentation> kvp in commandToDoc)
        {
            if ((kvp.Value.type == type || type == CommandType.None) && kvp.Value.machines.Contains(machine))
            {
                yield return GetDescriptionOf(kvp.Key, machine);
            }
        }
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

    public static IEnumerable<string> GetAllFullDocumentationOfType(CommandType type, MachineType machine = MachineType.Shell)
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

    public static IEnumerable<string> GetAllNamesOfType(CommandType type, MachineType machine = MachineType.Shell)
    {
        foreach (KeyValuePair<Command, CommandDocumentation> kvp in commandToDoc)
        {
            if ((kvp.Value.type == type || type == CommandType.None) && kvp.Value.machines.Contains(machine))
            {
                yield return GetNameOf(kvp.Key);
            }
        }
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

public class ListColumn
{
    public string header;
    public List<ListElement> values;

    public ListColumn(string header, List<ListElement> values)
    {
        this.header = header;
        this.values = values;
    }
}

public class ListElement
{
    public string text;
    public Color color;

    public ListElement(string text, bool defaultColor = true, Color color = new Color())
    {
        this.color = color;
        if (defaultColor)
        {
            this.color = new Color(0, 1, 0.007843138f, 0.4980392f);
        }
        this.text = text;
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

public class CommandData
{
    public List<string> parameters;
    public Command c;

    public static CommandData Error {
        get {
            return new CommandData(new List<string>(), Command.Error);
        }
    }

    public CommandData(List<string> parameters, Command c)
    {
        this.c = c;
        this.parameters = parameters;
    }

    public CommandData () {

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