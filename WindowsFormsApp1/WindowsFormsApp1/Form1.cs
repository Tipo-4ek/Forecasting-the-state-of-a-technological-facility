using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        private SQLiteConnection SQLiteConn;

        public SQLiteConnection SQLiteConnectionConn { get; private set; }

        private DataTable dTable;
        private DataTable par_dtable;
        ToolStripLabel ALabel;
        ToolStripLabel ELabel;
        double par_E = -1;
        double par_A = -1;
        double alpha_plus_max = 0;
        double alpha_minus_min = 0;
        double m_minus_min = 0;
        double m_minus_max = 0;
        double decomp4_min = 999999;
        double decomp4_max = -1;

        public Form1()
        {
            InitializeComponent();
            ELabel = new ToolStripLabel();
            ALabel = new ToolStripLabel();
            



        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
            SQLiteConnectionConn = new SQLiteConnection();
            dTable = new DataTable();
            par_dtable = new DataTable();
            tabPage2.Parent = null;
            tabPage3.Parent = null;
            tabPage5.Parent = null;
            tabPage7.Parent = null;
            button1.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            this.comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;


        }

        public Image ByteToImage(byte[] imageBytes)
        {
            // Convert byte[] to Image
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = new Bitmap(ms);
            return image;
        }
        private bool OpenDBFile()
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Filter = "Текстовые файлы (*.sqlite)|*.sqlite| Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                SQLiteConn = new SQLiteConnection("Data Source =" + openFileDialog.FileName + ";Version = 3;");
                SQLiteConn.Open();
                SQLiteCommand command = new SQLiteCommand();
                command.Connection = SQLiteConn;
                return true;
            }
            else return false;
        }

        private string SQL_ALLTable() { return "SELECT * FROM Данные order by 1"; }
        private string SQL_EATable1() { return "SELECT value FROM `parameters`"; }

        private void ShowTable(string SQLQuery)
        {
            dTable.Clear();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(SQLQuery, SQLiteConn);
            adapter.Fill(dTable);

            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();

            for (int col = 0; col < dTable.Columns.Count; col++)
            {
                string ColName = dTable.Columns[col].ColumnName;
                dataGridView1.Columns.Add(ColName, ColName);
                dataGridView1.Columns[col].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            for (int row = 0; row < dTable.Rows.Count; row++)
            {
                dataGridView1.Rows.Add(dTable.Rows[row].ItemArray);

            }
            for (int i = 1; i < dataGridView1.Rows.Count; i++)
            {
                //begin from 1 to kick column[0] - epocha
                for (int j = 1; j < dataGridView1.Rows[i].Cells.Count; j++)
                {
                    double curr_val = Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value);

                    if ((curr_val > decomp4_max) && ((curr_val > decomp4_min) || decomp4_max == -1))
                        decomp4_max = curr_val;
                    if ((curr_val < decomp4_min) && ((curr_val < decomp4_max) || decomp4_max == -1) && (curr_val != 0))
                        decomp4_min = curr_val;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (OpenDBFile() == true)
            {
                string query = "SELECT data FROM `images` WHERE `id`= 1";
                SQLiteCommand cmd = new SQLiteCommand(query, SQLiteConn);

                SQLiteDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    byte[] a = (System.Byte[])rdr[0];
                    pictureBox1.Image = ByteToImage(a);
                }
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox1.Image = new Bitmap(pictureBox1.Image);

                statusStrip1.Items.Add(ELabel);
                statusStrip1.Items.Add(ALabel);

                string pars = "SELECT * FROM `parameters`";
                SQLiteCommand command = new SQLiteCommand(pars, SQLiteConn);
                command.CommandText = "SELECT * FROM `parameters`";
                DataTable data = new DataTable();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                adapter.Fill(data);
                Console.WriteLine($"Прочитано {data.Rows.Count} записей из таблицы БД");
                foreach (DataRow row in data.Rows)
                {
                    Console.WriteLine($"id = {row.Field<long>("id")} name = {row.Field<string>("name")} value = {row.Field<double>("value")}");
                    if (row.Field<long>("id") == 1)
                    {
                        textBox1.Text = row.Field<double>("value").ToString();
                        ELabel.Text = textBox1.Text + " , ";
                        par_E = row.Field<double>("value");
                    }

                    if (row.Field<long>("id") == 2)
                    {
                        textBox2.Text = row.Field<double>("value").ToString();
                        ALabel.Text = textBox2.Text;
                        par_A = row.Field<double>("value");
                    }
                    ShowTable(SQL_ALLTable());
                }

                tabPage2.Parent = tabControl1;
                tabPage3.Parent = tabControl1;
                tabPage5.Parent = tabControl1;
                button1.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e) // Удаление
        {
            string sql_query = "DELETE FROM `Данные` WHERE `Эпоха` = " + Convert.ToString(dataGridView1.Rows.Count - 2);
            Console.WriteLine(sql_query);
            SQLiteCommand cmd = new SQLiteCommand(sql_query, SQLiteConn);
            SQLiteDataReader rdr = cmd.ExecuteReader();
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            ShowTable(SQL_ALLTable());
        }

        private void button1_Click(object sender, EventArgs e) // Добавление
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            Random rnd = new Random();
            string sql_query = "INSERT INTO `Данные` VALUES (" + Convert.ToString(dataGridView1.Rows.Count - 1) + ", ";
            double val;
            double max = 0;
            for (int i = 1; i < dataGridView1.Columns.Count; i++) // Первый столбец без значений
            {
                for (int j = 1; j < dataGridView1.Rows.Count - 2; j++) // Первая строка без значений
                {
                    double difference = Math.Abs(Convert.ToDouble(dataGridView1.Rows[j + 1].Cells[i].Value) - Convert.ToDouble(dataGridView1.Rows[j].Cells[i].Value));
                    if (difference > max)
                    {
                        max = difference;
                    }
                }
                double difference1 = rnd.NextDouble() * (max - -max) + (-max);
                val = Math.Round(Convert.ToDouble(dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[i].Value) + difference1, 4);
                sql_query += val;
                if (i < dataGridView1.Columns.Count - 1)
                {
                    sql_query += ", ";
                }
                
            }

            sql_query = sql_query.Remove(sql_query.Length - 1) + ")";
            Console.WriteLine(sql_query);

            SQLiteCommand cmd = new SQLiteCommand(sql_query, SQLiteConn);
            SQLiteDataReader rdr = cmd.ExecuteReader();
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            ShowTable(SQL_ALLTable());
        }

        public void charts_init(Chart inp_chart, double y_min, double y_max)
        {
            inp_chart.ChartAreas[0].AxisY.Minimum = y_min;
            inp_chart.ChartAreas[0].AxisY.Maximum = y_max;
        }

        public void CreateSerie(int cells_x, int cells_y, string inp_nameSerie, Chart inp_chartName, bool isNeedXCut, long koeff_umnozhenia, double y_min, double y_max, DataGridView inp_datagrid)
        {
            double x, y;
           
            string Serie1 = inp_nameSerie;
            //Console.WriteLine("------------------------------");
            //Console.WriteLine(inp_nameSerie);

            inp_chartName.Series.Add(new Series(Serie1));
            inp_chartName.Series[Serie1].ChartType = (System.Windows.Forms.DataVisualization.Charting.SeriesChartType)4;
            inp_chartName.Series[Serie1].Enabled = true;
            inp_chartName.Series[Serie1].BorderWidth = 2;
            inp_chartName.Series[Serie1].MarkerStyle = MarkerStyle.Circle;
            inp_chartName.Series[Serie1].MarkerColor = Color.Red;
            inp_chartName.Series[Serie1].MarkerSize = 4;
            
            if (inp_chartName == chart1) {
                inp_chartName.ChartAreas[0].AxisX.Title = "m";
                inp_chartName.ChartAreas[0].AxisY.Title = "alpha";
            }
            if (inp_chartName == chart2)
            {
                inp_chartName.ChartAreas[0].AxisX.Title = "m";
                inp_chartName.ChartAreas[0].AxisY.Title = "t";
            }
            for (int p = 0; p < inp_datagrid.Rows.Count - 2; p++)
            {

               
                x = Convert.ToDouble(inp_datagrid.Rows[p].Cells[cells_x].Value);
                if (isNeedXCut) 
                    x = x - x % 0.0001;
                y = Convert.ToDouble(inp_datagrid.Rows[p].Cells[cells_y].Value) * koeff_umnozhenia;

                //Console.WriteLine("\n" + inp_nameSerie + " - " + x + " | " + y);
                inp_chartName.Series[Serie1].Points.AddXY(x, y);
                inp_chartName.Series[Serie1].Points[p].Label = Convert.ToString(p);
       

            }
            //Console.WriteLine("------------------------------");
            //inp_chartName.ChartAreas[0].AxisY.Minimum = y_min;
            //inp_chartName.ChartAreas[0].AxisY.Maximum = y_max;
            //inp_chartName.ChartAreas[0].AxisY.Minimum = 48;
            //inp_chartName.ChartAreas[0].AxisY.Maximum = 49;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ELabel.Text = textBox1.Text + " , ";
            ALabel.Text = textBox2.Text;
            par_E = Convert.ToDouble(textBox1.Text);
            par_A = Convert.ToDouble(textBox2.Text);
        }

        public void Decomposition1()
        {
           
            alpha_plus_max = 0;
            alpha_minus_min = 999999;
            m_minus_min = 999999;
            m_minus_max = 0;
      

            string[] name = { "Эпоха", "M", "alfa", "M+", "M-", "alfa+", "alfa-", "M(прогн)", "alfa(прогн)", "M+(прогн)", "M-(прогн)", "alfa+(прогн)", "alfa-(прогн)", "L", "R", "Устойчивость" };
            double M = 0, M_plus = 0, M_minus = 0, alfa = 0, alfa_plus = 0, alfa_minus = 0, M_prev = 0, M_prev_plus = 0, M_prev_minus = 0, M_progn = 0, M_plus_progn = 0, M_minus_progn = 0, alfa_progn = 0, alfa_plus_progn = 0, alfa_minus_progn = 0, sum_M = 0, sum_M_plus = 0, sum_M_minus = 0, M_progn_prev = 0, alfa_progn_prev = 0, sum_alfa = 0, sum_alfa_plus = 0, sum_alfa_minus = 0, M_progn_prev_plus = 0, M_progn_prev_minus = 0, alfa_progn_prev_plus = 0, alfa_progn_prev_minus = 0, L = 0, M_null = 0, R = 0, sum_M_progn = 0, sum_Mplus_progn = 0, sum_Mminus_progn = 0, sum_alfa_progn = 0, sum_alfaplus_progn = 0, sum_alfaminus_progn = 0 ;

            double[,] decomp = new double[dataGridView1.Rows.Count, dataGridView1.Columns.Count];
            dataGridView2.Columns.Clear();
            dataGridView2.Rows.Clear();
            
            for (int i = 0; i <name.Length; i++)
            {
                dataGridView2.Columns.Add(name[i], name[i]);
                dataGridView2.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                dataGridView2.Rows.Add(dataGridView1.Rows[i].Cells[0].Value);

       
            }
            dataGridView2.Rows.Add("Прогноз");

            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++) //M и alfa
            {
                for (int j = 1; j < dataGridView1.Columns.Count; j++)
                {
                    M += Math.Pow(Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value), 2);
                    M_plus += Math.Pow(Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value) + par_E, 2);
                    M_minus += Math.Pow(Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value) - par_E, 2);

                    alfa += Convert.ToDouble(dataGridView1.Rows[0].Cells[j].Value) * Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value);
                    alfa_plus += (Convert.ToDouble(dataGridView1.Rows[0].Cells[j].Value) + par_E) * (Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value) + par_E);
                    //Console.WriteLine(Convert.ToString(Convert.ToDouble(dataGridView1.Rows[0].Cells[j].Value) + par_E));
                    alfa_minus += (Convert.ToDouble(dataGridView1.Rows[0].Cells[j].Value) - par_E) * (Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value) - par_E);

                    /*                        Console.WriteLine(Convert.ToDouble(dataGridView1.Rows[0].Cells[j].Value));
                                            Console.WriteLine(Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value));*/

                }
                if (i == 0)
                {
                    /*Console.WriteLine("Присвоили М первой = {0}. Альфы = 0 ", Math.Sqrt(M));*/
                    M_prev = M;
                    M_prev_plus = M_plus;
                    M_prev_minus = M_minus;
                    alfa = 0;
                    alfa_minus = 0;
                    alfa_plus = 0;

                }
                else
                {
                    //Console.WriteLine("М первая {0}", Math.Sqrt(M_prev));
                    //Console.WriteLine("М {0}", Math.Sqrt(M));

                    alfa = alfa / (Math.Sqrt(M_prev) * Math.Sqrt(M));
                    /* Console.WriteLine("alpa_plus = {0}", alfa_plus);*/
                    alfa_plus = alfa_plus / (Math.Sqrt(M_prev_plus) * Math.Sqrt(M_plus));
                    /*double temp = (Math.Sqrt(M_prev_plus) * Math.Sqrt(M_plus));
                    Console.WriteLine("Делим это на {0} = {1}",temp, alfa_plus);*/
                    alfa_minus = alfa_minus / (Math.Sqrt(M_prev_minus) * Math.Sqrt(M_minus));
                    // Console.WriteLine("Альфа поделили {0}", alfa_plus);                
                    alfa = Math.Acos(alfa)*3600;
                    /* Console.WriteLine("Ща будем брать acos alfa ({0})", alfa_plus);*/
                    alfa_plus = Math.Acos(alfa_plus)*3600;
                    /*  Console.WriteLine("Взяли acos = {0}", alfa_plus);*/
                    alfa_minus = Math.Acos(alfa_minus)*3600;
                    //Console.WriteLine("Альфа косинус {0}", alfa_plus);
                }

                M = Math.Round(M,4);
                M_plus = Math.Round(M_plus, 4);
                M_minus = Math.Round(M_minus, 4);
               
                dataGridView2.Rows[i].Cells[1].Value = Math.Round(Math.Sqrt(M),4);
                dataGridView2.Rows[i].Cells[3].Value = Math.Round(Math.Sqrt(M_plus), 4);
                dataGridView2.Rows[i].Cells[4].Value = Math.Round(Math.Sqrt(M_minus), 4);
                dataGridView2.Rows[i].Cells[2].Value = alfa;
                dataGridView2.Rows[i].Cells[5].Value = alfa_plus;
                dataGridView2.Rows[i].Cells[6].Value = alfa_minus;
                sum_M += Math.Sqrt(M);
                sum_M_plus += Math.Sqrt(M_plus);
                sum_M_minus += Math.Sqrt(M_minus);
                sum_alfa += alfa;
                sum_alfa_plus += alfa_plus;
                sum_alfa_minus += alfa_minus;

                /// y_min
                
                if (alfa_minus < alpha_minus_min)
                    alpha_minus_min = alfa_minus;              
                if (M_minus < m_minus_min)
                    m_minus_min = M_minus;
              
                ///y_max
                
                if (alfa_plus > alpha_plus_max)
                    alpha_plus_max = alfa_plus;
                if (M_minus > m_minus_max)
                    m_minus_max = M_minus;

                M = 0;
                M_plus = 0;
                M_minus = 0;
                alfa = 0;
                alfa_plus = 0;
                alfa_minus = 0;

            }
            for (int i = 0; i < dataGridView2.Rows.Count - 2; i++) //M(прогн) и alfa(прогн)
            {
                if (i == 0)
                {
                    M_progn = par_A * Convert.ToDouble(dataGridView2.Rows[0].Cells[1].Value) + (1 - par_A) * (sum_M / (dataGridView2.Rows.Count - 2));
                    alfa_progn = par_A * Convert.ToDouble(dataGridView2.Rows[0].Cells[2].Value) + (1 - par_A) * (sum_alfa / (dataGridView2.Rows.Count - 2));
                    M_plus_progn = par_A * Convert.ToDouble(dataGridView2.Rows[0].Cells[3].Value) + (1 - par_A) * (sum_M_plus / (dataGridView2.Rows.Count - 2));
                    M_minus_progn = par_A * Convert.ToDouble(dataGridView2.Rows[0].Cells[4].Value) + (1 - par_A) * (sum_M_minus / (dataGridView2.Rows.Count - 2));
                    alfa_plus_progn = par_A * Convert.ToDouble(dataGridView2.Rows[0].Cells[5].Value) + (1 - par_A) * (sum_alfa_plus / (dataGridView2.Rows.Count - 2));
                    alfa_minus_progn = par_A * Convert.ToDouble(dataGridView2.Rows[0].Cells[6].Value) + (1 - par_A) * (sum_alfa_minus / (dataGridView2.Rows.Count - 2));
                    dataGridView2.Rows[i].Cells[7].Value = M_progn;
                    dataGridView2.Rows[i].Cells[8].Value = alfa_progn*3600;
                    dataGridView2.Rows[i].Cells[9].Value = M_plus_progn;
                    dataGridView2.Rows[i].Cells[10].Value = M_minus_progn;
                    dataGridView2.Rows[i].Cells[11].Value = alfa_plus_progn*3600;
                    dataGridView2.Rows[i].Cells[12].Value = alfa_minus_progn*3600;
                    M_progn_prev = M_progn;
                    alfa_progn_prev = alfa_progn;
                    M_progn_prev_plus = M_plus_progn;
                    M_progn_prev_minus = M_minus_progn;
                    alfa_progn_prev_plus = alfa_plus_progn;
                    alfa_progn_prev_minus = alfa_minus_progn;
                    sum_M_progn += M_progn;
                    sum_Mplus_progn += M_plus_progn;
                    sum_Mminus_progn += M_minus_progn;
                    sum_alfa_progn += alfa_progn;
                    sum_alfaplus_progn += alfa_plus_progn;
                    sum_alfaminus_progn += alfa_minus_progn;
                   
                    alfa_progn = 0;
                    alfa_plus_progn = 0;
                    alfa_minus_progn = 0;
                    M_progn = 0;
                    M_plus_progn = 0;
                    M_minus_progn = 0;
                    
                }
                else
                {
                    M_progn = par_A * Convert.ToDouble(dataGridView2.Rows[i].Cells[1].Value) + (1 - par_A) * M_progn_prev;
                    alfa_progn = par_A * Convert.ToDouble(dataGridView2.Rows[i].Cells[2].Value) + (1 - par_A) * alfa_progn_prev;
                    M_plus_progn = par_A * Convert.ToDouble(dataGridView2.Rows[i].Cells[3].Value) + (1 - par_A) * M_progn_prev_plus;
                    M_minus_progn = par_A * Convert.ToDouble(dataGridView2.Rows[i].Cells[4].Value) + (1 - par_A) * M_progn_prev_minus;
                    alfa_plus_progn = par_A * Convert.ToDouble(dataGridView2.Rows[i].Cells[5].Value) + (1 - par_A) * alfa_progn_prev_plus;
                    alfa_minus_progn = par_A * Convert.ToDouble(dataGridView2.Rows[i].Cells[6].Value) + (1 - par_A) * alfa_progn_prev_minus;
                    dataGridView2.Rows[i].Cells[7].Value = M_progn;
                    dataGridView2.Rows[i].Cells[8].Value = alfa_progn;
                    dataGridView2.Rows[i].Cells[9].Value = M_plus_progn;
                    dataGridView2.Rows[i].Cells[10].Value = M_minus_progn;
                    dataGridView2.Rows[i].Cells[11].Value = alfa_plus_progn;
                    dataGridView2.Rows[i].Cells[12].Value = alfa_minus_progn;
                    M_progn_prev = M_progn;
                    alfa_progn_prev = alfa_progn;
                    M_progn_prev_plus = M_plus_progn;
                    M_progn_prev_minus = M_minus_progn;
                    alfa_progn_prev_plus = alfa_plus_progn;
                    alfa_progn_prev_minus = alfa_minus_progn;
                    sum_M_progn += M_progn;
                    sum_Mplus_progn += M_plus_progn;
                    sum_Mminus_progn += M_minus_progn;
                    sum_alfa_progn += alfa_progn;
                    sum_alfaplus_progn += alfa_plus_progn;
                    sum_alfaminus_progn += alfa_minus_progn;
                    alfa_progn = 0;
                    alfa_plus_progn = 0;
                    alfa_minus_progn = 0;
                    M_progn = 0;
                    M_plus_progn = 0;
                    M_minus_progn = 0;
                }

            }

            for (int i = 0; i < dataGridView2.Rows.Count - 1; i++)
            {
                if (i==0)
                {
                    M_null = Convert.ToDouble(dataGridView2.Rows[i].Cells[1].Value);
                    R = Math.Abs((Convert.ToDouble(dataGridView2.Rows[i].Cells[1].Value) - M_null)/2);
                }

                L = Math.Abs(Convert.ToDouble(dataGridView2.Rows[i].Cells[3].Value) - Convert.ToDouble(dataGridView2.Rows[i].Cells[4].Value));
                R = Math.Abs((Convert.ToDouble(dataGridView2.Rows[i].Cells[1].Value) - M_null)/2);
                dataGridView2.Rows[i].Cells[13].Value = L;
                dataGridView2.Rows[i].Cells[14].Value = R;
                if (R-L==0)
                {
                    dataGridView2.Rows[i].Cells[15].Value = "Предаварийное";
                }
                if (R < L)
                {
                    dataGridView2.Rows[i].Cells[15].Value = "Нормальное";
                }
                else
                {
                    dataGridView2.Rows[i].Cells[15].Value = "Аварийное";
                }

                L = 0;
                R = 0;

            }

            dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[7].Value = par_A * (sum_M_progn / (dataGridView2.Rows.Count - 2)) + (1 - par_A) * Convert.ToDouble(dataGridView2.Rows[dataGridView2.Rows.Count - 3].Cells[7].Value);
            Console.WriteLine("Среднее знач M прогн {0}", sum_M_progn / (dataGridView2.Rows.Count - 2));
            Console.WriteLine("Последнее значение {0}", Convert.ToDouble(dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[7].Value));
            dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[8].Value = par_A * (sum_alfa_progn / (dataGridView2.Rows.Count - 2)) + (1 - par_A) * Convert.ToDouble(dataGridView2.Rows[dataGridView2.Rows.Count - 3].Cells[8].Value);
            dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[9].Value = par_A * (sum_Mplus_progn / (dataGridView2.Rows.Count - 2)) + (1 - par_A) * Convert.ToDouble(dataGridView2.Rows[dataGridView2.Rows.Count - 3].Cells[9].Value);
            dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[10].Value = par_A * (sum_Mminus_progn / (dataGridView2.Rows.Count - 2)) + (1 - par_A) * Convert.ToDouble(dataGridView2.Rows[dataGridView2.Rows.Count - 3].Cells[10].Value);
            dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[11].Value = par_A * (sum_alfaplus_progn / (dataGridView2.Rows.Count - 2)) + (1 - par_A) * Convert.ToDouble(dataGridView2.Rows[dataGridView2.Rows.Count - 3].Cells[11].Value);
            dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[12].Value = par_A * (sum_alfaminus_progn / (dataGridView2.Rows.Count - 2)) + (1 - par_A) * Convert.ToDouble(dataGridView2.Rows[dataGridView2.Rows.Count - 3].Cells[12].Value);
            L = Math.Abs(Convert.ToDouble(dataGridView2.Rows[dataGridView2.Rows.Count-2].Cells[9].Value) - Convert.ToDouble(dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[10].Value));
            R = Math.Abs((Convert.ToDouble(dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[7].Value) - M_null) / 2);
            dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[13].Value = L;
            dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[14].Value = R;
            if (R - L == 0)
            {
                dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[15].Value = "Предаварийное";
            }
            if (R < L)
            {
                dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[15].Value = "Нормальное";
            }
            else
            {
                dataGridView2.Rows[dataGridView2.Rows.Count - 2].Cells[15].Value = "Аварийное";
            }

        }
        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {

            Decomposition1();

            for (int x = 0; x < checkedListBox1.Items.Count; x++)
            {
                checkedListBox1.SetItemChecked(x, true);
            }
            checkedListBox1.SelectedItem = 1;
            Decomposition2();
            }


        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            charts_init(chart1, alpha_minus_min, alpha_plus_max);
            charts_init(chart2, alpha_minus_min, alpha_plus_max);
            charts_init(chart3, m_minus_min, m_minus_max);

            chart1.Series.Clear();
            chart2.Series.Clear();
            chart3.Series.Clear();
            if (checkedListBox1.CheckedItems.Count != 0)
            {
                // If so, loop through all checked items and print results.
                Console.WriteLine("alpha_minus_min {0}", alpha_minus_min);
                Console.WriteLine("alpha_plus_max {0}", alpha_plus_max);
                Console.WriteLine("m_minus_min {0}", m_minus_min);
                Console.WriteLine("m_minus_max {0}", m_minus_max);
                 CreateSerie(0, 2, "alp", chart2, false, 1, alpha_minus_min, alpha_plus_max, dataGridView2);
                 CreateSerie(0, 5, "alp+", chart2, false, 1, alpha_minus_min, alpha_plus_max, dataGridView2);
                 CreateSerie(0, 6, "alp-", chart2, false, 1, alpha_minus_min, alpha_plus_max, dataGridView2);
                 CreateSerie(0, 1, "t_m", chart3, false, 1, m_minus_min, m_minus_max, dataGridView2);
                 CreateSerie(0, 3, "t_m+", chart3, false, 1, m_minus_min, m_minus_max, dataGridView2);
                 CreateSerie(0, 4, "t_m-", chart3, false, 1, m_minus_min, m_minus_max, dataGridView2);

                for (int x = 0; x < checkedListBox1.Items.Count; x++)
                {
                    if (checkedListBox1.GetItemChecked(x))
                    {
                        
                        switch (x)
                        {
                            case 0:
                                CreateSerie(1, 2, "m_alpha", chart1, true, 1, alpha_minus_min, alpha_plus_max, dataGridView2);
                                break;
                            case 1:
                                CreateSerie(3, 5, "m+_alpha+", chart1, true, 1, alpha_minus_min, alpha_plus_max, dataGridView2);

                                break;
                            case 2:
                                CreateSerie(4, 6, "m-_alpha-", chart1, true, 1, alpha_minus_min, alpha_plus_max, dataGridView2);
                                break;
                            case 3:
                                CreateSerie(7, 8, "mprogn_alphaprogn", chart1, true, 1, alpha_minus_min, alpha_plus_max, dataGridView2);
                                break;
                            case 4:
                                CreateSerie(9, 11, "9_11", chart1, true, 1, alpha_minus_min, alpha_plus_max, dataGridView2);
                                break;
                            case 5:
                                CreateSerie(10, 12, "10_12", chart1, true, 1, alpha_minus_min, alpha_plus_max, dataGridView2);
                                break;

                        }
                    }
                }

            }
        }
        private void checkedListBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            chart6.Series.Clear();
            Console.WriteLine("decomp4_min {0}", decomp4_min);
            Console.WriteLine("decomp4_max {0}", decomp4_max);
            charts_init(chart6, decomp4_min, decomp4_max);
            for (int x = 0; x < checkedListBox3.Items.Count; x++)
            {
                if (checkedListBox3.GetItemChecked(x))
                {
                    CreateSerie(0, x, Convert.ToString(x+1), chart6, true, 1, alpha_minus_min, alpha_plus_max, dataGridView1);
                }
            }
        }
        private void Decomposition2()
        {
            string query = "SELECT data FROM `images` WHERE `id`= 1";
            SQLiteCommand cmd = new SQLiteCommand(query, SQLiteConn);

            SQLiteDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                byte[] a = (System.Byte[])rdr[0];
                pictureBox1.Image = ByteToImage(a);
            }
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.Image = new Bitmap(pictureBox1.Image);
            


        }

        private void button6_Click(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = -1;
            comboBox1.Items.Clear();
            string[] name_blocks = { "Блок A", "Блок B", "Блок C", "Блок D", "Блок E", "Блок F" };
            double kol = Convert.ToDouble(textBox3.Text);
            for (int i = 0; i < kol; i++)
            {
                comboBox1.Items.Add(name_blocks[i]);
            }
            /*            double kol_dotCount = dataGridView1.Columns.Count-1;
                        double kol_dotblockCount = Math.Truncate(kol_dotCount/kol);
                        Console.WriteLine(kol_dotCount);
                        Console.WriteLine(kol_dotblockCount);*/

        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            if (comboBox1.SelectedIndex != -1)
            {
                double metka = 0;
                double kol = Convert.ToDouble(textBox3.Text);
                double kol_dotCount = dataGridView1.Columns.Count - 1;
                double kol_dotblockCount = Math.Truncate(kol_dotCount / kol);
                Console.WriteLine(kol_dotCount);
                Console.WriteLine(kol_dotblockCount);
                for (int i = 1; i <= kol_dotblockCount; i++)
                {
                    metka = comboBox1.SelectedIndex * kol_dotblockCount + i;
                    listBox1.Items.Add(metka);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            double temp = 0;
            listBox2.Items.Add(listBox1.SelectedItem);
            listBox1.Items.Remove(listBox1.SelectedItem);
            if (listBox2.Items.Count > 1)
            {
                for (int i = 0; i < listBox2.Items.Count; i++)
                {
                    for (int j = i + 1; j < listBox2.Items.Count; j++)
                    {
                        if (Convert.ToDouble(listBox2.Items[i]) > Convert.ToDouble(listBox2.Items[j]))
                        {
                            temp = Convert.ToDouble(listBox2.Items[i]);
                            listBox2.Items[i] = Convert.ToDouble(listBox2.Items[j]);
                            listBox2.Items[j] = temp;
                        }
                    }
                }
            }

        }

        private void button8_Click(object sender, EventArgs e)
        {
            double temp = 0;
            listBox1.Items.Add(listBox2.SelectedItem);
            listBox2.Items.Remove(listBox2.SelectedItem);
            if (listBox1.Items.Count > 1)
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    for (int j = i + 1; j < listBox1.Items.Count; j++)
                    {
                        if (Convert.ToDouble(listBox1.Items[i]) > Convert.ToDouble(listBox1.Items[j]))
                        {
                            temp = Convert.ToDouble(listBox1.Items[i]);
                            listBox1.Items[i] = Convert.ToDouble(listBox1.Items[j]);
                            listBox1.Items[j] = temp;
                        }
                    }
                }
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            dataGridView3.Columns.Clear();
            dataGridView3.Rows.Clear();
            dataGridView3.Columns.Add("Эпоха", "Эпоха");
            dataGridView3.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            for (int i = 1; i <= listBox2.Items.Count; i++)
            {
                dataGridView3.Columns.Add(Convert.ToString(listBox2.Items[i - 1]), Convert.ToString(listBox2.Items[i - 1]));
                dataGridView3.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                dataGridView3.Rows.Add(dataGridView1.Rows[i].Cells[0].Value); ;
            }

            for (int i = 0; i < listBox2.Items.Count; i++)
            {
                for (int j = 0; j < dataGridView1.Rows.Count - 1; j++)
                {
                    Console.WriteLine(listBox2.Items[i]);
                    dataGridView3.Rows[j].Cells[i + 1].Value = Convert.ToDouble(dataGridView1.Rows[j].Cells[Convert.ToInt32(listBox2.Items[i])].Value);

                }
            }
            if (listBox2.Items.Count != 0)
            {
                tabPage7.Parent = tabControl2;
            }

        }

        private void tabControl2_MouseClick(object sender, MouseEventArgs e)
        {
            string[] name = { "Эпоха", "M", "alfa секунды", "M+", "M-", "alfa+ секунды", "alfa- секунды", "M(прогн)", "alfa(прогн) секунды", "M+(прогн)", "M-(прогн)", "alfa+(прогн) секунды", "alfa-(прогн) секунды", "L", "R", "Устойчивость" };
            double M = 0, M_plus = 0, M_minus = 0, alfa = 0, alfa_plus = 0, alfa_minus = 0, M_prev = 0, M_prev_plus = 0, M_prev_minus = 0, M_progn = 0, M_plus_progn = 0, M_minus_progn = 0, alfa_progn = 0, alfa_plus_progn = 0, alfa_minus_progn = 0, sum_M = 0, sum_M_plus = 0, sum_M_minus = 0, M_progn_prev = 0, alfa_progn_prev = 0, sum_alfa = 0, sum_alfa_plus = 0, sum_alfa_minus = 0, M_progn_prev_plus = 0, M_progn_prev_minus = 0, alfa_progn_prev_plus = 0, alfa_progn_prev_minus = 0, L = 0, M_null = 0, R = 0, sum_M_progn = 0, sum_Mplus_progn = 0, sum_Mminus_progn = 0, sum_alfa_progn = 0, sum_alfaplus_progn = 0, sum_alfaminus_progn = 0;

            dataGridView4.Columns.Clear();
            dataGridView4.Rows.Clear();
            for (int i = 0; i < name.Length; i++)
            {
                dataGridView4.Columns.Add(name[i], name[i]);
                dataGridView4.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            for (int i = 0; i < dataGridView3.Rows.Count - 1; i++)
            {
                dataGridView4.Rows.Add(dataGridView3.Rows[i].Cells[0].Value); ;
            }
            dataGridView4.Rows.Add("Прогноз");
            for (int i = 0; i < dataGridView3.Rows.Count - 1; i++) //M и alfa
            {
                for (int j = 1; j < dataGridView3.Columns.Count; j++)
                {
                    M += Math.Pow(Convert.ToDouble(dataGridView3.Rows[i].Cells[j].Value), 2);
                    M_plus += Math.Pow(Convert.ToDouble(dataGridView3.Rows[i].Cells[j].Value) + par_E, 2);
                    M_minus += Math.Pow(Convert.ToDouble(dataGridView3.Rows[i].Cells[j].Value) - par_E, 2);

                    alfa += Convert.ToDouble(dataGridView3.Rows[0].Cells[j].Value) * Convert.ToDouble(dataGridView3.Rows[i].Cells[j].Value);
                    alfa_plus += (Convert.ToDouble(dataGridView3.Rows[0].Cells[j].Value) + par_E) * (Convert.ToDouble(dataGridView3.Rows[i].Cells[j].Value) + par_E);
                    //Console.WriteLine(Convert.ToString(Convert.ToDouble(dataGridView3.Rows[0].Cells[j].Value) + par_E));
                    alfa_minus += (Convert.ToDouble(dataGridView3.Rows[0].Cells[j].Value) - par_E) * (Convert.ToDouble(dataGridView3.Rows[i].Cells[j].Value) - par_E);

                    /*                        Console.WriteLine(Convert.ToDouble(dataGridView3.Rows[0].Cells[j].Value));
                                            Console.WriteLine(Convert.ToDouble(dataGridView3.Rows[i].Cells[j].Value));*/

                }
                if (i == 0)
                {
                    /*Console.WriteLine("Присвоили М первой = {0}. Альфы = 0 ", Math.Sqrt(M));*/
                    M_prev = M;
                    M_prev_plus = M_plus;
                    M_prev_minus = M_minus;
                    alfa = 0;
                    alfa_minus = 0;
                    alfa_plus = 0;

                }
                else
                {
                    //Console.WriteLine("М первая {0}", Math.Sqrt(M_prev));
                    //Console.WriteLine("М {0}", Math.Sqrt(M));

                    alfa = alfa / (Math.Sqrt(M_prev) * Math.Sqrt(M));
                    /* Console.WriteLine("alpa_plus = {0}", alfa_plus);*/
                    alfa_plus = alfa_plus / (Math.Sqrt(M_prev_plus) * Math.Sqrt(M_plus));
                    /*double temp = (Math.Sqrt(M_prev_plus) * Math.Sqrt(M_plus));
                    Console.WriteLine("Делим это на {0} = {1}",temp, alfa_plus);*/
                    alfa_minus = alfa_minus / (Math.Sqrt(M_prev_minus) * Math.Sqrt(M_minus));
                    // Console.WriteLine("Альфа поделили {0}", alfa_plus);                
                    alfa = Math.Acos(alfa)*3600;
                    /* Console.WriteLine("Ща будем брать acos alfa ({0})", alfa_plus);*/
                    alfa_plus = Math.Acos(alfa_plus)*3600;
                    /*  Console.WriteLine("Взяли acos = {0}", alfa_plus);*/
                    alfa_minus = Math.Acos(alfa_minus)*3600;
                    //Console.WriteLine("Альфа косинус {0}", alfa_plus);
                }


                dataGridView4.Rows[i].Cells[1].Value = Math.Sqrt(M);
                dataGridView4.Rows[i].Cells[3].Value = Math.Sqrt(M_plus);
                dataGridView4.Rows[i].Cells[4].Value = Math.Sqrt(M_minus);
                dataGridView4.Rows[i].Cells[2].Value = alfa;
                dataGridView4.Rows[i].Cells[5].Value = alfa_plus;
                dataGridView4.Rows[i].Cells[6].Value = alfa_minus;
                sum_M += Math.Sqrt(M);
                sum_M_plus += Math.Sqrt(M_plus);
                sum_M_minus += Math.Sqrt(M_minus);
                sum_alfa += alfa;
                sum_alfa_plus += alfa_plus;
                sum_alfa_minus += alfa_minus;

                M = 0;
                M_plus = 0;
                M_minus = 0;
                alfa = 0;
                alfa_plus = 0;
                alfa_minus = 0;

            }
            for (int i = 0; i < dataGridView4.Rows.Count - 2; i++) //M(прогн) и alfa(прогн)
            {
                if (i == 0)
                {
                    M_progn = par_A * Convert.ToDouble(dataGridView4.Rows[0].Cells[1].Value) + (1 - par_A) * (sum_M / (dataGridView4.Rows.Count - 2));
                    alfa_progn = par_A * Convert.ToDouble(dataGridView4.Rows[0].Cells[2].Value) + (1 - par_A) * (sum_alfa / (dataGridView4.Rows.Count - 2));
                    M_plus_progn = par_A * Convert.ToDouble(dataGridView4.Rows[0].Cells[3].Value) + (1 - par_A) * (sum_M_plus / (dataGridView4.Rows.Count - 2));
                    M_minus_progn = par_A * Convert.ToDouble(dataGridView4.Rows[0].Cells[4].Value) + (1 - par_A) * (sum_M_minus / (dataGridView4.Rows.Count - 2));
                    alfa_plus_progn = par_A * Convert.ToDouble(dataGridView4.Rows[0].Cells[5].Value) + (1 - par_A) * (sum_alfa_plus / (dataGridView4.Rows.Count - 2));
                    alfa_minus_progn = par_A * Convert.ToDouble(dataGridView4.Rows[0].Cells[6].Value) + (1 - par_A) * (sum_alfa_minus / (dataGridView4.Rows.Count - 2));
                    dataGridView4.Rows[i].Cells[7].Value = M_progn;
                    dataGridView4.Rows[i].Cells[8].Value = alfa_progn * 3600;
                    dataGridView4.Rows[i].Cells[9].Value = M_plus_progn;
                    dataGridView4.Rows[i].Cells[10].Value = M_minus_progn;
                    dataGridView4.Rows[i].Cells[11].Value = alfa_plus_progn * 3600;
                    dataGridView4.Rows[i].Cells[12].Value = alfa_minus_progn * 3600;
                    M_progn_prev = M_progn;
                    alfa_progn_prev = alfa_progn;
                    M_progn_prev_plus = M_plus_progn;
                    M_progn_prev_minus = M_minus_progn;
                    alfa_progn_prev_plus = alfa_plus_progn;
                    alfa_progn_prev_minus = alfa_minus_progn;
                    sum_M_progn += M_progn;
                    sum_Mplus_progn += M_plus_progn;
                    sum_Mminus_progn += M_minus_progn;
                    sum_alfa_progn += alfa_progn;
                    sum_alfaplus_progn += alfa_plus_progn;
                    sum_alfaminus_progn += alfa_minus_progn;
                    alfa_progn = 0;
                    alfa_plus_progn = 0;
                    alfa_minus_progn = 0;
                    M_progn = 0;
                    M_plus_progn = 0;
                    M_minus_progn = 0;

                }
                else
                {
                    M_progn = par_A * Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value) + (1 - par_A) * M_progn_prev;
                    alfa_progn = par_A * Convert.ToDouble(dataGridView4.Rows[i].Cells[2].Value) + (1 - par_A) * alfa_progn_prev;
                    M_plus_progn = par_A * Convert.ToDouble(dataGridView4.Rows[i].Cells[3].Value) + (1 - par_A) * M_progn_prev_plus;
                    M_minus_progn = par_A * Convert.ToDouble(dataGridView4.Rows[i].Cells[4].Value) + (1 - par_A) * M_progn_prev_minus;
                    alfa_plus_progn = par_A * Convert.ToDouble(dataGridView4.Rows[i].Cells[5].Value) + (1 - par_A) * alfa_progn_prev_plus;
                    alfa_minus_progn = par_A * Convert.ToDouble(dataGridView4.Rows[i].Cells[6].Value) + (1 - par_A) * alfa_progn_prev_minus;
                    dataGridView4.Rows[i].Cells[7].Value = M_progn;
                    dataGridView4.Rows[i].Cells[8].Value = alfa_progn;
                    dataGridView4.Rows[i].Cells[9].Value = M_plus_progn;
                    dataGridView4.Rows[i].Cells[10].Value = M_minus_progn;
                    dataGridView4.Rows[i].Cells[11].Value = alfa_plus_progn;
                    dataGridView4.Rows[i].Cells[12].Value = alfa_minus_progn;
                    M_progn_prev = M_progn;
                    alfa_progn_prev = alfa_progn;
                    M_progn_prev_plus = M_plus_progn;
                    M_progn_prev_minus = M_minus_progn;
                    alfa_progn_prev_plus = alfa_plus_progn;
                    alfa_progn_prev_minus = alfa_minus_progn;
                    sum_M_progn += M_progn;
                    sum_Mplus_progn += M_plus_progn;
                    sum_Mminus_progn += M_minus_progn;
                    sum_alfa_progn += alfa_progn;
                    sum_alfaplus_progn += alfa_plus_progn;
                    sum_alfaminus_progn += alfa_minus_progn;
                    alfa_progn = 0;
                    alfa_plus_progn = 0;
                    alfa_minus_progn = 0;
                    M_progn = 0;
                    M_plus_progn = 0;
                    M_minus_progn = 0;
                }

            }

            for (int i = 0; i < dataGridView4.Rows.Count - 1; i++)
            {
                if (i == 0)
                {
                    M_null = Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value);
                    R = Math.Abs((Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value) - M_null) / 2);
                }

                L = Math.Abs(Convert.ToDouble(dataGridView4.Rows[i].Cells[3].Value) - Convert.ToDouble(dataGridView4.Rows[i].Cells[4].Value));
                R = Math.Abs((Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value) - M_null) / 2);
                dataGridView4.Rows[i].Cells[13].Value = L;
                dataGridView4.Rows[i].Cells[14].Value = R;
                if (R - L == 0)
                {
                    dataGridView4.Rows[i].Cells[15].Value = "Предаварийное";
                }
                if (R < L)
                {
                    dataGridView4.Rows[i].Cells[15].Value = "Нормальное";
                }
                else
                {
                    dataGridView4.Rows[i].Cells[15].Value = "Аварийное";
                }

                L = 0;
                R = 0;

            }

            dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[7].Value = par_A * (sum_M_progn / (dataGridView4.Rows.Count - 2)) + (1 - par_A) * Convert.ToDouble(dataGridView4.Rows[dataGridView4.Rows.Count - 3].Cells[7].Value);
            Console.WriteLine("Среднее знач M прогн {0}", sum_M_progn / (dataGridView4.Rows.Count - 2));
            Console.WriteLine("Последнее значение {0}", Convert.ToDouble(dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[7].Value));
            dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[8].Value = par_A * (sum_alfa_progn / (dataGridView4.Rows.Count - 2)) + (1 - par_A) * Convert.ToDouble(dataGridView4.Rows[dataGridView4.Rows.Count - 3].Cells[8].Value);
            dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[9].Value = par_A * (sum_Mplus_progn / (dataGridView4.Rows.Count - 2)) + (1 - par_A) * Convert.ToDouble(dataGridView4.Rows[dataGridView4.Rows.Count - 3].Cells[9].Value);
            dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[10].Value = par_A * (sum_Mminus_progn / (dataGridView4.Rows.Count - 2)) + (1 - par_A) * Convert.ToDouble(dataGridView4.Rows[dataGridView4.Rows.Count - 3].Cells[10].Value);
            dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[11].Value = par_A * (sum_alfaplus_progn / (dataGridView4.Rows.Count - 2)) + (1 - par_A) * Convert.ToDouble(dataGridView4.Rows[dataGridView4.Rows.Count - 3].Cells[11].Value);
            dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[12].Value = par_A * (sum_alfaminus_progn / (dataGridView4.Rows.Count - 2)) + (1 - par_A) * Convert.ToDouble(dataGridView4.Rows[dataGridView4.Rows.Count - 3].Cells[12].Value);
            L = Math.Abs(Convert.ToDouble(dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[9].Value) - Convert.ToDouble(dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[10].Value));
            R = Math.Abs((Convert.ToDouble(dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[7].Value) - M_null) / 2);
            dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[13].Value = L;
            dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[14].Value = R;
            if (R - L == 0)
            {
                dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[15].Value = "Предаварийное";
            }
            if (R < L)
            {
                dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[15].Value = "Нормальное";
            }
            else
            {
                dataGridView4.Rows[dataGridView4.Rows.Count - 2].Cells[15].Value = "Аварийное";
            }

        }

        private void tabControl1_TabIndexChanged(object sender, EventArgs e)
        {
            checkedListBox3.Items.Clear();
            for (int i = 1; i < dataGridView1.Columns.Count; i++)
            {
                checkedListBox3.Items.Add(Convert.ToString(i));
            }
            Console.WriteLine("Запихнули checkedListBox3");
        }

        private void checkedListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            chart3.Series.Clear();
            chart4.Series.Clear();
            chart5.Series.Clear();
            


            for (int x = 0; x < checkedListBox2.Items.Count; x++)
            {
                if (checkedListBox2.GetItemChecked(x))
                {

                    switch (x)
                    {
                        case 0:
                            CreateSerie(1, 2, checkedListBox2.Items[x].ToString(), chart3, true, 1, alpha_minus_min, alpha_plus_max, dataGridView4);
                            break;
                        case 1:
                            CreateSerie(3, 5, checkedListBox2.Items[x].ToString(), chart3, true, 1, alpha_minus_min, alpha_plus_max, dataGridView4);
                            break;
                        case 2:
                            CreateSerie(4, 6, checkedListBox2.Items[x].ToString(), chart3, true, 1, alpha_minus_min, alpha_plus_max, dataGridView4);
                            break;
                        case 3:
                            CreateSerie(7, 8, checkedListBox2.Items[x].ToString(), chart4, true, 1, alpha_minus_min, alpha_plus_max, dataGridView4);
                            break;
                        case 4:
                            CreateSerie(9, 11, checkedListBox2.Items[x].ToString(), chart4, true, 1, alpha_minus_min, alpha_plus_max, dataGridView4);
                            break;
                        case 5:
                            CreateSerie(10, 12, checkedListBox2.Items[x].ToString(), chart4, true, 1, alpha_minus_min, alpha_plus_max, dataGridView4);
                            break;
                        case 6:
                            CreateSerie(0, 1, checkedListBox2.Items[x].ToString(), chart5, true, 1, alpha_minus_min, alpha_plus_max, dataGridView4);
                            CreateSerie(0, 3, checkedListBox2.Items[x].ToString() + "_", chart5, true, 1, alpha_minus_min, alpha_plus_max, dataGridView4);
                            CreateSerie(0, 4, checkedListBox2.Items[x].ToString() + "__", chart5, true, 1, alpha_minus_min, alpha_plus_max, dataGridView4);
                            break;
                        

                    }
                }
            }
        }
    }
}
