// ======================================================================================
// File         : exBitmapFontEditor.cs
// Author       : Wu Jie 
// Last Change  : 07/15/2011 | 13:50:38 PM | Friday,July
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

partial class exBitmapFontEditor : EditorWindow {

    ///////////////////////////////////////////////////////////////////////////////
    // private variables
    ///////////////////////////////////////////////////////////////////////////////

    private exBitmapFont curEdit;
    private Object curFontInfo;

    private string newPath = "Assets/";
    private string newName = "New BitmapFont";

    private Vector2 scrollPos = Vector2.zero;

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("Window/ex2D/BitmapFont Editor %&f")]
    public static exBitmapFontEditor NewWindow () {
        exBitmapFontEditor newWindow = EditorWindow.GetWindow<exBitmapFontEditor>();
        return newWindow;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        name = "BitmapFont Editor";
        wantsMouseMove = true;
        autoRepaintOnSceneChange = true;
        // position = new Rect ( 50, 50, 800, 600 );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Init () {
        curFontInfo = null;
        if ( curEdit ) {
            string path = AssetDatabase.GetAssetPath(curEdit);
            string fontInfoPath = Path.Combine ( Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".txt" ); 
            curFontInfo = AssetDatabase.LoadAssetAtPath( fontInfoPath, typeof(Object) );
            if ( curFontInfo == null ) {
                fontInfoPath = Path.Combine ( Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".fnt" ); 
                curFontInfo = AssetDatabase.LoadAssetAtPath( fontInfoPath, typeof(Object) );
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Edit ( Object _obj ) {
        // check if repaint
        if ( curEdit != _obj ) {

            // check if we have exBitmapFont in the same directory
            Object obj = _obj; 
            if ( obj != null ) {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if ( string.IsNullOrEmpty(assetPath) == false ) {
                    string dirname = Path.GetDirectoryName(assetPath);
                    string filename = Path.GetFileNameWithoutExtension(assetPath);
                    obj = (exBitmapFont)AssetDatabase.LoadAssetAtPath( Path.Combine( dirname, filename ) + ".asset",
                                                                       typeof(exBitmapFont) );
                }
                if ( obj == null ) {
                    obj = _obj;
                }
            }

            // if this is another bitmapfont, swtich to it.
            if ( obj is exBitmapFont && obj != curEdit ) {
                curEdit = obj as exBitmapFont;
                Init();

                Repaint ();
                return;
            }
        }
    }

    // DISABLE: the focus only occur when main window lost foucs, then come in { 
    // // ------------------------------------------------------------------ 
    // // Desc: 
    // // ------------------------------------------------------------------ 

    // void OnFocus () {
    //     OnSelectionChange ();
    // }
    // } DISABLE end 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnSelectionChange () {
        Edit (Selection.activeObject);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnGUI () {
        if ( curEdit == null ) {
            GUILayout.Space(10);
            GUILayout.Label ( "Please select an BitmapFont asset" );
            return;
        }

        // ======================================================== 
        // if we have curEdit
        // ======================================================== 

        scrollPos = EditorGUILayout.BeginScrollView ( scrollPos, 
                                                      GUILayout.Width(position.width),
                                                      GUILayout.Height(position.height) );

        // draw label
        GUILayout.Space(10);
        GUILayout.Label ( AssetDatabase.GetAssetPath(curEdit) );

        // ======================================================== 
        // font info 
        // ======================================================== 

        Object newFontInfo = EditorGUILayout.ObjectField( "Font Info"
                                                          , curFontInfo
                                                          , typeof(Object)
#if !UNITY_3_0 && !UNITY_3_1 && !UNITY_3_3
                                                          , false
#endif
                                                          , GUILayout.Width(300) 
                                                        );
        if ( newFontInfo != curFontInfo ) {
            curFontInfo = newFontInfo;
            curEdit.editorNeedRebuild = true;
        }

        // ======================================================== 
        // page info
        // ======================================================== 

        GUI.enabled = false;
        foreach ( exBitmapFont.PageInfo pi in curEdit.pageInfos ) {
            EditorGUILayout.ObjectField( pi.texture.name
                                         , pi.texture
                                         , typeof(Texture2D)
#if !UNITY_3_0 && !UNITY_3_1 && !UNITY_3_3
                                         , false
#endif
                                         , GUILayout.Width(50)
                                         , GUILayout.Height(50) 
                                       );
        }
        GUI.enabled = true;

        // ======================================================== 
        // Build 
        // ======================================================== 

        GUI.enabled = curEdit.editorNeedRebuild;
        if ( GUILayout.Button("Build", GUILayout.MaxWidth(100) ) ) {
            curEdit.Build( curFontInfo );
        }
        GUI.enabled = true;

        EditorGUILayout.EndScrollView();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void CreateNewBitmapFont ( Object _fontInfo ) {
        // create atlas info
        EditorUtility.DisplayProgressBar( "Creating BitmapFont...",
                                          "Creating BitmapFont Asset...",
                                          0.1f );    

        // check if there have 
        if ( curEdit == null ) {
            curEdit = exBitmapFontUtility.Create( newPath, newName );
        }

        // check if we have the texture and textasset with the same name of bitmapfont 
        EditorUtility.DisplayProgressBar( "Creating BitmapFont...",
                                          "Check building ...",
                                          0.2f );    

        // if we have enough information, try to build the exBitmapFont asset
        curEdit.Build ( _fontInfo );

        EditorUtility.ClearProgressBar();    
    }
}
