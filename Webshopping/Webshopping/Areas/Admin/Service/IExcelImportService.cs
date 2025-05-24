namespace Webshopping.Areas.Admin.Service;

public interface IExcelImportService
{
    void ImportFromExcel(IFormFile fileEx);
}