using System;
using System.Collections.Generic;
using System.Text;

namespace ECSCore {
	interface ISystem {
		void OnCreateSystem ();
		void OnDestroySystem ();
		void OnStartSystem ();
		void OnStopSystem ();
	}
}
