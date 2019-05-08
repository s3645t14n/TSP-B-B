using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VoyagerTaskLab
{
    public partial class Form1 : Form
    {
        private const int CS_DROPSHADOW = 0x00020000;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        //класс точки double
        public class PointD
        {
            public double X { get; private set; }
            public double Y { get; private set; }

            public PointD(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        private double CountD(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }

        //данные
        public List<PointD> PList;//координаты точек
        double[,] AdjM, oAdjM;
        double minValue = Double.MaxValue;
        List<Point> pathArray, answerTour;
        int iterations = 0;

        public Form1()
        {
            this.KeyPreview = true;
            this.KeyPress += new KeyPressEventHandler(g_KeyDown);
            this.KeyPress += new KeyPressEventHandler(c_KeyDown);
            InitializeComponent();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            button3.Enabled = false;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 150;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 150;
            chart1.Series["Default"].Color = Color.Transparent;
            chart1.Series["Default"].Points.AddXY(0, 0);
            PList = new List<PointD>();
            pathArray = new List<Point>();
        }

        //выход
        private void Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //клик по графику
        private void Chart1_MouseClick(object sender, MouseEventArgs e)
        {
            chart1.Series["Points"].Points.Clear();
            double x = chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X);
            double y = chart1.ChartAreas[0].AxisY.PixelPositionToValue(e.Location.Y);
            chart1.Series["Points"].Points.AddXY(x, y);
            textBox1.Text = Math.Round(x, 8).ToString();
            textBox2.Text = Math.Round(y, 8).ToString();
        }
        
        //даблклик по графику
        private void Chart1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Chart1_MouseClick(sender, e);
            if (button1.Enabled == true)
                Button1_Click(sender, e);
        }

        //добавить точку
        private void Button1_Click(object sender, EventArgs e)
        {
            button3.Enabled = true;
            chart1.Series["Points"].Points.Clear();
            double x, y;
            Double.TryParse(textBox1.Text, out x);
            Double.TryParse(textBox2.Text, out y);
            PList.Add(new PointD(x, y));
            chart1.Series["Bubbles"].Points.DataBindXY(PList, "X", PList, "Y");

            //добавление линий
            chart1.Series["Lines"].Points.Clear();
            for (int i = 0; i < PList.Count; i++)
                for (int j = 0; j < PList.Count; j++)
                {
                    if (i != j)
                    {
                        chart1.Series["Lines"].Points.AddXY(PList[i].X, PList[i].Y);
                        chart1.Series["Lines"].Color = Color.Transparent;
                        chart1.Series["Lines"].Points.AddXY(PList[j].X, PList[j].Y);
                        chart1.Series["Lines"].Color = Color.Red;
                    }
                }

            //отправка списка в текстфилд
            textBox3.Text = "";
            for (int i = 0; i < PList.Count; i++)
            {
                if (i > 0)
                    textBox3.Text += "\r\n";
                textBox3.AppendText(i + ": " + Math.Round(PList[i].X, 5) + ", " + Math.Round(PList[i].Y, 5));
            }

            //расчет матрицы весов
            AdjM = new double[PList.Count, PList.Count];
            AdjM[0, 0] = Double.PositiveInfinity;
            for (int i = 1; i < PList.Count; i++)
                for (int j = 0; j < i; j++)
                {
                    AdjM[i, j] = CountD(PList[i].X, PList[i].Y, PList[j].X, PList[j].Y);
                    AdjM[j, i] = AdjM[i, j];
                    if (j == i - 1)
                        AdjM[j + 1, i] = Double.PositiveInfinity;
                }
            oAdjM = AdjM;
            OMatrix(AdjM);
        }

        //вывод матрицы и минимумов и стоимости
        private void OMatrix(double[,] m)
        {
            textBox5.Text += " ";
            for (int i = -1; i < m.GetLength(0); i++)
                if (i != -1)
                    textBox5.Text += i.ToString().PadRight(5, ' ') + " ";
                else
                    textBox5.Text += "   ";
            textBox5.Text += "Min";
            textBox5.Text += "\r\n--";
            for (int i = 0; i <= m.GetLength(0); i++)
                textBox5.Text += "------";
            textBox5.Text += "\r\n";
            for (int i = 0; i < m.GetLength(0); i++)
            {
                textBox5.Text += i.ToString().PadLeft(2, ' ') + ": ";
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    if (Double.IsInfinity(m[i, j]))
                        textBox5.Text += "нет   ";
                    else
                        textBox5.Text += Math.Round(m[i, j], 1).ToString().PadRight(5, ' ') + " ";
                }
                if (Double.IsInfinity(GetRowMin(m, i, i)))
                    textBox5.Text += "нет  \r\n";
                else
                    textBox5.Text += Math.Round(GetRowMin(m, i, i), 1).ToString().PadRight(5, ' ') + "\r\n";
            }
            textBox5.Text += "Min ";
            for (int j = 0; j < m.GetLength(1); j++)
                if (Double.IsInfinity(GetColMin(m, j, j)))
                    textBox5.Text += "нет   ";
                else
                    textBox5.Text += Math.Round(GetColMin(m, j, j), 1).ToString().PadRight(5, ' ') + " ";
            if (Double.IsInfinity(CCount(m)))
                textBox5.AppendText("нет   \r\n\r\n");
            else
                textBox5.AppendText(Math.Round(CCount(m), 1).ToString().PadRight(5, ' ') + "\r\n\r\n");
        }

        //минимум 1 строки
        private double GetRowMin(double[,] m, int i, int exc = -1)
        {
            double[] SubRow = new double[m.GetLength(1)];
            Buffer.BlockCopy(m, 8 * m.GetLength(1) * i, SubRow, 0, 8 * m.GetLength(1));
            if (exc != -1)
                SubRow[exc] = Double.PositiveInfinity;
            return SubRow.Min();
        }

        //минимум 1 столбца
        private double GetColMin(double[,] m, int j, int exc = -1)
        {
            double[] SubCol = new double[m.GetLength(0)];
            SubCol = Enumerable.Range(0, m.GetLength(0)).Select(xr => m[xr, j]).ToArray();
            if (exc != -1)
                SubCol[exc] = Double.PositiveInfinity;
            return SubCol.Min();
        }

        ////минимумы по строкам
        //private double[] RowMin(double[,] m)
        //{
        //    double[] res = new double[m.GetLength(0)];
        //    for (int i = 0; i < m.GetLength(0); i++)
        //        res[i] = GetRowMin(m, i, i);
        //    return res;
        //}

        ////минимумы по столбцам
        //private double[] ColMin(double[,] m)
        //{
        //    double[] res = new double[m.GetLength(1)];
        //    for (int j = 0; j < m.GetLength(1); j++)
        //        res[j] = GetColMin(m, j, j);
        //    return res;
        //}

        //функция редукции
        private double[,] MReduce(double[,] m)
        {
            double[,] resm = new double[m.GetLength(0), m.GetLength(1)];
            for (int i = 0; i < m.GetLength(0); i++)
                for (int j = 0; j < m.GetLength(1); j++)
                    if (m[i, j] < Double.PositiveInfinity)
                        resm[i, j] = m[i, j] - GetRowMin(m, i, i);
                    else
                        resm[i, j] = Double.PositiveInfinity;
            for (int i = 0; i < m.GetLength(0); i++)
                for (int j = 0; j < m.GetLength(1); j++)
                    if (m[i, j] < Double.PositiveInfinity)
                        resm[i, j] = m[i, j] - GetColMin(m, j, j);
                    else
                        resm[i, j] = Double.PositiveInfinity;
            return resm;
        }

        //функция подсчета стоимости тура по матрице
        private double CCount(double[,] m)
        {
            double res = 0;
            for (int i = 0; i < m.GetLength(0); i++)
                if (Double.IsInfinity(GetRowMin(m, i, i)))
                    continue;
                else
                    res += GetRowMin(m, i, i);
            for (int j = 0; j < m.GetLength(1); j++)
                if (Double.IsInfinity(GetColMin(m, j, j)))
                    continue;
                else
                    res += GetColMin(m, j, j);
            return res;
        }

        //функция нахождения нулевого ребра с наивысшей оценкой
        private Point FindMaxVElem(double[,] m)
        {
            double MaxV = Double.MinValue;
            Point MaxP = new Point();
            for (int i = 0; i < m.GetLength(0); i++)
                for (int j = 0; j < m.GetLength(1); j++)
                    if (m[i, j] == 0)
                    {
                        double s = 0;
                        if (!Double.IsInfinity(GetRowMin(m, i, j)))
                            s += GetRowMin(m, i, j);
                        if (!Double.IsInfinity(GetColMin(m, j, i)))
                            s += GetColMin(m, j, i);
                        if (Double.IsInfinity(s))
                            s = 0;
                        if (s > MaxV)
                        {
                            MaxV = s;
                            MaxP.X = i;
                            MaxP.Y = j;
                        }
                    }
            return MaxP;
        }

        //функция удаления строки/столбца и ??заНаНивания обратного пути
        private double[,] RCSubtract(double[,] m, int l, int n)
        {
            double[,] resm = new double[m.GetLength(0), m.GetLength(1)];
            for (int i = 0; i < m.GetLength(0); i++)
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    if (i == l || j == n)
                        resm[i, j] = Double.PositiveInfinity;
                    else
                        resm[i, j] = m[i, j];
                }
            resm[n, l] = Double.PositiveInfinity + 1;
            return resm;
        }

        //проверка матрицы на пустоту
        private bool isMEmpty(double[,] m)
        {
            bool res = true;
            for (int i = 0; i < m.GetLength(0); i++)
                for (int j = 0; j < m.GetLength(1); j++)
                    if (!(Double.IsInfinity(m[i, j])))
                        res = false;
            return res;
        }

        //получение путей
        private List<Point> getPaths(double[,] m)
        {
            //mintourv = CCount(MReduce(m));
            List<Point> resPathArray = pathArray;
            while (isMEmpty(m) == false)
            {
                m = MReduce(m);
                resPathArray.Add(FindMaxVElem(m));
                if (checkBox1.Checked == true)
                    textBox5.AppendText("\r\n Найдено: " + resPathArray.Last().X + ">" + resPathArray.Last().Y + "\r\n");
                m = RCSubtract(m, resPathArray.Last().X, resPathArray.Last().Y);
                if (checkBox1.Checked == true)
                    OMatrix(m);
            }
            if (checkBox2.Checked == true)
            {
                textBox4.AppendText("\r\nНайденные пути: ");
                for (int i = 0; i < resPathArray.Count; i++)
                    textBox4.Text += resPathArray[i].X + ">" + resPathArray[i].Y + "; ";
            }
            return resPathArray;
        }

        //получение сабтуров
        private List<List<Point>> getSubtours(List<Point> paths)
        {
            List<List<Point>> tourList = new List<List<Point>>();
            while (paths.Count >= 1)
            {
                List<Point> tPath = new List<Point>();
                Point next = paths[0];
                tPath.Add(next);
                int currentY = tPath.Last().Y, startX = tPath.Last().X;
                paths.Remove(next);
                while (tPath.Last().Y != startX)
                {
                    next = paths.Find(x => x.X == currentY);
                    if (next != new Point(0, 0))
                    {
                        tPath.Add(next);
                        paths.Remove(next);
                        currentY = tPath.Last().Y;
                    }
                    else
                        break;
                }
                if (tPath.Count == 1)
                    tPath.Add(new Point(tPath[0].Y, tPath[0].X));
                tourList.Add(tPath);
            }

            if (checkBox2.Checked == true)
            {
                textBox4.AppendText("\r\nНайденные туры:");
                int n = 0;
                foreach (List<Point> subList in tourList)
                {
                    textBox4.AppendText("\r\nТур " + n + ": ");
                    foreach (Point item in subList)
                        textBox4.Text += item.X + ">";
                    textBox4.Text += subList.Last().Y;
                    n++;
                }
            }
            
            return tourList;
        }

        //поиск минимального тура
        private List<Point> findMinTour(List<List<Point>> tourList)
        {
            List<Point> minTour = tourList[0];
            foreach (List<Point> subList in tourList)
                if (subList.Count < minTour.Count)
                    minTour = subList;

            if (checkBox2.Checked == true)
            {
                textBox4.AppendText("\r\nМинимальный тур: ");
                foreach (Point item in minTour)
                    textBox4.Text += item.X + ">";
                textBox4.Text += minTour.Last().Y;
            }

            return minTour;
        }

        //функция подсчета стоимости списка туров
        private double TCount(List<List<Point>> tours)
        {
            double res = 0;
            foreach (List<Point> subList in tours)
                foreach (Point item in subList)
                    res += AdjM[item.X, item.Y];

            if (checkBox2.Checked == true)
                textBox4.AppendText("\r\nСтоимость " + tours.Count + " туров: " + Math.Round(res, 2));
            return res;
        }

        //функция подсчета стоимости тура
        private double TCount(List<Point> tour)
        {
            double res = 0;
            foreach (Point item in tour)
                res += AdjM[item.X, item.Y];

            if (checkBox2.Checked == true)
                textBox4.AppendText("\r\nСтоимость турa: " + Math.Round(res, 2));
            return res;
        }

        //главная рекурсия
        private void mainRecursion(List<List<Point>> tours, double[,] oMatrix)
        {
            if (iterations < PList.Count * 10000)
            {
                iterations++;
                //List<Point> tour = findMinTour(tours);
                foreach (List<Point> sublist in tours)
                    if (TCount(sublist) < minValue)
                        foreach (Point subproblem in sublist)
                        {
                            double[,] m = oMatrix.Clone() as double[,];
                            m[subproblem.X, subproblem.Y] = Double.PositiveInfinity;
                            m[subproblem.Y, subproblem.X] = Double.PositiveInfinity;
                            //m = RCSubtract(m, subproblem.X, subproblem.Y);
                            List<List<Point>> tempTours = getSubtours(getPaths(m));
                            if (TCount(tempTours) < minValue)
                            {
                                if (tempTours.Count == 1)
                                {
                                    if (tempTours[0].Count == PList.Count)
                                    {
                                        answerTour = tempTours[0];
                                        minValue = TCount(tempTours[0]);
                                    }
                                }
                                else
                                    mainRecursion(tempTours, m);
                            }
                            else
                                if (checkBox2.Checked == true)
                                textBox4.AppendText("\r\nОТВЕРГНУТО (" + Math.Round(TCount(tempTours), 2) + "≥" + Math.Round(minValue, 2) + ")");
                        }
                    else
                        if (checkBox2.Checked == true)
                        textBox4.AppendText("\r\nОТВЕРГНУТО (" + Math.Round(TCount(sublist), 2) + "≥" + Math.Round(minValue, 2) + ")");
            }
            else
            {
                return;
            }
        }

        //рассчитать
        private void Button3_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button3.Enabled = false;
            textBox4.AppendText("Подождите...");

            List<List<Point>> tours = getSubtours(getPaths(AdjM));
            if (tours.Count == 1 && tours[0].Count == PList.Count)
            {
                answerTour = tours[0];
                minValue = TCount(tours[0]);
            }
            mainRecursion(tours, AdjM);

            textBox4.AppendText("\r\nФинальный тур: ");
            foreach (Point item in answerTour)
                textBox4.Text += item.X + ">";
            textBox4.Text += answerTour.Last().Y;
            textBox4.AppendText("\r\nФинальная стоимость: " + Math.Round(TCount(answerTour), 2));

            //int n = 0;
            //foreach (List<Point> subList in tours)
            //{
            //    chart1.Series.Add("Route" + n);
            //    chart1.Series["Route" + n].BorderWidth = 3;
            //    chart1.Series["Route" + n].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            //    foreach (Point item in subList)
            //        chart1.Series["Route" + n].Points.AddXY(PList[item.X].X, PList[item.X].Y);
            //    chart1.Series["Route" + n].Points.AddXY(PList[subList[0].X].X, PList[subList[0].X].Y);
            //    n++;
            //}

            chart1.Series["Lines"].Points.Clear();
            chart1.Series["Lines"].BorderWidth = 3;
            foreach (Point item in answerTour)
                chart1.Series["Lines"].Points.AddXY(PList[item.X].X, PList[item.X].Y);
            chart1.Series["Lines"].Points.AddXY(PList[answerTour.Last().Y].X, PList[answerTour.Last().Y].Y);
        }

        //горячие клавиши
        private void g_KeyDown(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'a' && button1.Enabled == true)
                Button1_Click(sender, e);
        }

        private void TextBox1_Enter(object sender, EventArgs e)
        {
            textBox1.SelectionStart = 0;
            textBox1.SelectionLength = textBox1.Text.Length;
        }

        private void TextBox2_Enter(object sender, EventArgs e)
        {
            textBox2.SelectionStart = 0;
            textBox2.SelectionLength = textBox2.Text.Length;
        }

        private void c_KeyDown(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'c' && button3.Enabled == true)
                Button3_Click(sender, e);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
