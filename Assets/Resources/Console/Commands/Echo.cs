using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Echo : ICommand
{
    public string GetKeyword()
    {
        return "echo";
    }

    public void Execute(List<string> args)
    {
        if (args.Count > 1)
            ConsoleManager.instance.Println(args[1]);
        else
            ConsoleManager.instance.Println();
    }
}
