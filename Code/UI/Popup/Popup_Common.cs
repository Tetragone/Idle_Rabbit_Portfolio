namespace Game.UI.Popup
{
    public class Popup_Common : BasePopup
    {
        public void IsUseYes(bool isUseYes)
        {
            ButtonYes.gameObject.SetActive(isUseYes);
            ButtonNo.gameObject.SetActive(isUseYes);
            ButtonOk.gameObject.SetActive(!isUseYes);
        }
    }
}
