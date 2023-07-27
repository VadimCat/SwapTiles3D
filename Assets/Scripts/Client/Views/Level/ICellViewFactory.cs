namespace Client.Views
{
    public interface ICellViewFactory
    {
        CellView Create(int x, int y);
    }
}