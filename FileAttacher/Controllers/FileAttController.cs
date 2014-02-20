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
    //[RoutePrefix("api/v1/FileAtt")]
    public class FileAttController : RavenApiController
    {
        public FileAttController()
        {
            var test = RequestMessage;
        }

        #region Remove
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> RemoveFile(FileProtoContain data)
        {
            string cID = data.centerIndex;
            Guid fileID = data.ID;

            var result = await Remove(cID, fileID);

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

                // delete file from folder
                Center careCenter = await session.LoadAsync<Center>(centerID); // load care center given ID

                Folder current = null;
                Folder temp = careCenter.RootFolder;
                Queue<Folder> q = new Queue<Folder>(); // dfs
                q.Enqueue(temp); // put root on top

                while (q.Count > 0) // while folders remain
                {
                    current = q.Dequeue();

                    foreach (var file in current.FileAtts)
                    {
                        if (file != null)
                        {
                            if (file.g == fileID) // file found!
                            {
                                f = file; // get file ref
                                break; // break foreach if found
                            }
                        }
                    }
                    if(f != null) 
                    {
                        break; // if found break while loop
                    }
                    else // !found
                    {
                        if(current.Folders.Count > 0)
                        {
                            foreach (var folder in current.Folders) // add all current avail folders to queue
                            {
                                q.Enqueue(folder);
                            }
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
                    current.FileAtts.Remove(f);
                    await session.SaveChangesAsync();

                    result.Value = "successful remove of file w/ guidID" + fileID;
                }
            }

            return result;
        }
        #endregion

        #region Create/Save
        //[Route("SaveUploads")]
        [HttpPost]
        public async Task<HttpResponseMessage> SaveUploads(FileProtoContain data)
        {
            String cID = data.centerIndex;
            Guid g = data.ID;
            List<FileAtt> files = data.FileAtts;

            var result = await BulkSave(cID, g, files);

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

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {                
                // delete file from folder
                Center careCenter = await session.LoadAsync<Center>(centerID); // load care center given ID

                Folder targetFolder = null;
                Folder temp = careCenter.RootFolder;
                
                // quick check to see if at root since this is most likely use case
                if (temp.g == folderId)
                {
                    targetFolder = temp; // set to target

                    targetFolder.FileAtts.Add(f); // add to target
                    await session.SaveChangesAsync();

                    result.Value = "successful add of file to folder w/ guidID" + folderId;

                    return result;
                }

                // else not at root lvl and begin bfs

                Queue<Folder> q = new Queue<Folder>(); // bfs queue
                q.Enqueue(temp); // put root on top

                while (q.Count > 0) // while folders remain
                {
                    Folder current = q.Dequeue();

                    foreach (var folder in current.Folders)
                    {
                        if (folder != null)
                        {
                            if (folder.g == folderId) // folder found!
                            {
                                targetFolder = folder; // get folder ref
                                break; // break foreach if found
                            }
                        }
                    }
                    if(f != null) 
                    {
                        break; // if found break while loop
                    }
                    else // !found
                    {
                        if (current.Folders.Count > 0)
                        {
                            foreach (var folder in current.Folders) // add all current avail folders to queue
                            {
                                q.Enqueue(folder);
                            }
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
                }
            }

            return result;
        }
        #endregion
    }
}