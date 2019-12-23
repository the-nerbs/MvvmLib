using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvvmLib.Tests.Standalone
{
    [TestClass]
    public class ObservableObjectTests
    {
        class TestObject : ObservableObject
        {
            public int theValue;


            public void Set(int value)
            {
                Set(ref theValue, value);
            }

            public void Set(int value, string propertyName)
            {
                Set(ref theValue, value, propertyName);
            }


            public void SetIfChanged(int value)
            {
                SetIfChanged(ref theValue, value);
            }

            public void SetIfChanged(int value, string propertyName)
            {
                SetIfChanged(ref theValue, value, propertyName);
            }


            public void SetIfChangedWithComparer(int value, IEqualityComparer<int> comparer)
            {
                SetIfChanged(ref theValue, value, comparer);
            }

            public void SetIfChangedWithComparer(int value, IEqualityComparer<int> comparer, string propertyName)
            {
                SetIfChanged(ref theValue, value, comparer, propertyName);
            }


            public void Raise(string propertyName)
            {
                RaisePropertyChanged(propertyName);
            }
        }

        private class MagnitudeComparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y)
            {
                return Math.Abs(x) == Math.Abs(y);
            }

            public int GetHashCode(int obj)
            {
                return Math.Abs(obj);
            }
        }


        [TestMethod]
        public void TestSetNoPropertyName()
        {
            var obj = new TestObject();

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.Set(5);
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(new[] { nameof(obj.Set) }, changes);
        }

        [TestMethod]
        public void TestSetWithPropertyName()
        {
            var obj = new TestObject();

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.Set(5, "test");
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(new[] { "test" }, changes);
        }

        [TestMethod]
        public void TestSetWithNullPropertyName()
        {
            var obj = new TestObject();

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.Set(5, null);
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(new string[] { null }, changes);
        }

        [TestMethod]
        public void TestSetWithEmptyPropertyName()
        {
            var obj = new TestObject();

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.Set(5, string.Empty);
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(new[] { string.Empty }, changes);
        }


        [TestMethod]
        public void TestSetIfChangedNoPropertyName()
        {
            var obj = new TestObject();

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.SetIfChanged(5);
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(new[] { nameof(obj.SetIfChanged) }, changes);
        }

        [TestMethod]
        public void TestSetIfChangedNoPropertyNameNoChange()
        {
            var obj = new TestObject();
            obj.theValue = 5;

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.SetIfChanged(5);
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(Array.Empty<string>(), changes);
        }

        [TestMethod]
        public void TestSetIfChangedWithPropertyName()
        {
            var obj = new TestObject();

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.SetIfChanged(5, "test");
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(new[] { "test" }, changes);
        }

        [TestMethod]
        public void TestSetIfChangedWithPropertyNameNoChange()
        {
            var obj = new TestObject();
            obj.theValue = 5;

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.SetIfChanged(5, "test");
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(Array.Empty<string>(), changes);
        }

        [TestMethod]
        public void TestSetIfChangedWithNullPropertyName()
        {
            var obj = new TestObject();

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.SetIfChanged(5, null);
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(new string[] { null }, changes);
        }

        [TestMethod]
        public void TestSetIfChangedWithEmptyPropertyName()
        {
            var obj = new TestObject();

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.SetIfChanged(5, string.Empty);
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(new[] { string.Empty }, changes);
        }


        [TestMethod]
        public void TestSetIfChangedWithComparerNoPropertyName()
        {
            var obj = new TestObject();

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.SetIfChangedWithComparer(5, new MagnitudeComparer());
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(new[] { nameof(obj.SetIfChangedWithComparer) }, changes);
        }

        [TestMethod]
        public void TestSetIfChangedWithComparerNoPropertyNameNoChange()
        {
            var obj = new TestObject();
            obj.theValue = -5;

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.SetIfChangedWithComparer(5, new MagnitudeComparer());
            });

            Assert.AreEqual(-5, obj.theValue);
            CollectionAssert.AreEqual(Array.Empty<string>(), changes);
        }

        [TestMethod]
        public void TestSetIfChangedWithComparerWithPropertyName()
        {
            var obj = new TestObject();

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.SetIfChangedWithComparer(5, new MagnitudeComparer(), "test");
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(new[] { "test" }, changes);
        }

        [TestMethod]
        public void TestSetIfChangedWithComparerWithPropertyNameNoChange()
        {
            var obj = new TestObject();
            obj.theValue = 5;

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.SetIfChangedWithComparer(5, new MagnitudeComparer(), "test");
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(Array.Empty<string>(), changes);
        }

        [TestMethod]
        public void TestSetIfChangedWithComparerWithNullPropertyName()
        {
            var obj = new TestObject();

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.SetIfChangedWithComparer(5, new MagnitudeComparer(), null);
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(new string[] { null }, changes);
        }

        [TestMethod]
        public void TestSetIfChangedWithComparerWithEmptyPropertyName()
        {
            var obj = new TestObject();

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.SetIfChangedWithComparer(5, new MagnitudeComparer(), string.Empty);
            });

            Assert.AreEqual(5, obj.theValue);
            CollectionAssert.AreEqual(new[] { string.Empty }, changes);
        }


        [TestMethod]
        public void TestRaisePropertyChanged()
        {
            var obj = new TestObject();

            var changes = CapturePropertyChanges(obj, () =>
            {
                obj.Raise("test");
            });

            CollectionAssert.AreEqual(new[] { "test" }, changes);
        }

        [TestMethod]
        public void TestRaisePropertyChangedWithNoHandler()
        {
            var obj = new TestObject();

            // "assert" that this does not throw
            obj.Raise("test");
        }


        private List<string> CapturePropertyChanges(TestObject obj, Action action)
        {
            var changes = new List<string>();

            obj.PropertyChanged += (sender, e) =>
            {
                changes.Add(e.PropertyName);
            };

            action();

            return changes;
        }
    }
}
