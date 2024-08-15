
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using VRC.Udon.Common;
using System.Threading;
using UnityEditor;
using UdonSharpEditor;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SimpleAudioPlayer : UdonSharpBehaviour
{
    /* Items used within the script */
    private Animator teddyAnimator;
    private AudioSource teddyAudio;


    public AudioClip[] teddyClips;

    private float timer = 0.0f;
    private float cooldownTime = 0.5f;
    private bool timerLatch = false;

    public string stringVal = "glorp";
    public bool godTest = false;

    private void Start()
    {
        teddyAnimator = GetComponent<Animator>();
        teddyAudio = GetComponent<AudioSource>();
    }

    public override void OnPickupUseDown()
    {
        if (timerLatch)
        {
            float currentTime = Time.time;
            if (currentTime - timer > cooldownTime)
            {
                TeddyBearAudioHandler();
                timer = currentTime;
            }
        }
        else
        {
            TeddyBearAudioHandler();
            timer = Time.time;
            timerLatch = true;
        }
    }

    public void TeddyBearAudioHandler()
    {
        int audioRNG;
        audioRNG = Random.Range(0, 200);
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
[CustomEditor(typeof(TeddyBear))]
public class CustomInspectorEditor : Editor
{
    bool boolTest1 = false;

    GameObject gameObjectTest;

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
            switch(m_Tabs[m_TabsIndex])
            {
                case "Option One":
                    EditorGUILayout.HelpBox("One", MessageType.Info); 
                    break;
                case "Option Two":
                    EditorGUILayout.HelpBox("Two", MessageType.Info);
                    break;
            }
        }

        TeddyBear inspectorBehaviour = (TeddyBear)target;

        EditorGUI.BeginChangeCheck();

        gameObjectTest = inspectorBehaviour.gameObject;

        if (gameObjectTest.GetComponent<AudioSource>() == null )
        {
            EditorGUILayout.HelpBox("Object is missing an Audio Source!", MessageType.Error);
        }

        // A simple string field modification with Undo handling
        int test = EditorGUILayout.IntField("Num of Audio Clips", inspectorBehaviour.teddyClips.Length);
        string newStrVal = EditorGUILayout.TextField("String Val", inspectorBehaviour.stringVal);
        bool boolTest = EditorGUILayout.Toggle("Bool", inspectorBehaviour.godTest);

        if (boolTest)
        {
            EditorGUILayout.HelpBox("I'm over here stroking my dick I got lotion on my shit right now I'm horny as fuck man I'm a freak", MessageType.Info);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(inspectorBehaviour, "Modify string val");

            inspectorBehaviour.godTest = boolTest;
            inspectorBehaviour.stringVal = newStrVal;
        }
    }
}
#endif
