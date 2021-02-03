using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(_Template.SkillTemplate))]
public class SkillEditor : Editor {
    private bool show = true;
    public override void OnInspectorGUI(){
        serializedObject.Update();
        base.OnInspectorGUI();
        var template = (_Template.SkillTemplate) target;
        EditorGUILayout.Space();

        show = EditorGUILayout.Foldout(show, "Tiers");
        if(show){
            int total = (template.effects.Length + template.modifiers.Length);
            if(template.tierMatrix.Length < total * template.count) Array.Resize(ref template.tierMatrix, total * template.count);
            for(int r = 0; r < template.effects.Length; r++){
                EditorGUILayout.BeginHorizontal();
                _Template.ActionTemplate effect = template.effects[r];
                EditorGUILayout.LabelField($"{r}) ", GUILayout.Width(EditorGUIUtility.labelWidth));
                for(int c = 0; c < template.count; c++){
                    if(effect == null) continue;
                    int index = c + r * template.count;
                    template.tierMatrix[index] = EditorGUILayout.IntField(template.tierMatrix[index], GUILayout.Width(20));
                    template.tierMatrix[index] = Math.Min(effect.TierCount, Math.Max(0, template.tierMatrix[index]));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            for(int i = 0; i < template.modifiers.Length; i++){
                EditorGUILayout.BeginHorizontal();
                _Template.ActionModifierTemplate modifier = template.modifiers[i];
                int r = template.effects.Length + i;
                EditorGUILayout.LabelField($"{r}) ", GUILayout.Width(EditorGUIUtility.labelWidth));
                for(int c = 0; c < template.count; c++){
                    if(modifier == null) continue;
                    int index = c + r * template.count;
                    template.tierMatrix[index] = EditorGUILayout.IntField(template.tierMatrix[index], GUILayout.Width(20));
                    template.tierMatrix[index] = Math.Min(modifier.TierCount, Math.Max(0, template.tierMatrix[index]));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField($"Flow Matrix", GUILayout.Width(EditorGUIUtility.labelWidth));
            //TODO add support for cycles?
            if(template.modifierMatrix.Length < (template.modifiers.Length + 1) * total)
            Array.Resize(ref template.modifierMatrix, (template.modifiers.Length + 1) * total);
            for(int r = 0; r < total; r++){
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField ($"{r}) ", GUILayout.Width(40));
                for(int c = 0; c <= template.modifiers.Length; c++){
                    int index = r + c * total;
                    if(c <= total - r - 1){
                    template.modifierMatrix[index] = (_Template.TargetType) EditorGUILayout.EnumFlagsField(template.modifierMatrix[index], GUILayout.Width(50));
                    //template.modifierMatrix[index] = EditorGUILayout.Toggle(template.modifierMatrix[index], GUILayout.Width(20));
                    }else{
                    template.modifierMatrix[index] = _Template.TargetType.None;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        if(GUI.changed) EditorUtility.SetDirty(target);
        serializedObject.ApplyModifiedProperties();
    }
}