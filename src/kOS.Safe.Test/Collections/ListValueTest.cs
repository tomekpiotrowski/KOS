﻿using System;
using System.Collections.Generic;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using NUnit.Framework;

namespace kOS.Safe.Test.Collections
{
    [TestFixture]
    public class ListValueTest
    {
        [Test]
        public void CanCreate()
        {
            var list = new ListValue();
            Assert.IsNotNull(list);
        }

        [Test]
        public void CanAddItem()
        {
            var list = new ListValue();
            Assert.IsNotNull(list);
            var length = InvokeDelegate(list, "LENGTH");
            Assert.AreEqual(0,length);

            InvokeDelegate(list, "ADD", new object());

            length = InvokeDelegate(list, "LENGTH");
            Assert.AreEqual(1,length);
        }

        [Test]
        public void CanClear()
        {
            var list = new ListValue();

            InvokeDelegate(list, "ADD", new object());
            InvokeDelegate(list, "ADD", new object());

            var length = InvokeDelegate(list, "LENGTH");
            Assert.AreEqual(2,length);
            InvokeDelegate(list, "CLEAR");
            length = InvokeDelegate(list, "LENGTH");
            Assert.AreEqual(0,length);
        }

        [Test]
        public void CanGetIndex()
        {
            var list = new ListValue();

            var zedObject = new object();
            InvokeDelegate(list, "ADD", zedObject);
            var firstObject = new object();
            InvokeDelegate(list, "ADD", firstObject);
            var secondObject = new object();
            InvokeDelegate(list, "ADD", secondObject);
            var thirdObject = new object();
            InvokeDelegate(list, "ADD", thirdObject);

            var length = InvokeDelegate(list, "LENGTH");
            Assert.AreEqual(4,length);

            Assert.AreSame(zedObject, list[0]);
            Assert.AreSame(firstObject, list[1]);
            Assert.AreSame(secondObject, list[2]);
            Assert.AreSame(thirdObject, list[3]);
            Assert.AreNotSame(list[0],list[1]);
            Assert.AreNotSame(list[0],list[2]);
            Assert.AreNotSame(list[0],list[3]);
            Assert.AreNotSame(list[1],list[2]);
            Assert.AreNotSame(list[1],list[3]);
            Assert.AreNotSame(list[2],list[3]);
        }

        [Test]
        public void CopyIsACopy()
        {
            var list = new ListValue();

            var zedObject = new object();
            InvokeDelegate(list, "ADD", zedObject);
            var firstObject = new object();
            InvokeDelegate(list, "ADD", firstObject);
            var secondObject = new object();
            InvokeDelegate(list, "ADD", secondObject);
            var thirdObject = new object();
            InvokeDelegate(list, "ADD", thirdObject);

            var length = InvokeDelegate(list, "LENGTH");
            Assert.AreEqual(4,length);

            var copy = InvokeDelegate(list, "COPY") as ListValue;
            Assert.AreNotSame(list, copy);

            var copyLength = InvokeDelegate(copy, "LENGTH");
            Assert.AreEqual(4,copyLength);

            InvokeDelegate(copy, "CLEAR");

            copyLength = InvokeDelegate(copy, "LENGTH");
            Assert.AreEqual(0,copyLength);

            length = InvokeDelegate(list, "LENGTH");
            Assert.AreEqual(4,length);
        }

        [Test]
        public void CanTestContains()
        {
            var list = new ListValue();

            var zedObject = new object();
            InvokeDelegate(list, "ADD", zedObject);
            var firstObject = new object();
            InvokeDelegate(list, "ADD", firstObject);
            var secondObject = new object();
            var thirdObject = new object();

            var length = InvokeDelegate(list, "LENGTH");
            Assert.AreEqual(2,length);


            Assert.IsTrue((bool)InvokeDelegate(list, "CONTAINS", zedObject));
            Assert.IsTrue((bool)InvokeDelegate(list, "CONTAINS", firstObject));
            Assert.IsFalse((bool)InvokeDelegate(list, "CONTAINS", secondObject));
            Assert.IsFalse((bool)InvokeDelegate(list, "CONTAINS", thirdObject));
        }
        
        /// <summary>
        /// Creates a complex example of a nested list of lists and other
        /// things, to use in some of the tests to prove complex example cases.
        /// Returns a list that looks like so:
        /// <pre>
        /// list {
        ///     100,
        ///     200,
        ///     list {
        ///         list {
        ///             "inner string 1",
        ///             2
        ///         },
        ///         "string,one.two",
        ///         "string,one.three"
        ///     },
        ///     list {
        ///         "string,two.one",
        ///         "string,two.two"
        ///     },
        ///     "String, outer value"
        /// }
        /// </pre>
        /// This should be sufficiently complex to work with for testing a variety of cases.
        /// 
        /// </summary>
        /// <returns>A list containing the description above</returns>
        private ListValue MakeNestedExample()
        {
            const string OUTER_STRING = "String, outer value";
            
            ListValue list = new ListValue();
            ListValue innerList1 = new ListValue();
            ListValue innerList2 = new ListValue();
            ListValue innerInnerList = new ListValue();
            
            innerInnerList.Add( "inner string 1");
            innerInnerList.Add( 2 );
            
            innerList1.Add( innerInnerList );
            innerList1.Add( "string,one.two" );
            innerList1.Add( "string,one.three" );

            innerList2.Add( "string,two.one" );
            innerList2.Add( "string,two.two" );
            
            InvokeDelegate(list,"ADD", 100);
            InvokeDelegate(list,"ADD", 200);
            InvokeDelegate(list,"ADD", innerList1);            
            InvokeDelegate(list,"ADD", innerList2);            
            InvokeDelegate(list,"ADD", OUTER_STRING);
            
            return list;
        }
        
        [Test]
        public void EachListConstructor()
        {
            var baseList = new ListValue();
            var baseDelegate = ((NoArgsSuffix<int>.Del<int>)baseList.GetSuffix("LENGTH"));
            Assert.AreEqual(0, baseDelegate.Invoke());

            var castList = ListValue.CreateList(new List<object>());
            var castDelegate = ((NoArgsSuffix<int>.Del<int>)castList.GetSuffix("LENGTH"));
            Assert.AreEqual(0, castDelegate.Invoke());

            var copyDelegate = (NoArgsSuffix<ListValue>.Del<ListValue>)baseList.GetSuffix("COPY");
            var copyList = copyDelegate.Invoke();

            Assert.AreEqual(0, ((NoArgsSuffix<int>.Del<int>)copyList.GetSuffix("LENGTH")).Invoke());
        }

        private object InvokeDelegate(ListValue list, string suffixName, params object[] parameters)
        {
            var lengthObj = list.GetSuffix(suffixName);
            Assert.IsNotNull(lengthObj);
            var lengthDelegate = lengthObj as Delegate;
            Assert.IsNotNull(lengthDelegate);
            var length = lengthDelegate.DynamicInvoke(parameters);
            return length;
        }
    }
}
