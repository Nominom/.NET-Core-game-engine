using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Core.Shared
{
	public struct Matrix3x3 {
		public float M11,
			M12,
			M13,
			M21,
			M22,
			M23,
			M31,
			M32,
			M33;
		public Matrix3x3(Matrix4x4 m4x4) {
			this.M11 = m4x4.M11;
			this.M12 = m4x4.M12;
			this.M13 = m4x4.M13;
			this.M21 = m4x4.M21;
			this.M22 = m4x4.M22;
			this.M23 = m4x4.M23;
			this.M31 = m4x4.M31;
			this.M32 = m4x4.M32;
			this.M33 = m4x4.M33;
		}
	}
}
