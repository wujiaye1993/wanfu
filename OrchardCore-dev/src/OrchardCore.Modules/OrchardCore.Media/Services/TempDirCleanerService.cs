using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Media.Services
{
    public class TempDirCleanerService : IModularTenantEvents
    {
        private readonly IMediaFileStore _fileStore;
        private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger<TempDirCleanerService> _logger;

        public TempDirCleanerService(IMediaFileStore fileStore,
            AttachedMediaFieldFileService attachedMediaFieldFileService,
            ShellSettings shellSettings,
            ILogger<TempDirCleanerService> logger)
        {
            _fileStore = fileStore;
            _attachedMediaFieldFileService = attachedMediaFieldFileService;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public async Task ActivatedAsync()
        {
            if (_shellSettings.State != Environment.Shell.Models.TenantState.Uninitialized)
            {
                var tempDir = _attachedMediaFieldFileService.MediaFieldsTempSubFolder;

                if (await _fileStore.GetDirectoryInfoAsync(tempDir) == null)
                {
                    return;
                }

                var contents = await _fileStore.GetDirectoryContentAsync(tempDir);

                foreach (var c in contents)
                {
                    var result = c.IsDirectory ?
                        await _fileStore.TryDeleteDirectoryAsync(c.Path)
                        : await _fileStore.TryDeleteFileAsync(c.Path);

                    if (!result)
                    {
                        _logger.LogWarning("Temporary entry {0} could not be deleted.", c.Path);
                    }
                }
            }
        }

        public Task ActivatingAsync()
        {            
            return Task.CompletedTask;
        }

        public Task TerminatedAsync()
        {
            return Task.CompletedTask;
        }

        public Task TerminatingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
