// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaaf.Database {

	public class MyDropCreateDatabaseIfModelChanges<TContext> : DropCreateDatabaseIfModelChanges<TContext> where TContext : DbContext {
		private Action<TContext> seedMethod;
		public MyDropCreateDatabaseIfModelChanges (Action<TContext> seedMethod)
		{
			this.seedMethod = seedMethod;
		}
		protected override void Seed (TContext context)
		{
			base.Seed (context);
			if (seedMethod != null) {
				seedMethod (context);
			}
		}
	}
	public class MyCreateDatabaseIfNotExists<TContext> : CreateDatabaseIfNotExists<TContext> where TContext : DbContext {
		private Action<TContext> seedMethod;
		public MyCreateDatabaseIfNotExists (Action<TContext> seedMethod)
		{
			this.seedMethod = seedMethod;
		}
		protected override void Seed (TContext context)
		{
			base.Seed (context);
			if (seedMethod != null) {
				seedMethod (context);
			}
		}
	}


	public class NUnitInitializer<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext {

		private DropCreateDatabaseIfModelChanges<TContext> changer;
		private CreateDatabaseIfNotExists<TContext> creator;
		public NUnitInitializer ()
		{
			changer = new MyDropCreateDatabaseIfModelChanges<TContext> (this.Seed);
			creator = new MyCreateDatabaseIfNotExists<TContext> (this.Seed);
		}

		protected virtual void Seed (TContext context)
		{
		}

		public void InitializeDatabase (TContext context)
		{
			try {
				changer.InitializeDatabase (context);
			} catch (NotSupportedException) {
				// Database not existent
				creator.InitializeDatabase (context);
			}
		}
	}

}
