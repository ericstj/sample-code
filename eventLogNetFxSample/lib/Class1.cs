using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

namespace lib
{
    public static class EventLogHelper
    {
       public static IEnumerable<string> GetEventUsers()
       {
            using (var eventLog = new EventLogReader("Application"))
            {
                while(true)
                {
                    using (EventRecord record = eventLog.ReadEvent())
                    {
                        if (record == null)
                            break;

                        yield return record.UserId?.ToString();
                    }
                }
            }

       }
    }
}
