// ======================================================================================
// File         : exTileSheet.cs
// Author       : Wu Jie 
// Last Change  : 09/16/2011 | 10:14:28 AM | Friday,September
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// Tile Sheet Asset
///
///////////////////////////////////////////////////////////////////////////////

public class exTileSheet : ScriptableObject {

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    public Texture2D texture;   ///< tile-sheet build from atlas editor or photoshop
    public Material material;   ///< default material
    public int tileWidth = 32;  ///< the width of a tile grid
    public int tileHeight = 32; ///< the height of a tile grid
    public int padding = 1;     ///< the paddding between two tile grid
    public int row = 0;     ///< the row of the tile sheet
    public int col = 0;     ///< the column of the tile sheet

    // TODO { 
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [System.Serializable]
    public class Element {
        public GameObject prefab;
    }
    public List<Element> elements = new List<Element>();
    // } TODO end 

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// \param _index the index
    /// \param _col the out column  
    /// \param _row the out row  
    // ------------------------------------------------------------------ 

    public void GetColRow ( int _index, out int _col, out int _row ) {
        _row = Mathf.CeilToInt(_index / col);
        _col = _index - _row * col;
    }

    // ------------------------------------------------------------------ 
    /// \param _index the id of the tile grid
    /// \return the uv coordinate
    /// get the uv coordinate of the tile grid
    // ------------------------------------------------------------------ 

    public Rect GetTileUV ( int _index ) {
        int curCol, curRow;
        GetColRow ( _index, out curCol, out curRow );
        return GetTileUV ( curCol, curRow );
    }

    // ------------------------------------------------------------------ 
    /// \param _col the col of the tile grid
    /// \param _row the row of the tile grid
    /// \return the uv coordinate
    /// get the uv coordinate of the tile grid
    // ------------------------------------------------------------------ 

    public Rect GetTileUV ( int _col, int _row ) {
        if ( _col >= col || _row >= row )
            return new Rect ( 0.0f, 0.0f, 1.0f, 1.0f );

        int xStart = (tileWidth + padding) * _col + padding;
        int yStart = (texture.height - (tileHeight + padding) * (_row + 1));
        int xEnd = xStart + tileWidth; 
        int yEnd = yStart + tileHeight; 

        return new Rect ( (float)xStart / texture.width, 
                          (float)yStart / texture.height, 
                          (float)(xEnd - xStart) / texture.width,
                          (float)(yEnd - yStart) / texture.height );
    }
}

