namespace TrackingService
{
    using System;
    using System.Data.SqlClient;


    public static class SagaDbContextFactoryProvider
    {
        /// <summary>
        /// This is a list of the connection strings that we will attempt to find what LocalDb versions
        /// are on the local pc which we can run the unit tests against
        /// </summary>
        static readonly string[] _possibleLocalDbConnectionStrings = new[]
        {
            @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=MassTransitUnitTests_v12_2015;",
            // the localdb installed with VS 2015
            @"Data Source=(LocalDb)\ProjectsV12;Integrated Security=True;Initial Catalog=MassTransitUnitTests_v12;",
            // the localdb with VS 2013
            @"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog=MassTransitUnitTests_v11;"
            // the older version of localdb
        };

        static readonly Lazy<string> _connectionString = new Lazy<string>(GetLocalDbConnectionString);

        public static string ConnectionString => _connectionString.Value;

        /// <summary>
        /// Loops through the array of potential localdb connection strings to find one that we can use for the unit tests
        /// </summary>
        public static string GetLocalDbConnectionString()
        {
            // Lets find a localdb that we can use for our unit test
            foreach (var connectionString in _possibleLocalDbConnectionStrings)
            {
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        // It worked, we can save this as our connection string
                        return connectionString;
                    }
                }
                catch (Exception)
                {
                }
            }

            throw new InvalidOperationException(
                "Couldn't connect to any of the LocalDB Databases. You might have a version installed that is not in the list. Please check the list and modify as necessary");
        }
    }
}