/*
 * Name: Konstantin Shvedov
 * Date: 19/10/2018
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpreadsheetEngine
{
    //dll cell class that has its variables privated and protected,
    //getters and setter can be called to retrive them
    public abstract class Cell : INotifyPropertyChanged
    {
        private int _rowIndex;
        private int _columnIndex;
        protected string _cellCont;
        protected string _value;

        public event PropertyChangedEventHandler PropertyChanged;
        
        public Cell(int newRowIndex, int newColumnIndex)
        {
            _rowIndex = newRowIndex;
            _columnIndex = newColumnIndex;
        }

        public int Row
        {
            get
            {
                return _rowIndex;
            }
        }

        public int Column
        {
            get
            {
                return _columnIndex;
            }
        }

        public string Text
        {
            get
            {
                return _cellCont;
            }

            set
            {
                if(_cellCont != value)
                {
                    _cellCont = value;
                   OnPropertyChanged();
                }
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
