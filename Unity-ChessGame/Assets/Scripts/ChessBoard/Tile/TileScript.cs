using UnityEngine;

public class TileScript : MonoBehaviour
{

    public GameObject highlight;//悬停高亮
    private GameObject _tileHighlight;//合法高亮
    private TileManager _tileManager;//引入管理器
    public int TilePlacement{ get; private set; }

    //合法移动高亮
    public void HighlightTile()
    {
        _tileHighlight.SetActive(true);
    }
    //取消高亮方法
    public void UnHighlightTile()
    {
        _tileHighlight.SetActive(false);
    }
    void Start()
    {
        _tileManager = gameObject.GetComponentInParent<TileManager>();
        _tileHighlight = transform.Find("TileHighlight").gameObject;//从hierarchy找到悬停高亮预制体
        //根据本地位置和缩放，得到逻辑位置
        Vector3 localPosition = transform.localPosition;
        Vector3 localScale = transform.localScale;
        TilePlacement = (int)localPosition.z/(int)(10*localScale.z) * 8 + (int)localPosition.x/(int)(10*localScale.x);
    }
    //鼠标点击方法
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _tileManager.clickTile(TilePlacement);
        }
    }
    //鼠标悬停方法
    private void OnMouseEnter()
    {
        if (!BoardManager._humainPlayer) return;
        highlight.SetActive(true);
        highlight.transform.position = transform.position;
    }
    //鼠标离开方法
    private void OnMouseExit()
    {
        highlight.SetActive(false);
    }
}
