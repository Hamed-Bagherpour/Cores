﻿using EasyMicroservices.Cores.Database.Interfaces;
using EasyMicroservices.Cores.Interfaces;
using EasyMicroservices.Database.Interfaces;
using EasyMicroservices.Mapper.Interfaces;
using System;

namespace EasyMicroservices.Cores.AspEntityFrameworkCoreApi.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBaseUnitOfWork : IDisposable
#if (!NETSTANDARD2_0 && !NET45)
        , IAsyncDisposable
#endif
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IDatabase GetDatabase();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IMapperProvider GetMapper();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IUniqueIdentityManager GetUniqueIdentityManager();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IContractLogic<TEntity, TEntity, TEntity, TEntity, long> GetLongLogic<TEntity>()
           where TEntity : class, IIdSchema<long>;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TContract"></typeparam>
        /// <returns></returns>
        IContractLogic<TEntity, TContract, TContract, TContract, long> GetLongContractLogic<TEntity, TContract>()
           where TContract : class
           where TEntity : class, IIdSchema<long>;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IContractLogic<TEntity, TEntity, TEntity, TEntity, long> GetLongReadableLogic<TEntity>()
           where TEntity : class, IIdSchema<long>;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TContract"></typeparam>
        /// <returns></returns>
        IContractLogic<TEntity, TContract, TContract, TContract, long> GetLongReadableContractLogic<TEntity, TContract>()
           where TContract : class
           where TEntity : class, IIdSchema<long>;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        IContractLogic<TEntity, TEntity, TEntity, TEntity, TId> GetLogic<TEntity, TId>()
           where TEntity : class, IIdSchema<TId>;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TContract"></typeparam>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        IContractLogic<TEntity, TContract, TContract, TContract, TId> GetContractLogic<TEntity, TContract, TId>()
           where TContract : class
           where TEntity : class, IIdSchema<TId>;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        IContractLogic<TEntity, TEntity, TEntity, TEntity, TId> GetReadableLogic<TEntity, TId>()
           where TEntity : class, IIdSchema<TId>;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TContract"></typeparam>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        IContractLogic<TEntity, TContract, TContract, TContract, TId> GetReadableContractLogic<TEntity, TContract, TId>()
           where TContract : class
           where TEntity : class, IIdSchema<TId>;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IEasyQueryableAsync<TEntity> GetQueryableOf<TEntity>()
             where TEntity : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IEasyReadableQueryableAsync<TEntity> GetReadableOf<TEntity>()
             where TEntity : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IEasyWritableQueryableAsync<TEntity> GetWritableOf<TEntity>()
             where TEntity : class;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IEasyReadableQueryableAsync<TEntity> GetReadableQueryable<TEntity>()
            where TEntity : class, IIdSchema<long>;
    }
}