using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Telerik.JustMock.EntityFramework
{
	/// <summary>
	/// A user-provided delegate that can return the value of the primary key of the entity.
	/// The return value should be either:
	/// * the value of the primary key if is non-composite
	/// * an IEnumerable with the values of the key columns, if the key is composite
	/// </summary>
	public delegate object GetIdFunction<TEntity>(TEntity entity) where TEntity : class;

	/// <summary>
	/// An in-memory mock DbSet. DbContext instances created by <see cref="EntityFrameworkMock.Create"/> are
	/// initialized with instances of this class.
	/// </summary>
	/// <typeparam name="TEntity">The entity type.</typeparam>
	public class MockDbSet<TEntity> : DbSet<TEntity>, IDbAsyncEnumerable<TEntity>, IQueryable<TEntity> where TEntity : class
	{
		private ICollection<TEntity> data;
		private IQueryable<TEntity> asQueryable;

		/// <summary>
		/// The backing collection for this DbSet. All operations on the DbSet are made
		/// against this collection. The collection should be an instance of
		/// ObservableCollection&lt;T&gt; for the <see cref="Local"/> property to work.
		/// </summary>
		public ICollection<TEntity> Data
		{
			get { return this.data; }
			set
			{
				this.data = value;
				this.asQueryable = value.AsQueryable();
			}
		}

		/// <summary>
		/// A user-provided delegate that can return the value of the primary key of the entity.
		/// 
		/// This delegate is called by the <see cref="Find"/> method. If the entity's primary key property
		/// is called "Id" or "%Entity%Id" (where %Entity% is the name of the entity class), then a
		/// function is generated automatically to return the value of that property.
		/// 
		/// The return value should be either:
		/// * the value of the primary key if is non-composite
		/// * an IEnumerable with the values of the key columns, if the key is composite
		/// </summary>
		public GetIdFunction<TEntity> GetIdFunction { get; set; }

		/// <summary>
		/// Creates a DbSet with an empty ObservableCollection for its backing store.
		/// </summary>
		public MockDbSet()
		{
			this.Data = new ObservableCollection<TEntity>();
		}

		private void InitializeDefaultGetIdFunction()
		{
			var idProp = typeof(TEntity).GetProperty("Id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
				?? typeof(TEntity).GetProperty(typeof(TEntity).Name + "Id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

			if (idProp == null)
			{
				throw new InvalidOperationException("Couldn't automatically determine the entity key. Specify a 'get ID' function before using Find() using SetIdFunction() extension method.");
			}

			var entityParam = Expression.Parameter(typeof(TEntity));
			var getIdLambda = Expression.Lambda(typeof(GetIdFunction<TEntity>),
				Expression.Convert(Expression.MakeMemberAccess(entityParam, idProp), typeof(object)),
				entityParam);
			this.GetIdFunction = (GetIdFunction<TEntity>)getIdLambda.Compile();
		}

		public override TEntity Add(TEntity entity)
		{
			this.Data.Add(entity);
			return entity;
		}

		public override IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
		{
			foreach (var entity in entities)
			{
				this.Data.Add(entity);
			}
			return entities;
		}

		public override TEntity Attach(TEntity entity)
		{
			this.Data.Add(entity);
			return entity;
		}

		public override TDerivedEntity Create<TDerivedEntity>()
		{
			return Activator.CreateInstance<TDerivedEntity>();
		}

		public override TEntity Create()
		{
			return Activator.CreateInstance<TEntity>();
		}

		public override TEntity Find(params object[] keyValues)
		{
			if (this.GetIdFunction == null)
			{
				InitializeDefaultGetIdFunction();
			}

			foreach (var entity in this.Data)
			{
				var keys = GetIdFunction(entity);
				var keyCollection = keys as ICollection;
				if (keyCollection != null)
				{
					if (keyValues.Length != keyCollection.Count)
					{
						throw new InvalidOperationException("Number of keys passed to Find() is not equal to the number of keys on the entity.");
					}
					if (keyValues.SequenceEqual(keyCollection.Cast<object>()))
					{
						return entity;
					}
				}
				else
				{
					if (keyValues.Length != 1)
					{
						throw new InvalidOperationException("Number of keys passed to Find() is not equal to the number of keys on the entity.");
					}
					if (Object.Equals(keyValues[0], keys))
					{
						return entity;
					}
				}
			}

			return null;
		}

		public override TEntity Remove(TEntity entity)
		{
			this.Data.Remove(entity);
			return entity;
		}

		public override IEnumerable<TEntity> RemoveRange(IEnumerable<TEntity> entities)
		{
			foreach (var entity in entities)
			{
				this.Data.Remove(entity);
			}
			return entities;
		}

		/// <summary>
		/// Returns the backing collection if it is an ObservableCollection,
		/// otherwise returns a copy of it.
		/// </summary>
		public override ObservableCollection<TEntity> Local
		{
			get
			{
				return this.data as ObservableCollection<TEntity>
					?? new ObservableCollection<TEntity>(this.data);
			}
		}

		public virtual IDbAsyncEnumerator<TEntity> GetAsyncEnumerator()
		{
			return new TestDbAsyncEnumerator<TEntity>(this.data.GetEnumerator());
		}

		public virtual Type ElementType
		{
			get { return this.asQueryable.ElementType; }
		}

		public virtual Expression Expression
		{
			get { return this.asQueryable.Expression; }
		}

		public virtual IQueryProvider Provider
		{
			get { return new TestDbAsyncQueryProvider<TEntity>(this.asQueryable.Provider); }
		}

		public virtual IEnumerator<TEntity> GetEnumerator()
		{
			return this.data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.data.GetEnumerator();
		}
	}
}
