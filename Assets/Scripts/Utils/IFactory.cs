namespace Utils
{
    public interface IFactory<T>
    {
        T Create(int id);
        void Activate(T obj);
        void Deactivate(T obj);
    }
}