using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorSearch.Scraper
{
    public class Helpers
    {
        /// <summary>
        /// Execute the given action with a specified time limit
        /// </summary>
        /// <param name="timeSpan">Time limit for execution</param>
        /// <param name="codeBlock">Action to execute</param>
        /// <returns></returns>
        public static bool ExecuteWithTimeLimit(TimeSpan timeSpan, Action codeBlock)
        {
            try
            {
                Task task = Task.Factory.StartNew(() => codeBlock());
                task.Wait(timeSpan);
                return task.IsCompleted;
            }
            catch (AggregateException ae)
            {
                throw ae.InnerExceptions[0];
            }
        }
    }
}
