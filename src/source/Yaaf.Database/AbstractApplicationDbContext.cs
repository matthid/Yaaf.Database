// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yaaf.Database {
	public abstract class AbstractApplicationDbContext : DbContext {
		static AbstractApplicationDbContext ()
		{
			if (String.IsNullOrWhiteSpace (AppDomain.CurrentDomain.GetData ("DataDirectory") as string)) {
				System.AppDomain.CurrentDomain.SetData (
					"DataDirectory",
					System.AppDomain.CurrentDomain.BaseDirectory);
			}
		}

		protected virtual void Init ()
		{
		}

		private void MyInit ()
		{
			Init ();
			Database.Initialize (false);
		}

		public AbstractApplicationDbContext ()
			: base ()
		{
			MyInit ();
		}

		public AbstractApplicationDbContext (string nameOrConnection)
			: base (nameOrConnection)
		{
			MyInit ();
		}

		public Task MySaveChanges ()
		{
			return MySaveChanges (this);
		}
		

		public static async Task MySaveChanges (DbContext context)
		{
			bool saved = false;
			do {
				DbUpdateConcurrencyException concurrentError = null;
				try {
					await context.SaveChangesAsync ();
					saved = true;
				} catch (DbUpdateConcurrencyException e) {
					concurrentError = e;
				} catch (DbUpdateException e) {
					// Log error?
					throw;
				}
				if (concurrentError != null) {
					var e = concurrentError;
					Console.Error.WriteLine ("DbUpdateConcurrencyException: {0}", e);
					foreach (var entry in e.Entries) {
						switch (entry.State) {
						case EntityState.Deleted:
							//deleted on client, modified in store
							await entry.ReloadAsync ();
							entry.State = EntityState.Deleted;
							break;
						case EntityState.Modified:
							DbPropertyValues currentValues = entry.CurrentValues.Clone ();
							await entry.ReloadAsync ();
							if (entry.State == EntityState.Detached)
								//Modified on client, deleted in store
								context.Set (ObjectContext.GetObjectType (entry.Entity.GetType ())).Add (entry.Entity);
							else
							//Modified on client, Modified in store
                                {
								await entry.ReloadAsync ();
								entry.CurrentValues.SetValues (currentValues);
							}
							break;
						default:
							//For good luck
							await entry.ReloadAsync ();
							break;
						}
					}
				}

			} while (!saved);
		}
	}
}
