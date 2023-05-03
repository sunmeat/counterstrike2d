using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exam__SuperSooter
{

    class Rect
    {
        public Rect( int y, int x, int h, int w )
        {
            Y = y;
            X = x;
            H = h;
            W = w;
        }

        public bool IsPointInside(Point Pt)
        {
            return Pt.Y >= Y && Pt.Y < (Y + H) &&
                   Pt.X >= X && Pt.X < (X + W);
        }
        public Point CenterPoint()
        {
            return new Point(Y + H / 2, X + W / 2);
        }

        public int Y { get; private set; }
        public int X { get; private set; }
        public int H { get; private set; }
        public int W { get; private set; }
    }
    class Rib
    {
        public Rib(int first_, int last_, int weigth_)
        {
            first   = first_;
            last    = last_;
            weight = weigth_;
        }
        public int first   { get; private set; }
        public int last    { get; private set; }
        public int weight  { get; private set; }

    }
    class RectFinder
    {
        public RectFinder(int y_, int x_)
        {
            y = y_;
            x = x_;
        }
        public bool InsideRect(Rect Rc)
        {
            return (x >= Rc.X && x < Rc.X + Rc.W &&
                    y >= Rc.Y && y < Rc.Y + Rc.H   );
        }

        private int y, x;
    }

    class WayElem
    {
        //public WayElem( bool bExist_ )
        public bool bExist = false;
        public int IndPrevVertex = -10;
        public int SumDist = 0;
    }
    struct StateKeeper
    {
        public int index;
        public int Intermediate;
        public StateKeeper(int index_, int Intermediate_)
        {
            index = index_;
            Intermediate = Intermediate_;
        }
    }
    class Graph
    {
        public int start = 0, finish = 0;
        public Rect[] MakePath( int startInd, int finishInd )
        {
            if( startInd < 0 || finishInd < 0 ||
                startInd >= wayelems.Count    ||
                finishInd >= wayelems.Count )
                return new Rect[0];

            start = startInd;
            finish = finishInd;
            //for( int i = 0; i < wayelems.Count; ++i )
            //    wayelems[ i ].bExist = false;

            wayelems[ startInd ].bExist         = true;
            wayelems[ startInd ].SumDist        = 0;
            wayelems[ startInd ].IndPrevVertex  = -1;

            //try
            //{

                PassWay(startInd);
            //}
            //catch (System.StackOverflowException e)
            //{
            //    bool bGraphIsOk = false;
            //    return new Rect[0];
            //}

            if( ! wayelems[ finishInd ].bExist )
                return new Rect[0];
            else
            {
                List<Rect> ArrPath = new List<Rect>();

                int i = finishInd;
                do{
                    ArrPath.Add( vertexes[i] );
                    i = wayelems[i].IndPrevVertex;
                }while(i != -1 );

                if( i != -1 )
                    return new Rect[ 0 ]; //сюда попасть не сможем;

                ArrPath.Reverse();

                return ArrPath.ToArray();
            }

        }
        private void PassWay( int Intermediate )
        {
            Stack<StateKeeper> States = new Stack<StateKeeper>();

            //int i = 0;
            States.Push(new StateKeeper(0, Intermediate));//устанавливаем первый индекс

            do
            {
                StateKeeper state = States.Pop();

                int i = state.index;
                Intermediate = state.Intermediate;

                //if( Intermediate == finish )
                //    return;
                for (; i < ribs.Count; ++i)
                {
                    if (ribs[i].first == Intermediate)
                        if (ForStep(ribs[i].first, i, ribs[i].last))
                        {
                            States.Push(new StateKeeper(i, Intermediate));
                            States.Push(new StateKeeper(0, ribs[i].last));
                        }

                    if (ribs[i].last == Intermediate)
                        if (ForStep(ribs[i].last, i, ribs[i].first))
                        {
                            States.Push(new StateKeeper(i, Intermediate));
                            States.Push(new StateKeeper(0, ribs[i].first));
                        }
                }
            } while (States.Count > 0);
        }
        private bool ForStep( int IndVert1, int IndArc, int IndVert2 )
        {
            int NewDist = ribs[ IndArc ].weight + wayelems[IndVert1].SumDist;

            if( !wayelems[ IndVert2 ].bExist )
            {
                wayelems[ IndVert2 ].bExist = true;
                wayelems[ IndVert2 ].IndPrevVertex = IndVert1;
                wayelems[ IndVert2 ].SumDist = NewDist;
                return true;
                //PassWay( IndVert2 );
            }
            else
            {
                if( wayelems[ IndVert2 ].SumDist > NewDist )
                {
                    wayelems[ IndVert2 ].SumDist = NewDist;
                    wayelems[ IndVert2 ].IndPrevVertex = IndVert1;
                    return true;
                    //PassWay( IndVert2 );
                }
            }
            return false;
        }


        public int GetIndexOfVertex(int y, int x)
        {
            RectFinder RcFinder = new RectFinder(y, x);
            int index = vertexes.FindIndex(RcFinder.InsideRect);

            return index;// == -1 ? false : true;
        }

        public void Reset()
        {
            vertexes.Clear();
            ribs.Clear();
            wayelems.Clear();
        }

        public int AddVertex(Rect Rc)
        {
            vertexes.Add(Rc);
            wayelems.Add(new WayElem() );

            return vertexes.Count - 1;            
        }
        public void AddRib(Rib Rb)
        {
            ribs.Add(Rb);
        }
        private List<WayElem> wayelems = new List<WayElem>();
        private List<Rect> vertexes = new List<Rect>();
        public List<Rect> Vertexes
        {
            get { return vertexes; }
            private set{}
        }

        private List<Rib> ribs = new List<Rib>();
        public List<Rib> Ribs
        {
            get { return ribs; }
            private set {}// { ribs = value; }
        }
        
    }
}
