using FileAttacher.Models;
using Raven.Client;
using Raven.Client.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FileAttacher.Controllers
{
    public class FileAttController : RavenApiController
    {
        public FileAttController()
        {
            var test = RequestMessage;
        }

        /******************************************************************************/
        /***************************       GET       **********************************/
        /******************************************************************************/
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> GetAll()
        {

            var result = await GetFiles();

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value);
        }
        private async Task<Result<List<FileAtt>>> GetFiles()
        {

            var result = new Result<List<FileAtt>>();

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                var files = await session.Query<FileAtt>().Distinct().ToListAsync() as List<FileAtt>;
                result.Value = files;
            }

            //check before return?
            return result;
        }

        /*
         * Get Files by Ids 
         */
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> GetFilesByIds(List<string> fIds)
        {

            var result = await GetByIds(fIds);

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value);
        }
        private async Task<Result<List<FileAtt>>> GetByIds(List<string> fAtts)
        {
            var result = new Result<List<FileAtt>>();

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                foreach (var fId in fAtts)
                {
                    var file = await session.LoadAsync<FileAtt>(fId);

                    if (file == null)
                    {
                        result.AddError("file", "file not found");
                    }
                    else
                    {
                        result.Value.Add(file);
                    }
                }
            }

            return result;
        }

        /******************************************************************************/
        /***************************     REMOVE      **********************************/
        /******************************************************************************/
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> RemoveS3File(string f)
        {
            var result = await Remove(f);

            if(!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value); //result.Value = id removed
        }
        private async Task<Result> Remove(string Id)
        {

            var result = new Result();

            if (String.IsNullOrEmpty(Id))
            {
                result.AddError("File", "Name required for removal");
                return result;
            }

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                FileAtt existingFile = await session.LoadAsync<FileAtt>(Id);
                session.Delete(existingFile);
                await session.SaveChangesAsync();

                result.Value = "success"; // put string here instead with success remove msg?
            }

            return result;
        }

        /******************************************************************************/
        /***************************   CREATE/SAVE   **********************************/
        /******************************************************************************/
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> SaveUploads(List<FileAtt> files)
        {

            var result = await BulkSave(files);

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value);
        }
        private async Task<Result> Create(FileAtt f)
        {

            var result = new Result();

            if (String.IsNullOrEmpty(f.Filename))
            {
                result.AddError("File", "Name required for creation");
                return result;
            }
            else // set mime type on save now vs return with S3
            {
                f.MimeType = ReturnExtension(f.Extension);
            }

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                await session.StoreAsync(f);
                await session.SaveChangesAsync();

                result.Value = f.Id;
            }

            return result;
        }
        private async Task<Result> BulkSave(List<FileAtt> fAtts)
        {
            var result = new Result();

            foreach (var f in fAtts)
            {
                //check
                await Create(f);
            }

            return result;
        }

        private string ReturnExtension(string fileExtension)
        {
            try
            {
                fileExtension = fileExtension.ToLower();
                switch (fileExtension)
                {
                    case ".htm":
                    case ".html":
                    case ".log":
                        return "text/HTML";

                    case ".txt":
                        return "text/plain";

                    case ".doc":
                        return "application/ms-word";

                    case ".docx":
                        return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                    case ".dotx":
                        return "application/vnd.openxmlformats-officedocument.wordprocessingml.template";

                    case ".tiff":
                    case ".tif":
                        return "image/tiff";

                    case ".png":
                        return "image/png";

                    case ".asf":
                        return "video/x-ms-asf";

                    case ".avi":
                        return "video/avi";

                    case ".zip":
                        return "application/zip";

                    case ".xls":
                    case ".csv":
                        return "application/vnd.ms-excel";

                    case ".xlsx":
                        return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    case ".xltx":
                        return "application/vnd.openxmlformats-officedocument.spreadsheetml.template";

                    case ".gif":
                        return "image/gif";

                    case ".jpg":
                    case "jpeg":
                        return "image/jpeg";

                    case ".bmp":
                        return "image/bmp";

                    case ".wav":
                        return "audio/wav";

                    case ".mp3":
                        return "audio/mpeg3";

                    case ".mpg":
                    case "mpeg":
                        return "video/mpeg";

                    case ".rtf":
                        return "application/rtf";

                    case ".asp":
                        return "text/asp";

                    case ".pdf":
                        return "application/pdf";

                    case ".fdf":
                        return "application/vnd.fdf";

                    case ".ppt":
                        return "application/mspowerpoint";

                    case ".pptx":
                        return "application/vnd.openxmlformats-officedocument.presentationml.presentation";

                    case ".ppsx":
                        return "application/vnd.openxmlformats-officedocument.presentationml.slideshow";

                    case ".potx":
                        return "application/vnd.openxmlformats-officedocument.presentationml.template";

                    case ".dwg":
                        return "image/vnd.dwg";

                    case ".msg":
                        return "application/msoutlook";

                    case ".xml":
                    case ".sdxl":
                        return "application/xml";

                    case ".xdp":
                        return "application/vnd.adobe.xdp+xml";

                    default:
                        return "application/octet-stream";
                }
            }
            catch (Exception e)
            {
                return "application/octet-stream"; //default case
            }
        }
	}
}