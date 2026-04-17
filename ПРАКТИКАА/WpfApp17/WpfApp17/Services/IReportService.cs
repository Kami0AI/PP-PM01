using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp17.Models;

namespace WpfApp17.Services
{
    public interface IReportService
    {
        Task<List<Student>> ImportFromCsvAsync(string path);
        Task ExportAsync(List<Student> students, string path, int formatIndex); // 1=XLSX, 2=PDF, 3=CSV
    }
}