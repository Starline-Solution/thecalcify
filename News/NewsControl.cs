using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
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
using thecalcify.MarketWatch;

namespace thecalcify.News
{
    public partial class NewsControl : UserControl
    {
        private readonly string _username, _password;
        private static readonly string apiUrl = ConfigurationManager.AppSettings["ReutersApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient();
        public string _token;
        private CancellationTokenSource _cts;
        public int pageSize = 50;
        public int lastPageSize = 0;
        public int pageRefreshDelay = 120000;
        public string PrevCursor = string.Empty;
        private bool checkItem = true;

        public NewsControl(string username, string password, string token)
        {
            InitializeComponent();
            _username = username;
            _password = password;
            _token = token;
            this.Load += NewsControl_Load;

            // Configure HttpClient once
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;
            btnSearchNews.Click += BtnSearchNews_Click;
            UpdatePageInfo(1, pageSize);
            lastPageSize = pageSize;
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
                    // Prepare the API request for categories
                    var request = new HttpRequestMessage(HttpMethod.Get, "http://api.thecalcify.com/Client/Reuters/Categories");
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                    // Send async request
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                    // Read response as string
                    string jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    // Deserialize into CategoriesResponse
                    var categoriesResponse = JsonConvert.DeserializeObject<CategoriesResponse>(jsonString);

                    // Access categories from response
                    var categories = categoriesResponse?.Data?.FilterOptions?.Categories;

                    if (categories != null && !this.IsDisposed && this.IsHandleCreated)
                    {
                        // Update ComboBox on UI thread
                        if (cmbCategory.InvokeRequired)
                        {
                            cmbCategory.Invoke(new Action(() =>
                            {
                                try
                                {
                                    cmbCategory.Items.Clear();
                                    cmbCategory.Items.Add("All");
                                    foreach (var cat in categories)
                                        cmbCategory.Items.Add(cat);

                                    cmbCategory.DisplayMember = "Literal";
                                    cmbCategory.ValueMember = "Code";
                                }
                                catch (Exception ex)
                                {
                                    ApplicationLogger.Log($"[LoadCategoriesAsync_Invoke] Error: {ex.Message}");
                                }
                            }));
                        }
                        else
                        {
                            try
                            {
                                cmbCategory.Items.Clear();
                                cmbCategory.Items.Add("All");
                                foreach (var cat in categories)
                                    cmbCategory.Items.Add(cat);

                                cmbCategory.DisplayMember = "Literal";
                                cmbCategory.ValueMember = "Code";
                            }
                            catch (Exception ex)
                            {
                                ApplicationLogger.Log($"[LoadCategoriesAsync_Direct] Error: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[LoadCategoriesAsync] Error: {ex.Message}");
                MessageBox.Show("Error loading categories: " + ex.Message);
            }
        }

        private async void NewsControl_Load(object sender, EventArgs e)
        {
            try
            {
                // Load categories on form load
                await LoadCategoriesAsync();

                // Start periodic fetch
                _ = PeriodicFetchAsync(_cts.Token);
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[NewsControl_Load] Error: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbCategory != null)
                {
                    ClearSubCategories();
                    var selectedCategory = cmbCategory.SelectedItem as Category;

                    if (cmbCategory.SelectedItem.ToString().Equals("All"))
                    {
                        cmbSubCategory.Items.Clear();
                        cmbSubCategory.Items.Add("All");
                        cmbSubCategory.Enabled = true;
                        cmbSubCategory.SelectedIndex = 0;
                    }
                    else if (selectedCategory != null && selectedCategory.SubCategories != null && selectedCategory.SubCategories.Count > 0)
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
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[CmbCategory_SelectedIndexChanged] Error: {ex.Message}");
            }
        }

        private void ClearSubCategories()
        {
            try
            {
                if (cmbCategory != null)
                {
                    cmbSubCategory.Items.Clear();
                    cmbSubCategory.Items.Add("------Null------");
                    cmbSubCategory.DisplayMember = null; // Reset because it's just a string now
                    cmbSubCategory.ValueMember = null;
                    cmbSubCategory.Enabled = false;
                    cmbSubCategory.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[ClearSubCategories] Error: {ex.Message}");
            }
        }

        private async void BtnSearchNews_Click(object sender, EventArgs e)
        {
            try
            {
                // Clear old rows
                dgvNews.Rows.Clear();

                // Example category/subcategory selection
                string category = cmbCategory.SelectedItem?.ToString() ?? "All";
                string subCategory = cmbSubCategory.SelectedItem?.ToString() ?? "All";

                dgvNews.Rows.Add($"Showing {category} - {subCategory} news here...");

                // Fetch news data and update grid
                await FetchNewsDataAndUpdateGrid(category, subCategory, pageSize, string.Empty);
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[BtnSearchNews_Click] Error: {ex.Message}");
            }
        }

        private async Task PeriodicFetchAsync(CancellationToken cancellationToken)
        {
            string cursor = string.Empty;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Fetch periodically and update cursor
                    cursor = await FetchNewsDataAndUpdateGrid(string.Empty, string.Empty, pageSize, cursor);

                    if (PrevCursor.Equals(cursor))
                    {
                        // Same cursor as before, no changes
                    }

                    PrevCursor = cursor;
                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log($"[PeriodicFetchAsync_Fetch] Error: {ex.Message}");
                }

                try
                {
                    await Task.Delay(pageRefreshDelay, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // Task was canceled, exit loop
                    break;
                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log($"[PeriodicFetchAsync_Delay] Error: {ex.Message}");
                }
            }
        }

        private async Task<string> FetchNewsDataAndUpdateGrid(string category, string subcategory, int pageSize, string cursor)
        {
            var totalStopwatch = Stopwatch.StartNew();

            try
            {
                // Build API URL
                string baseUrl = BuildReutersApiUrl($"{apiUrl}/Items", pageSize, category, subcategory, cursor);

                // Send request
                var requestStopwatch = Stopwatch.StartNew();
                HttpResponseMessage response = await client.GetAsync(baseUrl);
                requestStopwatch.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid] Failed to fetch data. Status: {response.StatusCode}");
                    return cursor;
                }

                // Read and parse response
                string json = await response.Content.ReadAsStringAsync();

                ReutersResponse result = null;
                try
                {
                    // Handle double-encoded JSON
                    string innerJson = System.Text.Json.JsonSerializer.Deserialize<string>(json);
                    result = System.Text.Json.JsonSerializer.Deserialize<ReutersResponse>(innerJson);
                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid_Deserialize] Error: {ex.Message}");
                    return cursor;
                }

                if (result?.Data?.Search?.Items == null || result.Data.Search.Items.Count == 0)
                {
                    ApplicationLogger.Log("[FetchNewsDataAndUpdateGrid] No items found.");
                    return cursor;
                }

                var newsItems = result.Data.Search.Items;

                // Update DataGridView on UI thread
                _ = Invoke((Action)(() =>
                {
                    try
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
                                categoryName,      // Category
                                subcategoryName    // SubCategory
                            );

                            dgvNews.Rows[0].Tag = item;
                        }
                    }
                    catch (Exception ex)
                    {
                        ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid_Invoke] Error: {ex.Message}");
                    }
                }));

                totalStopwatch.Stop();

                // Return updated cursor
                return result.Data.Search.PageInfo?.EndCursor;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid] Error: {ex.Message}");
                return cursor;
            }
        }


        private async void dgvNews_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                dgvNews.Enabled = false;

                // Ignore header row clicks
                if (e.RowIndex < 0) return;

                // Get the clicked row and its NewsItem
                var row = dgvNews.Rows[e.RowIndex];
                var selected = row.Tag as NewsItem;
                if (selected == null) return;

                // Fetch full news details by VersionGuid
                ItemDto fullNews = await GetNewsByVersionGuidAsync(selected.VersionedGuid);

                // Show the details in a new form if available
                if (fullNews != null)
                {
                    using (var frm = new NewsDescription(fullNews))
                    {
                        frm.ShowDialog();
                        dgvNews.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[dgvNews_CellDoubleClick] Error: {ex.Message}");
            }
            finally
            {
                dgvNews.Enabled = true;
            }
        }

        private async Task<ItemDto> GetNewsByVersionGuidAsync(string versionGuid)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Prepare request to fetch full news details
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        $"http://api.thecalcify.com/Client/Reuters/ItemDescription?id={versionGuid}"
                    );
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                    // Send request
                    HttpResponseMessage response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    // Parse JSON (double-encoded)
                    string jsonString = await response.Content.ReadAsStringAsync();
                    string innerJson = JsonConvert.DeserializeObject<string>(jsonString);

                    var root = JsonConvert.DeserializeObject<RootDto>(innerJson);
                    ItemDto item = root?.data?.item;

                    return item;
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[GetNewsByVersionGuidAsync] Error: {ex.Message}");
                return null;
            }
        }

        // Event handlers for pagination
        private async void btnNextPage_Click(object sender, System.EventArgs e)
        {
            _cts?.Cancel(); // stop previous periodic fetch

            string newCusrsor = string.Empty;
            try
            {
                // Fetch next set of news using PrevCursor
                newCusrsor = await FetchNewsDataAndUpdateGrid(string.Empty, string.Empty, pageSize, newCusrsor);

                // Update page info label
                UpdatePageInfo(lastPageSize, lastPageSize + pageSize);
                lastPageSize = lastPageSize + pageSize;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[btnNextPage_Click] Error: {ex.Message}");
            }
            finally
            {
                // Restart periodic fetch
                _cts = new CancellationTokenSource();
                _ = Task.Run(() => PeriodicFetchAsync(_cts.Token));
            }
        }

        private async void btnPrevPage_Click(object sender, System.EventArgs e)
        {
            try
            {
                _cts?.Cancel(); // stop periodic fetch while reloading

                // Reset pagination variables
                PrevCursor = string.Empty;
                lastPageSize = 0;

                // Fetch first page of data again
                PrevCursor = await FetchNewsDataAndUpdateGrid(string.Empty, string.Empty, pageSize, PrevCursor);

                // Reset page info starting from 0
                UpdatePageInfo(0, pageSize);
                lastPageSize = pageSize;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[btnPrevPage_Click] Error: {ex.Message}");
            }
            finally
            {
                // Restart periodic fetch after reload
                _cts = new CancellationTokenSource();
                _ = Task.Run(() => PeriodicFetchAsync(_cts.Token));
            }
        }


        // Method to update page information
        public void UpdatePageInfo(int currentPage, int totalPages)
        {
            try
            {
                lblPageInfo.Text = $"Records {currentPage} of {totalPages}";

                // Optionally enable/disable pagination buttons
                // btnPrevPage.Enabled = currentPage > 1;
                // btnNextPage.Enabled = currentPage < totalPages;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[UpdatePageInfo] Error: {ex.Message}");
            }
        }

        // Utility method to build Reuters API URL with query parameters
        public static string BuildReutersApiUrl(string baseUrl, int pageSize, string category = null, string subCategory = null, string cursorToken = null)
        {
            try
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
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[BuildReutersApiUrl] Error: {ex.Message}");
                return baseUrl;
            }
        }

    }
}
