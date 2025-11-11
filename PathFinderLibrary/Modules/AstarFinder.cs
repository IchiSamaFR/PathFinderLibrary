using AstarLibrary.Interfaces;
using AstarLibrary.Models;

namespace AstarLibrary.Modules
{
    public class AstarFinder : IFinder
    {
        private const int DefaultWidth = 10;
        private const int DefaultHeight = 10;

        private bool _isFinished;
        private Dictionary<(int x, int y), AstarNode> _nodes = new();
        private List<INode> _nodesListCache;
        private AstarNode _startingNode;
        private AstarNode _endingNode;
        private (int x, int y) _startingPos;
        private (int x, int y) _endingPos;

        public bool PathFound => _endingNode?.PathFound ?? false;

        public bool PathFinished
        {
            get => _isFinished || PathFound;
            set => _isFinished = value;
        }

        public INode Starting
        {
            get => _startingNode;
        }
        public INode Ending
        {
            get => _endingNode;
        }

        public bool IsDiagonal { get; set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public List<INode> NodesList
        {
            get
            {
                if (_nodesListCache == null)
                {
                    _nodesListCache = _nodes.Values.Cast<INode>().ToList();
                }
                return _nodesListCache;
            }
        }

        public AstarFinder(int width = DefaultWidth, int height = DefaultHeight, (int x, int y)? start = null, (int x, int y)? end = null)
        {
            Width = width;
            Height = height;

            if (start != null)
            {
                SetEndPos(start.Value.x, start.Value.y);
            }
            else
            {
                SetEndPos(0, 0);
            }
            if (end != null)
            {
                SetEndPos(end.Value.x, end.Value.y);
            }
            else
            {
                SetEndPos(width - 1, height - 1);
            }
        }

        public INode SelectNextNode()
        {
            if (PathFinished)
            {
                return null;
            }

            var nodeToSelect = FindLowestCostNode();

            if (nodeToSelect != null)
            {
                nodeToSelect.Select();
                return nodeToSelect;
            }

            PathFinished = true;
            return null;
        }

        public List<INode> SelectPath()
        {
            while (!PathFinished)
            {
                var nodeToSelect = FindLowestCostNode();

                if (nodeToSelect != null)
                {
                    nodeToSelect.Select();
                }
                else
                {
                    PathFinished = true;
                }
            }

            return _endingNode?.GetEndPath().Cast<INode>().ToList() ?? new List<INode>();
        }

        public void ToggleWall(int x, int y)
        {
            var node = GetNode(x, y);
            if (node != null && !node.IsStartNode && !node.IsEndNode)
            {
                node.IsWall = !node.IsWall;
            }
        }

        public void Clear()
        {
            PathFinished = false;

            _nodes.Clear();
            _nodesListCache = null;

            SetStartPos(_startingPos.x, _startingPos.y);
            SetEndPos(_endingPos.x, _endingPos.y);
        }
        public void Reset()
        {
            PathFinished = false;

            // Remove all nodes out of bounds
            var outOfBoundsNodes = _nodes.Where(kvp => !IsValidPosition(kvp.Key.x, kvp.Key.y)).ToList();
            foreach (var kvp in outOfBoundsNodes)
            {
                _nodes.Remove(kvp.Key);
            }
            foreach (var node in _nodes.Values)
            {
                node.Reset();
            }

            SetStartPos(_startingPos.x, _startingPos.y);
            SetEndPos(_endingPos.x, _endingPos.y);
            _nodesListCache = null;
        }

        private AstarNode FindLowestCostNode()
        {
            AstarNode nodeToSelect = null;

            foreach (var node in _nodes.Values)
            {
                if (!node.IsWall && !node.IsChecked && node.Fcost > 0)
                {
                    if (nodeToSelect == null || node.Fcost < nodeToSelect.Fcost)
                    {
                        nodeToSelect = node;
                    }
                }
            }

            return nodeToSelect;
        }

        public void SetGridSize(int width, int height)
        {
            Width = width;
            Height = height;

            var outOfBoundsNodes = _nodes.Where(kvp => !IsValidPosition(kvp.Key.x, kvp.Key.y) || !kvp.Value.IsWall).ToList();
            foreach (var kvp in outOfBoundsNodes)
            {
                _nodes.Remove(kvp.Key);
            }
            SetStartPos(_startingPos.x, _startingPos.y);
            SetEndPos(_endingPos.x, _endingPos.y);
            _nodesListCache = null;
        }

        public void SetStartPos(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                return;
            }

            if (_startingNode != null)
            {
                _startingNode.IsStartNode = false;
            }

            _startingPos = (x, y);
            _startingNode = GetOrCreateNode(x, y);

            if (_endingPos != default)
            {
                _startingNode.SetCost(0, _endingPos);
            }

            _startingNode.IsStartNode = true;
            _nodesListCache = null;
        }

        public void SetEndPos(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                return;
            }

            if (_endingNode != null)
            {
                _endingNode.IsEndNode = false;
            }

            _endingPos = (x, y);
            _endingNode = GetOrCreateNode(x, y);

            if (_startingNode != null)
            {
                _startingNode.SetCost(0, _endingPos);
            }

            _endingNode.IsEndNode = true;
            _nodesListCache = null;
        }

        private List<AstarNode> GetNodesAround((int x, int y) pos)
        {
            List<AstarNode> nodes = new List<AstarNode>(IsDiagonal ? 8 : 4);

            if (IsDiagonal)
            {
                for (int x = pos.x - 1; x <= pos.x + 1; x++)
                {
                    for (int y = pos.y - 1; y <= pos.y + 1; y++)
                    {
                        if (x == pos.x && y == pos.y)
                        {
                            continue;
                        }

                        AddNodeIfExists(nodes, x, y);
                    }
                }
            }
            else
            {
                AddNodeIfExists(nodes, pos.x + 1, pos.y);
                AddNodeIfExists(nodes, pos.x - 1, pos.y);
                AddNodeIfExists(nodes, pos.x, pos.y + 1);
                AddNodeIfExists(nodes, pos.x, pos.y - 1);
            }

            return nodes;
        }

        private void AddNodeIfExists(List<AstarNode> nodes, int x, int y)
        {
            if (IsValidPosition(x, y))
            {
                nodes.Add(GetOrCreateNode(x, y));
            }
        }

        public AstarNode GetNode(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                return null;
            }

            _nodes.TryGetValue((x, y), out var node);
            return node;
        }

        public AstarNode GetOrCreateNode(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                return null;
            }

            if (!_nodes.TryGetValue((x, y), out var node))
            {
                node = new AstarNode((x, y))
                {
                    EndNodePos = _endingPos
                };
                _nodes[(x, y)] = node;
                node.NodesAround = GetNodesAround((x, y));
                _nodesListCache = null;
            }

            return node;
        }

        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}