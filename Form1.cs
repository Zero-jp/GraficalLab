using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace GraficalLab
{
    public partial class Form1 : Form
    {
        /* Общте моменты */
        Point startPoint;// точка старта
        Rectangle rect;
        SolidBrush pen = new SolidBrush(Color.Black);// кисть
        List<PointF> pointList = new List<PointF>(); // Исходный массив точек для кривой Бизье
        public Form1()
        {
            InitializeComponent();
            this.groupBox1.Visible = false;
        }

        private void Plot(Point point)
        /* Функция рисования пикселя */
        {
            int pixelSize = 2;
            Graphics gr = this.CreateGraphics();
            PaintEventArgs paintEvArgs = new PaintEventArgs(gr, this.ClientRectangle);
            paintEvArgs.Graphics.FillRectangle(pen, point.X, point.Y, pixelSize, pixelSize);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        /* Сохранение координат точки старта. */
        {
            startPoint.X = e.X;
            startPoint.Y = e.Y;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        /* Передача координат точек старта и конца соответсвующему методу. */
        {
            Point endPoint = new Point(e.X, e.Y);
            if (this.comboBox1.SelectedItem.ToString() == "Отрезок (DDA-отрезок)")
            {
                DigitalDifferencialAnalizatorAlg(startPoint, endPoint);
                this.groupBox1.Visible = false;
            }
            if (this.comboBox1.SelectedItem.ToString() == "Отрезок (Брезенхем)")
            {
                BrezenkhemAlgLine(startPoint, endPoint);
                this.groupBox1.Visible = false;
            }
            if (this.comboBox1.SelectedItem.ToString() == "Отрезок (Нецелочисленные координаты)")
            {
                this.groupBox1.Visible = true;
            }
            if (this.comboBox1.SelectedItem.ToString() == "Окружность")
            {
                BrezenkhemAlgCircle(startPoint, (int)Math.Sqrt(Math.Pow(e.X - startPoint.X, 2) + Math.Pow(e.Y - startPoint.Y, 2)));
                this.groupBox1.Visible = false;
            }
            if (this.comboBox1.SelectedItem.ToString() == "Кривая Безье")
            {
                if (pointList.Count == 10)
                    return;
                pointList.Add(new PointF(e.X, e.Y));
                DecasteldjoAlgBusier();
                this.groupBox1.Visible = false;
            }
            if (this.comboBox1.SelectedItem.ToString() == "Алгоритм средней точки")
            {
                MidpointAlg(startPoint, new Point(e.X, e.Y));
                this.groupBox1.Visible = false;
            }
            if (this.comboBox1.SelectedItem.ToString() == "Отсечение Коэна-Сазерлэнда")
            {
                CohenSutherlandAlg(rect, new Point(startPoint.X, startPoint.Y), new Point(e.X, e.Y));
                this.groupBox1.Visible = false;
            }
            if (this.comboBox1.SelectedItem.ToString() == "Отсечение Кируса-Бека")
            {
                Polygon polygon = new Polygon();
                polygon.Add(new PointF(rect.Right, rect.Top));
                polygon.Add(new PointF(rect.Right, rect.Bottom));
                polygon.Add(new PointF(rect.Left, rect.Bottom));
                polygon.Add(new PointF(rect.Left, rect.Top));
                Segment seg = new Segment(new PointF(startPoint.X, startPoint.Y), new PointF(e.X, e.Y));
                if (polygon.CyrusBeckClip(ref seg))
                    BrezenkhemAlgLine(new Point((int)seg.A.X, (int)seg.A.Y), new Point((int)seg.B.X, (int)seg.B.Y));
                this.groupBox1.Visible = false;
            }
            if (this.comboBox1.SelectedItem == null)
            {
                return;
            }
        }
        private void NonIntCoord_Btn_Click(object sender, EventArgs e)
        /* Нажатие на кнопку отрисовки линии по нецелочисленным координатам */
        {
            NonIntegerAlg(float.Parse(textBox1.Text, CultureInfo.InvariantCulture.NumberFormat), float.Parse(textBox4.Text, CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(textBox2.Text, CultureInfo.InvariantCulture.NumberFormat), float.Parse(textBox3.Text, CultureInfo.InvariantCulture.NumberFormat));
        }
        private void DigitalDifferencialAnalizatorAlg(Point startCords, Point endCords)
        /* Отрисовка линии методом цифрового дифференциального анализатора */
        {
            /* Координаты конца отрезка(сдвинутые в начало координат) */
            float x = endCords.X - startCords.X,
                y = endCords.Y - startCords.Y;

            double err = Math.Abs((double)y / (double)x);// текущая погрешность по ординате
            double deltaErr;// величина приращения
            Point currentPoint = new Point(0, 0);// значение текущей точки, которое сдвинуто в начало координат
            int octantNum = 0;// номер октанта, в котором находится конечная точка

            /* Определение октанта */
            if (x >= 0 && y >= 0 && Math.Abs((double)y / (double)x) <= 1)
                octantNum = 1;
            if (x >= 0 && y >= 0 && Math.Abs((double)y / (double)x) > 1)
            {
                octantNum = 2;
                x = y;
                err = 1 / err;
            }
            if (x < 0 && y >= 0 && Math.Abs((double)y / (double)x) > 1)
            {
                octantNum = 3;
                x = y;
                err = 1 / err;
            }
            if (x < 0 && y >= 0 && Math.Abs((double)y / (double)x) <= 1)
                octantNum = 4;
            if (x < 0 && y < 0 && Math.Abs((double)y / (double)x) <= 1)
                octantNum = 5;
            if (x < 0 && y < 0 && Math.Abs((double)y / (double)x) > 1)
            {
                octantNum = 6;
                x = y;
                err = 1 / err;
            }
            if (x >= 0 && y < 0 && Math.Abs((double)y / (double)x) > 1)
            {
                octantNum = 7;
                x = y;
                err = 1 / err;
            }
            if (x >= 0 && y < 0 && Math.Abs((double)y / (double)x) <= 1)
                octantNum = 8;

            deltaErr = err;
            /* Процесс отрисовки линии */
            while (currentPoint.X < Math.Abs(x))
            {
                switch (octantNum)
                {
                    case 1:
                        {
                            Point point = new Point(currentPoint.X + startCords.X, currentPoint.Y + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 2:
                        {
                            Point point = new Point(currentPoint.Y + startCords.X, currentPoint.X + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 3:
                        {
                            Point point = new Point(-currentPoint.Y + startCords.X, currentPoint.X + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 4:
                        {
                            Point point = new Point(-currentPoint.X + startCords.X, currentPoint.Y + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 5:
                        {
                            Point point = new Point(-currentPoint.X + startCords.X, -currentPoint.Y + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 6:
                        {
                            Point point = new Point(-currentPoint.Y + startCords.X, -currentPoint.X + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 7:
                        {
                            Point point = new Point(currentPoint.Y + startCords.X, -currentPoint.X + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 8:
                        {
                            Point point = new Point(currentPoint.X + startCords.X, -currentPoint.Y + startCords.Y);
                            Plot(point);
                        }
                        break;
                }

                if (err > 1 / 2)
                {
                    // Диагональное смещение
                    currentPoint.X += 2;
                    currentPoint.Y += 2;

                    // т.к. произошло смещение по y на единицу вверх:
                    err += deltaErr - 1;
                }
                else
                {
                    // Горизонтальное смещение
                    currentPoint.X += 2;
                    err += deltaErr;
                }
            }
        }
        private void BrezenkhemAlgLine(Point startCords, Point endCords)
        /* Отрисовка линии методом Брезенхема */
        {
            /* Координаты конца отрезка(сдвинутые в начало координат) */
            int x = endCords.X - startCords.X,
                y = endCords.Y - startCords.Y;

            double err = Math.Abs(2 * y) - Math.Abs(x);// текущая погрешность по ординате
            double deltaErrDiagonal;// величина приращения по диагонали
            double deltaErrHorizontal;// величина приращения по горизонтали
            Point currentPoint = new Point(0, 0);// значение текущей точки, которое сдвинуто в начало координат
            int octantNum = 0;// номер октанта, в котором находится конечная точка

            if (x >= 0 && y >= 0 && Math.Abs((double)y / (double)x) <= 1)
                octantNum = 1;
            if (x >= 0 && y >= 0 && Math.Abs((double)y / (double)x) > 1)
            {
                octantNum = 2;
                err = Math.Abs(2 * x) - Math.Abs(y);
                x = y;
            }
            if (x < 0 && y >= 0 && Math.Abs((double)y / (double)x) > 1)
            {
                octantNum = 3;
                err = Math.Abs(2 * x) - Math.Abs(y);
                x = y;
            }
            if (x < 0 && y >= 0 && Math.Abs((double)y / (double)x) <= 1)
                octantNum = 4;
            if (x < 0 && y < 0 && Math.Abs((double)y / (double)x) <= 1)
                octantNum = 5;
            if (x < 0 && y < 0 && Math.Abs((double)y / (double)x) > 1)
            {
                octantNum = 6;
                err = Math.Abs(2 * x) - Math.Abs(y);
                x = y;
            }
            if (x >= 0 && y < 0 && Math.Abs((double)y / (double)x) > 1)
            {
                octantNum = 7;
                err = Math.Abs(2 * x) - Math.Abs(y);
                x = y;
            }
            if (x >= 0 && y < 0 && Math.Abs((double)y / (double)x) <= 1)
                octantNum = 8;

            deltaErrDiagonal = err - Math.Abs(x);
            deltaErrHorizontal = err + Math.Abs(x);

            while (currentPoint.X < Math.Abs(x))
            {
                switch (octantNum)
                {
                    case 1:
                        {
                            Point point = new Point(currentPoint.X + startCords.X, currentPoint.Y + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 2:
                        {
                            Point point = new Point(currentPoint.Y + startCords.X, currentPoint.X + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 3:
                        {
                            Point point = new Point(-currentPoint.Y + startCords.X, currentPoint.X + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 4:
                        {
                            Point point = new Point(-currentPoint.X + startCords.X, currentPoint.Y + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 5:
                        {
                            Point point = new Point(-currentPoint.X + startCords.X, -currentPoint.Y + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 6:
                        {
                            Point point = new Point(-currentPoint.Y + startCords.X, -currentPoint.X + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 7:
                        {
                            Point point = new Point(currentPoint.Y + startCords.X, -currentPoint.X + startCords.Y);
                            Plot(point);
                        }
                        break;
                    case 8:
                        {
                            Point point = new Point(currentPoint.X + startCords.X, -currentPoint.Y + startCords.Y);
                            Plot(point);
                        }
                        break;
                }

                if (err > 0)
                {
                    // Диагональное смещение
                    currentPoint.X += 2;
                    currentPoint.Y += 2;

                    // т.к. произошло смещение по y на единицу вверх:
                    err += deltaErrDiagonal;
                }
                else
                {
                    // Горизонтальное смещение
                    currentPoint.X += 2;
                    err += deltaErrHorizontal;
                }
            }
        }
        private void NonIntegerAlg(float startX, float startY, float finishX, float finishY)
        /* Отрисовка линии по нецелочисленным координатам */
        {
            // т.к. у нас начальная точка может не быть нулем, 
            // параллельным переносом сдвинуть наш будущий отрезок в начало координат
            int c = 1000;// масштабный коэффициент
            Point currentPoint = new Point(0, 0);// значение текущей точки сдвинуто в начало координат

            float delta_h, delta_v;//приращения на 1 пиксель при сдвиге на x,y

            int kvadr;//номер квадранта

            float x = finishX - startX,
                 y = finishY - startY;// координаты конца отрезка(сдвинутые к началу координат)

            if (y > 0)
            {
                if (x > 0)
                    kvadr = 4;
                else
                    kvadr = 3;
            }
            else
            {
                if (x > 0)
                    kvadr = 1;
                else
                    kvadr = 2;
            }

            double h = 0, v = 0;
            /* Проверка пораллельных случаев */
            if (x == 0)
            {
                while (Math.Abs(currentPoint.Y) <= Math.Abs(y))
                {
                    switch (kvadr)
                    {
                        case 1:
                            {
                                Point point = new Point(currentPoint.X + (int)Math.Round(startX), currentPoint.Y + (int)Math.Round(startY));
                                Plot(point);
                            }
                            break;
                        case 2:
                            {
                                Point point = new Point(-currentPoint.X + (int)Math.Round(startX), currentPoint.Y + (int)Math.Round(startY));
                                Plot(point);
                            }
                            break;
                        case 3:
                            {
                                Point point = new Point(-currentPoint.X + (int)Math.Round(startX), -currentPoint.Y + (int)Math.Round(startY));
                                Plot(point);
                            }
                            break;
                        case 4:
                            {
                                Point point = new Point(currentPoint.X + (int)Math.Round(startX), -currentPoint.Y + (int)Math.Round(startY));
                                Plot(point);
                            }
                            break;
                    }
                    currentPoint.Y--;
                }
                return;
            }// параллельно ОУ

            if (y == 0)
            {
                while (currentPoint.X <= Math.Abs(x))
                {
                    switch (kvadr)
                    {
                        case 1:
                            {
                                Point point = new Point(currentPoint.X + (int)Math.Round(startX), currentPoint.Y + (int)Math.Round(startY));
                                Plot(point);
                            }
                            break;
                        case 2:
                            {
                                Point point = new Point(-currentPoint.X + (int)Math.Round(startX), currentPoint.Y + (int)Math.Round(startY));
                                Plot(point);
                            }
                            break;
                        case 3:
                            {
                                Point point = new Point(-currentPoint.X + (int)Math.Round(startX), -currentPoint.Y + (int)Math.Round(startY));
                                Plot(point);
                            }
                            break;
                        case 4:
                            {
                                Point point = new Point(currentPoint.X + (int)Math.Round(startX), -currentPoint.Y + (int)Math.Round(startY));
                                Plot(point);
                            }
                            break;
                    }
                    currentPoint.X++;
                }
                return;
            }// параллельно ОХ

            delta_h = c / (Math.Abs(x));
            delta_v = c / (Math.Abs(y));

            /* Не пораллельный случай */
            while ((h < c) && (v < c))
            {
                switch (kvadr)
                {
                    case 1:
                        {
                            Point point = new Point(currentPoint.X + (int)Math.Round(startX), currentPoint.Y + (int)Math.Round(startY));
                            Plot(point);
                        }
                        break;
                    case 2:
                        {
                            Point point = new Point(-currentPoint.X + (int)Math.Round(startX), currentPoint.Y + (int)Math.Round(startY));
                            Plot(point);
                        }
                        break;
                    case 3:
                        {
                            Point point = new Point(-currentPoint.X + (int)Math.Round(startX), -currentPoint.Y + (int)Math.Round(startY));
                            Plot(point);
                        }
                        break;
                    case 4:
                        {
                            Point point = new Point(currentPoint.X + (int)Math.Round(startX), -currentPoint.Y + (int)Math.Round(startY));
                            Plot(point);
                        }
                        break;
                }

                if (h < v)
                {
                    // Сдвиг по горизонтали
                    currentPoint.X++;
                    h += delta_h;
                }
                else if (h > v)
                {
                    // Сдвиг по вертикали
                    currentPoint.Y--;
                    v += delta_v;
                }
                else
                {
                    h = v;//Вырожденный случай
                    switch (kvadr)
                    {
                        case 1:
                            {
                                Point point = new Point(currentPoint.X + (int)Math.Round(startX), currentPoint.Y + (int)Math.Round(startY) + 1);
                                Plot(point);
                            }
                            break;
                        case 2:
                            {
                                Point point = new Point(-currentPoint.X + (int)Math.Round(startX), currentPoint.Y + (int)Math.Round(startY) + 1);
                                Plot(point);
                            }
                            break;
                        case 3:
                            {
                                Point point = new Point(-currentPoint.X + (int)Math.Round(startX), -currentPoint.Y + (int)Math.Round(startY) + 1);
                                Plot(point);
                            }
                            break;
                        case 4:
                            {
                                Point point = new Point(currentPoint.X + (int)Math.Round(startX), -currentPoint.Y + (int)Math.Round(startY) + 1);
                                Plot(point);
                            }
                            break;
                    }
                    currentPoint.X++;
                    currentPoint.Y--;
                    h += delta_h;
                    v += delta_v;
                }
            }
        }
        private void BrezenkhemAlgCircle(Point mid, int radius)
        /* Отрисовка круга методом Брезенхема */
        {
            int x = 0, y = radius;//текущий пиксель
            int delta = 1 - 2 * radius;//смещение по y
            int error;//смещение по x
            while (y >= 0)
            {
                Plot(new Point(mid.X + x, mid.Y + y));
                Plot(new Point(mid.X + x, mid.Y - y));
                Plot(new Point(mid.X - x, mid.Y + y));
                Plot(new Point(mid.X - x, mid.Y - y));
                error = 2 * (delta + y) - 1;
                if (delta < 0 && error <= 0)
                {
                    x++;
                    delta += 2 * x + 1;
                    continue;
                }
                error = 2 * (delta - x) - 1;
                if (delta > 0 && error > 0)
                {
                    y--;
                    delta += 1 - 2 * y;
                    continue;
                }
                x++;
                delta += 2 * (x - y);
                y--;
            }
        }
        private void DecasteldjoAlgBusier()
        /* Отрисовка кривой Бизье */
        {
            int j = 0;
            float step = 0.01f;//шаг увелечение параметра t

            PointF[] result = new PointF[101];//Конечный массив точек кривой
            for (float t = 0; t < 1; t += step)
            {
                float ytmp = 0;
                float xtmp = 0;
                for (int i = 0; i < pointList.Count; i++)
                { // проходим по каждой точке
                    float b = BernshteynPolinom(i, pointList.Count - 1, t); // вычисляем наш полином Бернштейна
                    xtmp += pointList[i].X * b; // записываем и прибавляем результат
                    ytmp += pointList[i].Y * b;
                }
                result[j] = new PointF(xtmp, ytmp);
                j++;

            }
            this.Refresh();
            for (int i = 0; i < result.Length - 1; i++)
            {
                BrezenkhemAlgLine(new Point((int)result[i].X, (int)result[i].Y), new Point((int)result[i + 1].X, (int)result[i + 1].Y));
            }
        }
        static float BernshteynPolinom(int i, int n, float t)
        /* Функция вычисления полинома Бернштейна для кривой бизье */
        {
            return (Fuctorial(n) / (Fuctorial(i) * Fuctorial(n - i))) * (float)Math.Pow(t, i) * (float)Math.Pow(1 - t, n - i);
        }
        static int Fuctorial(int n)
        /* Вычисление факториала для кривой бизье */
        {
            int res = 1;
            for (int i = 1; i <= n; i++)
                res *= i;
            return res;
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        /* Отрисовка рамки области отсечения */
        {
            rect = new Rectangle(100, 100, 200, 200);
            BrezenkhemAlgLine(new Point(rect.Right, rect.Top), new Point(rect.Right, rect.Bottom));
            BrezenkhemAlgLine(new Point(rect.Right, rect.Bottom), new Point(rect.Left, rect.Bottom));
            BrezenkhemAlgLine(new Point(rect.Left, rect.Bottom), new Point(rect.Left, rect.Top));
            BrezenkhemAlgLine(new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Top));
        }

        int LEFT = 1;// двоичное 0001 
        int RIGHT = 2;// двоичное 0010
        int BOT = 4;// двоичное 0100
        int TOP = 8;// двоичное 1000
        int PointCode(Rectangle r, Point p)
        /* Для алгоритма Коэна-Сазерленда и средней точки */
        {
            return (p.X < r.Left ? LEFT : 0) + (p.X > r.Right ? RIGHT : 0) + (p.Y > (r.Bottom) ? BOT : 0) + (p.Y < (r.Top) ? TOP : 0);
        }
        int CohenSutherlandAlg(Rectangle r, Point a, Point b)
        /* Отсечение отрезка */
        {
            int code_a, code_b, code; /* код концов отрезка */
            Point c; /* одна из точек */

            code_a = PointCode(r, a);
            code_b = PointCode(r, b);

            /* пока одна из точек отрезка вне прямоугольника */
            while ((code_a | code_b) != 0)
            {
                /* если обе точки с одной стороны прямоугольника, то отрезок не пересекает прямоугольник */
                if ((code_a & code_b) != 0)
                    return -1;

                /* выбираем точку c с ненулевым кодом */
                if (code_a != 0)
                {
                    code = code_a;
                    c = a;
                }
                else
                {
                    code = code_b;
                    c = b;
                }
                if ((code & LEFT) != 0)
                {
                    c.Y += (a.Y - b.Y) * (r.Left - c.X) / (a.X - b.X);
                    c.X = r.Left;
                }
                else if ((code & RIGHT) != 0)
                {
                    c.Y += (a.Y - b.Y) * (r.Right - c.X) / (a.X - b.X);
                    c.X = r.Right;
                }
                else if ((code & BOT) != 0)
                {
                    c.X += (a.X - b.X) * (r.Bottom - c.Y) / (a.Y - b.Y);
                    c.Y = r.Bottom;
                }
                else if ((code & TOP) != 0)
                {
                    c.X += (a.X - b.X) * (r.Top - c.Y) / (a.Y - b.Y);
                    c.Y = r.Top;
                }

                /* обновляем код */
                if (code == code_a)
                {
                    a = c;
                    code_a = PointCode(r, a);
                }
                else
                {
                    b = c;
                    code_b = PointCode(r, b);
                }
            }
            /* оба кода равны 0, следовательно обе точки в прямоугольнике */
            BrezenkhemAlgLine(new Point(a.X, a.Y), new Point(b.X, b.Y));
            return 0;
        }
        void MidpointAlg(Point a, Point b)
        /* Алгоритм средней точки отсечения отрезка */
        {
            if (Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y)) < 2)
                return;
            int code_a = PointCode(rect, a);
            int code_b = PointCode(rect, b);
            if ((code_a & code_b) != 0)
                return;
            if ((code_a | code_b) == 0)
            {
                BrezenkhemAlgLine(new Point(a.X, a.Y), new Point(b.X, b.Y));
                return;
            }
            MidpointAlg(a, new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2));
            MidpointAlg(new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2), b);
        }

    }
    public struct Segment
    {
        public readonly PointF A, B;

        public Segment(PointF a, PointF b)
        {
            A = a;
            B = b;
        }

        public bool OnLeft(PointF p)
        {
            var ab = new PointF(B.X - A.X, B.Y - A.Y);
            var ap = new PointF(p.X - A.X, p.Y - A.Y);
            return ab.Cross(ap) >= 0;
        }

        public PointF Normal
        {
            get
            {
                return new PointF(B.Y - A.Y, A.X - B.X);
            }
        }

        public PointF Direction
        {
            get
            {
                return new PointF(B.X - A.X, B.Y - A.Y);
            }
        }

        public float IntersectionParameter(Segment that)
        {
            var segment = this;
            var edge = that;

            var segmentToEdge = edge.A.Sub(segment.A);
            var segmentDir = segment.Direction;
            var edgeDir = edge.Direction;

            var t = edgeDir.Cross(segmentToEdge) / edgeDir.Cross(segmentDir);

            if (float.IsNaN(t))
            {
                t = 0;
            }

            return t;
        }

        public Segment Morph(float tA, float tB)
        {
            var d = Direction;
            return new Segment(A.Add(d.Mul(tA)), A.Add(d.Mul(tB)));
        }
    }
    public class Polygon : List<PointF>
    {
        public Polygon()
            : base()
        { }

        public Polygon(int capacity)
            : base(capacity)
        { }

        public Polygon(IEnumerable<PointF> collection)
            : base(collection)
        { }

        public bool IsConvex
        {
            get
            {
                if (Count >= 3)
                {
                    for (int a = Count - 2, b = Count - 1, c = 0; c < Count; a = b, b = c, ++c)
                    {
                        if (!new Segment(this[a], this[b]).OnLeft(this[c]))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public IEnumerable<Segment> Edges
        {
            get
            {
                if (Count >= 2)
                {
                    for (int a = Count - 1, b = 0; b < Count; a = b, ++b)
                    {
                        yield return new Segment(this[a], this[b]);
                    }
                }
            }
        }

        public bool CyrusBeckClip(ref Segment subject)
        /* Алгоритм Кируса-Бека */
        {
            var subjDir = subject.Direction;
            var tA = 0.0f;
            var tB = 1.0f;
            foreach (var edge in Edges)
            {
                switch (Math.Sign(edge.Normal.Dot(subjDir)))
                {
                    case -1:
                        {
                            var t = subject.IntersectionParameter(edge);
                            if (t > tA)
                            {
                                tA = t;
                            }
                            break;
                        }
                    case +1:
                        {
                            var t = subject.IntersectionParameter(edge);
                            if (t < tB)
                            {
                                tB = t;
                            }
                            break;
                        }
                    case 0:
                        {
                            if (!edge.OnLeft(subject.A))
                            {
                                return false;
                            }
                            break;
                        }
                }
            }
            if (tA > tB)
            {
                return false;
            }
            subject = subject.Morph(tA, tB);
            return true;
        }
    }
    public static class PointExtensions
    {
        public static PointF Add(this PointF a, PointF b)
        {
            return new PointF(a.X + b.X, a.Y + b.Y);
        }

        public static PointF Sub(this PointF a, PointF b)
        {
            return new PointF(a.X - b.X, a.Y - b.Y);
        }

        public static PointF Mul(this PointF a, float b)
        {
            return new PointF(a.X * b, a.Y * b);
        }

        public static float Dot(this PointF a, PointF b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static float Cross(this PointF a, PointF b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
    }
}