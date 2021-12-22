using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AsynchronousSocketHelper;
using Newtonsoft.Json;
using System.Threading;

namespace CovidViewerClient
{
    public partial class MainForm : Form
    {
        public AsynchronousSocketClient client = null;


        Mutex mutex = new Mutex();
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            client.SendMessage("", ClientHeader.RequestCountryHeader);
        }

        public void FetchHeader(string headers)
        {
            if (InvokeRequired)
            {
                Action fetchAction = delegate
                {
                    FetchHeader(headers);
                };

                Invoke(fetchAction);
                return;
            }

            string[] listHeader;
            try
            {
                listHeader = JsonConvert.DeserializeObject<string[]>(headers);
            }
            catch (System.Exception ex)
            {
                return;
            }
            
            comboBoxCountry.Items.Clear();
            comboBoxCountry.Items.AddRange(listHeader);
        }

        public void FetchData(string rawData)
        {
            if (InvokeRequired)
            {
                Invoke((Action)delegate { FetchData(rawData); });
                return;
            }

            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawData);

            dataGridView1.Rows.Clear();

            foreach (var field in data)
            {
                dataGridView1.Rows.Add(field.Key, field.Value);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string countryName = comboBoxCountry.SelectedItem.ToString();
            client.SendMessage(countryName, ClientHeader.RequestCountryData);
        }
    }
}
