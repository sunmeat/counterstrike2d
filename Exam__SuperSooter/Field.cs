using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exam__SuperSooter
{
    class Field
    {
        static private Random Rnd = new Random();

        private Graph m_Graph;
        private int SWATLocationGrInd = 0;
        private int TerrorsLocationGrInd = 0;

        public const int FieldW = 80;
        public const int FieldH = 40;

        public readonly char cWall;

        private readonly char[,] aField;

        public readonly Rect[] PathForGood = null;
        public readonly Rect[] PathForBad = null;


        

        public Field()
        {
            m_Graph = new Graph();
            aField = new char[FieldH, FieldW];
            
            cWall = Convert.ToChar(0x2593);
            
            for (int i = 0; i < aField.GetLength(0); ++i)
                for (int j = 0; j < aField.GetLength(1); ++j)
                    aField[i, j] = cWall;

            SetTunnels();

            PathForGood = m_Graph.MakePath(SWATLocationGrInd, TerrorsLocationGrInd);
            PathForBad = new Rect[ PathForGood.Length ];
            
            for (int i = 0; i < PathForGood.Length; ++i)
                PathForBad[i] = PathForGood[i];

            Array.Reverse(PathForBad);// т.к. плохие парни идут с другой стороны
            //PathForBad = PathForBad.Reverse(); 

            InitDraw();
        }

        public char GetElem(int y, int x)
        {
            if (y < 0 || y >= FieldH ||
                x < 0 || x >= FieldW)
            {
                return cWall;
            }
            return aField[y, x];
        }
        public bool Step(int prevY, int prevX, char PrevC, int y, int x, char c, ConsoleColor color)
        {
            if (y < 0 || y >= FieldH ||
                x < 0 || x >= FieldW)
                return false;

            aField[prevY, prevX] = PrevC;
            aField[y, x] = c;

            Console.ForegroundColor = color;

            Console.SetCursorPosition(prevX, prevY);

            Console.Write(PrevC);


            Console.SetCursorPosition(x, y);


            Console.Write(c);
            Console.ForegroundColor = ConsoleColor.White;

            return true;
        }

        //public void GameOver(string msg)
        //{
        //    Console.SetCursorPosition(14, 27);
        //    Console.Write(msg);
        //}

        private void InitDraw()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            //попробовать вывести aField.ToString();

            for (int i = 0; i < FieldH; ++i)
                for (int j = 0; j < FieldW; ++j)
                {
                    Console.SetCursorPosition(j, i);
                    Console.Write(aField[i, j]);
                }
            Console.ForegroundColor = ConsoleColor.White;
        }

        //дальше идут функции, строящие случайный лабиринт и
        //формирующие граф,
        //их дофига...
        private void SetTunnels()
        {
            SWATLocationGrInd = MakeEndGraph( 2, 2, 1, 1 );
            TerrorsLocationGrInd = MakeEndGraph(FieldW - 1 - 2,FieldH - 1 - 2, -1, -1);
            int randX = 0;
            int randY = 0;
            int nCrosses = 0;
            while (nCrosses < 10)
            {
                Rect Rc;
                if (MakeGoodCrossPoint(ref randY, ref randX, out Rc))
                {
                    int InitIndex = m_Graph.AddVertex(Rc);
                    int randNo = Rnd.Next(4);

                    MakeEmptySpaceAt(randY, randX);
                    if (randNo != 0)
                        DigTunnelInDirection(randY, randX, 1, 0, InitIndex);
                    if (randNo != 1)
                      DigTunnelInDirection(randY, randX, -1, 0, InitIndex);
                    if (randNo != 2)
                        DigTunnelInDirection(randY, randX, 0, 1, InitIndex);
                    if (randNo != 3)
                        DigTunnelInDirection(randY, randX, 0, -1, InitIndex);

                        
                    ++nCrosses;
                }
                else 
                    break;

            }


        }

        private int MakeEndGraph(int x, int y, int dy, int dx)
        {
            MakeEmptySpaceAt(y, x);
            int RetIndex = m_Graph.AddVertex(new Rect(y-2, x-2, 5, 5));

            int tempY = y;
            for (int i = 0; i < FieldH / 2; ++i)
            {
                DigOneStep(tempY, x, dy, 0);
                tempY += dy;
            }

            int NextIndex = m_Graph.AddVertex(new Rect(tempY - 2, x - 2, 5, 5));
            m_Graph.AddRib(new Rib(RetIndex, NextIndex, FieldH / 2 - 4));
            MakeEmptySpaceAt(tempY, x);

            int tempX = x;
            for (int i = 0; i < FieldW / 2; ++i)
            {
                DigOneStep(y, tempX, 0, dx);
                tempX += dx;
            }

            NextIndex = m_Graph.AddVertex(new Rect(y - 2, tempX - 2, 5, 5));
            m_Graph.AddRib(new Rib(RetIndex, NextIndex, FieldW / 2 - 4));
            MakeEmptySpaceAt(y, tempX);

            return RetIndex;
        }
        private void MakeEmptySpaceAt( int y, int x )
        {
            for( int i = 0; i < 5; ++i )
                for (int j = 0; j < 5; ++j)
                    aField[y - 2 + i, x - 2 + j] = ' ';
        }
        private void DigTunnelInDirection(int y, int x, int dy, int dx, int InitIndex)
        {
            int weight = 0;
            while(IsPointOnEmpty(y,x))
            {
                y += dy;
                x += dx;
                ++weight;
            }
            
            while ( IsGoodPointForTunnel(y, x, dy, dx) )
            {
                DigOneStep(y, x, dy, dx);
                y += dy;
                x += dx;
                ++weight;
            }
            Rect Rc;
            if (IsWithinField(y, x))//если вышли на тоннель
            {
                y += (dy * 2);
                x += (dx * 2);
                Rc = new Rect(y - 2, x - 2, 5, 5);

                int NewIndex = m_Graph.AddVertex(Rc);
                m_Graph.AddRib(new Rib(InitIndex, NewIndex, weight ));

                SearchTwoGraphs(y, x, dy, dx, NewIndex);
            }
            else//значит вышли за пределы поля
            {
                y -= (dy * 2);
                x -= (dy * 2);
                Rc = new Rect(y - 2, x - 2, 5, 5);
                //weight -= 3;

                int NewIndex = m_Graph.AddVertex(Rc);
                m_Graph.AddRib(new Rib(InitIndex, NewIndex, weight ));
            }
            if (weight < 0) throw new Exception("вес ребра < 0 ");
        }

        private bool IsGoodPointForTunnel(int y, int x, int dy, int dx)
        {
            return  IsPointOnWall(y, x)                         &&
                    IsPointOnWall(y - dx, x - dy)               &&
                    IsPointOnWall(y + dx, x + dy)               &&
                    IsPointOnWall(y - (dx * 2), x - (dy * 2))   &&
                    IsPointOnWall(y + (dx * 2), x + (dy * 2));
        }
        
        private void DigOneStep(int y, int x, int dy, int dx)
        {
            aField[y, x] = ' ';
            aField[y - dx, x - dy] = ' ';
            aField[y + dx, x + dy] = ' ';
            aField[y - (dx * 2), x - (dy * 2)] = ' ';
            aField[y + (dx * 2), x + (dy * 2)] = ' ';
        }
        private void SearchTwoGraphs(int y, int x, int dy, int dx, int InitIndex)
        {
            //здесь dy dx - это перпендикулярное направление нашего тоннеля, меняем их местами
            int temp = dy;
            dy = dx;
            dx = temp;

            int tempY = y;
            int tempX = x;
            int weight = 0;

            //ищем граф в одну сторону
            while (IsWithinField(tempY, tempX))
            {
                tempY += dy;
                tempX += dx;
                ++weight;
                int index = m_Graph.GetIndexOfVertex(tempY, tempX);
                if ( index != -1 && index != InitIndex )
                {
                    m_Graph.AddRib(new Rib(InitIndex, index, weight));
                    break;
                }
            }

            tempY = y;
            tempX = x;
            weight = 0;

            //ищем граф в другую сторону
            while (IsWithinField(tempY, tempX))
            {
                tempY -= dy;
                tempX -= dx;
                ++weight;
                int index = m_Graph.GetIndexOfVertex(tempY, tempX);
                if (index != -1 && index != InitIndex )
                {
                    m_Graph.AddRib(new Rib(InitIndex, index, weight));
                    break;
                }
            }
        }
        private bool MakeGoodCrossPoint(ref int randY, ref int randX, out Rect Rc)
        {
            int tries = 0;
            do
            {
                if (tries > 200) 
                {
                    Rc = new Rect(-1000, -1000, -1000, -1000 ); // умышленно бредовый Rect
                    return false;//точка не найдена
                }
                ++tries;
                randY = Rnd.Next(6, FieldH - 5);
                randX = Rnd.Next(6, FieldW - 5);

                if (aField[randY, randX] != cWall)
                    continue;
                if (LengthToEmpty(randY, randX, 1, 0) <  8)
                    continue;                            
                if (LengthToEmpty(randY, randX, -1, 0) < 8)
                    continue;                            
                if (LengthToEmpty(randY, randX, 0, 1) <  8)
                    continue;                            
                if (LengthToEmpty(randY, randX, 0, -1) < 8)
                    continue;

                break;
            } while (true);

            //создаем Rect для графа
            Rc = new Rect(randY - 2, randX - 2, 5, 5);
            //точка найдена
            return true;
        }
        //если x,y внутри поля - true
        private bool IsWithinField(int y, int x)
        {
            return y >= 0 && y < FieldH &&
                   x >= 0 && x < FieldW;
        }
        //если x,y внутри поля и на стене - true
        private bool IsPointOnWall(int y, int x)
        {
            return IsWithinField( y, x ) &&
                   aField[y, x] == cWall;
        }
        //если x,y внутри поля и на пустом месте - true
        private bool IsPointOnEmpty(int y, int x)
        {
            return IsWithinField(y, x) &&
                   aField[y, x] == ' ';
        }
        //проверяет, упирается ли направление в другой граф
        private bool IsGraphClose(int y, int x, int dy, int dx)
        {
            return(  m_Graph.GetIndexOfVertex(y, x) != -1                   ||                     //если точка на уровне с 
                     m_Graph.GetIndexOfVertex(y - dx, x - dy) != -1         ||           //существующим графом - нафиг
                     m_Graph.GetIndexOfVertex(y + dx, x + dy) != -1         ||
                     m_Graph.GetIndexOfVertex(y - (dx*2), x - (dy*2)) != -1 ||  
                     m_Graph.GetIndexOfVertex(y + (dx*2), x + (dy*2)) != -1 || 
                     m_Graph.GetIndexOfVertex(y - (dx*3), x - (dy*3)) != -1 ||  
                     m_Graph.GetIndexOfVertex(y + (dx*3), x + (dy*3)) != -1 
                     );
        }
        //расстояние до тоннеля от точки по заданному направлению
        private int LengthToEmpty(int y, int x,int dy, int dx)
        {
            int len;
            for ( len = 1; ; ++len)
            {
                if (IsGraphClose(y, x, dy, dx)) //уперлись в граф, мне такого не надо
                    return 0;                   //

                if ( ! IsGoodPointForTunnel( y, x, dy, dx ))
                    break;
                y += dy;
                x += dx;
            }
            return len;
        }

        
    }
}
