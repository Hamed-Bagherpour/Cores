﻿using EasyMicroservices.Database.Interfaces;

namespace EasyMicroservices.Cores.Database.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUniqueIdentityManager
    {
        /// <summary>
        /// 
        /// </summary>
        string StartUniqueIdentity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        long MicroserviceId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string MicroserviceName { get; set; }
        /// <summary>
        /// update unique identity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <returns>is need update database</returns>
        bool UpdateUniqueIdentity<TEntity>(IContext context, TEntity entity);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="uniqueIdentity"></param>
        /// <returns></returns>
        bool IsUniqueIdentityForThisTable<TEntity>(IContext context, string uniqueIdentity);
        /// <summary>
        /// get table first segment item of unique identity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        string GetTableUniqueIdentity<TEntity>(IContext context);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniqueIdentity"></param>
        /// <returns></returns>
        string GetLastTableUniqueIdentity(string uniqueIdentity);
    }
}
