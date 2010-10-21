using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRMSamples
{
    public class PostViewModel
    {
        public string Body { get; set; }

        public int Id { get; set; }

        public DateTime CreationDate { get; set; }

        public int VotesCount { get; set; }
    }
}
