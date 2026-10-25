using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;

[InitializeOnLoad]
public static class CompileAndPlayRunner
{
    private const string PendingKey =
        "BitMiller.CompileAndPlay.Pending";

    private static int idleUpdates;

    static CompileAndPlayRunner()
    {
        if (SessionState.GetBool(PendingKey, false))
        {
            idleUpdates = 0;
            EditorApplication.update -= WaitForUnity;
            EditorApplication.update += WaitForUnity;
        }
    }

    public static void Start()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (SessionState.GetBool(PendingKey, false))
        {
            return;
        }

        SessionState.SetBool(PendingKey, true);
        idleUpdates = 0;

        EditorApplication.update -= WaitForUnity;
        EditorApplication.update += WaitForUnity;

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        //AssetDatabase.Refresh();
    }

    private static void WaitForUnity()
    {
        if (!SessionState.GetBool(PendingKey, false))
        {
            EditorApplication.update -= WaitForUnity;
            return;
        }

        if (EditorApplication.isCompiling ||
            EditorApplication.isUpdating)
        {
            idleUpdates = 0;
            return;
        }

        idleUpdates++;

        if (idleUpdates < 5)
        {
            return;
        }

        EditorApplication.update -= WaitForUnity;
        SessionState.SetBool(PendingKey, false);

        EditorApplication.EnterPlaymode();
    }
}

[EditorToolbarElement(ID, typeof(SceneView))]
public class CompileAndPlayButton : EditorToolbarButton
{
    public const string ID = "BitMiller/CompileAndPlay";

    public CompileAndPlayButton()
    {
        text = "Compile + Play";
        tooltip = "Refresh assets, compile, then enter Play mode";
        clicked += CompileAndPlayRunner.Start;
    }
}

[Overlay(typeof(SceneView), "Compile and Play")]
public class CompileAndPlayToolbar : ToolbarOverlay
{
    public CompileAndPlayToolbar()
        : base(CompileAndPlayButton.ID)
    {
    }
}
