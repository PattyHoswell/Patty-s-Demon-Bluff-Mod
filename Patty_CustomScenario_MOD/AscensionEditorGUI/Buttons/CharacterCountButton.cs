using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppTMPro;
using Patty_CustomScenario_MOD.QoL;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons
{
    /// <summary>
    /// Button handler to increase/decrease the amount of character to show or in deck view. for Characters Amount inside of <see cref="CharacterCountContent"/>
    /// </summary>
    public class CharacterCountButton : MonoBehaviour
    {
        public CharacterCountButton() : base(ClassInjector.DerivedConstructorPointer<CharacterCountButton>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public CharacterCountButton(IntPtr ptr) : base(ptr) { }

        public AllCharacterMenuButton ParentContentButton { get; internal set; }
        public CharactersCount TargetCharCount { get; internal set; }
        public TextMeshProUGUI TextMeshPro { get; internal set; }
        public bool IsDeck { get; internal set; }
        public int Amount
        {
            get
            {
                return TargetCharCount.GetAmount(CharacterType, IsDeck);
            }
            set
            {
                var result = Mathf.Clamp(value, 0, int.MaxValue);
                switch (CharacterType)
                {
                    case ECharacterType.Villager:
                        if (IsDeck)
                            TargetCharCount.dTown = result;
                        else
                            TargetCharCount.town = result;
                        break;
                    case ECharacterType.Outcast:
                        if (IsDeck)
                            TargetCharCount.dOuts = result;
                        else
                            TargetCharCount.outs = result;
                        break;
                    case ECharacterType.Minion:
                        if (IsDeck)
                            TargetCharCount.dMinion = result;
                        else
                            TargetCharCount.minion = result;
                        break;
                    case ECharacterType.Demon:
                        if (IsDeck)
                            TargetCharCount.dDemon = result;
                        else
                            TargetCharCount.demon = result;
                        break;
                }
                TargetCharCount.UpdateAllCharCount();
                ParentContentButton.UpdateLabelName();
            }
        }
        public Button DecreaseButton { get; internal set; }
        public Button IncreaseButton { get; internal set; }
        public ECharacterType CharacterType { get; internal set; }
        public event Action<CharacterCountButton, int, int> OnAmountChanged;

        internal bool initialized;

        void Awake()
        {
            Initialize();
        }

        void Initialize()
        {
            if (initialized)
                return;
            initialized = true;

            TextMeshPro = transform.Find("Amount/Amt").GetComponent<TextMeshProUGUI>();
            DecreaseButton = transform.Find("Amount/Decrease").GetComponent<Button>();
            IncreaseButton = transform.Find("Amount/Increase").GetComponent<Button>();

            DecreaseButton.onClick.AddListener((UnityAction)(DecreaseAmt));
            IncreaseButton.onClick.AddListener((UnityAction)(IncreaseAmt));
        }

        public void DecreaseAmt()
        {
            Amount -= 1;
            SetValue(Amount);
        }

        public void IncreaseAmt()
        {
            Amount += 1;
            SetValue(Amount);
        }

        internal void Setup()
        {
            Initialize();
            SetValue(Amount);
        }

        public void SetValue(int amount, bool notification = true)
        {
            var previousAmount = Amount;
            Amount = amount;
            TextMeshPro.text = amount.ToString();
            if (notification)
            {
                OnAmountChanged?.Invoke(this, previousAmount, Amount);
            }
        }
    }
}
