using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using _Template;

public class LocationEditor : EditorWindow {
    [UnityEditor.Callbacks.OnOpenAsset(1)]
    private static bool Callback(int instanceID, int line){
        var template = EditorUtility.InstanceIDToObject(instanceID) as LocationTemplate;
        if(template == null) return false;
        OpenWindow(template);
        return true;
    }
    public static void OpenWindow(LocationTemplate template){
        var window = EditorWindow.GetWindow<LocationEditor>("Location Editor", Type.GetType("UnityEditor.SceneView,UnityEditor"));
        window.template = template;
        window.Show();
    }
    private LocationTemplate template;
    void OnSelectionChange(){Close();}
    public void OnInspectorUpdate(){Repaint();}
    public void OnGUI(){
        if(template == null) return;
        int columns = template.columns;
        int rows = template.rows;
        Handles.BeginGUI();
        float size = 0.8f * Mathf.Min(position.width, position.height) / Mathf.Max(columns, rows);
        Handles.color = new Color(1,1,1,1);
        Vector3 pan = new Vector3(0.5f * position.width, 0.5f * position.height, 0);
        pan -= new Vector3(0.5f * size * columns, 0.5f * size * rows, 0);
        for(int c = 0; c <= columns; c++)
            Handles.DrawLine(new Vector3(c * size, 0, 0)+pan, new Vector3(c * size, rows * size, 0)+pan);
        for(int r = 0; r <= rows; r++)
            Handles.DrawLine(new Vector3(0, r * size, 0)+pan, new Vector3(size * columns, r * size, 0)+pan);
        Handles.EndGUI();

        if(template.units == null) template.units = new UnitTemplate[0];
        if(template.units.Length < columns * rows) Array.Resize(ref template.units, columns * rows);
        for(int c = 0; c < columns; c++)
        for(int r = 0; r < rows; r++){
            int index = r + c * rows;
            template.units[index] = EditorGUI.ObjectField(
                new Rect(pan.x + c * size, pan.y + (rows - 1 - r) * size, size, size),
                template.units[index], typeof(UnitTemplate), false
            ) as UnitTemplate;
        }
        EditorUtility.SetDirty(template);
    }
}