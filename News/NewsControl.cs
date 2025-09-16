using DocumentFormat.OpenXml.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify.News
{
    public partial class NewsControl : UserControl
    {
        private readonly string _username, _password;

        public string _token;
        public NewsControl(string username, string password, string token)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _token = token;
            LoadCategoriesAsync();
            cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;
            btnSearchNews.Click += BtnSearchNews_Click;
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, "http://api.thecalcify.com/Client/Reuters/Categories");
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                    // Use async call instead of .Result
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                    string jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var categoriesResponse = JsonConvert.DeserializeObject<CategoriesResponse>(jsonString);

                    // Access categories
                    var categories = categoriesResponse?.Data?.FilterOptions?.Categories;

                    if (categories != null)
                    {
                        // **Invoke to UI thread**
                        if (cmbCategory.InvokeRequired)
                        {
                            cmbCategory.Invoke(new Action(() =>
                            {
                                cmbCategory.Items.Clear();
                                foreach (var cat in categories)
                                    cmbCategory.Items.Add(cat);

                                cmbCategory.DisplayMember = "Literal";
                                cmbCategory.ValueMember = "Code";
                            }));
                        }
                        else
                        {
                            cmbCategory.Items.Clear();
                            foreach (var cat in categories)
                                cmbCategory.Items.Add(cat);

                            cmbCategory.DisplayMember = "Literal";
                            cmbCategory.ValueMember = "Code";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories: " + ex.Message);
            }
        }


        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCategory != null)
            {
                ClearSubCategories();
                var selectedCategory = cmbCategory.SelectedItem as Category;
                if (selectedCategory != null && selectedCategory.SubCategories != null && selectedCategory.SubCategories.Count > 0)
                {
                    cmbSubCategory.Items.Clear();
                    foreach (var subCat in selectedCategory.SubCategories)
                    {
                        cmbSubCategory.Items.Add(subCat);
                    }

                    // Show the Literal property in the combobox
                    cmbSubCategory.DisplayMember = "Literal";
                    cmbSubCategory.ValueMember = "Code";
                    cmbSubCategory.Enabled = true;
                }
                else
                {
                    ClearSubCategories();
                }
            }
        }

        private void ClearSubCategories()
        {
            if (cmbCategory != null)
            {
                cmbSubCategory.Items.Clear();
                cmbSubCategory.Items.Add("------Null------");
                cmbSubCategory.DisplayMember = null; // needed because it's just a string now
                cmbSubCategory.ValueMember = null;
                cmbSubCategory.Enabled = false;
                cmbSubCategory.SelectedIndex = 0;
            }
        }
        private void BtnSearchNews_Click(object sender, EventArgs e)
        {
            // Clear old rows
            dgvNews.Rows.Clear();

            // Example data filling - replace this with DB/API later
            string category = cmbCategory.SelectedItem?.ToString() ?? "All";
            string subCategory = cmbSubCategory.SelectedItem?.ToString() ?? "All";

            dgvNews.Rows.Add($"Showing {category} - {subCategory} news here...");
        }
    }
}
