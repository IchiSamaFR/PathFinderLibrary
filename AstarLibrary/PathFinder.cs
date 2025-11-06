using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AstarLibrary
{
    public class PathFinder
    {
        private const int DefaultWidth = 10;
        private const int DefaultHeight = 10;

        private bool _isDiagonal;
        private bool _isFinished;
        private Dictionary<(int x, int y), Node> _nodes;
        private List<Node> _nodesListCache;
        private Node _startingNode;
        private Node _endingNode;
        private (int x, int y) _startingPos;
        private (int x, int y) _endingPos;

        public bool PathFound => _endingNode?.PathFound ?? false;

        public bool PathFinished
        {
            get => _isFinished || PathFound;
            set => _isFinished = value;
        }

        public bool IsDiagonal
        {
            get => _isDiagonal;
            set
            {
                if (_isDiagonal != value)
                {
                    _isDiagonal = value;
                    UpdateNodesAround();
                }
            }
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public List<Node> NodesList
        {
            get
            {
                if (_nodesListCache == null)
                {
                    _nodesListCache = _nodes.Values.ToList();
                }
                return _nodesListCache;
            }
        }

        public PathFinder(int width = DefaultWidth, int height = DefaultHeight, (int x, int y)? start = null, (int x, int y)? end = null)
        {
            Width = width;
            Height = height;
            _nodes = new Dictionary<(int x, int y), Node>();

            SetStartPos(start ?? (0, 0));
            SetEndPos(end ?? (width - 1, height - 1));
        }

        public Node SelectNextNode()
        {
            if (PathFinished)
            {
                return null;
            }

            Node nodeToSelect = FindLowestCostNode();

            if (nodeToSelect != null)
            {
                nodeToSelect.Select();
                return nodeToSelect;
            }

            PathFinished = true;
            return null;
        }

        public List<Node> SelectPath()
        {
            while (!PathFinished)
            {
                Node nodeToSelect = FindLowestCostNode();

                if (nodeToSelect != null)
                {
                    nodeToSelect.Select();
                }
                else
                {
                    PathFinished = true;
                }
            }

            return _endingNode?.GetEndPath() ?? new List<Node>();
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

            SetStartPos(_startingPos);
            SetEndPos(_endingPos);
        }
        public void Reset()
        {
            PathFinished = false;

            // Remove all nodes that are not walls
            var wallNodes = _nodes.Where(kvp => !kvp.Value.IsWall).ToList();
            foreach (var kvp in wallNodes)
            {
                _nodes.Remove(kvp.Key);
            }

            SetStartPos(_startingPos);
            SetEndPos(_endingPos);
            _nodesListCache = null;
        }

        private Node FindLowestCostNode()
        {
            Node nodeToSelect = null;

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

        private void UpdateNodesAround()
        {
            foreach (var node in _nodes.Values)
            {
                node.NodesAround = GetNodesAround(node.Pos);
            }
            _nodesListCache = null;
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
            SetStartPos(_startingPos);
            SetEndPos(_endingPos);
            _nodesListCache = null;
        }

        public void SetStartPos((int x, int y) startPos)
        {
            if (!IsValidPosition(startPos.x, startPos.y))
            {
                return;
            }

            if (_startingNode != null)
            {
                _startingNode.IsStartNode = false;
            }

            _startingPos = startPos;
            _startingNode = GetOrCreateNode(startPos.x, startPos.y);

            if (_endingPos != default)
            {
                _startingNode.SetCost(0, _endingPos);
            }

            _startingNode.IsStartNode = true;
            _nodesListCache = null;
        }

        public void SetEndPos((int x, int y) endPos)
        {
            if (!IsValidPosition(endPos.x, endPos.y))
            {
                return;
            }

            if(_endingNode != null)
            {
                _endingNode.IsEndNode = false;
            }

            _endingPos = endPos;
            _endingNode = GetOrCreateNode(endPos.x, endPos.y);

            if (_startingNode != null)
            {
                _startingNode.SetCost(0, _endingPos);
            }

            _endingNode.IsEndNode = true;
            _nodesListCache = null;
        }

        private List<Node> GetNodesAround((int x, int y) pos)
        {
            List<Node> nodes = new List<Node>(IsDiagonal ? 8 : 4);

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

        private void AddNodeIfExists(List<Node> nodes, int x, int y)
        {
            if (IsValidPosition(x, y))
            {
                nodes.Add(GetOrCreateNode(x, y));
            }
        }

        private Node GetNode(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                return null;
            }

            _nodes.TryGetValue((x, y), out var node);
            return node;
        }

        private Node GetOrCreateNode(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                return null;
            }

            if (!_nodes.TryGetValue((x, y), out var node))
            {
                node = new Node((x, y))
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