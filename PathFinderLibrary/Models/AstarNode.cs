using AstarLibrary.Interfaces;

namespace AstarLibrary.Models
{
    public class AstarNode : INode
    {
        private float _multiplier = 1;

        public List<AstarNode> NodesAround;

        public bool IsEndNode { get; set; } //Is the ending node
        public bool IsStartNode { get; set; } //Is the start node
        public bool PathFound { get; set; }

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
        public bool IsChecked { get; set; }

        public AstarNode Previous;
        public (int x, int y) Pos { get; private set; }
        public (int x, int y) EndNodePos { get; set; }

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

        public AstarNode((int x, int y) pos)
        {
            Pos = pos;
        }

        public void Select()
        {
            IsChecked = true;
            CheckAround();
        }
        public void Reset()
        {
            Gcost = 0;
            Hcost = 0;
            IsChecked = false;
            PathFound = false;
        }

        public List<AstarNode> SetEndPath()
        {
            var lst = new List<AstarNode>();
            PathFound = true;

            if (Previous != null)
			{
				lst.Add(Previous);
				lst.AddRange(Previous.SetEndPath());
			}
            return lst;
        }
        public List<AstarNode> GetEndPath()
        {
            var lst = new List<AstarNode>();

            if (Previous != null)
            {
                lst.Add(Previous);
				lst.AddRange(Previous.SetEndPath());
			}
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
                AstarNode node = NodesAround[i];
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