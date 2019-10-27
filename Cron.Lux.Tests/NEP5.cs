using Cron.Lux.Core;
using NUnit.Framework;

namespace Cron.Lux.Tests
{
    [TestFixture]
    public class NEP5Tests
    {
        private CronAPI api;
        private NEP5 token;

        [OneTimeSetUp]
        public void Init()
        {
            api = CronRPC.ForMainNet();
            token = api.GetToken("RPX");
        }

        [Test]
        public void GetSymbol()
        {
            var symbol = token.Symbol;
            Assert.IsTrue(symbol == "RPX");
        }

        [Test]
        public void GetName()
        {
            var name = token.Name;
            Assert.IsTrue(name == "Red Pulse Token");
        }

        [Test]
        public void GetDecimals()
        {
            var decimals = token.Decimals;
            Assert.IsTrue(decimals == 8);
        }

    }
}
