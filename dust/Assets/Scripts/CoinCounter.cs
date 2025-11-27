using TMPro;
using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    public TMP_Text counter;

    public void setValue(int value)
    {
        counter.text = value.ToString();
    }
}
