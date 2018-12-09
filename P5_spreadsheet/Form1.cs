/*
 * Name: Konstantin Shvedov
 * Date: 26/11/2018
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpreadsheetEngine;
using System.IO;

namespace P5_spreadsheet
{
    public partial class Form1 : Form
    {
        public const double _curVersion = 5.0;
        private Spreadsheet _Spreadsheet = new Spreadsheet(50, 26);
        private Cell _curEditCell;

        public Form1()
        {
            InitializeComponent();
            dataGridView1.Dock = DockStyle.Fill;
            _curEditCell = _Spreadsheet.getCell(0, 0);
        }


        private void CellSignalPropChanged(object sender, PropertyChangedEventArgs e)
        {
            Cell curCell = sender as Cell;
            if ((e.PropertyName == "Value") || (e.PropertyName == "Text"))
            {
                dataGridView1[curCell.Column, curCell.Row].Value = curCell.Value;
            }
        }

        //starts editing the cells
        void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            int curRow = e.RowIndex;
            int curCol = e.ColumnIndex;
            Cell _curEditCell = _Spreadsheet.getCell(curRow, curCol);
            dataGridView1.Rows[curRow].Cells[curCol].Value = _curEditCell.Text;
            cellContTextBox.Text = _curEditCell.Text;
        }

        //called when Form1 is created, creats all the rows and columns
        private void Form1_Load(object sender, EventArgs e)
        {
            _Spreadsheet.CellPropChanged += CellSignalPropChanged;
            dataGridView1.CellBeginEdit += dataGridView1_CellBeginEdit;
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;

            dataGridView1.Columns.Clear();
            for (int i = 65; i <= 91; i++)
            {
                DataGridViewColumn col = new DataGridViewTextBoxColumn();
                col.HeaderText = char.ConvertFromUtf32(i);
                col.Name = char.ConvertFromUtf32(i);
                dataGridView1.Columns.Add(col);
            }

            dataGridView1.Rows.Add(50);

            for (int i = 0; i < 50; i++)
            {
                dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
        }

        //Funcrtion for when cell is clicked sets the it to the cell thats edited
        //also sets the top text box to content of the cell
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Cell nEditCell = _Spreadsheet.getCell(e.RowIndex, e.ColumnIndex);
            if (nEditCell != null)
            {
                _curEditCell = nEditCell;
                cellContTextBox.Text = nEditCell.Text;
            }
            cellContTextBox.Focus();
        }

        //calls event onthe demo button kick
        private void button1_Click(object sender, EventArgs e)
        {
            Random tempRand = new Random();

            for (int i = 0; i < 50; i++)
            {
                _Spreadsheet._Array[i, 1].Text = "This is cell B" + (i + 1).ToString();
            }

            for (int i = 0; i < 50; i++)
            {
                _Spreadsheet._Array[i, 0].Text = "=B" + (i + 1).ToString();
            }
            for (int i = 0; i < 50; i++)
            {
                int nRow = tempRand.Next(0, 49);
                int nCol = tempRand.Next(2, 25);
                _Spreadsheet._Array[nRow, nCol].Text = "Privet MIR!";
            }
        }

        //checks if enter key has been pressed
        private void cellContTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Return)
            {
                _curEditCell.Text = cellContTextBox.Text;
            }
        }

        //ends editing the cell
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int curRow = e.RowIndex;
            int curCol = e.ColumnIndex;

            Cell nEditCell = _Spreadsheet.getCell(e.RowIndex, e.ColumnIndex);

            try
            {
                nEditCell.Text = dataGridView1.Rows[curRow].Cells[curCol].Value.ToString();
            }
            catch (NullReferenceException)
            {
                nEditCell.Text = "";
            }
        }

        private void loadFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream tempStream;
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.InitialDirectory = "c:\\";
            openFile.Filter = "XML files (*.xml)|*.xml";
            openFile.FilterIndex = 2;
            openFile.RestoreDirectory = true;

            if ((openFile.ShowDialog() == DialogResult.OK) && ((tempStream = openFile.OpenFile()) != null))
            {
                _Spreadsheet.FormatTable();
                _Spreadsheet.LoadXML(tempStream);
                tempStream.Close();
            }
        }

        private void saveFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream tempStream;
            SaveFileDialog saveFile = new SaveFileDialog();

            saveFile.InitialDirectory = "c:\\";
            saveFile.Filter = "XML files (*.xml)|*.xml";
            saveFile.FilterIndex = 2;
            saveFile.RestoreDirectory = true;

            if ((saveFile.ShowDialog() == DialogResult.OK) && ((tempStream = saveFile.OpenFile()) != null))
            {
                _Spreadsheet.SaveXML(tempStream);
                tempStream.Close();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aboutProgram = new About(_curVersion);
            aboutProgram.Location = Location;
            aboutProgram.Show();
        }
    }
}
