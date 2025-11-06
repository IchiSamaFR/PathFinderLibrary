using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstarLibrary
{
    public class Node
    {
        private bool _isChecked = false;
        private float _multiplier = 1;

        public List<Node> NodesAround;

        public bool IsEndNode; //Is the ending node
        public bool IsStartNode; //Is the start node
        public bool PathFound;

        public bool IsWall
        {
            get
            {
                return _multiplier == 0;
            }
            set
            {
                _multiplier = value ? 0 : 1;
            }
        }
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
            }
        }

        public Node Previous;
        public (int x, int y) Pos;
        public (int x, int y) EndNodePos;

        public float Multiplier
        {
            get
            {
                return _multiplier;
            }
            set
            {
                _multiplier = value;
            }
        }

        public float Gcost = 0; //Distance from starting node
        public float Hcost = 0; //Distance from ending node
        public float Fcost { get => Gcost + Hcost; }

        public Node((int x, int y) pos)
        {
            Pos = pos;
        }

        public void Select()
        {
            IsChecked = true;
            CheckAround();
        }
        public void ResetValues()
        {
            Gcost = 0;
            Hcost = 0;
            IsChecked = false;
            PathFound = false;
        }

        public List<Node> SetEndPath()
        {
            var lst = new List<Node>();
            PathFound = true;

            if (Previous != null)
                lst.AddRange(Previous.SetEndPath());
            return lst;
        }
        public List<Node> GetEndPath()
        {
            var lst = new List<Node>();

            if (Previous != null)
                lst.AddRange(Previous.SetEndPath());
            return lst;
        }

        public void SetCostMultiplier(float multiplier)
        {
            Multiplier = multiplier;
        }
        public bool SetCost(float gcost, (int x, int y) end)
        {
            if (IsWall)
                return false;

            Gcost = gcost;
            EndNodePos = end;

            if (IsEndNode)
            {
                SetEndPath();
                return true;
            }

            Hcost = SumHcost(end);
            return false;
        }
        private float SumHcost((int x, int y) end)
        {
            int x = end.x - Pos.x >= 0 ? end.x - Pos.x : Pos.x - end.x;
            int y = end.y - Pos.y >= 0 ? end.y - Pos.y : Pos.y - end.y;

            int val = 0;
            if (x - y >= 0)
            {
                val += x * 10;
                val += (x - (x - y)) * 4;
            }
            else
            {
                val += y * 10;
                val += (y - (y - x)) * 4;
            }
            return val;
        }

        private void CheckAround()
        {
            for (int i = 0; i < NodesAround.Count; i++)
            {
                float newGCost;
                Node node = NodesAround[i];
                if (node.Pos.x != Pos.x && node.Pos.y != Pos.y)
                {
                    newGCost = Gcost + 14 * node.Multiplier;
                }
                else
                {
                    newGCost = Gcost + 10 * node.Multiplier;
                }

                if (!node.IsStartNode && (node.Gcost <= 0 || node.Gcost > newGCost))
                {
                    node.Previous = this;
                    if (node.SetCost(newGCost, EndNodePos))
                    {
                        break;
                    }
                }
            }
        }
    }
}