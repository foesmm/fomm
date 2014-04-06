using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fomm.Updater
{
  public interface IReleaseProvider
  {
    bool GetLatest(ref Release release);
  }
}
