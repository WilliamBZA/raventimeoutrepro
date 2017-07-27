using System;
using System.Threading.Tasks;
using NServiceBus;
using Raven.Client.Document;
using System.Security.Cryptography;
using System.Text;
using Raven.Client.Document.DTC;
using System.IO;

class Program
{
    static void Main()
    {
        AsyncMain().GetAwaiter().GetResult();
    }

    static async Task AsyncMain()
    {
        Console.Title = "Samples.RavenDB.Server";
        using (new RavenHost())
        {
            var endpointConfiguration = new EndpointConfiguration("Samples.RavenDB.Server");

            var resourceManagerId = DeterministicGuidBuilder("Samples.RavenDB.Server" + Environment.MachineName);
            var dtcRecoveryBasePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var recoveryPath = Path.Combine(dtcRecoveryBasePath, "NServiceBus.RavenDB", resourceManagerId.ToString());

            var store = new DocumentStore
            {
                ConnectionStringName = "NServiceBus/Persistence/RavenDB",
                ResourceManagerId = resourceManagerId,
                TransactionRecoveryStorage = new LocalDirectoryTransactionRecoveryStorage(recoveryPath),
                DefaultDatabase = "test"
            };

            store.Initialize();

            var persistence = endpointConfiguration.UsePersistence<RavenDBPersistence>();
            // Only required to simplify the sample setup
            persistence.DoNotSetupDatabasePermissions();
            persistence.SetDefaultDocumentStore(store);

            endpointConfiguration.UseTransport<LearningTransport>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }

    static Guid DeterministicGuidBuilder(string input)
    {
        // use MD5 hash to get a 16-byte hash of the string
        using (var provider = new MD5CryptoServiceProvider())
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = provider.ComputeHash(inputBytes);
            // generate a guid from the hash:
            return new Guid(hashBytes);
        }
    }
}