using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileAttacher.Models
{
    public interface IResult
    {
        dynamic Info { get; set; }

        List<Error> Errors { get; set; }

        bool IsValid { get; }

        void AddError(string key, string message);
    }

    public class Result : IResult
    {
        public dynamic Info { get; set; }

        public string Value { get; set; }

        public Result()
        {
            Errors = new List<Error>();
        }

        #region IResult Members

        public List<Error> Errors { get; set; }

        public bool IsValid
        {
            get { return Errors == null || Errors.Count == 0; }
        }

        public void AddError(string key, string message)
        {
            if (Errors == null)
            {
                Errors = new List<Error>();
            }
            Errors.Add(new Error(key, message));
        }

        #endregion
    }

    public class Result<T> : IResult
    {
        public dynamic Info { get; set; }

        public T Value { get; set; }

        public Result()
        {
            Errors = new List<Error>();
        }

        #region IResult Members

        public int TotalRecords { get; set; }

        public bool IsStale { get; set; }

        public List<Error> Errors { get; set; }

        public bool IsValid
        {
            get { return Errors == null || Errors.Count == 0; }
        }

        public void AddError(string key, string message)
        {
            if (Errors == null)
            {
                Errors = new List<Error>();
            }
            Errors.Add(new Error(key, message));
        }

        #endregion
    }

    public class FetchResult<T> : IResult
    {
        public dynamic Info { get; set; }

        public IEnumerable<T> Value { get; set; }

        public int? TotalRecords { get; set; }

        public bool IsStale { get; set; }

        public FetchResult()
        {
            Errors = new List<Error>();
        }

        #region IResult Members

        public List<Error> Errors { get; set; }

        public bool IsValid
        {
            get { return Errors == null || Errors.Count == 0; }
        }

        public void AddError(string key, string message)
        {
            if (Errors == null)
            {
                Errors = new List<Error>();
            }
            Errors.Add(new Error(key, message));
        }

        #endregion
    }
}