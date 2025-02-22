using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ulko
{
    public class SpriteImporter : EditorWindow
    {
        [MenuItem("Window/Sprite Importer")]
        static void ShowItemTool()
        {
            var window = GetWindow<SpriteImporter>();
            window.titleContent.text = "Sprite Importer";
            window.minSize = new Vector2(800, 400);
        }

        private string spriteFullpath;
        private string spriteRelativePath;
        private string spriteName;
        private Texture2D texture;
        private float fps = 12;

        private int walkDownStartFrame = 19;
        private int walkLeftStartFrame = 10;
        private int walkRightStartFrame = 28;
        private int walkUpStartFrame = 1;
        private int walkFrameCount = 8;

        private void OnGUI()
        {
            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Select Image", GUILayout.Width(120)))
                    {
                        spriteFullpath = EditorUtility.OpenFilePanel("Select Image", "", "png");
                        if (spriteFullpath.Length != 0)
                        {
                            spriteRelativePath = spriteFullpath.Replace(Application.dataPath, "Assets");
                            texture = (Texture2D)AssetDatabase.LoadAssetAtPath(spriteRelativePath, typeof(Texture2D));
                        }
                    }

                    texture = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), allowSceneObjects: false);
                }

                spriteName = EditorGUILayout.TextField("Sprite Name", spriteName);
                fps = EditorGUILayout.FloatField("FPS", fps);

                walkDownStartFrame = EditorGUILayout.IntField("Walk Down Start Frame", walkDownStartFrame);
                walkLeftStartFrame = EditorGUILayout.IntField("Walk Left Start Frame", walkLeftStartFrame);
                walkRightStartFrame = EditorGUILayout.IntField("Walk Right Start Frame", walkRightStartFrame);
                walkUpStartFrame = EditorGUILayout.IntField("Walk Up Start Frame", walkUpStartFrame);
                walkFrameCount = EditorGUILayout.IntField("Walk Frame Count", walkFrameCount);

                if (texture != null)
                {
                    if (GUILayout.Button("Generate", GUILayout.Width(120)))
                    {
                        spriteRelativePath = AssetDatabase.GetAssetPath(texture);

                        var sprites = AssetDatabase.LoadAllAssetsAtPath(spriteRelativePath).OfType<Sprite>().ToArray();

                        var tokens = spriteRelativePath.Split('/');
                        string outputFolder = "";
                        for (int i = 0; i < tokens.Length - 1; ++i)
                        {
                            outputFolder += tokens[i];
                            outputFolder += "/";
                        }

                        var idleDown = ScriptableObject.CreateInstance<SpriteAnimation>();
                        idleDown.fps = fps;
                        idleDown.frames.Add(sprites[walkDownStartFrame - 1]);

                        AssetDatabase.CreateAsset(idleDown, outputFolder + spriteName + "_IdleDown.asset");

                        var idleLeft = ScriptableObject.CreateInstance<SpriteAnimation>();
                        idleLeft.fps = fps;
                        idleLeft.frames.Add(sprites[walkLeftStartFrame - 1]);

                        AssetDatabase.CreateAsset(idleLeft, outputFolder + spriteName + "_IdleLeft.asset");

                        var idleRight = ScriptableObject.CreateInstance<SpriteAnimation>();
                        idleRight.fps = fps;
                        idleRight.frames.Add(sprites[walkRightStartFrame - 1]);

                        AssetDatabase.CreateAsset(idleRight, outputFolder + spriteName + "_IdleRight.asset");

                        var idleUp = ScriptableObject.CreateInstance<SpriteAnimation>();
                        idleUp.fps = fps;
                        idleUp.frames.Add(sprites[walkUpStartFrame - 1]);

                        AssetDatabase.CreateAsset(idleUp, outputFolder + spriteName + "_IdleUp.asset");

                        var idle = ScriptableObject.CreateInstance<DirectionalSpriteAnimation>();
                        idle.down = idleDown;
                        idle.left = idleLeft;
                        idle.right = idleRight;
                        idle.up = idleUp;

                        AssetDatabase.CreateAsset(idle, outputFolder + spriteName + "_Idle.asset");

                        var walkDown = ScriptableObject.CreateInstance<SpriteAnimation>();
                        walkDown.fps = fps;
                        for (int i = 0; i < walkFrameCount; ++i)
                        {
                            walkDown.frames.Add(sprites[walkDownStartFrame + i]);
                        }

                        AssetDatabase.CreateAsset(walkDown, outputFolder + spriteName + "_WalkDown.asset");

                        var walkLeft = ScriptableObject.CreateInstance<SpriteAnimation>();
                        walkLeft.fps = fps;
                        for (int i = 0; i < walkFrameCount; ++i)
                        {
                            walkLeft.frames.Add(sprites[walkLeftStartFrame + i]);
                        }

                        AssetDatabase.CreateAsset(walkLeft, outputFolder + spriteName + "_WalkLeft.asset");

                        var walkRight = ScriptableObject.CreateInstance<SpriteAnimation>();
                        walkRight.fps = fps;
                        for (int i = 0; i < walkFrameCount; ++i)
                        {
                            walkRight.frames.Add(sprites[walkRightStartFrame + i]);
                        }

                        AssetDatabase.CreateAsset(walkRight, outputFolder + spriteName + "_WalkRight.asset");

                        var walkUp = ScriptableObject.CreateInstance<SpriteAnimation>();
                        walkUp.fps = fps;
                        for (int i = 0; i < walkFrameCount; ++i)
                        {
                            walkUp.frames.Add(sprites[walkUpStartFrame + i]);
                        }

                        AssetDatabase.CreateAsset(walkUp, outputFolder + spriteName + "_WalkUp.asset");

                        var walk = ScriptableObject.CreateInstance<DirectionalSpriteAnimation>();
                        walk.down = walkDown;
                        walk.left = walkLeft;
                        walk.right = walkRight;
                        walk.up = walkUp;

                        AssetDatabase.CreateAsset(walk, outputFolder + spriteName + "_Walk.asset");

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }
        }
    }
}