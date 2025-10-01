using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EnglishDolphin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static Dictionary<string, string> Translations;
    internal const string Translatable = "[一-龠ぁ-ゔァ-ヴーａ-ｚＡ-Ｚ０-９々〆〤]+";
    internal const string Page = "([0-9]+/[0-9]+) ページ";
    public static byte[] titleData;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        titleData = File.ReadAllBytes(BepInEx.Paths.PluginPath + "/EnglishDolphin/res/DolphinTitle.png");

        InitDictionary();

        // Patch methods
        var harmony = new Harmony("dev.penguinowl.dolphin.english");
        harmony.PatchAll();
    }

    private void InitDictionary()
    {
        int count = 0;
        Translations = new Dictionary<string, string>();
        var data = File.ReadAllText(BepInEx.Paths.PluginPath + "/EnglishDolphin/res/en.txt", System.Text.Encoding.UTF8);
        var pairs = data.Split(new[] { "\n\n" }, StringSplitOptions.None);
        foreach (string pair in pairs)
        {
            // Logger.Log(LogLevel.Info, "Trying " + pair);
            try
            {
                var strings = pair.Replace("\\\\", "").Split("\n-\n", StringSplitOptions.None);
                if (strings.Length >= 2)
                {
                    Translations.Add(strings[0].Trim(), strings[1]);
                    count++;
                    // Logger.Log(LogLevel.Info, "Found translation " + strings[0] + " to " + strings[1]);
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e.ToString());
            }
        }
        Logger.Log(LogLevel.Info, "Found " + count + " translations");
    }
}

[HarmonyPatch(typeof(UnityEngine.UI.Text))]
[HarmonyPatch(nameof(UnityEngine.UI.Text.text), MethodType.Setter)]
class Patch01
{
    static void Prefix(UnityEngine.UI.Text __instance, ref string value)
    {
        if (value == null)
        {
            return;
        }
        string key = value.Trim().Replace("\r", "");
        if (!Regex.IsMatch(key, Plugin.Translatable))
        {
            return;
        }
        else if (Regex.IsMatch(key, Plugin.Page))
        {
            value = Regex.Replace(key, Plugin.Page, "Page $1");
        }
        else if (Plugin.Translations.ContainsKey(key))
        {
            value = Plugin.Translations[key];
        }
        else if (!key.Equals(""))
        {
            Plugin.Logger.Log(LogLevel.Info, "No translation for: \"" + key + "\"");
        }
    }

}

[HarmonyPatch(typeof(UnityEngine.UI.MaskableGraphic))]
[HarmonyPatch("OnEnable")]
class Patch02
{
    static void Postfix(UnityEngine.UI.MaskableGraphic __instance)
    {
        if (!(__instance is UnityEngine.UI.Text))
            return;
        var instance = (UnityEngine.UI.Text)__instance;
        if (instance.text == null)
        {
            return;
        }
        string key = instance.text.Trim().Replace("\r", "");
        if (!Regex.IsMatch(key, Plugin.Translatable))
        {
            return;
        }
        else if (Regex.IsMatch(key, Plugin.Page))
        {
            instance.text = Regex.Replace(key, Plugin.Page, "Page $1");
        }
        else if (Plugin.Translations.ContainsKey(key))
        {
            instance.text = Plugin.Translations[key];
        }
        else if (!key.Equals(""))
        {
            Plugin.Logger.Log(LogLevel.Info, "No translation for: \"" + key + "\"");
        }
    }

}

[HarmonyPatch(typeof(UnityEngine.GameObject))]
[HarmonyPatch("SetActive")]
class Patch03
{
    static void Prefix(GameObject __instance, ref bool value)
    {
        if (__instance.name != "DolphinTitle")
            return;
        if (!value)
            return;

        SpriteRenderer renderer = __instance.GetComponent<SpriteRenderer>();
        renderer.drawMode = SpriteDrawMode.Sliced;

        ImageConversion.LoadImage(renderer.sprite.texture, Plugin.titleData);
    }

}

