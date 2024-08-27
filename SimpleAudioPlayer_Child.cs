
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class SimpleAudioPlayer_Child : UdonSharpBehaviour
{
    AudioSource SAPChild_AudioSource;
    [SerializeField] AudioClip[] SAPChild_AudioClips;

    [UdonSynced, FieldChangeCallback(nameof(limitsIn)), HideInInspector] public int[] limits;

    //Can use this int for both Random and Sequence types, as they both wont be used simultaneously
    [UdonSynced] int syncedInt = 0;

    [HideInInspector] public int[] selectedTabs = new int[2];
    [HideInInspector] public int[] soundSequence_int;
    [HideInInspector] public int selectedSound, maxRNGVal = 0;
    [HideInInspector] public bool isError, firstSerializationComplete = false;
    [HideInInspector] private float firstSerializationTimer = 0.0f;

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

    void Start()
    {
        if (Networking.IsMaster)
        {
            firstSerializationComplete = true;
        }
        SAPChild_AudioSource = GetComponent<AudioSource>();
    }

    void ProblemReject()
    {
        Debug.LogError("There is a problem with the setup of your Simple Audio Player script. Please resolve the problem before using the script.");
        Debug.LogWarning("Please contact me on the yoGerto GitHub page if you are having technical issues or bugs");
    }

    public void PlaySound_AllSettings()
    {
        if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer)
        {
            Debug.Log("I am not the owner of this script");
            return;
        }

        if (isError)
        {
            ProblemReject();
            return;
        }

        // We only change values if we are in the Multiple Sounds mode so only check for that
        // If it's single sound then just RequestSerialization at the end
        if (selectedTabs[0] == 1)
        {
            // Random Mode
            if (selectedTabs[1] == 0)
            {
                syncedInt = Random.Range(1, maxRNGVal + 1);
            }
            // Sequence Mode
            else if (selectedTabs[1] == 1)
            {
                syncedInt++;
            }
            // Catch all
            else
            {
                Debug.LogError("Invalid tab selection");
                return;
            }
        }

        RequestSerialization();
        OnDeserialization();
    }

    private void Update()
    {
        if (!firstSerializationComplete)
        {
            firstSerializationTimer += Time.deltaTime;
            if (firstSerializationTimer > 2.0f)
            {
                // Use first Serialization to get up-to-date data
                // Exclude sound playing logic from first Serialization
                // This time out is here just incase they do not Serialize for whatever reason, whether valid or not.
                firstSerializationComplete = true;
            }
        }
    }

    public override void OnDeserialization()
    {
        if (firstSerializationComplete)
        {
            if (selectedTabs[0] == 0)
            {
                SAPChild_AudioSource.PlayOneShot(SAPChild_AudioClips[selectedSound]);
            }
            else if (selectedTabs[0] == 1)
            {
                if (selectedTabs[1] == 0)
                {
                    int storedVal = 0;
                    for (int i = 0; i < limits.Length; i++)
                    {
                        //...can definitely rewrite this, feels very inefficient this way
                        for (int j = 1; j <= limits[i]; j++)
                        {
                            if ((j + storedVal) == syncedInt)
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
                else if (selectedTabs[1] == 1)
                {
                    int correctSoundFile = syncedInt - 1;
                    int integerDivision = correctSoundFile / soundSequence_int.Length;
                    correctSoundFile = correctSoundFile - (integerDivision * soundSequence_int.Length);

                    SAPChild_AudioSource.PlayOneShot(SAPChild_AudioClips[soundSequence_int[correctSoundFile]]);
                }
            }
        }

        // Skip the first Serialization if player is joining instance with modified variables
        // Data will still be gotten but the logic for playing sounds with data will be skipped for first cycle
        if (!firstSerializationComplete)
        {
            firstSerializationComplete = true;
        }

    }
}
