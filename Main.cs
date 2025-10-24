using BepInEx;
using BepInExUtils.Attributes;
using BepinExUtils.Console.Behaviour;
using UnityEngine;
using BepInExLogger = BepInEx.Logging.Logger;

namespace BepinExUtils.Console;

[BepInUtils("io.github.ykysnk.BepinExUtils.Console", "BepinEx Utils Console", Version)]
[BepInDependency("io.github.ykysnk.BepinExUtils", "0.9.0")]
[ConfigBind<KeyCode>("ConsoleToggleKey", SectionOptions, KeyCode.F2, "Toggle Console")]
[ConfigBind<int>("ConsoleLogLimit", SectionOptions, 1000, "Max log count in Console")]
[ConfigBind<string>("ConsoleLogFormat", SectionOptions, "[{0:HH:mm:ss.fffffff}] [{1}] ({2}): {3}", "Log format")]
public partial class Main
{
    private const string SectionOptions = "Options";
    private const string Version = "0.0.2";

    public void Init()
    {
        var obj = new GameObject("BepinExUtils.Console", typeof(Behaviour.Console), typeof(CommandHandler));
        DontDestroyOnLoad(obj);
        obj.hideFlags |= HideFlags.HideAndDontSave;
        BepInExLogger.Listeners.Add(new Behaviour.Console.LogListener());
    }
}