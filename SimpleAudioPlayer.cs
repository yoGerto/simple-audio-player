
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
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEditorInternal;
using VRC.Core;







#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif


/* Creating a Custom Property Drawer from this website:
 * https://catlikecoding.com/unity/tutorials/editor/custom-data/
 * https://catlikecoding.com/unity/tutorials/editor/custom-list/
 */


[Serializable]
public class AudioSequence
{
    public int clipIndex;
    public AudioClip clip;
}

[CustomPropertyDrawer(typeof(AudioSequence))]
public class AudioSequenceDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //Returned value seems to be 2 higher than what is listed here, could this be from the margin?
        return 20f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //...really need to do some more reading on Serialization, but I got the info on getting the reference of the SimpleAudioPlayer object from this forum post:
        // https://discussions.unity.com/t/custompropertydrawer-and-monobehaviours/650121

        SimpleAudioPlayer SAP_Object = property.serializedObject.targetObject as SimpleAudioPlayer;

        // Setting the height of the Rect that the fields are drawn into slightly smaller than the width of the property
        position.height = 20f;

        label = EditorGUI.BeginProperty(position, label, property);
        EditorGUILayout.BeginHorizontal();

        // Trying to get the AudioClip Field to end at the same halfway point as the toolbar above it
        // Seems a little fiddly as sometimes it wants either a random +2 or -2 to get it in line
        var desiredWidth = ((Screen.width / 2) - 3) - position.x;

        // Using a new Rect variable to store the original dimensions of the position Rect for easier use
        // Also allows us to use the position rect without Mutating it
        Rect oneHalf = position;
        oneHalf.width /= 2;

        Rect otherHalf = position;
        otherHalf.x += oneHalf.width;
        otherHalf.width = oneHalf.width;

        //Reduce size of otherHalf square by 25% and center it
        otherHalf.x += (otherHalf.width * 0.5f) * 0.5f;
        otherHalf.width *= 0.5f;

        //Debug.Log("SAP_Object.soundClips.Length = " + SAP_Object.soundClips.Length);
        //Debug.Log("property.FindPropertyRelative(clipIndex).intValue = " + property.FindPropertyRelative("clipIndex").intValue);

        // The array indexing being out of range is handled elsewhere, but I will leave this here as a just in case
        // If the array is outside the index range, it will display a LabelField instead of a PropertyField
        if ((property.FindPropertyRelative("clipIndex").intValue > SAP_Object.soundClips.Length - 1) || property.FindPropertyRelative("clipIndex").intValue < 0)
        {
            EditorGUI.LabelField(oneHalf, "Invalid Audio Clip!");
        }
        else
        {
            EditorGUI.PropertyField(oneHalf, property.FindPropertyRelative("clip"), GUIContent.none);
        }

        // How do I properly express 1/3 as a float??
        //otherHalf.width *= 0.33f;
        otherHalf.width *= (1f / 3f);

        EditorGUI.PropertyField(otherHalf, property.FindPropertyRelative("clipIndex"), GUIContent.none);

        otherHalf.x += otherHalf.width;
        if (GUI.Button(otherHalf, "+"))
        {
            property.FindPropertyRelative("clipIndex").intValue += 1;
        }
        otherHalf.x += otherHalf.width;
        if (GUI.Button(otherHalf, "-"))
        {
            property.FindPropertyRelative("clipIndex").intValue -= 1;
        }

        EditorGUILayout.EndHorizontal();
        EditorGUI.EndProperty();

    }
}


public class SimpleAudioPlayer : UdonSharpBehaviour
{

    public AudioClip[] soundClips;

    //public testThingy[] testThingies;

    [SerializeField] public GameObject SAP_Child;
    [HideInInspector] public GameObject glorpington;
    SimpleAudioPlayer_Child SAP_Child_Script;

    public int targetSound = 0;
    public int m_Tabs_Serialized = 0;
    public int m_TabsChild_Serialized = 0;

    public bool isError = false;
    //public int[] limits = {};

    public int[] soundSequence = { 1, 2, 3, 4 };

    public int[] soundSequence_int;

    private void Start()
    {
        Debug.Log(isError);
        SAP_Child_Script = SAP_Child.GetComponent<SimpleAudioPlayer_Child>();
        SAP_Child_Script.SetProgramVariable("SAPChild_AudioClips", soundClips);
        //SAP_Child_Script.SetProgramVariable("limits", limits);
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

    /* Approach to create an Array List in Custom Editor from this StackOverflow question:
     * https://stackoverflow.com/questions/47753367/how-to-display-modify-array-in-the-editor-window
     * Also the Unity Docs were useful in understanding how to properly Serialize properties so the data in the Editor can be stored: 
     * https://docs.unity3d.com/ScriptReference/Editor.html
     * ALSO the UdonSharp API docs were used to understanding the differences in implementing this in U# versus Unity's default C#:
     * https://udonsharp.docs.vrchat.com/editor-scripting/
     */

    //List<AudioClipTracking> listTracking = new List<AudioClipTracking>();



    SerializedProperty tabIndex_Serialized;
    SerializedProperty tabIndexChild_Serialized;
    SerializedProperty targetSound_Serialized;
    SerializedProperty soundClips_Serialized;
    SerializedProperty audioSequence_Serialized;
    SerializedProperty audioRandom_Serialized;
    SerializedProperty listTracking_Serialized;

    SerializedProperty testThingies_Serialized;
    SerializedProperty serializedFromEditor_Serialized;

    SerializedProperty soundSequence_int_Serialized;

    ReorderableList list;

    private void OnEnable()
    {
        // Set up all SerializedProperties in here

        /* I'd like to do more reading into the idea of Serialization, but my understanding of it is:
         * It is used to make private variables visible in the editor [SerializeField]
         * It is used to make variables more 'permanent', as in when you close the program, the settings chosen in the custom UI remain when it is opened again.
         */

        tabIndex_Serialized = serializedObject.FindProperty("m_Tabs_Serialized");
        tabIndexChild_Serialized = serializedObject.FindProperty("m_TabsChild_Serialized");
        targetSound_Serialized = serializedObject.FindProperty("targetSound");
        soundClips_Serialized = serializedObject.FindProperty("soundClips");
        audioSequence_Serialized = serializedObject.FindProperty("audioSequence_Array");
        audioRandom_Serialized = serializedObject.FindProperty("audioRandom");
        //listTracking_Serialized = serializedObject.FindProperty("listTracking");

        serializedFromEditor_Serialized = serializedObject.FindProperty("serializedFromEditor");
        testThingies_Serialized = serializedObject.FindProperty("testThingies");

        soundSequence_int_Serialized = serializedObject.FindProperty("soundSequence_int");

        SimpleAudioPlayer SAP_Object = serializedObject.targetObject as SimpleAudioPlayer;


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

            //EditorGUI.PropertyField(new Rect(rect.x, rect.y, 100f, EditorGUIUtility.singleLineHeight), soundSequence_int_Serialized.GetArrayElementAtIndex(index), GUIContent.none);

            Rect secondHalfRect = rect;
            secondHalfRect.width /= 2;
            secondHalfRect.x += secondHalfRect.width;
            secondHalfRect.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(secondHalfRect, soundClips_Serialized.GetArrayElementAtIndex(soundSequence_int_Serialized.GetArrayElementAtIndex(index).intValue), GUIContent.none);

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

        //inspectorBehaviour.testThingies = new testThingy[1];
        //inspectorBehaviour.testThingies[0].clip = inspectorBehaviour.soundClips[0];
        //inspectorBehaviour.testThingies[0].clipIndex = 0;

        //if (soundSequence_int_Serialized.arraySize > 1)
        //{
        //    Debug.Log(soundSequence_int_Serialized.GetArrayElementAtIndex(0).intValue);
        //}

        Debug.Log(soundSequence_int_Serialized.arraySize);

        //for (int i = 0; i < inspectorBehaviour.soundSequence.Length; i++)
        //{
        //    if (inspectorBehaviour.soundSequence[i] < 1)
        //    {
        //        inspectorBehaviour.soundSequence[i] = 1;
        //    }
        //}

        //for (int i = 0; i < inspectorBehaviour.testThingies.Length; i++)
        //{
        //    inspectorBehaviour.testThingies[i].clipIndex = i;
        ////}
        //if (inspectorBehaviour.testThingies.Length != inspectorBehaviour.soundClips.Length)
        //{
        //    inspectorBehaviour.testThingies = new testThingy[inspectorBehaviour.soundClips.Length];
        //    //for (int i = 0; i < inspectorBehaviour.testThingies.Length; i++)
        //    //{
        //    //    inspectorBehaviour.testThingies[i].clipIndex = i;
        //    //}
        //}
        //for (int i = 0; i < inspectorBehaviour.testThingies.Length; i++)
        //{
        //    inspectorBehaviour.testThingies[i].clip = inspectorBehaviour.soundClips[i];
        //    inspectorBehaviour.testThingies[i].clipIndex = i;
        //}

        EditorGUILayout.PropertyField(soundClips_Serialized, true);


        bool duplicateElement = false;

        for (int i = 0; i < inspectorBehaviour.soundClips.Length; i++)
        {
            for (int j = i + 1; j < inspectorBehaviour.soundClips.Length; j++)
            {
                if (inspectorBehaviour.soundClips[i].name == inspectorBehaviour.soundClips[j].name)
                {
                    duplicateElement = true;
                    ////Delete element j from array, which you can't do lol
                    ////Make a new array?
                    //AudioClip[] tempAudioArray = new AudioClip[inspectorBehaviour.soundClips.Length - 1];
                    //int numOfTimesAdded = 0;
                    //for (int k = 0; k < inspectorBehaviour.soundClips.Length; k++)
                    //{
                    //    if (k != j)
                    //    {
                    //        tempAudioArray[numOfTimesAdded] = inspectorBehaviour.soundClips[k];
                    //        Debug.Log("numOfTimesAdded = " + numOfTimesAdded);
                    //        Debug.Log("tempAudioArray[numOfTimesAdded] = " + tempAudioArray[numOfTimesAdded]);
                    //        numOfTimesAdded++;

                    //        //Debug.Log(k);
                    //    }
                    //}
                    //inspectorBehaviour.soundClips = tempAudioArray;
                }
            }
        }

        serializedObject.ApplyModifiedProperties();

        if (duplicateElement)
        {
            EditorGUILayout.HelpBox("A duplicate Element is present! Please resolve before using the script.", MessageType.Error);
            inspectorBehaviour.isError = true;
            return;
        }
        else
        {
            inspectorBehaviour.isError = false;
        }

        if (inspectorBehaviour.soundClips.Length == 0)
        {
            EditorGUILayout.HelpBox("Drag and drop your audio clips in to begin using the script", MessageType.Info);
            inspectorBehaviour.isError = true;
            // returning here makes a nice effect where the remainder of the UI is not drawn unless an Audio file has been inserted into the script!
            return;
        }
        else
        {
            inspectorBehaviour.isError = false;
        }

        EditorGUILayout.BeginVertical();
        tabIndex_Serialized.intValue = GUILayout.Toolbar(tabIndex_Serialized.intValue, m_Tabs);
        serializedObject.ApplyModifiedProperties();
        GUILayout.Space(5);
        EditorGUILayout.EndVertical();

        //inspectorBehaviour.learningLists.Clear();
        //Debug.Log(inspectorBehaviour.learningLists)
        //Debug.Log(inspectorBehaviour.learningLists.Count);
        //if (inspectorBehaviour.learningLists.Count < inspectorBehaviour.soundClips.Length)
        //{
        //    inspectorBehaviour.learningLists.Add(1);
        //}
        //else if (inspectorBehaviour.learningLists.Count > inspectorBehaviour.soundClips.Length)
        //{
        //    inspectorBehaviour.learningLists.Remove(1);
        //}

        //if (listTracking.Count != inspectorBehaviour.soundClips.Length)
        //{
        //    // We would have two situations here
        //    // One where it's less than, one where it's more than
        //    // If it's more than, simply remake the list
        //    listTracking.Clear();
        //    for (int i = 0; i < inspectorBehaviour.soundClips.Length; i++)
        //    {
        //        listTracking.Add(new AudioClipTracking
        //        {
        //            track = inspectorBehaviour.soundClips[i],
        //            clipIndex = i,
        //            ratio = 1
        //        });
        //    }
        //}


        //listTracking.Add(new AudioClipTracking
        //{
        //    track = inspectorBehaviour.soundClips[0],
        //    clipIndex = 0,
        //    ratio = inspectorBehaviour.learningLists[0]
        //});

        //Debug.Log(listTracking[0].ratio);
        //Debug.Log(inspectorBehaviour.listTracking.Count);
        //int maxLimitsValue = 0;

        //for (int i = 0; i < limits.Length; i++)
        //{
        //    maxLimitsValue += limits[i];
        //}

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
                                //EditorGUILayout.PropertyField(audioRandom_Serialized);

                                Rect testRect = EditorGUILayout.BeginHorizontal();

                                // ...For some reason you need to write GUI content to the rectangle for it to be in the correct position
                                // But it needs to be done specifically with GUILayout, IDK why TBH.
                                // This is a bit of a hacky approach but gets me what I'm looking for
                                GUILayout.Label("");
                                EditorGUI.LabelField(testRect, inspectorBehaviour.soundClips[i].name);
                                testRect.width /= 2;
                                testRect.x += testRect.width;
                                testRect.width /= 2;
                                //limits[i] = EditorGUI.IntField(testRect, limits[i]);
                                //EditorGUI.IntField(testRect, limits[i]);
                                //Debug.Log(inspectorBehaviour.learningLists[i]);
                                //inspectorBehaviour.learningLists[i] = EditorGUI.IntField(testRect, inspectorBehaviour.learningLists[i]);
                                //listTracking[i].ratio = EditorGUI.IntField(testRect, listTracking[i].ratio);
                                //Debug.Log(inspectorBehaviour.listTracking[i].ratio);
                                //if (inspectorBehaviour.learningLists[i] < 1)
                                //{
                                //    inspectorBehaviour.learningLists[i] = 1;
                                //}
                                testRect.x += testRect.width;
                                //EditorGUI.FloatField(testRect, ((float)limits[i] / (float)maxLimitsValue) * 100);

                                //GUILayout.Label(inspectorBehaviour.soundClips[i].name);
                                //EditorGUILayout.LabelField(inspectorBehaviour.soundClips[i].name);
                                //testRect.x += testRect.width / 2;
                                //testRect.width /= 2;
                                //limits[i] = EditorGUILayout.IntField(limits[i]);
                                //limits[i] = EditorGUILayout.IntField(inspectorBehaviour.soundClips[i].name, limits[i]);
                                //EditorGUILayout.FloatField(((float)limits[i] / (float)maxLimitsValue) * 100);
                                EditorGUILayout.EndHorizontal();
                            }

                            break;
                        case "Sequence":
                            //EditorGUILayout.PropertyField(ColorPoint_Serialized);
                            //inspectorBehaviour.audioSequence.clip = inspectorBehaviour.soundClips[inspectorBehaviour.audioSequence.clipIndex];
                            //for (int i = 0; i < inspectorBehaviour.audioSequence_Array.Length; i++)
                            //{
                            //    if (inspectorBehaviour.audioSequence_Array[i].clipIndex > inspectorBehaviour.soundClips.Length - 1)
                            //    {
                            //        inspectorBehaviour.audioSequence_Array[i].clipIndex = inspectorBehaviour.soundClips.Length - 1;
                            //    }
                            //    else if (inspectorBehaviour.audioSequence_Array[i].clipIndex < 0)
                            //    {
                            //        inspectorBehaviour.audioSequence_Array[i].clipIndex = 0;
                            //    }
                            //    else
                            //    {
                            //        inspectorBehaviour.audioSequence_Array[i].clip = inspectorBehaviour.soundClips[inspectorBehaviour.audioSequence_Array[i].clipIndex];
                            //    }
                            //}

                            serializedObject.Update();
                            list.DoLayoutList();
                            serializedObject.ApplyModifiedProperties();

                            //EditorGUILayout.PropertyField(soundSequence_int_Serialized);
                            //EditorGUILayout.PropertyField(soundSequence_int_Serialized.GetArrayElementAtIndex(0));
                            //Rect TestRect = GUILayoutUtility.GetLastRect();
                            //Debug.Log(EditorGUI.GetPropertyHeight(soundSequence_int_Serialized.GetArrayElementAtIndex(0)));
                            ////soundSequence_int.Array.data[0]
                            //serializedObject.ApplyModifiedProperties();
                            break;
                    }
                    break;
            }
        }
        serializedObject.ApplyModifiedProperties();

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
            //inspectorBehaviour.limits = limits;
            //if (inspectorBehaviour.serializedFromEditor.Count != listTracking.Count)
            //{
            //    inspectorBehaviour.serializedFromEditor.Clear();
            //    for (int i = 0; i < listTracking.Count; i++)
            //    {
            //        inspectorBehaviour.serializedFromEditor[i] = listTracking[i].clipIndex;
            //        Debug.Log(inspectorBehaviour.serializedFromEditor[i]);
            //    }
            //}

        }
    }
    
}
#endif
