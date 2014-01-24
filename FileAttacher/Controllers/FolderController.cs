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

        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> GetRoot()
        {

            var result = await GetRootFolder();

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value);
        }
        private async Task<Result<Folder>> GetRootFolder()
        {

            var result = new Result<Folder>();

            using (var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                var rootFolder = await session.LoadAsync<Folder>("\0");
                //var folders = await session.Query<Folder>().Where(x => x.Id == "0").Take(1).Distinct().ToListAsync() as List<Folder>;
                //var rootFolder = folders.FirstOrDefault(); 
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

    }
}