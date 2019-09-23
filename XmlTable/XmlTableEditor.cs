using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
namespace XmlTable
{
 
    public partial class XmlTableEditor : Form
    {
        public static XmlTableEditor mainTable;
        //DataTable data;
        // XmlDocument xml;
        string xmlPath;
        XmlDocument xmlDoc;
        Stack<InnerData> innerDataList;
        
        InnerData curData
        {
            get
            {
                return innerDataList.Peek();
            }
        }
        public XmlTableEditor()
        {
            InitializeComponent();
            innerDataList =new Stack<InnerData>();
        }
        public bool ColumnName(string name)
        {
            if (!curData.data.Columns.Contains(name))
            {
                curData.data.Columns.Add(name);
                return true;
            }
            return false;
        }
        private void XmlTableEditor_Load(object sender, EventArgs e)
        {
            string command = Environment.CommandLine;//获取进程命令行参数
            string[] para = command.Split('\"');
            if (para.Length > 3)
            {
                string pathC = para[3];
                OpenXml(pathC);
            }
            mainTable = this;
        }
        private void ParseInnerXml(int col,int row, string xmlstr)
        {
            var xmlDoc = new XmlDocument();
            var data = new DataTable();
            xmlDoc.LoadXml(" <子单元格编辑>" + xmlstr+ "</子单元格编辑>");
            var innerData = new InnerData()
            {
                data = data,
                col = col,
                row = row,
                xml = xmlDoc,
                xmlDoc=xmlDoc,
            };
            ParseXml(innerData);
            innerDataList.Push(innerData);
            
        }
        public static bool IsXml(string str)
        {
            return str.Contains("<") && str.Contains(">");
        }
        public bool IsGrid(XmlNode xml)
        {
            var isGrid= true;
            if (!IsXml(xml.FirstChild.InnerXml))
            {
                return false;
            }
            var name = xml.FirstChild.Name;
            foreach (XmlNode node in xml.ChildNodes)
            {
                if (node.Name != name)
                {
                    isGrid = false;
                    break;
                }
            }
           
            return isGrid;
        }
     
        private void ParseXml(InnerData innerData)
        {
            var root = innerData.xml.LastChild;
            var colList = root.FirstChild;
      
            innerData.isGrid = IsGrid(root);
            if (innerData.isGrid)
            {
             
                int x = 0;
                foreach (XmlNode rowList in root.ChildNodes)
                {
                    innerData.data.Rows.Add();
                
                    int y = 0;
                    foreach (XmlNode cell in rowList.ChildNodes)
                    {
                    
                        if (!innerData.data.Columns.Contains(cell.Name))
                        {
                            innerData.data.Columns.Add(cell.Name);
                        }
                        innerData.data.Rows[x][cell.Name] =cell.InnerXml;
                        y++;
                    }
                    x++;
                 
                }
            }
            else 
            {
                innerData.data.Columns.Add(colList.Name);

                int x = 0;
                foreach (XmlNode cell in root.ChildNodes)
                {
                    
                    int y = 0;
                 
                    if (!innerData.data.Columns.Contains(cell.Name))
                    {
                        innerData.data.Columns.Add(cell.Name);
                  
                    }
                    innerData.data.Rows.Add();
                    innerData.data.Rows[x][cell.Name]= cell.InnerXml;
                    y++;
                    x++;

                }
            }
            
            tableView.DataSource = innerData.data;
            CheckReadOnly();
          
        }
        private void CheckReadOnly()
        {
            for (int i = 0; i < tableView.ColumnCount; i++)
            {
                tableView.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                for (int j = 0; j < tableView.RowCount; j++)
                {
                    if (tableView.Rows[j].Cells[i].Value != null)
                    {
                        tableView[i, j].ReadOnly = IsXml(tableView.Rows[j].Cells[i].Value.ToString());
                    }
                   
                }
            }
        }
        private void ParseRootXml(string xmlstr)
        {
            xmlDoc = new XmlDocument();
            var data = new DataTable();
            xmlDoc.LoadXml(xmlstr);
            Text = xmlDoc.LastChild.Name + " - XmlViewer";
            var innerData = new InnerData()
            {
                data = data,
                xmlDoc=xmlDoc,
                col = -1,
                row = -1,
                xml = xmlDoc,
            };
            ParseXml(innerData);
            innerDataList.Push(innerData);
        }
        private void SaveXmlFile()
        {
            xmlDoc.Save(xmlPath);
            statusLabel.Text = "文件保存成功【" + xmlPath + "】";
        }
        private void OpenXml(string path)
        {
            xmlPath = path;
            var xmlstr = FileManager.Load(path);
            ParseRootXml(xmlstr);
        }
        private void openXmlFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (openXmlFileDialog.CheckPathExists)
            {
                OpenXml(openXmlFileDialog.FileName);
               
            }
        }

        private void xmlFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openXmlFileDialog.ShowDialog();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isEditMode)
            {
               tableView.EndEdit();
            }
          
            if (innerDataList.Count > 1)
            {
                var innerData = innerDataList.Pop();
                curData.data.Rows[innerData.row][innerData.col] = innerData.UpdateChange().InnerXml;
                tableView.DataSource = curData.data;
                CheckReadOnly();
            }
            else
            {
                curData.UpdateChange();
                SaveXmlFile();
            }
           
        }

        private void tableView_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }
            var cell = tableView[e.ColumnIndex, e.RowIndex];
            if (cell.ReadOnly)
            {
                ParseInnerXml(cell.ColumnIndex , e.RowIndex,cell.Value.ToString());
            }
        }


        DataGridViewCell selectCell;
        DataGridViewColumn selectColumn;
        DataGridViewRow selectRow;
        private void tableView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                statusLabel.Text = "选择表格【" + (e.RowIndex+1) + "," + tableView.Columns[e.ColumnIndex].Name + "】";
                selectCell = tableView[e.ColumnIndex, e.RowIndex];
                selectRow = null;
              //  deleteRowToolStripMenuItem.Enabled = false;
            }
            else
            {
                selectCell = null;
                if (e.ColumnIndex < 0 && e.RowIndex < 0)
                {
                    statusLabel.Text = "";
                }
                else if(e.ColumnIndex<0)
                {
                    statusLabel.Text = "选择【" + e.RowIndex+1+ "】行";
                    selectRow = tableView.Rows[e.RowIndex];
                    selectColumn = null;
                 //   deleteRowToolStripMenuItem.Enabled = true;
                }
                else if(e.RowIndex<0)
                {
                    statusLabel.Text = "选择【" +tableView.Columns[e.ColumnIndex].Name + "】列";
                    selectRow = null;
                    selectColumn = tableView.Columns[e.ColumnIndex];
                }
               
                   
            }
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isEditMode)
            {
                if (selectCell != null && selectCell.Value!= null)
                {
                    string text = selectCell.Value.ToString();
                    if (text == "")
                    {
                        Clipboard.SetText(" ");
                    }
                    else
                    {
                        Clipboard.SetText(text);
                    }
                    statusLabel.Text = "复制【" + selectCell.Value.ToString() + "】";
                }
            }
           
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isEditMode)
            {
                selectCell.Value = Clipboard.GetText();
                selectCell.ReadOnly = IsXml(selectCell.Value.ToString());
                statusLabel.Text = "粘贴【" + selectCell.Value.ToString() + "】";
            }
        }

        private void deleteRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectCell!=null)
            {
                selectCell.Value = "";
            }
            if (selectRow != null)
            {
                foreach (DataGridViewCell cell in selectRow.Cells)
                {
                    cell.Value = "";
                }
                //tableView.Rows.Remove(selectRow);
            }
        }

        private void AddColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectColumn != null)
            {
                var name = new ColumnName();
                name.Show();
            }
        }

        private void deleteColumntoolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectColumn != null)
            {
                tableView.Columns.Remove(selectColumn);
            }
        }
        bool isEditMode=false;
        private void tableView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            isEditMode = true;
            CopyToolStripMenuItem.Enabled = false;
            pasteToolStripMenuItem.Enabled = false;
            statusLabel.Text = "编辑单元格";
        }

        private void tableView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            isEditMode = false;
            CopyToolStripMenuItem.Enabled = true;
            pasteToolStripMenuItem.Enabled = true;
            statusLabel.Text = "结束编辑单元格";
        }

        private void tableView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }

    public class InnerData
    {
        public int col;
        public int row;
        public DataRowCollection lastCell;
        public DataTable data;
        public XmlNode xml;
        public XmlDocument xmlDoc;
        public bool isGrid;
        public XmlNode UpdateChange()
        {
            var root = xml.LastChild;

            List<XmlNode> removeList = new List<XmlNode>();
            for (int row = 0; row < data.Rows.Count; row++)
            {
                var rowNode = root.ChildNodes[row];
                
                if(rowNode==null)
                {
                    if(root.FirstChild != null)
                    {
                        rowNode = xmlDoc.CreateElement(root.FirstChild.Name);
                        root.AppendChild(rowNode);
                    }
                }
               
                if(XmlTableEditor.IsXml(root.FirstChild.InnerXml))
                {
                    for (int col = 0; col < data.Columns.Count; col++)
                    {
                        var cell = data.Rows[row][col].ToString();
                        if (string.IsNullOrEmpty(cell))
                        {
                            continue;
                        }
                        var name = data.Columns[col].ColumnName;
                        var cellNode = rowNode.SelectSingleNode(name);
                        if (cellNode != null)
                        {
                            cellNode.InnerXml = cell;
                        }
                        else
                        {
                            var node = xmlDoc.CreateElement(data.Columns[col].ColumnName);
                            node.InnerXml = cell;
                         
                            rowNode.AppendChild(node);
                        }
                    }
                   
                }
                else
                {
                    var info = data.Rows[row][0].ToString();
                  
                        rowNode.InnerXml = data.Rows[row][0].ToString();
                    
                }
                if (string.IsNullOrWhiteSpace(rowNode.InnerText))
                {
                    removeList.Add(rowNode);
                }
            }
            foreach (var node in removeList)
            {
                root.RemoveChild(node);
            }
            return root;
           
        }
    }
}
