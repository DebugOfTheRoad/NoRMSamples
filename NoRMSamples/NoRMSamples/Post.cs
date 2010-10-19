using System;
using System.Collections.Generic;

namespace NoRMSamples
{
    public class Post:Entity<Post>
    {
        public string Body { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifyDate { get; set; }
        public IList<string > Tags { get; set; }
        public IList<Comment> Comments { get; set; }
        public PostStatistics Statistics { get; set; }
        
    }
}