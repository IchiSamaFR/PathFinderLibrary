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
        bool IsDiagonal { get; }

        bool PathFound { get; }
        bool PathFinished { get; }

        INode Starting { get; }
        INode Ending { get; }
        List<INode> NodesList { get; }
    }
}
