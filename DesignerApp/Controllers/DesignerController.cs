using System.Collections.Specialized;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using OptimaJet.Workflow;
using WorkflowLib;

namespace DesignerApp.Controllers
{
    public class DesignerController : Controller
    {
        // Menampilkan tampilan awal untuk controller ini
        public IActionResult Index()
        {
            return View();
        }

        // Endpoint API yang menangani permintaan POST dan GET dari designer workflow
        [Obsolete]
        public async Task<IActionResult> Api()
        {
            Stream? filestream = null;
            var isPost = Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);

            // Menangani file upload jika metode request adalah POST dan ada file yang diupload
            if (isPost && Request.Form.Files != null && Request.Form.Files.Count > 0)
                filestream = Request.Form.Files[0].OpenReadStream();

            // Mengumpulkan parameter query dari URL
            var pars = new NameValueCollection();
            foreach (var q in Request.Query)
            {
                pars.Add(q.Key, q.Value.First());
            }

            // Jika metode request adalah POST, tambahkan parameter dari form data ke NameValueCollection
            if (isPost)
            {
                var parsKeys = pars.AllKeys;
                foreach (string key in Request.Form.Keys)
                {
                    if (!parsKeys.Contains(key))
                    {
                        pars.Add(key, Request.Form[key]);
                    }
                }
            }

            // Memanggil API designer dari WorkflowInit untuk memproses permintaan
            (string res, bool hasError) = await WorkflowInit.Runtime.DesignerAPIAsync(pars, filestream);

            // Menentukan jenis operasi dari parameter dan mengembalikan hasil yang sesuai
            var operation = pars["operation"]?.ToLower();
            if (operation == "downloadscheme" && !hasError)
                // Mengembalikan hasil sebagai file XML jika operasi adalah downloadscheme
                return File(Encoding.UTF8.GetBytes(res), "text/xml");
            else if (operation == "downloadschemebpmn" && !hasError)
                // Mengembalikan hasil sebagai file XML jika operasi adalah downloadschemebpmn
                return File(UTF8Encoding.UTF8.GetBytes(res), "text/xml");

            // Mengembalikan konten sebagai string jika tidak ada operasi pengunduhan atau ada kesalahan
            return Content(res);
        }
    }
}
