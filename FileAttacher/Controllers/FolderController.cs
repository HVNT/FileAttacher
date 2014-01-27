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

        /******************************************************************************/
        /***************************       GET       **********************************/
        /******************************************************************************/
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> GetFolder(string id)
        {
            if (id == null) // ... || "" ??
            {
                var result = await GetRootFolder();

                if (!result.IsValid)
                    return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

                return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value);
            }
            else
            {
                var result = await GetFolderById(id);

                if (!result.IsValid)
                    return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

                return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value);
            }
        }
        private async Task<Result<Folder>> GetRootFolder()
        {

            var result = new Result<Folder>();

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                var rootFolder = await session.LoadAsync<Folder>("folders/33"); //location of root
                if (rootFolder.FileAttsIds.Count > 0)
                {
                    foreach (var id in rootFolder.FileAttsIds)
                    {
                        // load each id and add to fileatts
                        var file = await session.LoadAsync<FileAtt>(id); // get file
                        rootFolder.FileAtts.Add(file); // add file
                    }
                }
                if (rootFolder.FoldersIds.Count > 0)
                {
                    foreach (var id in rootFolder.FoldersIds)
                    {
                        var folder = await session.LoadAsync<Folder>(id); // get folder
                        rootFolder.Folders.Add(folder); // add folder
                    }
                }
                
                if (rootFolder == null)
                {
                    result.AddError("root", "root Folder not found");
                }
                else
                {
                    result.Value = rootFolder;
                }
            }

            return result;
        }
        private async Task<Result<Folder>> GetFolderById(string fId) {

            var result = new Result<Folder>();

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                var rootFolder = await session.LoadAsync<Folder>(fId); //get folder
                if (rootFolder.FileAttsIds.Count > 0)
                {
                    foreach (var id in rootFolder.FileAttsIds)
                    {
                        // load each id and add to fileatts
                        var file = await session.LoadAsync<FileAtt>(id); // get file
                        rootFolder.FileAtts.Add(file); // add file
                    }
                }
                if (rootFolder.FoldersIds.Count > 0)
                {
                    foreach (var id in rootFolder.FoldersIds)
                    {
                        var folder = await session.LoadAsync<Folder>(id); // get folder
                        rootFolder.Folders.Add(folder); // add folder
                    }
                }
                if (rootFolder == null)
                {
                    result.AddError("root", "root Folder not found");
                }
                else
                {
                    result.Value = rootFolder;
                }
            }

            return result;
        }

        //public async Task<HttpResponseMessage> GetFolderInternals(string Id)

        /******************************************************************************/
        /***************************   CREATE/SAVE   **********************************/
        /******************************************************************************/
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> SaveFolder(string fID, Folder f)
        {

            var result = await Create(fID, f);

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value);
        }
        private async Task<Result> Create(string folderID, Folder f)
        {

            var result = new Result();

            if (String.IsNullOrEmpty(f.Filename))
            {
                result.AddError("Folder", "Name required for creation");
                return result;
            }

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                await session.StoreAsync(f);

                var rootFolder = await session.LoadAsync<Folder>(folderID); //location of root
                rootFolder.FoldersIds.Add(f.Id);

                await session.SaveChangesAsync();

                result.Value = f.Id;
            }

            return result;
        }

        /******************************************************************************/
        /***************************     MODIFY      **********************************/
        /******************************************************************************/

        /*
        public async Task<Result> AddFileToFolder(string idFile, string idDestination) // old folder?
        {


        }
         */

        /*
        public async Task<Result> AddFolderToFolder(string idFolder, string idDestination)
        {

        }
        */
        /******************************************************************************/
        /***************************     REMOVE      **********************************/
        /******************************************************************************/
        

    }
}