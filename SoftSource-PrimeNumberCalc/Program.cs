namespace DemoNS
{
    class CalcPrimes
    {
        public static ulong highestPrimeFound = 2;
        static ulong currentWorkingNumber = 2;

        public static bool isPrime(ulong n)
        {
            if (n <= 1) return false;
            for (ulong i = 2; i < n; i++)
                if (n % i == 0) return false;
            return true;

        }

        static void Main()
        {
            Console.WriteLine("Press any key to begin.");
            Console.ReadKey();

            var autoEvent = new AutoResetEvent(false);
            var statusChecker = new StatusChecker(60);
            var cancelToken = new CancellationTokenSource();
            CancellationToken ct = cancelToken.Token;

            // Create a timer that invokes CheckStatus after one second, 
            Console.WriteLine("{0:h:mm:ss.fff} Creating timer.\n", DateTime.Now);
            var stateTimer = new Timer(statusChecker.CheckStatus, autoEvent, 1000, 1000);

            var task = Task.Run(() =>
            {
                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    currentWorkingNumber++;
                    if (isPrime(currentWorkingNumber)) highestPrimeFound = currentWorkingNumber;
                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                }
            }, cancelToken.Token);

            // When autoEvent signals, change the period to every half second.
            autoEvent.WaitOne();
            stateTimer.Dispose();
            Console.WriteLine("\nDestroying timer.");
        }
    }

    class StatusChecker
    {
        private int invokeCount;
        private int maxCount;

        public StatusChecker(int count)
        {
            invokeCount = 0;
            maxCount = count;
        }

        // This method is called by the timer delegate.
        public void CheckStatus(Object? stateInfo)
        {
            AutoResetEvent? autoEvent = stateInfo as AutoResetEvent;
            Console.WriteLine($"{DateTime.Now.ToString("h:mm:ss.fff")} -  Count: {++invokeCount} - Highest Prime Found: {CalcPrimes.highestPrimeFound.ToString("N0")}.");

            if (invokeCount == maxCount)
            {
                // Reset the counter and signal the waiting thread.
                invokeCount = 0;
                autoEvent?.Set();
            }
        }
    }
}