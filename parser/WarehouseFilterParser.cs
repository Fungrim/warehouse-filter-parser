using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;

namespace WarehouseFilterParser {

    public class Parser {

        private readonly Order order;
        private readonly Warehouse warehouse;

        public Parser(Order order, Warehouse warehouse) {
            this.order = order;
            this.warehouse = warehouse;
        }

        public bool Evaluate(String expression) {
            var lexer = new WarehouseFilterGrammarLexer(new AntlrInputStream(expression));
            var parser = new WarehouseFilterGrammarParser(new CommonTokenStream(lexer));
            return (bool) new Visitor(order, warehouse).Visit(parser.parse());
        }
    }

    public class Order {

        private readonly string deliveryCountry;
        private readonly List<string> skus;

        public Order(String deliveryCountry, List<String> skus) {
            this.deliveryCountry = deliveryCountry;
            this.skus = skus;
        }

        public String getAttribute(String name) {
            if(name.Equals("shippingAddress.country")) {
                return deliveryCountry;
            } 
            return null;
        }

        public Boolean LineItemsContainsSku(String s) {
            return skus.Contains(s);
        }
    }

    public class Warehouse {
        private readonly bool isFulfillable;

        public Warehouse(bool isFulfillable) {
            this.isFulfillable = isFulfillable;
        }

        public bool IsFulfillable(Order order) {
            return this.isFulfillable; // check inventory status
        }
    }

    class Visitor : WarehouseFilterGrammarBaseVisitor<Object> {

        private Order order;
        private Warehouse warehouse;

        public Visitor(Order order, Warehouse warehouse) {
            this.order = order;
            this.warehouse = warehouse;
        }

        public override object VisitParse([NotNull] WarehouseFilterGrammarParser.ParseContext context)
        {
            return base.Visit(context.expression()); 
        }
        
        public override object VisitDecimalExpression([NotNull] WarehouseFilterGrammarParser.DecimalExpressionContext context)
        {
            return Double.Parse(context.DECIMAL().GetText());
        }

        public override object VisitStringExpression([NotNull] WarehouseFilterGrammarParser.StringExpressionContext context)
        {
            return context.STRING().GetText();
        }

        public override object VisitIdentifierExpression([NotNull] WarehouseFilterGrammarParser.IdentifierExpressionContext context)
        {
            var attr = order.getAttribute(context.IDENTIFIER().GetText());
            if(attr == null) {
                throw new ApplicationException("no such attribute: " + attr);
            }
            return ToUnqoutedString(attr);
        }

        public override object VisitNotExpression([NotNull] WarehouseFilterGrammarParser.NotExpressionContext context)
        {
            return !((bool)this.Visit(context.expression()));
        }

        public override object VisitParenExpression([NotNull] WarehouseFilterGrammarParser.ParenExpressionContext context)
        {
            return base.Visit(context.expression());
        }

        public override object VisitMethodCall([NotNull] WarehouseFilterGrammarParser.MethodCallContext context)
        {
            var meth = context.IDENTIFIER().GetText();
            if (meth.Equals("is_fulfillable")) 
            {
                return this.warehouse.IsFulfillable(this.order);
            }
            else if(meth.Equals("lineItems_contains_sku")) {
                if(context.argumentList().IsEmpty) {
                    throw new ApplicationException("missing argument: method lineItems_contains_sku requires exactly one argument");
                }
                var arg = this.Visit(context.argumentList().GetChild(0));
                return order.LineItemsContainsSku(ToUnqoutedString(arg));
            }
            throw new ApplicationException("not implemented: method " + meth);
        }

        public override object VisitComparatorExpression([NotNull] WarehouseFilterGrammarParser.ComparatorExpressionContext context)
        {
            if (context.op.EQ() != null)
            {
                var l = this.Visit(context.left);
                // TODO more robust type checking
                if(l.GetType() == typeof(String)) 
                {
                    return ToUnqoutedString(l).Equals(ToUnqoutedString(this.Visit(context.right)));
                } else {
                    return l.Equals(this.Visit(context.right));
                }
            }
            else if (context.op.LE() != null)
            {
                return AsDouble(context.left) <= AsDouble(context.right);
            }
            else if (context.op.GE() != null)
            {
                return AsDouble(context.left) >= AsDouble(context.right);
            }
            else if (context.op.LT() != null)
            {
                return AsDouble(context.left) < AsDouble(context.right);
            }
            else if (context.op.GT() != null)
            {
                return AsDouble(context.left) > AsDouble(context.right);
            } 
            else if (context.op.NE() != null) {
                return !this.Visit(context.left).Equals(this.Visit(context.right));
            }
            throw new ApplicationException("not implemented: comparator operator " + context.op.GetText());
        }

        public override object VisitBinaryExpression([NotNull] WarehouseFilterGrammarParser.BinaryExpressionContext context)
        {
            if (context.op.AND() != null)
            {
                return AsBool(context.left) && AsBool(context.right);
            }
            else if (context.op.OR() != null)
            {
                return AsBool(context.left) || AsBool(context.right);
            }
            throw new ApplicationException("not implemented: binary operator " + context.op.GetText());
        }

        public override object VisitBoolExpression([NotNull] WarehouseFilterGrammarParser.BoolExpressionContext context)
        {
            return bool.Parse(context.GetText());
        }

        private bool AsBool(WarehouseFilterGrammarParser.ExpressionContext context)
        {
            return (bool)Visit(context);
        }

        private double AsDouble(WarehouseFilterGrammarParser.ExpressionContext context)
        {
            return (double)Visit(context);
        }

        private String ToUnqoutedString(Object o) {
            var s = o.ToString().Trim();
            if(s.StartsWith("\"") && s.EndsWith("\"")) {
                s = s.Substring(1, s.Length - 2);
            }
            return s;
        }
    }
}