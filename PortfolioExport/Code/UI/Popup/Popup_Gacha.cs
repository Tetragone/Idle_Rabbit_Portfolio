using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Game.Calculate;
using Game.Common;
using Game.UI.Icon;

namespace Game.UI.Popup
{
    public class Popup_Gacha : BasePopup
    {
        [SerializeField]
        private Icon_SimpleItem ResItem;
        [SerializeField]
        private Transform ParentItem;
        [SerializeField]
        private Button ButtonSkip;

        [SerializeField]
        private Animator AniGacha;

        private List<Icon_SimpleItem> Items = new List<Icon_SimpleItem>();
        private List<ItemValue> ItemValues = new List<ItemValue>();

        private bool IsAni = false;
        private bool IsFirst = false;
        private float Timer = 0f;
        private const float NextTime = 0.5f;
        private int NowIdx = 0;

        public void InitEquip(List<string> equips, int lv)
        {
            NowIdx = 0;
            ButtonSkip.gameObject.SetActive(true);
            TextTitle.text = "장비 뽑기 결과";
            IsAni = true;
            IsFirst = true;
            ItemValues.Clear();

            for (int i = 0; i < equips.Count; i++)
            {
                bool isHave = false;

                foreach (var item in ItemValues)
                {
                    if (item.Id == equips[i])
                    {
                        isHave = true;
                        item.Count++;
                        break;
                    }
                }

                if (!isHave)
                {
                    ItemValue item = new ItemValue();
                    item.Type = ItemType.Equip;
                    item.Id = equips[i];
                    item.Count = 1;
                    GachaGrade grade = GachaCalculator.GetGachaGrade(GachaType.Equip, equips[i], lv);
                    item.Grade = grade;
                    ItemValues.Add(item);
                }
            }

            ItemsSetting();
        }

        public void InitSkill(List<string> skill)
        {
            NowIdx = 0;
            ButtonSkip.gameObject.SetActive(true);
            TextTitle.text = "스킬 뽑기 결과";
            IsAni = true;
            IsFirst = true;
            ItemValues.Clear();

            for (int i = 0; i < skill.Count; i++)
            {
                bool isHave = false;

                foreach (var item in ItemValues)
                {
                    if (item.Id == skill[i])
                    {
                        isHave = true;
                        item.Count++;
                        break;
                    }
                }

                if (!isHave)
                {
                    ItemValue item = new ItemValue();
                    item.Type = ItemType.Skill;
                    item.Id = skill[i];
                    item.Count = 1;
                    GachaGrade grade = GachaCalculator.GetGachaGrade(GachaType.Skill, skill[i], 0);
                    item.Grade = grade;
                    ItemValues.Add(item);
                }
            }

            ItemsSetting();
        }

        private void ItemsSetting()
        {
            int idx = 0;

            foreach (var item in ItemValues)
            {
                if (idx >= Items.Count)
                {
                    var newItem = Instantiate(ResItem, ParentItem);
                    Items.Add(newItem);
                }

                Items[idx].Init(item.Type, item.Id, item.Count);
                Items[idx].gameObject.SetActive(false);
                idx++;
            }

            for (; idx < Items.Count; idx++)
            {
                Items[idx].gameObject.SetActive(false);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            ButtonOk.onClick.AddListener(() =>
            {
                Close();
            });

            ButtonSkip.onClick.AddListener(() =>
            {
                AniGacha.gameObject.SetActive(false);
                IsAni = false;

                for (; NowIdx < ItemValues.Count; NowIdx++)
                {
                    Items[NowIdx].gameObject.SetActive(true);
                }

                ButtonSkip.gameObject.SetActive(false);
            });

            TextOk.text = "확인";
        }

        protected override void Update()
        {
            base.Update();

            if (IsAni)
            {
                if (IsFirst || Timer > NextTime && AniGacha.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f)
                {
                    IsFirst = false;

                    if (NowIdx < ItemValues.Count)
                    {
                        Items[NowIdx].gameObject.SetActive(true);
                        AniGacha.gameObject.SetActive(true);
                        AniGacha.transform.SetParent(Items[NowIdx].gameObject.transform, false);
                        AniGacha.transform.localPosition = Vector3.zero;
                        AniGacha.transform.localScale = Vector3.one;
                        AniGacha.SetTrigger(ItemValues[NowIdx].Grade.ToString());
                        Timer = 0f;
                        NowIdx++;
                    }
                    else
                    {
                        IsAni = false;
                        ButtonSkip.gameObject.SetActive(false);
                        AniGacha.gameObject.SetActive(false);
                    }
                }

                Timer += Time.deltaTime;
            }
        }

        private class ItemValue
        {
            public ItemType Type;
            public GachaGrade Grade;
            public string Id;
            public int Count;
        }
    }
}
