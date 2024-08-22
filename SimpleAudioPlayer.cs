
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using VRC.Udon.Common;
using System.Threading;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;



#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

/* Creating a Custom Property Drawer from this website:
 * https://catlikecoding.com/unity/tutorials/editor/custom-data/
 * https://catlikecoding.com/unity/tutorials/editor/custom-list/
 */

[Serializable]
public class ColorPoint
{
    public Color color;
    public Vector3 position;
}

[Serializable]
public class AudioSequence
{
    public int clipIndex;
    public AudioClip clip;
}

[CustomPropertyDrawer(typeof(ColorPoint))]
public class ColorPointDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //Returned value seems to be 2 higher than what is listed here, could this be from the margin?
        return Screen.width < 333 ? 38f : 18f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Debug.Log(position.height);
        label = EditorGUI.BeginProperty(position, label, property);
        Rect contentPosition = EditorGUI.PrefixLabel(position, label);

        if (position.height > 22f)
        {
            position.height = 20f;
            //EditorGUI.indentLevel += 1;
            contentPosition = EditorGUI.IndentedRect(position);

            // ...I'm not entirely sure how to check if the GUI element you're drawing is that of the elements in an array
            // But for some reason the name of the property of the elements in an array is "data" so checking for that seems to work?
            // I just want to make the content of the array stretch to fit the entire width of the array bar
            if (property.name == "data")
            {
                contentPosition.x = 24f;
                contentPosition.width += 20f;
                //contentPosition.width *= 1.1f;
            }
            contentPosition.y += 18f;
        }

        Debug.Log(position.height);
        contentPosition.width *= 0.75f;
        EditorGUI.indentLevel = 0;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("position"), GUIContent.none);
        contentPosition.x += contentPosition.width * 1.015f;
        contentPosition.width /= 3f;
        EditorGUIUtility.labelWidth = 14f;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("color"), new GUIContent("C"));
        EditorGUI.EndProperty();

    }
}

[CustomPropertyDrawer(typeof(AudioSequence))]
public class AudioSequenceDrawer : PropertyDrawer
{
    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //{
    //    //Returned value seems to be 2 higher than what is listed here, could this be from the margin?
    //    return Screen.width < 333 ? 38f : 18f;
    //}

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //var test = new Label("Test");
        var test = new GUIContent("Test");
        label = EditorGUI.BeginProperty(position, test, property);
        EditorGUIUtility.labelWidth = 30f;
        Rect contentPosition = EditorGUI.PrefixLabel(position, GUIContent.none);


        var widthBeforeAdjustment = contentPosition.width;

        Debug.Log("contentPosition.x before = " + contentPosition.x);
        // In the following line, I want the element to end at the same point as the toolbar line above it, but using Screen.width / 2 is slightly off from it
        // I suspect this is because the foldout arrow takes some screen space, if I was using css styling I could create some divs to contain the content then use that total space instead
        // Perhaps this is doable here because I know it supports styling. Further investigation is needed
        // However, for now adding a 2 to it seems to get it as close to center as possible. Not an ideal fix but it will do!
        //contentPosition.x = (Screen.width / 2) + 2;
        Debug.Log("contentPosition.x after = " + contentPosition.x);

        var desiredWidth = ((Screen.width / 2) + 2) - contentPosition.x;
        contentPosition.width = desiredWidth;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("clip"), GUIContent.none);
        contentPosition.width = widthBeforeAdjustment - desiredWidth;
        contentPosition.x = (Screen.width / 2) + 2;

        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("clipIndex"), GUIContent.none);

        EditorGUI.EndProperty();
    }
}

public class SimpleAudioPlayer : UdonSharpBehaviour
{
    public AudioClip[] soundClips;

    [SerializeField] public GameObject SAP_Child;
    [HideInInspector] public GameObject glorpington;
    SimpleAudioPlayer_Child SAP_Child_Script;

    public int targetSound = 0;
    public int m_Tabs_Serialized = 0;
    public int m_TabsChild_Serialized = 0;
    public int[] limits = { 1, 2, 3, 4 };
    public int[] soundSequence = { 1, 2, 3, 4 };

    public AudioSequence audioSequence;
    public AudioSequence[] audioSequence_Array;
    public ColorPoint point;
    public ColorPoint[] points;
    public Vector3 vector;
    public Vector3[] vectors;

    private void Start()
    {
        Debug.Log(limits[1]);
        SAP_Child_Script = SAP_Child.GetComponent<SimpleAudioPlayer_Child>();
        SAP_Child_Script.SetProgramVariable("SAPChild_AudioClips", soundClips);
        SAP_Child_Script.SetProgramVariable("limits", limits);
    }

    public override void OnPickup()
    {
        Networking.SetOwner(Networking.LocalPlayer, SAP_Child);
    }

    public override void OnPickupUseDown()
    {
        SAP_Child.GetComponent<SimpleAudioPlayer_Child>().SendCustomEvent("GenerateRNGVal");
    }

}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
[CustomEditor(typeof(SimpleAudioPlayer))]
public class CustomInspectorEditor : Editor
{

    GameObject gameObjectTest;
    GameObject instantiatedGameObject;

    /* Learning how to create Tabs in the editor window from this YouTube tutorial from The Messy Coder:
     * https://www.youtube.com/watch?v=-sJRvRirJ9Q
     */

    private string[] m_Tabs = { "Single Sound", "Multiple Sounds"};
    private string[] m_Tabs_Child = { "Random", "Sequence"};

    int[] limits;
    int[] soundSequence;

    /* Approach to create an Array List in Custom Editor from this StackOverflow question:
     * https://stackoverflow.com/questions/47753367/how-to-display-modify-array-in-the-editor-window
     * Also the Unity Docs were useful in understanding how to properly Serialize properties so the data in the Editor can be stored: 
     * https://docs.unity3d.com/ScriptReference/Editor.html
     * ALSO the UdonSharp API docs were used to understanding the differences in implementing this in U# versus Unity's default C#:
     * https://udonsharp.docs.vrchat.com/editor-scripting/
     */

    SerializedProperty tabIndex_Serialized;
    SerializedProperty tabIndexChild_Serialized;
    SerializedProperty targetSound_Serialized;
    SerializedProperty soundClips_Serialized;
    SerializedProperty soundSequence_Serialized;

    SerializedProperty ColorPoint_Serialized;
    SerializedProperty audioSequence_Serialized;

    private void OnEnable()
    {
        // Set up all SerializedProperties in here

        /* I'd like to do more reading into the idea of Serialization, but my understanding of it is:
         * It is used to make private variables visible in the editor
         * It is used to make variables more 'permanent', as in when you close the program, the settings chosen in the custom UI remain when it is opened again.
         */

        tabIndex_Serialized = serializedObject.FindProperty("m_Tabs_Serialized");
        tabIndexChild_Serialized = serializedObject.FindProperty("m_TabsChild_Serialized");
        targetSound_Serialized = serializedObject.FindProperty("targetSound");
        soundClips_Serialized = serializedObject.FindProperty("soundClips");
        soundSequence_Serialized = serializedObject.FindProperty("soundSequence");
        ColorPoint_Serialized = serializedObject.FindProperty("points");
        audioSequence_Serialized = serializedObject.FindProperty("audioSequence");
    }

    public override void OnInspectorGUI()
    {
        // I will not be drawing the default inspector here, I would like to pick and choose which UI elements from the original list I display

        // Unity's documentation states that this must be called every time OnInspectorGUI is called, not entirely sure what it does?
        serializedObject.Update();

        // This line creates a reference to the original script which we are designing the Custom UI for.
        // It allows us to access variables and properties of the original class
        SimpleAudioPlayer inspectorBehaviour = (SimpleAudioPlayer)target;

        // This line is nice because it allows us to display that element in the default manner in the Custom UI
        // Here, the soundClips variable can be displayed in the default manner, which supports dragging and dropping files into the header to add them to the list. Great!
        EditorGUILayout.PropertyField(soundClips_Serialized, true);
        serializedObject.ApplyModifiedProperties();

        if (inspectorBehaviour.soundClips.Length == 0)
        {
            EditorGUILayout.HelpBox("Drag and drop your audio clips in to begin using the script", MessageType.Info);

            // returning here makes a nice effect where the remainder of the UI is not drawn unless an Audio file has been inserted into the script!
            return;
        }

        //EditorGUILayout.PropertyField(ColorPoint_Serialized);
        //DrawDefaultInspector();

        EditorGUILayout.BeginVertical();
        tabIndex_Serialized.intValue = GUILayout.Toolbar(tabIndex_Serialized.intValue, m_Tabs);
        serializedObject.ApplyModifiedProperties();
        GUILayout.Space(5);
        EditorGUILayout.EndVertical();

        limits = inspectorBehaviour.limits;
        soundSequence = inspectorBehaviour.soundSequence;
        Debug.Log(soundSequence.Length);
        Debug.Log(limits.Length);

        if (tabIndex_Serialized.intValue >= 0)
        {
            switch (m_Tabs[tabIndex_Serialized.intValue])
            {
                case "Single Sound":
                    if (inspectorBehaviour.soundClips.Length > 1)
                    {
                        EditorGUILayout.IntSlider(targetSound_Serialized, 0, inspectorBehaviour.soundClips.Length - 1);
                        EditorGUILayout.ObjectField(inspectorBehaviour.soundClips[targetSound_Serialized.intValue], typeof(AudioClip));
                    }
                    else
                    {
                        EditorGUILayout.ObjectField(inspectorBehaviour.soundClips[0], typeof(AudioClip));
                    }
                    serializedObject.ApplyModifiedProperties();
                    break;
                case "Multiple Sounds":
                    EditorGUILayout.BeginVertical();
                    tabIndexChild_Serialized.intValue = GUILayout.Toolbar(tabIndexChild_Serialized.intValue, m_Tabs_Child);
                    serializedObject.ApplyModifiedProperties();
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();

                    switch (m_Tabs_Child[tabIndexChild_Serialized.intValue])
                    {
                        case "Random":
                            for (int i = 0; i < inspectorBehaviour.soundClips.Length; i++)
                            {
                                limits[i] = EditorGUILayout.IntField(inspectorBehaviour.soundClips[i].name, limits[i]);
                                if (limits[i] < 1)
                                {
                                    limits[i] = 1;
                                }
                            }
                            break;
                        case "Sequence":
                            //EditorGUILayout.PropertyField(ColorPoint_Serialized);
                            inspectorBehaviour.audioSequence.clip = inspectorBehaviour.soundClips[inspectorBehaviour.audioSequence.clipIndex];
                            EditorGUILayout.PropertyField(audioSequence_Serialized);
                            serializedObject.ApplyModifiedProperties();
                            break;
                    }


                    //if (tabIndexChild_Serialized.intValue == 0)
                    //{
                    //    EditorGUILayout.HelpBox("1", MessageType.Error);
                    //}
                    //else
                    //{
                    //    EditorGUILayout.HelpBox("2", MessageType.Warning);
                    //}
                    break;
            }
        }


        EditorGUI.BeginChangeCheck();

        GameObject glorp = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameObject.prefab");
        inspectorBehaviour.glorpington = glorp;

        gameObjectTest = inspectorBehaviour.gameObject;

        if (gameObjectTest.GetComponent<AudioSource>() == null)
        {
            EditorGUILayout.HelpBox("Object is missing an Audio Source!", MessageType.Error);
        }

        //Debug.Log(inspectorBehaviour.GetComponentInChildren<SimpleAudioPlayer_Child>());

        if (inspectorBehaviour.GetComponentInChildren<SimpleAudioPlayer_Child>() == null)
        {
            EditorGUILayout.HelpBox("No Valid SAP Child was found! Use the button below generate one.", MessageType.Error);
            //Debug.LogError("No Valid SAP Children were found! Use the button inside of the script to generate one.");

            if (GUILayout.Button("Test"))
            {
                if (instantiatedGameObject == null)
                {
                    instantiatedGameObject = (GameObject)PrefabUtility.InstantiatePrefab(glorp, inspectorBehaviour.transform);
                    inspectorBehaviour.SAP_Child = instantiatedGameObject;
                }
                else
                {
                    Debug.LogWarning("A valid SAP Child has been detected. Please delete the existing SAP Child if you wish to regenerate");
                }
            }

        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(inspectorBehaviour, "Modify string val");

            inspectorBehaviour.targetSound = targetSound_Serialized.intValue;
            inspectorBehaviour.limits = limits;

        }
    }
}
#endif
