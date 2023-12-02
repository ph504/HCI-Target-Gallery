using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;

public class GunShoot : MonoBehaviour
{
    public bool aimAssist;
    [SerializeField] private float assistRadius;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float impactForce = 100f;
    [SerializeField] private float range = 100f;
    [SerializeField] private float firingRate = 15f;
    [SerializeField] private int trialCount;
    [SerializeField] private int maxBlocks;
    public Camera FPSCam;

    private float nextTimetoFire = 0f;
    private int missCount;
    private int hitCount;
    private int trialsNum;
    private int blockCount;
    private float accuracy;
    private float aimDuration;
    private float startTime;
    private bool isBreak;

    [SerializeField] private AudioSource fireAudioSource;

    [SerializeField] private GameObject breakUI;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private TextMeshProUGUI blockText;
    [SerializeField] private TextMeshProUGUI trialText;

    public MouseLook mouseLookScript;
    public AnalogJoystickController analogJoystickControllerScript;
    public GyroAim gyroAimScript;

    private MonoBehaviour[] inputComponents;
    void Start()
    {
        inputComponents = new MonoBehaviour[] {  mouseLookScript, analogJoystickControllerScript, gyroAimScript};
        trialText.text = UpdateText("trials",trialsNum, trialCount);
        blockText.text = UpdateText("blocks",blockCount, maxBlocks);
        startTime = Time.time;
    }
    private void Update()
    {
        // Debug.Log(blockCount);
        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimetoFire)
        {
            nextTimetoFire = Time.time + 1f / firingRate;
            if(!isBreak)
                Shoot();
                // If the firing sound is already playing, stop it
            if (fireAudioSource.isPlaying)
            {
                fireAudioSource.Stop();
            }

            // Play the firing sound
            fireAudioSource.Play();
        }
    }

    public string UpdateText(string prompt, int currentCount, int totalCount)
    {
        return prompt + " " + currentCount + "/" + totalCount;
    }

    public void CheckForBreak()
    {
        hitCount+=1;
        trialsNum+=1;
        startTime = Time.time;
        // Debug.Log( " blockCount: " + blockCount +" trialsNum: " + trialsNum + " hitCount: " + hitCount + " missCount: " + missCount + " accuracy: " + accuracy + " aimDuration: " + aimDuration);
       int controllerIndex = PlayerPrefs.GetInt("ControllerIndex", 0);
       string inputType = inputComponents[controllerIndex].GetType().Name;
        // Create a string with the data you want to save
        string data = ""+inputType+","+(blockCount+1) + "," + trialsNum + "," + hitCount + "," + missCount + "," + accuracy + "," + aimDuration+"\n";
        Debug.Log(data);
        SaveData(data);
        if(hitCount>=trialCount)
        {
            blockCount+=1;
            breakUI.SetActive(true);
            mouseLook.VisualizeCursor(true);
            // SaveData();
            hitCount = 0;
            Time.timeScale = 0;
            isBreak = true;
            if (blockCount>=maxBlocks)
            {
                gameObject.GetComponent<ChangeScene>().LoadScene();
            }
        }
        trialText.text = UpdateText("trials",trialsNum, trialCount);
        blockText.text = UpdateText("blocks",blockCount, maxBlocks);
    }

    public void SaveData(string data)
    {
            // Get the path of the file
        string path = Path.Combine(Application.dataPath, "saveData.csv");
        // Write the data to the file
        File.AppendAllText(path, data);
    }

    public void AfterBreak()
    {
        startTime = Time.time;
        Time.timeScale = 1.0f;
        isBreak = false;
        missCount = 0;
        trialsNum = 0;
        trialText.text = UpdateText("trials",trialsNum, trialCount);
        
    }
    void Shoot()
    {
        RaycastHit hit;
        if (aimAssist)
        {
            if(Physics.SphereCast(FPSCam.transform.position, assistRadius, FPSCam.transform.forward, out hit, range))
            {
                Hit(hit);
                Vector3 distanceVec = hit.transform.position - FPSCam.transform.position;
                accuracy = distanceVec.magnitude - assistRadius;
                CheckForBreak();
            }
            else
            {
                missCount+=1;
            }
        }
        else if (Physics.Raycast(FPSCam.transform.position, FPSCam.transform.forward, out hit, range))
        {
            Hit(hit);
            Vector3 distanceVec = hit.transform.position - FPSCam.transform.position;
            accuracy = distanceVec.magnitude;
            CheckForBreak();
        }
        else
        {
            missCount+=1;
        }
    }

    void Hit(RaycastHit hit)
    {
        aimDuration = Time.time - startTime;
        // Debug.Log(hit.transform.name);

        if (hit.rigidbody != null)
        {
            hit.rigidbody.AddForce(-hit.normal * impactForce);
        }
        Target target = hit.transform.GetComponent<Target>();
        if(target != null){
            target.TakeDamage(damage);
        }
    }
}
