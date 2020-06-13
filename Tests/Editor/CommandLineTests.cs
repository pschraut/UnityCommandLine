//
// CommandLine for Unity. Copyright (c) 2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityCommandLine
//
#pragma warning disable IDE1006, IDE0017
using NUnit.Framework;
using Oddworm.Framework;

namespace Oddworm.EditorFramework.Tests
{
    class CommandLineTests
    {
        enum Fruit
        {
            None,
            Banana,
            Apple,
            Coconut,
            Pineapple,
            Kiwi
        }

        [System.Flags]
        enum Fruits
        {
            Banana = 1 << Fruit.Banana,
            Apple = 1 << Fruit.Apple,
            Coconut = 1 << Fruit.Coconut,
            Pineapple = 1 << Fruit.Pineapple,
            Kiwi = 1 << Fruit.Kiwi
        }

        const string k_CommandLine1 = @"
-Fruit Pineapple
-Fruits ""Apple, Coconut, Kiwi""
-String Yes
-QuotedString ""Hello World""
-Int 1337 // a comment
-NegativeInt -7331
// another comment
-Float 3.21
-NegativeFloat -1.23
-Bool 1/* a block comment...
...*/-BoolString true
";

        const string k_CommandLine2 = @"-Fruit Pineapple -Fruits ""Apple, Coconut, Kiwi"" -String Yes -QuotedString ""Hello World"" -Int 1337 -NegativeInt -7331 -Float 3.21 -NegativeFloat -1.23 -Bool 1 -BoolString true";
        const string k_CommandLine3 = @"-Fruit Pineapple -Fruits ""Apple, Coconut, Kiwi"" -String Yes -Int 1337 -NegativeInt -7331 -Float 3.21 -NegativeFloat -1.23 -Bool 1 -BoolString true -QuotedString ""Hello World""";
        const string k_CommandLine4 = @"-Fruit Pineapple -String Yes -Int 1337 -NegativeInt -7331 -Float 3.21 -NegativeFloat -1.23 -Bool 1 -BoolString true -QuotedString ""Hello World"" -Fruits ""Apple, Coconut, Kiwi""";
        const string k_CommandLine5 = @"/*comment*/-Fruit Pineapple/*comment*/ /*comment*/-String Yes/*comment*/ /*comment*/-Int 1337/*comment*/ /*comment*/-NegativeInt -7331/*comment*/ /*comment*/-Float 3.21/*comment*/ /*comment*/-NegativeFloat/*comment*/ /*comment*/-1.23/*comment*/ /*comment*/-Bool 1/*comment*/ /*comment*/-BoolString true/*comment*/ /*comment*/-QuotedString ""Hello World""/*comment*/ /*comment*/-Fruits ""Apple, Coconut, Kiwi""/*comment*/";
        const string k_CommandLine6 = @"/*comment*/-Fruit/*comment*/Pineapple/*comment*/-Fruits/*comment*/""Apple, Coconut, Kiwi""/*comment*/-String/*comment*/Yes/*comment*/-QuotedString/*comment*/""Hello World""/*comment*/-Int/*comment*/1337/*comment*/-NegativeInt/*comment*/-7331/*comment*/-Float/*comment*/3.21/*comment*/-NegativeFloat/*comment*/-1.23/*comment*/-Bool/*comment*/1/*comment*/-BoolString/*comment*/true/*comment*/";

        const string k_CommandLine7 = @"// ----------------------
// this is a comment
// ------------------------------------
-Fruit Pineapple // comment
//comment
-Fruits ""Apple, Coconut, Kiwi"" //comment
-String Yes//comment
//comment
-QuotedString ""Hello World""// comment /* abc */
//comment
-Int 1337 // a comment/*
foobar */
//comment
-NegativeInt -7331
//comment
-Float 3.21
//comment
-NegativeFloat -1.23
//comment
-Bool 1/* a block comment...
...*/-BoolString true
//comment
";

        static readonly string[] s_SameCommandLines1 = new string[]
        {
            k_CommandLine1, k_CommandLine2, k_CommandLine3,
            k_CommandLine4, k_CommandLine5, k_CommandLine6,
            k_CommandLine7
        };

        [Test]
        public void IsEnabled()
        {
#if ODDWORM_COMMANDLINE_DISABLE
            CommandLine.isEnabled = true;
            Assert.AreEqual(CommandLine.isEnabled, false);

            CommandLine.isEnabled = false;
            Assert.AreEqual(CommandLine.isEnabled, false);
#else
            CommandLine.isEnabled = true;
            Assert.AreEqual(CommandLine.isEnabled, true);

            CommandLine.isEnabled = false;
            Assert.AreEqual(CommandLine.isEnabled, false);

            CommandLine.isEnabled = true;
            Assert.AreEqual(CommandLine.isEnabled, true);
#endif
        }

        [Test]
        public void String()
        {
            foreach (var enabled in new[] { true, false })
            {
                CommandLine.isEnabled = enabled;

                foreach (var s in s_SameCommandLines1)
                {
                    CommandLine.Init(s);

                    if (CommandLine.isEnabled)
                        Assert.AreEqual(CommandLine.GetString("-String", ""), "Yes");
                    else
                        Assert.AreEqual(CommandLine.GetString("-String", "bla"), "bla");
                }
            }
        }

        [Test]
        public void QuotedString()
        {
            foreach (var enabled in new[] { true, false })
            {
                CommandLine.isEnabled = enabled;

                foreach (var s in s_SameCommandLines1)
                {
                    CommandLine.Init(s);

                    if (CommandLine.isEnabled)
                        Assert.AreEqual(CommandLine.GetString("-QuotedString", ""), "Hello World");
                    else
                        Assert.AreEqual(CommandLine.GetString("-QuotedString", "bla bla"), "bla bla");
                }
            }
        }

        [Test]
        public void Enum()
        {
            foreach (var enabled in new[] { true, false })
            {
                CommandLine.isEnabled = enabled;

                foreach (var s in s_SameCommandLines1)
                {
                    CommandLine.Init(s);

                    if (CommandLine.isEnabled)
                        Assert.AreEqual(CommandLine.GetEnum<Fruit>("-Fruit", Fruit.None), Fruit.Pineapple);
                    else
                        Assert.AreEqual(CommandLine.GetEnum<Fruit>("-Fruit", Fruit.None), Fruit.None);
                }
            }
        }

        [Test]
        public void FlagsEnum()
        {
            foreach (var enabled in new[] { true, false })
            {
                CommandLine.isEnabled = enabled;

                foreach (var s in s_SameCommandLines1)
                {
                    CommandLine.Init(s);

                    if (CommandLine.isEnabled)
                        Assert.AreEqual(CommandLine.GetEnum<Fruits>("-Fruits", 0), Fruits.Apple | Fruits.Coconut | Fruits.Kiwi);
                    else
                        Assert.AreEqual(CommandLine.GetEnum<Fruits>("-Fruits", 0), (Fruits)0);
                }
            }
        }

        [Test]
        public void Int()
        {
            foreach (var enabled in new[] { true, false })
            {
                CommandLine.isEnabled = enabled;

                foreach (var s in s_SameCommandLines1)
                {
                    CommandLine.Init(s);

                    if (CommandLine.isEnabled)
                    {
                        Assert.AreEqual(CommandLine.GetInt("-Int", 0), 1337);
                        Assert.AreEqual(CommandLine.GetInt("-NegativeInt", 0), -7331);
                    }
                    else
                    {
                        Assert.AreEqual(CommandLine.GetInt("-Int", 0), 0);
                        Assert.AreEqual(CommandLine.GetInt("-NegativeInt", 0), 0);
                    }
                }
            }
        }

        [Test]
        public void Float()
        {
            foreach (var enabled in new[] { true, false })
            {
                CommandLine.isEnabled = enabled;

                foreach (var s in s_SameCommandLines1)
                {
                    CommandLine.Init(s);

                    if (CommandLine.isEnabled)
                    {
                        Assert.AreEqual(CommandLine.GetFloat("-Float", 0), 3.21f);
                        Assert.AreEqual(CommandLine.GetFloat("-NegativeFloat", 0), -1.23f);
                    }
                    else
                    {
                        Assert.AreEqual(CommandLine.GetFloat("-Float", 0), 0);
                        Assert.AreEqual(CommandLine.GetFloat("-NegativeFloat", 0), 0);
                    }
                }
            }
        }

        [Test]
        public void Bool()
        {
            foreach (var enabled in new[] { true, false })
            {
                CommandLine.isEnabled = enabled;

                foreach (var s in s_SameCommandLines1)
                {
                    CommandLine.Init(s);

                    if (CommandLine.isEnabled)
                    {
                        Assert.AreEqual(CommandLine.GetBool("-Bool", false), true);
                        Assert.AreEqual(CommandLine.GetBool("-BoolString", false), true);
                    }
                    else
                    {
                        Assert.AreEqual(CommandLine.GetBool("-Bool", false), false);
                        Assert.AreEqual(CommandLine.GetBool("-BoolString", false), false);
                    }
                }
            }
        }
    }
}
