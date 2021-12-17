using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Office.Interop.Excel;
using DataTable = System.Data.DataTable;
using __.Properties;

namespace __
{
    public partial class Form1 : Form
    {

        List<string> filename_unique_list = new List<string>();
        DataTable dataTable = new DataTable();
        string mysplitchar = "','";
        public Form1()
        {
            InitializeComponent();
            //button1.Text = "打开记录表";
        }

        private void savetofile()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string aa = dir + "\\TestTxt.txt";

            
            if (!File.Exists(aa))
            {
                FileStream fs1 = new FileStream(aa, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs1);
                foreach (string a in filename_unique_list)
                {
                    sw.WriteLine(a);
                }
                sw.Close();
                fs1.Close();
            }
            else
            {
                FileStream fs = new FileStream(aa, FileMode.Open, FileAccess.Write);
                StreamWriter sr = new StreamWriter(fs);
                sr.Write(renderstr());
                sr.Close();
                fs.Close();
            }
            toolStripStatusLabel1.Text = "保存成功，请见桌面！";

        }

        private void button1_Click(object sender, EventArgs e)
        {
            buttonset("处理中……");

            dataTable = this.GetDataFromExcelByCom();
            if (dataTable == null)
                return;
            //dataGridView1.DataSource = dataTable;
            buttonReset();

            button5.PerformClick();

        }


        #region excel to dataset

        DataTable GetDataFromExcelByCom(bool hasTitle = false)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Excel(*.xlsx)|*.xlsx|Excel(*.xls)|*.xls";
            
            //openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFile.Multiselect = true;
            if (openFile.ShowDialog() == DialogResult.Cancel)
            {
                //恢复button1可用状态
                buttonReset();
                return null;
            }
            DataTable dt = new DataTable();


            foreach (string Wenjian in openFile.FileNames)
            {
                Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Sheets sheets;
                object oMissiong = System.Reflection.Missing.Value;
                Microsoft.Office.Interop.Excel.Workbook workbook = null;

                try
                {
                    if (app == null) return null;
                    workbook = app.Workbooks.Open(Wenjian, oMissiong, oMissiong, oMissiong, oMissiong, oMissiong,
                        oMissiong, oMissiong, oMissiong, oMissiong, oMissiong, oMissiong, oMissiong, oMissiong, oMissiong);
                    sheets = workbook.Worksheets;
                    int ii = 0;
                    foreach( var sheet in sheets)
                    {
                        ii++;
                        //将数据读入到DataTable中
                        Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)sheets.get_Item(ii);//读取第一张表   
                        if (worksheet == null) return null;

                        int iRowCount = worksheet.UsedRange.Rows.Count;
                        int iColCount = worksheet.UsedRange.Columns.Count;
                        //生成列头
                        for (int i = 0; i < iColCount; i++)
                        {
                            var name = "column" + i;
                            if (hasTitle)
                            {
                                var txt = ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, i + 1]).Text.ToString();
                                if (!string.IsNullOrWhiteSpace(txt)) name = txt;
                            }
                            while (dt.Columns.Contains(name)) name = name + "_1";//重复行名称会报错。
                            dt.Columns.Add(new DataColumn(name, typeof(string)));
                        }
                        //生成行数据
                        Microsoft.Office.Interop.Excel.Range range;
                        int rowIdx = hasTitle ? 2 : 1;
                        for (int iRow = rowIdx; iRow <= iRowCount; iRow++)
                        {
                            DataRow dr = dt.NewRow();
                            for (int iCol = 1; iCol <= iColCount; iCol++)
                            {
                                range = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[iRow, iCol];
                                dr[iCol - 1] = (range.Value2 == null) ? "" : range.Text.ToString();
                            }
                            dt.Rows.Add(dr);
                        }

                    }

                }
                catch { return null; }
                finally
                {
                    workbook.Close(false, oMissiong, oMissiong);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                    workbook = null;
                    app.Workbooks.Close();
                    app.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
                    app = null;
                                    }
            }
            return dt;

        }
        #endregion

        #region button set color 
        private void buttonset(string ss)
        {
            button1.Text = ss;
            button1.Enabled = true;
            button1.BackColor = Color.GreenYellow;
        }
        private void buttonReset()
        {
            button1.Text = "打开记录表(可多选）";
            button1.Enabled = true;
            button1.BackColor = Color.White;
        }
        #endregion

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] namelist = new string[] { "顿号" ,"竖线", "分号_英", "空格", "回车" };
            label3.Text = namelist[comboBox1.SelectedIndex];

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //选择顿号
            comboBox1.SelectedIndex = 0;
            //直接缩小方便查看
            this.Width = 734;
            this.Height = 367;
        }

        //重新分隔，使用combox分隔号
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text!=string.Empty)
            {
                textBox1.Text = renderstr();
            }
    
        }

        public string renderstr()
        {
            string fulltext = "";
            foreach (string a in filename_unique_list)
            {
                if (textBox_splitChar.Text.Trim()==string.Empty)
                {
                    if (comboBox1.SelectedIndex == 4)
                    {
                        fulltext += a + Environment.NewLine;
                    }
                    else
                    {
                        fulltext += a + comboBox1.SelectedItem;

                    }
                }
                else
                {
                    fulltext += a + textBox_splitChar.Text.Trim();
                }

            }

            return fulltext.Substring(0,fulltext.Length-1);
        }
        public string renderlist(HashSet<string> list)
        {
            string fulltext = "";
            foreach (string a in list)
            {
                if (textBox_splitChar.Text.Trim() == string.Empty)
                {
                    if (comboBox1.SelectedIndex == 4)
                    {
                        fulltext += a + Environment.NewLine;
                    }
                    else
                    {
                        fulltext += a + comboBox1.SelectedItem;
                    }
                }
                else
                {
                    fulltext += a + textBox_splitChar.Text.Trim();
                }

            }
            if (fulltext == "")
            {
                return "";
            }
            else
                return fulltext.Substring(0, fulltext.Length - 1);
        }
        //解析string 到list
        public void derenderstr()
        {
            filename_unique_list = new List<string>();
            string fulltext = textBox1.Text;
            string split_str = textBox_splitChar.Text.Trim();
            string split_str2 = comboBox1.SelectedItem.ToString();
            if (split_str== string.Empty)
            {
                if(comboBox1.SelectedIndex != 4)
                filename_unique_list = new List<string>(fulltext.Split(new string[] { split_str2 }, StringSplitOptions.RemoveEmptyEntries));
                else 
                {
                    filename_unique_list = new List<string>(fulltext.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
                }
            }
            else
            {
                filename_unique_list = new List<string>(fulltext.Split(textBox_splitChar.Text.Trim()[0]));
            }
            toolStripStatusLabel1.Text = "解析到：" + filename_unique_list.Count().ToString() + "个";

            return;
        }

        //保存
        private void button3_Click(object sender, EventArgs e)
        {
            if (filename_unique_list.Count>0)
            {
                savetofile();

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox1.Text!=string.Empty)
            {
                derenderstr();

            }
        }

        //处理数据dataset to list
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                HashSet<string> filename_list = new HashSet<string>();
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    //匹配导出的记录行
                    string textdata = dataTable.Rows[i]["column11"].ToString();
                    string myregex = @"《[^》]*》";
                    foreach (Match filename in Regex.Matches(textdata, myregex))
                    {
                        filename_list.Add(filename.Value);
                    }
                }

                filename_unique_list = new List<string>(filename_list);
                //filename_unique_list = filename_list.Distinct().ToList();
                toolStripStatusLabel1.Text = "共找到：" + filename_unique_list.Count().ToString() + "个";
                textBox1.Text = renderstr();

                //dataGridView1.DataSource = dataTable;
            }
            catch (Exception )
            {
                toolStripStatusLabel1.Text = "DataSet 为空，请先选择开文件";
            }
        }

        //分离按钮
        private void button6_Click(object sender, EventArgs e)
        {
            if (filename_unique_list.Count==0)
            {
                toolStripStatusLabel1.Text = "?????";
                return;
            }
            this.Height = 623;
            this.Width = 1176;
            //string words = "制度，流程，规定，办法，管理";
            string wordszhidu =textBoxzhidus.Text;
            string[] zhidulikelist = wordszhidu.Split('，');
            string wordsjilu =textBoxjilus.Text;
            string[] jilulikelist = wordsjilu.Split('，');
            string wordspaichu = textpaichus.Text;
            string[] paichulikelist = wordspaichu.Split('，');

            HashSet<string> zhidu = new HashSet<string>();
            HashSet<string> jilu = new HashSet<string>();
            HashSet<string> paichu = new HashSet<string>();

            List<string> filename_SET = new List<string>();
            filename_unique_list.ForEach(i => filename_SET.Add(i));

            //首先过滤排除词汇
            foreach (var filename in filename_SET)
            {
                foreach (var word in paichulikelist)
                {
                    if (filename.Contains(word))
                    {
                        paichu.Add(filename);
                    }
                }
            }
            foreach(var f in paichu)
            {
                filename_SET.Remove(f);
            }
            //制度
            foreach (var filename in filename_SET)
            {
                foreach (var word in zhidulikelist)
                {
                    if (filename.Contains(word))
                    {
                        zhidu.Add(filename);
                    }
                }
            }
            foreach (var f in zhidu)
            {
                filename_SET.Remove(f);
            }
            // 记录

            foreach (var filename in filename_SET)
            {
                foreach (var word in jilulikelist)
                {
                    if (filename.Contains(word))
                    {
                        jilu.Add(filename);
                    }
                }
            }
            foreach (var f in jilu)
            {
                filename_SET.Remove(f);
            }
            // 剩余
            foreach (var filename in filename_SET)
            {
                jilu.Add(filename);
                //foreach (var word in paichulikelist)
                //{
                   
                //}
            }

            textBoxzhidu.Text = renderlist(zhidu);
            textBoxjilu.Text = renderlist(jilu);
            textBoxpaichu.Text = renderlist(paichu);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            if (textBox_splitChar.Text == "")
            {
                textBox_splitChar.Text = mysplitchar;
                label1.ForeColor = Color.RoyalBlue;
            }
            else
            {
                mysplitchar = textBox_splitChar.Text.Trim();
                textBox_splitChar.Text = "";
                label1.ForeColor = Color.DarkGray;
            }
        }

        //变成py列表
        private void button7_Click(object sender, EventArgs e)
        {
            string fulltext = "'";
            foreach (string a in filename_unique_list)
            {
                fulltext += a + "','";
            }
            fulltext = fulltext.Substring(0, fulltext.Length - 2);
            textBox1.Text = fulltext;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        //为了三个textbox可以平均分配他们的宽度
        private void Form1_Resize(object sender, EventArgs e)
        {
            int san = (this.Width) / 3;

            textBoxzhidu.Left = 0;
            textBoxzhidu.Width =san;
            textBoxzhidus.Left = 0+5;
            textBoxzhidus.Width = san-5;



            textBoxjilu.Left = san;
            textBoxjilu.Width = san;
            label5.Left = san;
            textBoxjilus.Left = san+5;
            textBoxjilus.Width = san-5;

            textBoxpaichu.Left = san *2;
            textBoxpaichu.Width = san-35;
            label6.Left = san*2;
            textpaichus.Left = san*2 +5;
            textpaichus.Width = san-40;


        }

        //保存关键词
        private void button8_Click(object sender, EventArgs e)
        {
            Settings.Default.jilu = textBoxjilus.Text;
            Settings.Default.zhidu = textBoxzhidus.Text;
            Settings.Default.paichu = textpaichus.Text;
            toolStripStatusLabel1.Text = "已保存。";
        }
        //读取关键词
        private void button9_Click(object sender, EventArgs e)
        {
            textBoxjilus.Text = Settings.Default.jilu;
            textBoxzhidus.Text = Settings.Default.zhidu;
            textpaichus.Text = Settings.Default.paichu;
            toolStripStatusLabel1.Text = "已读取。";

        }
    }
}
