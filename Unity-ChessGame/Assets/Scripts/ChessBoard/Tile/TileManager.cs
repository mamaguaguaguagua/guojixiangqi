using System.Collections.Generic;
using System.Linq;
using ChessModel;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    //数组存储所有棋盘格脚本脚本
    private TileScript[] tileList;
    //引入boardManger用于通信
    private BoardManager boardManager;

    private void Awake()
    {
        tileList = gameObject.GetComponentsInChildren<TileScript>();
        boardManager = gameObject.GetComponentInParent<BoardManager>();
    }
    #region 交互处理
    /// <summary>
    /// 点击瓦片方法
    /// </summary>
    /// <param name="tilePlacement">位置索引</param>
    public void clickTile(int tilePlacement)
    {
        boardManager.ClickTile(tilePlacement);
    }
    /// <summary>
    /// 瓦片正中心的位置
    /// </summary>
    /// <param name="position">位置索引</param>
    /// <returns></returns>
    public Vector3 getCoordinatesByTilePlacement(int position)
    {
        Vector3 tileCoord = getTile(position).transform.position;
        tileCoord.x += (int)(5 * transform.localScale.x);
        tileCoord.y += (int)(5 * transform.localScale.y);
        tileCoord.z += (int)(5 * transform.localScale.z);
        return tileCoord;
    }
    /// <summary>
    /// 更新高亮方法，显示所有合法高亮的提示格
    /// </summary>
    /// <param name="moves"></param>
    public void updateLegalMoves(List<Move> moves)
    {
        foreach (var tile in tileList)
        {
            tile.UnHighlightTile();
        }

        foreach (var position in moves.Select(move => move.EndPosition))
        {
            getTile(position).GetComponent<TileScript>().HighlightTile();
        }
    }

    /// <summary>
    /// 根据位置索引得到对应的瓦片
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject getTile(int position)
    {
        return tileList[position].gameObject;
    }

    #endregion


}
