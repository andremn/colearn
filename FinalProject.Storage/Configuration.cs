using System.Configuration;

namespace FinalProject.Storage
{
    internal static class Configuration
    {
        public const string AccountKey =
            "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

        public const string AccountName = "devstoreaccount1";

        public static string StorageConnectionString => ConfigurationManager.AppSettings["StorageConnectionString"];
    }
}