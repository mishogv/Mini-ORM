namespace MiniORM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;

    internal class ChangeTracker<T>
        where T: class, new()
    {
        private readonly List<T> allEntities;
        private readonly List<T> added;
        private readonly List<T> removed;

        public ChangeTracker(IEnumerable<T> entities)
        {
            this.added = new List<T>();
            this.removed = new List<T>();
            this.allEntities = this.CloneEntities(entities);
        }

        public IReadOnlyCollection<T> AllEntities 
            => this.allEntities.AsReadOnly();

        public IReadOnlyCollection<T> Added 
            => this.added.AsReadOnly();

        public IReadOnlyCollection<T> Removed
            => this.removed.AsReadOnly();

        public void Add(T item) 
            => this.added.Add(item); 
        
        public void Remove(T item) 
            => this.removed.Add(item);

        public IEnumerable<T> GetModifiedEntities(DbSet<T> dbSet)
        {
            var modifiedEnitites =  new List<T>();

            var primaryKeys = typeof(T)
                .GetProperties()
                .Where(x => x.HasAttribute<KeyAttribute>())
                .ToArray();


            foreach (var proxyEntity in this.AllEntities)
            {
                var primaryKeyValues = GetPrimaryKeyValues(primaryKeys, proxyEntity).ToArray();

                var entity = dbSet
                    .Entities
                    .Single(x =>
                    GetPrimaryKeyValues(primaryKeys, x).SequenceEqual(primaryKeyValues));

                if (this.IsModified(proxyEntity, entity))
                {
                    modifiedEnitites.Add(entity);
                }
            }

            return modifiedEnitites;
        }

        private bool IsModified(T entity, T proxyEntity)
        {
            var monitoredProperties = typeof(T)
                .GetProperties()
                .Where(x => DbContext.AllowedSqlTypes.Contains(x.PropertyType));

            var modifiedProperties = monitoredProperties
                .Where(x => !Equals(x.GetValue(entity), x.GetValue(proxyEntity)))
                .ToArray();

            var isModified = modifiedProperties.Any();

            return isModified;
        }

        private static IEnumerable<object> GetPrimaryKeyValues(IEnumerable<PropertyInfo> primaryKeys, T entity)
        {
            return primaryKeys.Select(x => x.GetValue(entity));
        }

        private List<T> CloneEntities(IEnumerable<T> entities)
        {
            var clonedEntities = new List<T>();

            var propertiesToClone = typeof(T)
                .GetProperties()
                .Where(x => DbContext.AllowedSqlTypes.Contains(x.PropertyType))
                .ToArray();

            foreach (var entity in entities)
            {
                var clonedEntity = Activator.CreateInstance<T>();

                foreach (var propertyInfo in propertiesToClone)
                {
                    var value = propertyInfo.GetValue(entity);
                    propertyInfo.SetValue(clonedEntity, value);
                }

                clonedEntities.Add(clonedEntity);
            }

            return clonedEntities;
        }
    }
}