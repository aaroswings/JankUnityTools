using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using static UnityEditor.MenuItem;
//using UnityEditor.Animation;

public static class JankTools
{
    /* 
    ===========================
    FLIP ANIMATION CLIPS 
    ===========================
    */
    [MenuItem("Assets/JankTools/Create Flipped Clip", false, 14)]
    private static void FlipClips()
    {
        List<AnimationClip> clips = GetSelectedClips();
        if (clips != null && clips.Count > 0)
        {
            foreach (AnimationClip clip in clips)
            {
                FlipClip(clip);
            }
        }
    }

    public static List<AnimationClip> GetSelectedClips()
    {
        var clips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Assets);
        List<AnimationClip> animClips = new List<AnimationClip>();
        if (clips.Length > 0)
        {
            foreach (var clip in clips)
            {
                animClips.Add(clip as AnimationClip);
            }
            return animClips;
        }
        return null;
    }

    private static void FlipClip(AnimationClip clip)
    {
        if (clip == null)
            return;

        // Figure out the file path of the new animation clip
        string directoryPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(clip));
        string fileName = Path.GetFileName(AssetDatabase.GetAssetPath(clip)).Split('.')[0];
        string fileExtension = Path.GetExtension(AssetDatabase.GetAssetPath(clip));

        string outFileName = fileName;

        if (fileName.EndsWith("_R"))
            outFileName = fileName.Substring(0, outFileName.Length - 2) + "_L";
        else if (fileName.EndsWith("_L"))
            outFileName = fileName.Substring(0, outFileName.Length - 2) + "_R";
        else
            outFileName = outFileName + "_flipped";

        string newFilePath = directoryPath + Path.DirectorySeparatorChar + outFileName + fileExtension;

        AnimationClip newClip = new AnimationClip();

        // Loop over all curves in original clip, flip _R to _L in paths and flip euler rotation angles for y/z
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
        {
            string[] pathTokens = binding.path.Split('/');

            for (int i = 0; i < pathTokens.Length; i++)
            {
                if (pathTokens[i].EndsWith("_L"))
                    pathTokens[i] = pathTokens[i].Substring(0, pathTokens[i].Length - 2) + "_R";
                else if (pathTokens[i].EndsWith("_R"))
                    pathTokens[i] = pathTokens[i].Substring(0, pathTokens[i].Length - 2) + "_L";
            }

            string newPath = string.Join("/", pathTokens);
            var animationCurve = AnimationUtility.GetEditorCurve(clip, binding);

            Keyframe[] keyframes = animationCurve.keys;
            for (int i = 0; i < keyframes.Length; i++)
            {
                if (binding.propertyName == "localEulerAnglesRaw.y")
                    keyframes[i].value = -keyframes[i].value;
                if (binding.propertyName == "localEulerAnglesRaw.z")
                    keyframes[i].value = -keyframes[i].value;
            }
            newClip.SetCurve(newPath, binding.type, binding.propertyName, new AnimationCurve(keyframes));
        }

        AssetDatabase.CreateAsset(newClip, newFilePath);
        AssetDatabase.SaveAssets();
    }
}
