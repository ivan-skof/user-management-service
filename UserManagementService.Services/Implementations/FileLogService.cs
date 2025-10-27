using System.Text;
using UserManagementService.Services.Interfaces;


namespace UserManagementService.Services.Implementations
{
    public class FileLogService : ILogService
    {
        private readonly string _logDirectory;
        private readonly string _hostName;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public FileLogService()
        {
            _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            _hostName = Environment.MachineName;

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public async Task LogAsync(string level, string methodName, string requestParams, string message, string clientIp, string clientName)
        {
            var logFileName = $"log_{DateTime.Now:yyyyMMdd}.txt";
            var logFilePath = Path.Combine(_logDirectory, logFileName);

            var logEntry = new StringBuilder();
            logEntry.AppendLine("=".PadRight(80, '='));
            logEntry.AppendLine($"Level: {level}");
            logEntry.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            logEntry.AppendLine($"Client IP: {clientIp}");
            logEntry.AppendLine($"Client Name: {clientName}");
            logEntry.AppendLine($"Host: {_hostName}");
            logEntry.AppendLine($"API Method: {methodName}");
            logEntry.AppendLine($"Request Parameters: {requestParams}");
            logEntry.AppendLine($"Message: {message}");
            logEntry.AppendLine("=".PadRight(80, '='));
            logEntry.AppendLine();

            await _semaphore.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(logFilePath, logEntry.ToString());
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}