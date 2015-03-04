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
using System.Threading.Tasks;

namespace Yaaf.Database {
	public abstract class AbstractApplicationIdentityDbContext<TUser> : IdentityDbContext<TUser> where TUser : IdentityUser {
		static AbstractApplicationIdentityDbContext ()
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

		public AbstractApplicationIdentityDbContext ()
			: base ()
		{
			MyInit ();
		}

		public AbstractApplicationIdentityDbContext (string nameOrConnection)
			: base (nameOrConnection)
		{
			MyInit ();
		}

		public Task MySaveChanges ()
		{
			return AbstractApplicationDbContext.MySaveChanges (this);
		}
	}
}
