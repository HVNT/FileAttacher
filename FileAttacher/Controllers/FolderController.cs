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
            if (id == "root") // ... || "" ??
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
                foreach (var id in rootFolder.FileAttsIds)
                {
                    // load each id and add to fileatts
                    var file = await session.LoadAsync<FileAtt>(id); // get file
                    rootFolder.FileAtts.Add(file); // add file
                }
                foreach (var id in rootFolder.FoldersIds)
                {
                    var folder = await session.LoadAsync<Folder>(id); // get folder
                    rootFolder.Folders.Add(folder); // add folder
                }
                /*
                if (rootFolder == null)
                {
                    result.AddError("root", "root Folder not found");
                }
                else
                {
                    result.Value = rootFolder;
                }
                 */
            }

            return result;
        }
        private async Task<Result<Folder>> GetFolderById(string id) {

            var result = new Result<Folder>();

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                var folder = await session.LoadAsync<Folder>(id); // folder @ id
                foreach (var i in folder.FileAttsIds)
                {
                    var file = await session.LoadAsync<FileAtt>(i); // get file
                    folder.FileAtts.Add(file); // add file
                }
                foreach (var i in folder.FoldersIds)
                {
                    var _folder = await session.LoadAsync<Folder>(i); // get folder
                    folder.Folders.Add(_folder); // add folder
                }

                /*
                if (folder == null)
                {
                    result.AddError("root", "root Folder not found");
                }
                else
                {
                    result.Value = folder;
                }
                 */
            }

            return result;
        }

        //public async Task<HttpResponseMessage> GetFolderInternals(string Id)

        /******************************************************************************/
        /***************************   CREATE/SAVE   **********************************/
        /******************************************************************************/
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> SaveFolder(Folder f)
        {

            var result = await Create(f);

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value);
        }
        private async Task<Result> Create(Folder f)
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