using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPITest.Model;

namespace WebAPITest
{
    public partial class frmMain : Form
    {
        private Session _session = new Session();

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUser.Text;
            string password = txtPassword.Text;

            var encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));

            string serializedData = $"grant_type=password&username={Uri.EscapeUriString(user)}&password={Uri.EscapeUriString(password)}";
            var content = new StringContent(serializedData, Encoding.UTF8, "application/x-www-form-urlencoded");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);

            using var result = client.PostAsync(txtLoginUrl.Text, content).Result;

            if (result.IsSuccessStatusCode)
            {
                string jsonString = result.Content.ReadAsStringAsync().Result;
                var oAuth2Response = JsonConvert.DeserializeObject<OAuth2Response>(jsonString);
                if (oAuth2Response == null)
                    throw new InvalidOperationException("Cannot deserialize OAuth2Response");
                txtToken.Text = oAuth2Response.access_token;
            }
            else
            {
                MessageBox.Show(result.ReasonPhrase, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void chkViewPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = chkViewPassword.Checked;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            txtName.Text = txtName.Text.Trim();

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please insert the name of the request", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (_session.Requests.Contains(txtName.Text))
            {
                MessageBox.Show("Request already present", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            Request request = new Request
            {
                Name = txtName.Text,
                Verb = cmbVerb.Text,
                Url = txtRequestUrl.Text,
                Payload = txtPayload.Text,
                Answer = txtResult.Text,
                SendAuth = chkSendAuth.Checked
            };

            _session.Requests.Add(request);
            lstRequests.Items.Add(request);
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (!CheckSelectedAndShowMessage())
                return;

            var request = (Request)lstRequests.SelectedItem;

            txtName.Text = request.Name;
            cmbVerb.Text = request.Verb;
            txtRequestUrl.Text = request.Url;
            txtPayload.Text = request.Payload;
            txtResult.Text = request.Answer;
            chkSendAuth.Checked = request.SendAuth;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (!CheckSelectedAndShowMessage())
                return;

            var request = (Request)lstRequests.SelectedItem;

            lstRequests.Items.Remove(request);
            _session.Requests.Remove(request);
        }


        private bool CheckSelectedAndShowMessage()
        {
            if (lstRequests.SelectedItem == null)
                MessageBox.Show("Please select a request", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            return lstRequests.SelectedItem != null;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            StringContent stringContent = null;
            if (!string.IsNullOrWhiteSpace(txtPayload.Text))
                stringContent = new StringContent(txtPayload.Text, Encoding.UTF8, "application/json");

            using var client = new HttpClient();

            if (chkSendAuth.Checked)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", txtToken.Text);


            try
            {

                var httpRequestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod(cmbVerb.Text),
                    RequestUri = new Uri(txtRequestUrl.Text),
                    Content = stringContent
                };

                using var result = client.SendAsync(httpRequestMessage).Result;
                if (result.IsSuccessStatusCode)
                {
                    string resultString = result.Content.ReadAsStringAsync().Result;
                    txtResult.Text = resultString;
                }
                else
                {
                    txtResult.Text = (int)result.StatusCode + " " + result.ReasonPhrase;
                }
            }
            catch (Exception exception)
            {
                txtResult.Text = exception.Message;
            }


            tabControl.SelectedIndex = 1;
        }

        private void saveSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Session file (*.webapi)|*.webapi";
            saveFileDialog.Title = "Save a WebAPI session file";
            saveFileDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog.FileName == "") 
                return;

            _session.LoginUrl = txtLoginUrl.Text;
            _session.User = txtUser.Text;
            _session.Password = txtPassword.Text;
            _session.Token = txtToken.Text;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Session));
            using var textWriter = new StreamWriter(saveFileDialog.FileName);
            xmlSerializer.Serialize(textWriter, _session);
        }

        private void openSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Session file (*.webapi)|*.webapi";
            openFileDialog.Title = "Open a WebAPI session file";
            openFileDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (openFileDialog.FileName == "")
                return;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Session));
            using var textReader = new StreamReader(openFileDialog.FileName);
            _session = (Session)xmlSerializer.Deserialize(textReader);

            txtLoginUrl.Text = _session.LoginUrl;
            txtUser.Text = _session.User;
            txtPassword.Text = _session.Password;
            txtToken.Text = _session.Token;

            lstRequests.Items.Clear();
            lstRequests.Items.AddRange(_session.Requests.ToArray());
        }
    }
}
