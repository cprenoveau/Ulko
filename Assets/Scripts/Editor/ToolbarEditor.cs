using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace Ulko
{
	[InitializeOnLoad]
	public class ToolbarBuildButton
	{
		static ToolbarBuildButton()
		{
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
		}

		static void OnToolbarGUI()
		{
			GUILayout.FlexibleSpace();

			bool playFromStartup = GUILayout.Toggle(EditorPrefs.GetBool("ForcePlayFromStartup"),
				new GUIContent("Play from Start", "Starts the game from the startup scene."),
				GUILayout.Height(20));

			EditorPrefs.SetBool("ForcePlayFromStartup", playFromStartup);
		}
	}
}
