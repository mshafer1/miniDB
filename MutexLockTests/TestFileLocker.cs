using Xunit;

using MutexLocks;
namespace MutexLocksTests
{
    public class TestFileLocker
    {
        private readonly string lock_name = "test.lock";

        [Fact]
        public void TestCanGetLock()
        {
            using (var mut = new FileMutex(this.lock_name))
            {
                mut.Get();
                // inheritently assert a no-throw
            }
        }

        [Fact]
        public void CanNotGetLockTwiceAtOnce()
        {
            using (var mut = new FileMutex(this.lock_name))
            {
                mut.Get();

                var secondMutex = new FileMutex(this.lock_name);
                Assert.Throws<MutexException>(() =>
                {
                    secondMutex.Get();
                });
            }
        }

        [Fact]
        public void CanGetLockAfterUsing()
        {
            using (var mut = new FileMutex(this.lock_name))
            {
                mut.Get();
            }

            var secondMutex = new FileMutex(this.lock_name);
            secondMutex.Get();
            // inheritently assert no-throw
            secondMutex.Dispose();
        }

        [Fact]
        public void TestTwoUsingInARow()
        {
            var mut = new FileMutex(this.lock_name);
            using (var lockItem = mut.Get())
            {
                // unsafe code
            }

            using (var lockItem = mut.Get())
            {
                // unsafe code
            }
        }

        [Fact]
        public void BasicWorkFlowWorks()
        {
            var mut = new FileMutex(this.lock_name);
            using (var lockItem = mut.Get())
            {
                // unsafe code
            }

            using (var lockItem = mut.Get())
            {
                // unsafe code
            }

            var secondMutex = new FileMutex(this.lock_name);
            var lockItem2 = secondMutex.Get();

            Assert.Throws<MutexException>(() =>
            {
                mut.Get();
            });

            lockItem2.Dispose();

            mut.Get();
            // inherently assert a no-throw
        }
    }
}
