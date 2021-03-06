// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class ExtensionExceptionTests
    {
        [TestMethod]
        [UnitTest]
        public void TestSerialization()
        {
            var info = new ErrorInfo("test_case_error", "This is an error")
            {
                Target = "ToString_Default",
                Details = new[]
                {
                    new ErrorInfo("detail_code", "Detail one")
                    {
                        Details = new[]
                        {
                            new ErrorInfo("nested_datail", "nesting"),
                        },
                        Target = "First",
                        Inner = new TestInnerError("inner_code")
                        {
                            Data = "data item",
                        },
                    },
                    new ErrorInfo("another_detail", "yet again"),
                },
                Inner = new TestInnerError("inner_code_2")
                {
                    Data = "data item",
                    Inner = new TestInnerError("inner_code_3")
                    {
                        Data = "data item",
                    },
                },
            };

            var exception = new ExtensionException(info, "An error", null);

            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(stream, exception);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }

                stream.Seek(0, SeekOrigin.Begin);
                ExtensionException deserialized = null;
                try
                {
                    deserialized = (ExtensionException)formatter.Deserialize(stream);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }

                Assert.AreEqual(exception.ToString(), deserialized.ToString());
            }
        }

        [Serializable]
        private sealed class TestInnerError
            : InnerError
        {
            public TestInnerError(string code)
                : base(code)
            {
            }

            private TestInnerError(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                this.Data = info.GetString(nameof(Data));
            }

            public string Data { get; set; }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);

                info.AddValue(nameof(Data), this.Data);
            }
        }
    }
}
