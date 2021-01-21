namespace Lib9c.Tests.Action
{
    using System;
    using Nekoyume.Action;
    using Xunit;

    public class ByteSerializerTest
    {
        public ByteSerializerTest()
        {
        }

        [Fact]
        public void CreateAvatar2SerializeAndDeserialize()
        {
            var action = new CreateAvatar2()
            {
                index = 2,
                hair = 1,
                ear = 4,
                lens = 5,
                tail = 7,
                name = "test",
            };

            var serialized = ByteSerializer.Serialize(action);
            var deserialized = ByteSerializer.Deserialize<CreateAvatar2>(serialized);
            Assert.Equal(2, deserialized.index);
            Assert.Equal(1, deserialized.hair);
            Assert.Equal(4, deserialized.ear);
            Assert.Equal(5, deserialized.lens);
            Assert.Equal(7, deserialized.tail);
            Assert.Equal("test", deserialized.name);
        }

        [Fact]
        public void RewardGoldSerializeAndDeserialize()
        {
            var action = new RewardGold();
            var serialized = ByteSerializer.Serialize(action);
            ByteSerializer.Deserialize<RewardGold>(serialized);
        }

        [Fact]
        public void ThrowIfTryDeserializeNotAllowedType()
        {
            var irregular = new CustomClass();
            var serialized = ByteSerializer.Serialize(irregular);
            Assert.Throws<ArgumentException>(() =>
            {
                ByteSerializer.Deserialize<CustomClass>(serialized);
            });
        }

        [Serializable]
        public class CustomClass
        {
            public CustomClass()
            {
            }
        }
    }
}
