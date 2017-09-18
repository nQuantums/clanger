// cppclangtest.cpp : コンソール アプリケーションのエントリ ポイントを定義します。
//

#include "stdafx.h"

#pragma warning(push)
#pragma warning(disable:4146)
#pragma warning(disable:4141)
#pragma warning(disable:4267)
#pragma warning(disable:4244)
#pragma warning(disable:4291)
#pragma warning(disable:4819)
#include "clang/AST/AST.h"
#include "clang/AST/ASTContext.h"
#include "clang/AST/ASTConsumer.h"
#include "clang/AST/DeclVisitor.h"
#include "clang/Frontend/ASTConsumers.h"
#include "clang/Frontend/FrontendActions.h"
#include "clang/Frontend/CompilerInstance.h"
#include "clang/Lex/Preprocessor.h"
#include "clang/Tooling/CommonOptionsParser.h"
#include "clang/Tooling/Tooling.h"
#pragma warning(pop)

#pragma comment(lib, "Version.lib")

//#pragma comment(lib, "LLVMAArch64AsmParser.lib")
//#pragma comment(lib, "LLVMAArch64AsmPrinter.lib")
//#pragma comment(lib, "LLVMAArch64CodeGen.lib")
//#pragma comment(lib, "LLVMAArch64Desc.lib")
//#pragma comment(lib, "LLVMAArch64Disassembler.lib")
//#pragma comment(lib, "LLVMAArch64Info.lib")
//#pragma comment(lib, "LLVMAArch64Utils.lib")
//#pragma comment(lib, "LLVMAMDGPUAsmParser.lib")
//#pragma comment(lib, "LLVMAMDGPUAsmPrinter.lib")
//#pragma comment(lib, "LLVMAMDGPUCodeGen.lib")
//#pragma comment(lib, "LLVMAMDGPUDesc.lib")
//#pragma comment(lib, "LLVMAMDGPUDisassembler.lib")
//#pragma comment(lib, "LLVMAMDGPUInfo.lib")
//#pragma comment(lib, "LLVMAMDGPUUtils.lib")
//#pragma comment(lib, "LLVMAnalysis.lib")
//#pragma comment(lib, "LLVMARMAsmParser.lib")
//#pragma comment(lib, "LLVMARMAsmPrinter.lib")
//#pragma comment(lib, "LLVMARMCodeGen.lib")
//#pragma comment(lib, "LLVMARMDesc.lib")
//#pragma comment(lib, "LLVMARMDisassembler.lib")
//#pragma comment(lib, "LLVMARMInfo.lib")
//#pragma comment(lib, "LLVMAsmParser.lib")
//#pragma comment(lib, "LLVMAsmPrinter.lib")
#pragma comment(lib, "LLVMBinaryFormat.lib")
#pragma comment(lib, "LLVMBitReader.lib")
//#pragma comment(lib, "LLVMBitWriter.lib")
//#pragma comment(lib, "LLVMBPFAsmPrinter.lib")
//#pragma comment(lib, "LLVMBPFCodeGen.lib")
//#pragma comment(lib, "LLVMBPFDesc.lib")
//#pragma comment(lib, "LLVMBPFDisassembler.lib")
//#pragma comment(lib, "LLVMBPFInfo.lib")
//#pragma comment(lib, "LLVMCodeGen.lib")
#pragma comment(lib, "LLVMCore.lib")
//#pragma comment(lib, "LLVMCoroutines.lib")
//#pragma comment(lib, "LLVMCoverage.lib")
//#pragma comment(lib, "LLVMDebugInfoCodeView.lib")
//#pragma comment(lib, "LLVMDebugInfoDWARF.lib")
//#pragma comment(lib, "LLVMDebugInfoMSF.lib")
//#pragma comment(lib, "LLVMDebugInfoPDB.lib")
//#pragma comment(lib, "LLVMDemangle.lib")
//#pragma comment(lib, "LLVMDlltoolDriver.lib")
//#pragma comment(lib, "LLVMExecutionEngine.lib")
//#pragma comment(lib, "LLVMGlobalISel.lib")
//#pragma comment(lib, "LLVMHexagonAsmParser.lib")
//#pragma comment(lib, "LLVMHexagonCodeGen.lib")
//#pragma comment(lib, "LLVMHexagonDesc.lib")
//#pragma comment(lib, "LLVMHexagonDisassembler.lib")
//#pragma comment(lib, "LLVMHexagonInfo.lib")
//#pragma comment(lib, "LLVMInstCombine.lib")
//#pragma comment(lib, "LLVMInstrumentation.lib")
//#pragma comment(lib, "LLVMInterpreter.lib")
//#pragma comment(lib, "LLVMipo.lib")
//#pragma comment(lib, "LLVMIRReader.lib")
//#pragma comment(lib, "LLVMLanaiAsmParser.lib")
//#pragma comment(lib, "LLVMLanaiAsmPrinter.lib")
//#pragma comment(lib, "LLVMLanaiCodeGen.lib")
//#pragma comment(lib, "LLVMLanaiDesc.lib")
//#pragma comment(lib, "LLVMLanaiDisassembler.lib")
//#pragma comment(lib, "LLVMLanaiInfo.lib")
#pragma comment(lib, "LLVMLibDriver.lib")
#pragma comment(lib, "LLVMLineEditor.lib")
#pragma comment(lib, "LLVMLinker.lib")
#pragma comment(lib, "LLVMLTO.lib")
#pragma comment(lib, "LLVMMC.lib")
//#pragma comment(lib, "LLVMMCDisassembler.lib")
//#pragma comment(lib, "LLVMMCJIT.lib")
#pragma comment(lib, "LLVMMCParser.lib")
//#pragma comment(lib, "LLVMMipsAsmParser.lib")
//#pragma comment(lib, "LLVMMipsAsmPrinter.lib")
//#pragma comment(lib, "LLVMMipsCodeGen.lib")
//#pragma comment(lib, "LLVMMipsDesc.lib")
//#pragma comment(lib, "LLVMMipsDisassembler.lib")
//#pragma comment(lib, "LLVMMipsInfo.lib")
#pragma comment(lib, "LLVMMIRParser.lib")
//#pragma comment(lib, "LLVMMSP430AsmPrinter.lib")
//#pragma comment(lib, "LLVMMSP430CodeGen.lib")
//#pragma comment(lib, "LLVMMSP430Desc.lib")
//#pragma comment(lib, "LLVMMSP430Info.lib")
//#pragma comment(lib, "LLVMNVPTXAsmPrinter.lib")
//#pragma comment(lib, "LLVMNVPTXCodeGen.lib")
//#pragma comment(lib, "LLVMNVPTXDesc.lib")
//#pragma comment(lib, "LLVMNVPTXInfo.lib")
//#pragma comment(lib, "LLVMObjCARCOpts.lib")
//#pragma comment(lib, "LLVMObject.lib")
//#pragma comment(lib, "LLVMObjectYAML.lib")
#pragma comment(lib, "LLVMOption.lib")
//#pragma comment(lib, "LLVMOrcJIT.lib")
//#pragma comment(lib, "LLVMPasses.lib")
//#pragma comment(lib, "LLVMPowerPCAsmParser.lib")
//#pragma comment(lib, "LLVMPowerPCAsmPrinter.lib")
//#pragma comment(lib, "LLVMPowerPCCodeGen.lib")
//#pragma comment(lib, "LLVMPowerPCDesc.lib")
//#pragma comment(lib, "LLVMPowerPCDisassembler.lib")
//#pragma comment(lib, "LLVMPowerPCInfo.lib")
#pragma comment(lib, "LLVMProfileData.lib")
#pragma comment(lib, "LLVMRuntimeDyld.lib")
#pragma comment(lib, "LLVMScalarOpts.lib")
#pragma comment(lib, "LLVMSelectionDAG.lib")
//#pragma comment(lib, "LLVMSparcAsmParser.lib")
//#pragma comment(lib, "LLVMSparcAsmPrinter.lib")
//#pragma comment(lib, "LLVMSparcCodeGen.lib")
//#pragma comment(lib, "LLVMSparcDesc.lib")
//#pragma comment(lib, "LLVMSparcDisassembler.lib")
//#pragma comment(lib, "LLVMSparcInfo.lib")
#pragma comment(lib, "LLVMSupport.lib")
#pragma comment(lib, "LLVMSymbolize.lib")
//#pragma comment(lib, "LLVMSystemZAsmParser.lib")
//#pragma comment(lib, "LLVMSystemZAsmPrinter.lib")
//#pragma comment(lib, "LLVMSystemZCodeGen.lib")
//#pragma comment(lib, "LLVMSystemZDesc.lib")
//#pragma comment(lib, "LLVMSystemZDisassembler.lib")
//#pragma comment(lib, "LLVMSystemZInfo.lib")
#pragma comment(lib, "LLVMTableGen.lib")
#pragma comment(lib, "LLVMTarget.lib")
#pragma comment(lib, "LLVMTransformUtils.lib")
#pragma comment(lib, "LLVMVectorize.lib")
//#pragma comment(lib, "LLVMX86AsmParser.lib")
//#pragma comment(lib, "LLVMX86AsmPrinter.lib")
//#pragma comment(lib, "LLVMX86CodeGen.lib")
//#pragma comment(lib, "LLVMX86Desc.lib")
//#pragma comment(lib, "LLVMX86Disassembler.lib")
//#pragma comment(lib, "LLVMX86Info.lib")
//#pragma comment(lib, "LLVMX86Utils.lib")
//#pragma comment(lib, "LLVMXCoreAsmPrinter.lib")
//#pragma comment(lib, "LLVMXCoreCodeGen.lib")
//#pragma comment(lib, "LLVMXCoreDesc.lib")
//#pragma comment(lib, "LLVMXCoreDisassembler.lib")
//#pragma comment(lib, "LLVMXCoreInfo.lib")
//#pragma comment(lib, "LLVMXRay.lib")

#pragma comment(lib, "clangAnalysis.lib")
#pragma comment(lib, "clangARCMigrate.lib")
#pragma comment(lib, "clangAST.lib")
#pragma comment(lib, "clangASTMatchers.lib")
#pragma comment(lib, "clangBasic.lib")
#pragma comment(lib, "clangCodeGen.lib")
#pragma comment(lib, "clangDriver.lib")
#pragma comment(lib, "clangDynamicASTMatchers.lib")
#pragma comment(lib, "clangEdit.lib")
#pragma comment(lib, "clangFormat.lib")
#pragma comment(lib, "clangFrontend.lib")
#pragma comment(lib, "clangFrontendTool.lib")
#pragma comment(lib, "clangIndex.lib")
#pragma comment(lib, "clangLex.lib")
#pragma comment(lib, "clangParse.lib")
#pragma comment(lib, "clangRewrite.lib")
#pragma comment(lib, "clangRewriteFrontend.lib")
#pragma comment(lib, "clangSema.lib")
#pragma comment(lib, "clangSerialization.lib")
#pragma comment(lib, "clangStaticAnalyzerCheckers.lib")
#pragma comment(lib, "clangStaticAnalyzerCore.lib")
#pragma comment(lib, "clangStaticAnalyzerFrontend.lib")
#pragma comment(lib, "clangTooling.lib")
#pragma comment(lib, "clangToolingCore.lib")
#pragma comment(lib, "clangToolingRefactor.lib")
//#pragma comment(lib, "libclang.lib")

using namespace std;
using namespace llvm;
using namespace clang;
using namespace clang::tooling;


// ***************************************************************************
//          プリプロセッサからのコールバック処理
// ***************************************************************************

class PPCallbacksTracker : public PPCallbacks {
private:
	Preprocessor &PP;
public:
	PPCallbacksTracker(Preprocessor &pp) : PP(pp) {}
	void InclusionDirective(SourceLocation HashLoc,
		const Token &IncludeTok,
		llvm::StringRef FileName,
		bool IsAngled,
		CharSourceRange FilenameRange,
		const FileEntry *File,
		llvm::StringRef SearchPath,
		llvm::StringRef RelativePath,
		const clang::Module *Imported) override {
		errs() << "InclusionDirective : ";
		if (File) {
			if (IsAngled)   errs() << "<" << File->getName() << ">\n";
			else            errs() << "\"" << File->getName() << "\"\n";
		} else {
			errs() << "not found file ";
			if (IsAngled)   errs() << "<" << FileName << ">\n";
			else            errs() << "\"" << FileName << "\"\n";
		}
	}
};

// ***************************************************************************
//          ASTウォークスルー
// ***************************************************************************

//llvm\tools\clang\include\clang\AST\DeclVisitor.hの36行目参照↓voidは指定できない。
class ExampleVisitor : public DeclVisitor<ExampleVisitor, bool> {
private:
	PrintingPolicy      Policy;
	const SourceManager &SM;
public:
	ExampleVisitor(CompilerInstance *CI) : Policy(PrintingPolicy(CI->getASTContext().getPrintingPolicy())),
		SM(CI->getASTContext().getSourceManager()) {
		Policy.Bool = 1;    // print()にてboolが_Boolと表示されないようにする
	}   //↑http://clang.llvm.org/doxygen/structclang_1_1PrintingPolicy.html#a4a4cff4f89cc3ec50381d9d44bedfdab
private:
	// インデント制御
	struct CIndentation {
		int             IndentLevel;
		CIndentation() : IndentLevel(0) {}
		void Indentation(raw_ostream &OS) const {
			for (int i = 0; i < IndentLevel; ++i) OS << "  ";
		}
		// raw_ostream<<演算子定義
		friend raw_ostream &operator<<(raw_ostream &OS, const CIndentation &aCIndentation) {
			aCIndentation.Indentation(OS);
			return OS;
		}
	} indentation;
	class CIndentationHelper {
	private:
		ExampleVisitor  *parent;
	public:
		CIndentationHelper(ExampleVisitor *aExampleVisitor) : parent(aExampleVisitor) {
			parent->indentation.IndentLevel++;
		}
		~CIndentationHelper() { parent->indentation.IndentLevel--; }
	};
#define INDENTATION CIndentationHelper CIndentationHelper(this)
public:
	// DeclContextメンバーの1レベルの枚挙処理
	void EnumerateDecl(DeclContext *aDeclContext) {
		for (DeclContext::decl_iterator i = aDeclContext->decls_begin(), e = aDeclContext->decls_end(); i != e; i++) {
			Decl *D = *i;
			if (indentation.IndentLevel == 0) {
				errs() << "TopLevel : " << D->getDeclKindName();                                    // Declの型表示
				if (NamedDecl *N = dyn_cast<NamedDecl>(D))  errs() << " " << N->getNameAsString();  // NamedDeclなら名前表示
				errs() << " (" << D->getLocation().printToString(SM) << ")\n";                      // ソース上の場所表示
			}
			Visit(D);       // llvm\tools\clang\include\clang\AST\DeclVisitor.hの38行目
		}
	}

	// class/struct/unionの処理
	virtual bool VisitCXXRecordDecl(CXXRecordDecl *aCXXRecordDecl, bool aForce = false) {
		// 参照用(class foo;のような宣言)なら追跡しない
		if (!aCXXRecordDecl->isCompleteDefinition()) {
			return true;
		}

		// 名前無しなら表示しない(ただし、強制表示されたら表示する:Elaborated用)
		if (!aCXXRecordDecl->getIdentifier() && !aForce) {
			return true;
		}

		errs() << indentation << "<<<====================================\n";

		// TopLevelなら参考のためlibToolingでも表示する
		if (indentation.IndentLevel == 0) {
			aCXXRecordDecl->print(errs(), Policy);
			errs() << "\n";
			errs() << indentation << "--------------------\n";
		}

		// クラス定義の処理
		errs() << indentation << "CXXRecordDecl : " << aCXXRecordDecl->getNameAsString() << " {\n";
		{
			INDENTATION;

			// 基底クラスの枚挙処理
			for (CXXRecordDecl::base_class_iterator Base = aCXXRecordDecl->bases_begin(), BaseEnd = aCXXRecordDecl->bases_end();
				Base != BaseEnd;
				++Base) {                                          // ↓型名を取り出す(例えば、Policy.Bool=0の時、bool型は"_Bool"となる)
				errs() << indentation << "Base : " << Base->getType().getAsString(Policy) << "\n";
			}

			// メンバーの枚挙処理
			EnumerateDecl(aCXXRecordDecl);
		}
		errs() << indentation << "}\n";
		errs() << indentation << "====================================>>>\n";
		return true;
	}

	// メンバー変数の処理
	virtual bool VisitFieldDecl(FieldDecl *aFieldDecl) {
		// 名前無しclass/struct/unionでメンバー変数が直接定義されている時の対応
		CXXRecordDecl *R = NULL;      // 名前無しの時、内容を表示するためにCXXRecordDeclをポイントする
		const Type *T = aFieldDecl->getType().split().Ty;
		if (T->getTypeClass() == Type::Elaborated) {
			R = cast<ElaboratedType>(T)->getNamedType()->getAsCXXRecordDecl();
			if (R && (R->getIdentifier()))  R = NULL;
		}
		// 内容表示
		if (R) {
			errs() << indentation << "FieldDecl : <no-name-type> " << aFieldDecl->getNameAsString() << "\n";
			VisitCXXRecordDecl(R, true);    // 名前無しclass/struct/unionの内容表示
		} else {
			errs() << indentation << "FieldDecl : " << aFieldDecl->getType().getAsString(Policy) << " " << aFieldDecl->getNameAsString() << "\n";
		}
		return true;
	}

	// namespaceの処理(配下を追跡する)
	virtual bool VisitNamespaceDecl(NamespaceDecl *aNamespaceDecl) {
		errs() << "NamespaceDecl : namespace " << aNamespaceDecl->getNameAsString() << " {\n";
		EnumerateDecl(aNamespaceDecl);
		errs() << "} end of namespace " << aNamespaceDecl->getNameAsString() << "\n";
		return true;
	}

	// extern "C"/"C++"の処理(配下を追跡する)
	virtual bool VisitLinkageSpecDecl(LinkageSpecDecl*aLinkageSpecDecl) {
		string lang;
		switch (aLinkageSpecDecl->getLanguage()) {
		case LinkageSpecDecl::lang_c:   lang = "C"; break;
		case LinkageSpecDecl::lang_cxx: lang = "C++"; break;
		}
		errs() << "LinkageSpecDecl : extern \"" << lang << "\" {\n";
		EnumerateDecl(aLinkageSpecDecl);
		errs() << "} end of extern \"" << lang << "\"\n";
		return true;
	}
};

// ***************************************************************************
//          定番の処理
// ***************************************************************************

class ExampleASTConsumer : public ASTConsumer {
private:
	ExampleVisitor *visitor; // doesn't have to be private
public:
	explicit ExampleASTConsumer(CompilerInstance *CI) : visitor(new ExampleVisitor(CI)) {
		// プリプロセッサからのコールバック登録
		Preprocessor &PP = CI->getPreprocessor();
		PP.addPPCallbacks(llvm::make_unique<PPCallbacksTracker>(PP));
	}
	// AST解析結果の受取
	virtual void HandleTranslationUnit(ASTContext &Context) {
		errs() << "\n\nHandleTranslationUnit()\n";
		visitor->EnumerateDecl(Context.getTranslationUnitDecl());
	}
};

class ExampleFrontendAction : public SyntaxOnlyAction /*ASTFrontendAction*/ {
public:
	virtual std::unique_ptr<ASTConsumer> CreateASTConsumer(CompilerInstance &CI, StringRef file) {
		return llvm::make_unique<ExampleASTConsumer>(&CI); // pass CI pointer to ASTConsumer
	}
};

static cl::OptionCategory MyToolCategory("My tool options");



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
	CommonOptionsParser op(argsCount, (const char**)&args[0], MyToolCategory);
	ClangTool Tool(op.getCompilations(), op.getSourcePathList());
	return Tool.run(newFrontendActionFactory<ExampleFrontendAction>().get());
}
