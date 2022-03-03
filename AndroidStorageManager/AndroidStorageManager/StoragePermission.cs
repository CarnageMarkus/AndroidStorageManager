using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidStorageManager
{
    public interface IStoragePermission
    {
        StoragePermissionResult GetPermissionStatus();
        Task<StoragePermissionResult> RequestStoragePermission();
    }

    public class StoragePermission
    {
        public string Name { get; set; }
        public bool Granted { get; set; }
    }

    public class StoragePermissionResult
    {
        public string Description { get; set; }
        public List<StoragePermission> Permissions { get; set; }

        public bool Granted() => Permissions?.All(x => x.Granted) ?? false;
    }
}
