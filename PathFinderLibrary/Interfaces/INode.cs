namespace AstarLibrary.Interfaces
{
    public interface INode
    {
        bool IsEndNode { get; }
        bool IsStartNode { get; }
        bool PathFound { get; }
        bool IsWall { get; }
        bool IsChecked { get; }
        (int x, int y) Pos { get; }

        void Reset();
    }
}
