using UnityEngine;
using TMPro;

public   class ScoreCounter : MonoBehaviour
{
    public static ScoreCounter Instance { get; private set; }

    private int _score;

    public int Score
    {
        get => _score;

        set
        {
            if (_score == value) 
            {
                return;
            }

            _score = value;

            scoreText.SetText($"Runs : {_score}"); //interpolated string
        }
    }

    [SerializeField] private TextMeshProUGUI scoreText;

    private void Awake()
    {
        Instance = this;
    }
}
