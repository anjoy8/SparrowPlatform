using SparrowPlatform.API.Utils;
using SparrowPlatform.Application.ViewModels;
using SparrowPlatform.Infrastruct.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NETCore.MailKit.Core;
using System;
using System.IO;

namespace SparrowPlatform.API.Controllers
{
    /// <summary>
    /// Role manager
    /// </summary>
    [ApiController]
    [Route("api/spa/[controller]")]
    [Authorize(Policy = Authorizor.OnlyCanReadDemo)]
    public class BlobController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly IEmailService _emailService;

        public BlobController(ILogger<UserController> logger,
            IHttpContextAccessor accessor,
            IEmailService emailService
            )
        {
            _logger = logger;
            _accessor = accessor;
            this._emailService = emailService;
        }

        /// <summary>
        /// Download blob file
        /// </summary>
        /// <returns></returns>
        [HttpGet("Download/{url}")]
        public object Download(string url = "")
        {
            try
            {
                var urlData = url.Split('/');
                if (urlData.Length < 3)
                {
                    return ApiResultVo<FileStreamResult>.error("The url format is incorrect.");
                }

                string containerName = urlData[1];
                string fileName = urlData[urlData.Length - 1];
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(AzureADAppSetup.blobFileDownloadConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                string newDirPath = url.Substring($"{urlData[0]}/{urlData[1]}/".Length);
                Console.WriteLine("newDirPath: " + newDirPath);
                CloudBlockBlob blob = container.GetBlockBlobReference(newDirPath);

                if (!Directory.Exists("file-cache"))
                {
                    Directory.CreateDirectory("file-cache");
                }
                using (FileStream fs = new(@$"file-cache/{newDirPath}", FileMode.Create, FileAccess.Write))
                {
                    blob.DownloadToStreamAsync(fs).Wait();
                }

                var stream = System.IO.File.OpenRead($"file-cache/{newDirPath}");

                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception e)
            {
               Console.WriteLine("Exception: " + e);
                return ApiResultVo<FileStreamResult>.error(e.Message);
            }
        }

        /// <summary>
        /// upload a file to blob
        /// </summary>
        /// <returns></returns>
        [HttpPost("upload")]
        public object Upload(IFormFile file)
        {
            if (file == null)
            {
                return ApiResultVo<FileStreamResult>.error("the file is empty.");
            }

            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(AzureADAppSetup.blobFileDownloadConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("ibills");

                var fileName = file.FileName.Substring(0, file.FileName.LastIndexOf("."));
                var fileExtensionName = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                var timeStampName = DateTime.Now.AddHours(8).ToString("yyyyMMdd-HHmmss");
                var uploadBlobFileName = $"{fileName}-{timeStampName}.{fileExtensionName}";
                CloudBlockBlob blob = container.GetBlockBlobReference(uploadBlobFileName);

                if (blob.ExistsAsync().Result)
                {
                    return ApiResultVo<FileStreamResult>.error("There is a file with the same name already exists.");
                }

                blob.UploadFromStreamAsync(file.OpenReadStream()).Wait();

                return ApiResultVo<string>.ok($"https://{AzureADAppSetup.blobAccountName}.xxxxx.cn/xxx/{uploadBlobFileName}");
            }
            catch (Exception e)
            {
                return ApiResultVo<FileStreamResult>.error(e.Message);
            }
        }

    }
}
