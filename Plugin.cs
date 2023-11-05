using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Utilla;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using HoneyLib;
using HoneyLib.Events;

namespace ClipOverlay
{
    [HarmonyPatch]
    [BepInDependency("org.legoandmars.gorillatag.utilla")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static GameObject TagThingi = null;
        public static Text TagText = null;
        public static Text FPSText = null;
        public static string screenshotPath;

        [HarmonyPatch(typeof(GorillaTagger), "Update")]
        [HarmonyPostfix]
        private static void Update_Postfix()
        {
            if (TagThingi != null)
            {
                TagThingi.SetActive(true);
            }
        }

        void Start()
        {
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void OnEnable()
        {
            TagThingi.SetActive(true);
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void Awake()
        {
            Utilla.Events.GameInitialized += GameInitialized;
        }

        private void GameInitialized(object sender, EventArgs e)
        {
            Utilla.Events.GameInitialized += GameInitialized;
            HoneyLib.Events.Events.TagHitLocal += TagHitLocal;

            TagThingi = new GameObject("Tag Thingi");
            TagThingi.transform.SetParent(GorillaTagger.Instance.mainCamera.transform, false);
            TagThingi.transform.localPosition = new Vector3(0.1f, 0.07f, 0.5f);
            TagThingi.transform.localScale = new Vector3(0.0025f, 0.0025f, 0.0025f);

            var canvas = TagThingi.AddComponent<Canvas>();
            canvas.sortingOrder = 999;

            GameObject textObject = new GameObject("Tag Text");
            textObject.transform.SetParent(TagThingi.transform, false);
            textObject.AddComponent<CanvasRenderer>();
            TagText = textObject.AddComponent<Text>();
            TagText.font = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/NameTagAnchor/NameTagCanvas/Text/")?.GetComponent<Text>().font;
            TagText.color = Color.white;
            TagText.alignment = TextAnchor.MiddleCenter;
            TagText.material.shader = Shader.Find("GUI/Text Shader");

           
            GameObject fpsObject = new GameObject("FPS Text");
            fpsObject.transform.SetParent(TagThingi.transform, false);
            fpsObject.AddComponent<CanvasRenderer>();
            FPSText = fpsObject.AddComponent<Text>();
            FPSText.font = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/NameTagAnchor/NameTagCanvas/Text/")?.GetComponent<Text>().font;
            FPSText.color = Color.green;
            FPSText.alignment = TextAnchor.UpperRight;
            FPSText.material.shader = Shader.Find("GUI/Text Shader");

            RectTransform tagRectTransform = TagText.GetComponent<RectTransform>();
            RectTransform fpsRectTransform = FPSText.GetComponent<RectTransform>();
            fpsRectTransform.anchoredPosition = new Vector2(tagRectTransform.rect.width / 2f, -tagRectTransform.rect.height / 2f);

            screenshotPath = Path.Combine(Paths.PluginPath, "tags");

            if (!Directory.Exists(screenshotPath))
            {
                Directory.CreateDirectory(screenshotPath);
            }
        }

        void OnDisable()
        {
            TagThingi.SetActive(false);
            HarmonyPatches.RemoveHarmonyPatches();
        }

        public void TagHitLocal(object sender, TagHitLocalArgs e)
        {
            string playerName = e.taggedPlayer.NickName;
            StartCoroutine(DisplayTagMessage(playerName));

            StartCoroutine(takescreenshot(playerName));
        }

        IEnumerator DisplayTagMessage(string playerName)
        {
            string message = "YOU JUST SNAPPED " + playerName + "!";
            Debug.Log(message);
            TagText.text = message;
            TagThingi.SetActive(true);

            float displayDuration = 1f;

            yield return new WaitForSeconds(displayDuration);

            TagText.text = "";
        }

        IEnumerator takescreenshot(string playerName)
        {
            yield return new WaitForEndOfFrame();

            string screenshotFilename = $"Tag_screenshot_{playerName}_{DateTime.Now:yyyyMMddHHmmss}.png";

            ScreenCapture.CaptureScreenshot(Path.Combine(screenshotPath, screenshotFilename));

            Debug.Log($"screenshot saved: {screenshotFilename}");
        }

        void Update()
        {
            if (FPSText != null)
            {
                FPSText.text = "FPS: " + (1f / Time.deltaTime).ToString("F0");
            }
        }
    }
}
