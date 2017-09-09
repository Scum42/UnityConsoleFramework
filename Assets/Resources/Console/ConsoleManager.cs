using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleManager : MonoBehaviour
{
    public static ConsoleManager instance = null;

    [Tooltip("If the console can be used at all.")]
    public bool consoleEnabled = true;
    [Tooltip("The input (as defined in the Input Manager) that will open and close the console.")]
    public string showConsoleInput = "ShowConsole";
    [Tooltip("The folder that will be scanned for commands. Must be in the 'Resources' hierarchy.")]
    public string commandsFolder = "Console/Commands";

    private Dictionary<string, ICommand> commands;
    InputField inputTextField;
    Text consoleText;
    GameObject console;

    private void Awake()
    {
        // Spawn the console
        console = Instantiate(Resources.Load("Console/Console")) as GameObject;
        console.transform.SetParent(GameObject.Find("Canvas").transform, false);

        // Get the prefab's input and text fields
        inputTextField = console.GetComponentInChildren<InputField>();
        consoleText = console.GetComponentInChildren<Text>();

        inputTextField.onEndEdit.AddListener(Submit);

        console.SetActive(false);
        
        // Initialize the dictionary of commands
        commands = new Dictionary<string, ICommand>();

        // Step through everything in the defined commands folder.
        foreach (UnityEngine.Object o in Resources.LoadAll(commandsFolder))
        {
            // The type of the asset as defined by it's name.
            Type oType = Type.GetType(o.name);

            // If the type inherits from ICommand, but is not ICommand itself...
            if (typeof(ICommand).IsAssignableFrom(oType) && oType != typeof(ICommand))
            {
                // ... make an instance of the current object.
                ICommand c = Activator.CreateInstance(oType) as ICommand;

                // Add the command to the dictionary, with its keyword as its key.
                commands.Add(c.GetKeyword(), c);
            }
        }

        if (!instance) instance = this;
    }

    private void Update()
    {
        if (consoleEnabled && Input.GetButtonDown(showConsoleInput))
            ToggleShow();
    }

    public void ToggleShow()
    {
        console.SetActive(!console.activeSelf);
        inputTextField.text = "";
    }

    public void Print(string str)
    {
        consoleText.text += str;
    }

    public void Println(string str)
    {
        Print(str);
        consoleText.text += "\n";
    }

    public void Submit(string cmd)
    {
        // Clear out the text field
        inputTextField.text = "";

        // First tokenize the string, keeping quoted strings.
        List<string> args = new List<string>();
        string arg = "";
        bool quoted = false;

        foreach (char c in cmd)
        {
            switch (c)
            {
                case '"':
                    quoted = !quoted;
                    break;

                case ' ':
                    if (!quoted)
                    {
                        arg = arg.Trim();
                        if (arg != "") args.Add(arg);
                        arg = "";
                    }
                    else
                    {
                        arg += c;
                    }
                    break;

                default:
                    arg += c;
                    break;
            }
        }

        // The last argument won't end in a space, so add it here.
        arg = arg.Trim();
        if (arg != "") args.Add(arg);

        // The first token is the command, and the whole list is passed as arguments.
        if (commands.ContainsKey(args[0])) commands[args[0]].Execute(args);
        else Println("'" + args[0] + "' is not a recognized command.");
    }
}
