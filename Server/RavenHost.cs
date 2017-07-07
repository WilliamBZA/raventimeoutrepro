using System;
using System.Diagnostics;
using Raven.Client.Embedded;
using System.IO;

class RavenHost :
    IDisposable
{
    public RavenHost()
    {
        EmptyFolder("Data");
        EmptyFolder("RavenSampleData");

        documentStore = new EmbeddableDocumentStore
        {
            DataDirectory = "Data",
            UseEmbeddedHttpServer = true,
            DefaultDatabase = "RavenSampleData",
            Configuration =
            {
                Port = 32076,
                PluginsDirectory = Environment.CurrentDirectory,
                HostName = "localhost",
                DefaultStorageTypeName = "esent"
            }
        };
        documentStore.Initialize();
        // since hosting a fake raven server in process remove it from the logging pipeline
        Trace.Listeners.Clear();
        Trace.Listeners.Add(new DefaultTraceListener());
        Console.WriteLine("Raven server started on http://localhost:32076/");
    }

    private void EmptyFolder(string folder)
    {
        if (Directory.Exists(folder))
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch { }
        }
    }

    EmbeddableDocumentStore documentStore;

    public void Dispose()
    {
        documentStore?.Dispose();
    }
}