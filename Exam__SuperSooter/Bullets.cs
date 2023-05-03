using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exam__SuperSooter
{
    class Bullet
    {
        //enum ECoorsIncr { ByY, ByX };

        private readonly Field fld;
        private readonly GameManager m_GameManager;
        private readonly ETeam team;

        private readonly ConsoleColor color;

        private int y1, x1, y2, x2;
        private int y, x;
        private int PrevY, PrevX;
        //private double tg;
        //int delta;
        private char cBullet;



        public Bullet(GameManager GameManager_, Field fld_, ETeam team_, int y1_, int x1_, int y2_, int x2_)
        {
            m_GameManager   = GameManager_;
            fld             = fld_;
            team            = team_;
            y1 = y1_;
            x1 = x1_;
            y2 = y2_;
            x2 = x2_;
            y = y1;
            x = x1;

            NewPosition();
            
            cBullet = '*';

            color = ETeam.GoodDude == team ? ConsoleColor.Green : ConsoleColor.Red;

            m_GameManager.AddBullet(this);
        }

        public void SingleAction()
        {
            PrevY = y; PrevX = x;
            NewPosition();

            char cElem = fld.GetElem(y, x);

            if (cElem != ' ' && cElem != cBullet)
            {
                m_GameManager.HitSomething(this, new Point(y, x));
                fld.Step(PrevY, PrevX, ' ', PrevY, PrevX, ' ', color);
            }
            else
                fld.Step(PrevY, PrevX, ' ', y, x, cBullet, color);
        }

       
        public bool CoordsCoincide(Point Pt)
        {
            return y1 == Pt.Y && x1 == Pt.X;
        }
        private void NewPosition()
        {
            double tg = (double)( y2 - y1 ) / (x2 - x1);


            if (Math.Abs(tg) < 1)
            {
                int delta = (x2 < x1) ? -1 : 1; 
                x += delta;
                y = (int)((x - x1) * tg) + y1;
            }
            else
            {
                int delta = (y2 < y1) ? -1 : 1;
                y += delta;
                x = (int)((y - y1) / tg) + x1;
            }
        }
    }
}
