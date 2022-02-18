using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;


namespace WindowsFormsApp1
{
 
    public partial class Form1 : Form
    {
        public class ComboBoxItem
        {
            string _Contents;
            public string Contents
            {
                get { return _Contents; }
                set { _Contents = value; }
            }
            object _Tag;
            public object Tag
            {
                get { return _Tag; }
                set { _Tag = value; }
            }
            public ComboBoxItem(string contents, object tag)
            {
                this._Contents = contents;
                this._Tag = tag;
            }

            public override string ToString() { return _Contents; }
        }



        public Form1()
        {
            InitializeComponent();
            {
                XDocument xdoc = XDocument.Load("http://www.cbr.ru/scripts/XML_daily.asp?date_req=05/05/2020");
                foreach (XElement phoneElement in xdoc.Element("ValCurs").Elements("Valute"))
                {
                    XAttribute id = phoneElement.Attribute("ID");
                    XElement name = phoneElement.Element("Name");
                    XElement priceElement = phoneElement.Element("Value");

                    var ComboBoxItem = new ComboBoxItem(name.Value.ToString(), id.Value.ToString());

                    comboBox1.Items.Add(ComboBoxItem);
                }
                comboBox1.SelectedIndex = 0;
            }
        }




        private void button1_Click_1(object sender, EventArgs e)
        {
            DateTime? dtStart = dateTimePicker1.Value;
            DateTime? dtEnd = dateTimePicker2.Value;

            if (dtStart == null || dtEnd == null)
            {
                MessageBox.Show("Установите обе даты");
                return;
            }
            if (dtStart >= dtEnd)
            {
                var a = dtStart;
                dtStart = dtEnd;
                dtEnd = a;
            }

            string stStart = dtStart.Value.ToString("dd/MM/yyyy");
            string stEnd = dtEnd.Value.ToString("dd/MM/yyyy");
            string valutaCode = ((ComboBoxItem)comboBox1.SelectedItem).Tag.ToString();
            string valutaName = ((ComboBoxItem)comboBox1.SelectedItem).Contents.ToString();
            string query = string.Concat("http://www.cbr.ru/scripts/XML_dynamic.asp?date_req1=",
                stStart, "&date_req2=", stEnd, "&VAL_NM_RQ=", valutaCode);
            XDocument xdoc = XDocument.Load(query);

            chart1.Series.Clear();

            chart1.Name = valutaName;
            chart1.Series.Add("curr");
            chart1.Series["curr"].ChartType = SeriesChartType.Line;
            chart1.Series["curr"].XValueType = ChartValueType.DateTime;
            chart1.Series["curr"].LegendText = valutaName;

            foreach (XElement phoneElement in xdoc.Element("ValCurs").Elements("Record"))
            {
                XAttribute date = phoneElement.Attribute("Date");
                XElement value = phoneElement.Element("Value");

                double val = double.Parse(value.Value.ToString());
                string strDate = date.Value.ToString();

                DateTime dt = DateTime.ParseExact(strDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                chart1.Series["curr"].Points.AddXY(dt, val);

            }
            chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;
            chart1.ChartAreas[0].RecalculateAxesScale();
        }

            private void Form1_Resize(object sender, EventArgs e)
        {
            chart1.Size = new Size(this.Width - 300, this.Height - 100);
        }
    }
}
