using System;

namespace kOS.Safe.Test
{
    public class TestLogger : ILogger
    {
        public void Log(string text)
        {
            Console.WriteLine(text);
        }

        public void Log(Exception e)
        {
            throw new NotImplementedException();
        }

        public void SuperVerbose(string s)
        {
            Console.WriteLine(s);
        }

        public void LogWarning(string s)
        {
            throw new NotImplementedException();
        }

        public void LogException(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void LogError(string s)
        {
            throw new NotImplementedException();
        }
    }
}
