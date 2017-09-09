using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConsoleManager : MonoBehaviour
{
    #region Static members

    public static ConsoleManager instance = null;

    #endregion

    #region Public members

    [Header("Setup")]

    [Tooltip("If the console can be used at all.")]
    public bool consoleEnabled = true;

    [Tooltip("The input (as defined in the Input Manager) that will open and close the console.")]
    public string showConsoleInput = "ShowConsole";

    [Tooltip("The folder that will be scanned for commands. Must be in the 'Resources' hierarchy.")]
    public string commandsFolder = "Console/Commands";

    [Tooltip("The canvas that the console will be attached to when it is created.")]
    public Canvas canvas;

    #endregion

    #region Private members

    // The dictionary of commands
    private Dictionary<string, ICommand> commands = null;

    // The input field where the user enters commands.
    private InputField inputField = null;

    // The scroll rect for the console window
    private ScrollRect scrollRect = null;

    // The text inside the console window
    private Text outputText = null;

    // The GameObject that represents the entire console
    private GameObject consolePrefab = null;

    #endregion

    #region Monobehaviour methods

    private void Awake()
    {
        // If the console is enabled, then create one, otherwise don't bother.
        if (consoleEnabled) Initialize();

        // Set the static instance so this can be referenced easily.
        if (instance == null) instance = this;
    }

    private void Update()
    {
        if (consoleEnabled && Input.GetButtonDown(showConsoleInput))
            ToggleShow();
    }

    #endregion

    #region Public Methods

    public void ToggleShow()
    {
        // Activate the prefab
        consolePrefab.SetActive(!consolePrefab.activeSelf);
    }

    public void Print(string str)
    {
        outputText.text += str;
        ScrollToBottom();
    }

    public void Println(string str = "")
    {
        Print(str + "\n");
    }

    public void Clear()
    {
        outputText.text = "";
    }

    public void Submit(string cmd)
    {
        // Print the whole command into the console for reference
        Println("> " + cmd);

        // Parse the command
        List<string> args = ParseCommand(cmd);

        // If the command is empty, don't run anything.
        if (args.Count != 0)
        {
            // The first token is the command, and the whole list is passed as arguments
            // and print an error message if the command doesn't exist
            if (commands.ContainsKey(args[0])) commands[args[0]].Execute(args);
            else Println("'" + args[0] + "' is not a recognized command.");
        }

        // Refocus on the input field and clear it
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        inputField.OnPointerClick(new PointerEventData(EventSystem.current));
        inputField.text = "";

        ScrollToBottom();
    }

    #endregion

    #region Private methods

    private void Initialize()
    {
        // If the prefab is not null, we are already initialized.
        if (consolePrefab != null) return;

        // Spawn the console
        consolePrefab = Instantiate(Resources.Load("Console/Console")) as GameObject;
        consolePrefab.transform.SetParent(canvas.transform, false);
        consolePrefab.name = "Console";

        // Set the scroll view so we can control the scroll position
        scrollRect = consolePrefab.transform.GetChild(0).GetComponent<ScrollRect>();

        // Get the prefab's input and text fields
        inputField = consolePrefab.GetComponentInChildren<InputField>();
        outputText = consolePrefab.GetComponentInChildren<Text>();

        // Set up the input field to submit commands
        inputField.onEndEdit.AddListener(Submit);

        // Hide the console
        consolePrefab.SetActive(false);

        // Initialize the dictionary of commands
        commands = new Dictionary<string, ICommand>();

        // If we've already generated the commands dictionary, we're done.
        if (commands.Count != 0) return;

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
    }

    private void Cleanup()
    {
        // If the prefab is null, we're already not initialized.
        if (consolePrefab == null) return;

        // Set the component refs inside the prefab to null.
        inputField = null;
        scrollRect = null;
        outputText = null;

        // Note: the commands dictionary is NOT deleted to avoid
        //       regenerating it.

        // Destroy the console and set it to null.
        Destroy(consolePrefab);
        consolePrefab = null;
    }

    private List<string> ParseCommand(string cmd)
    {
        // Split an entire entered command into tokens, delimited
        // by spaces, but keeping quoted strings as a single token.
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

        // The last argument won't end in a space (probably), so add it here.
        arg = arg.Trim();
        if (arg != "") args.Add(arg);

        return args;
    }

    private void ScrollToBottom()
    {
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0;
        }
    }

    #endregion
}
