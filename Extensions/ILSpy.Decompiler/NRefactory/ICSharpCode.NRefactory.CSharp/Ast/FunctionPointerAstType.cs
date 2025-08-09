using System;
using ICSharpCode.NRefactory.TypeSystem;

namespace ICSharpCode.NRefactory.CSharp {
	public class FunctionPointerAstType : AstType {
		public static readonly TokenRole PointerRole = new TokenRole("*");
		public static readonly Role<AstType> CallingConventionRole = new Role<AstType>("CallConv", AstType.Null);

		public bool HasUnmanagedCallingConvention { get; set; }

		public AstNodeCollection<AstType> CallingConventions {
			get { return GetChildrenByRole(CallingConventionRole); }
		}

		public AstNodeCollection<ParameterDeclaration> Parameters {
			get { return GetChildrenByRole(Roles.Parameter); }
		}

		public AstType ReturnType {
			get { return GetChildByRole(Roles.Type); }
			set { SetChildByRole(Roles.Type, value); }
		}

		public override void AcceptVisitor(IAstVisitor visitor)
		{
			visitor.VisitFunctionPointerType(this);
		}

		public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
		{
			return visitor.VisitFunctionPointerType(this);
		}

		public override S AcceptVisitor<T, S>(IAstVisitor<T, S> visitor, T data)
		{
			return visitor.VisitFunctionPointerType(this, data);
		}

		protected internal override bool DoMatch(AstNode other, PatternMatching.Match match)
		{
			return other is FunctionPointerAstType o
				   && this.CallingConventions.DoMatch(o.CallingConventions, match)
				   && this.Parameters.DoMatch(o.Parameters, match)
				   && this.ReturnType.DoMatch(o.ReturnType, match);
		}

		public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider = null)
		{
			return SpecialType.UnknownType;
		}
	}
}
