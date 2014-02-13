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
    [RoutePrefix("api/v1/Folder")]
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
        public async Task<HttpResponseMessage> SaveFolder(FolderProtoContain data)
        {
            string cID = data.centerIndex;
            Guid folderID = data.folderID;
            Folder f = data.newfolder;

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
        public async Task<HttpResponseMessage> RemoveFolder(FolderProtoContain data)
        {
            string cID = data.centerIndex;
            Guid folderID = data.folderID;

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
                Folder current = null;

                Queue<Folder> q = new Queue<Folder>(); // dfs
                q.Enqueue(temp); // put root on top

                while (q.Count > 0) // while folders remain
                {
                    current = q.Dequeue();

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
                    current.Folders.Remove(targetFolder);
                    await session.SaveChangesAsync();

                    result.Value = "successful remove of folder to folder w/ guidID" + folderID;
                }
            }

            return result;
        }
        #endregion
        
        #region MOVE_FILE
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> MoveFile(FolderProtoContain data)
        {
            string cID = data.centerIndex;
            Guid currFolderID = data.folderID;
            Guid fileToMoveID = data.newfolder.FileAtts[0].g;
            Guid targetFolderID = data.newfolder.g;

            var result = await Move(cID, currFolderID, fileToMoveID, targetFolderID);

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value); //result.Value = id removed
        }

        private async Task<Result> Move(string cID, Guid currFolderID, Guid fileToMoveID, Guid targetFolderID)
        {
            var result = new Result();

            if (String.IsNullOrEmpty(cID)) // have more handlers here
            {
                result.AddError("No centerID", "centerID required for removal");
                return result;
            }

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                Folder oldFolder = null;
                Center careCenter = await session.LoadAsync<Center>(cID); // load care center given ID
                Folder temp = careCenter.RootFolder;
                Folder current = null;

                Queue<Folder> q = new Queue<Folder>(); // dfs
                q.Enqueue(temp); // put root on top

                while (q.Count > 0) // while folders remain
                {
                    current = q.Dequeue();

                    foreach (var folder in current.Folders)
                    {
                        if (folder.g == currFolderID) // folder found!
                        {
                            oldFolder = folder; // get folder ref
                            break; // break foreach if found
                        }
                    }
                    if (oldFolder != null)
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

                if (oldFolder == null) // shit
                {
                    result.AddError("No Folder found to add to", "under that guid uhoh.");
                    return result;
                }
                else // all good, targetFolder to delete was found
                {
                    FileAtt fileToMove = null;

                    foreach (var file in oldFolder.FileAtts)
                    {
                        if (file.g == fileToMoveID) // file found!
                        {
                            // get ref of file so we can remove from currFolder
                            fileToMove = file;

                            // remove file at currLocation so we can move it
                            oldFolder.FileAtts.Remove(file);
                            break; // break foreach if found
                        }
                    }
                    // now find the folder we are moving  the file too
                    Folder _targetFolder = null;
                    Folder _temp = careCenter.RootFolder;
                    Folder _current = null;

                    Queue<Folder> _q = new Queue<Folder>(); // dfs
                    _q.Enqueue(temp); // put root on top

                    while (_q.Count > 0) // while folders remain
                    {
                        _current = _q.Dequeue();

                        foreach (var folder in _current.Folders)
                        {
                            if (folder.g == currFolderID) // folder found!
                            {
                                _targetFolder = folder; // get folder ref
                                break; // break foreach if found
                            }
                        }
                        if (_targetFolder != null)
                        {
                            break; // if found break while loop
                        }
                        else // !found
                        {
                            foreach (var folder in _current.Folders) // add all current avail folders to queue
                            {
                                q.Enqueue(folder);
                            }
                        }
                    }
                    if (_targetFolder == null) // shit
                    {
                        result.AddError("No Folder found to add to", "under that guid uhoh.");
                        return result;
                    }
                    else // all good, targetFolder to move new file to was found
                    {
                        _targetFolder.FileAtts.Add(fileToMove); // add file
                    }

                    await session.SaveChangesAsync(); // save changes
                    result.Value = "successful move of file " + fileToMove.Filename + " to folder " + _targetFolder.Filename;
                }
            }

            return result;
        }
        #endregion
    }
}