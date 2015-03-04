// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaaf.Database {
	public static class FSharpHelper {

		public static FSharpOption<T> ToFSharpS<T> (T? data) where T : struct
		{
			if (data.HasValue) {
				return FSharpOption<T>.Some (data.Value);
			} else {
				return FSharpOption<T>.None;
			}
		}

		public static FSharpOption<T> ToFSharp<T> (T data) where T : class
		{
			if (data == null) {
				return FSharpOption<T>.None;
			}
			return FSharpOption<T>.Some (data);
		}

		public static T? FromFSharpS<T> (FSharpOption<T> data) where T : struct
		{
			if (data == FSharpOption<T>.None) {
				return null;
			} else {
				return data.Value;
			}
		}

		public static T FromFSharp<T> (FSharpOption<T> data) where T : class
		{
			return FromFSharp (data, null);
		}
		public static T FromFSharp<T> (FSharpOption<T> data, T def)
		{
			if (data == null) {
				return def;
			}
			return data.Value;
		}
	}
}
