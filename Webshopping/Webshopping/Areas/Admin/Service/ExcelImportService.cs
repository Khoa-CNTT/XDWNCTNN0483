using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Webshopping.Models;
using Webshopping.Repository;

namespace Webshopping.Areas.Admin.Service;

class ExcelImportService : IExcelImportService
{
    private readonly IWebHostEnvironment _env;
    private readonly DataContext _context;

    public ExcelImportService(IWebHostEnvironment env, DataContext context)
    {
        _env = env;
        _context = context;
    }

    public void ImportFromExcel(IFormFile fileEx)
    {
        var uploadDir = Path.Combine(_env.WebRootPath, "img");
        if (!Directory.Exists(uploadDir))
            Directory.CreateDirectory(uploadDir);

        var products = new List<ProductModel>();
        var categories = new List<CategoryModel>();
        var brands = new List<BrandModel>();

        using var memoryStream = new MemoryStream();
        fileEx.CopyTo(memoryStream);
        memoryStream.Position = 0;

        using var document = SpreadsheetDocument.Open(memoryStream, false);
        var workbookPart = document.WorkbookPart;
        var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;

        foreach (var sheet in workbookPart.Workbook.Sheets.OfType<Sheet>())
        {
            var sheetName = sheet.Name?.Value;
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
            var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

            var images = sheetName == "Product" ? ExtractImages(worksheetPart, uploadDir) : new Dictionary<int, string>();

            switch (sheetName)
            {
                case "Product":
                case "product":
                    products.AddRange(ReadProducts(sheetData, sharedStrings, images));
                    break;
                case "Category":
                case "category":
                    categories.AddRange(ReadCategories(sheetData, sharedStrings));
                    break;
                case "Brand":
                case "brand":
                    brands.AddRange(ReadBrands(sheetData, sharedStrings));
                    break;
            }
        }

        _context.Products.AddRange(products);
        _context.Categories.AddRange(categories);
        _context.Brands.AddRange(brands);
        _context.SaveChanges();
    }

    private List<ProductModel> ReadProducts(SheetData sheetData, SharedStringTable sharedStrings, Dictionary<int, string> images)
    {
        var products = new List<ProductModel>();
        foreach (var row in sheetData.Elements<Row>().Skip(1))
        {
            var cells = row.Elements<Cell>().ToList();
            var product = new ProductModel
            {
                Name = GetCellValue(cells, 0, sharedStrings),
                Slug = GetCellValue(cells, 1, sharedStrings),
                Description = GetCellValue(cells, 2, sharedStrings),
                Price = decimal.Parse(GetCellValue(cells, 3, sharedStrings)),
                CapitalPrice = decimal.Parse(GetCellValue(cells, 4, sharedStrings)),
                BrandID = int.Parse(GetCellValue(cells, 5, sharedStrings)),
                CategoryID = int.Parse(GetCellValue(cells, 6, sharedStrings)),
                Quantity = int.Parse(GetCellValue(cells, 7, sharedStrings)),
                Sold = int.Parse(GetCellValue(cells, 8, sharedStrings)),
                Img = images.TryGetValue((int)row.RowIndex.Value, out var img) ? img : "default.jpg"
            };
            products.Add(product);
        }
        return products;
    }

    private List<CategoryModel> ReadCategories(SheetData sheetData, SharedStringTable sharedStrings)
    {
        var categories = new List<CategoryModel>();
        foreach (var row in sheetData.Elements<Row>().Skip(1))
        {
            var cells = row.Elements<Cell>().ToList();
            var category = new CategoryModel
            {
                Name = GetCellValue(cells, 0, sharedStrings),
                Description = GetCellValue(cells, 1, sharedStrings),
                Slug = GetCellValue(cells, 2, sharedStrings),
                Status = int.Parse(GetCellValue(cells, 3, sharedStrings))
            };
            categories.Add(category);
        }
        return categories;
    }

    private List<BrandModel> ReadBrands(SheetData sheetData, SharedStringTable sharedStrings)
    {
        var brands = new List<BrandModel>();
        foreach (var row in sheetData.Elements<Row>().Skip(1))
        {
            var cells = row.Elements<Cell>().ToList();
            var brand = new BrandModel
            {
                Name = GetCellValue(cells, 0, sharedStrings),
                Description = GetCellValue(cells, 1, sharedStrings),
                Slug = GetCellValue(cells, 2, sharedStrings),
                Status = int.Parse(GetCellValue(cells, 3, sharedStrings))
            };
            brands.Add(brand);
        }
        return brands;
    }

    private Dictionary<int, string> ExtractImages(WorksheetPart worksheetPart, string uploadDir)
    {
        var images = new Dictionary<int, string>();
        var drawingsPart = worksheetPart.DrawingsPart;

        if (drawingsPart == null)
            return images;

        int imgIndex = 2;
        foreach (var imagePart in drawingsPart.ImageParts)
        {
            using var imgStream = imagePart.GetStream();
            string ext = GetImageExtension(imagePart.ContentType);
            var random = new Random();
            var randomNumber = random.Next(1, 9999);  // Sinh số ngẫu nhiên từ 1000 đến 9999
            var fileName = $"img{randomNumber}{ext}"; // Tên tệp mới là img{random số}{extension}
            string savePath = Path.Combine(uploadDir, fileName);
            using var fs = new FileStream(savePath, FileMode.Create);
            imgStream.CopyTo(fs);

            images[imgIndex++] = fileName;
        }

        return images;
    }

    private string GetImageExtension(string contentType)
    {
        return contentType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/gif" => ".gif",
            "image/bmp" => ".bmp",
            _ => ".img"
        };
    }

    private string GetCellValue(List<Cell> cells, int index, SharedStringTable sharedStrings)
    {
        if (index >= cells.Count) return "";

        var cell = cells[index];
        string value = cell.InnerText;

        if (cell.DataType?.Value == CellValues.SharedString && sharedStrings != null)
            return sharedStrings.ElementAt(int.Parse(value)).InnerText;

        return value;
    }
}