/*
 * Name: Konstantin Shvedov
 * Date: 26/11/2018
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace SpreadsheetEngine
{
    public class Spreadsheet
    {
        public Cell[,] _Array;
        public PropertyChangedEventHandler CellPropChanged;

        //class to make cell 
        private class SpreadSheetCell : Cell
        {
            private ExpTree _expTree;
            public Stack<SpreadSheetCell> _mConnections;
            public bool _hasConnections => _mConnections.Peek() != null;

            public SpreadSheetCell(int nRow, int nColumn) : base(nRow, nColumn)
            {
                _mConnections = new Stack<SpreadSheetCell>();
                _mConnections.Push(null);
            }

            public void setValue(string nValue)
            {
                base.Value = nValue;
            }

            public void makeNewExpTree(string expression)
            {
                if (expression == "")
                {
                    _expTree = null;
                }
                else
                {
                    _expTree = new ExpTree(expression);
                }
            }

            public void connectedCellPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                evalTree();
            }

            public void evalTree()
            {
                try
                {
                    if (_expTree != null)
                    {
                        base.Value = Convert.ToString(_expTree.Eval());
                    }
                    else if (_mConnections.Peek() != null)
                    {
                        base.Value = _mConnections.Peek().Value;
                    }
                    else
                    {
                        base.Value = base.Text;
                    }
                }
                catch
                {
                    base.Value = "#REF!";
                }
            }

            public bool hasSelfReference()
            {
                SpreadSheetCell nCell = this;
                return helperHasSelfReference(nCell, new HashSet<Tuple<int, int>>());
            }

            //this function makes sure that the refrenced cells dont cause a loop
            private bool helperHasSelfReference(SpreadSheetCell nCell, HashSet<Tuple<int, int>> nHashSet)
            {
                Tuple<int, int> tempCell = new Tuple<int, int>(nCell.Row, nCell.Column);
                if (nHashSet.Contains(tempCell)) return true;
                nHashSet.Add(tempCell);
                foreach (var tCell in nCell._mConnections.Where(a => a != null))
                {
                    if (helperHasSelfReference(tCell, nHashSet)) return true;
                }
                nHashSet.Remove(tempCell);
                return false;
            }
        }

        //when new exprssion is inputed into cell a new stack is formed of connections to cells
        private void SubscribeToCells(string expression, SpreadSheetCell nCell)
        {
            List<string> cellList = getCellsFromExp(expression);
            foreach(string item in cellList)
            {
                Tuple<int, int> nCellPos = getCellPosition(item);
                SpreadSheetCell tempCell = (SpreadSheetCell)getCell(nCellPos.Item1, nCellPos.Item2);
                tempCell.PropertyChanged += nCell.connectedCellPropertyChanged;
                nCell._mConnections.Push(tempCell);
            }
        }

        //when new exprssion is inputed into cell all previous connections to cell are terminated using this function
        private void UnSubscribeFromCells(SpreadSheetCell nCell)
        {
            while(nCell._mConnections.Peek() != null)
            {
                SpreadSheetCell tempCell = nCell._mConnections.Pop();
                tempCell.PropertyChanged -= nCell.connectedCellPropertyChanged;
            }
        }

        public List<string> getCellsFromExp(string expression)
        {
            string pattern = @"([A-Z]\d+)";
            MatchCollection matches = Regex.Matches(expression, pattern);
            List<string> cellList = new List<string>();
            foreach (Match match in matches)
            {
                if (match.ToString() != "") cellList.Add(match.ToString());
            }
            if (cellList.Count() > 0) return cellList;
            else return null;
        }

        //function that cheks if item is an operator
        private bool opChekHelper(char cChar)
        {
            switch (cChar)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '(':
                case ')':
                case '_':
                    return true;
            }
            return false;
        }

        //gets cell by using coordinates
        public Cell getCell(int nRow, int nColumn)
        {
            return _Array[nRow, nColumn];
        }

        //returns the cell name
        private string getCellName(int nRow, int nCol)
        {
            string cellName = char.ConvertFromUtf32('A' + nCol);
            cellName += (nRow + 1).ToString();
            return cellName;
        }

        //returns a tuple of the cells coordinates
        private Tuple<int, int> getCellPosition(string nCellName)
        {
            string tempString = nCellName;
            char aCol = tempString[0];
            int nRow = int.Parse(tempString.Substring(1)) - 1;
            int nCol = aCol - 'A';
            Tuple<int, int> nCellPos = new Tuple<int, int>(nRow, nCol);
            return nCellPos;
        }

        public int RowCount
        {
            get
            {
                return _Array.GetLength(0);
            }
        }

        public int ColumnCount
        {
            get
            {
                return _Array.GetLength(1);
            }
        }

        //makes new SpreadSheet with a specific amount cells
        public Spreadsheet(int newRows, int newColumns)
        {
            _Array = new Cell[newRows, newColumns];
            ExpTree newTree = new ExpTree("0");
            for (int x = 0; x < newRows; x++)
            {
                for (int y = 0; y < newColumns; y++)
                {
                    string cellName = getCellName(x, y);
                    _Array[x, y] = new SpreadSheetCell(x, y);
                    _Array[x, y].PropertyChanged += SignalPropertyChanged;
                    ExpTree.SetVar(cellName, _Array[x, y]);
                }
            }
            
        }

        public void SignalPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SpreadSheetCell tempCell = sender as SpreadSheetCell;
            if (e.PropertyName == "Text")
            {
                Check(sender as Cell);

                CellPropChanged(tempCell, new PropertyChangedEventArgs("Text"));
            }
            else
            {
                CellPropChanged(tempCell, new PropertyChangedEventArgs("Value"));
            }
        }

        //Checks if any of the cells refered to by the cell are out of bounds
        private bool outOfBounds(SpreadSheetCell nCell)
        {
            List<string> cellList = getCellsFromExp(nCell.Text.Substring(1));
            List<Tuple<int, int>> tempList = new List<Tuple<int, int>>();
            if (cellList != null)
            {
                foreach (string name in cellList)
                {
                    tempList.Add(getCellPosition(name));
                }
                foreach (var coord in tempList)
                {
                    if (coord.Item1 > this.RowCount || coord.Item2 > this.ColumnCount)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //checks contents content of cell and acts accordingly
        private void Check(Cell nCell)
        {
            SpreadSheetCell tempCell = nCell as SpreadSheetCell;
            if (tempCell._hasConnections) UnSubscribeFromCells(tempCell);//since the text within the cell has been changed
                                                                         //all connections with other cells are broken
            if (string.IsNullOrEmpty(tempCell.Text))
            {
                tempCell.setValue("");
                CellPropChanged(nCell, new PropertyChangedEventArgs("Value"));
            }
            else if (tempCell.Text[0] == '=')
            {
                if (outOfBounds(tempCell))
                {
                    tempCell.setValue("#REF!");
                    return;
                }
                else
                {
                    tempCell.makeNewExpTree(tempCell.Text.Substring(1));
                    if (getCellsFromExp(tempCell.Text.Substring(1)) != null)
                    {
                        SubscribeToCells(tempCell.Text.Substring(1), tempCell);
                    }
                    if (tempCell.hasSelfReference())
                    {
                        tempCell.setValue("#SELFREF!");
                        UnSubscribeFromCells(tempCell);
                    }
                    else
                    {
                        if(getCellsFromExp(tempCell.Text.Substring(1)).Count <= 1)
                        {
                            tempCell.makeNewExpTree("");
                        }
                        tempCell.evalTree();
                    }
                }
                CellPropChanged(nCell, new PropertyChangedEventArgs("Value"));
                    
            }
            else
            {
                tempCell.setValue(tempCell.Text);
                CellPropChanged(nCell, new PropertyChangedEventArgs("Value"));
            }
        }

        //Function that clears the whole table, (used before loading)
        public void FormatTable()
        {
            foreach (Cell cell in _Array)
            {
                if (!string.IsNullOrEmpty(cell.Text)) cell.Text = "";
            }
        }

        //Functions that returns a list of all the cells for a more efficient
        //and smaller saving file
        private List<Cell> GetUsedCells()
        {
            List<Cell> cellList = new List<Cell>();
            foreach (Cell cell in _Array)
            {
                if (!string.IsNullOrEmpty(cell.Text)) cellList.Add(cell);
            }
            return cellList;
        }

        //This fucntion loads all cells from under spreadsheet and places
        //the contentn in the spreadsheet
        public void LoadXML(Stream fStream)
        {
            XDocument xmlFile = XDocument.Load(fStream);
            var cells = xmlFile.Descendants("cell");

            foreach(XElement cell in cells)
            {
                int newCellRow = (int)cell.Descendants("row").First();
                int newCellColumn = (int)cell.Descendants("column").First();
                string newCellText = (string)cell.Descendants("content").First();
                Cell newCell = getCell(newCellRow, newCellColumn);
                newCell.Text = newCellText;
            }
        }

        //This fucntion first asks for a list of all used cells, and then
        //using a loop creates XElements that save the info for it
        public void SaveXML(Stream fStream)
        {
            List<Cell> usedCellList = GetUsedCells();
            XDocument xmlFile = new XDocument(new XDeclaration("1.0", "utf-8", null));
            XElement spreadSheet = new XElement("spreadsheet");
            xmlFile.Add(spreadSheet);
            foreach(Cell cell in usedCellList)
            {
                XElement usedCell = new XElement("cell",
                    new XElement("row", cell.Row.ToString()),
                    new XElement("column", cell.Column.ToString()),
                    new XElement("content", cell.Text));
                spreadSheet.Add(usedCell);
            }

            xmlFile.Save(fStream);
        }
    }
}
