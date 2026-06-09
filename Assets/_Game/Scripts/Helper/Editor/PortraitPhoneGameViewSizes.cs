#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Registers portrait phone aspect ratios and common device resolutions in the Game View dropdown.
/// Editor-only; does not affect runtime builds.
/// </summary>
[InitializeOnLoad]
public static class PortraitPhoneGameViewSizes
{
    private const string MenuPath = "Tools/Game View/Add Portrait Phone Sizes";

    private static readonly (int width, int height, string label)[] PortraitRatios =
    {
        (3, 4, "Phone 3:4 (Legacy)"),
        (9, 16, "Phone 9:16 (Standard)"),
        (9, 17, "Phone 9:17"),
        (9, 18, "Phone 9:18"),
        (10, 19, "Phone 10:19"),
        (9, 19, "Phone 9:19"),
        (18, 39, "Phone 9:19.5 (iPhone X+)"),
        (9, 20, "Phone 9:20 (Pixel/Android)"),
        (9, 21, "Phone 9:21 (Ultra Tall)"),
        (9, 22, "Phone 9:22"),
    };

    private static readonly (int width, int height, string label)[] PortraitResolutions =
    {
        (360, 640, "Phone 360x640 (Small Android)"),
        (390, 844, "Phone 390x844 (iPhone 14/15)"),
        (412, 915, "Phone 412x915 (Pixel 7)"),
        (430, 932, "Phone 430x932 (iPhone 14 Pro Max)"),
        (750, 1334, "Phone 750x1334 (iPhone SE)"),
        (1080, 1920, "Phone 1080x1920 (FHD)"),
        (1080, 2340, "Phone 1080x2340 (Galaxy S23)"),
        (1080, 2400, "Phone 1080x2400 (Pixel 7)"),
        (1170, 2532, "Phone 1170x2532 (iPhone 14)"),
        (1179, 2556, "Phone 1179x2556 (iPhone 14 Pro)"),
        (1284, 2778, "Phone 1284x2778 (iPhone 14 Plus)"),
        (1290, 2796, "Phone 1290x2796 (iPhone 14 Pro Max)"),
        (1440, 3120, "Phone 1440x3120 (Galaxy S24 Ultra)"),
    };

    static PortraitPhoneGameViewSizes()
    {
        EditorApplication.delayCall += AutoRegisterOnLoad;
    }

    private static void AutoRegisterOnLoad()
    {
        RegisterSizes();
    }

    [MenuItem(MenuPath)]
    private static void RegisterSizesMenu()
    {
        int added = RegisterSizes();
        Debug.Log($"[PortraitPhoneGameViewSizes] Added {added} Game View size(s). Open Game View and use the aspect dropdown to select them.");
    }

    private static int RegisterSizes()
    {
        int added = 0;

        foreach (var entry in PortraitRatios)
        {
            if (TryAddSize(GameViewSizeKind.AspectRatio, entry.width, entry.height, entry.label))
            {
                added++;
            }
        }

        foreach (var entry in PortraitResolutions)
        {
            if (TryAddSize(GameViewSizeKind.FixedResolution, entry.width, entry.height, entry.label))
            {
                added++;
            }
        }

        return added;
    }

    private enum GameViewSizeKind
    {
        AspectRatio = 0,
        FixedResolution = 1,
    }

    private static bool TryAddSize(GameViewSizeKind kind, int width, int height, string label)
    {
        if (SizeExists(label))
        {
            return false;
        }

        AddCustomSize(kind, width, height, label);
        return true;
    }

    private static bool SizeExists(string label)
    {
        return FindSizeIndex(label) >= 0;
    }

    private static int FindSizeIndex(string label)
    {
        object group = GetCurrentGameViewSizeGroup();
        if (group == null)
        {
            return -1;
        }

        Type groupType = group.GetType();
        MethodInfo getTotalCount = groupType.GetMethod("GetTotalCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        MethodInfo getGameViewSize = groupType.GetMethod("GetGameViewSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (getTotalCount == null || getGameViewSize == null)
        {
            return -1;
        }

        int count = (int)getTotalCount.Invoke(group, null);
        PropertyInfo baseTextProperty = null;

        for (int i = 0; i < count; i++)
        {
            object gameViewSize = getGameViewSize.Invoke(group, new object[] { i });
            if (gameViewSize == null)
            {
                continue;
            }

            baseTextProperty ??= gameViewSize.GetType().GetProperty(
                "baseText",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (baseTextProperty == null)
            {
                return -1;
            }

            string name = (string)baseTextProperty.GetValue(gameViewSize);
            if (name == label)
            {
                return i;
            }
        }

        return -1;
    }

    private static void AddCustomSize(GameViewSizeKind kind, int width, int height, string label)
    {
        Type gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
        Type gameViewSizeEnumType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
        if (gameViewSizeType == null || gameViewSizeEnumType == null)
        {
            Debug.LogWarning("[PortraitPhoneGameViewSizes] Could not resolve Unity GameViewSize types.");
            return;
        }

        ConstructorInfo constructor = gameViewSizeType.GetConstructor(new[]
        {
            gameViewSizeEnumType,
            typeof(int),
            typeof(int),
            typeof(string),
        });

        if (constructor == null)
        {
            Debug.LogWarning("[PortraitPhoneGameViewSizes] Could not resolve Unity GameViewSize constructor.");
            return;
        }

        object customSize = constructor.Invoke(new object[]
        {
            Enum.ToObject(gameViewSizeEnumType, (int)kind),
            width,
            height,
            label,
        });

        object group = GetCurrentGameViewSizeGroup();
        if (group == null)
        {
            return;
        }

        MethodInfo addCustomSize = group.GetType().GetMethod(
            "AddCustomSize",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        addCustomSize?.Invoke(group, new[] { customSize });
    }

    private static object GetCurrentGameViewSizeGroup()
    {
        Type sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        if (sizesType == null)
        {
            return null;
        }

        PropertyInfo instanceProperty = sizesType.GetProperty(
            "instance",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

        object sizesInstance = instanceProperty?.GetValue(null);
        if (sizesInstance == null)
        {
            return null;
        }

        PropertyInfo currentGroupProperty = sizesType.GetProperty(
            "currentGroup",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        return currentGroupProperty?.GetValue(sizesInstance);
    }
}
#endif
