using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace SpaceEngineersPrograms
{
    internal class UnitTestsProgram : MyGridProgram
    {
        private delegate void TestDelegate();

        private class AssertionException : Exception
        {
            public AssertionException() : base()
            {
                // ...
            }

            public AssertionException(string message) : base(message)
            {
                // ...
            }

            public AssertionException(string message, Exception innerException) : base(message, innerException)
            {
                // ...
            }
        }

        private static class Assert
        {
            public static void IsTrue(bool state)
            {
                if (!state)
                {
                    throw new AssertionException(nameof(state) + " did not return true.");
                }
            }

            public static void IsFalse(bool state)
            {
                if (state)
                {
                    throw new AssertionException(nameof(state) + " did not return false.");
                }
            }

            public static void Equals<T>(T left, T right) where T : IEquatable<T>
            {
                if (!(left.Equals(right)))
                {
                    throw new AssertionException(nameof(left) + " and " + nameof(right) + " are not equal.");
                }
            }

            public static void NotEquals<T>(T left, T right) where T : IEquatable<T>
            {
                if (left.Equals(right))
                {
                    throw new AssertionException(nameof(left) + " and " + nameof(right) + " are equal.");
                }
            }

            public static void IsNull(object obj)
            {
                if (obj != null)
                {
                    throw new AssertionException(nameof(obj) + " is not null.");
                }
            }

            public static void IsNotNull(object obj)
            {
                if (obj == null)
                {
                    throw new AssertionException(nameof(obj) + " is null.");
                }
            }

            public static void AreArraysEqual<T>(IReadOnlyList<T> left, IReadOnlyList<T> right) where T : IEquatable<T>
            {
                IsNotNull(left);
                IsNotNull(right);
                Equals(left.Count, right.Count);
                for (int i = 0; i < left.Count; i++)
                {
                    Equals(left[i], right[i]);
                }
            }
        }

        private static void TestArgumentStrings()
        {
            ArgumentString argument_1_string = "argument 1";
            ArgumentString argument_2_string = ArgumentString.Required("argument 2");
            ArgumentString argument_3_string = ArgumentString.Optional("argument 3");
            Assert.IsFalse(argument_1_string.IsOptional);
            Assert.Equals(argument_1_string.Argument, "argument 1");
            Assert.IsFalse(argument_2_string.IsOptional);
            Assert.Equals(argument_2_string.Argument, "argument 2");
            Assert.IsTrue(argument_3_string.IsOptional);
            Assert.Equals(argument_3_string.Argument, "argument 3");
        }

        private static void TestStorageDataSerializer()
        {
            string data_string = "0123456789ABCDEF";
            byte[] data_bytes = new byte[] { 0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE };
            Assert.IsTrue(StorageDataSerializer.ValidateHexString(data_string));
            StorageDataSerializer storage_data_serializer_1 = new StorageDataSerializer(data_string);
            StorageDataSerializer storage_data_serializer_2 = new StorageDataSerializer(data_bytes);
            Assert.Equals(storage_data_serializer_1.StorageString, storage_data_serializer_2.StorageString);
            Assert.AreArraysEqual(storage_data_serializer_1.ReadBytes(8), storage_data_serializer_2.ReadBytes(8));
        }

        private static void TestCommands()
        {
            Commands<bool> bool_commands = new Commands<bool>();
            Commands<int> int_commands = new Commands<int>('|', 42);
            Assert.IsTrue(bool_commands.Add("command_1", (arguments) => bool.Parse(arguments[0])));
            Assert.IsTrue(bool_commands.Add("command_2", (arguments) => (bool.Parse(arguments[0]) || bool.Parse(arguments[1]))));
            Assert.IsTrue(bool_commands.Add("command_3", (arguments) => (bool.Parse(arguments[0]) || bool.Parse(arguments[1]) || bool.Parse(arguments[2]))));
            Assert.IsTrue(int_commands.Add("command_1", (arguments) => int.Parse(arguments[0])));
            Assert.IsTrue(int_commands.Add("command_2", (arguments) => (int.Parse(arguments[0]) + int.Parse(arguments[1]))));
            Assert.IsTrue(int_commands.Add("command_3", (arguments) => (int.Parse(arguments[0]) + int.Parse(arguments[1]) + int.Parse(arguments[2]))));
            Assert.Equals(bool_commands.Delimiter, ' ');
            Assert.Equals(int_commands.Delimiter, '|');
            Assert.IsFalse(bool_commands.DefaultParseReturnValue);
            Assert.Equals(int_commands.DefaultParseReturnValue, 42);
            for (int i = 1; i < 4; i++)
            {
                Assert.IsTrue(bool_commands.AddAliases("command_" + i, "cmd_" + i, "c" + i));
                Assert.IsTrue(int_commands.AddAliases("command_" + i, "cmd_" + i, "c" + i));
                Assert.IsTrue(bool_commands.CommandLookup.ContainsKey("command_" + i));
                Assert.IsTrue(bool_commands.CommandLookup.ContainsKey("cmd_" + i));
                Assert.IsTrue(bool_commands.CommandLookup.ContainsKey("c" + i));
                Assert.IsTrue(int_commands.CommandLookup.ContainsKey("command_" + i));
                Assert.IsTrue(int_commands.CommandLookup.ContainsKey("cmd_" + i));
                Assert.IsTrue(int_commands.CommandLookup.ContainsKey("c" + i));
            }
            Assert.IsFalse(bool_commands.Parse("command_1 false"));
            Assert.IsTrue(bool_commands.Parse("command_1 true"));

            Assert.IsFalse(bool_commands.Parse("cmd_1 false"));
            Assert.IsTrue(bool_commands.Parse("cmd_1 true"));

            Assert.IsFalse(bool_commands.Parse("c1 false"));
            Assert.IsTrue(bool_commands.Parse("c1 true"));

            Assert.IsFalse(bool_commands.Parse("command_2 false false"));
            Assert.IsTrue(bool_commands.Parse("command_2 true false"));
            Assert.IsTrue(bool_commands.Parse("command_2 false true"));
            Assert.IsTrue(bool_commands.Parse("command_2 true true"));

            Assert.IsFalse(bool_commands.Parse("cmd_2 false false"));
            Assert.IsTrue(bool_commands.Parse("cmd_2 true false"));
            Assert.IsTrue(bool_commands.Parse("cmd_2 false true"));
            Assert.IsTrue(bool_commands.Parse("cmd_2 true true"));

            Assert.IsFalse(bool_commands.Parse("c2 false false"));
            Assert.IsTrue(bool_commands.Parse("c2 true false"));
            Assert.IsTrue(bool_commands.Parse("c2 false true"));
            Assert.IsTrue(bool_commands.Parse("c2 true true"));

            Assert.IsFalse(bool_commands.Parse("command_3 false false false"));
            Assert.IsTrue(bool_commands.Parse("command_3 true false false"));
            Assert.IsTrue(bool_commands.Parse("command_3 false true false"));
            Assert.IsTrue(bool_commands.Parse("command_3 true true false"));
            Assert.IsTrue(bool_commands.Parse("command_3 false false true"));
            Assert.IsTrue(bool_commands.Parse("command_3 true false true"));
            Assert.IsTrue(bool_commands.Parse("command_3 false true true"));
            Assert.IsTrue(bool_commands.Parse("command_3 true true true"));

            Assert.IsFalse(bool_commands.Parse("cmd_3 false false false"));
            Assert.IsTrue(bool_commands.Parse("cmd_3 true false false"));
            Assert.IsTrue(bool_commands.Parse("cmd_3 false true false"));
            Assert.IsTrue(bool_commands.Parse("cmd_3 true true false"));
            Assert.IsTrue(bool_commands.Parse("cmd_3 false false true"));
            Assert.IsTrue(bool_commands.Parse("cmd_3 true false true"));
            Assert.IsTrue(bool_commands.Parse("cmd_3 false true true"));
            Assert.IsTrue(bool_commands.Parse("cmd_3 true true true"));

            Assert.IsFalse(bool_commands.Parse("c3 false false false"));
            Assert.IsTrue(bool_commands.Parse("c3 true false false"));
            Assert.IsTrue(bool_commands.Parse("c3 false true false"));
            Assert.IsTrue(bool_commands.Parse("c3 true true false"));
            Assert.IsTrue(bool_commands.Parse("c3 false false true"));
            Assert.IsTrue(bool_commands.Parse("c3 true false true"));
            Assert.IsTrue(bool_commands.Parse("c3 false true true"));
            Assert.IsTrue(bool_commands.Parse("c3 true true true"));

            Assert.Equals(int_commands.Parse("command_1|10"), 10);

            Assert.Equals(int_commands.Parse("cmd_1|10"), 10);

            Assert.Equals(int_commands.Parse("c1|10"), 10);

            Assert.Equals(int_commands.Parse("command_2|10|20"), 30);

            Assert.Equals(int_commands.Parse("cmd_2|10|20"), 30);

            Assert.Equals(int_commands.Parse("c2|10|20"), 30);

            Assert.Equals(int_commands.Parse("command_3|10|20|30"), 60);

            Assert.Equals(int_commands.Parse("cmd_3|10|20|30"), 60);

            Assert.Equals(int_commands.Parse("c3|10|20|30"), 60);
        }

        private void TestFileSystem()
        {
            FileSystem file_system = new FileSystem(this);
            file_system.ChangeDirectory("/");
            if (file_system.IsDirectory("/test"))
            {
                file_system.Delete("/test");
            }
            Assert.IsFalse(file_system.IsDirectory("/test"));
            Assert.IsTrue(file_system.CreateDirectory("/test"));
            Assert.IsTrue(file_system.CreateDirectory("/test/directory"));
            Assert.IsTrue(file_system.IsDirectory("/test"));
            Assert.IsFalse(file_system.AppendFile("/test/append", "test" + Environment.NewLine));
            Assert.IsFalse(file_system.WriteFile("/test/append", "test" + Environment.NewLine));
            Assert.IsFalse(file_system.Delete("/test/append"));
            Assert.IsTrue(file_system.CreateFile("/test/append"));
            Assert.IsTrue(file_system.AppendFile("/test/append", "test" + Environment.NewLine));
            Assert.IsTrue(file_system.WriteFile("/test/append", "test" + Environment.NewLine));
            Assert.IsTrue(file_system.Delete("/test/append"));
            Assert.IsFalse(file_system.ChangeDirectory("/test/not-a-directory"));
            Assert.IsTrue(file_system.ChangeDirectory("/test/directory"));
            Assert.Equals(file_system.CurrentDirectory, "/test/directory");
            //Assert.IsTrue(file_system.ChangeDirectory("/"));
            Assert.IsFalse(file_system.Delete("/test/not-a-directory"));
            Assert.IsTrue(file_system.Delete("/test/directory"));
            Assert.IsTrue(file_system.Delete("/test"));
        }

        private struct UnitTest
        {
            public TestDelegate OnUnitTest { get; private set; }

            public string UnitTestName { get; private set; }

            public UnitTest(TestDelegate onUnitTest, string unitTestName)
            {
                if (onUnitTest == null)
                {
                    throw new ArgumentNullException(nameof(onUnitTest));
                }
                if (unitTestName == null)
                {
                    throw new ArgumentNullException(nameof(unitTestName));
                }
                OnUnitTest = onUnitTest;
                UnitTestName = unitTestName;
            }
        }

        private class UnitTests
        {
            private MyGridProgram gridProgram;

            private List<UnitTest> unitTests = new List<UnitTest>();

            public UnitTests(MyGridProgram gridProgram)
            {
                if (gridProgram == null)
                {
                    throw new ArgumentNullException(nameof(gridProgram));
                }
                this.gridProgram = gridProgram;
            }

            public void Add(TestDelegate onUnitTest, string unitTestName)
            {
                unitTests.Add(new UnitTest(onUnitTest, unitTestName));
            }

            public void Test()
            {
                foreach (UnitTest unit_test in unitTests)
                {
                    try
                    {
                        gridProgram.Echo("Testing " + unit_test.UnitTestName + "...");
                        unit_test.OnUnitTest();
                        gridProgram.Echo("Test " + unit_test.UnitTestName + " was successful!");
                    }
                    catch (Exception e)
                    {
                        gridProgram.Echo("Test " + unit_test.UnitTestName + " failed!");
                        gridProgram.Echo(e.ToString());
                    }
                }
            }
        }

        private void Main(string argument, UpdateType updateSource)
        {
            UnitTests unit_tests = new UnitTests(this);
            unit_tests.Add(TestArgumentStrings, nameof(TestArgumentStrings));
            unit_tests.Add(TestStorageDataSerializer, nameof(TestStorageDataSerializer));
            unit_tests.Add(TestCommands, nameof(TestCommands));
            unit_tests.Add(TestFileSystem, nameof(TestFileSystem));
            unit_tests.Test();
        }
    }
}
