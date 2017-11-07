// cppclangtest.cpp : コンソール アプリケーションのエントリ ポイントを定義します。
//

#include "stdafx.h"
#include <iostream>
#include <iomanip>
#include <vector>
#include <unordered_map>
#include <unordered_set>
#include <string>

#include "Entity.h"
#include "Analyzer.h"



//using namespace std;
//using namespace llvm;
//using namespace clang;
//using namespace clang::tooling;


//// ***************************************************************************
////          プリプロセッサからのコールバック処理
//// ***************************************************************************
//
//class PPCallbacksTracker : public clang::PPCallbacks {
//private:
//	clang::Preprocessor &PP;
//public:
//	PPCallbacksTracker(clang::Preprocessor &pp) : PP(pp) {}
//	void InclusionDirective(clang::SourceLocation HashLoc,
//		const clang::Token &IncludeTok,
//		llvm::StringRef FileName,
//		bool IsAngled,
//		clang::CharSourceRange FilenameRange,
//		const clang::FileEntry *File,
//		llvm::StringRef SearchPath,
//		llvm::StringRef RelativePath,
//		const clang::Module *Imported) override {
//		llvm::errs() << "InclusionDirective : ";
//		if (File) {
//			if (IsAngled)   llvm::errs() << "<" << File->getName() << ">\n";
//			else            llvm::errs() << "\"" << File->getName() << "\"\n";
//		} else {
//			llvm::errs() << "not found file ";
//			if (IsAngled)   llvm::errs() << "<" << FileName << ">\n";
//			else            llvm::errs() << "\"" << FileName << "\"\n";
//		}
//	}
//};

//// ***************************************************************************
////          ASTウォークスルー
//// ***************************************************************************
//
////llvm\tools\clang\include\clang\AST\DeclVisitor.hの36行目参照↓voidは指定できない。
//class ExampleVisitor : public clang::DeclVisitor<ExampleVisitor, bool> {
//private:
//	clang::PrintingPolicy      Policy;
//	const clang::SourceManager &SM;
//public:
//	ExampleVisitor(clang::CompilerInstance *CI) : Policy(clang::PrintingPolicy(CI->getASTContext().getPrintingPolicy())),
//		SM(CI->getASTContext().getSourceManager()) {
//		Policy.Bool = 1;    // print()にてboolが_Boolと表示されないようにする
//	}   //↑http://clang.llvm.org/doxygen/structclang_1_1PrintingPolicy.html#a4a4cff4f89cc3ec50381d9d44bedfdab
//private:
//	// インデント制御
//	struct CIndentation {
//		int             IndentLevel;
//		CIndentation() : IndentLevel(0) {}
//		void Indentation(clang::raw_ostream &OS) const {
//			for (int i = 0; i < IndentLevel; ++i) OS << "  ";
//		}
//		// raw_ostream<<演算子定義
//		friend clang::raw_ostream &operator<<(clang::raw_ostream &OS, const CIndentation &aCIndentation) {
//			aCIndentation.Indentation(OS);
//			return OS;
//		}
//	} indentation;
//	class CIndentationHelper {
//	private:
//		ExampleVisitor  *parent;
//	public:
//		CIndentationHelper(ExampleVisitor *aExampleVisitor) : parent(aExampleVisitor) {
//			parent->indentation.IndentLevel++;
//		}
//		~CIndentationHelper() { parent->indentation.IndentLevel--; }
//	};
//#define INDENTATION CIndentationHelper CIndentationHelper(this)
//public:
//	// DeclContextメンバーの1レベルの枚挙処理
//	void EnumerateDecl(clang::DeclContext *aDeclContext) {
//		for (clang::DeclContext::decl_iterator i = aDeclContext->decls_begin(), e = aDeclContext->decls_end(); i != e; i++) {
//			clang::Decl *D = *i;
//			D->dumpColor();
//			//if (indentation.IndentLevel == 0) {
//			//	llvm::errs() << "TopLevel : " << D->getDeclKindName();                                    // Declの型表示
//			//	if (clang::NamedDecl *N = clang::dyn_cast<clang::NamedDecl>(D))  llvm::errs() << " " << N->getNameAsString();  // NamedDeclなら名前表示
//			//	llvm::errs() << " (" << D->getLocation().printToString(SM) << ")\n";                      // ソース上の場所表示
//			//}
//			//Visit(D);       // llvm\tools\clang\include\clang\AST\DeclVisitor.hの38行目
//		}
//	}
//
//	// class/struct/unionの処理
//	virtual bool VisitCXXRecordDecl(clang::CXXRecordDecl *aCXXRecordDecl, bool aForce = false) {
//		// 参照用(class foo;のような宣言)なら追跡しない
//		if (!aCXXRecordDecl->isCompleteDefinition()) {
//			return true;
//		}
//
//		// 名前無しなら表示しない(ただし、強制表示されたら表示する:Elaborated用)
//		if (!aCXXRecordDecl->getIdentifier() && !aForce) {
//			return true;
//		}
//
//		llvm::errs() << indentation << "<<<====================================\n";
//
//		// TopLevelなら参考のためlibToolingでも表示する
//		if (indentation.IndentLevel == 0) {
//			aCXXRecordDecl->print(llvm::errs(), Policy);
//			llvm::errs() << "\n";
//			llvm::errs() << indentation << "--------------------\n";
//		}
//
//		// クラス定義の処理
//		llvm::errs() << indentation << "CXXRecordDecl : " << aCXXRecordDecl->getNameAsString() << " {\n";
//		{
//			INDENTATION;
//
//			// 基底クラスの枚挙処理
//			for (clang::CXXRecordDecl::base_class_iterator Base = aCXXRecordDecl->bases_begin(), BaseEnd = aCXXRecordDecl->bases_end();
//				Base != BaseEnd;
//				++Base) {                                          // ↓型名を取り出す(例えば、Policy.Bool=0の時、bool型は"_Bool"となる)
//				llvm::errs() << indentation << "Base : " << Base->getType().getAsString(Policy) << "\n";
//			}
//
//			// メンバーの枚挙処理
//			EnumerateDecl(aCXXRecordDecl);
//		}
//		llvm::errs() << indentation << "}\n";
//		llvm::errs() << indentation << "====================================>>>\n";
//		return true;
//	}
//
//	// メンバー変数の処理
//	virtual bool VisitFieldDecl(clang::FieldDecl *aFieldDecl) {
//		// 名前無しclass/struct/unionでメンバー変数が直接定義されている時の対応
//		clang::CXXRecordDecl *R = NULL;      // 名前無しの時、内容を表示するためにCXXRecordDeclをポイントする
//		const clang::Type *T = aFieldDecl->getType().split().Ty;
//		if (T->getTypeClass() == clang::Type::Elaborated) {
//			R = clang::cast<clang::ElaboratedType>(T)->getNamedType()->getAsCXXRecordDecl();
//			if (R && (R->getIdentifier()))  R = NULL;
//		}
//		// 内容表示
//		if (R) {
//			llvm::errs() << indentation << "FieldDecl : <no-name-type> " << aFieldDecl->getNameAsString() << "\n";
//			VisitCXXRecordDecl(R, true);    // 名前無しclass/struct/unionの内容表示
//		} else {
//			llvm::errs() << indentation << "FieldDecl : " << aFieldDecl->getType().getAsString(Policy) << " " << aFieldDecl->getNameAsString() << "\n";
//		}
//		return true;
//	}
//
//	// namespaceの処理(配下を追跡する)
//	virtual bool VisitNamespaceDecl(clang::NamespaceDecl *aNamespaceDecl) {
//		llvm::errs() << "NamespaceDecl : namespace " << aNamespaceDecl->getNameAsString() << " {\n";
//		EnumerateDecl(aNamespaceDecl);
//		llvm::errs() << "} end of namespace " << aNamespaceDecl->getNameAsString() << "\n";
//		return true;
//	}
//
//	// extern "C"/"C++"の処理(配下を追跡する)
//	virtual bool VisitLinkageSpecDecl(clang::LinkageSpecDecl*aLinkageSpecDecl) {
//		std::string lang;
//		switch (aLinkageSpecDecl->getLanguage()) {
//		case clang::LinkageSpecDecl::lang_c:   lang = "C"; break;
//		case clang::LinkageSpecDecl::lang_cxx: lang = "C++"; break;
//		}
//		llvm::errs() << "LinkageSpecDecl : extern \"" << lang << "\" {\n";
//		EnumerateDecl(aLinkageSpecDecl);
//		llvm::errs() << "} end of extern \"" << lang << "\"\n";
//
//		return true;
//	}
//
//	virtual bool VisitFunctionDecl(clang::FunctionDecl* D) {
//		//D->getQualifiedNameAsString
//		//auto body = D->getBody();
//		//body->
//		//EnumerateDecl(D);
//		return true;
//	}
//};
//
//// ***************************************************************************
////          定番の処理
//// ***************************************************************************
//
//class ExampleASTConsumer : public clang::ASTConsumer {
//private:
//	ExampleVisitor *visitor; // doesn't have to be private
//public:
//	explicit ExampleASTConsumer(clang::CompilerInstance *CI) : visitor(new ExampleVisitor(CI)) {
//		// プリプロセッサからのコールバック登録
//		clang::Preprocessor &PP = CI->getPreprocessor();
//		PP.addPPCallbacks(llvm::make_unique<PPCallbacksTracker>(PP));
//	}
//	// AST解析結果の受取
//	virtual void HandleTranslationUnit(clang::ASTContext &Context) {
//		llvm::errs() << "\n\nHandleTranslationUnit()\n";
//		visitor->EnumerateDecl(Context.getTranslationUnitDecl());
//	}
//};
//
//class ExampleFrontendAction : public clang::SyntaxOnlyAction /*ASTFrontendAction*/ {
//public:
//	virtual std::unique_ptr<clang::ASTConsumer> CreateASTConsumer(clang::CompilerInstance &CI, StringRef file) {
//		return llvm::make_unique<ExampleASTConsumer>(&CI); // pass CI pointer to ASTConsumer
//	}
//};


//================================================================

//
//class Analyzer {
//public:
//	std::unordered_set<std::unique_ptr<Entity>, Entity::Hash, Entity::Equals> Entities;
//	std::vector<std::unique_ptr<clang::CompilerInstance>> CompilerInstances;
//};
//
//class MyVisitor : public clang::RecursiveASTVisitor<MyVisitor> {
//private:
//	std::shared_ptr<Analyzer> _Analyzer;
//	clang::PrintingPolicy policy;
//	clang::CompilerInstance& compilerInstance;
//	clang::SourceManager& sourceManager;
//
//public:
//	typedef clang::RecursiveASTVisitor<MyVisitor> base;
//
//	MyVisitor(std::shared_ptr<Analyzer> analyzer, clang::CompilerInstance& compilerInstance)
//		: compilerInstance(compilerInstance)
//		, policy(clang::PrintingPolicy(compilerInstance.getASTContext().getPrintingPolicy()))
//		, sourceManager(compilerInstance.getASTContext().getSourceManager()) {
//		_Analyzer = analyzer;
//		policy.Bool = 1; // print()にてboolが_Boolと表示されないようにする
//	}
//
//	bool TraverseDecl(clang::Decl *x) {
//		// your logic here
//		//llvm::errs() << x->getDeclKindName() << "\n"; // Declの型表示
//		//if (clang::NamedDecl *N = clang::dyn_cast<clang::NamedDecl>(x)) {
//		//	llvm::errs() << " " << N->getNameAsString();  // NamedDeclなら名前表示
//		//}
//		//llvm::errs() << " (" << x->getLocation().printToString(sourceManager) << ")\n"; // ソース上の場所表示
//		std::cout << "--------" << std::endl;
//		x->dumpColor();
//
//
//		//llvm::SmallString<512> buf;
//		//clang::index::generateUSRForDecl(x, buf);
//		//buf.push_back('\0');
//		//std::cout << buf.c_str() << std::endl;
//
//		base::TraverseDecl(x); // Forward to base class
//		return true; // Return false to stop the AST analyzing
//	}
//	bool TraverseVarDecl(clang::VarDecl* D) {
//		_Analyzer->Entities.insert(std::make_unique<Entity>(D));
//		base::TraverseVarDecl(D);
//		return true;
//	}
//	bool TraverseStmt(clang::Stmt *x) {
//		// your logic here
//		//llvm::errs() << x->getDeclKindName() << "\n"; // Declの型表示
//		//if (clang::NamedDecl *N = clang::dyn_cast<clang::NamedDecl>(x)) {
//		//	llvm::errs() << " " << N->getNameAsString();  // NamedDeclなら名前表示
//		//}
//		//llvm::errs() << " (" << x->getLocation().printToString(sourceManager) << ")\n"; // ソース上の場所表示
//		//std::cout << "--------" << std::endl;
//		//x->dumpColor();
//		base::TraverseStmt(x);
//		return true;
//	}
//	bool TraverseType(clang::QualType x) {
//		//// your logic here
//		//std::cout << "--------" << std::endl;
//		//x->dump();
//
//		base::TraverseType(x);
//		return true;
//	}
//	bool VisitBinaryOperator(clang::BinaryOperator *S) {
//		//auto kind = S->getValueKind();
//		//std::cout << S->getOpcodeStr().str() << std::endl;
//		return true;
//	}
//	//bool TraverseBinaryOperator(clang::BinaryOperator* x) {
//	//	return true;
//	//}
//	bool VisitFunctionDecl(clang::FunctionDecl* D) {
//		_Analyzer->Entities.insert(std::make_unique<Entity>(D));
//		llvm::SmallString<512> buf;
//		clang::index::generateUSRForDecl(D, buf);
//		buf.push_back('\0');
//		std::cout << buf.c_str() << std::endl;
//		return true;
//	}
//	bool VisitCallExpr(clang::CallExpr *S) {
//		auto D = S->getCalleeDecl();
//
//		llvm::SmallString<512> buf;
//		clang::index::generateUSRForDecl(D, buf);
//		buf.push_back('\0');
//		std::cout << buf.c_str() << std::endl;
//
//		return true;
//	}
//};
//
//class MyAstConsumer : public clang::ASTConsumer {
//public:
//	explicit MyAstConsumer(std::shared_ptr<Analyzer> analyzer, clang::CompilerInstance& compilerInstance) {
//		_Analyzer = analyzer;
//		this->visitor = std::make_shared<MyVisitor>(_Analyzer, compilerInstance);
//	}
//
//	virtual void HandleTranslationUnit(clang::ASTContext& context) {
//		// Traversing the translation unit decl via a RecursiveASTVisitor
//		// will visit all nodes in the AST.
//		visitor->TraverseDecl(context.getTranslationUnitDecl());
//	}
//
//private:
//	std::shared_ptr<Analyzer> _Analyzer;
//	// A RecursiveASTVisitor implementation.
//	std::shared_ptr<MyVisitor> visitor;
//};
//
//class MyFrontendAction : public clang::SyntaxOnlyAction /*ASTFrontendAction*/ {
//	std::shared_ptr<Analyzer> _Analyzer;
//
//public:
//	MyFrontendAction(std::shared_ptr<Analyzer> analyzer) {
//		_Analyzer = analyzer;
//	}
//
//	virtual std::unique_ptr<clang::ASTConsumer> CreateASTConsumer(clang::CompilerInstance& compilerInstance, StringRef file) {
//		return llvm::make_unique<MyAstConsumer>(_Analyzer, compilerInstance); // pass CI pointer to ASTConsumer
//	}
//};
//
//class MyFrontendActionFactory : public clang::tooling::FrontendActionFactory {
//	std::shared_ptr<Analyzer> _Analyzer;
//public:
//	MyFrontendActionFactory(std::shared_ptr<Analyzer> analyzer) {
//		_Analyzer = analyzer;
//	}
//
//	/// \brief Invokes the compiler with a FrontendAction created by create().
//	bool runInvocation(std::shared_ptr<clang::CompilerInvocation> Invocation, clang::FileManager *Files, std::shared_ptr<clang::PCHContainerOperations> PCHContainerOps, clang::DiagnosticConsumer *DiagConsumer) override {
//		//return clang::tooling::FrontendActionFactory::runInvocation(Invocation, Files, PCHContainerOps, DiagConsumer);
//
//		auto ci = std::make_unique<clang::CompilerInstance>(std::move(PCHContainerOps));
//
//		// Create a compiler instance to handle the actual work.
//		clang::CompilerInstance& Compiler = *ci.get();
//		Compiler.setInvocation(std::move(Invocation));
//		Compiler.setFileManager(Files);
//
//		// The FrontendAction can have lifetime requirements for Compiler or its
//		// members, and we need to ensure it's deleted earlier than Compiler. So we
//		// pass it to an std::unique_ptr declared after the Compiler variable.
//		std::unique_ptr<clang::FrontendAction> ScopedToolAction(create());
//
//		// Create the compiler's actual diagnostics engine.
//		Compiler.createDiagnostics(DiagConsumer, /*ShouldOwnClient=*/false);
//		if (!Compiler.hasDiagnostics())
//			return false;
//
//		Compiler.createSourceManager(*Files);
//
//		const bool Success = Compiler.ExecuteAction(*ScopedToolAction);
//
//		Files->clearStatCaches();
//
//		_Analyzer->CompilerInstances.push_back(std::move(ci));
//
//		return Success;
//	}
//
//	clang::FrontendAction *create() override {
//		return new MyFrontendAction(_Analyzer);
//	}
//};


static llvm::cl::OptionCategory MyToolCategory("My tool options");



int main(int argc, const char **argv) {
	std::vector<std::string> sourceFiles;
	sourceFiles.push_back("sample/sample1.cpp");
	sourceFiles.push_back("sample/sample2.cpp");

	std::vector<std::string> clangOptions;
	clangOptions.push_back("-std=c++14");
	clangOptions.push_back("-ferror-limit=9999");

	clangOptions.push_back("-fms-extensions");
	clangOptions.push_back("-fms-compatibility");

	clangOptions.push_back("-DUNICODE");
	clangOptions.push_back("-DWIN32");
	clangOptions.push_back("-DNDEBUG");
	clangOptions.push_back("-D_WINDOWS");
	clangOptions.push_back("-D_USRDLL");
	clangOptions.push_back("-DASTMCDLL_EXPORTS");
	clangOptions.push_back("-w");
	clangOptions.push_back("-Waddress-of-temporary");
	clangOptions.push_back("-Wwrite-strings");
	clangOptions.push_back("-Wint-to-pointer-cast");
	clangOptions.push_back("-Wunused-value");

	std::vector<const char*> args;

	// 実行ファイル名
	args.push_back("test.exe");

	// TODO: 自作ツール用オプション
	// ソースファイル
	for (auto& s : sourceFiles) {
		args.push_back(s.c_str());
	}

	// clang 用オプション
	args.push_back("--");
	for (auto& o : clangOptions) {
		args.push_back(o.c_str());
	}

	auto argsCount = (int)args.size();
	clang::tooling::CommonOptionsParser op(argsCount, (const char**)&args[0], MyToolCategory);
	clang::tooling::ClangTool Tool(op.getCompilations(), op.getSourcePathList());

	auto analyzer = std::make_shared<Analyzer>();
	auto mf = std::make_shared<MyFrontendActionFactory>(analyzer);

	Tool.run(mf.get());

	for (auto& e : analyzer->Entities) {
		if (e->Usr) {
			std::cout << e->Usr;
		}
		std::cout << " " << e->Name();
		std::cout << std::endl;
	}




	//clang::CompilerInstance compiler;

	//compiler.createDiagnostics(argc, argv);
	//compiler.getInvocation().setLangDefaults(clang::IK_CXX);
	//clang::CompilerInvocation::CreateFromArgs(
	//	compiler.getInvocation(),
	//	argv + 1, argv + argc,
	//	compiler.getDiagnostics()
	//);

	//compiler.setTarget(
	//	clang::TargetInfo::CreateTargetInfo(
	//		compiler.getDiagnostics(),
	//		compiler.getTargetOpts()
	//	)
	//);

	//compiler.createFileManager();
	//compiler.createSourceManager(compiler.getFileManager());
	//compiler.createPreprocessor();
	//compiler.createASTContext();
	//compiler.setASTConsumer(new ast_consumer);
	//compiler.createSema(false, NULL);

	//auto const n = compiler.getFrontendOpts().Inputs.size();
	//if (n == 0) {
	//	llvm::errs() << "No input file.\n";
	//	return 1;
	//} else if (n > 1) {
	//	llvm::errs() << "There are too many inputs.\n";
	//	return 1;
	//}

	//compiler.InitializeSourceManager(
	//	compiler.getFrontendOpts().Inputs[0].second
	//);

	//clang::ParseAST(
	//	compiler.getPreprocessor(),
	//	&compiler.getASTConsumer(),
	//	compiler.getASTContext()
	//);


	return 0;
}

