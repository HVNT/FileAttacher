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
    public class FolderController : RavenApiController
    {
        public FolderController()
        {
            var test = RequestMessage;
        }

        #region GET
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> GetFolder(string cID)
        {
            var result = await GetRootFolder(cID);

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value);
        }
        private async Task<Result<Folder>> GetRootFolder(string centerID)
        {

            var result = new Result<Folder>();

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                Center careCenter = await session.LoadAsync<Center>(centerID);

                if (careCenter == null)
                {
                    result.AddError("root", "root Folder not found");
                }
                else
                {
                    result.Value = careCenter.RootFolder;
                }
            }

            return result;
        }
        #endregion

        #region CREATE/SAVE
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> SaveFolder(string cID, Guid folderID, Folder f)
        {

            var result = await Create(cID, folderID, f);

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, f);
        }
        private async Task<Result> Create(string centerID, Guid folderID, Folder f)
        {

            var result = new Result();

            if (String.IsNullOrEmpty(f.Filename))
            {
                result.AddError("Folder", "Name required for creation");
                return result;
            }

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {

                Folder targetFolder = null;
                Center careCenter = await session.LoadAsync<Center>(centerID); // load care center given ID
                Folder temp = careCenter.RootFolder;

                // quick check to see if at root since this is most likely use case\
                if (temp.g == folderID)
                {
                    targetFolder = temp; // set to target

                    targetFolder.Folders.Add(f); // add to target
                    await session.SaveChangesAsync();

                    result.Value = "successful add of file to folder w/ guidID" + folderID;

                    return result;
                }
                Queue<Folder> q = new Queue<Folder>(); // dfs
                q.Enqueue(temp); // put root on top

                while (q.Count > 0) // while folders remain
                {
                    Folder current = q.Dequeue();

                    foreach (var folder in current.Folders)
                    {
                        if (folder.g == folderID) // folder found!
                        {
                            targetFolder = folder; // get folder ref
                            break; // break foreach if found
                        }
                    }
                    if (targetFolder!=null)
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
                    targetFolder.Folders.Add(f);
                    await session.SaveChangesAsync();

                    result.Value = "successful add of folder to folder w/ guidID" + folderID;
                }
            }

            return result;
        }
        #endregion

        #region DELETE
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> RemoveFolder(string cID, Guid folderID)
        {
            var result = await Remove(cID, folderID);

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value); //result.Value = id removed
        }
        private async Task<Result> Remove(string centerID, Guid folderID)
        {

            var result = new Result();

            if (String.IsNullOrEmpty(centerID))
            {
                result.AddError("No centerID", "centerID required for removal");
                return result;
            }

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                Folder targetFolder = null;
                Center careCenter = await session.LoadAsync<Center>(centerID); // load care center given ID
                Folder temp = careCenter.RootFolder;

                Queue<Folder> q = new Queue<Folder>(); // dfs
                q.Enqueue(temp); // put root on top

                while (q.Count > 0) // while folders remain
                {
                    Folder current = q.Dequeue();

                    foreach (var folder in current.Folders)
                    {
                        if (folder.g == folderID) // folder found!
                        {
                            targetFolder = folder; // get folder ref
                            break; // break foreach if found
                        }
                    }
                    if (targetFolder != null)
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
                else // all good, targetFolder to delete was found
                {
                    // delete targetFolder
                    session.Delete(targetFolder);
                    await session.SaveChangesAsync();

                    result.Value = "successful add of folder to folder w/ guidID" + folderID;
                }
            }

            return result;
        }
        #endregion
    }
}