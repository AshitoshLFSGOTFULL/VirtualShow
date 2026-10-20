/*
using System.Runtime;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PDFReader))]
public class PDFReaderEditor : Editor
{
   public int index = 0;
   public override void OnInspectorGUI()
   {
        base.OnInspectorGUI();

        PDFReader l_pdfReader = (PDFReader)target;

        EditorGUILayout.LabelField("Index", index.ToString());

        if (GUILayout.Button("Load Pages"))
        {
            l_pdfReader.ReadPDF();
        }
        if(GUILayout.Button("+"))
        {
            if(index < l_pdfReader.GetTotalPages())
            {
                Debug.Log("print +");
                index++;
                l_pdfReader.ReadPages(index);
            }
        }

        if (GUILayout.Button("-"))
        {
            if(index > 0)
            {
                Debug.Log("print -");
                index--;
                l_pdfReader.ReadPages(index);
            }
        }

        if (GUILayout.Button("Reset Index"))
        {
            Debug.Log("Reset");
            index = 0;
            l_pdfReader.ReadPages(index);
        }

    }
}
*/
