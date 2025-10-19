using BepInExUtils.Commands;
using UnityEngine;

namespace BepinExUtils.Console.Behaviour;

public class CommandHandler : MonoBehaviour
{
    public CommandHandler() => Console.OnConsoleEnterCommand += OnConsoleEnterCommand;

    private static async Task OnConsoleEnterCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command)) return;
        var args = CommandSplit(command);
        if (args.Length < 1) return;
        var commandName = args[0];
        var commandArgs = args.Skip(1).ToArray();
        var success = await CommandManager.ExecuteCommand(commandName, commandArgs);
        if (!success)
            await Utils.Logger.ErrorAsync($"Unknown command: {commandName}");
    }

    private static string[] CommandSplit(string command)
    {
        var ret = new List<string>();
        var tempStr = "";
        var isStrArg = false;

        foreach (var commandChar in command)
            switch (commandChar)
            {
                case ' ':
                    if (isStrArg)
                    {
                        tempStr += commandChar;
                        break;
                    }

                    Clear();
                    break;
                case '"':
                    if (isStrArg)
                        Clear();
                    else
                        tempStr = "";

                    isStrArg = !isStrArg;
                    break;
                default:
                    tempStr += commandChar;
                    break;
            }

        Clear();
        return ret.ToArray();

        void Clear()
        {
            if (string.IsNullOrEmpty(tempStr)) return;
            ret.Add(tempStr);
            tempStr = "";
        }
    }
}