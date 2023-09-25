using ClosedXML.Excel;
using HemnetScraperService.HemnetScraperModel;
using HemnetScraperService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HemnetScraperService.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDbContextFactory<HemnetDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;

        public IndexModel(ILogger<IndexModel> logger, IDbContextFactory<HemnetDbContext> dbContextFactory, IConfiguration configuration)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
        }

        public ExcelFileModel? ExcelFile { get; set; }
        public void OnGet()
        {
            ExcelFile = GenerateExcelReport();
        }

        public FileResult OnGetDownloadExcelReport(string excelFilePath)
        {
            byte[] data = System.IO.File.ReadAllBytes(excelFilePath);
            return File(data, "application/octet-stream", "HemnetReport.xlsx");
        }

        public ExcelFileModel? GenerateExcelReport()
        {
            XLWorkbook? workbook = null;
            try
            {
                using var hemnetDbContext = _dbContextFactory.CreateDbContext();
                var lastScrapingDate = hemnetDbContext.ScraperTask.Max(p => p.MondayDateTime);

                if (!lastScrapingDate.HasValue)
                    return null;

                string formattedDate = lastScrapingDate.Value.ToString("MM-dd-yyyy");
                string outputFilePath = Environment.ExpandEnvironmentVariables($"%ProgramData%\\Hemnet\\ExcelReports\\{formattedDate}_{Guid.NewGuid()}.xlsx");
                var excelFileModel = new ExcelFileModel()
                {
                    StartDateFormatted = lastScrapingDate.Value.ToString("MM/dd/yyyy"),
                    EndDateFormatted = lastScrapingDate.Value.AddDays(7).AddMinutes(-1).ToString("MM/dd/yyyy"),
                    FilePath = outputFilePath,
                };

                var packages = _configuration.GetSection("AppSettings:Packages").Get<string[]>();
                var prices = _configuration.GetSection("AppSettings:Prices").Get<string[]>();
                var locations = _configuration.GetSection("AppSettings:Cities").Get<List<HemnetLocation>>()!.Select(p => p.LocationName).ToList();

                List<ExcelHeader> hemnetCalculatorHeaders = (from city in locations!
                                                             from price in prices!
                                                             from package in packages!
                                                             orderby package, int.Parse(price)
                                                             select new ExcelHeader { Row1Column = package, Row2Column = price, Row3Column = city }).ToList();

                List<Dictionary<ExcelHeader, object?>> lines = new();

                foreach (var scrapingTask in hemnetDbContext.ScraperTask.OrderBy(p => p.MondayDateTime))
                {
                    var yearToDate = hemnetDbContext.ScraperTask.Where(p => p.MondayDateTime!.Value.Year == scrapingTask.MondayDateTime!.Value.Year && p.MondayDateTime <= scrapingTask.MondayDateTime).ToList();
                    var quarterToDate = yearToDate.GroupBy(p => (p.MondayDateTime!.Value.Month - 1) / 3 + 1);

                    var currentQuarter = quarterToDate.OrderByDescending(p => p.Key).First();

                    Dictionary<ExcelHeader, object?> currentLine = new()
                    {
                        //[new ExcelHeader() { Row3Column = "Date" }] = scrapingTask.MondayDateTime!.Value.ToString("MM/dd/yyyy"),
                        [new ExcelHeader() { Row3Column = "Date" }] = scrapingTask.MondayDateTime!.Value,
                        [new ExcelHeader() { Row3Column = nameof(HemnetListingsData.NewListingsCount) }] = scrapingTask.ListingData?.NewListingsCount!,
                        [new ExcelHeader() { Row3Column = nameof(HemnetListingsData.TotalListingsCount) }] = scrapingTask.ListingData?.TotalListingsCount!,
                        [new ExcelHeader() { Row3Column = "New Listings YTD" }] = yearToDate.Sum(p => p.ListingData?.NewListingsCount),
                        [new ExcelHeader() { Row3Column = "New Listings QTD" }] = currentQuarter.Sum(p => p.ListingData?.NewListingsCount),
                        [new ExcelHeader() { Row3Column = nameof(HemnetListingsData.AverageAskingPrice) }] = ((int?)scrapingTask?.ListingData?.AverageAskingPrice)!,

                        [new ExcelHeader() { Row3Column = "PLUS Listings Count" }] = scrapingTask?.ListingData?.PackageCounter?.FirstOrDefault(p => p.Key == "PLUS").Value!,
                        [new ExcelHeader() { Row3Column = "PREMIUM Listings Count" }] = scrapingTask?.ListingData?.PackageCounter?.FirstOrDefault(p => p.Key == "PREMIUM").Value!,
                    };

                    Dictionary<ExcelHeader, int?> calculatorData = hemnetCalculatorHeaders.ToDictionary(k => k, v => default(int?));
                    foreach (var location in scrapingTask?.PriceCalculatorLocations!)
                    {
                        foreach (var package in location.PackagePrices!)
                        {
                            var columnKey = new ExcelHeader() { Row1Column = package.Key, Row2Column = location!.HomeStartingPrice.GetValueOrDefault().ToString(), Row3Column = location.LocationName?.ToString()! };
                            calculatorData[columnKey] = package.Value;
                        }
                    }
                    Dictionary<ExcelHeader, int?> calculatorAverage = calculatorData.GroupBy(p => new { p.Key.Row1Column, p.Key.Row2Column })
                                                                                    .ToDictionary(k => new ExcelHeader { Row2Column = "Averages", Row3Column = $"{k.Key.Row1Column} {k.Key.Row2Column}" },
                                                                                                  v => (int?)v.Average(p => p.Value));

                    currentLine.Add(new ExcelHeader() { Row2Column = "Averages", Row3Column = "Average Pricing" }, Math.Round(calculatorAverage.Average(p => p.Value)!.Value, 0));

                    foreach (var priceCalc in calculatorAverage.Union(calculatorData!))
                    {
                        currentLine.Add(priceCalc.Key, priceCalc.Value);
                    }

                    lines.Add(currentLine);
                }


                IXLWorksheet worksheet;
                string templateFile = "template.xlsx";
                if (System.IO.File.Exists(templateFile))
                {
                    workbook = new(templateFile);
                    worksheet = workbook.Worksheet(1);
                }
                else
                {
                    workbook = new();
                    worksheet = workbook.Worksheets.Add("Sheet1");
                    var allHeaders = lines[0].Keys.ToList();
                    for (int i = 0; i < allHeaders.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = allHeaders[i].Row1Column;
                        worksheet.Cell(2, i + 1).Value = allHeaders[i].Row2Column;
                        worksheet.Cell(3, i + 1).Value = allHeaders[i].Row3Column;
                    }

                    MergeColumns(1, worksheet);
                    MergeColumns(2, worksheet);
                    MergeColumns(3, worksheet);

                    worksheet.Row(3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(3).Style.Border.BottomBorderColor = XLColor.Black;
                }

                worksheet.Column(1).LastCellUsed().TryGetValue(out DateTime lastUsedDate);
                var filteredData = lines.Where(p =>
                {
                    var currentDate = (DateTime)p[new ExcelHeader() { Row3Column = "Date" }]!;
                    return currentDate > lastUsedDate;
                }).ToList();

                var lastRow = worksheet.LastRowUsed().RowBelow();
                lastRow.Cell(1).InsertData(filteredData.Select(p => p.Values));

                worksheet.CellsUsed().Style.NumberFormat.Format = "#,##0";
                worksheet.Column(1).Style.DateFormat.Format = "mm\\/dd\\/yyyy";

                //FIX THE DATES
                //foreach (var cell in worksheet.Column("A").CellsUsed())
                //{
                //    if (DateTime.TryParse(cell.GetString(), out DateTime date))
                //    {
                //        cell.Value = date;
                //    }
                //}

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(outputFilePath);
                return excelFileModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel Report Exception");
                throw;
            }
            finally
            {
                workbook?.Dispose();
            }
        }

        public void MergeColumns(int rowNumber, IXLWorksheet worksheet)
        {
            IXLRow row = worksheet.Row(rowNumber);
            int currentColumn = 1;
            int columnCount = worksheet.RangeUsed().ColumnCount();
            while (currentColumn < columnCount)
            {
                string cellValue = row.Cell(currentColumn).Value.ToString();
                int startMergeColumnIndex = currentColumn;
                int endMergeColumnIndex = 0;

                while (cellValue == row.Cell(++currentColumn).Value.ToString())
                    endMergeColumnIndex = currentColumn;

                if (endMergeColumnIndex > startMergeColumnIndex && !string.IsNullOrWhiteSpace(cellValue))
                {
                    IXLRange mergedRange = worksheet.Range(row.Cell(startMergeColumnIndex), row.Cell(endMergeColumnIndex));
                    mergedRange.Merge();
                    mergedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    if (rowNumber == 1)
                    {
                        mergedRange.FirstCell().WorksheetColumn().Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        mergedRange.FirstCell().WorksheetColumn().Style.Border.BottomBorderColor = XLColor.Black;
                    }
                }
            }
        }
    }
}