// 🎓 TUTORIAL FILE - Database connection with FAKE credentials

using System;

namespace TutorialApp
{
    public class DatabaseService
    {
        // ⚠️ FAKE CONNECTION STRING (will be detected and masked)
        private readonly string _connectionString = 
            "Server=ProductionDB;Database=CustomerData;User=admin;Password=SuperSecret123";
        
        // ⚠️ FAKE INTERNAL HOSTNAME (will be detected and masked as HOSTNAME_0)
        private readonly string _backupServer = "db-backup.internal.company.com";
        
        // ⚠️ FAKE PRIVATE IP (will be detected and masked as IP_ADDRESS_1)
        private readonly string _cacheServer = "10.0.0.25";
        
        public void Connect()
        {
            Console.WriteLine($"Connecting to: {_connectionString}");
            Console.WriteLine($"Backup server: {_backupServer}");
            Console.WriteLine($"Cache server: {_cacheServer}");
        }
        
        public string GetCustomerData(string customerId)
        {
            // ⚠️ FAKE CREDIT CARD (will be detected and masked as CREDIT_CARD_0)
            var testCard = "4111-1111-1111-1111";
            
            // ⚠️ FAKE SSN (will be detected and masked as SSN_1)
            var testSsn = "987-65-4321";
            
            return $"Customer: {customerId}, Card: {testCard}, SSN: {testSsn}";
        }
    }
}

