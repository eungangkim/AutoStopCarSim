using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PoseCopyTool : EditorWindow
{
    private GameObject sourceCharacter;
    private GameObject targetCharacter;

    [MenuItem("Tools/Pedestrian/Copy Pose By Normalized Bone Name")]
    public static void ShowWindow()
    {
        GetWindow<PoseCopyTool>("Copy Pose");
    }

    private void OnGUI()
    {
        GUILayout.Label("Copy Pose By Normalized Bone Name", EditorStyles.boldLabel);

        sourceCharacter = (GameObject)EditorGUILayout.ObjectField(
            "Source Character",
            sourceCharacter,
            typeof(GameObject),
            true
        );

        targetCharacter = (GameObject)EditorGUILayout.ObjectField(
            "Target Character",
            targetCharacter,
            typeof(GameObject),
            true
        );

        if (GUILayout.Button("Copy Local Rotations"))
        {
            CopyPose();
        }
    }

    private void CopyPose()
    {
        if (sourceCharacter == null || targetCharacter == null)
        {
            Debug.LogWarning("Source Character와 Target Character를 모두 넣어주세요.");
            return;
        }

        Transform[] sourceBones = sourceCharacter.GetComponentsInChildren<Transform>();
        Transform[] targetBones = targetCharacter.GetComponentsInChildren<Transform>();

        Dictionary<string, Transform> targetBoneMap = new Dictionary<string, Transform>();

        foreach (Transform targetBone in targetBones)
        {
            string normalizedName = NormalizeBoneName(targetBone.name);

            if (!targetBoneMap.ContainsKey(normalizedName))
            {
                targetBoneMap.Add(normalizedName, targetBone);
            }
        }

        int copiedCount = 0;

        foreach (Transform sourceBone in sourceBones)
        {
            string normalizedName = NormalizeBoneName(sourceBone.name);

            if (targetBoneMap.TryGetValue(normalizedName, out Transform targetBone))
            {
                Undo.RecordObject(targetBone, "Copy Bone Rotation");

                targetBone.localRotation = sourceBone.localRotation;

                copiedCount++;
            }
        }

        Debug.Log($"Pose 복사 완료: {copiedCount}개 bone localRotation 복사됨");
    }

    private string NormalizeBoneName(string boneName)
    {
        // 예: mixamorig8:Hips -> Hips
        int colonIndex = boneName.IndexOf(':');

        if (colonIndex >= 0 && colonIndex < boneName.Length - 1)
        {
            return boneName.Substring(colonIndex + 1);
        }

        return boneName;
    }
}