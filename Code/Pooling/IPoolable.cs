namespace Game.Pooling
{
    public interface IPoolable
    {
        public void OnObtain();
        public void OnDispose();
    }
}
