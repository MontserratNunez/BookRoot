using Application.Common.Result;

namespace Application.Interfaces
{
    public interface IExportService
    {
        Task<Result<string>> GenerateHtmlExport(string username, string filter, string theme = "floral", DateTime? from = null, DateTime? to = null);
    }
}
