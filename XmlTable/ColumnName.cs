using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XmlTable
{
    public partial class ColumnName : Form
    {
        public string value;
        public ColumnName()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
           
            if (XmlTableEditor.mainTable != null) {
                if (XmlTableEditor.mainTable.ColumnName(inputName.Text))
                {
                    Close();
                }
                else
                {
                    MessageBox.Show("列名已存在");
                }
            } 
        }
    }
}
