using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BepinExUtils.Console.Behaviour;

[PublicAPI]
public class Console : MonoBehaviour
{
    public delegate Task OnConsoleEnterCommandEvent(string command);

    private const string ResourcePath = "BepinExUtils.Console.Resources.AssetBundles.bepinexutils.console.bundle";
    private const string CanvasPath = "Assets/BepinExUtils.Console/Canvas.prefab";
    private const string LogTextPath = "Assets/BepinExUtils.Console/LogText.prefab";
    private const string LoggerBoxTransform = "Panel/LoggerBox";
    private const string ContentTransform = "Panel/LoggerBox/Viewport/Content";
    private const string InputTransform = "Panel/InputBox/Input";
    private const string InputButtonTransform = "Panel/InputBox/EnterButton";
    private const string TitleBoxTransform = "Panel/TitleBox";
    private const string HandleLeftTransform = "Panel/HandleLeft";
    private const string HandleRightTransform = "Panel/HandleRight";
    private const string HandleUpTransform = "Panel/HandleUp";
    private const string HandleDownTransform = "Panel/HandleDown";
    private const string HandleUpLeftTransform = "Panel/HandleUpLeft";
    private const string HandleUpRightTransform = "Panel/HandleUpRight";
    private const string HandleDownLeftTransform = "Panel/HandleDownLeft";
    private const string HandleDownRightTransform = "Panel/HandleDownRight";
    private static bool _isAwake;
    private static GameObject? _logTextPrefab;
    private static GameObject? _console;
    private static GameObject? _consoleContent;
    private static GameObject? _consoleInput;
    private static Console? _instance;
    private static ScrollRect? _scrollRect;
    private static VerticalLayoutGroup? _verticalLayoutGroup;
    private static ContentSizeFitter? _contentSizeFitter;
    private static readonly List<InputField> LOGList = [];
    internal static bool LastVisible;
    internal static CursorLockMode LastLockState;
    internal static bool CurrentlySettingCursor;

    public static bool Active
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            _console?.SetActive(value);
            CurrentlySettingCursor = true;

            // TODO: Set EventSystem.current

            if (value)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                ToTheButton();
            }
            else
            {
                Cursor.lockState = LastLockState;
                Cursor.visible = LastVisible;
            }

            CurrentlySettingCursor = false;
        }
    }

    public static Console Instance => _instance ?? throw new NullReferenceException(nameof(Instance));

    private void Awake()
    {
        if (_isAwake)
        {
            Destroy(gameObject);
            return;
        }

        _isAwake = true;
        _instance = this;
        LastVisible = Cursor.visible;
        LastLockState = Cursor.lockState;
        CreateConsole();
    }

    private void Update()
    {
        if (!UnityInput.Current.GetKeyDown(Configs.ConsoleToggleKey)) return;
        Active = !Active;
    }

    private void OnDestroy()
    {
        _isAwake = false;
    }

    public static event OnConsoleEnterCommandEvent? OnConsoleEnterCommand;

    private void CreateConsole()
    {
        var asm = Assembly.GetExecutingAssembly();
        Utils.Logger.Debug($"asm: {asm}");
        var bundle = asm.GetEmbeddedAssetBundle(ResourcePath);
        Utils.Logger.Debug($"bundle: {bundle}");

        foreach (var assetName in bundle.GetAllAssetNames())
            Utils.Logger.Debug($"assetName: {assetName}");

        var prefab = bundle.LoadAsset<GameObject>(CanvasPath);
        Utils.Logger.Debug($"prefab: {prefab}");
        var obj = Instantiate(prefab, transform, true);
        Utils.Logger.Debug($"obj: {obj}");
        _console = obj;

        var loggerBoxTransform = obj.transform.Find(LoggerBoxTransform);
        if (loggerBoxTransform && loggerBoxTransform.TryGetComponent<ScrollRect>(out var scrollRect))
            _scrollRect = scrollRect;

        var contentTransform = obj.transform.Find(ContentTransform);
        if (contentTransform)
            _consoleContent = contentTransform.gameObject;

        if (contentTransform && contentTransform.TryGetComponent<VerticalLayoutGroup>(out var verticalLayoutGroup))
            _verticalLayoutGroup = verticalLayoutGroup;

        if (contentTransform && contentTransform.TryGetComponent<ContentSizeFitter>(out var contentSizeFitter))
            _contentSizeFitter = contentSizeFitter;

        var inputTransform = obj.transform.Find(InputTransform);
        if (inputTransform && inputTransform.TryGetComponent<InputField>(out var input))
        {
            input.onSubmit.AddListener(Enter);
            _consoleInput = inputTransform.gameObject;
        }

        var inputButtonTransform = obj.transform.Find(InputButtonTransform);
        if (inputButtonTransform && inputButtonTransform.TryGetComponent<Button>(out var button))
            button.onClick.AddListener(ButtonClick);

        var titleBoxTransform = obj.transform.Find(TitleBoxTransform);
        titleBoxTransform?.gameObject.AddComponent<ConsoleTitleBox>();

        var handleLeftTransform = obj.transform.Find(HandleLeftTransform);
        var handleLeft = handleLeftTransform?.gameObject.AddComponent<ConsoleHandle>();
        handleLeft?.SetAxis(ConsoleHandle.Axis.Horizontal);

        var handleRightTransform = obj.transform.Find(HandleRightTransform);
        var handleRight = handleRightTransform?.gameObject.AddComponent<ConsoleHandle>();
        handleRight?.SetAxis(ConsoleHandle.Axis.Horizontal);

        var handleUpTransform = obj.transform.Find(HandleUpTransform);
        var handleUp = handleUpTransform?.gameObject.AddComponent<ConsoleHandle>();
        handleUp?.SetAxis(ConsoleHandle.Axis.Vertical);

        var handleDownTransform = obj.transform.Find(HandleDownTransform);
        var handleDown = handleDownTransform?.gameObject.AddComponent<ConsoleHandle>();
        handleDown?.SetAxis(ConsoleHandle.Axis.Vertical);

        var handleUpLeftTransform = obj.transform.Find(HandleUpLeftTransform);
        var handleUpLeft = handleUpLeftTransform?.gameObject.AddComponent<ConsoleHandle>();
        handleUpLeft?.SetAxis(ConsoleHandle.Axis.Diagonal);

        var handleUpRightTransform = obj.transform.Find(HandleUpRightTransform);
        var handleUpRight = handleUpRightTransform?.gameObject.AddComponent<ConsoleHandle>();
        handleUpRight?.SetAxis(ConsoleHandle.Axis.Diagonal);

        var handleDownLeftTransform = obj.transform.Find(HandleDownLeftTransform);
        var handleDownLeft = handleDownLeftTransform?.gameObject.AddComponent<ConsoleHandle>();
        handleDownLeft?.SetAxis(ConsoleHandle.Axis.Diagonal);

        var handleDownRightTransform = obj.transform.Find(HandleDownRightTransform);
        var handleDownRight = handleDownRightTransform?.gameObject.AddComponent<ConsoleHandle>();
        handleDownRight?.SetAxis(ConsoleHandle.Axis.Diagonal);

        var prefab2 = bundle.LoadAsset<GameObject>(LogTextPath);
        Utils.Logger.Debug($"prefab2: {prefab2}");
        _logTextPrefab = prefab2;
        obj.SetActive(false);
    }

    public static void ToTheButton()
    {
        if (!_verticalLayoutGroup || !_contentSizeFitter || !_scrollRect) return;
        Canvas.ForceUpdateCanvases();
        _verticalLayoutGroup
            .CalculateLayoutInputHorizontal(); // If not call this method first [NullReferenceException] will happen. Very funny.
        _verticalLayoutGroup.CalculateLayoutInputVertical();
        _contentSizeFitter.SetLayoutVertical();
        _scrollRect.verticalNormalizedPosition = 0f;
    }

    public static async Task RawLog(string message)
    {
        if (!_consoleContent || !_logTextPrefab || string.IsNullOrEmpty(message)) return;
        var objs = await InstantiateAsync(_logTextPrefab, new InstantiateParameters
        {
            worldSpace = false,
            parent = _consoleContent.transform
        });
        var obj = objs?.GetValueOrDefault(0);
        if (!obj || !obj.TryGetComponent<InputField>(out var inputField)) return;
        inputField.text = message;
        LOGList.Add(inputField);

        if (LOGList.Count > Configs.ConsoleLogLimit)
        {
            Destroy(LOGList[0].gameObject);
            LOGList.RemoveAt(0);
        }

        if (obj.TryGetComponent<ContentSizeFitter>(out var contentSizeFitter))
            contentSizeFitter.SetLayoutVertical();
        ToTheButton();
    }

    public static void Info(string message) => _ = RawLog($"<color=white>{message}</color>");
    public static async Task InfoAsync(string message) => await RawLog($"<color=white>{message}</color>");
    public static void Warning(string message) => _ = RawLog($"<color=yellow>{message}</color>");
    public static async Task WarningAsync(string message) => await RawLog($"<color=yellow>{message}</color>");
    public static void Error(string message) => _ = RawLog($"<color=red>{message}</color>");
    public static async Task ErrorAsync(string message) => await RawLog($"<color=red>{message}</color>");
    public static void Debug(string message) => _ = RawLog($"<color=green>{message}</color>");
    public static async Task DebugAsync(string message) => await RawLog($"<color=green>{message}</color>");

    private static void Enter(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        _ = RawLog($"<color=gray> > {message}</color>");
        ClearInputField();
        Task.Run(() => OnConsoleEnterCommand?.Invoke(message));
    }

    private static void ClearInputField()
    {
        if (!_consoleInput || !_consoleInput.TryGetComponent<InputField>(out var input)) return;
        input.text = "";
    }

    private static void ButtonClick()
    {
        if (!_consoleInput || !_consoleInput.TryGetComponent<InputField>(out var input)) return;
        Enter(input.text);
    }

    internal sealed class LogListener : ILogListener
    {
        public void Dispose()
        {
        }

        [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            var log = string.Format(Configs.ConsoleLogFormat, DateTime.Now, eventArgs.Level,
                eventArgs.Source.SourceName, eventArgs.Data);

            switch (eventArgs.Level)
            {
                case LogLevel.Fatal:
                case LogLevel.Error:
                    Error(log);
                    break;
                case LogLevel.Debug:
                    Debug(log);
                    break;
                case LogLevel.Warning:
                    Warning(log);
                    break;
                default:
                    Info(log);
                    break;
            }
        }
    }
}