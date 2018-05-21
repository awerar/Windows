using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Actions;

public enum Response
{
    Succes, None, WindowError, ModuleError, Finished, NameError, CountError
}

public abstract class Action
{
    protected ActionData data;

    protected WindowManager windowManager;
    protected ShellManager shell;
    protected DataBaseManager databaseMan;

    public static Dictionary<Command, Action> CommandToAction = new Dictionary<Command, Action>()
    {
        {Command.Load, new Load()},
        {Command.Help, new Help() },
        {Command.Modules, new Modules() },
        {Command.Name_error, new NameError() },
        {Command.Parameter_error, new ParameterError() },
        {Command.Windows, new Windows() },
        {Command.Unload, new Unload() },
        {Command.Load_database, new Load_database() },
        {Command.Back, new Back() },
        {Command.Files, new Files() },
        {Command.Open, new Open() }
    };

    public void Initialize(ActionData data, ShellManager shell, WindowManager windowManager, DataBaseManager databaseMan)
    {
        this.data = data;
        this.shell = shell;
        this.windowManager = windowManager;
        this.databaseMan = databaseMan;
    }

    public static Action Create (ActionData data, ShellManager shell, WindowManager windowManager, DataBaseManager databaseMan)
    {
        CommandToAction[data.c].Initialize(data, shell, windowManager, databaseMan);
        return CommandToAction[data.c];
    }

    public abstract Response Execute();
}

public class ActionData
{
    public Command c;
    public Response r;
    public List<string> parameters;

    public ActionData(string rawInput, ShellManager shell, Response r = Response.None)
    {
        this.r = r;

        List<string> choppedData = new List<string>(rawInput.Split(' '));

        try
        {
            c = (Command)Enum.Parse(typeof(Command), ShellManager.FormatString(choppedData[0]));
            choppedData.RemoveAt(0);
        }
        catch (ArgumentException)
        {
            c = Command.Name_error;
            return;
        }

        if (!CommandHelper.CommandIsOfType(c, shell.CurrentMachine.type))
        {
            c = Command.Name_error;
            return;
        }

        if (choppedData.Count != CommandHelper.GetParametersOf(c).Count)
        {
            c = Command.Parameter_error;
            return;
        }
        else
        {
            parameters = choppedData;
        }
    }
}

namespace Actions
{
    public class Help : Action
    {
        public override Response Execute()
        {
            switch (data.r)
            {
                case Response.None:
                    string str = string.Empty;
                    IEnumerable<string> documentetion = new List<string>();

                    switch (shell.CurrentMachine.type)
                    {
                        case MachineType.Shell:
                            documentetion = CommandHelper.GetAllFullDocumentationOfType(CommandType.Basic);
                            shell.Print("=== Basic Commands ===\n", true);
                            break;

                        case MachineType.Database:
                            documentetion = CommandHelper.GetAllFullDocumentationOfType(CommandType.None, MachineType.Database);
                            shell.Print("=== Commands ===\n", true);
                            break;
                    }

                    foreach (string s in documentetion)
                    {
                        str += "\n\n\t" + s;
                    }

                    str = str.Substring(2);

                    shell.Print(str);

                    if (shell.CurrentMachine.type == MachineType.Shell)
                    {
                        shell.Print("\n === For a list of Advanced Commands visit the help module === ", true);
                    }

                    return Response.Finished;
            }
            return Response.Finished;
        }
    }

    public class NameError : Action
    {
        public override Response Execute()
        {
            switch (data.r)
            {
                case Response.None:
                    shell.Print("Unknown command! Type 'Help' for a list of basic commands!");
                    return Response.Finished;
            }
            return Response.Finished;
        }
    }

    public class ParameterError : Action
    {
        public override Response Execute()
        {
            switch (data.r)
            {
                case Response.None:
                    shell.Print("Wrong ammount of paramaters added! Type 'Help' for more info!");
                    return Response.Finished;
            }
            return Response.Finished;
        }
    }

    public class Modules : Action
    {
        public override Response Execute()
        {
            switch (data.r)
            {
                case Response.None:
                    List<ListColumn> columns = new List<ListColumn>();
                    List<ListElement> modules = new List<ListElement>();
                    List<ListElement> status = new List<ListElement>();

                    foreach(ModuleType m in windowManager.GetModules())
                    {
                        modules.Add(new ListElement(m.ToString()));
                        status.Add(new ListElement(windowManager.ModuleToStatus[m].ToString()));
                        if (windowManager.ModuleToStatus[m] == Status.Used)
                        {
                            status[status.Count - 1].color = new Color(1, 0, 0, 0.5f);
                        }
                    }

                    columns.Add(new ListColumn("Name", modules));
                    columns.Add(new ListColumn("Status", status));

                    shell.PrintList(columns);

                    /*string output = string.Empty;
                    foreach (ModuleType module in windowManager.GetModules())
                    {
                        output += "\t\n" + module.ToString() + " - " + windowManager.ModuleToStatus[module].ToString();
                    }
                    shell.Print(output.Substring(2));*/
                    return Response.Finished;
            }
            return Response.Finished;
        }
    }

    public class Windows : Action
    {
        public override Response Execute()
        {
            switch (data.r) {
                case Response.None:
                    string output = string.Empty;
                    foreach (int window in windowManager.GetWindows())
                    {
                        output += "\t\n" + window + " - " + windowManager.WindowToStatus[windowManager.IndexToWindow[window]].ToString();
                    }
                    shell.Print(output.Substring(2));
                    return Response.Finished;
            }
            return Response.Finished;
        }
    }

    public class Load : Action
    {
        public override Response Execute()
        {
            switch (data.r)
            {
                case Response.None:
                    string mString = ShellManager.FormatString(data.parameters[0]);

                    ModuleType m;
                    try
                    {
                        m = (ModuleType)Enum.Parse(typeof(ModuleType), mString);
                    }
                    catch (ArgumentException)
                    {
                        return Response.ModuleError;
                    }

                    if (!windowManager.ModuleIsOpenableByPlayer(m) || windowManager.ModuleToStatus[m] == Status.Used)
                    {
                        return Response.ModuleError;
                    }

                    int w = int.Parse(data.parameters[1]);

                    if (!windowManager.WindowAvalible(w))
                    {
                        return Response.WindowError;
                    }

                    windowManager.LoadModuleOnWindow(m, w);
                    return Response.Succes;

                case Response.ModuleError:
                    shell.Print("The module requested is not avalible. Type 'Modules' for a list of avalible modules.");
                    return Response.Finished;

                case Response.WindowError:
                    shell.Print("The requested window is currently in use or doesn't exist. Type 'Windows' for a list of avalible windows");
                    return Response.Finished;

                case Response.Succes:
                    shell.Print("The requested module " + data.parameters[0] + " has succesfully been loaded on window " + data.parameters[1] + ".");
                    return Response.Finished;
            }
            return Response.Finished;
        }
    }

    public class Unload : Action
    {
        public int timesTriedToUnloadOnShell = 0;

        public override Response Execute()
        {
            switch(data.r)
            {
                case Response.None:
                    if (windowManager.WindowAvalible(int.Parse(data.parameters[0])))
                    {
                        return Response.WindowError;
                    }

                    if (windowManager.WindowToModule[windowManager.IndexToWindow[int.Parse(data.parameters[0])]] == ModuleType.Shell)
                    {
                        timesTriedToUnloadOnShell++;
                        return Response.ModuleError;
                    }

                    windowManager.UnloadOnWindow(int.Parse(data.parameters[0]));

                    return Response.Finished;

                case Response.WindowError:
                    shell.Print("Window either doesn't exist or doesn't have module loaded on it. For more info, tpye 'help' or visit the 'commands' page in the shell help menu.");
                    return Response.Finished;

                case Response.ModuleError:
                    /*switch(timesTriedToUnloadOnShell)
                    {
                        case 1:
                            shell.Print("You can't unload the module you're on, dummy! How would you then continue? Don't try this again!");
                            break;

                        case 2:
                            shell.Print("What part of 'Dont try this again did you not get?'");
                            break;

                        case 3:
                            shell.Print("Why would you even want to unload this module?");
                            break;

                        case 4:
                            shell.Print("IF YOU TRY THIS ONCE MORE...");
                            break;

                        case 5:
                            windowManager.UnloadOnWindow(windowManager.WindowToIndex[windowManager.ModuleToWindow[ModuleType.Main_menu]]);
                            shell.Print("Well, how about now? Do you STILL wanna unload this window?");
                            break;

                        case 6:
                            windowManager.UnloadOnWindow(windowManager.WindowToIndex[windowManager.ModuleToWindow[ModuleType.Shell]]);
                            break;
                    }*/
                    return Response.Finished;
            }

            return Response.Finished;
        }
    }

    public class Load_database : Action
    {
        public override Response Execute()
        {
            switch (data.r)
            {
                case Response.None:
                    string name = data.parameters[0];

                    if (!databaseMan.HasDatabaseByName(name))
                    {
                        return Response.NameError;
                    }

                    shell.CurrentMachine = databaseMan.GetDataBase(name);
                    return Response.Succes;

                case Response.NameError:
                    shell.Print("The database you are trying to access doesn't exist. Did you spell the name right?");
                    return Response.Finished;

                case Response.Succes:
                    shell.Print("You are now in database " + data.parameters[0] + ". To get back, type the command 'Back'.");
                    break;
            }
            return Response.Finished;
        }
    }

    public class Back : Action
    {
        public override Response Execute()
        {
            switch (data.r) {

                case Response.None:
                shell.CurrentMachine = new Shell();
                return Response.Succes;

                case Response.Succes:
                    shell.Print("You're now in the shell.");
                    return Response.Finished;
            }
            return Response.Finished;
        }
    }

    public class Files : Action
    {
        public override Response Execute()
        {
            switch (data.r) {
                case Response.None:
                    if ((shell.CurrentMachine as DataBase).files.Length == 0)
                    {
                        return Response.CountError;
                    }

                    string str = string.Empty;

                    foreach (File f in (shell.CurrentMachine as DataBase).files)
                    {
                        str += f.name + "\n";
                    }

                    shell.Print(str);
                    break;

                case Response.CountError:
                    shell.Print("Could not list the files on this database since it doesn't have any.");
                    break;
            }

            return Response.Finished;
        }
    }

    public class Open : Action
    {
        public override Response Execute()
        {
            switch (data.r)
            {
                case Response.None:
                    File? fn = (shell.CurrentMachine as DataBase).GetFile(data.parameters[0]);

                    if (fn == null)
                    {
                        return Response.NameError;
                    }

                    File f = fn.Value;

                    shell.Print("==== File begin ====", true);
                    shell.Print(f.file.text);
                    shell.Print("==== File end ====\n", true);
                    break;

                case Response.NameError:
                    shell.Print("Could not open the requested file since there's no file named " + data.parameters[0] + ". Did you misspel the name?");
                    break;
            }

            return Response.Finished;
        }
    }
}
