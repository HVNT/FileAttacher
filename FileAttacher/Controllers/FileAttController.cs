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
        //async prop
        [HttpPost]
        public async Task<HttpResponseMessage> SaveUploads(List<FileAtt> files)
        {

            var result = await BulkSave(files);

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAll()
        {

            var result = await GetFiles();

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> RemoveFile(string Id)
        {
            var result = await Remove(Id);

            if(!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, result.Value); //result.Value = id removed
        }

        private async Task<Result<List<FileAtt>>> GetFiles()
        {

            var result = new Result<List<FileAtt>>();

            using(var session = RavenApiController.DocumentStore.OpenAsyncSession())
            {
                var files = await session.Query<FileAtt>().Distinct().ToListAsync() as List<FileAtt>;
                result.Value = files;
            }

            //check before return?
            return result;
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

        private async Task<Result> Create(FileAtt f)
        {

            var result = new Result();

            if (String.IsNullOrEmpty(f.Filename))
            {
                result.AddError("File", "Name required for creation");
                return result;
            }

            using(var session = RavenApiController.DocumentStore.OpenAsyncSession())
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
	}
}