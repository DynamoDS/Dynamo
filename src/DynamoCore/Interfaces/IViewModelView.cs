namespace Dynamo.UI
{
    public interface IViewModelView<out T>
    {
        T ViewModel { get; }
    }
}
