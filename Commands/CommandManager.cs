using JetBrains.Annotations;

namespace BepinExUtils.Console.Commands;

public class CommandManager
{
    public delegate void Command(string[] args);

    public delegate void OnCommandManagerInitEvent(CommandManager commandManager);

    private static readonly List<CommandInfo> DefaultCommands =
    [
        new()
        {
            Name = "help",
            Description = "Show command infos",
            Command = _ =>
            {
                var cmdInfos = Instance?._infos.Values.Select(cmd => $"{cmd.Name} - {cmd.Description}");
                if (cmdInfos == null) return;
                Utils.Logger.Info($"Available commands:\n{string.Join("\n", cmdInfos)}");
            }
        },
        new()
        {
            Name = "echo",
            Description = "Echo args",
            Command = args => Utils.Logger.Info(string.Join("\n", args))
        }
    ];

    private readonly Dictionary<string, CommandInfo> _infos = [];

    private CommandManager()
    {
        OnCommandManagerInit?.Invoke(this);
        Utils.Logger.Debug("CommandManager init");
    }

    [UsedImplicitly] public static CommandManager? Instance { get; private set; } = new();

    [UsedImplicitly] public static event OnCommandManagerInitEvent? OnCommandManagerInit;

    internal static void Init()
    {
        DefaultCommands.ForEach(AddCommand);
    }

    [UsedImplicitly]
    private void AddCommandInternal(CommandInfo commandInfo) => _infos.Add(commandInfo.Name, commandInfo);

    [UsedImplicitly]
    public static void AddCommand(CommandInfo commandInfo) => Instance?.AddCommandInternal(commandInfo);

    [UsedImplicitly]
    public static void AddCommand(string commandName, string description, Command command) => AddCommand(new()
    {
        Name = commandName,
        Description = description,
        Command = command
    });

    private bool TryExecuteCommandInternal(string command, params string[] args)
    {
        if (!_infos.TryGetValue(command, out var info)) return false;
        info.Command(args);
        return true;
    }

    [UsedImplicitly]
    public static bool TryExecuteCommand(string command, params string[] args) =>
        Instance != null && Instance.TryExecuteCommandInternal(command, args);
}