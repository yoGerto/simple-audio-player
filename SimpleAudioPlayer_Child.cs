
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SimpleAudioPlayer_Child : UdonSharpBehaviour
{
    AudioSource SAPChild_AudioSource;
    [SerializeField] AudioClip[] SAPChild_AudioClips;

    [SerializeField] SimpleAudioPlayer SAP;
    [SerializeField] GameObject SAP_GameObject;

    [SerializeField] public int test = 0;

    //[UdonSynced, FieldChangeCallback(nameof(RNGValueReciever))] public int test2 = 0;
    [UdonSynced] public int RNGVal = 0;

    [UdonSynced, FieldChangeCallback(nameof(limitsIn))] public int[] limits;
    [SerializeField] public int maxRNGVal;

    void Start()
    {
        SAP = this.transform.parent.GetComponent<SimpleAudioPlayer>();
        SAP_GameObject = this.transform.parent.gameObject;
        SAPChild_AudioSource = GetComponent<AudioSource>();
    }

    public int[] limitsIn
    {
        set
        {
            limits = value;
            maxRNGVal = 0;
            for (int i = 0; i < value.Length; i++)
            {
                maxRNGVal += value[i];
            }
        }
        get { return limits; }
    }

    public void GenerateRNGVal()
    {
        if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer)
        {
            Debug.Log("I am not the owner of this script");
            return;
        }
        //Random.Range 2nd parameter is maxExclusive, so +1 to it to capture full range of values
        RNGVal = Random.Range(1, maxRNGVal + 1);
        RequestSerialization();
        OnDeserialization();
    }

    public override void OnDeserialization()
    {
        Debug.Log("RNG Val = " + RNGVal);

        int storedVal = 0;

        for (int i = 0; i < limits.Length; i++)
        {
            for (int j = 1; j <= limits[i]; j++)
            {
                if ((j + storedVal) == RNGVal)
                {
                    // The .Length property returns the size of the array, but i index starts at 0, so add one to i to see if we are at an index greater than the length of the AudioClip array
                    // i.e. if AudioClips had 3 objects in it, it's max index would be [2], but if i was 3 (the 4th index), .Length would return 3 and they would be equal
                    // But if we index AudioClips[3], this would crash the script as we are indexing out of the array range, which is understandable
                    // ...I'm not sure how often this would happen but I feel it's worth it to check just in case!
                    if (i + 1 > SAPChild_AudioClips.Length)
                    {
                        Debug.LogError("Attempted to play sound outside array index range! Returning...");
                        return;
                    }

                    SAPChild_AudioSource.PlayOneShot(SAPChild_AudioClips[i]);
                    return;
                }
            }
            storedVal += limits[i];
        }
    }
}
