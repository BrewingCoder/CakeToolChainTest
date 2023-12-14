namespace SimpleDLL.Nunit.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var m = new ExampleItems();
            int i = 3;
            int f = 5;
            long expected = 15;

            long actual = m.Multiply(i, f);
          
            Assert.That(actual,Is.EqualTo(expected));
        }
    }
}