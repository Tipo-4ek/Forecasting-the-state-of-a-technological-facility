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
   
        public Form1()
        {
            InitializeComponent();

        }
        private void Form1_Load(object sender, EventArgs e)
        {
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
        private string SQL_ALLTable1() { return "SELECT value FROM `parameters`"; }

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
                par_dtable.Clear();

                ShowTable(SQL_ALLTable());
                string pars = "SELECT * FROM `parameters`";
                SQLiteCommand command = new SQLiteCommand(pars, SQLiteConn);
                double par_e = -1;
                double par_a = -1;
                command.CommandText = "SELECT * FROM `parameters`";
                DataTable data = new DataTable();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                adapter.Fill(data);
                Console.WriteLine($"Прочитано {data.Rows.Count} записей из таблицы БД");
                foreach (DataRow row in data.Rows)
                {
                    Console.WriteLine($"id = {row.Field<long>("id")} name = {row.Field<string>("name")} value = {row.Field<double>("value")}");
                    if (row.Field<long>("id") == 1) { 
                        label3.Text = "E = " + row.Field<double>("value").ToString();
                        par_e = row.Field<double>("value");
                    }

                    if (row.Field<long>("id") == 2) {
                        label4.Text = "A = " + row.Field<double>("value").ToString();
                        par_a = row.Field<double>("value");
                    }
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

  
    }
}
