using System.Xml.Linq;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.DbPersistence;


namespace WorkflowLib
{
    public static class WorkflowInit
    {
        // Menggunakan Lazy<T> untuk memastikan bahwa WorkflowRuntime hanya diinisialisasi ketika dibutuhkan.
        [Obsolete]
        private static readonly Lazy<WorkflowRuntime> LazyRuntime = new Lazy<WorkflowRuntime>(InitWorkflowRuntime);

        // Property yang mengembalikan instance dari WorkflowRuntime. LazyRuntime.Value akan menginisialisasi runtime jika belum diinisialisasi.
        [Obsolete]
        public static WorkflowRuntime Runtime
        {
            get { return LazyRuntime.Value; }
        }

        // Properti untuk menyimpan connection string ke database. Tipe nullable (string?) untuk menghindari warning jika belum diinisialisasi.
        public static string? ConnectionString { get; set; }

        // Metode untuk menginisialisasi WorkflowRuntime. Ini dipanggil saat LazyRuntime.Value diakses.
        [Obsolete]
        private static WorkflowRuntime InitWorkflowRuntime()
        {
            // Memastikan bahwa ConnectionString telah diinisialisasi sebelum runtime dijalankan.
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new Exception("Please init ConnectionString before calling the Runtime!");
            }

            // Membuat instance MSSQLProvider untuk penyimpanan workflow di database SQL Server menggunakan ConnectionString.
            var dbProvider = new MSSQLProvider(ConnectionString);

            // Membuat builder workflow yang menggunakan provider database untuk menyimpan dan memuat definisi workflow.
            var builder = new WorkflowBuilder<XElement>(
                dbProvider,
                new OptimaJet.Workflow.Core.Parser.XmlWorkflowParser(), // Parser untuk workflow dalam format XML
                dbProvider
            ).WithDefaultCache(); // Menggunakan cache default untuk meningkatkan performa.

            // Membuat instance WorkflowRuntime dengan builder dan provider penyimpanan (persistence provider).
            var runtime = new WorkflowRuntime()
                .WithBuilder(builder)  // Mengatur builder untuk runtime
                .WithPersistenceProvider(dbProvider) // Mengatur penyimpanan untuk runtime
                .EnableCodeActions()  // Mengaktifkan action yang ditulis dalam kode
                .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn() // Mengaktifkan auto update skema database sebelum mendapatkan perintah yang tersedia
                .AsSingleServer();  // Menjalankan sebagai instance server tunggal (untuk keperluan clustering atau distribusi)

            // Menambahkan plugin dasar ke runtime. Plugin ini bisa digunakan untuk fungsi tambahan seperti mengirim email.
            var plugin = new OptimaJet.Workflow.Plugins.BasicPlugin();
            runtime.WithPlugin(plugin);

            // Event yang dipanggil ketika aktivitas workflow berubah.
            runtime.ProcessActivityChanged += (sender, args) => { };

            // Event yang dipanggil ketika status workflow berubah.
            runtime.ProcessStatusChanged += (sender, args) => { };

            // Memulai workflow runtime. Ini penting agar workflow bisa dieksekusi dan diproses.
            runtime.Start();

            // Mengembalikan instance runtime yang telah diinisialisasi.
            return runtime;
        }
    }
}
