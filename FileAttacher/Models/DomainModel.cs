using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileAttacher.Models
{
    public class DomainModel
    {
        public DomainModel()
        {
            StartedAt = DateTime.UtcNow;
            //Activation = ActivationStates.Active;
        }

        public string Id { get; set; }

        public string Summary { get; set; }

        #region Internal use only

        public DateTime StartedAt { get; protected set; }

        public DateTime? EndedAt { get; protected set; }

        //public ActivationStates Activation { get; set; }

        #endregion

        public virtual Result Validate() // should make abstract
        {
            return new Result();
        }
    }
}