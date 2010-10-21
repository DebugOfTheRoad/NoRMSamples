﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FizzWare.NBuilder;
using Norm;
using Norm.Configuration;
using NUnit.Framework;
using Norm.BSON;

namespace NoRMSamples
{
    [TestFixture]
    public class QueringExamples
    {
        private Mongod _proc;
        private IList<Post> _postsInDb;

        [TestFixtureSetUp]
        public void SetupTestFixture()
        {
            //starts new mongodb proc
            _proc = new Mongod();

            // Post collection in MongoDb will be named posts
            MongoConfiguration.Initialize(
                conf=>conf.For<Post>(typeConf=>typeConf.UseCollectionNamed("posts"))
                );
            //generates list of 30 posts and inserts them to db);
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {    
               _postsInDb= Builder<Post>.CreateListOfSize(30)
                    .WhereAll()
                        .Have(post =>post.Comments = Builder<Comment>.CreateListOfSize(10).Build())
                        .Have(post =>post.Tags=new List<string>{"python","c++","c#"})
                        .Have(post=>post.Statistics=Builder<PostStatistics>.CreateNew().Build())
                        .Have(x=>x.Id=0).Build().ToList();

                db.GetCollection<Post>().Insert(_postsInDb);
              
            }
        }

        [TestFixtureTearDown]
        public void TearDownTestFixture()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                //deletes generated posts from db
                db.GetCollection<Post>().Delete(new{});
            
            }
            _proc.Dispose();
        }


        [Test]
        public void should_return_every_post_from_posts_collection()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell:  db.posts.find({});

                var postsFromLinqProviderQuery = db.GetCollection<Post>().AsQueryable().ToList();

                var postsAnonymousObjects = db.GetCollection<Post>().Find().ToList();


                postsFromLinqProviderQuery.Count.ShouldEqual(30);
                postsAnonymousObjects.Count.ShouldEqual(30);

                postsFromLinqProviderQuery.ShouldContainOnly(_postsInDb);
                postsAnonymousObjects.ShouldContainOnly(_postsInDb);
            }
        }

        [Test]
        public void should_return_all_posts_with_title_Title_1_and_author_name_Author_1()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.find({Title: 'Title 1',AuthorName:'Author 1'})

                var postsFromLinqProviderQuery = db.GetCollection<Post>().AsQueryable().Where(post=>post.Title=="Title1" && post.AuthorName=="AuthorName1").ToList();

                var postsAnonymousObjects = db.GetCollection<Post>().Find(new{ Title="Title1", AuthorName="AuthorName1"}).ToList();

                //this is in memory query
                var postsInDbWithTitleAndAuthorName=_postsInDb.Where(post => post.Title == "Title1" && post.AuthorName == "AuthorName1");

                postsFromLinqProviderQuery.ShouldContainOnly(postsInDbWithTitleAndAuthorName);
                postsAnonymousObjects.ShouldContainOnly(postsInDbWithTitleAndAuthorName);
            }
        }

        [Test]
        public void should_return_single_post_with_id_1()
        {

            //MongoDB shell: db.posts.find({_id:1});
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var postFromLinqProviderQuery = db.GetCollection<Post>().AsQueryable().Single(p => p.Id == _postsInDb[0].Id);
                var postAnonymousObjects = db.GetCollection<Post>().FindOne(new { Id = _postsInDb[0].Id });

                postFromLinqProviderQuery.ShouldNotBeNull();
                postAnonymousObjects.ShouldNotBeNull();

                postFromLinqProviderQuery.Id.ShouldEqual(_postsInDb[0].Id);
                postAnonymousObjects.Id.ShouldEqual(_postsInDb[0].Id);

            }
        }


        [Test]
        public void should_return_null_for_post_with_id_non_existent()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                //MongoDB shell: db.posts.find({_id:1234});
                var post = db.GetCollection<Post>().AsQueryable().Where(p => p.Id == 1234).SingleOrDefault();
                post.ShouldBeNull();
            }
        }


        [Test]
        public void should_return_post_body_of_posts_with_votes_count_equals_3()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                // mongoDB: db.posts.find({Statistics.VotesCount: 3}, {Body: 1});
                var postsBodyLinq = db.GetCollection<Post>().AsQueryable().Where(p => p.Statistics.VotesCount == 3).Select(p => p.Body).ToList();
               
                //in memory
                var postBodyFromDB = _postsInDb.Where(p => p.Statistics.VotesCount == 3).Select(p => p.Body);

                postsBodyLinq.ShouldContainOnly(postBodyFromDB);
              

            }
        }

        [Test]
        public void should_return_views_count_of_posts_with_votes_count_equals_3()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                // mongoDB: db.posts.find({Statistics.VotesCount: 3}, {Statistics.ViewsCount: 1});
                var postsBodyLinq = db.GetCollection<Post>().AsQueryable().Where(p => p.Statistics.VotesCount == 3).Select(p => p.Statistics.ViewsCount).ToList();
               
                //in memory
                var postBodyFromDB = _postsInDb.Where(p => p.Statistics.VotesCount == 3).Select(p => p.Statistics.ViewsCount);

          
                postsBodyLinq.ShouldContainOnly(postBodyFromDB);


            }
        }

        [Test]
        public void should_return_List_of_PostViewModels_from_posts_with_votes_count_less_than_3()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                // mongoDB: db.posts.find({'Statistics.VotesCount': 3}, {Body:1,CreationDate:1,'Statistics.VotesCount': 1});

                var postsBodyLinq = db.GetCollection<Post>().AsQueryable()
                                                            .Where(p => p.Statistics.VotesCount < 3)
                                                            .Select(p => new PostViewModel
                                                            {
                                                                Body=p.Body,
                                                                Id=p.Id,
                                                                CreationDate=p.CreationDate,
                                                                VotesCount=p.Statistics.VotesCount
                                                            }).ToList();
               
               //todo:assert
          
             


            }
        }


        [Test]
        public void should_return_every_post_from_posts_collection_ordered_by_title()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell:  db.posts.find({}).sort({Title:1});

                var postsFromLinqProviderQuery = db.GetCollection<Post>().AsQueryable().OrderBy(p => p.Title).ToList();

                var postsAnonymousObjects = db.GetCollection<Post>().Find(new { }, new { Title = OrderBy.Ascending }).ToList();

                Assert.True(_postsInDb.OrderBy(x => x.Title).SequenceEqual(postsFromLinqProviderQuery));
                Assert.True(_postsInDb.OrderBy(x => x.Title).SequenceEqual(postsAnonymousObjects));

            }
        }


        [Test]
        public void should_skip_ten_post_and_return_20_ordered_by_title_from_db()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                //mongodb: db.posts.find().skip(20).limit(10).sort({Title:1});
                var postsFromLinqProviderQuery = db.GetCollection<Post>().AsQueryable().OrderBy(post => post.Title).Skip(10).Take(20).ToList();
                var postsAnonymousObjects = db.GetCollection<Post>().Find(new { }, new { Title = OrderBy.Ascending }, 20, 10).ToList();

                postsFromLinqProviderQuery.Count.ShouldEqual(20);
                postsAnonymousObjects.Count.ShouldEqual(20);
            }
        }



        [Test]
        public void should_return_20_newest_posts()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB: db.posts.find().limit(20).sort({CreationDate:-1});
                var postsFromLinqProviderQuery = db.GetCollection<Post>().AsQueryable().OrderByDescending(post => post.CreationDate).Take(20).ToList();
                var postsAnonymousObjects = db.GetCollection<Post>().Find(new {},new{CreationDate=OrderBy.Descending}, 20,0).ToList();

                postsFromLinqProviderQuery.ShouldContainOnly(_postsInDb.OrderByDescending(x => x.CreationDate).Take(20).ToList());
                postsAnonymousObjects.ShouldContainOnly(_postsInDb.OrderByDescending(x => x.CreationDate).Take(20).ToList());
            }
        }


        [Test]
        public void should_return_all_posts_with_votes_count_greated_than_3()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.find({'Statistics.VotesCount:{ $gt:3'}});

                var postsFromLinqProviderQuery = db.GetCollection<Post>().AsQueryable().Where(post => post.Statistics.VotesCount>3).ToList();

                
                var postsInDbWithVotesCountGreaterThan3 = _postsInDb.Where(post => post.Statistics.VotesCount > 3);

               postsFromLinqProviderQuery.ShouldContainOnly(postsInDbWithVotesCountGreaterThan3);
                
            }
        }


        [Test]
        public void should_return_all_posts_with_votes_count_greated_than_or_equal_3()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.find({'Statistics.VotesCount:{ $gte:3'}});

                var postsFromLinqProviderQuery = db.GetCollection<Post>().AsQueryable().Where(post => post.Statistics.VotesCount >= 3).ToList();


                var postsInDbWithVotesCountGreaterThanOrEqual3 = _postsInDb.Where(post => post.Statistics.VotesCount >= 3);

                postsFromLinqProviderQuery.ShouldContainOnly(postsInDbWithVotesCountGreaterThanOrEqual3);

            }
        }


        [Test]
        public void should_return_all_posts_with_votes_count_less_than_5()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.find({'Statistics.VotesCount:{ $lt:5'}});

                var postsFromLinqProviderQuery = db.GetCollection<Post>().AsQueryable().Where(post => post.Statistics.VotesCount < 5).ToList();


                var postsInDbWithVotesCountLessThan5 = _postsInDb.Where(post => post.Statistics.VotesCount < 5);

                postsFromLinqProviderQuery.ShouldContainOnly(postsInDbWithVotesCountLessThan5);

            }
        }

        [Test]
        public void should_return_all_posts_with_votes_count_less_than_or_equal_5()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.find({'Statistics.VotseCount:{ $lte:5'}});

                var postsFromLinqProviderQuery = db.GetCollection<Post>().AsQueryable().Where(post => post.Statistics.VotesCount <= 5).ToList();


                var postsInDbWithVotesCountLessThanOrEqual5 = _postsInDb.Where(post => post.Statistics.VotesCount <= 5);

                postsFromLinqProviderQuery.ShouldContainOnly(postsInDbWithVotesCountLessThanOrEqual5);

            }
        }

        [Test]
        public void should_return_all_posts_with_votes_count_greater_than_3_and_less_than_or_equal_5()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.find({'Statistics.VotesCount:{$gt:3, $lte:5'}});

                var postsFromLinqProviderQuery = db.GetCollection<Post>().AsQueryable().Where(post =>post.Statistics.VotesCount>3 && post.Statistics.VotesCount <= 5).ToList();


                var postsInRange = _postsInDb.Where((post => post.Statistics.VotesCount > 3 && post.Statistics.VotesCount <= 5));

                postsFromLinqProviderQuery.ShouldContainOnly(postsInRange);

            }
        }

        [Test]
        public void should_return_all_posts_with_viewsCount_not_equal_3()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.find({'Statistics.ViewsCount:{$ne:3}});

                var postsFromLinqProviderQuery = db.GetCollection<Post>().AsQueryable().Where(post => post.Statistics.ViewsCount!=3).ToList();


                var postsInRange = _postsInDb.Where(post => post.Statistics.ViewsCount != 3);

                postsFromLinqProviderQuery.ShouldContainOnly(postsInRange);

            }
        }

        [Test]
        public void should_return_all_posts_with_tags_c_sharp_or_python()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.find({'Tags:{$in:['c#','python']}});

                var postsAnonymous = db.GetCollection<Post>().Find(new {Tags = Q.In("python", "c#")}).ToList();
                //todo: asert
             

        }
        }


        [Test]
        public void should_return_all_posts_with_tags_c_sharp_and_python()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.find({'Tags:{$all:['c#','python']}});
                var tags = new List<string> { "c#", "pyhon" };

                var posts = db.GetCollection<Post>().Find(new { Tags = Q.All("python", "c#") }).ToList();


            }
        }
        [Test]
        public void should_return_all_posts_with_AuthorName_not_equal_Rob_and_not_equal_John()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //todo:sprawdzić
                //MongoDB shell: db.posts.find({'AuthorNaame:{$nin:['Author 1','John']}});

                var posts = db.GetCollection<Post>().Find(new { AuthorName = Q.NotIn("AuthorName1", "John") }).ToList();

                //var authorNames=new List<string>{"AuthorName1","John"};
                //var postsLinq=db.GetCollection<Post>().AsQueryable().Where(post=>)

                var postsInDB = _postsInDb.Where(post => post.AuthorName != "AuthorName1" && post.AuthorName != "John").ToList();
                posts.ShouldContainOnly(postsInDB);
               

            }
        }
        [Test]
        public void should_return_all_posts_where_AuthorId_exists()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
              

                var posts = db.GetCollection<Post>().Find(new { AuthorId = Q.Exists(true) }).ToList();

                posts.ShouldBeEmpty();


            }
        }
        [Test]
        public void should_return_all_posts_where_AuthorName_equals_Author_1_or_tag_equal_c_sharp()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.find({$or:[ { AuthorName : "AuthorName1" } , { Tags : "c#" } ]});
               
                var posts = db.GetCollection<Post>().Find(Q.Or(new {AuthorName="AuthorName1"},new {Tags="c#"})).ToList();
               
                posts.Count.ShouldEqual(30);


            }
        }
        [Test]
        public void should_return_all_posts_where_title_starts_with_title()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

            
                var posts2 =
                    db.GetCollection<Post>().AsQueryable().Where(post => Regex.IsMatch(post.Title, "^Title")).ToList();
               
                posts2.Count.ShouldEqual(30);


            }
        }
        [Test]
        public void should_return_all_posts_with_tag_python()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.find({Tags:"python"});

                var posts = db.GetCollection<Post>().Find(new { Tags = "python" }).ToList();
                var posts2 =
                    db.GetCollection<Post>().AsQueryable().Where(post => post.Tags.Any(x => x == "python")).ToList();
                posts2.ForEach(x=>x.Tags.ShouldContain("python"));
                posts.ForEach(x=>x.Tags.ShouldContain("python"));


            }
        }
        [Test]
        public void should_return_total_count_of_posts_in_db_where_author_name_equals_AuthotName1()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.find({AuthorName:"AuthorName1"}).count();

                var postsCount = db.GetCollection<Post>().Count(new {AuthorName="AuthorName1"});
               
               
                postsCount.ShouldEqual(1);

            }
            

        }
        [Test]
        public void should_return_all_posts_where_comments_body_is_equal_Body_1_and_comments_author_name_is_equal_Author1()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

               

                var posts = db.GetCollection<Post>().Find(new {Comments= Q.ElementMatch(new Comment(){AuthorName = "AuthorName1",Body = "Body1"}) }).ToList();
              

                posts.Count.ShouldEqual(30);


            }
        }
        [Test]
        public void should_return_all_posts_where_second_comment_author_name_equals_Author_2()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.find({'Comments.1.AuthorName':'Author 1'});

                var posts =
                    db.GetCollection<Post>().AsQueryable().Where(x => x.Comments[1].AuthorName == "Author 1").ToList();

                
                posts.ShouldBeEmpty();


            }
        }
        [Test]
        public void should_return_distinct_tags_from_posts_collections()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                //MongoDB shell: db.posts.distict('Tags');

                var tags =
                    db.GetCollection<Post>().Distinct<string>("Tags");

                //todo:assert
                tags.Count().ShouldEqual(3);


            }
            
        }
    }
}
