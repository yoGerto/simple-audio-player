
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using UnityEditor;
using VRC.SDK3.Components;
using System.Diagnostics;


#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
using UnityEditorInternal;
#endif

public class SimpleAudioPlayer : UdonSharpBehaviour
{
    [SerializeField] public GameObject SAP_Child;
    [HideInInspector] public GameObject SAP_Child_Prefab;
    SimpleAudioPlayer_Child SAP_Child_Script;

    public AudioClip[] soundClips;
    public int targetSound, m_Tabs_Serialized, m_TabsChild_Serialized = 0;
    public int[] limits;
    public int[] soundSequence_int;
    public bool isError = false;

    public int objectType;

    private void Start()
    {
        Debug.Log("m_Tabs_Serialized = " + m_Tabs_Serialized);
        Debug.Log("m_TabsChild_Serialized = " + m_TabsChild_Serialized);
        SAP_Child_Script = SAP_Child.GetComponent<SimpleAudioPlayer_Child>();
        SAP_Child_Script.SetProgramVariable("SAPChild_AudioClips", soundClips);

        //SAP_Child_Script.SetProgramVariable("limits", limits);

        SAP_Child_Script.selectedTabs[0] = m_Tabs_Serialized;
        SAP_Child_Script.selectedTabs[1] = m_TabsChild_Serialized;
        SAP_Child_Script.isError = isError;

        // If objectType is set to Pickup
        if (objectType == 0)
        {
            // This line turns off the Interact event
            DisableInteractive = true;
        }
        else
        {
            // Only do this if a VRCPickup component exists
            if (GetComponent<VRCPickup>() != null)
            {
                GetComponent<VRCPickup>().pickupable = false;
            }
        }

        if (m_Tabs_Serialized == 0)
        {
            // This is the single sound setting, meaning we only need to send through which index of the soundClips array needs to be played.
            SAP_Child_Script.selectedSound = targetSound;
        }
        else if (m_Tabs_Serialized == 1)
        {
            if (m_TabsChild_Serialized == 0)
            {
                // We are in the random tab, so pass through the limits array
                // Using SetProgramVariable here because we have a FieldChangeCallback attribute on the limits variable in the other script.
                // So Udon stipulates we need to use this function to set the variable rather than using the classname.variable
                SAP_Child_Script.SetProgramVariable("limits", limits);
            }
            else
            {
                // We are in the Sequence tab, so pass the sequence array through
                SAP_Child_Script.soundSequence_int = soundSequence_int;
            }
        }
        else
        {
            SAP_Child_Script.isError = true;
        }
    }

    public override void OnPickup()
    {
        Debug.Log("OnPickup Entered");
        Networking.SetOwner(Networking.LocalPlayer, SAP_Child);
    }

    public override void OnPickupUseDown()
    {
        SAP_Child_Script.PlaySound_AllSettings();
    }

    public override void Interact()
    {
        Networking.SetOwner(Networking.LocalPlayer, SAP_Child);
        SAP_Child_Script.PlaySound_AllSettings();
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
    private string[] dropdownOptions = { "Pickup", "Interact" };


    /* Approach to create an Array List in Custom Editor from this StackOverflow question:
     * https://stackoverflow.com/questions/47753367/how-to-display-modify-array-in-the-editor-window
     * (The Array List didn't end up being super helpful because UdonSharp does not like Lists)
     * (Also UdonSharp does not allow you to create custom classes at all, which sucks as if it could I would have made more functionality possible and easier editor scripting)
     * 
     * Also the Unity Docs were useful in understanding how to properly Serialize properties so the data in the Editor can be stored: 
     * https://docs.unity3d.com/ScriptReference/Editor.html
     * ALSO the UdonSharp API docs were used to understanding the differences in implementing this in U# versus Unity's default C#:
     * https://udonsharp.docs.vrchat.com/editor-scripting/
     */

    SerializedProperty tabIndex_Serialized;
    SerializedProperty tabIndexChild_Serialized;
    SerializedProperty targetSound_Serialized;
    SerializedProperty soundClips_Serialized;
    SerializedProperty soundSequence_int_Serialized;

    ReorderableList list;

    private void OnEnable()
    {
        // Set up all SerializedProperties in here

        /* I'd like to do more reading into the idea of Serialization, but my understanding of it is:
         * It is used to make private variables visible in the editor [SerializeField]
         * It is used to make variables more 'permanent', as in when you close the program, the settings chosen in the custom UI remain when it is opened again. (SerializeProperty)
         * We also use Serialization when sending network data through RequestSerialization(). Serialization seems to relate to the general concept of writing data out to somewhere.
         */

        tabIndex_Serialized = serializedObject.FindProperty("m_Tabs_Serialized");
        tabIndexChild_Serialized = serializedObject.FindProperty("m_TabsChild_Serialized");
        targetSound_Serialized = serializedObject.FindProperty("targetSound");
        soundClips_Serialized = serializedObject.FindProperty("soundClips");

        soundSequence_int_Serialized = serializedObject.FindProperty("soundSequence_int");

        SimpleAudioPlayer SAP_Object = serializedObject.targetObject as SimpleAudioPlayer;

        #region ReorderableList and drawElementCallback
        list = new ReorderableList(serializedObject, serializedObject.FindProperty("soundSequence_int"), true, false, true, true);
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {

            var element = list.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;

            if (soundSequence_int_Serialized.GetArrayElementAtIndex(index).intValue < 0)
            {
                soundSequence_int_Serialized.GetArrayElementAtIndex(index).intValue = 0;
            }
            else if (soundSequence_int_Serialized.GetArrayElementAtIndex(index).intValue > SAP_Object.soundClips.Length - 1)
            {
                soundSequence_int_Serialized.GetArrayElementAtIndex(index).intValue = SAP_Object.soundClips.Length - 1;
            }

            Rect secondHalfRect = rect;
            secondHalfRect.width /= 2;
            secondHalfRect.x += secondHalfRect.width;
            secondHalfRect.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.ObjectField(secondHalfRect, soundClips_Serialized.GetArrayElementAtIndex(soundSequence_int_Serialized.GetArrayElementAtIndex(index).intValue), GUIContent.none);

            // Making a Rect of width 20 to contain the label
            Rect labelRect = rect;
            labelRect.width = 20f;
            labelRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PrefixLabel(labelRect, new GUIContent("#" + (index + 1).ToString()));

            // Make new Rect for the rest of the content, make it shorter by the length of the labelRect then divide it by 2 for the two Property Fields
            Rect propRect = rect;
            propRect.width = secondHalfRect.width - labelRect.width;
            propRect.x += labelRect.width;
            propRect.width *= 0.25f;
            propRect.height = EditorGUIUtility.singleLineHeight;


            EditorGUI.PropertyField(propRect, soundSequence_int_Serialized.GetArrayElementAtIndex(index), GUIContent.none);


            propRect.x += propRect.width;
            //propRect.width *= 1.5f;

            if (GUI.Button(propRect, "+"))
            {
                soundSequence_int_Serialized.GetArrayElementAtIndex(index).intValue++;
            }

            propRect.x += propRect.width;

            if (GUI.Button(propRect, "-"))
            {
                soundSequence_int_Serialized.GetArrayElementAtIndex(index).intValue--;
            }

        };
        #endregion
    }


    public override void OnInspectorGUI()
    {
        // I will not be drawing the default inspector here, I would like to pick and choose which UI elements from the original list I display
        // Draws the default convert to UdonBehaviour button, program asset field, sync settings, etc.
        //if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

        serializedObject.Update();
        SimpleAudioPlayer inspectorBehaviour = (SimpleAudioPlayer)target;

        // Reset this to false at the beginning of the script and use it to track if any setup errors are present
        // i.e. if someone tries to use Multiple Sounds with only a single sound clip being set
        inspectorBehaviour.isError = false;

        #region Object Type Dropdown
        Rect configRect = EditorGUILayout.BeginHorizontal();
        float labelWidth = EditorStyles.label.CalcSize(new GUIContent("Object Type")).x;
        // Making the dropdown box (popup) "Responsive"
        GUILayout.Label("Object Type");
        if (Screen.width > 190)
        {
            configRect.width /= 2;
            configRect.x += configRect.width;
        }
        else
        {
            configRect.x += labelWidth + 5;
            configRect.width = Screen.width - (labelWidth + 5);
        }
        inspectorBehaviour.objectType = EditorGUI.Popup(configRect, inspectorBehaviour.objectType, dropdownOptions);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);
        #endregion

        #region Interact Options on Dropdown
        if (inspectorBehaviour.objectType == 1)
        {
            inspectorBehaviour.GetComponent<VRCInteractable>().proximity = EditorGUILayout.Slider("Proximity", inspectorBehaviour.GetComponent<VRCInteractable>().proximity, 0, 100);
            inspectorBehaviour.GetComponent<VRCInteractable>().interactText = EditorGUILayout.TextField("Interaction Text", inspectorBehaviour.GetComponent<VRCInteractable>().interactText);
        }
        GUILayout.Space(5);
        #endregion

        EditorGUILayout.PropertyField(soundClips_Serialized, true);

        bool duplicateElement = false;

        for (int i = 0; i < inspectorBehaviour.soundClips.Length; i++)
        {
            for (int j = i + 1; j < inspectorBehaviour.soundClips.Length; j++)
            {
                if (inspectorBehaviour.soundClips[i].name == inspectorBehaviour.soundClips[j].name)
                {
                    duplicateElement = true;
                }
            }
        }

        serializedObject.ApplyModifiedProperties();

        if (duplicateElement)
        {
            EditorGUILayout.HelpBox("A duplicate Sound Clip is present! Please resolve before using the script.", MessageType.Error);
            inspectorBehaviour.isError = true;
            return;
        }

        if (inspectorBehaviour.soundClips.Length == 0)
        {
            EditorGUILayout.HelpBox("Drag and drop your audio clips in to begin using the script", MessageType.Info);
            inspectorBehaviour.isError = true;
            // returning here makes a nice effect where the remainder of the UI is not drawn unless an Audio file has been inserted into the script!
            return;
        }

        #region Handling Limits Array
        int maxLimitsValue = 0;

        for (int i = 0; i < inspectorBehaviour.limits.Length; i++)
        {
            maxLimitsValue += inspectorBehaviour.limits[i];
        }

        if (inspectorBehaviour.limits.Length != inspectorBehaviour.soundClips.Length)
        {
            int[] newArray = new int[inspectorBehaviour.soundClips.Length];
            if (inspectorBehaviour.limits.Length < inspectorBehaviour.soundClips.Length)
            {
                // First set all values of array to 1, saves us having to calculate how many empty slots need to be filled.
                for (int i = 0; i < newArray.Length; i++)
                {
                    newArray[i] = 1;
                }

                // then copy and overwrite values from the original array.
                for (int i = 0; i < inspectorBehaviour.limits.Length; i++)
                {
                    newArray[i] = inspectorBehaviour.limits[i];
                }
            }
            else if (inspectorBehaviour.limits.Length > inspectorBehaviour.soundClips.Length)
            {
                for (int i = 0; i < newArray.Length; i++)
                {
                    newArray[i] = inspectorBehaviour.limits[i];
                }
            }
            inspectorBehaviour.limits = newArray;
        }
        #endregion

        #region Primary and Secondary Toolbars
        EditorGUILayout.BeginVertical();
        tabIndex_Serialized.intValue = GUILayout.Toolbar(tabIndex_Serialized.intValue, m_Tabs);
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndVertical();
        GUILayout.Space(5);

        if (tabIndex_Serialized.intValue >= 0)
        {
            switch (m_Tabs[tabIndex_Serialized.intValue])
            {
                case "Single Sound":
                    if (targetSound_Serialized.intValue > inspectorBehaviour.soundClips.Length - 1)
                    {
                        targetSound_Serialized.intValue = inspectorBehaviour.soundClips.Length - 1;
                    }
                    EditorGUILayout.IntSlider(targetSound_Serialized, 0, inspectorBehaviour.soundClips.Length - 1);
                    EditorGUILayout.ObjectField(inspectorBehaviour.soundClips[targetSound_Serialized.intValue], typeof(AudioClip), false);
                    serializedObject.ApplyModifiedProperties();
                    break;

                case "Multiple Sounds":
                    if (inspectorBehaviour.soundClips.Length == 1)
                    {
                        EditorGUILayout.HelpBox("Please use more than one Audio Clip to use this setting.", MessageType.Error);
                        inspectorBehaviour.isError = true;
                        break;
                    }
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
                                Rect testRect = EditorGUILayout.BeginHorizontal();

                                //300 is a good value for responsiveness

                                // ...For some reason you need to write GUI content to the rectangle for it to be in the correct position
                                // But it needs to be done specifically with GUILayout, not sure why.
                                // This is a bit of a hacky approach but gets me what I'm looking for.
                                GUILayout.Label("");
                                testRect.width /= 2;
                                EditorGUI.LabelField(testRect, inspectorBehaviour.soundClips[i].name);
                                testRect.x += testRect.width;

                                Rect secondHalf = testRect;


                                secondHalf.width *= 0.3f;

                                inspectorBehaviour.limits[i] = EditorGUI.IntField(secondHalf, inspectorBehaviour.limits[i]);
                                if (inspectorBehaviour.limits[i] < 0)
                                {
                                    inspectorBehaviour.limits[i] = 0;
                                }


                                if (Screen.width < 300)
                                {
                                    secondHalf.width = 39f;
                                }

                                secondHalf.x += testRect.width * 0.5f;
                                float calc = ((float)inspectorBehaviour.limits[i] / (float)maxLimitsValue) * 100;
                                string stringFieldText = calc.ToString("0.00");
                                EditorGUI.TextField(secondHalf, stringFieldText);
                                secondHalf.x -= 15f;
                                EditorGUI.LabelField(secondHalf, new GUIContent("%"));
                                EditorGUILayout.EndHorizontal();
                            }
                            if (maxLimitsValue == 0)
                            {
                                EditorGUILayout.HelpBox("Please select random chance values for a sound to play.", MessageType.Error);
                                inspectorBehaviour.isError = true;
                            }
                            break;

                        case "Sequence":
                            serializedObject.Update();
                            list.DoLayoutList();
                            if (list.count == 0)
                            {
                                EditorGUILayout.HelpBox("Please add entries to the sequence.", MessageType.Error);
                                inspectorBehaviour.isError = true;
                            }
                            serializedObject.ApplyModifiedProperties();
                            break;
                    }
                    break;
            }
        }
        serializedObject.ApplyModifiedProperties();
        #endregion


        EditorGUI.BeginChangeCheck();

        GameObject SAP_Child_Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimpleAudioPlayer/SAP Child.prefab");
        inspectorBehaviour.SAP_Child_Prefab = SAP_Child_Prefab;

        gameObjectTest = inspectorBehaviour.gameObject;

        if (inspectorBehaviour.GetComponentInChildren<SimpleAudioPlayer_Child>() == null)
        {
            inspectorBehaviour.isError = true;
            EditorGUILayout.HelpBox("No Valid SAP Child was found! Use the button below generate one.", MessageType.Error);

            if (GUILayout.Button("Generate SAP Child"))
            {
                if (instantiatedGameObject == null)
                {
                    instantiatedGameObject = (GameObject)PrefabUtility.InstantiatePrefab(SAP_Child_Prefab, inspectorBehaviour.transform);
                    inspectorBehaviour.SAP_Child = instantiatedGameObject;
                }
            }
        }

        if (inspectorBehaviour.SAP_Child.GetComponent<AudioSource>() == null)
        {
            inspectorBehaviour.isError = true;
            EditorGUILayout.HelpBox("SAP Child is missing an Audio Source! Re-add the component, or delete and regenerate.", MessageType.Error);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(inspectorBehaviour, "Modify string val");

            inspectorBehaviour.targetSound = targetSound_Serialized.intValue;

        }
    }
    
}
#endif
