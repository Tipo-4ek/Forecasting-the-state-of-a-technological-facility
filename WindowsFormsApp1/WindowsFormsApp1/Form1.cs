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
        double par_E=-1;
        double par_A=-1;
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
            }
            }

        private void button3_Click(object sender, EventArgs e) // Удаление
        {
            string sql_query = "DELETE FROM `Данные` WHERE `Эпоха` = " + Convert.ToString(dataGridView1.Rows.Count-2);
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
            string sql_query = "INSERT INTO `Данные` VALUES (" + Convert.ToString(dataGridView1.Rows.Count -1) + ", ";
            double val;
            double max = 0 ;
            for (int i = 1; i<dataGridView1.Columns.Count; i++) // Первый столбец без значений
            {
                for (int j = 1; j<dataGridView1.Rows.Count-2; j++) // Первая строка без значений
                {
                    double difference = Math.Abs(Convert.ToDouble(dataGridView1.Rows[j + 1].Cells[i].Value) - Convert.ToDouble(dataGridView1.Rows[j].Cells[i].Value));
                  if ( difference > max) {
                        max = difference;
                    }
                }
                double difference1 = rnd.NextDouble() * (max - -max) + (-max);
                val = Math.Round(Convert.ToDouble(dataGridView1.Rows[dataGridView1.Rows.Count-2].Cells[i].Value) + difference1,4);
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

        private void button4_Click(object sender, EventArgs e)
        {
            ELabel.Text = textBox1.Text + " , ";
            ALabel.Text = textBox2.Text;
            par_E = Convert.ToDouble(textBox1.Text);
            par_A = Convert.ToDouble(textBox2.Text);
        }

        private void Decomposition1()
        {
            string[] name = {"Эпоха","M","alfa","M+","M-","alfa+","alfa-","M(прогн)", "alfa(прогн)", "M+(прогн)", "M-(прогн)", "alfa+(прогн)", "alfa-(прогн)","R","L","Устойчивость"};
            double M=0,M_plus=0,M_minus=0,alfa=0,alfa_plus=0,alfa_minus=0,M_prev=0,M_prev_plus=0,M_prev_minus=0, M_progn = 0, M_plus_progn = 0, M_minus_progn = 0, alfa_progn = 0, alfa_plus_progn = 0, alfa_minus_progn = 0,sum_M=0,sum_M_plus=0,sum_M_minus=0, M_progn_prev = 0;

            double[,] decomp = new double[dataGridView1.Rows.Count, dataGridView1.Columns.Count];
            dataGridView2.Columns.Clear();
            dataGridView2.Rows.Clear();
            for (int i = 0; i < 16; i++)
            {
                dataGridView2.Columns.Add(name[i],name[i]);
                dataGridView2.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                dataGridView2.Rows.Add(dataGridView1.Rows[i].Cells[0].Value); ;
            }

            for (int i = 0; i < dataGridView1.Rows.Count-1; i++) //M и alfa
            {
                for (int j = 1; j < dataGridView1.Columns.Count; j++)
                {
                    M += Math.Pow(Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value),2);
                    M_plus += Math.Pow(Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value) + par_E, 2);
                    M_minus += Math.Pow(Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value) - par_E, 2);

                    alfa += Convert.ToDouble(dataGridView1.Rows[0].Cells[j].Value) * Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value);
                    alfa_plus += (Convert.ToDouble(dataGridView1.Rows[0].Cells[j].Value)+par_E) * (Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value)+par_E);
                    //Console.WriteLine(Convert.ToString(Convert.ToDouble(dataGridView1.Rows[0].Cells[j].Value) + par_E));
                    alfa_minus+= (Convert.ToDouble(dataGridView1.Rows[0].Cells[j].Value) - par_E) * (Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value) - par_E);

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
                    alfa = Math.Acos(alfa);
                   /* Console.WriteLine("Ща будем брать acos alfa ({0})", alfa_plus);*/
                    alfa_plus = Math.Acos(alfa_plus);
                  /*  Console.WriteLine("Взяли acos = {0}", alfa_plus);*/
                    alfa_minus = Math.Acos(alfa_minus);
                     //Console.WriteLine("Альфа косинус {0}", alfa_plus);
                }
         

                dataGridView2.Rows[i].Cells[1].Value = Math.Sqrt(M);
                dataGridView2.Rows[i].Cells[3].Value = Math.Sqrt(M_plus);
                dataGridView2.Rows[i].Cells[4].Value = Math.Sqrt(M_minus);
                dataGridView2.Rows[i].Cells[2].Value = alfa;
                dataGridView2.Rows[i].Cells[5].Value = alfa_plus;
                dataGridView2.Rows[i].Cells[6].Value = alfa_minus;
                sum_M += Math.Sqrt(M);
                sum_M_plus += Math.Sqrt(M_plus);
                sum_M_minus += Math.Sqrt(M_minus);

                M = 0;
                M_plus = 0;
                M_minus = 0;
                alfa = 0;
                alfa_plus = 0;
                alfa_minus = 0;
                
            }
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++) //M(прогн) и alfa(прогн)
            {
                if (i == 0)
                {
                    M_progn = par_A * Convert.ToDouble(dataGridView2.Rows[0].Cells[1].Value) + (1 - par_A) * (sum_M / (dataGridView2.Rows.Count - 1));
                    dataGridView2.Rows[i].Cells[7].Value = M_progn;
                    Console.WriteLine(Convert.ToDouble(dataGridView2.Rows[0].Cells[1].Value));
                    M_progn_prev = M_progn;
                    M_progn = 0;
                }
                else
                {
                    M_progn = par_A * Convert.ToDouble(dataGridView2.Rows[i].Cells[1].Value) + (1 - par_A) * M_progn_prev;
                    dataGridView2.Rows[i].Cells[7].Value = M_progn;
                    M_progn_prev = M_progn;
                    M_progn = 0;
                }

            }
                }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {

            Decomposition1();
        }
    }
}
