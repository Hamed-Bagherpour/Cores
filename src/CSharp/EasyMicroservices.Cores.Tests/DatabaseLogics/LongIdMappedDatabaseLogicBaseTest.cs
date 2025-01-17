﻿using EasyMicroservices.Cores.Contracts.Requests;
using EasyMicroservices.Cores.Database.Interfaces;
using EasyMicroservices.Cores.Database.Logics;
using EasyMicroservices.Cores.Database.Managers;
using EasyMicroservices.Cores.Relational.EntityFrameworkCore.Intrerfaces;
using EasyMicroservices.Cores.Tests.Contracts.Common;
using EasyMicroservices.Cores.Tests.DatabaseLogics.Database.Contexts;
using EasyMicroservices.Cores.Tests.DatabaseLogics.Database.Entities;
using EasyMicroservices.Database.EntityFrameworkCore.Providers;
using EasyMicroservices.Database.Interfaces;
using EasyMicroservices.Mapper.CompileTimeMapper.Providers;
using EasyMicroservices.Mapper.Interfaces;
using EasyMicroservices.Mapper.SerializerMapper.Providers;
using EasyMicroservices.ServiceContracts;
using Microsoft.EntityFrameworkCore;

namespace EasyMicroservices.Cores.Tests.Database
{
    public class DatabaseBuilder : IEntityFrameworkCoreDatabaseBuilder
    {
        public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("Test DB");
        }
    }

    public class LongIdMappedDatabaseLogicBaseTest
    {
        public LongIdMappedDatabaseLogicBaseTest()
        {
        }

        IContractLogic<SubjectEntity, SubjectEntity, SubjectEntity, SubjectEntity, long> GetSubjectContractLogic()
        {
            return new LongIdMappedDatabaseLogicBase<SubjectEntity, SubjectEntity, SubjectEntity, SubjectEntity>(GetDatabase().GetReadableOf<SubjectEntity>(), GetDatabase().GetWritableOf<SubjectEntity>(), GetMapper(), GetUniqueIdentityManager());
        }
        IContractLogic<CategoryEntity, CategoryEntity, CategoryEntity, CategoryEntity, long> GetCategoryContractLogic()
        {
            return new LongIdMappedDatabaseLogicBase<CategoryEntity, CategoryEntity, CategoryEntity, CategoryEntity>(GetDatabase().GetReadableOf<CategoryEntity>(), GetDatabase().GetWritableOf<CategoryEntity>(), GetMapper(), GetUniqueIdentityManager());
        }

        IContractLogic<ProfileEntity, ProfileEntity, ProfileEntity, ProfileEntity, long> GetProfileContractLogic()
        {
            return new LongIdMappedDatabaseLogicBase<ProfileEntity, ProfileEntity, ProfileEntity, ProfileEntity>(GetDatabase().GetReadableOf<ProfileEntity>(), GetDatabase().GetWritableOf<ProfileEntity>(), GetMapper(), GetUniqueIdentityManager());
        }

        IContractLogic<UserEntity, UserEntity, UserEntity, UserEntity, long> GetContractLogic()
        {
            return new LongIdMappedDatabaseLogicBase<UserEntity, UserEntity, UserEntity, UserEntity>(GetDatabase().GetReadableOf<UserEntity>(), GetDatabase().GetWritableOf<UserEntity>(), GetMapper(), GetUniqueIdentityManager());
        }

        IContractLogic<UserEntity, UpdateUserContract, UpdateUserContract, UserEntity, long> GetUpdateContractLogic()
        {
            return new LongIdMappedDatabaseLogicBase<UserEntity, UpdateUserContract, UpdateUserContract, UserEntity>(GetDatabase().GetReadableOf<UserEntity>(), GetDatabase().GetWritableOf<UserEntity>(), GetMapper(), GetUniqueIdentityManager());
        }

        public virtual IDatabase GetDatabase()
        {
            return new EntityFrameworkCoreDatabaseProvider(new MyTestContext(new DatabaseBuilder()));
        }

        const long TableContextId = 150;
        const long ProfileTableContextId = 151;
        const long CategoryTableContextId = 152;
        const long SubjectTableContextId = 153;
        public virtual IUniqueIdentityManager GetUniqueIdentityManager()
        {
            var manager = new DefaultUniqueIdentityManager("1-1", 5, "TestExample");
            manager.InitializeTables(5, manager.GetContextName(typeof(MyTestContext)), manager.GetTableName(typeof(UserEntity)), TableContextId);
            manager.InitializeTables(5, manager.GetContextName(typeof(MyTestContext)), manager.GetTableName(typeof(ProfileEntity)), ProfileTableContextId);
            manager.InitializeTables(5, manager.GetContextName(typeof(MyTestContext)), manager.GetTableName(typeof(CategoryEntity)), CategoryTableContextId);
            manager.InitializeTables(5, manager.GetContextName(typeof(MyTestContext)), manager.GetTableName(typeof(SubjectEntity)), SubjectTableContextId);
            return manager;
        }

        public virtual IMapperProvider GetMapper()
        {
            var mapper = new CompileTimeMapperProvider(new SerializerMapperProvider(new EasyMicroservices.Serialization.Newtonsoft.Json.Providers.NewtonsoftJsonProvider()));
            return mapper;
        }

        [Theory]
        [InlineData("Ali")]
        [InlineData("Reza")]
        [InlineData("Javad")]
        public async Task<UserEntity> AddAsync(string userName)
        {
            await using var logic = GetContractLogic();
            var addUser = new UserEntity()
            {
                UserName = userName
            };
            var user = await logic.Add(addUser);
            Assert.True(user.IsSuccess);
            Assert.True(user.Result > 0);
            var foundUser = await logic.GetById(new GetIdRequestContract<long>()
            {
                Id = user.Result
            });
            Assert.True(foundUser.IsSuccess);
            Assert.True(foundUser.Result.CreationDateTime > DateTime.Now.AddMinutes(-5));
            Assert.Equal(user.Result, foundUser.Result.Id);
            Assert.Equal(addUser.UserName, foundUser.Result.UserName);
            Assert.NotEmpty(foundUser.Result.UniqueIdentity);
            var allUsers = await logic.GetAll();
            Assert.Contains(allUsers.Result, x => x.Id == user.Result);
            var allFilterUsers = await logic.Filter(new FilterRequestContract()
            {
                IsDeleted = false
            });
            Assert.True(allFilterUsers.Result.All(x => allUsers.Result.Any(i => x.Id == i.Id)));
            var ids = DefaultUniqueIdentityManager.DecodeUniqueIdentity(foundUser.Result.UniqueIdentity);
            Assert.Equal(ids.Last(), foundUser.Result.Id);
            Assert.Equal(TableContextId, ids[^2]);
            return foundUser.Result;
        }

        [Theory]
        [InlineData("Ali")]
        [InlineData("Reza")]
        [InlineData("Javad")]
        public async Task AddBulkAsync(string userName)
        {
            await using var logic = GetContractLogic();
            List<UserEntity> items = new List<UserEntity>();
            for (int i = 0; i < 10; i++)
            {
                items.Add(new UserEntity()
                {
                    UserName = userName + Guid.NewGuid()
                });
            }
            var user = await logic.AddBulk(items);
            Assert.True(user.IsSuccess);
            foreach (var item in items)
            {
                var foundUser = await logic.GetBy(x => x.UserName == item.UserName);
                Assert.True(foundUser.IsSuccess);
                Assert.True(foundUser.Result.CreationDateTime > DateTime.Now.AddMinutes(-5));
                Assert.True(items.Any(x => x.UserName == item.UserName));
            }
        }

        public async Task<ProfileEntity> AddProfileAsync(long userId, string name)
        {
            await using var logic = GetContractLogic();
            var user = await logic.GetById(userId);
            Assert.True(user.IsSuccess);
            await using var profileLogic = GetProfileContractLogic();
            var addedProfile = await profileLogic.AddEntity(new ProfileEntity()
            {
                FirstName = name,
                LastName = "Yousefi",
                UniqueIdentity = user.Result.UniqueIdentity,
                UserId = userId
            });
            return addedProfile.Result;
        }

        [Theory]
        [InlineData("Mahdi", "Mahdi1")]
        [InlineData("Hassan", "Hassan1")]
        public async Task UpdateAsync(string userName, string toUserName)
        {
            await using var logic = GetUpdateContractLogic();
            var added = await AddAsync(userName);
            added.UserName = toUserName;
            var updateResult = await logic.Update(new UpdateUserContract()
            {
                Id = added.Id,
                UniqueIdentity = added.UniqueIdentity,
                UserName = added.UserName
            });
            Assert.NotNull(updateResult.Result.ModificationDateTime);
            Assert.Equal(updateResult.Result.CreationDateTime, added.CreationDateTime);
            Assert.True(updateResult.Result.CreationDateTime > DateTime.Now.AddMinutes(-5));
            Assert.True(updateResult.Result.ModificationDateTime > DateTime.Now.AddMinutes(-5));
            var found = await logic.GetById(new GetIdRequestContract<long>()
            {
                Id = added.Id
            });
            Assert.NotNull(found.Result.ModificationDateTime);
            Assert.Equal(found.Result.CreationDateTime, added.CreationDateTime);
            Assert.True(found.Result.CreationDateTime > DateTime.Now.AddMinutes(-5));
            Assert.True(found.Result.ModificationDateTime > DateTime.Now.AddMinutes(-5));
            Assert.Equal(found.Result.UserName, toUserName);
        }

        [Theory]
        [InlineData("Mahdi", "Mahdi1")]
        [InlineData("Hassan", "Hassan1")]
        public async Task UpdateBulkAsync(string userName, string toUserName)
        {
            await using var logic = GetContractLogic();
            List<UserEntity> items = new List<UserEntity>();
            for (int i = 0; i < 10; i++)
            {
                items.Add(new UserEntity()
                {
                    UserName = userName + Guid.NewGuid()
                });
            }
            var user = await logic.AddBulk(items);
            Assert.True(user.IsSuccess);
            List<UserEntity> updateItems = new List<UserEntity>();
            foreach (var item in items)
            {
                var foundUser = await logic.GetBy(x => x.UserName == item.UserName);
                Assert.True(foundUser.IsSuccess);
                Assert.True(foundUser.Result.CreationDateTime > DateTime.Now.AddMinutes(-5));
                Assert.True(items.Any(x => x.UserName == item.UserName));
                updateItems.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<UserEntity>(Newtonsoft.Json.JsonConvert.SerializeObject(foundUser.Result)));
            }
            foreach (var item in updateItems)
            {
                item.UserName += "Updated";
            }
            await using var logic2 = GetContractLogic();
            var updateResult = await logic2.UpdateBulk(updateItems);
            foreach (var item in updateItems)
            {
                var found = await logic2.GetBy(x => x.UserName == item.UserName);
                Assert.True(found);
                Assert.NotNull(found.Result.ModificationDateTime);
                Assert.Equal(found.Result.CreationDateTime, item.CreationDateTime);
                Assert.True(found.Result.CreationDateTime > DateTime.Now.AddMinutes(-5));
                Assert.True(found.Result.ModificationDateTime > DateTime.Now.AddMinutes(-5));
                var find = items.FirstOrDefault(x => x.UserName == found.Result.UserName.Replace("Updated", ""));
                Assert.Equal(found.Result.UserName, find.UserName + "Updated");
            }
        }

        [Theory]
        [InlineData("Ahmad")]
        [InlineData("Yasin")]
        public async Task UniqueIdentityAsync(string userName)
        {
            await using var logic = GetContractLogic();
            var added = await AddAsync(userName);
            var found = await logic.GetByUniqueIdentity(new GetUniqueIdentityRequestContract()
            {
                UniqueIdentity = added.UniqueIdentity
            });
            Assert.Equal(found.Result.UserName, userName);

            var foundAll = await logic.GetAllByUniqueIdentity(new GetUniqueIdentityRequestContract()
            {
                UniqueIdentity = DefaultUniqueIdentityManager.CutUniqueIdentityFromEnd(added.UniqueIdentity, 2)
            });
            Assert.Contains(foundAll.Result, x => x.UserName == userName);

            for (int i = 0; i < 10; i++)
            {
                var addedProfile = await AddProfileAsync(added.Id, $"Ali{i}");
            }
            await using var profileLogic = GetProfileContractLogic();
            var foundAllProfiles = await profileLogic.GetAllByUniqueIdentity(new GetUniqueIdentityRequestContract()
            {
                UniqueIdentity = added.UniqueIdentity
            });
            Assert.Contains(foundAllProfiles.Result, x => x.FirstName.StartsWith("Ali"));
            Assert.Equal(foundAllProfiles.Result.Count, 10);

            var onlyUniqueIdentity = await profileLogic.GetByUniqueIdentity(new GetUniqueIdentityRequestContract()
            {
                UniqueIdentity = added.UniqueIdentity
            }, default, q => q.Where(x => x.FirstName == "Ali5"));
            Assert.Contains(foundAllProfiles.Result, x => x.FirstName == "Ali5");
        }

        [Theory]
        [InlineData("Hossein")]
        [InlineData("HosseinA")]
        [InlineData("HosseinB")]
        [InlineData("HosseinC")]
        public async Task HardDeleteAsync(string userName)
        {
            await using var logic = GetContractLogic();
            var added = await AddAsync(userName);
            var found = await logic.GetById(new GetIdRequestContract<long>()
            {
                Id = added.Id
            });
            Assert.Equal(found.Result.Id, added.Id);
            var deleted = await logic.HardDeleteById(new DeleteRequestContract<long>()
            {
                Id = found.Result.Id
            });
            Assert.True(deleted);
            found = await logic.GetById(new GetIdRequestContract<long>()
            {
                Id = added.Id
            });
            Assert.Equal(FailedReasonType.NotFound, found.Error.FailedReasonType);
        }

        [Theory]
        [InlineData("Ali", new string[] { "Hossein", "HosseinA", "HosseinB", "HosseinC" })]
        public async Task HardDeleteBulkAsync(string name, string[] userNames)
        {
            await using var logic = GetContractLogic();
            List<long> ids = new List<long>();
            foreach (var item in userNames)
            {
                UserEntity added = await AddAsync(item);
                var found = await logic.GetById(new GetIdRequestContract<long>()
                {
                    Id = added.Id
                });
                ids.Add(added.Id);
                Assert.Equal(found.Result.Id, added.Id);
            }
            var deleted = await logic.HardDeleteBulkByIds(new DeleteBulkRequestContract<long>()
            {
                Ids = ids
            });
            Assert.True(deleted);
            foreach (var item in ids)
            {
                var found = await logic.GetById(new GetIdRequestContract<long>()
                {
                    Id = item
                });
                Assert.Equal(FailedReasonType.NotFound, found.Error.FailedReasonType);
            }
        }

        [Theory]
        [InlineData("HosseinSoftDelete")]
        [InlineData("RezaSoftDelete")]
        public async Task SoftDeleteAsync(string userName)
        {
            await using var logic = GetContractLogic();
            var added = await AddAsync(userName);
            var found = await logic.GetById(new GetIdRequestContract<long>()
            {
                Id = added.Id
            });
            Assert.Equal(found.Result.Id, added.Id);
            var deleted = await logic.SoftDeleteById(new SoftDeleteRequestContract<long>
            {
                Id = found.Result.Id,
                IsDelete = true
            });
            Assert.True(deleted);
            found = await logic.GetById(new GetIdRequestContract<long>()
            {
                Id = added.Id
            });
            Assert.Equal(FailedReasonType.NotFound, found.Error.FailedReasonType);
        }

        [Theory]
        [InlineData("Ali", new string[] { "Hossein", "HosseinA", "HosseinB", "HosseinC" })]
        public async Task SoftDeleteBulkByIdsAsync(string name, string[] userNames)
        {
            await using var logic = GetContractLogic();
            List<long> ids = new List<long>();
            foreach (var userName in userNames)
            {
                var added = await AddAsync(userName);
                var found = await logic.GetById(new GetIdRequestContract<long>()
                {
                    Id = added.Id
                });
                ids.Add(added.Id);
                Assert.Equal(found.Result.Id, added.Id);
            }

            var deleted = await logic.SoftDeleteBulkByIds(new SoftDeleteBulkRequestContract<long>
            {
                Ids = ids,
                IsDelete = true
            });
            Assert.True(deleted);
            foreach (var item in ids)
            {
                var found = await logic.GetById(new GetIdRequestContract<long>()
                {
                    Id = item
                });
                Assert.False(found.IsSuccess);
                Assert.Equal(FailedReasonType.NotFound, found.Error.FailedReasonType);
            }
        }

        [Theory]
        [InlineData("Ali", new string[] { "reza", "javad", "hassan" })]
        public async Task SelfChildTestAsync(string parentName, string[] chilren)
        {
            await using var logic = GetCategoryContractLogic();
            var addUser = new CategoryEntity()
            {
                Name = parentName,
                UniqueIdentity = "A-B"
            };
            var user = await logic.Add(addUser);
            Assert.True(user.IsSuccess);
            Assert.True(user.Result > 0);
            var foundUser = await logic.GetById(new GetIdRequestContract<long>()
            {
                Id = user.Result
            });
            foreach (var item in chilren)
            {
                var chidlAddUser = new CategoryEntity()
                {
                    Name = item,
                    UniqueIdentity = foundUser.Result.UniqueIdentity
                };
                var childUser = await logic.Add(chidlAddUser);
                Assert.True(childUser.IsSuccess);
                Assert.True(childUser.Result > 0);
            }

            var allResult = await logic.GetAllByUniqueIdentity(foundUser.Result);
            Assert.True(allResult.HasItems);
            Assert.True(chilren.All(x => allResult.Result.Any(y => y.Name == x)));
            Assert.True(allResult.Result.Any(y => y.Name == parentName));

            var onlyChildrenResult = await logic.GetAllByUniqueIdentity(foundUser.Result, type: DataTypes.GetUniqueIdentityType.OnlyChilren);
            Assert.True(onlyChildrenResult.HasItems);
            Assert.True(chilren.All(x => onlyChildrenResult.Result.Any(y => y.Name == x)));
            Assert.True(!onlyChildrenResult.Result.Any(y => y.Name == parentName));

            var onlyParentResult = await logic.GetAllByUniqueIdentity(foundUser.Result, type: DataTypes.GetUniqueIdentityType.OnlyParent);
            Assert.True(onlyParentResult.HasItems);
            Assert.True(chilren.All(x => !onlyParentResult.Result.Any(y => y.Name == x)));
            Assert.True(onlyParentResult.Result.Any(y => y.Name == parentName));
        }

        [Theory]
        [InlineData("Ali", new string[] { "reza", "javad", "hassan" })]
        public async Task ChildTestAsync(string parentName, string[] chilren)
        {
            await using var logic = GetCategoryContractLogic();
            await using var subjectlogic = GetSubjectContractLogic();
            var addUser = new CategoryEntity()
            {
                Name = parentName,
                UniqueIdentity = "A-B"
            };
            var user = await logic.Add(addUser);
            Assert.True(user.IsSuccess);
            Assert.True(user.Result > 0);
            var foundUser = await logic.GetById(new GetIdRequestContract<long>()
            {
                Id = user.Result
            });
            foreach (var item in chilren)
            {
                var chidlAddUser = new CategoryEntity()
                {
                    Name = item,
                    UniqueIdentity = foundUser.Result.UniqueIdentity
                };
                var childUser = await logic.Add(chidlAddUser);
                Assert.True(childUser.IsSuccess);
                Assert.True(childUser.Result > 0);
            }

            var allResult = await logic.GetAllByUniqueIdentity(foundUser.Result);
            Assert.True(allResult.HasItems);
            Assert.True(chilren.All(x => allResult.Result.Any(y => y.Name == x)));
            Assert.True(allResult.Result.Any(y => y.Name == parentName));

            foreach (var item in allResult.Result)
            {
                var chidlAddUser = new SubjectEntity()
                {
                    Name = "Subcject" + item.Name,
                    UniqueIdentity = item.UniqueIdentity
                };
                var childUser = await subjectlogic.Add(chidlAddUser);
                Assert.True(childUser.IsSuccess);
                Assert.True(childUser.Result > 0);
            }
            var onlyChildrenResult = await subjectlogic.GetAllByUniqueIdentity(foundUser.Result, type: DataTypes.GetUniqueIdentityType.OnlyChilren);
            Assert.True(onlyChildrenResult.HasItems);
            Assert.True(chilren.All(x => onlyChildrenResult.Result.Any(y => y.Name == "Subcject" + x)));
            Assert.True(!onlyChildrenResult.Result.Any(y => y.Name == "Subcject" + parentName));

            var onlyParentResult = await subjectlogic.GetAllByUniqueIdentity(foundUser.Result, type: DataTypes.GetUniqueIdentityType.OnlyParent);
            Assert.True(onlyParentResult.HasItems);
            Assert.True(chilren.All(x => !onlyParentResult.Result.Any(y => y.Name == "Subcject" + x)));
            Assert.True(onlyParentResult.Result.Any(y => y.Name == "Subcject" + parentName));
        }
    }
}
