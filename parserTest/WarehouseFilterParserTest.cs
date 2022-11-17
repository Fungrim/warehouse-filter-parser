
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarehouseFilterParser;

namespace WarehouseFilterParserTest {

    [TestClass]
    public class EvaluationTests {

        [TestMethod]
        public void FulfillableWithSpecificSkuAndCountry() {
            var w = new Warehouse(true);
            var skus = new List<string>();
            skus.Add("a1");
            skus.Add("a2");
            var o = new Order("DE", skus);
            Assert.IsTrue(new Parser(o, w).Evaluate("lineItems_contains_sku(\"a1\") AND is_fulfillable() AND NOT shippingAddress.country = \"SE\"")); 
        }

        [TestMethod]
        public void ShouldFindSkuAmongItems() {
            var w = new Warehouse(true);
            var skus = new List<string>();
            skus.Add("a1");
            skus.Add("a2");
            var o = new Order("SE", skus);
            Assert.IsTrue(new Parser(o, w).Evaluate("lineItems_contains_sku(\"a1\")")); 
            Assert.IsFalse(new Parser(o, w).Evaluate("lineItems_contains_sku(\"a3\")")); 
        }

        [TestMethod]
        public void TrivialOrderAttribute() {
            var w = new Warehouse(true);
            var o = new Order("SE", new List<string>());
            Assert.IsTrue(new Parser(o, w).Evaluate("shippingAddress.country = \"SE\"")); 
            Assert.IsFalse(new Parser(o, w).Evaluate("NOT shippingAddress.country = \"SE\"")); 
        }

        [TestMethod]
        public void TrivialBoolean() {
            var w = new Warehouse(true);
            var o = new Order("SE", new List<string>());
            Assert.IsTrue(new Parser(o, w).Evaluate("true")); 
            Assert.IsFalse(new Parser(o, w).Evaluate("false")); 
        }

        [TestMethod]
        public void TrivialStringComparison() {
            var w = new Warehouse(true);
            var o = new Order("SE", new List<string>());
            Assert.IsTrue(new Parser(o, w).Evaluate("\"a\" = \"a\"")); 
            Assert.IsFalse(new Parser(o, w).Evaluate("\"a\" != \"a\"")); 
        }

        [TestMethod]
        public void TrivialIsFulfillable() {
            var w = new Warehouse(true);
            var o = new Order("SE", new List<string>());
            Assert.IsTrue(new Parser(o, w).Evaluate("is_fulfillable()"));
            var w2 = new Warehouse(false); 
            Assert.IsFalse(new Parser(o, w2).Evaluate("is_fulfillable()")); 
        }

        [TestMethod]
        public void SimpleBinaryOperation() {
            var w = new Warehouse(true);
            var o = new Order("SE", new List<string>());
            Assert.IsTrue(new Parser(o, w).Evaluate("true AND true"));
            Assert.IsFalse(new Parser(o, w).Evaluate("true AND NOT true")); 
        }

        [TestMethod]
        public void SimpleBinaryOperationWithGroup() {
            var w = new Warehouse(true);
            var o = new Order("SE", new List<string>());
            Assert.IsTrue(new Parser(o, w).Evaluate("true AND (true OR false)"));
            Assert.IsFalse(new Parser(o, w).Evaluate("true AND NOT (true OR false)")); 
        }
    }
}