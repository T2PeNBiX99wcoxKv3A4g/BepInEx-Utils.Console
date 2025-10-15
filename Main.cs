using BepInEx;
using BepInExUtils.Attributes;
using BepinExUtils.Console.Behaviour;
using UnityEngine;

namespace BepinExUtils.Console;

[BepInUtils("io.github.ykysnk.BepinExUtils.Console", "BepinEx Utils Console", Version)]
[BepInDependency("io.github.ykysnk.BepinExUtils", "0.8.1")]
[ConfigBind<KeyCode>("ConsoleToggleKey", SectionOptions, KeyCode.F2, "Toggle Console")]
[ConfigBind<int>("ConsoleLogLimit", SectionOptions, 1000, "Max log count in Console")]
[ConfigBind<string>("ConsoleLogFormat", SectionOptions, "[{0:HH:mm:ss}] [{1}] ({2}): {3}", "Log format")]
public partial class Main
{
    private const string SectionOptions = "Options";
    private const string Version = "0.0.1";

    protected override void PostAwake()
    {
        var obj = new GameObject("BepinExUtils.Console", typeof(Behaviour.Console), typeof(CommandHandler));
        DontDestroyOnLoad(obj);
        obj.hideFlags |= HideFlags.HideAndDontSave;
        BepInEx.Logging.Logger.Listeners.Add(new Behaviour.Console.LogListener());
    }
}