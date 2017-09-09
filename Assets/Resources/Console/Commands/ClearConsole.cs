using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearConsole : ICommand
{
    public string GetKeyword()
    {
        return "clear";
    }

    public void Execute(List<string> args)
    {
        ConsoleManager.instance.Clear();
    }
}
