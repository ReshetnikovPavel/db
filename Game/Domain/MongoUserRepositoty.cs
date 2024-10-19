using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);
            var options = new CreateIndexOptions { Unique = true };
            var index = Builders<UserEntity>.IndexKeys.Ascending(x => x.Login);
            userCollection.Indexes.CreateOne(new CreateIndexModel<UserEntity>(index, options));
        }

        public UserEntity Insert(UserEntity user)
        {
            userCollection.InsertOne(user);
            return user;
        }

        public UserEntity FindById(Guid id)
        {
            var filter = Builders<UserEntity>.Filter.Eq(x => x.Id, id);
            return userCollection.Find(filter).FirstOrDefault();
        }

        public UserEntity GetOrCreateByLogin(string login)
        {
            var filter = Builders<UserEntity>.Filter.Eq(x => x.Login, login);
            var user = userCollection.Find(filter).FirstOrDefault();
            if (user is default(UserEntity))
                return Insert(new UserEntity { Login = login });
            return user;
        }

        public void Update(UserEntity user)
        {
            var filter = Builders<UserEntity>.Filter.Eq(x => x.Id, user.Id);
            userCollection.ReplaceOne(filter, user);
        }

        public void Delete(Guid id)
        {
            var filter = Builders<UserEntity>.Filter.Eq(x => x.Id, id);
            userCollection.DeleteOne(filter);
        }

        // Для вывода списка всех пользователей (упорядоченных по логину)
        // страницы нумеруются с единицы
        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            var totalCount = userCollection.CountDocuments(new BsonDocument());
            var users = userCollection.Find(new BsonDocument())
                .SortBy(x => x.Login)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToList();
            return new PageList<UserEntity>(users, totalCount, pageNumber, pageSize);
        }

        // Не нужно реализовывать этот метод
        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }
    }
}
