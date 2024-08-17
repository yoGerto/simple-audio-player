
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

public class SimpleAudioPlayer : UdonSharpBehaviour
{
    /* Items used within the script */
    private Animator teddyAnimator;
    private AudioSource teddyAudio;

    public GameObject emptyGameObject;

    public AudioClip[] teddyClips;

    public AudioClip testAudioClip;

    private float timer = 0.0f;
    private float cooldownTime = 0.5f;
    private bool timerLatch = false;

    [NonSerialized] public string stringVal = "glorp";
    [NonSerialized] public bool godTest = false;

    [UdonSynced] private int RNGValue = 0;

    [SerializeField] public GameObject SAP_Child;
    [SerializeField] SimpleAudioPlayer_Child SAP_Child_Script;

    private void Start()
    {
        teddyAnimator = GetComponent<Animator>();
        teddyAudio = GetComponent<AudioSource>();
        SendCustomEvent("SetChildScriptObjects");
    }

    public void SetChildScriptObjects()
    {
        Debug.Log("Entered!");
        SAP_Child.GetComponent<SimpleAudioPlayer_Child>().SetProgramVariable("test", 1);
        SAP_Child.GetComponent<SimpleAudioPlayer_Child>().SetProgramVariable("SAPChild_AudioClips", testAudioClip);
    }

    public override void OnPickup()
    {
        Networking.SetOwner(Networking.LocalPlayer, SAP_Child);
    }

    public override void OnPickupUseDown()
    {
        //RNGValue = UnityEngine.Random.Range(0, 200);
        //SAP_Child.GetComponent<SimpleAudioPlayer_Child>().SetProgramVariable("test2", RNGValue);
        SAP_Child.GetComponent<SimpleAudioPlayer_Child>().SendCustomEvent("GenerateRNGVal");
        //RNGValue = UnityEngine.Random.Range(0, 200);
        //Debug.Log(RNGValue.ToString());
        //RequestSerialization();
    }

    public void TeddyBearAudioHandler()
    {
        int audioRNG;
        audioRNG = UnityEngine.Random.Range(0, 200);
        if (audioRNG < 100)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PlayTeddyBearAudio0");
        }
        else if (audioRNG >= 100 && audioRNG < 199)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PlayTeddyBearAudio1");
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PlayTeddyBearAudio2");
        }
    }

    public void PlayTeddyBearAudio0()
    {
        teddyAudio.PlayOneShot(teddyClips[0]);
        teddyAnimator.SetInteger("BearState", 1);
        SendCustomEventDelayedSeconds("ResetAnimatorParam", 0.05f);
    }

    public void PlayTeddyBearAudio1()
    {
        teddyAudio.PlayOneShot(teddyClips[1]);
        teddyAnimator.SetInteger("BearState", 2);
        SendCustomEventDelayedSeconds("ResetAnimatorParam", 0.05f);
    }

    public void PlayTeddyBearAudio2()
    {
        teddyAudio.PlayOneShot(teddyClips[2]);
    }

    public void ResetAnimatorParam()
    {

        teddyAnimator.SetInteger("BearState", 0);
    }

}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
[CustomEditor(typeof(SimpleAudioPlayer))]
public class CustomInspectorEditor : Editor
{
    bool boolTest1 = false;

    GameObject gameObjectTest;
    GameObject instantiatedGameObject;

    /* Learning how to create Tabs in the editor window from this YouTube tutorial from The Messy Coder:
     * https://www.youtube.com/watch?v=-sJRvRirJ9Q
     */

    private string[] m_Tabs = { "Option One", "Option Two", "Option Three", "Option Four" };
    private int m_TabsIndex = -1;

    public override void OnInspectorGUI()
    {
        // Draws the default convert to UdonBehaviour button, program asset field, sync settings, etc.
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

        DrawDefaultInspector();

        EditorGUILayout.BeginVertical();
        m_TabsIndex = GUILayout.Toolbar(m_TabsIndex, m_Tabs);
        EditorGUILayout.EndVertical();

        if (m_TabsIndex >= 0)
        {
            switch (m_Tabs[m_TabsIndex])
            {
                case "Option One":
                    EditorGUILayout.HelpBox("One", MessageType.Info);
                    break;
                case "Option Two":
                    EditorGUILayout.HelpBox("Two", MessageType.Info);
                    break;
            }
        }

        SimpleAudioPlayer inspectorBehaviour = (SimpleAudioPlayer)target;

        EditorGUI.BeginChangeCheck();

        gameObjectTest = inspectorBehaviour.gameObject;        
        
        if (gameObjectTest.GetComponent<AudioSource>() == null)
        {
            EditorGUILayout.HelpBox("Object is missing an Audio Source!", MessageType.Error);
        }

        //Debug.Log(inspectorBehaviour.GetComponentInChildren<SimpleAudioPlayer_Child>());

        if (inspectorBehaviour.GetComponentInChildren<SimpleAudioPlayer_Child>() == null)
        {
            EditorGUILayout.HelpBox("No Valid SAP Children were found! Use the button below generate one.", MessageType.Error);
            //Debug.LogError("No Valid SAP Children were found! Use the button inside of the script to generate one.");
        }

        if (GUILayout.Button("Test"))
        {
            if (instantiatedGameObject == null)
            {
                instantiatedGameObject = (GameObject)PrefabUtility.InstantiatePrefab(inspectorBehaviour.emptyGameObject, inspectorBehaviour.transform);
                inspectorBehaviour.SAP_Child = instantiatedGameObject;
            }
            else
            {
                Debug.LogWarning("A valid SAP Child has been detected. Please delete the existing SAP Child if you wish to regenerate");
            }
        }

        // A simple string field modification with Undo handling
        //int test = EditorGUILayout.IntField("Num of Audio Clips", inspectorBehaviour.teddyClips.Length);
        //string newStrVal = EditorGUILayout.TextField("String Val", inspectorBehaviour.stringVal);
        bool boolTest = EditorGUILayout.Toggle("Bool", inspectorBehaviour.godTest);

        if (boolTest)
        {
            EditorGUILayout.HelpBox("Testing", MessageType.Info);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(inspectorBehaviour, "Modify string val");

            inspectorBehaviour.godTest = boolTest;
            //inspectorBehaviour.stringVal = newStrVal;
        }
    }
}
#endif
