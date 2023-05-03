using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


////namespace Exam__SuperSooter
////{
////    class Program
////    {
////        static void Main(string[] args)
////        {
////        }
////    }
////}


namespace Exam__SuperSooter
{
    enum ETeam { GoodDude, BadDude, NoOne };
    enum EMode { Walk, Turn, Fight };

    class Point
    {
        public readonly int Y, X;
        public Point ( int Y_, int X_ )
        {
            Y = Y_;
            X = X_;
        }

        public override bool Equals(object obj)
        {
            Point Pt_ = obj as Point;

            if (Pt_ == null)
                return false;

            return this.Y == Pt_.Y && this.X == Pt_.X;
        }
    }

    interface Enemy
    {
        Point GetLocation();
        bool IsKilled();
        void Hit(int damage = 1);
    }

    interface IDude
    {
        void SingleAction();
    }

    class Dude : Enemy, IDude
    {
        public static char cGoodDude;
        public static char cBadDude;

        protected static readonly double DeltaAngle;

        protected static Random Rnd = new Random();

        protected int y;
        protected int x;

        protected char prevC;
        protected readonly Field fld;
        protected readonly GameManager m_GameManager;
        
        EMode mode;
        protected readonly ETeam team;

        protected int health = 5;
        protected int ReloadTime = 0;
        private Enemy m_Enemy = null;
        
        public ETeam Team
        {
            get{return team;}
        }

        private readonly char cFace;
        private readonly ConsoleColor color;

        private int CurrPathIndex = 0;
        private int TurningSteps;
        private int prevDy, prevDx;

        private readonly Rect[] Path = null;

        static Dude()
        {
            cGoodDude = char.ConvertFromUtf32(1)[0];
            cBadDude  = char.ConvertFromUtf32(2)[0];

            DeltaAngle = 1 * Math.PI / 180;
        }
        public Dude(GameManager GameManager_, Field fld_, ETeam team_, int y_, int x_  )
        {
            y = y_;
            x = x_;
            prevC = ' ';
            m_GameManager = GameManager_;
            fld = fld_;
            team = team_;

            if( ETeam.GoodDude == team )
            {
                cFace = cGoodDude;
                Path  = fld.PathForGood;
                color = ConsoleColor.Green;
            }
            else{
                cFace = cBadDude;
                Path = fld.PathForBad;
                color = ConsoleColor.Red;
            }

            m_Enemy = null;

            
        }

        public void SingleAction()
        {
            if (CurrPathIndex == Path.Length)
            {
                m_GameManager.Victory(team);
                return;
            }
            switch( mode )
            {
                case EMode.Walk: Walk();
                    break;
                case EMode.Turn: Turn();
                    break;
                case EMode.Fight: Fight();
                    break;
            }
        }

        public bool IdentifiedByCoors(int y_, int x_)
        {
            return y == y_ && x == x_;
        }

        private void Turn()
        {
            if (TurningSteps > 0)
            {
                --TurningSteps;
                MakeStep(prevDy, prevDx);
            }
            else
            {
                ++CurrPathIndex;
                mode = EMode.Walk;
            }
        }
        private void Walk()
        {
            m_Enemy = LookForEnemies();
            
            if( m_Enemy != null )
            {
                mode = EMode.Fight;
                return;
            }
            else
            {
                if( CurrPathIndex < Path.Length && IsNextPointReached() )
                {
                    mode = EMode.Turn;
                    TurningSteps = Rnd.Next(2, 4);
                    return;
                    //++CurrPathIndex;
                }
                if( CurrPathIndex < Path.Length )
                {
                    int dy, dx;
                    MakeCurrDirection(out dy, out dx);
                    prevDy = dy; prevDx = dx;

                    MakeStep( dy, dx);
                }
            }
        }
        private void Fight()
        {
            if (ReloadTime-- > 0) return;

            ReloadTime = 15;

            if (m_Enemy == null || m_Enemy.IsKilled( ))
            {
                m_Enemy = null;
                mode = EMode.Walk;
                return;
            }

            Point Pt = m_Enemy.GetLocation();

            new Bullet(m_GameManager, fld, team, y, x, Pt.Y, Pt.X);

            fld.Step(y, x, cFace, y, x, cFace, color);
        }

        //Enemy:
        public Point GetLocation()
        {
            return new Point( y, x );
        }
        public bool IsKilled()
        {
            return !(health > 0);
        }
        public void Hit(int damage ) 
        {
            health -= damage;

            if (IsKilled())
            {
                fld.Step(y, x, prevC, y, x, prevC, color);
            }
        }
        //

        //вспомогательные ф-ции ходьбы
        private bool IsNextPointReached()
        {
            return Path[ CurrPathIndex ].IsPointInside( new Point( y, x ) );
        }
        private void MakeStep(int dy, int dx)
        {
            //int dy, dx;
            //MakeCurrDirection(out dy, out dx);

            //prevDy = dy; prevDx = dx;

            char cElem = fld.GetElem( y + dy, x + dx );

            if (cElem == cFace)
                StepAside(dy, dx);
            else if (cElem == ' ')
            {
                fld.Step(y, x, prevC, y + dy, x + dx, cFace, color);

                prevC = cElem;
                y += dy;
                x += dx;
            }
        }
        private void StepAside(int dy, int dx)
        {
            int temp = dy;
            dy = dx;
            dx = temp;

            char cElem = fld.GetElem( y + dy, x + dx );
            if (cElem != ' ')
            {
                dy = -dy;
                dx = -dx;
                
                cElem = fld.GetElem( y + dy, x + dx );
                if (cElem != ' ')
                    return;
            }


            fld.Step(y, x, prevC, y + dy, x + dx, cFace, color);

            prevC = cElem;
            y += dy;
            x += dx;
        }
        private void MakeCurrDirection( out int dy, out int dx )
        {
            Point CurrDestPt = Path[ CurrPathIndex ].CenterPoint();

            int deltaY = CurrDestPt.Y - y;
            int deltaX = CurrDestPt.X - x;

            if( Math.Abs( deltaY ) < Math.Abs( deltaX ) )
            {
                dy = 0;
                dx = deltaX / Math.Abs( deltaX );
            }
            else
            {
                dy = deltaY / Math.Abs( deltaY );
                dx = 0;
            }
        }

        //вспомогательные ф-ции поиска врага
        private Enemy LookForEnemies()
        {
            double angle = - Math.PI / 2;

            while (angle < Math.PI/ 2 )
            {
                double tg = Math.Tan( angle );
                tg = Math.Tan(angle);  
                if (Math.PI % angle == 0)
                    tg = 1000000;
                else
                    tg = Math.Tan(angle);

                Enemy enemy = null;
                
                enemy = Math.Abs(tg) > 1 ? LookByY(tg, 1): LookByX( tg, 1 ) ; 
               
                if( enemy == null )
                    enemy = Math.Abs(tg) > 1 ? LookByY(tg, -1) : LookByX(tg, -1); 

                if (enemy != null)
                    return enemy;

                angle += DeltaAngle;
            }
            return null;
        }
        private bool IsEnemyDude(char cElem)
        {
            if( cElem == cBadDude && cFace == cGoodDude )
                return true;

            else if( cElem == cGoodDude && cFace == cBadDude )
                return true;

            return false;
        }
        private Enemy LookByY(double tg, int delta)
        {
            if (delta == 0) return null;

            delta = delta < 0 ? -1 : 1;

            int tempY = 0;
            int tempX = 0;
            char cElem = fld.cWall;

            do
            {
                tempY += delta;
                tempX = (int)(tempY / tg);
                cElem = fld.GetElem(y + tempY, x + tempX);
            } while (cElem == ' ' && cElem != '*');

            Enemy enemy = null;
            if (IsEnemyDude(cElem))
                enemy = m_GameManager.GetThisDude(new Point(y + tempY, x + tempX));

            return enemy;
        }
        private Enemy LookByX(double tg, int delta)
        {
            if (delta == 0) return null;

            delta = delta < 0 ? -1 : 1;

            int tempY = 0;
            int tempX = 0;
            char cElem = fld.cWall;

            do
            {
                tempX += delta;

                tempY = (int)(tempX * tg);
                cElem = fld.GetElem(y + tempY, x + tempX);
            } while (cElem == ' '&& cElem !='*');

            Enemy enemy = null;
            if (IsEnemyDude(cElem))
                enemy = m_GameManager.GetThisDude(new Point(y + tempY, x + tempX));

            return enemy;
        }
        //вспомогательные ф-ции стрельбы
    }


}