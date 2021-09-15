using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

using TinyJson;



public class oldPaintTrees : MonoBehaviour
{

    private class Window : EditorWindow
    {
        [MenuItem("TP/Window")]
        static void OpenPaintTreesWindow()
        {
            Window window = (Window)GetWindow(typeof(Window));

            window.Show();

        }

        public void OnEnable()
        {

            Action action = () =>
            {
                rootVisualElement.Query<Button>().ForEach((button) =>
                {
                    button.text = button.text.EndsWith("Button") ? "Button (Clicked)" : "Button";
                });
            };

            // Get a reference to the Button from UXML and assign it its action.
            var uxmlButton = rootVisualElement.Q<Button>("the-uxml-button");
            uxmlButton.RegisterCallback<MouseUpEvent>((evt) => action());

            // Create a new Button with an action and give it a style class.
            var csharpButton = new Button(action) { text = "C# Button" };
            csharpButton.AddToClassList("some-styled-button");
            rootVisualElement.Add(csharpButton);



            var setList = new List<string>(0);
            setList = getPaintSets();
            Func<VisualElement> makeItem = () => new Label();
            Action<VisualElement, int> bindItem = async (e, i) => (e as Label).text = setList[i];

            const int itemHeight = 26;
            var listView = new ListView(setList, itemHeight, makeItem, bindItem);
            listView.selectionType = SelectionType.Multiple;
            listView.onItemsChosen += obj => Debug.Log(obj);
            listView.onSelectionChange += objects => Debug.Log(objects);
            listView.style.flexGrow = 1.0f;
            listView.style.fontSize = 24;
            rootVisualElement.Add(listView);


        }

        void OnGUI()
        {

            GUIStyle style1 = new GUIStyle();
            GUIStyle style2 = new GUIStyle();
            GUI.skin.label.fontSize = 32;


            style1.alignment = TextAnchor.MiddleCenter;
            style2.alignment = TextAnchor.UpperLeft;
            style1.fontSize = 64;

            Rect dropArea = GUILayoutUtility.GetRect(100f, 400f, GUILayout.ExpandWidth(true));

            GUI.Box(dropArea, "Drag Prefabs Here", style1);
        }

        public class testObject
        {
            public string _name { get; set; }
            public List<string> arrayList { get; set; }
        }

        class PaintTreesConfig
        {
            public List<PaintSet> paintSets { get; set; }
        }

        class PaintSet
        {
            string name { get; set; }
            List<TreePrototype> prototypes;
        }
        private static List<string> getPaintSets()
        {

            testObject new1 = new testObject();
            new1._name = "blake";
            new1.arrayList = new List<string>();
            new1.arrayList.Add("one");
            new1.arrayList.Add("two");
            new1.arrayList.Add("three");

            var jsonstring = new1.ToJson();
            testObject objFromJson = jsonstring.FromJson<testObject>();

            Debug.Log("obj._name : " + objFromJson._name);
            objFromJson.arrayList.ForEach(delegate (string _line)
            {
                Debug.Log(_line);
            });

            // Check if config file exists
            if (!File.Exists("Resources/PaintTrees/config.json"))
            {
                // ASK User to create a new List with a Name

                // IF no config file exists generate a new blank config file
                PaintTreesConfig config = new PaintTreesConfig();
                config.paintSets = new List<PaintSet>();
            }


            // IF config file does exists Load the available Tree Sets



            List<string> setsFromFile;
            string fileJson = File.ReadAllText(Application.dataPath +
            "/Resources/PaintTrees/sets.json");

            setsFromFile = fileJson.FromJson<List<string>>();
            for (int i = 0; i < setsFromFile.Count; i++)
            {
                setsFromFile[i] = setsFromFile[i].Trim('"');
                Debug.Log(setsFromFile[i]);
            }


            return setsFromFile;

        }






    }
}



