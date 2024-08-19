
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
using UnityEditorInternal;


#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

public class SimpleAudioPlayer : UdonSharpBehaviour
{
    public AudioClip[] soundClips;

    [SerializeField] public GameObject SAP_Child;
    [HideInInspector] public GameObject glorpington;
    SimpleAudioPlayer_Child SAP_Child_Script;

    public int targetSound = 0;
    public int m_TabsSerialized = 0;
    public int[] limits = { 1, 2, 3, 4 };

    private void Start()
    {
        Debug.Log(limits[1]);
        SAP_Child_Script = SAP_Child.GetComponent<SimpleAudioPlayer_Child>();
        //SendCustomEvent("SetChildScriptObjects");
        SAP_Child_Script.SetProgramVariable("SAPChild_AudioClips", soundClips);
        SAP_Child_Script.SetProgramVariable("limits", limits);
    }

    public void SetChildScriptObjects()
    {
        Debug.Log("Entered!");
        //int[] limits = { 1, 2, 3, 4 };
        //SAP_Child.GetComponent<SimpleAudioPlayer_Child>().SetProgramVariable("SAPChild_AudioClips", teddyClips);
        //SAP_Child.GetComponent<SimpleAudioPlayer_Child>().SetProgramVariable("limits", limits);
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

    private string[] m_Tabs = { "Single Sound", "Multiple Sounds - Random", "Multiple Sounds - Sequence", "Option Four" };
    private int m_TabsIndex = 0;

    int sliderValue = 0;

    int[] limits;

    /* Approach to create an Array List in Custom Editor from this StackOverflow question:
     * https://stackoverflow.com/questions/47753367/how-to-display-modify-array-in-the-editor-window
     * Also the Unity Docs were useful in understanding how to properly Serialize properties so the data in the Editor can be stored: 
     * https://docs.unity3d.com/ScriptReference/Editor.html
     * ALSO the UdonSharp API docs were used to understanding the differences in implementing this in U# versus Unity's default C#:
     * https://udonsharp.docs.vrchat.com/editor-scripting/
     */

    SerializedProperty intTest;
    SerializedProperty tabIndex_Serialized;
    SerializedProperty targetSound_Serialized;
    SerializedProperty soundClips_Serialized;

    private void OnEnable()
    {
        // Set up all SerializedProperties in here

        /* I'd like to do more reading into the idea of Serialization, but my understanding of it is:
         * It is used to make private variables visible in the editor
         * It is used to make variables more 'permanent', as in when you close the program, the settings chosen in the custom UI remain when it is opened again.
         */

        intTest = serializedObject.FindProperty("selectedValue");
        tabIndex_Serialized = serializedObject.FindProperty("m_TabsSerialized");
        targetSound_Serialized = serializedObject.FindProperty("targetSound");
        soundClips_Serialized = serializedObject.FindProperty("soundClips");
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

        //DrawDefaultInspector();

        EditorGUILayout.BeginVertical();
        tabIndex_Serialized.intValue = GUILayout.Toolbar(tabIndex_Serialized.intValue, m_Tabs);
        serializedObject.ApplyModifiedProperties();
        GUILayout.Space(5);
        EditorGUILayout.EndVertical();

        limits = inspectorBehaviour.limits;

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
                    if (inspectorBehaviour.soundClips.Length == 1)
                    {
                        EditorGUILayout.HelpBox("Only one sound is available. Please add more sounds.", MessageType.Info);
                    }
                    else
                    {
                        Debug.Log("soundClips.Length = " + inspectorBehaviour.soundClips.Length);
                        for (int i = 0; i < inspectorBehaviour.soundClips.Length; i++)
                        {
                            //int inttest = 0;
                            string nameOfField = inspectorBehaviour.soundClips[i].name;
                            Debug.Log("nameOfField = " + nameOfField);
                            limits[i] = EditorGUILayout.IntField(nameOfField, limits[i]);
                        }
                    }

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
