using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Battle
{
    public class PlayerController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public static bool IsControlling => !IsAuto;
        public static Vector3 Diff = Vector3.zero;

        [SerializeField]
        private GameObject ObjBaseController;
        [SerializeField]
        private Image BaseContorller;
        [SerializeField]
        private GameObject ObjMoveController;
        [SerializeField]
        private Image MoveController;

        private Color BaseStartColor;
        private Color MoveStarteColor;
        private const float TouchAlphaMulti = 0.7f;
        private readonly Vector3 BasePosition = new Vector3(200f, 200f, 0f);
        private readonly float MaxDistance = 200f;
        private static bool IsAuto = true;
        private bool IsTouched = false;
        private float AutoTime = 10f;
        private float Timer = 0f;
        private Vector3 StartPosition;

        public void OnBeginDrag(PointerEventData eventData)
        {
            IsTouched = true;
            Timer = 0f;
            IsAuto = false;
            ObjBaseController.transform.position = eventData.position;
            StartPosition = eventData.position;
            StartPosition.z = 0f;
            ObjMoveController.transform.position = eventData.position;
            BaseContorller.color = BaseStartColor;
            MoveController.color = MoveStarteColor;
            Diff = Vector3.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 position = eventData.position;
            position.z = 0f;
            Diff = position - StartPosition;

            if (Vector3.Distance(StartPosition, position) > MaxDistance)
            {
                Diff = Diff * MaxDistance / Vector3.Distance(StartPosition, position);
                position = StartPosition + Diff;
            }

            ObjMoveController.transform.position = position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            ObjBaseController.transform.position = BasePosition;
            ObjMoveController.transform.position = BasePosition;
            ChangeAlpha(BaseContorller, BaseStartColor);
            ChangeAlpha(MoveController, MoveStarteColor);
            IsTouched = false;
            Diff = Vector3.zero;
        }

        private void ChangeAlpha(Image image, Color baseColor)
        {
            Color newColor = baseColor;
            newColor.a *= TouchAlphaMulti;
            image.color = newColor;
        }

        private void Awake()
        {
            BaseStartColor = BaseContorller.color;
            MoveStarteColor = MoveController.color;
            ChangeAlpha(BaseContorller, BaseStartColor);
            ChangeAlpha(MoveController, MoveStarteColor);
        }

        private void Update()
        {
            if (!IsTouched)
            {
                Timer += Time.deltaTime;

                if (Timer > AutoTime)
                {
                    Timer = 0f;
                    IsAuto = true;
                }
            }
        }
    }
}
