using System;

namespace ProtoCore.AST.AssociativeAST
{
	public class Analyser
	{
        public DependencyTracker Analyse(LanguageCodeBlock cb, ProtoCore.CompileTime.Context ct)
		{
			//@TODO: Replace this with a parser
			
			//For now, assume that the code block is
			
			//a = 1..1000..+1
			//b = SQRT(a)
			//c = a * 2
			//d = 
			//{
			//		FromPoint(a<1>, b<2>, c<3>)
			//}
			
			
			//This will give us an AST representation
			
			DependencyTracker tracker = new DependencyTracker();
			
			throw new NotImplementedException();
			
		}
	}
}

