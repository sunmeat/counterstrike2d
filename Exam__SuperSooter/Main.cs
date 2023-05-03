using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace Exam__SuperSooter
{
    public class Win32Interop
    {
        [DllImport("crtdll.dll")]
        public static extern int _kbhit();
    }

    class DudeFinder
    {
        private Point Pt;
        public DudeFinder( Point Pt_ )
        {
            Pt = Pt_;
        }
        public bool CoordsCoincide( IDude dude )
        {
            Enemy e = dude as Enemy;
            if (e != null)
                return Pt.Equals(e.GetLocation());
            else
                return false;
            
        }
    }

    class GameManager
    {
        private bool bGameOn = true;
        private ETeam victor = ETeam.NoOne;
        private Field fld;
        private Hero m_Hero;
        private List<IDude> Dudes;
        private List<Bullet> Bullets;
        private List<Bullet> BulletsToDelete;

        public GameManager()
        {
            fld = new Field();

            Bullets = new List<Bullet>();
            BulletsToDelete = new List<Bullet>();

            Dudes = new List<IDude>();

            for (int i = 1; i < 5; ++i)
                Dudes.Add(new Dude(this, fld, ETeam.GoodDude, 4 % i, i / 4));
            for (int i = 1; i < 5; ++i)
                Dudes.Add(new Dude(this, fld, ETeam.BadDude, Field.FieldH - 4 % i - 1, Field.FieldW - i / 4 - 1));

            m_Hero = new Hero(this, fld);
            Dudes.Add(m_Hero);
        }

        public void AddBullet( Bullet blt )
        {
            Bullets.Add(blt);
        }
        public void Run()
        {

            long DCount = 0;
            long BCount = 0;

            while (bGameOn)
            {
                if (Dudes.Count == 1 && !m_Hero.IsKilled()) break;

                if (Win32Interop._kbhit() != 0)
                {
                    ConsoleKeyInfo ki = Console.ReadKey(true);
                    MoveHero(ki);
                }

                if (DateTime.Now.Ticks - DCount > 1000000)
                {
                    DCount = DateTime.Now.Ticks;

                    foreach (IDude dude in Dudes)
                        dude.SingleAction();
                }
                if (DateTime.Now.Ticks - BCount > 500000)
                {
                    BCount = DateTime.Now.Ticks;
                    foreach (Bullet blt in Bullets)
                        blt.SingleAction();

                    foreach (Bullet blt in BulletsToDelete)
                        Bullets.Remove(blt);

                    BulletsToDelete.Clear();
                }
                
            }
            string sMessage;

            if (victor == ETeam.NoOne)
                sMessage = "Все умерли... кроме вас, а так не интересно!";
            else
                sMessage = "Победили " + (victor == ETeam.GoodDude ? "хорошие :-D!" : "плохие :-/!");
            Console.SetCursorPosition(Console.WindowWidth / 2 - sMessage.Length / 2, Console.WindowHeight / 2);

            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.DarkRed;

            Console.WriteLine(sMessage);

            Console.ReadKey();
        }
        public void Victory( ETeam team )
        {
            bGameOn = false;
            victor = team;
        }

        public Enemy GetThisDude(Point Pt)
        {
            DudeFinder DF = new DudeFinder( Pt );
            int index = Dudes.FindIndex(DF.CoordsCoincide);

            if (index == -1) return null;
            return Dudes[index] as Enemy;
        }
        public void HitSomething(Bullet blt, Point Pt)
        {
            Enemy dude = GetThisDude(Pt);


            if (dude != null)
            {
                dude.Hit();
                if (dude.IsKilled())
                    Dudes.Remove(dude as Dude);
            }
            else if (m_Hero.IsHeroHere(Pt))
                m_Hero.Hit(1);

            BulletsToDelete.Add(blt);
        }

        private void MoveHero(ConsoleKeyInfo ki)
        {
            if (m_Hero == null) return;
            if (ki.Key == ConsoleKey.DownArrow)
                m_Hero.Step(1, 0);
            else if (ki.Key == ConsoleKey.UpArrow)
                m_Hero.Step(-1, 0);
            else if (ki.Key == ConsoleKey.RightArrow)
                m_Hero.Step(0, 1);
            else if (ki.Key == ConsoleKey.LeftArrow)
                m_Hero.Step(0, -1);
            else if (ki.Key == ConsoleKey.Spacebar)
                m_Hero.Shoot();
        }

    }

    class Program
    {
        private static Field fld;

        private static bool bGameOn = true;
        
        static void Main(string[] args)
        {
            Console.Title = "Super Shooter!";

            Console.SetWindowSize(Field.FieldW, Field.FieldH + 1);

            Console.SetBufferSize(Field.FieldW, Field.FieldH + 1);

            Console.CursorVisible = false;

            while (true)
            {

                GameManager GM = new GameManager();

                GM.Run();

                string sQuery = "Чтобы продолжить нажмите \"y\"";

                Console.SetCursorPosition(Console.WindowWidth / 2 - sQuery.Length / 2, Console.WindowHeight / 2 + 2);
                Console.WriteLine(sQuery);
                ConsoleKeyInfo ki = Console.ReadKey();

                if (ki.KeyChar != 'y') break;

                Console.BackgroundColor = ConsoleColor.Black;
            }


        }
    }



    class Hero: Enemy, IDude
    {
        private static Random Rnd = new Random();
        private int y = 0, prev_dy;
        private int x = 0, prev_dx;
        private readonly char cFace = Dude.cGoodDude;
        private readonly ConsoleColor color;
        private readonly Field fld;
        private int health = 5;
        GameManager m_GameManager;


        public Hero(GameManager GameManager_, Field Fld_)
        {
            m_GameManager = GameManager_;
            fld = Fld_;

            y = x = 1;

            color = ConsoleColor.Cyan;
            fld.Step(y, x, ' ', y, x, cFace, color);
            

            prev_dy = 1;
            prev_dx = 0;

        }

        public void SingleAction()
        {

        }

        public void Step(int dy, int dx)
        {
            if (!(health > 0)) return;
            prev_dy = dy;
            prev_dx = dx;

            char cElem = fld.GetElem( y + dy, x + dx );

            if (cElem != ' ')
                return;
            else
            {
                fld.Step(y, x, ' ', y + dy, x + dx, cFace, color);

                y += dy;
                x += dx;
            }
        }
        public void Shoot()
        {
            if (!(health > 0)) return;
            new Bullet(m_GameManager, fld, ETeam.GoodDude, y + prev_dy, x + prev_dx, y + 2 * prev_dy, x + 2 * prev_dx);
            
        }

        public void Hit(int damage)
        {
            health -= damage;
            if (!(health > 0)) fld.Step(y, x, ' ', y, x, ' ', color);
        }
        public Point GetLocation()
        {
            return new Point( y, x );
        }
        public bool IsKilled()
        {
            return !(health > 0);
        }


        public bool IsHeroHere(Point pt)
        {
            return pt.Y == y && pt.X == x;
        }
    }


}
