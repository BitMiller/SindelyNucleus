using UnityEditor;
using UnityEditor.Compilation;

public static class CompileScripts
{
    [MenuItem("Tools/Compile Scripts")]
    public static void Compile()
    {
        CompilationPipeline.RequestScriptCompilation();
    }
}