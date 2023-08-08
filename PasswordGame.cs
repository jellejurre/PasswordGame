using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class PasswordGame : EditorWindow
{
    private List<Func<(bool, string)>> requirements;

    #region ObjectShit
    public static (bool, string) HasGameObjectNamedHelloInScene()
    {
        GameObject[] obj =  GameObject.FindObjectsOfType<GameObject>();
        foreach (var gameObject in obj)
        {
            if (gameObject.name == "Hello")
            {
                return (true, "Have a GameObject in your scene named \"Hello\"");
            }
        }

        return (false, "Have a GameObject in your scene named \"Hello\"");
    }

    public static (bool, string) HasGameObjectWithMissingScriptInScene()
    {
        GameObject[] obj =  GameObject.FindObjectsOfType<GameObject>();
        foreach (var gameObject in obj)
        {
            
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject) > 0)
            {
                return (true, "Have a GameObject in your scene with a missing script");
            }
        }

        return (false, "Have a GameObject in your scene with a missing script");
    }

    public static (bool, string) HaveAGameObjectInAWeirdPosition()
    {
        GameObject[] obj =  GameObject.FindObjectsOfType<GameObject>();
        (bool xpassed, bool ypassed, bool zpassed) = (false, false, false);
        foreach (var gameObject in obj)
        {
            if (gameObject.transform.position.x == obj.Length)
            {
                xpassed = true;
                if (gameObject.transform.position.y == DateTime.Now.DayOfYear)
                {
                    ypassed = true;
                    
                    if (Vector3.Dot(gameObject.transform.localPosition, Vector3.one) == 17)
                    {
                        return (true, "Have an object in your scene with a weird position");
                    }
                }
            }
        }

        if (!xpassed)
        {
            return (false, "Have a GameObject in your scene with its global x position being the amount of objects in your scene");
        }

        if (!ypassed)
        {
            return (false, "Have a GameObject in your scene with its global x position being the amount of objects in your scene AND" 
                + " its global y position being the current day in the year");

        }
        return (false, "Have a GameObject in your scene with its global x position being the amount of objects in your scene AND" 
                       + " its global y position being the current day in the year"
                       + " AND all its local position x y and z add up to 17");
    }

    public static (bool, string) HaveABlueObject()
    {
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();
        foreach (var meshRenderer in renderers)
        {
            if (meshRenderer.sharedMaterial == null)
            {
                continue;
            }
            if (meshRenderer.sharedMaterial.color.b > 0.8
                && meshRenderer.sharedMaterial.color.g < 0.2
                && meshRenderer.sharedMaterial.color.r < 0.2)
            {
                return (true, "You're blue da bo dee da bo dai");
            }
        }

        if (renderers.Length > 0)
        {
            return (false, "You have a thing, but it's not blue (i'm a dumb script, use default material)");
        }

        return (false, "Your don't have any blue in your scene :(");
    }

    public static (bool, string) HaveAtMaxTwoChildren()
    {
        int topLevelObjects = 0;
        GameObject[] objs = FindObjectsOfType<GameObject>();
        foreach (var obj in objs)
        {
            if (obj.transform.parent == null)
            {
                topLevelObjects++;
            }

            if (topLevelObjects > 2)
            {
                return (false, "Only 2 root objects in scene view");
            }
            if(obj.transform.childCount > 2)
            {
                return (false, "GameObject can have at max 2 children");
            }
        }

        return (true, "Nice and neat :)");
    }
    
    private static bool isJotaroGettingCloserCheck = false;
    private static Vector3 jotaroPosition;
    private static Vector3 dioPosition;
    private static float lastdistance;
    private static DateTime lastTimeGotCloser;
    
    public static (bool, string) IsJotaroGettingCloser()
    {
        var jotaro = GameObject.Find("Jotaro");
        var dio = GameObject.Find("Dio");

        if (jotaro == null || dio == null)
        {
            return (false, "Jotaro and Dio aren't in the scene fighting");
        }
        
        if (jotaro.transform.parent != dio.transform.parent)
        {
            return (false, "Jotaro and Dio aren't inside the same object to fight");
        }
        
        jotaroPosition = jotaro.transform.position;
        dioPosition = dio.transform.position;
        
        var distance = Vector3.Distance(jotaroPosition, dioPosition);
        if (distance < lastdistance)
        {
            isJotaroGettingCloserCheck = true;
            lastTimeGotCloser = DateTime.Now;
        }
        else if (Math.Abs(distance - lastdistance) > 0.1f)
        {
            isJotaroGettingCloserCheck = false;
        }
        else
        {
            if (isJotaroGettingCloserCheck)
            {
                if (DateTime.Now - lastTimeGotCloser < TimeSpan.FromSeconds(120))
                {
                    isJotaroGettingCloserCheck = true;
                }
                else
                {
                    isJotaroGettingCloserCheck = false;
                }
            }
        }
        
        lastdistance = distance;

        return (isJotaroGettingCloserCheck, isJotaroGettingCloserCheck ? "Oh? You're Approaching Me?" : "Jotaro can't beat the shit out of Dio if he doesn't get closer");

    }

    public static (bool, string) PleaseCheckMe()
    {
        if (Selection.activeObject is GameObject obj)
        {
            if (obj.name == "Please Check My Scene")
            {
                return (true, "You were kind enough");
            }
        }

        return (false, "Have an object selected called \"Please Check My Scene\"");
    }
    
    #endregion

    #region WindowShit

    private static (bool, string) WindowFitsAspectRatio(float initwidth, float initheight, float width, float height) 
    {
        string message = $"Game fits the aspect ratio {width}:{height}.";
        bool passed = Math.Abs((initwidth/initheight) - (width/height)) < 0.01f;
        return (passed, message);
    }

    private static float minimizeEndTime;
    private static (bool, string) WasMinimizedInTheLastTwoMinutes()
    {
        if (!UnityEditorInternal.InternalEditorUtility.isApplicationActive)
        {
            minimizeEndTime = Time.realtimeSinceStartup + 120;
        }
    
        string message = $"Minimized project in the last two minutes.";
        bool passed = Time.realtimeSinceStartup < minimizeEndTime;
    
        return (passed, message);
    }


    private static GUIContent flashIcon = null;
    private static (bool, string) HasMoreThanFiveErrors()
    {
        flashIcon = EditorGUIUtility.IconContent("d_BuildSettings.FlashPlayer");
        bool passed = GetErrorCount() > 7;
        Color og = GUI.contentColor;
        GUI.contentColor = passed ? Color.green : Color.red;
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Has", GUILayout.ExpandWidth(false));
        GUILayout.Label(flashIcon, GUILayout.ExpandWidth(false), GUILayout.Height(18));
        var lastRect = GUILayoutUtility.GetLastRect();
        if (lastRect.Contains(Event.current.mousePosition))
        {
            throw new Exception("Flash is not supported!");
        }
        GUILayout.Label($"more than 7 errors.", GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUI.contentColor = og;
        return (passed, "");
    }

    private static (bool, string) HasLessThanTenErrors()
    {
        string message = $"Has less than 10 errors.";
        bool passed = GetErrorCount() < 10;
        return (passed, message);
    }

    private static int GetErrorCount()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(AssetDatabase));
        Type logEntries = assembly.GetType("UnityEditor.LogEntries");
        int count = (int)logEntries.GetMethod("GetCount").Invoke(null, null);
        return count;
    }

    static bool IsVRLabsNamespaceLoaded = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith("VRLabs")); 
    
    public static (bool, string) MustHaveCodeFromVRLabs()
    {
        return (IsVRLabsNamespaceLoaded, "You should use some VRLabs stuff, like The Avatars 3.0 Manager! Or the Marker! or... you get it");
    }

    private static Editor distractionPreview;
    private static AnimationClip distractionClip;
    private static (bool, string) MakeHimVibe()
    {
        if (distractionPreview == null)
        {
            distractionClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Distraction.anim");
            if (distractionClip == null)
            {
                distractionClip = AssetDatabase
                    .FindAssets("t:AnimationClip")
                    .Select(x => AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(x)))
                    .FirstOrDefault(x => x.name == "Distraction");
            }
            distractionPreview = Editor.CreateEditor(distractionClip);
        }

        distractionPreview.HasPreviewGUI();
        distractionPreview.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256,256), EditorStyles.whiteLabel);
        var avatarPreview = distractionPreview.GetType().GetField("m_AvatarPreview", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(distractionPreview);
        var timeControl = avatarPreview.GetType().GetField("timeControl", BindingFlags.Public | BindingFlags.Instance).GetValue(avatarPreview);
        var isPlaying = (bool)timeControl.GetType().GetProperty("playing", BindingFlags.Public | BindingFlags.Instance).GetValue(timeControl);
    
        bool passed = isPlaying;
        string message = passed ? "He's vibing :)": "He's not vibing :(";
        return (passed, message);
    }
    
    // public static (bool, string) LayoutNeedsToBeTwoByThree()
    // {
    //     // LayoutUtility.
    // }
    //
    // public (bool, string) HasFloatingWindows()
    // {
    //     int floatingWindows = 0;
    //     bool passedDockedRatio = false;
    //
    //     EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
    //     foreach (var window in windows)
    //     {
    //         if (!window.IsDocked()) floatingWindows++;
    //         if (window.IsDocked() && window.position.width > 1500) passedDockedRatio = true;
    //     }
    // }
    #endregion

    #region LightShit

    public static (bool, string) MustPraiseTheSun()
    {
        var sceneLights = GameObject.FindObjectsOfType<Light>();

        Light selectedLight = null;
        foreach (var light in sceneLights)
        {
            if (light.type == LightType.Directional && light.isActiveAndEnabled &&
                light.intensity > 0.5f && light.color.r + light.color.g + light.color.b > 1.5f)
            {
                selectedLight = light;
                break;
            }
        }
        
        if (selectedLight == null)
            return (false, "There is no bright sun to praise");
        
        var rotator = selectedLight.gameObject.GetComponent<Rotator>();
        if (rotator == null)
        {
            selectedLight.gameObject.AddComponent<Rotator>();
        }
        
        GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();

        foreach (var obj in objects)
        {
            Vector3 directionToLight = selectedLight.transform.forward;
            Vector3 objectDirection = obj.transform.forward;
            
            if(obj == selectedLight.gameObject)
                continue;
            if (Vector3.Dot(directionToLight, objectDirection) < -0.7f)
            {
                return (true, "Praise the sun!");
            }
        
        }
            
        return (false, "No GameObject is praising the sun (with its face)");
    }
    
    
    #endregion
    
    [MenuItem("PasswordGame/Password Game")]
    public static void ShowExample()
    {
        PasswordGame wnd = GetWindow<PasswordGame>();
        wnd.titleContent = new GUIContent("PasswordGame");
        
        wnd.requirements = new List<Func<(bool, string)>>()
        {
            HasGameObjectNamedHelloInScene,
            HasGameObjectWithMissingScriptInScene,
            () => WindowFitsAspectRatio((int)wnd.position.width, (int)wnd.position.height, 16, 9),
            HaveAGameObjectInAWeirdPosition,
            WasMinimizedInTheLastTwoMinutes,
            MakeHimVibe,
            MustPraiseTheSun,
            MustHaveCodeFromVRLabs,
            PleaseCheckMe,
            HaveABlueObject,
            HasMoreThanFiveErrors,
            HaveAtMaxTwoChildren,
            HasLessThanTenErrors,
            IsJotaroGettingCloser,
        };
        
    }

    public class Rotator : MonoBehaviour
    {
        private Vector3 rotationDirection;
        private float rotationSpeed;
        private DateTime lastTimeDirectionChanged = DateTime.Now;

        void Update()
        {
            if(DateTime.Now - lastTimeDirectionChanged > TimeSpan.FromSeconds(20))
            {
                rotationDirection = UnityEngine.Random.insideUnitSphere.normalized;
                lastTimeDirectionChanged = DateTime.Now;
            }
        
            transform.Rotate(rotationDirection * (0.01f * Time.deltaTime));
        }
    }
    
    private void OnGUI()
    {
        Repaint();
        if (requirements == null)
        {
            Close();
            ShowExample();
        }
        for (var i = 0; i < requirements.Count; i++)
        {
            var (passed, status) = requirements[i]();
            if (status == "")
            {
                if (passed)
                {
                    continue;
                }
                break;
            }
            GUI.color = passed ? Color.green : Color.red;
            GUILayout.Label(status);
            GUI.color = Color.white;
            if (!passed)
            {
                break;
            }
        }
    }
    
    
}
