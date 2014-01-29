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
        public async Task<HttpResponseMessage> GetFolder(string centerID)
        {
            var result = await GetRootFolder(centerID);

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
        /******************************************************************************/
        /***************************   CREATE/SAVE   **********************************/
        /******************************************************************************/
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> SaveFolder(string fID, Folder f)
        {

            var result = await Create(fID, f);

            if (!result.IsValid)
                return RequestMessage.CreateResponse(HttpStatusCode.BadRequest, result.Errors.First().Message);

            return RequestMessage.CreateResponse(HttpStatusCode.OK, f);
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
                rootFolder.Folders.Add(f);

                await session.SaveChangesAsync();

                //result.Value = f.Id;
            }

            return result;
        }
        #endregion
        
    }
}