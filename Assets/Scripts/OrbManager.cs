using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OrbManager : MonoBehaviour
{
    
    /// <summary>
    /// オブジェクト参照
    /// </summary>
    private GameObject gameManager; // ゲームマネージャー

    /// <summary>
    /// public変数
    /// </summary>
    public Sprite[] orbPicture = new Sprite[3]; // オーブ絵

    public enum ORB_KIND            // オーブ種類定義（int型値保持ver）
    {
        BLUE = 1,
        GREEN = 5,
        PURPLE = 10
    }

    private ORB_KIND orbKind;       // 列挙体ORB_KIND型変数

    // Start is called before the first frame update
    void Start()
    {
        // ゲームマネージャークラスを文字列で参照
        gameManager = GameObject.Find("GameManager");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// オーブ取得
    /// </summary>
    public void TouchOrb()
    {
        // マウスボタンが押されていない場合
        if (Input.GetMouseButton(0) == false)
        {
            return;
        }

        // オーブのRectTransform取得
        RectTransform rect = GetComponent<RectTransform>();

        // オーブの軌跡設定
        Vector3[] path = {
            // 中間点（現在x座標*1.5、y座標300）
            new Vector3(rect.localPosition.x * 1.5f, 300f, 0f),
            // 終点（x座標0、y座標150）
            new Vector3(0f, 150f, 0f),
        };

        // DOTweenを使ったアニメ設定（座標配列、時間、パスタイプ）
        rect.DOLocalPath(path, 0.5f, PathType.CatmullRom)
            // イージング設定
            .SetEase(Ease.OutQuad)
            // アニメーション終了時呼び出しメソッド指定
            .OnComplete(AddOrbPoint);
        
        // サイズ変更アニメ設定（目標サイズ配列、時間）
        rect.DOScale(new Vector3(0.5f, 0.5f, 0f), 0.5f);
        
    }

    /// <summary>
    /// オーブ画像設定
    /// </summary>
    /// <param name="kind">オーブの種類</param>
    public void SetKind(ORB_KIND kind)
    {
        orbKind = kind;

        switch (orbKind)
        {
            case ORB_KIND.BLUE:
                GetComponent<Image>().sprite = orbPicture[0];
                break;
            case ORB_KIND.GREEN:
                GetComponent<Image>().sprite = orbPicture[1];
                break;
            case ORB_KIND.PURPLE:
                GetComponent<Image>().sprite = orbPicture[2];
                break;
        }
    }

    // オーブアニメ終了後にポイント加算処理
    void AddOrbPoint()
    {
        // オーブ種類判定及びGetOrb呼び出し
        switch (orbKind)
        {
            case ORB_KIND.BLUE:
                gameManager.GetComponent<GameManager>().GetOrb((int)ORB_KIND.BLUE);
                break;
            case ORB_KIND.GREEN:
                gameManager.GetComponent<GameManager>().GetOrb((int)ORB_KIND.GREEN);
                break;
            case ORB_KIND.PURPLE:
                gameManager.GetComponent<GameManager>().GetOrb((int)ORB_KIND.PURPLE);
                break;
        }

        // オーブのゲームオブジェクトを破棄
        Destroy(this.gameObject);
    }
}
