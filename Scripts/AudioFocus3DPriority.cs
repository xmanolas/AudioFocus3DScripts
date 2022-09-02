using UnityEngine;


/// <summary>
/// 
/// Variation of the AudioFocus3D class that takes into account the priority of Audiosources.
/// The priority attribute is utilised in order to select particular Audiosources instead of a blanket selection.
///
/// The AudioFocus3D class uses the camera FOV to identify Audiosources within and outside the screen.
/// It can be used as a primitive dway to dynamically alter Audiosource attributes depending on whether
/// the emitting game objects are on or off screen processing.
/// 
/// </summary>

public class AudioFocus3DPriority : MonoBehaviour
{

    // FOV declarations
    public Camera mainCamera;
    private AudioSource[] audioSources;
    private readonly float rampSpeed = 200.0f;
    private int prioritySound = 256;
    private bool soundOnFOV = false;
    public bool onOffSwitch = false;

    // Volume Attenuation declarations
    [Range(0.01f, 1.0f)]
    public float volAtt = 1.0f;
    private float[] initialVol;

    // LPF declarations
    [Range(20.0f, 22000.0f)]
    public float cutOffLPF = 100.0f;
    [Range(1.0f, 10.0f)]
    public float resLPF = 1.0f;

    void Start()
    {

        // Collect all audio sources and their initial volume levels
        audioSources = (AudioSource[])GameObject.FindObjectsOfType(typeof(AudioSource));
        initialVol = new float[audioSources.Length];
        
        for (int i = 0; i < audioSources.Length; i++)
        {
            initialVol[i] = audioSources[i].volume;

            if (audioSources[i].priority < prioritySound)
            {
                prioritySound = audioSources[i].priority;
            }
        }

        // Update Audio components every 100ms
        InvokeRepeating(nameof(FindFOVAudioSources), 0.1f, 0.1f);
        //CreateLPF();
        //SetLPFParams(cutOffFreq, lowPassResQ);
        
    }

    void Update()
    {

        // *** CODE FOR TESTING THE FILTERS IN-GAME - IGNORE *** //

        // Check whether the scene contains any audio sources and initialise components
        //FindFOVAudioSources();
        //CreateLPF();
        //SetLPFParams(cutOffLPF, resLPF);

        // LP Filter controls
        /*if (Input.GetKey(KeyCode.RightArrow))
        {
            cutOffFreq += 10.0f;
            SetLPFParams(cutOffLPF, resLPF);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            cutOffFreq -= 10.0f;
            SetLPFParams(cutOffLPF, resLPF);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            lowPassResQ += 0.01f;
            SetLPFParams(cutOffLPF, resLPF);
        }

        if (Input.GetKey(KeyCode.W))
        {
            lowPassResQ -= 0.01f;
            SetLPFParams(cutOffLPF, resLPF);
        }*/

        // *** CODE FOR TESTING THE FILTERS IN-GAME - IGNORE *** //

    }

    // Find audio sources within and outside trigger area
    private void FindFOVAudioSources() 
    {

        if (onOffSwitch == true)
        {
            // Original code from Sebastian Lague https://www.youtube.com/watch?v=rQG9aUWarwE https://github.com/SebLague/Field-of-View
            for (int i = 0; i < audioSources.Length; i++)
            {
                // Process audio sources with SpatialBlend > 0 (not fully 2D)
                //if ((Vector3.Angle(mainCamera.transform.forward, dirToTarget) > mainCamera.fieldOfView) && audioSources[i].gameObject.GetComponent<AudioSource>().spatialBlend != 0.0f)
                if (audioSources[i].priority == prioritySound)
                {
                    Vector3 dirToTarget = (audioSources[i].gameObject.transform.position - mainCamera.transform.position).normalized;

                    if (Vector3.Angle(mainCamera.transform.forward, dirToTarget) > mainCamera.fieldOfView)
                    {
                        // Code for sound objects outside camera FOV
                        soundOnFOV = false;
                    }
                    else
                    {
                        // Code for sound objects within camera FOV
                        soundOnFOV = true;
                    }
                }
                else
                {
                    if (soundOnFOV == true)
                    {
                        // Code for sound objects outside camera FOV
                        AdjustVolume(i, audioSources[i].volume, initialVol[i]);
                        // LPF Off
                        //EnableLPF(i, false);
                    }
                    else
                    {
                        // Code for sound objects within camera FOV
                        AdjustVolume(i, audioSources[i].volume, initialVol[i] * volAtt);
                        // LPF On
                        //EnableLPF(i, true);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                AdjustVolume(i, audioSources[i].volume, initialVol[i]);
            }
        }

    }




    // ========================== Volume =========================

    private void AdjustVolume(int i, float startVol, float endVol)
    {

        // Adjust volume level with linear interpolation for ramping between values
        audioSources[i].volume = Mathf.Lerp(startVol, endVol, rampSpeed * Time.deltaTime);

    }




    // =========================== LPF ===========================

    private void CreateLPF()
    {

        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].gameObject.AddComponent<AudioLowPassFilter>();
        }

    }

    private void EnableLPF(int i, bool enableLPF)
    {

        audioSources[i].GetComponent<AudioLowPassFilter>().enabled = enableLPF;

    }

    private void SetLPFParams(float cutoffFreq, float lowpassResQ)
    {

        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].gameObject.GetComponent<AudioLowPassFilter>().cutoffFrequency = cutoffFreq;
            audioSources[i].gameObject.GetComponent<AudioLowPassFilter>().lowpassResonanceQ = lowpassResQ;
        }

    }




    // ===================== dB Conversions ======================

    private float LinearToDecibel(float linear)
    {

        float dB;

        if (linear != 0)
        {
            dB = 20.0f * Mathf.Log10(linear);
        }  
        else
        {
            dB = -144.0f;
        }
            
        return dB;

    }

    private float DecibelToLinear(float dB)
    {

        float linear = Mathf.Pow(10.0f, dB / 20.0f);

        return linear;

    }

}
