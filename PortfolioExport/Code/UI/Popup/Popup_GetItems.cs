using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.UI.Icon;

namespace Game.UI.Popup
{
    public class Popup_GetItems : BasePopup
    {
        [SerializeField]
        private Icon_SimpleItem ResItem;
        [SerializeField]
        private Transform ParentItem;

        private List<Icon_SimpleItem> Items = new List<Icon_SimpleItem>();
        private float Timer = 0f;
        private const float CloseTime = 5.0f;
        private int Idx = 0;

        public void AddItems(IReadOnlyDictionary<Currency, double> items)
        {
            foreach (var key in items.Keys)
            {
                if (Idx >= Items.Count)
                {
                    Icon_SimpleItem item = Instantiate(ResItem, ParentItem);
                    Items.Add(item);
                }

                Items[Idx].Init(ItemType.Currency, key.ToString(), items[key]);
                Items[Idx].gameObject.SetActive(true);
                Idx++;
            }
        }

        public void AddItem(ItemType type, string id, double count)
        {
            if (Idx >= Items.Count)
            {
                Items.Add(Instantiate(ResItem, ParentItem));
            }

            Items[Idx].Init(type, id, count);
            Items[Idx].gameObject.SetActive(true);
            Idx++;
        }

        public void AddEquips(Dictionary<string, int> equips)
        {
            foreach (var key in equips.Keys)
            {
                if (Idx >= Items.Count)
                {
                    Icon_SimpleItem item = Instantiate(ResItem, ParentItem);
                    Items.Add(item);
                }

                Items[Idx].Init(ItemType.Equip, key.ToString(), equips[key]);
                Items[Idx].gameObject.SetActive(true);
                Idx++;
            }
        }

        public void Init(string title = "")
        {
            Idx = 0;
            Timer = 0f;

            foreach (var item in Items)
            {
                item.gameObject.SetActive(false);
            }

            if (title == "")
            {
                TextTitle.text = "아이템 획득";
            }
            else
            {
                TextTitle.text = title;
            }
        }

        public void Init(Dictionary<Currency, int> items, string title = "")
        {
            Idx = 0;
            Timer = 0f;

            int idx = 0;

            foreach (var key in items.Keys)
            {
                if (idx >= Items.Count)
                {
                    Icon_SimpleItem item = Instantiate(ResItem, ParentItem);
                    Items.Add(item);
                }

                Items[idx].Init(ItemType.Currency, key.ToString(), items[key]);
                Items[idx].gameObject.SetActive(true);
                idx++;
            }

            for (; idx < Items.Count; idx++)
            {
                Items[idx].gameObject.SetActive(false);
            }

            if (title == "")
            {
                TextTitle.text = "아이템 획득";
            }
            else
            {
                TextTitle.text = title;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            ButtonOk.onClick.AddListener(Close);
            TextOk.text = "확인";
            TextTitle.text = "아이템 획득";
        }

        protected override void Update()
        {
            base.Update();

            Timer += Time.deltaTime;

            if (Timer > CloseTime && !IsEnd)
            {
                Close();
            }
        }
    }
}
