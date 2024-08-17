
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SimpleAudioPlayer_Child : UdonSharpBehaviour
{
    AudioSource SAPChild_AudioSource;
    [SerializeField] AudioClip SAPChild_AudioClips;

    [SerializeField] SimpleAudioPlayer SAP;
    [SerializeField] GameObject SAP_GameObject;

    [SerializeField] public int test = 0;

    //[UdonSynced, FieldChangeCallback(nameof(RNGValueReciever))] public int test2 = 0;
    [UdonSynced] public int test2 = 0;

    void Start()
    {
        SAP = this.transform.parent.GetComponent<SimpleAudioPlayer>();
        SAP_GameObject = this.transform.parent.gameObject;
        SAPChild_AudioSource = GetComponent<AudioSource>();
    }

    public void GenerateRNGVal()
    {
        if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer)
        {
            Debug.Log("I am not the owner of this script");
        }
        test2 = Random.Range(0, 100);
        RequestSerialization();
        OnDeserialization();
    }

    //public int RNGValueReciever
    //{
    //    set
    //    {
    //        if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer)
    //        {
    //            return;
    //        }

    //        test2 = value;
    //        RequestSerialization();
    //        OnDeserialization();
    //    }
    //    get { return test2; }
    //}

    private void Update()
    {
        Debug.Log("test = " + test);
    }

    public override void OnDeserialization()
    {
        Debug.Log("RNG Val = " + test2);
    }
}
