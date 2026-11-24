namespace Game.Data
{
    public interface IBaseData<T>
    {
        public void SetToDefault();
        public void SetData(T beforeData);
    }
}
