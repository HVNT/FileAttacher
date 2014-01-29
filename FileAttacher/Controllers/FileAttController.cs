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
        /***************************     REMOVE      **********************************/
        /******************************************************************************/
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> RemoveFile(string centerID, Guid fileID)
        {
            var result = await Remove(centerID, fileID);

            if(!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value); //result.Value = id removed
        }
        private async Task<Result> Remove(string centerID, Guid fileID)
        {

            var result = new Result();

            if (String.IsNullOrEmpty(centerID))
            {
                result.AddError("No centerID", "centerID required for removal");
                return result;
            }

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                FileAtt f = null;
                Boolean found = false;

                // delete file from folder
                Center careCenter = await session.LoadAsync<Center>(centerID); // load care center given ID
                
                Folder temp = careCenter.RootFolder;
                Queue<Folder> q = new Queue<Folder>(); // dfs
                q.Enqueue(temp); // put root on top

                while (q.Count > 0) // while folders remain
                {
                    Folder current = q.Dequeue();

                    foreach (var file in current.FileAtts)
                    {
                        if (file.g == fileID) // file found!
                        {
                            f = file; // get file ref
                            found = true; // set found to true for while break
                            break; // break foreach if found
                        }
                    }
                    if(found) 
                    {
                        break; // if found break while loop
                    }
                    else // !found
                    {
                        foreach (var folder in current.Folders) // add all current avail folders to queue
                        {
                            q.Enqueue(folder);
                        }
                    }
                }

                if (f == null) // shit
                {
                    result.AddError("No File found", "under that guid uhoh.");
                    return result;
                }
                else // all good, file was found
                {
                    session.Delete(f);
                    await session.SaveChangesAsync();

                    result.Value = "successful remove of file w/ guidID" + fileID;
                }
            }

            return result;
        }

        /******************************************************************************/
        /***************************   CREATE/SAVE   **********************************/
        /******************************************************************************/
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> SaveUploads(string centerID, Guid folderID, List<FileAtt> files)
        {

            var result = await BulkSave(centerID, folderID, files);

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value);
        }
        private async Task<Result> BulkSave(string centerID, Guid folderId, List<FileAtt> fAtts)
        {
            var result = new Result();

            foreach (var f in fAtts)
            {
                //check
                await Create(centerID, folderId, f);
            }

            return result;
        }
        private async Task<Result> Create(string centerID, Guid folderId, FileAtt f)
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
                
                Folder targetFolder = null;
                Boolean found = false;

                // delete file from folder
                Center careCenter = await session.LoadAsync<Center>(centerID); // load care center given ID
                
                Folder temp = careCenter.RootFolder;
                Queue<Folder> q = new Queue<Folder>(); // dfs
                q.Enqueue(temp); // put root on top

                while (q.Count > 0) // while folders remain
                {
                    Folder current = q.Dequeue();

                    foreach (var folder in current.Folders)
                    {
                        if (folder.g == folderId) // folder found!
                        {
                            targetFolder = folder; // get folder ref
                            found = true; // set found to true for while break
                            break; // break foreach if found
                        }
                    }
                    if(found) 
                    {
                        break; // if found break while loop
                    }
                    else // !found
                    {
                        foreach (var folder in current.Folders) // add all current avail folders to queue
                        {
                            q.Enqueue(folder);
                        }
                    }
                }

                if (targetFolder == null) // shit
                {
                    result.AddError("No Folder found to add to", "under that guid uhoh.");
                    return result;
                }
                else // all good, folder to add the file too was found
                {
                    // add file to targetFolder
                    targetFolder.FileAtts.Add(f);
                    await session.SaveChangesAsync();

                    result.Value = "successful add of file to folder w/ guidID" + folderId;
                }
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