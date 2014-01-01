using System;
using System.Collections.Generic;
using System.Text;

namespace fomm.Transactions
{
	/// <summary>
	/// This class is used to communicate 
	/// </summary>
	public class PreparingEnlistment : Enlistment
	{
		internal bool? VoteToCommit { get; set; }

		public override void Done()
		{
			base.Done();
			if (!VoteToCommit.HasValue)
				VoteToCommit = true;
		}

		public void Prepared()
		{
			VoteToCommit = true;
		}

		public void ForceRollback()
		{
			VoteToCommit = false;
		}

		public PreparingEnlistment()
		{
			VoteToCommit = null;
		}
	}
}
