using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand
{
    // The code to be executed when the command is invoked.
    void Execute(List<string> args);

    // The keyword that will invoke the command in the console.
    string GetKeyword();
}
