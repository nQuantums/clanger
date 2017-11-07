#pragma once
#include "Entity.h"

class Analyzer {
public:
	std::unordered_set<std::unique_ptr<Entity>, Entity::Hash, Entity::Equals> Entities;
	std::vector<std::unique_ptr<clang::CompilerInstance>> CompilerInstances;
};

class MyVisitor : public clang::RecursiveASTVisitor<MyVisitor> {
private:
	std::shared_ptr<Analyzer> _Analyzer;
	clang::PrintingPolicy policy;
	clang::CompilerInstance& compilerInstance;
	clang::SourceManager& sourceManager;

public:
	typedef clang::RecursiveASTVisitor<MyVisitor> base;

	MyVisitor(std::shared_ptr<Analyzer> analyzer, clang::CompilerInstance& compilerInstance)
		: compilerInstance(compilerInstance)
		, policy(clang::PrintingPolicy(compilerInstance.getASTContext().getPrintingPolicy()))
		, sourceManager(compilerInstance.getASTContext().getSourceManager()) {
		_Analyzer = analyzer;
		policy.Bool = 1; // print()にてboolが_Boolと表示されないようにする
	}

	bool TraverseDecl(clang::Decl *x) {
		// your logic here
		//llvm::errs() << x->getDeclKindName() << "\n"; // Declの型表示
		//if (clang::NamedDecl *N = clang::dyn_cast<clang::NamedDecl>(x)) {
		//	llvm::errs() << " " << N->getNameAsString();  // NamedDeclなら名前表示
		//}
		//llvm::errs() << " (" << x->getLocation().printToString(sourceManager) << ")\n"; // ソース上の場所表示
		//std::cout << "--------" << std::endl;
		x->dumpColor();


		//llvm::SmallString<512> buf;
		//clang::index::generateUSRForDecl(x, buf);
		//buf.push_back('\0');
		//std::cout << buf.c_str() << std::endl;

		base::TraverseDecl(x); // Forward to base class
		return true; // Return false to stop the AST analyzing
	}
	bool TraverseVarDecl(clang::VarDecl* D) {
		_Analyzer->Entities.insert(std::make_unique<Entity>(D));
		base::TraverseVarDecl(D);
		return true;
	}
	bool TraverseStmt(clang::Stmt *x) {
		// your logic here
		//llvm::errs() << x->getDeclKindName() << "\n"; // Declの型表示
		//if (clang::NamedDecl *N = clang::dyn_cast<clang::NamedDecl>(x)) {
		//	llvm::errs() << " " << N->getNameAsString();  // NamedDeclなら名前表示
		//}
		//llvm::errs() << " (" << x->getLocation().printToString(sourceManager) << ")\n"; // ソース上の場所表示
		//std::cout << "--------" << std::endl;
		//x->dumpColor();
		base::TraverseStmt(x);
		return true;
	}
	bool TraverseType(clang::QualType x) {
		//// your logic here
		//std::cout << "--------" << std::endl;
		//x->dump();

		base::TraverseType(x);
		return true;
	}
	bool VisitBinaryOperator(clang::BinaryOperator *S) {
		_Analyzer->Entities.insert(std::make_unique<Entity>(S));
		//auto kind = S->getValueKind();
		//std::cout << S->getOpcodeStr().str() << std::endl;
		return true;
	}
	//bool TraverseBinaryOperator(clang::BinaryOperator* x) {
	//	return true;
	//}
	bool VisitFunctionDecl(clang::FunctionDecl* D) {
		_Analyzer->Entities.insert(std::make_unique<Entity>(D));
		//llvm::SmallString<512> buf;
		//clang::index::generateUSRForDecl(D, buf);
		//buf.push_back('\0');
		//std::cout << buf.c_str() << std::endl;
		return true;
	}
	bool VisitCallExpr(clang::CallExpr *S) {
		_Analyzer->Entities.insert(std::make_unique<Entity>(S));
		//auto D = S->getCalleeDecl();

		//llvm::SmallString<512> buf;
		//clang::index::generateUSRForDecl(D, buf);
		//buf.push_back('\0');
		//std::cout << buf.c_str() << std::endl;

		return true;
	}
};

class MyAstConsumer : public clang::ASTConsumer {
public:
	explicit MyAstConsumer(std::shared_ptr<Analyzer> analyzer, clang::CompilerInstance& compilerInstance) {
		_Analyzer = analyzer;
		this->visitor = std::make_shared<MyVisitor>(_Analyzer, compilerInstance);
	}

	virtual void HandleTranslationUnit(clang::ASTContext& context) {
		// Traversing the translation unit decl via a RecursiveASTVisitor
		// will visit all nodes in the AST.
		visitor->TraverseDecl(context.getTranslationUnitDecl());
	}

private:
	std::shared_ptr<Analyzer> _Analyzer;
	// A RecursiveASTVisitor implementation.
	std::shared_ptr<MyVisitor> visitor;
};

class MyFrontendAction : public clang::SyntaxOnlyAction /*ASTFrontendAction*/ {
	std::shared_ptr<Analyzer> _Analyzer;

public:
	MyFrontendAction(std::shared_ptr<Analyzer> analyzer) {
		_Analyzer = analyzer;
	}

	virtual std::unique_ptr<clang::ASTConsumer> CreateASTConsumer(clang::CompilerInstance& compilerInstance, StringRef file) {
		return llvm::make_unique<MyAstConsumer>(_Analyzer, compilerInstance); // pass CI pointer to ASTConsumer
	}
};

class MyFrontendActionFactory : public clang::tooling::FrontendActionFactory {
	std::shared_ptr<Analyzer> _Analyzer;
public:
	MyFrontendActionFactory(std::shared_ptr<Analyzer> analyzer) {
		_Analyzer = analyzer;
	}

	/// \brief Invokes the compiler with a FrontendAction created by create().
	bool runInvocation(std::shared_ptr<clang::CompilerInvocation> Invocation, clang::FileManager *Files, std::shared_ptr<clang::PCHContainerOperations> PCHContainerOps, clang::DiagnosticConsumer *DiagConsumer) override {
		//return clang::tooling::FrontendActionFactory::runInvocation(Invocation, Files, PCHContainerOps, DiagConsumer);

		auto ci = std::make_unique<clang::CompilerInstance>(std::move(PCHContainerOps));

		// Create a compiler instance to handle the actual work.
		clang::CompilerInstance& Compiler = *ci.get();
		Compiler.setInvocation(std::move(Invocation));
		Compiler.setFileManager(Files);

		// The FrontendAction can have lifetime requirements for Compiler or its
		// members, and we need to ensure it's deleted earlier than Compiler. So we
		// pass it to an std::unique_ptr declared after the Compiler variable.
		std::unique_ptr<clang::FrontendAction> ScopedToolAction(create());

		// Create the compiler's actual diagnostics engine.
		Compiler.createDiagnostics(DiagConsumer, /*ShouldOwnClient=*/false);
		if (!Compiler.hasDiagnostics())
			return false;

		Compiler.createSourceManager(*Files);

		const bool Success = Compiler.ExecuteAction(*ScopedToolAction);

		Files->clearStatCaches();

		_Analyzer->CompilerInstances.push_back(std::move(ci));

		return Success;
	}

	clang::FrontendAction *create() override {
		return new MyFrontendAction(_Analyzer);
	}
};
