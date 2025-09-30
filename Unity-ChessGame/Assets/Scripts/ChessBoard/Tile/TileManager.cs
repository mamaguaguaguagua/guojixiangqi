using System.Collections.Generic;
using System.Linq;
using ChessModel;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    //����洢�������̸�ű��ű�
    private TileScript[] tileList;
    //����boardManger����ͨ��
    private BoardManager boardManager;

    private void Awake()
    {
        tileList = gameObject.GetComponentsInChildren<TileScript>();
        boardManager = gameObject.GetComponentInParent<BoardManager>();
    }
    #region ��������
    /// <summary>
    /// �����Ƭ����
    /// </summary>
    /// <param name="tilePlacement">λ������</param>
    public void clickTile(int tilePlacement)
    {
        boardManager.ClickTile(tilePlacement);
    }
    /// <summary>
    /// ��Ƭ�����ĵ�λ��
    /// </summary>
    /// <param name="position">λ������</param>
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
    /// ���¸�����������ʾ���кϷ���������ʾ��
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
    /// ����λ�������õ���Ӧ����Ƭ
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject getTile(int position)
    {
        return tileList[position].gameObject;
    }

    #endregion


}
