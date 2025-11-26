using System.IO;
using System.Text;
using UnityEditor;

namespace Littale {
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : Editor {

        const string ENUM_SFX = "SfxID";
        const string ENUM_MUSIC = "MusicID";

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            AudioManager ctx = (AudioManager)target;
            UnityEngine.GUILayout.Space(20);

            if (UnityEngine.GUILayout.Button("Generate Audio Enums")) {
                GenerateEnum(ctx);
            }
        }

        void GenerateEnum(AudioManager ctx) {
            MonoScript self = MonoScript.FromScriptableObject(this);

            string path = AssetDatabase.GetAssetPath(self); // Get path of this script
            string scriptDirectory = Path.GetDirectoryName(path); // Get directory

            string targetDirectory = Path.Combine(scriptDirectory, "../Generated/");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// This is an auto-generated file. Do not modify directly.");
            // Generate SFX enum
            sb.AppendLine("public enum " + ENUM_SFX + " {");
            sb.AppendLine("\tNone,");

            foreach (var sfx in ctx.sfxs) {
                string cleanName = sfx.name.Replace(" ", "_").Replace("-", "_");
                sb.AppendLine("\t" + cleanName + ",");
            }

            sb.AppendLine("}");

            // Generate Music enum
            sb.AppendLine("public enum " + ENUM_MUSIC + " {");
            sb.AppendLine("\tNone,");

            foreach (var music in ctx.musics) {
                string cleanName = music.name.Replace(" ", "_").Replace("-", "_");
                sb.AppendLine("\t" + cleanName + ",");
            }

            sb.AppendLine("}");

            if (!Directory.Exists(targetDirectory)) {
                Directory.CreateDirectory(targetDirectory); // Ensure directory exists
            }

            string finalPath = Path.Combine(targetDirectory, "AudioEnums.cs");
            File.WriteAllText(finalPath, sb.ToString());
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("Generated Audio Enums at: " + finalPath);
        }

    }
}