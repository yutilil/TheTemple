using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 定数定義
    /// </summary>
    private const int MAX_ORB = 20;         // オーブ最大数
    private const int MAX_LEVEL = 2;        // 最大お寺レベル
    private const double RESPAWN_TIME = 0.5;// オーブ再発生秒数
    
    /// <summary>
    /// データセーブ用キー
    /// </summary>
    private const string KEY_SCORE = "SCORE";
    private const string KEY_LEVEL = "LEVEL";
    private const string KEY_ORB = "ORB";
    private const string KEY_TIME = "TIME";
    
    /// <summary>
    /// オブジェクト参照変数
    /// MonoBehaviour継承クラスでクラス変数（public変数）を宣言すると
    /// インスペクターに設定項目（プロパティ）として表示され、エディタ上で
    /// 値を設定できる
    /// </summary>
    public GameObject orbPrefab;        // オーブプレハブ
    public GameObject smokePrefab;      // 煙プレハブ
    public GameObject kusudamaPrefab;   // くす玉プレハブ
    public GameObject canvasGame;       // ゲームキャンパス
    public GameObject textScore;        // スコアテキスト
    public GameObject imageTemple;      // お寺イメージ
    public GameObject imageMokugyo;     // 木魚イメージ

    public AudioClip getScoreSE;        // 効果音：スコアゲット
    public AudioClip levelUpSE;         // 効果音：レベルアップ
    public AudioClip clearSE;           // 効果音：クリア

    /// <summary>
    /// private変数
    /// </summary>
    private int score;              // 現在のスコア
    private int nextScore;          // レベルアップまでに必要なスコア
    private int currentOrb;         // 現在のオーブ数
    private int templeLevel;        // 寺レベル
    private DateTime lastDateTime;  // 前回オーブを生成した時間
    private int[] nextScoreTable = new int[]{50, 200, 500}; // レベルアップ値
    private AudioSource audioSource;// オーディオソース
    private bool isClear = false;   // クリア判定（初期値：false）

    // Start is called before the first frame update
    void Start()
    {
        // オーディオソース取得（ゲームスタート時）
        audioSource = this.gameObject.GetComponent<AudioSource>();

        // クリア後の呼び出しでない場合
        if (!isClear)
        {
            // 初期設定（セーブデータ読み込み）
            score = PlayerPrefs.GetInt(KEY_SCORE, 0);
            templeLevel = PlayerPrefs.GetInt(KEY_LEVEL, 0);
            currentOrb = PlayerPrefs.GetInt(KEY_ORB, MAX_ORB);
            
            // 開始時刻設定
            string time = PlayerPrefs.GetString(KEY_TIME, "");
            if (time == "")
            {
                // 時間がセーブされていない場合は現在時刻
                lastDateTime = DateTime.UtcNow;
            }
            else
            {
                // 時間がセーブされている場合は復元
                long temp = Convert.ToInt64(time);
                lastDateTime = DateTime.FromBinary(temp);
                //lastDateTime = DateTime.FromBinary(Convert.ToInt64(time));
            }
        }
        // クリア後の呼び出しの場合
        else
        {
            score = 0;
            templeLevel = 0;
            currentOrb = MAX_ORB;
            lastDateTime = DateTime.UtcNow;

            // プレイデータ破棄
            PlayerPrefs.DeleteAll();
        }

        // 初期オーブ生成
        for (int i = 0; i < currentOrb; i++)
        {
            CreateOrb();
        }

        // 寺初期設定
        nextScore = nextScoreTable[templeLevel];
        imageTemple.GetComponent<TempleManager>().SetTemplePicture(templeLevel);
        imageTemple.GetComponent<TempleManager>().SetTempleScale(score, nextScore);

        // スコア初期値設定
        RefreshScoreText();

        // デバッグ
        //Debug.Log(lastDateTime);
        //Debug.Log(lastDateTime.ToBinary());
        //Debug.Log(lastDateTime.ToBinary().ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if (currentOrb < MAX_ORB)
        {
            // 現在時から最終取得時を引く(経過時間が算出される)
            TimeSpan timeSpan = DateTime.UtcNow - lastDateTime;
            
            // このif文はwhile真偽判定と同じことをしているので、いらない
            //if (timeSpan >= TimeSpan.FromSeconds(RESPAWN_TIME))
            //{
                while(timeSpan >= TimeSpan.FromSeconds(RESPAWN_TIME))
                {
                    CreateNewOrb();
                    // 経過時間からオーブ発生秒数を引く
                    timeSpan -= TimeSpan.FromSeconds(RESPAWN_TIME);
                }
            //}
        }
    }

    /// <summary>
    /// 新規オーブ生成
    /// </summary>
    public void CreateNewOrb()
    {
        // 新規最終取得時設定
        lastDateTime = DateTime.UtcNow;
        
        // オーブ最大数チェック
        if (currentOrb >= MAX_ORB)
        {
            return;
        }
        CreateOrb();
        currentOrb++;

        // セーブ処理
        SaveGameDate();
    }
    
    /// <summary>
    /// オーブ入手
    /// </summary>
    /// <param name="getScore">オーブ種類によって変化する得点</param>
    public void GetOrb(int getScore)
    {
        // スコアゲット効果音
        audioSource.PlayOneShot(getScoreSE);

        // 木魚アニメ再生状態取得
        AnimatorStateInfo stateInfo = 
        imageMokugyo.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        // 木魚アニメ再生状態確認
        if (stateInfo.fullPathHash == Animator.StringToHash("Base Layer.get@ImageMokugyo"))
        {
            // すでに再生中なら先頭から再生
            imageMokugyo.GetComponent<Animator>().Play(stateInfo.fullPathHash, 0, 0.0f);
        }
        else
        {
            // 指定したTrigger型パラメータをtrueにする
            imageMokugyo.GetComponent<Animator>().SetTrigger("isGetScore");
        }

        // score加算
        score += getScore;

        // レベルアップ値を超えないよう制限
        if (score > nextScore)
        {
            score = nextScore;
        }
        
        // レベルアップ処理(判定含む)
        TempleLevelUp();
        // スコアボード更新
        RefreshScoreText();
        // 寺スケール設定
        imageTemple.GetComponent<TempleManager>().SetTempleScale(score, nextScore);

        // クリア判定
        if (score == nextScore && templeLevel == MAX_LEVEL)
        {
            ClearEffect();
        }

        // 現在オーブ数一つ減らす
        currentOrb--;

        // セーブ処理
        SaveGameDate();
    }

    // オーブ生成
    void CreateOrb()
    {
        // プレハブからオーブクローンを取得
        GameObject orb = (GameObject)Instantiate(orbPrefab);
        // オーブに親を設定（canvasGameのtransform）
        orb.transform.SetParent(canvasGame.transform, false);
        // オーブの出現位置をRandomにて指定
        orb.transform.localPosition = new Vector3(
            UnityEngine.Random.Range(-300.0f, 300.0f),
            UnityEngine.Random.Range(-140.0f, -500.0f),
            0f
        );

        // オーブ種類設定
        int kind = UnityEngine.Random.Range(0, templeLevel + 1);
        switch (kind)
        {
            case 0:
                orb.GetComponent<OrbManager>().SetKind(OrbManager.ORB_KIND.BLUE);
                break;
            case 1:
                orb.GetComponent<OrbManager>().SetKind(OrbManager.ORB_KIND.GREEN);
                break;
            case 2:
                orb.GetComponent<OrbManager>().SetKind(OrbManager.ORB_KIND.PURPLE);
                break;
        }
    }

    // スコアテキスト更新
    void RefreshScoreText()
    {
        textScore.GetComponent<Text>().text = "徳：" + score + " / " + nextScore;
    }

    // 寺レベル管理
    void TempleLevelUp()
    {
        if (score >= nextScore && templeLevel < MAX_LEVEL)
        {
            templeLevel++;
            score = 0;

            // レベルアップ時の演出呼び出し
            TempleLevelUpEffect();

            nextScore = nextScoreTable[templeLevel];
            imageTemple.GetComponent<TempleManager>().SetTemplePicture(templeLevel);
        }
    }

    // 寺レベルアップ演出
    void TempleLevelUpEffect()
    {
        // smokePrefabからクローンを一つ取得
        GameObject smoke = (GameObject)Instantiate(smokePrefab);
        // canvasGameの子に設定
        smoke.transform.SetParent(canvasGame.transform, false);
        // 表示順を寺と木魚の間に設定
        smoke.transform.SetSiblingIndex(2);

        // レベルアップ効果音
        audioSource.PlayOneShot(levelUpSE);

        // 0.5秒後に廃棄
        Destroy(smoke, 0.5f);
    }

    // クリア演出
    void ClearEffect()
    {
        // kusudamaPrefabからクローンを一つ取得
        GameObject kusudama = (GameObject)Instantiate(kusudamaPrefab);
        // canvasGameの子に設定
        kusudama.transform.SetParent(canvasGame.transform, false);

        // クリア効果音
        audioSource.PlayOneShot(clearSE);

        // 1.5秒後に破棄
        Destroy(kusudama, 1.5f);

        // クリア判定ture
        isClear = true;
        // Startメソッドを読んで初期化
        Start();
    }

    // ゲームデータセーブ
    void SaveGameDate()
    {
        // 各種データをキーと値で登録
        PlayerPrefs.SetInt(KEY_SCORE, score);
        PlayerPrefs.SetInt(KEY_LEVEL, templeLevel);
        PlayerPrefs.SetInt(KEY_ORB, currentOrb);
        PlayerPrefs.SetString(KEY_TIME, lastDateTime.ToBinary().ToString());

        // セーブ
        PlayerPrefs.Save();
    }
}
