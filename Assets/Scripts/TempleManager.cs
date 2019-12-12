using UnityEngine;
using UnityEngine.UI;

public class TempleManager : MonoBehaviour
{
    /// <summary>
    /// public変数
    /// </summary>
    public Sprite[] templePicture = new Sprite[3];  // 寺の絵

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 寺の絵を設定
    /// </summary>
    /// <param name="level">現在の寺レベル</param>
    public void SetTemplePicture(int level)
    {
        GetComponent<Image>().sprite = templePicture[level];
    }

    /// <summary>
    /// 寺の大きさを設定
    /// </summary>
    /// <param name="score">現在のscore</param>
    /// <param name="nextScore">レベルアップに必要なscore</param>
    public void SetTempleScale(int score, int nextScore)
    {
        float scale = 0.5f + (((float)score / (float)nextScore) / 2.0f);
        transform.localScale = new Vector3(scale, scale, 1.0f);
    }
}
