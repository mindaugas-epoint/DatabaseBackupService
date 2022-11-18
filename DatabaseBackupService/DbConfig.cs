namespace DatabaseBackupService
{
    public class DBConfig
    {
        public string SqlServerType { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool WindowsOS { get; set; }

        public string GetConnectionString()
        {
            string connectionString;

            if (SqlServerType == "mysql")
            {
                connectionString = $"Server={ServerName};Database={DatabaseName};User ID={UserName};Password={Password};CHARSET=utf8;";
            } 
            else
            {
                connectionString = $"Data Source={ServerName};Initial Catalog={DatabaseName};Persist Security Info=True;Encrypt=True;TrustServerCertificate=True;User ID={UserName};Password={Password}";
            }
            return connectionString;
        }
    }
}
