using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCommand : ICommand
{
    public string GetKeyword()
    {
        return "test";
    }

    public void Execute(List<string> args)
    {
        Debug.Log("Test command executed.");
    }
}
