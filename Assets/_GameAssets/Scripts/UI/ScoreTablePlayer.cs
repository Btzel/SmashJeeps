using TMPro;
using Unity.Collections;
using UnityEngine;

public class ScoreTablePlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text _rankingText;
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private TMP_Text _killsText;
    [SerializeField] private Color _ownerColor;

    public void SetScoreTableData(string rank, FixedString32Bytes playerName, string kills, bool isOwner)
    {
        _rankingText.text = rank;
        _playerNameText.text = playerName.ToString();
        _killsText.text = kills;
        _playerNameText.color = isOwner ? _ownerColor : Color.white;
    }
}
