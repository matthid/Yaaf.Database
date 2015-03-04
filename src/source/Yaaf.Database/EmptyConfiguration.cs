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

	public class EmptyConfiguration : DbConfiguration {
		public EmptyConfiguration ()
		{
			//SetDefaultConnectionFactory (new LocalDbConnectionFactory ("v11.0"));
		}
	}
}
