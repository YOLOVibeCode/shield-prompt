// 🎓 TUTORIAL FILE - Contains FAKE secrets for learning ShieldPrompt
// These are NOT real credentials - they're educational examples!

using System;

namespace TutorialApp
{
    public class Program
    {
        // ⚠️ FAKE DATABASE NAME (will be detected and masked as DATABASE_0)
        private const string DatabaseName = "ProductionDB";
        
        // ⚠️ FAKE IP ADDRESS (will be detected and masked as IP_ADDRESS_0)
        private const string ServerIp = "192.168.1.50";
        
        // ⚠️ FAKE AWS KEY (will be detected and masked as AWS_KEY_0)
        private const string AwsAccessKey = "AKIAIOSFODNN7EXAMPLE";
        
        // ⚠️ FAKE SSN (will be detected and masked as SSN_0)
        private const string AdminSsn = "123-45-6789";
        
        // ⚠️ FAKE PASSWORD (will be detected and masked as PASSWORD_0)
        private const string ApiPassword = "MySecretPassword123";
        
        public static void Main(string[] args)
        {
            Console.WriteLine("🎓 ShieldPrompt Tutorial Application");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            Console.WriteLine($"Database: {DatabaseName}");
            Console.WriteLine($"Server IP: {ServerIp}");
            Console.WriteLine($"AWS Key: {AwsAccessKey}");
            Console.WriteLine($"Admin SSN: {AdminSsn}");
            Console.WriteLine($"API Password: {ApiPassword}");
            Console.WriteLine();
            Console.WriteLine("⚠️  These are FAKE credentials for tutorial purposes!");
            Console.WriteLine("📝 When you copy this file with ShieldPrompt:");
            Console.WriteLine("   • All sensitive values will be automatically masked");
            Console.WriteLine("   • You'll see safe aliases like DATABASE_0, IP_ADDRESS_0");
            Console.WriteLine("   • ChatGPT will never see the real values");
        }
    }
}

