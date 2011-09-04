// ======================================================================================
// File         : exPostProcessor.cs
// Author       : Wu Jie 
// Last Change  : 08/06/2011 | 22:07:41 PM | Saturday,August
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
// class ex2D_PostProcessor 
// 
// Purpose: 
// 
///////////////////////////////////////////////////////////////////////////////

class ex2D_PostProcessor : AssetPostprocessor {

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static void OnPostprocessAllAssets ( string[] _importedAssets,
                                         string[] _deletedAssets,
                                         string[] _movedAssets,
                                         string[] _movedFromAssetPaths ) 
    {
        foreach ( string path in _importedAssets ) {
            // check if we are .ex2D_AtlasDB or .ex2D_SpriteAnimationDB
            if ( string.Equals(path, exAtlasDB.dbPath, System.StringComparison.CurrentCultureIgnoreCase) || 
                 string.Equals(path, exSpriteAnimationDB.dbPath, System.StringComparison.CurrentCultureIgnoreCase) )
            {
                continue;
            }

            // check if we are asset
            if ( Path.GetExtension(path) != ".asset" )
                continue;

            //
            Object obj = (Object)AssetDatabase.LoadAssetAtPath ( path, typeof(Object) );
            if ( obj == null )
                continue;

            // ======================================================== 
            // exAtlasInfo
            // ======================================================== 

            if ( obj is exAtlasInfo ) {
                exAtlasInfo atlasInfo = obj as exAtlasInfo;
                exAtlasDB.AddAtlasInfo(atlasInfo);
            }

            // ======================================================== 
            // exSpriteAnimClip
            // ======================================================== 

            if ( obj is exSpriteAnimClip ) {
                exSpriteAnimClip spAnimClip = obj as exSpriteAnimClip;
                exSpriteAnimationDB.AddSpriteAnimClip(spAnimClip);
            }
        }

        //
        List<string> atlasInfoGUIDs = new List<string>();
        foreach ( string path in _deletedAssets ) {
            // check if we are .ex2D_AtlasDB or .ex2D_SpriteAnimationDB
            if ( string.Equals(path, exAtlasDB.dbPath, System.StringComparison.CurrentCultureIgnoreCase) || 
                 string.Equals(path, exSpriteAnimationDB.dbPath, System.StringComparison.CurrentCultureIgnoreCase) )
            {
                continue;
            }

            // check if we are asset
            if ( Path.GetExtension(path) != ".asset" )
                continue;

            // 
            string guid = AssetDatabase.AssetPathToGUID(path);

            // check if we have the guid in the exAtlasInfo
            if ( exAtlasDB.HasAtlasInfoGUID( guid ) ) {
                exAtlasDB.RemoveAtlasInfo(guid);
                atlasInfoGUIDs.Add(guid);
            }
            // check if we have the guid in the exSpriteAnimClip
            else if ( exSpriteAnimationDB.HasSpriteAnimClipGUID( guid ) ) {
                exSpriteAnimationDB.RemoveSpriteAnimClip(guid);
            }
        }
        exSceneHelper.UpdateSceneSprites(atlasInfoGUIDs);

        // DISABLE { 
        // for ( int i = 0; i < _movedAssets.Length; ++i )
        //     Debug.Log("Moved Asset: " + _movedAssets[i] + " from: " + _movedFromAssetPaths[i]);
        // } DISABLE end 
    }
}

///////////////////////////////////////////////////////////////////////////////
// class ex2D_SaveAssetsProcessor 
// 
// Purpose: 
// 
///////////////////////////////////////////////////////////////////////////////

class ex2D_SaveAssetsProcessor : SaveAssetsProcessor {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static void OnWillSaveAssets ( string[] _paths ) {
        List<exAtlasInfo> rebuildAtlasInfos = new List<exAtlasInfo>();
        List<exSpriteAnimClip> rebuildSpriteAnimClips = new List<exSpriteAnimClip>();

        //
        foreach ( string path in _paths ) {
            // check if we are .ex2D_AtlasDB or .ex2D_SpriteAnimationDB
            if ( string.Equals(path, exAtlasDB.dbPath, System.StringComparison.CurrentCultureIgnoreCase) || 
                 string.Equals(path, exSpriteAnimationDB.dbPath, System.StringComparison.CurrentCultureIgnoreCase) )
            {
                continue;
            }

            // check if we are asset
            if ( Path.GetExtension(path) != ".asset" )
                continue;

            Object obj = (Object)AssetDatabase.LoadAssetAtPath ( path, typeof(Object) );
            if ( obj == null )
                continue;

            // ======================================================== 
            // build exAtlasInfo 
            // ======================================================== 

            if ( obj is exAtlasInfo ) {
                exAtlasInfo atlasInfo = obj as exAtlasInfo;
                if ( atlasInfo.needRebuild ) {
                    rebuildAtlasInfos.Add(atlasInfo);
                }
            }

            // ======================================================== 
            // build exSpriteAnimClip 
            // ======================================================== 

            if ( obj is exSpriteAnimClip ) {
                exSpriteAnimClip spAnimClip = obj as exSpriteAnimClip;
                if ( spAnimClip.editorNeedRebuild )
                    rebuildSpriteAnimClips.Add(spAnimClip);
            }

            // TODO { 
            // // ======================================================== 
            // // build exBitmapFont 
            // // ======================================================== 

            // if ( obj is exBitmapFont ) {
            //     exBitmapFont bitmapFont = obj as exBitmapFont;
            //     if ( bitmapFont.editorNeedRebuild )
            //         Object fontInfo = exEditorHelper.LoadAssetFromGUID(bitmapFont.fontinfoGUID);
            //         bitmapFont.Build( fontInfo );
            // }
            // } TODO end 
        }

        // NOTE: we need to make sure exAtlasInfo build before exSpriteAnimClip,
        //       because during build, exAtlasDB will update ElementInfo, and exSpriteAnimClip need this for checking error. 

        // ======================================================== 
        // build exAtlasInfo first
        // ======================================================== 

        foreach ( exAtlasInfo atlasInfo in rebuildAtlasInfos ) {
            exAtlasInfoUtility.Build(atlasInfo);
            // build sprite animclip that used this atlasInfo
            exAtlasInfoUtility.BuildSpAnimClipsFromRebuildList(atlasInfo);
        }

        // ======================================================== 
        // build exSpriteAnimClip 
        // ======================================================== 

        foreach ( exSpriteAnimClip spAnimClip in rebuildSpriteAnimClips ) {
            spAnimClip.Build();
        }

        // ======================================================== 
        // post build 
        // ======================================================== 

        List<string> rebuildAtlasInfoGUIDs = new List<string>();
        foreach ( exAtlasInfo atlasInfo in rebuildAtlasInfos ) {
            rebuildAtlasInfoGUIDs.Add( exEditorHelper.AssetToGUID(atlasInfo) );
        }
        exSceneHelper.UpdateSceneSprites (rebuildAtlasInfoGUIDs);

        // NOTE: without this you will got leaks message
        EditorUtility.UnloadUnusedAssets();
    }
}