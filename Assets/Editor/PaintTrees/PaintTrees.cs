using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.IO;
using System.Collections.Generic;
using SimpleValidator;
using SimpleValidator.Exceptions;
using TinyJson;
using System.Linq;

public class PaintTrees : EditorWindow
{

    public static string configFilePath = "Assets/Editor/PaintTrees/config.json";
    [SerializeField] Config configuration;
    PrefabCollection activeCollection;
    Label prevListElement;
    Label currListElement;
    public static bool config;
    [MenuItem("Window/UI Toolkit/PaintTrees")]
    public static void ShowExample()
    {
        config = configExists(configFilePath);
        PaintTrees wnd = GetWindow<PaintTrees>();
        wnd.titleContent = new GUIContent("PaintTrees");
        Vector2 windowSize = new Vector2(400, 600);
        wnd.minSize = windowSize;
    }

    public void OnEnable()
    {
        checkConfig();


    }

    void OnInspectorUpdate()
    {
        Repaint();
    }


    private void checkConfig()
    {
        if (!config)
        {
            createConfig();
        }
        configuration = GetConfig(configFilePath);



    }


    public void OnListSelect(MouseDownEvent e, string text)
    {
        Debug.Log("event: " + e.target + " you clicked on " + text + " " + e.clickCount + " times.");

        if (e.clickCount > 1)
            SetActiveCollection(text);
    }

    public void SetActiveCollection(string name)
    {
        IEnumerable<PrefabCollection> collection = configuration.collections.Where(collection => collection.name == name);
        activeCollection = collection.First<PrefabCollection>();
        var activeCollectionLabel = rootVisualElement.Q<Label>("activeCollection");
        activeCollectionLabel.text = "active collection: ";

        var activeCollectionContent = rootVisualElement.Q<Label>("activeCollectionContent");
        activeCollectionContent.text = name;
        activeCollectionContent.ToggleInClassList("activeCollectionLabel");

        var listElement = rootVisualElement.Q<Label>(activeCollection.name);
        if (listElement != null)
        {
            listElement.ToggleInClassList("activeCollectionLabel");
            currListElement = listElement;

            if (prevListElement == null)
            {
                prevListElement = listElement;
            }
            else
            {
                prevListElement.ToggleInClassList("activeCollectionLabel");
                prevListElement = listElement;
            }

        }

    }

    private void CollectionList()
    {

        var listItems = new List<string>(configuration.collections.Count);
        for (int i = 0; i <= configuration.collections.Count - 1; i++)
            listItems.Add(configuration.collections[i].name);

        Func<VisualElement> makeItem = () => new Label();

        Action<VisualElement, int> bindItem = (e, i) => { (e as Label).text = listItems[i]; (e as Label).name = listItems[i]; e.RegisterCallback<MouseDownEvent>((_e) => OnListSelect(_e, listItems[i])); };

        var listView = rootVisualElement.Q<ListView>();
        listView.makeItem = makeItem;
        listView.bindItem = bindItem;
        listView.itemsSource = listItems;
        listView.selectionType = SelectionType.Multiple;




        // listView.ScrollToItem(listView.itemsSource.IndexOf(activeCollection));
        listItems.Sort((a, z) => a.CompareTo(z));
        configuration.collections.Sort((a, z) => a.name.CompareTo(z.name));
        // Debug.Log("The index of this item is : " + listView.itemsSource.IndexOf(activeCollection));




        if (activeCollection != null)
        {
            listView.ScrollToItem(configuration.collections.IndexOf(activeCollection) - 1);
            SetActiveCollection(activeCollection.name);
            //listView.ScrollToItem(configuration.collections.IndexOf(activeCollection));
            // Debug.Log("The index of this item is : " + configuration.collections.IndexOf(activeCollection));



        }

    }
    public void CreateGUI()
    {

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/PaintTrees/USS/PaintTrees.uss");
        rootVisualElement.styleSheets.Add(styleSheet);

        var HeaderUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/PaintTrees/UXML/Header.uxml");
        VisualElement elementsFromHeaderUXML = HeaderUXML.Instantiate();
        rootVisualElement.Add(elementsFromHeaderUXML);

        var CollectionListUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/PaintTrees/UXML/CollectionList.uxml");
        VisualElement elementsFromCollectionListUXML = CollectionListUXML.Instantiate();
        rootVisualElement.Add(elementsFromCollectionListUXML);


        var createCollectionML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/PaintTrees/UXML/CreateCollection.uxml");
        VisualElement elementsFromCreateCollectionML = createCollectionML.Instantiate();

        CollectionList();

        Action<string> validateAndCreateCollection = (name) =>
        {
            createCollection(name);
            configuration = GetConfig(configFilePath);
            SetActiveCollection(name);
            CollectionList();

        };

        rootVisualElement.Add(elementsFromCreateCollectionML);
        var createButton = rootVisualElement.Q<Button>("createButton");
        var collectionNameField = rootVisualElement.Q<TextField>("newCollectionName");
        createButton.RegisterCallback<MouseUpEvent>((evt) => validateAndCreateCollection(collectionNameField.text));

        var footerUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/PaintTrees/UXML/Footer.uxml");
        VisualElement elementsFromFooterUXML = footerUXML.Instantiate();
        rootVisualElement.Add(elementsFromFooterUXML);
    }

    private static bool configExists(string configFilePath)
    {
        //validateFilePath(configFilePath);
        if (File.Exists(configFilePath))
        {
            return true;
        }

        return false;
    }

    private static bool validateTextInput(string name)
    {
        Validator validator = new Validator();

        validator.IsMinLength(name, 1).WithMessage("Name has to be longer than 0 characters.")
        .IsMaxLength(name, 128).WithMessage("Name is too long.")
        .IsNotNull(name).WithMessage("Name is NULL");


        if (!validator.IsValid)
        {
            validator.ThrowValidationException();
        }
        return true;

    }

    private Config GetConfig(string name)
    {
        validateTextInput(configFilePath);
        string fileJson = File.ReadAllText(configFilePath);
        Config config = fileJson.FromJson<Config>();

        return config;

    }

    private void createConfig()
    {
        PrefabCollection collection = new PrefabCollection();
        collection.name = name;
        Config newConfig = new Config();
        newConfig.collections.Add(collection);

        string jsonPaintCollection = newConfig.ToJson();
        if (!File.Exists(configFilePath))
        {

            File.WriteAllText(configFilePath, jsonPaintCollection);

        }
    }
    private void createCollection(string name)
    {
        validateTextInput(name);
        if (!config)
        {
            createConfig();

        }

        PrefabCollection newCollection = new PrefabCollection();
        newCollection.name = name;
        Config currentConfig = GetConfig(configFilePath);
        currentConfig.collections.Add(newCollection);
        File.WriteAllText(configFilePath, currentConfig.ToJson());




    }
}