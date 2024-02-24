using UnityEditor;
using UnityEngine;

public class ReadmeWindow : EditorWindow
{
    private const string ReadmeShownKey = "ReadmeShown";

    private string readmeText = "Thank you for using the descenders modkit!\nTo get started, go to Tools > Descenders Competitive > Verify > Proper Verify\n\nOr go to the installation wiki https://github.com/nohumanman/descenders-modkit/wiki\n\nLicensed under GPL-3.0 License.\n\nFor the developers' and authors' protection, there is no warranty for this free software.  For both users' and authors' sake, the GPL requires that modified versions be marked as changed, so that their problems will not be attributed erroneously to authors of previous versions."; // Replace this with your actual README content

    [MenuItem("Help/Show Readme")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ReadmeWindow));
    }

    private void OnGUI()
    {
        GUILayout.Label("Readme", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(readmeText, MessageType.Info);
    }

    [InitializeOnLoadMethod]
    private static void CheckAndShowReadme()
    {
        if (!PlayerPrefs.HasKey(ReadmeShownKey))
        {
            ShowWindow();
            PlayerPrefs.SetInt(ReadmeShownKey, 1);
            PlayerPrefs.Save();
        }
    }
}
