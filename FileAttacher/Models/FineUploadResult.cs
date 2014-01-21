using Newtonsoft.Json.Linq;
using System.Web.Mvc;

namespace FineUploader
{
    /// <remarks>
    /// Docs at https://github.com/valums/file-uploader/blob/master/server/readme.md
    /// </remarks>
    public class FineUploaderResult : ActionResult
    {
        public const string ResponseContentType = "text/plain";

        private readonly bool _success;
        private readonly string _error;
        private readonly bool? _preventRetry;
        private readonly JObject _otherData;

        public FineUploaderResult(bool success, object otherData = null, string error = null, bool? preventRetry = null)
        {
            _success = success;
            _error = error;
            _preventRetry = preventRetry;

            if (otherData != null)
                _otherData = JObject.FromObject(otherData);
        }

        /*
         * @param ControllerContext - encapsulates information about an HTTP request
         * that matches specified RouteBase adn ControllerBase instances
         */
        public override void ExecuteResult(ControllerContext context)
        {
            /* 
             * @HttpContext - encapsulates all HTTP-specific information about an
             * individual HTTP request
             */
            var response = context.HttpContext.Response;
            response.ContentType = ResponseContentType;

            response.Write(BuildResponse());
        }

        public string BuildResponse()
        {
            var response = _otherData ?? new JObject();
            response["success"] = _success;

            if (!string.IsNullOrWhiteSpace(_error))
                response["error"] = _error;

            if (_preventRetry.HasValue)
                response["preventRetry"] = _preventRetry.Value;

            return response.ToString();
        }
    }
}