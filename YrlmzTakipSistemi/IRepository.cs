using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace YrlmzTakipSistemi.Repositories
{
    public interface IRepository<T> where T : class
    {
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
        T GetById(int id);
        List<T> GetAll(string filter = null); 
    }
}