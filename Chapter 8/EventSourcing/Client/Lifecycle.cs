using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    using System.Configuration;

    using RestSharp;
    using Newtonsoft.Json;
    using NEventStore;
    using Contracts;

    public partial class Lifecycle : Form
    {
        RestClient client = new RestClient(ConfigurationManager.AppSettings["commerceService"]);
        public Lifecycle()
        {
            InitializeComponent();
        }

        private void btn_AddToInventory_Click(object sender, EventArgs e)
        {
            var request = new RestRequest($"/Inventory?productId={txtProd.Text}&productName={txtProdName.Text}&supplierName={txtSup.Text}&warehouseCode={txtCode.Text}", Method.POST);
            IRestResponse response = client.Execute(request);
            lblProductID.Visible = true;
            lblProductName.Visible = true;
            lblStatus.Visible = true;

            var content = response.Content;

            lblProductID.Text = txtProd.Text;
            lblProductName.Text = txtProdName.Text;
            lblStatus.Text = "Added";
        }

        private void btn_ShipToCustomer_Click(object sender, EventArgs e)
        {
            var request = new RestRequest($"Shipping?productName={txtProdName.Text}&warehouseName={txtCode.Text}&customerName={txtCusName.Text}", Method.POST);
            IRestResponse response = client.Execute(request);
            var content = response.Content;
            lblStatus.Text = "Shipped";
        }

        private void btn_DeliveredToCustomer_Click(object sender, EventArgs e)
        {
            var request = new RestRequest($"Customer?productId={txtProd.Text}&customerName={txtCusName.Text}", Method.POST);
            IRestResponse response = client.Execute(request);
            var content = response.Content;
            lblStatus.Text = "Delivered";
        }

        private void btn_ReshipToCustomer_Click(object sender, EventArgs e)
        {
            var request = new RestRequest($"Customer?productId={txtProd.Text}&customerName={txtCusName.Text}", Method.PUT);
            IRestResponse response = client.Execute(request);
            var content = response.Content;
            lblStatus.Text = "Reshipped";
        }

        private void btn_AuditLifecycle_Click(object sender, EventArgs e)
        {
            txtAuditLog.Text = "";
            var request = new RestRequest($"Audit?correlationCode={txtProd.Text}", Method.GET);
            IRestResponse response = client.Execute(request);
            var content = response.Content;
            var events = JsonConvert.DeserializeObject<List<string>>(content);
            foreach(var eve in events)
            {
                txtAuditLog.Text+= eve;
                txtAuditLog.Text += Environment.NewLine;
                txtAuditLog.Text += Environment.NewLine;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void Lifecycle_Load(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
