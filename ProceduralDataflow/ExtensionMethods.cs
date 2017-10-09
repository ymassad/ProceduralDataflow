using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace ProceduralDataflow
{
    public static class ExtensionMethods
    {
        private static CancellationTokenSource canceledCancellationTokenSource;

        static ExtensionMethods()
        {
            canceledCancellationTokenSource = new CancellationTokenSource();
            canceledCancellationTokenSource.Cancel();
        }

        public static AsyncCollection<T>.TakeResult TryTakeImmediatlyOrReturnNull<T>(this AsyncCollection<T> collectionForReentrantItems)
        {
            try
            {
                return collectionForReentrantItems.TryTake(canceledCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
    }
}
