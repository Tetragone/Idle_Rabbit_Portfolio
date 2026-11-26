using System.Collections.Generic;

using UnityEngine;

using Game.Battle;
using Game.Battle.Stage;
using Game.UI.Icon;

namespace Game.UI.Popup
{
    public class Popup_RoguelikeSelect : BasePopup
    {
        [SerializeField] private Icon_RoguelikeEffect ResEffect;
        [SerializeField] private Transform ParentEffects;

        private readonly List<Icon_RoguelikeEffect> Icons = new List<Icon_RoguelikeEffect>();
        private RoguelikeStage Stage;
        private int WaveNumber;

        protected override void Awake()
        {
            base.Awake();

            // 효과 선택은 필수이므로 배경 클릭과 닫기 버튼으로 취소할 수 없다.
            if (ButtonBlock != null)
            {
                ButtonBlock.onClick.RemoveAllListeners();
            }

            ButtonClose?.gameObject.SetActive(false);
            ButtonYes?.gameObject.SetActive(false);
            ButtonNo?.gameObject.SetActive(false);
            ButtonOk?.gameObject.SetActive(false);
        }

        public void Init(RoguelikeStage stage)
        {
            if (stage.State != RoguelikeState.Selecting || stage.ChoiceIds.Count < 1 ||
                stage.ChoiceIds.Count > RoguelikeStage.ChoiceCount)
            {
                throw new System.InvalidOperationException("효과 선택 팝업에는 1~3개의 후보가 필요합니다.");
            }

            Stage = stage;
            WaveNumber = stage.WaveNumber;
            TextTitle.text = "효과 선택";

            for (int i = 0; i < stage.ChoiceIds.Count; i++)
            {
                if (i >= Icons.Count)
                {
                    Icons.Add(Instantiate(ResEffect, ParentEffects));
                }

                Icons[i].Init(stage.ChoiceIds[i], SelectEffect);
                Icons[i].gameObject.SetActive(true);
            }

            for (int i = stage.ChoiceIds.Count; i < Icons.Count; i++)
            {
                Icons[i].gameObject.SetActive(false);
            }
        }

        private void SelectEffect(string id)
        {
            if (BattleMgr.Instance.TryGetRoguelike(out var current) && current == Stage &&
                current.WaveNumber == WaveNumber && BattleMgr.Instance.TrySelectRoguelikeEffect(id))
            {
                Close();
            }
        }

        public override void CloseAction()
        {
            Stage = null;
        }
    }
}
