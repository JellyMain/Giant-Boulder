using Structures;
using TMPro;
using UnityEngine;


public class StatisticsUIObject : MonoBehaviour
{
    [SerializeField] private ObjectType objectType;
    [SerializeField] private TMP_Text amountText;
    public ObjectType ObjectType => objectType;
    public TMP_Text AmountText => amountText;
}
