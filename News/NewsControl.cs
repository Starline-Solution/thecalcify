using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
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
        public int pageSize = 100;
        public int lastPageSize = 0;
        public int pageRefreshDelay = 60000;
        public string PrevCursor = string.Empty;
        private bool buttonClicked = false;
        private string categoryLiteral = string.Empty;
        private string subcategoryLiteral = string.Empty;
        private string categoryCode = string.Empty;
        private string subcategoryCode = string.Empty;
        private string cursor = string.Empty;
        private int totalRecords = 0;
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

            // Smooth scrolling
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                dgvNews,
                new object[] { true }
            );

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
                btnNextPage.Enabled = true;

                // Example category/subcategory selection
                Category category = cmbCategory.SelectedItem as Category;
                Category subCategory = cmbSubCategory.SelectedItem as Category;

                buttonClicked = true;

                if (category != null)
                {
                    categoryLiteral = category.Literal;
                    categoryCode = category.Code;
                }
                else
                {
                    categoryLiteral = string.Empty;
                    categoryCode = string.Empty;
                }

                if (subCategory != null)
                {
                    subcategoryCode = subCategory.Code ?? string.Empty;
                    subcategoryLiteral = subCategory.Literal ?? string.Empty;
                }
                else
                {
                    subcategoryLiteral = string.Empty;
                    subcategoryCode = string.Empty;
                }


                // Fetch news data and update grid
                await FetchNewsDataAndUpdateGrid(categoryCode ?? string.Empty, subcategoryCode ?? string.Empty, pageSize, string.Empty);

                UpdatePageInfo(1, pageSize);
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[BtnSearchNews_Click] Error: {ex.Message}");
            }
        }

        private async Task PeriodicFetchAsync(CancellationToken cancellationToken)
        {
            //string cursor = string.Empty;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Fetch periodically and update cursor
                    await FetchNewsDataAndUpdateGrid(categoryCode, subcategoryCode, pageSize, string.Empty);

                    newsUpdateLable.Visible = true; 
                    newsUpdateLable.Text = $"Last News Updated At: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";

                    UpdatePageInfo(1, pageSize);
                    //lastPageSize = 1;


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

        private async Task FetchNewsDataAndUpdateGrid(string category, string subcategory, int pageSize, string cursorValue)
        {
            int rowCount = 0;
            if(dgvNews.Rows.Count > 0)
            {
                rowCount = dgvNews.Rows.Count;
            }
            try
            {
                // Build API URL
                string baseUrl = BuildReutersApiUrl($"{apiUrl}/Items", pageSize, category, subcategory, cursorValue);

                // Send request
                HttpResponseMessage response = await client.GetAsync(baseUrl);

                if (!response.IsSuccessStatusCode)
                {
                    ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid] Failed to fetch data. Status: {response.StatusCode}");
                }

                // Read and parse response
                string json = await response.Content.ReadAsStringAsync();

                ReutersResponse result = null;
                try
                {
                    // Handle double-encoded JSON
                    string innerJson = System.Text.Json.JsonSerializer.Deserialize<string>(json);
                    result = System.Text.Json.JsonSerializer.Deserialize<ReutersResponse>(innerJson);
                    cursor = result.Data.Search.PageInfo.EndCursor;
                    totalRecords = result.Data.Search.TotalHits;
                    if (result.Data.Search.PageInfo.HasNextPage == false)
                     btnNextPage.Enabled = false;   

                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid_Deserialize] Error: {ex.Message}");
                }

                if (result?.Data?.Search?.Items == null || result.Data.Search.Items.Count == 0)
                {
                    MessageBox.Show("No related data found.....","News Alert",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    cmbCategory.SelectedItem = "All";
                    // Call search button again with new category
                    BtnSearchNews_Click(this, EventArgs.Empty);
                    ApplicationLogger.Log("[FetchNewsDataAndUpdateGrid] No items found.");
                }

                // ✅ Sort by IST time (descending)
                var newsItems = result.Data.Search.Items
                    .OrderBy(item => DateTimeOffset.Parse(item.SortTimestamp).ToOffset(TimeSpan.FromHours(5.5)))
                    .ToList();


                if (this.InvokeRequired)
                {
                    // Update DataGridView on UI thread
                    _ = Invoke((Action)(() =>
                    {
                        try
                        {
                            if (this.IsDisposed || !this.IsHandleCreated || dgvNews.IsDisposed)
                            {
                                _cts?.Cancel(); // stop previous periodic fetch
                                                // Restart periodic fetch after reload
                                _cts = new CancellationTokenSource();
                                _ = Task.Run(() => PeriodicFetchAsync(_cts.Token));
                                ApplicationLogger.Log("[FetchNewsDataAndUpdateGrid] News Restarted.");
                                return;
                            }


                            dgvNews.Rows.Clear();
                            foreach (var item in newsItems)
                            {
                                // Convert date format (assuming FirstCreated is a valid datetime string)
                                DateTimeOffset dto = DateTimeOffset.Parse(item.SortTimestamp);
                                DateTimeOffset istTime = dto.ToOffset(TimeSpan.FromHours(5.5)); // Convert to IST time
                                string formattedTime = istTime.ToString("dd/MM/yyyy HH:mm:ss");

                                // Get the selected category and subcategory names
                                var selectedCategory = cmbCategory.SelectedItem as Category;
                                string categoryName = "N/A";
                                if (selectedCategory?.Literal != null && buttonClicked && categoryLiteral != null)
                                    categoryName = categoryLiteral;

                                var selectedSubCategory = cmbSubCategory.SelectedItem as Category;
                                string subcategoryName = "N/A";
                                if (selectedSubCategory?.Literal != null && buttonClicked && subcategoryLiteral != null)
                                    subcategoryName = subcategoryLiteral;

                                if (dgvNews.Columns.Count == 0)
                                {
                                    dgvNews.Columns.Add("Time", "Time");
                                    dgvNews.Columns.Add("Title", "Title");
                                    dgvNews.Columns.Add("Category", "Category");
                                    dgvNews.Columns.Add("SubCategory", "SubCategory");
                                }

                                dgvNews.Rows.Insert(0,
                                formattedTime,    // Time
                                item.HeadLine,    // Title
                                categoryName,     // Category
                                subcategoryName   // SubCategory
                            );

                                // Tag the row with the news item for future reference
                                dgvNews.Rows[0].Tag = item;
                            }

                            // Set specific column widths
                            dgvNews.Columns[0].Width = 145;  // Time column width
                            dgvNews.Columns[1].Width = 835;  // Title column width
                            dgvNews.Columns[2].Width = 250;  // Category column width
                            dgvNews.Columns[3].Width = 250;  // SubCategory column width
                        }
                        catch (Exception ex)
                        {
                            ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid_Invoke] Error: {ex.Message}");
                            ApplicationLogger.LogException(ex);
                        }
                    }));
                }
                else
                {
                    try
                    {

                        if (this.IsDisposed || !this.IsHandleCreated || dgvNews.IsDisposed)
                        {
                            _cts?.Cancel(); // stop previous periodic fetch
                            // Restart periodic fetch after reload
                            _cts = new CancellationTokenSource();
                            _ = Task.Run(() => PeriodicFetchAsync(_cts.Token));
                            ApplicationLogger.Log("[FetchNewsDataAndUpdateGrid] News Restarted.");
                            return;
                        }

                        dgvNews.Rows.Clear();
                        foreach (var item in newsItems)
                        {
                            // Convert date format (assuming FirstCreated is a valid datetime string)
                            DateTimeOffset dto = DateTimeOffset.Parse(item.SortTimestamp);
                            DateTimeOffset istTime = dto.ToOffset(TimeSpan.FromHours(5.5)); // Convert to IST time
                            string formattedTime = istTime.ToString("dd/MM/yyyy HH:mm:ss");

                            // Get the selected category and subcategory names
                            var selectedCategory = cmbCategory.SelectedItem as Category;
                            string categoryName = "N/A";
                            if (selectedCategory?.Literal != null && buttonClicked && categoryLiteral != null)
                                categoryName = categoryLiteral;

                            var selectedSubCategory = cmbSubCategory.SelectedItem as Category;
                            string subcategoryName = "N/A";
                            if (selectedSubCategory?.Literal != null && buttonClicked && subcategoryLiteral != null)
                                subcategoryName = subcategoryLiteral;


                            if (dgvNews.Columns.Count == 0)
                            {
                                dgvNews.Columns.Add("Time", "Time");
                                dgvNews.Columns.Add("Title", "Title");
                                dgvNews.Columns.Add("Category", "Category");
                                dgvNews.Columns.Add("SubCategory", "SubCategory");
                            }

                            dgvNews.Rows.Insert(0,
                            formattedTime,    // Time
                            item.HeadLine,    // Title
                            categoryName,     // Category
                            subcategoryName   // SubCategory
                        );

                            // Tag the row with the news item for future reference
                            dgvNews.Rows[0].Tag = item;
                        }

                        // Set specific column widths
                        dgvNews.Columns[0].Width = 145;  // Time column width
                        dgvNews.Columns[1].Width = 835;  // Title column width
                        dgvNews.Columns[2].Width = 250;  // Category column width
                        dgvNews.Columns[3].Width = 250;  // SubCategory column width
                    }
                    catch (Exception ex)
                    {
                        ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid_Invoke] Error: {ex.Message}");
                        ApplicationLogger.LogException(ex);
                    }
                }

            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid] Error: {ex.Message}");
            }
        }

        private async void DgvNews_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
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
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[dgvNews_CellDoubleClick] Error: {ex.Message}");
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
        private async void BtnNextPage_Click(object sender, System.EventArgs e)
        {
            //_cts?.Cancel(); // stop previous periodic fetch

            try
            {
                // Fetch next set of news using PrevCursor
                await FetchNewsDataAndUpdateGrid(categoryCode, subcategoryCode, pageSize, cursor);

                // Update page info label
                UpdatePageInfo(lastPageSize, lastPageSize + pageSize);
                lastPageSize = lastPageSize + pageSize;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[btnNextPage_Click] Error: {ex.Message}");
            }
        }

        private async void BtnPrevPage_Click(object sender, System.EventArgs e)
        {
            try
            {
                btnNextPage.Enabled = true;

                // Reset pagination variables
                PrevCursor = string.Empty;
                lastPageSize = 0;

                // Fetch first page of data again
                await FetchNewsDataAndUpdateGrid(categoryCode, subcategoryCode, pageSize, PrevCursor);

                // Reset page info starting from 0
                UpdatePageInfo(1, pageSize);
                lastPageSize = pageSize;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[btnPrevPage_Click] Error: {ex.Message}");
            }
        }


        // Method to update page information
        public void UpdatePageInfo(int currentPage, int totalPages)
        {
            try
            {
                // Check if we need to invoke the update to the UI thread
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        try
                        {
                            if (totalPages > totalRecords)
                                totalPages = totalRecords;

                            lblPageInfo.Text = $"Records {currentPage} To {totalPages} Out of {totalRecords}";
                        }
                        catch (Exception innerEx)
                        {
                            ApplicationLogger.Log($"[UpdatePageInfo] Error in Invoke: {innerEx.Message}");
                        }
                    }));
                }
                else
                {
                    try
                    {
                        if (totalPages > totalRecords)
                            totalPages = totalRecords;

                        lblPageInfo.Text = $"Records {currentPage} To {totalPages} Out of {totalRecords}";
                    }
                    catch (Exception innerEx)
                    {
                        ApplicationLogger.Log($"[UpdatePageInfo] Error: {innerEx.Message}");
                    }
                }
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
                    queryParams.Add($"cursorToken={Uri.EscapeDataString(cursorToken)}");

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
