using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Models
{
    public class Robot : Model3D, IUpdatable
    {
        private Point _desiredPoint;
        public Point desiredPoint { get { return _desiredPoint; } }
        private Point _currentPoint;
        public Point currentPoint { get { return _currentPoint; } }
        private List<Point> route = new List<Point>();
        private List<RobotTask> tasks = new List<RobotTask>();
        private Chest _chest;
        public Chest chest { get { return _chest; } }
        public Robot(decimal x, decimal y, decimal z, decimal rotationX, decimal rotationY, decimal rotationZ)
        {
            this.type = "robot";
            this.guid = Guid.NewGuid();

            this._x = x;
            this._y = y;
            this._z = z;

            this._rX = rotationX;
            this._rY = rotationY;
            this._rZ = rotationZ;

            this._chest = null;
        }

        public Robot(Point point)
        {
            this.type = "robot";
            this.guid = Guid.NewGuid();

            this._x = point.x;
            this._y = point.y;
            this._z = point.z;

            _currentPoint = point;
            this._desiredPoint = _currentPoint;
            route.Add(_currentPoint);

            this._chest = null;
        }

        public void AddTask(RobotTask task)
        {
            tasks.Add(task);
        }
        public void MoveOverPath(Graph pointGraph, Point point)
        {
            //calculates the route the chest takes with the dijkstra algorithm and then proceeds to use it in RobotMove.
            _desiredPoint = point;
            if (route.Count == 1)
            {
                route = DijkstraClass.Dijkstra(pointGraph, currentPoint, desiredPoint);
            }
            if (IsOnPoint(this, route[1]))
            {
                this.CurrentPoint(route[1]);
                if (route.Count != 1)
                {
                    route.RemoveAt(0);
                }
            }
        }

        public void Move(Point point)
        {
            if (this.x < point.x)
            {
                this.Move(this.x + 0.1m, this.y, this.z);
            }
            else if (this.x > point.x)
            {
                this.Move(this.x - 0.1m, this.y, this.z);
            }

            if (this.z < point.z)
            {
                this.Move(this.x, this.y, this.z + 0.1m);
            }
            else if (this.z > point.z)
            {
                this.Move(this.x, this.y, this.z - 0.1m);
            }
        }

        public bool IsOnPoint(Robot robot, Point point)
        {
            if (robot.x == point.x)
            {
                if (robot.y == point.y)
                {
                    if (robot.z == point.z)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public void AssignPoint(Point point)
        {
            _desiredPoint = point;
            needsUpdate = true;
        }

        public void CurrentPoint(Point point)
        {
            _currentPoint = point;
        }

        public void AssignChest(Chest chest)
        {
            if (chest.point == this.currentPoint)
            {
                chest.AssignPoint(null);
                this._chest = chest;
                _chest.Move(this._x, this._y + 1, this._z);
                needsUpdate = true;
            }
        }

        public void RemoveChest(Boat boat)
        {
            boat.AddChest(this.chest);
            this._chest = null;
        }

        public void RemoveChest(Point point)
        {
            point.AddChest(this.chest);
            this._chest.AssignPoint(point);
            this._chest = null;
        }

        public override bool Update(int tick)
        {
            if (tasks != null && tasks.Any())
            {
                if (tasks.First().TaskComplete(this))
                {
                    tasks.RemoveAt(0);

                    if (tasks.Count == 0)
                    {
                        tasks = null;
                    }
                }
                if (tasks != null)
                {
                    tasks.First().StartTask(this);
                }
            }
            if (route.Count > 1)
            {
                //moves the robot with the chest
                this.Move(route[1]);
                if (this.chest != null)
                {
                    _chest.Move(this._x, this._y + 1, this._z);
                }
            }
            return base.Update(tick);
        }
    }
}