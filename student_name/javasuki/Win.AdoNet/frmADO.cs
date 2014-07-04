using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using Mini.Data;

namespace Win.AdoNet
{
    public partial class frmADO : Form
    {
        public frmADO()
        {
            InitializeComponent();
            if (this.DesignMode) return;

            string cnfgName = Application.ExecutablePath + ".config";
            DbConfig.Get(cnfgName);
        }

        int CurrentOrdID
        {
            get
            {
                return this.tOrdNO.Tag == DBNull.Value ? 0 : Convert.ToInt32(this.tOrdNO.Tag);
            }
        }

        Action<Exception> ShowExceptionAction = ex => MessageBox.Show(ex.Message);
        private void Form1_Load(object sender, EventArgs e)
        {
            this.initGridView();
            var dt = DbFactory.DbSelect("select * from OrdData", ShowExceptionAction);
            this.bindSrc.DataSource = dt;

            this.tOrdNO.Bind(t => t.Tag, this.bindSrc, "OrdID");
            this.tOrdNO.Bind(t => t.Text, this.bindSrc, "OrdNO"); 
            this.dDate.Bind(d => d.Value, this.bindSrc, "OrdTime");
            this.cmbCust.Bind(c => c.SelectedValue, this.bindSrc, "CustID");
            this.fillCustomer();
            this.fillOrderDetails(this.CurrentOrdID);
        }

        #region init gridview
        private void initGridView()
        {
            foreach (string dbFld in new List<string> { "AutoID", "OrdID", "PrdID", "Price", "QNum", "LPrice" })
            {
                var col = this.dataGridView1.Columns[dbFld];
                col.DataPropertyName = dbFld;
                if (dbFld == "OrdID")
                    col.Visible = false;
                else if (dbFld == "PrdID")
                {
                    var cmbCol = col as DataGridViewComboBoxColumn;
                    cmbCol.DataSource = DbFactory.DbSelect("select * from PrdData");
                    cmbCol.ValueMember = "PrdID";
                    cmbCol.DisplayMember = "PrdName";
                    var cmbCell = cmbCol.CellTemplate as DataGridViewComboBoxCell;
                }
            }
            this.dataGridView1.AutoGenerateColumns = false;
        }
        #endregion

        private void bindSrc_CurrentChanged(object sender, EventArgs e)
        {
            this.fillOrderDetails(this.CurrentOrdID);
        }

        private void fillCustomer()
        {
            var dt = DbFactory.DbSelect("select * from CustData", ShowExceptionAction);            
            this.cmbCust.DisplayMember = "CustName";
            this.cmbCust.ValueMember = "CustID";
            this.cmbCust.DataSource = dt;
        }

        private void fillOrderDetails(int ordID)
        {
            var dt = DbFactory.DbSelect("select * from OrdDetails where OrdID=" + ordID);
            this.dataGridView1.DataSource = dt;

            dt.TableNewRow += (s, e) =>
            {
                #region 新行
                e.Row["OrdID"] = CurrentOrdID;
                e.Row["Price"] = 0;
                e.Row["QNum"] = 1;
                e.Row["LPrice"] = 0;
                #endregion
            };

            dt.RowChanged += (s, e) =>
            {
                //TODO：有效检查
                #region insert/update
                var lst = new List<object>
                {
                    new { Name="p0", Value=e.Row["OrdID"]},
                    new { Name="p1", Value=e.Row["PrdID"]},
                    new { Name="p2", Value=e.Row["Price"]},
                    new { Name="p3", Value=e.Row["QNum"]},
                    new { Name="p4", Value=e.Row["LPrice"]}
                };

                if (e.Action == DataRowAction.Add)
                {
                    string strSQL = "insert into OrdDetails(OrdID,PrdID,Price,QNum,LPrice) values(@p0,@p1,@p2,@p3,@p4)";
                    int autoValue = DbFactory.DbInsert(strSQL, lst, "OrdID", ex => e.Row.RowError = ex.Message);
                    e.Row["AutoID"] = autoValue;
                }
                else if (e.Action == DataRowAction.Change)
                {
                    lst.RemoveAt(0);
                    string strSQL = "update OrdDetails set PrdID=@p1,Price=@p2,QNum=@p3,LPrice=@p4 where AutoID=" + e.Row["AutoID"];
                    DbFactory.DbUpdate(strSQL, lst, ex => e.Row.RowError = ex.Message);
                }
                #endregion
            };

            dt.RowDeleting += (s, e) =>
            {
                //TODO：有效检查
                #region delete
                string strSQL = "delete from OrdDetails where AutoID=" + e.Row["AutoID"];                
                DbFactory.DbDelete(strSQL, dbAct: ex => e.Row.RowError = ex.Message);
                #endregion
            };
        }

        #region 价格
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (this.dataGridView1.Columns[e.ColumnIndex].DataPropertyName == "PrdID")
            {                
                int prdID = Convert.ToInt32(this.dataGridView1[e.ColumnIndex, e.RowIndex].Value); //产品ID
                var dt = (this.dataGridView1.Columns["PrdID"] as DataGridViewComboBoxColumn).DataSource as DataTable;
                
                var price = (from r 
                          in dt.AsEnumerable()
                          where r.Field<int>("PrdID") == prdID 
                          select r.Field<decimal>("Price")).First();
                this.dataGridView1[e.ColumnIndex + 1, e.RowIndex].Value = price; //默认Price
            }
            else if (this.dataGridView1.Columns[e.ColumnIndex].DataPropertyName == "Price" ||
                    this.dataGridView1.Columns[e.ColumnIndex].DataPropertyName == "QNum")
            {
                var c1 = this.dataGridView1[3, e.RowIndex].Value;  //Price
                var c2 = this.dataGridView1[4, e.RowIndex].Value;  //QNum
                if (c1 != null && c2 != null)
                    this.dataGridView1[5, e.RowIndex].Value = Convert.ToInt32(c1) * Convert.ToInt32(c2); //LPrice
            }
            else if (this.dataGridView1.Columns[e.ColumnIndex].DataPropertyName == "LPrice")
            {
                this.calcTotlePrice();
            }
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            this.calcTotlePrice();
        }

        private void calcTotlePrice()
        {
            double all=0;
            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                object o = this.dataGridView1.Rows[i].Cells["LPrice"].Value;
                if (o == null) continue;
                double d = 0;
                try
                {
                    all += double.TryParse(o.ToString(), out d) ? d : 0;
                }
                catch { }
            }

            this.label4.Text = all.ToString("c");
        }
        #endregion

        #region 订单主表
        private void bindSrc_AddingNew(object sender, AddingNewEventArgs e)
        {
            this.btnSave.Enabled = 
            this.cmbCust.Enabled = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //TODO：有效检查
            bool isNew = true; //判断是添加还是编辑
            if (isNew)
            {
                string strSQL = "insert into OrdData(OrdNO,CustID,OrdTime) values(@OrdNO,@CustID,@OrdTime)";
                var lst = new List<object>
                {
                    new { Name="OrdNO", Value=this.tOrdNO.Text},
                    new { Name="CustID", Value=this.cmbCust.SelectedValue},
                    new { Name="OrdTime", Value=this.dDate.Value}
                };

                int autoValue = DbFactory.DbInsert(strSQL, lst, "OrdID", ShowExceptionAction);
                lst.Insert(0, new { Name = "OrdID", Value = autoValue });

                var dt = (DataTable)this.bindSrc.DataSource;
                var row = dt.NewRow();
                foreach (dynamic dy in lst)
                    row[dy.Name] = dy.Value;
                dt.ImportRow(row);

                this.btnSave.Enabled = 
                this.cmbCust.Enabled = false;
            }
            else
            {

            }
        }

        private void bindingNavigatorDeleteItem_Click(object sender, EventArgs e)
        {
            int deleteOrdID = this.CurrentOrdID;

            //可使用DB触发器
            string strSQL = "delete from OrdData where OrdID=" + deleteOrdID + ";" +
                            "delete from OrdDetails where OrdID=" + deleteOrdID;

            DbFactory.DbDelete(strSQL, dbAct: ShowExceptionAction);
        }
        #endregion


    }
}
