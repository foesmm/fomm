using System;
using System.Collections.Generic;
using System.Text;

namespace fomm.Transactions
{
	public class Enlistment
	{
		internal bool DoneProcessing { get; set; }

		public virtual void Done()
		{
			DoneProcessing = true;
		}

		public Enlistment()
		{
			DoneProcessing = false;
		}
	}
}
