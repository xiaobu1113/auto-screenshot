using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoScreenShotAI.Services
{
    class AutoDeleteService
    {
        public void AutoDelete(bool enable, string outputDir, string autoDeletionDays)
        {
            if (!enable) return;
            string dir = outputDir.Trim();
            if (!Directory.Exists(dir)) return;
            if (!int.TryParse(autoDeletionDays, out int days) || days <= 0) return;

            foreach (var sub in Directory.GetDirectories(dir))
            {
                string name = Path.GetFileName(sub);
                if (DateTime.TryParseExact(name, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                {
                    if ((DateTime.Now.Date - date.Date).Days > days)
                        try { Directory.Delete(sub, true); } catch { }

                }

            }
        }
    }
}
