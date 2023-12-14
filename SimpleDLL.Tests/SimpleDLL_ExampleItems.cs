namespace SimpleDLL.Tests
{
    public class SimpleDLL_ExampleItems
    {
        [Fact]
        public void Multiply_PositiveNumbers_ReturnsExpected()
        {
            var m = new ExampleItems();
            int i = 3;
            int f = 5;
            long expected = 15;

            long actual = m.Multiply(i, f);
          
            Assert.Equal(expected, actual);
        }
    }
}