using DocumentFormat.OpenXml.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using thecalcify.Helper;

namespace thecalcify.News
{
    public partial class NewsControl : UserControl
    {
        private readonly string _username, _password;
        private static readonly string apiUrl = ConfigurationManager.AppSettings["ReutersApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient();
        public string _token;
        private CancellationTokenSource _cts;


        public NewsControl(string username, string password, string token)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _token = token;
            LoadCategoriesAsync().GetAwaiter().GetResult();


            // Configure HttpClient once
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;
            btnSearchNews.Click += BtnSearchNews_Click;

            _cts = new CancellationTokenSource();

            try
            {
                _ = PeriodicFetchAsync(_cts.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                //_cts.Cancel();
                //_cts.Dispose();
            }
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

                    if (categories != null && !this.IsDisposed && this.IsHandleCreated)
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

        private async void BtnSearchNews_Click(object sender, EventArgs e)
        {
            // Clear old rows
            dgvNews.Rows.Clear();

            // Example data filling - replace this with DB/API later
            string category = cmbCategory.SelectedItem?.ToString() ?? "All";
            string subCategory = cmbSubCategory.SelectedItem?.ToString() ?? "All";

            dgvNews.Rows.Add($"Showing {category} - {subCategory} news here...");


            // Fetch news data and get the new cursor
            await FetchNewsDataAndUpdateGrid(category, subCategory, 20, string.Empty);

        }

        private async Task PeriodicFetchAsync(CancellationToken cancellationToken)
        {
            string cursor = string.Empty;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Fetch news data and get the new cursor
                    cursor = await FetchNewsDataAndUpdateGrid(string.Empty, string.Empty, 20, cursor);
                }
                catch (Exception)
                {
                    //Invoke((Action)(() => lblStatus.Text = $"Error: {ex.Message}"));
                }

                try
                {
                    await Task.Delay(3000, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private async Task<string> FetchNewsDataAndUpdateGrid(string category, string subcategory, int pageSize, string cursor)
        {
            var totalStopwatch = Stopwatch.StartNew();

            string baseUrl = BuildReutersApiUrl($"{apiUrl}/Items", pageSize, category, subcategory, cursor);

            var requestStopwatch = Stopwatch.StartNew();
            HttpResponseMessage response = await client.GetAsync(baseUrl);
            requestStopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                //Invoke((Action)(() => lblStatus.Text = $"Failed to fetch data. Status: {response.StatusCode}"));
                return cursor;
            }

            string json = await response.Content.ReadAsStringAsync();

            ReutersResponse result = null;
            try
            {
                // Handle double-encoded JSON
                string innerJson = System.Text.Json.JsonSerializer.Deserialize<string>(json);
                result = System.Text.Json.JsonSerializer.Deserialize<ReutersResponse>(innerJson);
            }
            catch (Exception)
            {
                //Invoke((Action)(() => lblStatus.Text = $"Deserialization error: {ex.Message}"));
                return cursor;
            }

            if (result?.Data?.Search?.Items == null || result.Data.Search.Items.Count == 0)
            {
                //Invoke((Action)(() => lblStatus.Text = "No items found."));
                return cursor;
            }

            var newsItems = result.Data.Search.Items;

            // Update DataGridView on UI thread
            _ = Invoke((Action)(() =>
            {
                dgvNews.Rows.Clear();
                foreach (var item in newsItems)
                {
                    DateTimeOffset dto = DateTimeOffset.Parse(item.FirstCreated);
                    DateTimeOffset istTime = dto.ToOffset(TimeSpan.FromHours(5.5));
                    string formattedTime = istTime.ToString("dd/MM/yyyy HH:mm:ss");

                    var selectedCategory = cmbCategory.SelectedItem as Category;
                    string categoryName = selectedCategory?.Literal ?? "N/A";

                    var selectedSubCategory = cmbSubCategory.SelectedItem as Category;
                    string subcategoryName = selectedSubCategory?.Literal ?? "N/A";


                    dgvNews.Rows.Insert(0,
                        formattedTime,     // Time
                        item.HeadLine,     // Title
                        categoryName,          // Category
                        subcategoryName        // SubCategory
                    );
                }

            }));



            totalStopwatch.Stop();

            // Return the updated cursor for pagination
            return result.Data.Search.PageInfo?.EndCursor;
        }

        public static string BuildReutersApiUrl(string baseUrl, int pageSize, string category = null, string subCategory = null, string cursorToken = null)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(category))
                queryParams.Add($"category={Uri.EscapeDataString(category)}");

            if (!string.IsNullOrWhiteSpace(subCategory))
                queryParams.Add($"subCategory={Uri.EscapeDataString(subCategory)}");

            queryParams.Add($"pageSize={(pageSize > 0 ? pageSize : 20)}");

            if (!string.IsNullOrWhiteSpace(cursorToken))
                queryParams.Add($"cursor={Uri.EscapeDataString(cursorToken)}");

            return $"{baseUrl}?{string.Join("&", queryParams)}";
        }
    }
}
