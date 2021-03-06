﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;
using System;

public class ControllerScript : MonoBehaviour
{

    SteamVR_TrackedObject trackedObj;
    SteamVR_Controller.Device device;

    public Overlay overlay;
    public GameObject canvas;
    public Overlay loadingSceen;
    public Sprite cursorSprite;
    public GameObject HMD;
    public GameObject otherController;


    GameObject cursor { get; set; } 
    GameObject yesButton;
    GameObject noButton;
    Color normalColor;
    Color highlightedColor;
    Color pressedColor;
    bool hasOverlay;
    bool inYes;
    bool inNo;
    bool hasPressedYes;
    float canvasWidth;
    float canvasHeight;
    Vector3 yesButtonPos;
    Rect yesButtonRect;
    Vector3 noButtonPos;
    Rect noButtonRect;
    float x;
    float y;
    float timer;
    string buildPath;
    string launcherPath;

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Start()
    {
        instantiateCursor();

        hasOverlay = false;
        hasPressedYes = false;

        normalColor = new Color32(45, 77, 170, 255);
        highlightedColor = new Color32(135, 166, 255, 255);
        pressedColor = new Color32(0, 255, 2, 255);


        yesButton = GameObject.FindGameObjectWithTag("Yes");
        noButton = GameObject.FindGameObjectWithTag("No");
        overlay.gameObject.SetActive(false);
        loadingSceen.gameObject.SetActive(false);
        canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        canvasHeight = canvas.GetComponent<RectTransform>().rect.height;
        yesButtonPos = yesButton.transform.localPosition;
        yesButtonRect = yesButton.GetComponent<RectTransform>().rect;
        noButtonPos = noButton.transform.localPosition;
        noButtonRect = noButton.GetComponent<RectTransform>().rect;
        inYes = false;
        inNo = false;

        buildPath = Environment.CurrentDirectory;
        launcherPath = Path.Combine(buildPath, "revivelauncher.exe");
        launcherPath = launcherPath.Replace("\\", "/");

    }

    void Update()
    {
        timer += Time.deltaTime;
        device = SteamVR_Controller.Input((int)trackedObj.index);
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.System))
        {
            Process[] processes = Process.GetProcessesByName("revivelauncher");
            if(processes.Length == 0)
            {
                if (!overlay.gameObject.activeSelf)
                {
                    toggleCursor();
                    spawnOverlay(overlay);
                    //RenderSettings.skybox = skyboxMat;
                    hasOverlay = true;

                }
                else
                {
                    toggleCursor();
                    overlay.gameObject.SetActive(false);
                    hasOverlay = false;

                }
            }
        }
        if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (inYes)
            {
                toggleCursor();
                yesButton.GetComponent<Button>().image.color = Color.green;
                UnityEngine.Debug.Log("pressed yes");
                hasPressedYes = true;
                overlay.gameObject.SetActive(false);


                Process[] processes = Process.GetProcessesByName("revivelauncher");
                for (int i = 0; i < processes.Length; i++)
                {
                    UnityEngine.Debug.Log("nag sulod dri \n");
                    processes[i].Kill();
                }
                Process.Start(launcherPath);
                spawnOverlay(loadingSceen);
                timer = 0;
                //StartCoroutine(disableLoading());
            }
            else if (inNo)
            {
                toggleCursor();
                noButton.GetComponent<Button>().image.color = pressedColor;
                UnityEngine.Debug.Log("pressed no");
                overlay.gameObject.SetActive(false);
            }
        }
        if (hasPressedYes && timer >= 3.0f)
        {
            loadingSceen.gameObject.SetActive(false);
        }
        if (hasOverlay)
        {
            var uvs = overlay.getUVs(gameObject.transform.position, gameObject.transform.forward);
            x = uvs.x * canvasWidth;
            y = (1 - uvs.y) * canvasHeight;

            if (x != 0 && y != canvasHeight)
            {
                x -= (canvasWidth / 2);
                y -= (canvasHeight / 2);
            }
            Vector2 pos = new Vector2(x, y);
            cursor.transform.localPosition = pos;

        }
        if ((x > (yesButtonPos.x - (yesButtonRect.width / 2)) && x < (yesButtonPos.x + (yesButtonRect.width / 2))) && (y < (yesButtonPos.y + (yesButtonRect.height / 2)) && y > (yesButtonPos.y - (yesButtonRect.height / 2))))
        {
            yesButton.GetComponent<Button>().image.color = highlightedColor;
            inYes = true;
        }
        else if ((x > (noButtonPos.x - (noButtonRect.width / 2)) && x < (noButtonPos.x + (noButtonRect.width / 2))) && (y < (noButtonPos.y + (noButtonRect.height / 2)) && y > (noButtonPos.y - (noButtonRect.height / 2))))
        {
            noButton.GetComponent<Button>().image.color = highlightedColor;
            inNo = true;
        }
        else
        {
            resetButtons();
        }
    }

    private void resetButtons()
    {
        yesButton.GetComponent<Button>().image.color = normalColor;
        noButton.GetComponent<Button>().image.color = normalColor;
        inYes = false;
        inNo = false;
    }

    private void instantiateCursor()
    {
        cursor = new GameObject("cursor");

        Image image = cursor.AddComponent<Image>();
        image.sprite = cursorSprite;

        cursor.transform.SetParent(canvas.transform);
        cursor.transform.localPosition = Vector3.zero;
        cursor.transform.localRotation = Quaternion.identity;
        cursor.transform.localScale = Vector3.one / 5;
    }

    public void toggleCursor()
    {
        UnityEngine.Debug.Log(otherController.activeSelf);
        if (otherController.activeSelf)
        {
            otherController.GetComponent<ControllerScript>().cursor.GetComponent<Image>().enabled = !otherController.GetComponent<ControllerScript>().cursor.GetComponent<Image>().enabled;
            otherController.GetComponent<ControllerScript>().enabled = !otherController.GetComponent<ControllerScript>().enabled;
        }
    }

    public void spawnOverlay(Overlay overlay)
    {
        overlay.gameObject.SetActive(true);
        overlay.transform.position = new Vector3(HMD.gameObject.transform.position.x, 1.5f, HMD.gameObject.transform.position.z) + (new Vector3(HMD.transform.forward.x, 0f, HMD.transform.forward.z) * 3f);
        overlay.transform.rotation = Quaternion.Euler(0f, HMD.transform.eulerAngles.y, 0f);
    }

    IEnumerator disableLoading()
    {
        yield return new WaitForSeconds(3);

        loadingSceen.gameObject.SetActive(false);
    }
}
