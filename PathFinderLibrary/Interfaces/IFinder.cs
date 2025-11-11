using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstarLibrary.Interfaces
{
    public interface IFinder
    {
        int Width { get; }
        int Height { get; }
        bool IsDiagonal { get; set; }

        bool PathFound { get; }
        bool PathFinished { get; }

        INode Starting { get; }
        INode Ending { get; }
        List<INode> NodesList { get; }

        INode SelectNextNode();
        List<INode> SelectPath();
        void SetGridSize(int width, int height);
        void SetStartPos(int x, int y);
        void SetEndPos(int x, int y);
        void ToggleWall(int x, int y);
        void Reset();
        void Clear();
    }
}
